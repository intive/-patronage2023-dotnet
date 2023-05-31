using System.Globalization;

using CsvHelper;

using Intive.Patronage2023.Shared.Abstractions;

namespace Intive.Patronage2023.Modules.Budget.Application.Budget.Shared.Services;

/// <summary>
/// BudgetTransactionExportService class provides functionalities to export budget transaction to a .csv file and upload it to Azure Blob Storage.
/// </summary>
public class BudgetTransactionExportService : IBudgetTransactionExportService
{
	private readonly IBlobStorageService blobStorageService;
	private readonly ICsvService<GetBudgetTransactionTransferInfo> csvService;

	/// <summary>
	/// Initializes a new instance of the <see cref="BudgetTransactionExportService"/> class.
	/// DataService.
	/// </summary>
	/// <param name="blobStorageService">BlobStorageService.</param>
	/// <param name="csvService">1.</param>
	public BudgetTransactionExportService(IBlobStorageService blobStorageService, ICsvService<GetBudgetTransactionTransferInfo> csvService)
	{
		this.blobStorageService = blobStorageService;
		this.csvService = csvService;
	}

	/// <summary>
	/// Exports the budget transactions to a CSV file and uploads it to Azure Blob Storage.
	/// </summary>
	/// <param name="transactions">GetBudgetTransactionList To Export.</param>
	/// <returns>The URI of the uploaded file in the Azure Blob Storage.</returns>
	public async Task<string?> Export(GetBudgetTransactionTransferList? transactions)
	{
		string filename = this.csvService.GenerateFileNameWithCsvExtension();
		using (var memoryStream = new MemoryStream())
		await using (var streamWriter = new StreamWriter(memoryStream))
		await using (var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
		{
			this.csvService.WriteRecordsToMemoryStream(transactions!.BudgetTransactionsList, csv);
			memoryStream.Position = 0;

			await this.blobStorageService.UploadToBlobStorage(memoryStream, filename);
		}

		return await this.blobStorageService.GenerateLinkToDownload(filename);
	}
}