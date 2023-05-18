using System.Globalization;
using Azure.Storage.Blobs;
using CsvHelper.Configuration;
using Intive.Patronage2023.Modules.Budget.Application.Data.Budgets;
using Intive.Patronage2023.Shared.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Intive.Patronage2023.Modules.Budget.Application.Data.Service;

/// <summary>
/// BudgetImportService.
/// </summary>
public class BudgetImportService : IBudgetImportService
{
	private readonly IExecutionContextAccessor contextAccessor;
	private readonly BlobStorageService blobStorageService;
	private readonly ImportBudgetsFromBlobStorage importBudgetsFromBlob;
	private readonly ReadAndValidateBudgets readAndValidateBudgets;

	/// <summary>
	/// Initializes a new instance of the <see cref="BudgetImportService"/> class.
	/// DataService.
	/// </summary>
	/// <param name="contextAccessor">The ExecutionContextAccessor used for accessing context information.</param>
	/// <param name="blobStorageService">BlobStorageService.</param>
	/// <param name="importBudgetsFromBlob">ImportBudgetsFromBlobStorage.</param>
	/// <param name="readAndValidateBudgets">ReadAndValidateBudgets.</param>
	public BudgetImportService(IExecutionContextAccessor contextAccessor, BlobStorageService blobStorageService, ImportBudgetsFromBlobStorage importBudgetsFromBlob, ReadAndValidateBudgets readAndValidateBudgets)
	{
		this.contextAccessor = contextAccessor;
		this.blobStorageService = blobStorageService;
		this.importBudgetsFromBlob = importBudgetsFromBlob;
		this.readAndValidateBudgets = readAndValidateBudgets;
	}

	/// <summary>
	/// Imports budgets from a provided .csv file, validates them, and stores them in the system.
	/// </summary>
	/// <param name="file">The .csv file containing the budgets to be imported.</param>
	/// <returns>A tuple containing a list of any errors encountered during the import process and
	/// the URI of the saved .csv file in the Azure Blob Storage if any budgets were successfully imported.
	/// If no budgets were imported, the URI is replaced with a message stating "No budgets were saved.".</returns>
	public async Task<ImportResult> Import(IFormFile file)
	{
		var errors = new List<string>();

		string containerName = this.contextAccessor.GetUserId().ToString()!;
		BlobContainerClient containerClient = await this.blobStorageService.CreateBlobContainerIfNotExists(containerName);

		var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			HasHeaderRecord = false,
			Delimiter = ",",
		};

		var budgetInfos = this.readAndValidateBudgets.ReadAndValidateBudgetsMethod(file, csvConfig, errors);

		if (budgetInfos.Count() == 0)
		{
			return new ImportResult { ErrorsList = errors, Uri = "No budgets were saved." };
		}

		string uri = await this.blobStorageService.UploadToBlobStorage(budgetInfos, containerClient);

		string fileName = new Uri(uri).LocalPath;
		await this.importBudgetsFromBlob.Import(fileName, containerClient, csvConfig);

		return new ImportResult { ErrorsList = errors, Uri = uri };
	}
}