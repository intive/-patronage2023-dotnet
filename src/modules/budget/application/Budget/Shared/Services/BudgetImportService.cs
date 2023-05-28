using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Intive.Patronage2023.Modules.Budget.Application.Budget.ExportingBudgets;
using Intive.Patronage2023.Modules.Budget.Application.Budget.ImportingBudgets;
using Intive.Patronage2023.Modules.Budget.Domain;
using Intive.Patronage2023.Shared.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Intive.Patronage2023.Modules.Budget.Application.Budget.Shared.Services;

/// <summary>
/// BudgetImportService class provides functionalities to import budgets from a .csv file.
/// </summary>
public class BudgetImportService : IBudgetImportService
{
	private readonly IExecutionContextAccessor contextAccessor;
	private readonly IBlobStorageService blobStorageService;
	private readonly IBudgetDataService budgetDataService;
	private readonly ICsvService<GetBudgetTransferInfo> csvService;

	/// <summary>
	/// Initializes a new instance of the <see cref="BudgetImportService"/> class.
	/// DataService.
	/// </summary>
	/// <param name="contextAccessor">The ExecutionContextAccessor used for accessing context information.</param>
	/// <param name="blobStorageService">BlobStorageService.</param>
	/// <param name="budgetDataService">IDataHelper.</param>
	/// <param name="csvService">GetBudgetTransferList.</param>
	public BudgetImportService(IExecutionContextAccessor contextAccessor, IBlobStorageService blobStorageService, IBudgetDataService budgetDataService, ICsvService<GetBudgetTransferInfo> csvService)
	{
		this.contextAccessor = contextAccessor;
		this.blobStorageService = blobStorageService;
		this.budgetDataService = budgetDataService;
		this.csvService = csvService;
	}

	/// <summary>
	/// Imports budgets from a provided .csv file, validates them, and stores them in the system.
	/// </summary>
	/// <param name="file">The .csv file containing the budgets to be imported.</param>
	/// <returns>A tuple containing a list of any errors encountered during the import process and
	/// the URI of the saved .csv file in the Azure Blob Storage if any budgets were successfully imported.
	/// If no budgets were imported, the URI is replaced with a message stating "No budgets were saved.".</returns>
	public async Task<GetImportResult> Import(IFormFile file)
	{
		var errors = new List<string>();

		var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			HasHeaderRecord = false,
			Delimiter = ",",
		};

		var budgetInfos = this.budgetDataService.CreateValidBudgetsList(file, csvConfig, errors);

		if (budgetInfos.Result.BudgetsList.Count == 0)
		{
			return new GetImportResult(
				new BudgetAggregateList(new List<BudgetAggregate>()),
				new ImportResult { ErrorsList = errors, Uri = "No budgets were saved." });
		}

		string fileName = this.csvService.GenerateFileNameWithCsvExtension();
		using (var memoryStream = new MemoryStream())
		await using (var streamWriter = new StreamWriter(memoryStream))
		await using (var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
		{
			this.csvService.WriteRecordsToMemoryStream(budgetInfos.Result.BudgetsList, csv);
			memoryStream.Position = 0;

			await this.blobStorageService.UploadToBlobStorage(memoryStream, fileName);
		}

		string uri = await this.blobStorageService.GenerateLinkToDownload(fileName);

		var budgetsAggregateList = await this.budgetDataService.ConvertBudgetsFromCsvToBudgetAggregate(fileName, csvConfig);

		return new GetImportResult(budgetsAggregateList, new ImportResult { ErrorsList = errors, Uri = uri });
	}
}