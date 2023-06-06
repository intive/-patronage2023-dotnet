using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using Intive.Patronage2023.Modules.Budget.Application.Budget.ImportingBudgetTransactions;
using Intive.Patronage2023.Modules.Budget.Application.Budget.Mappers;
using Intive.Patronage2023.Modules.Budget.Contracts.TransactionEnums;
using Intive.Patronage2023.Modules.Budget.Contracts.ValueObjects;
using Intive.Patronage2023.Modules.Budget.Domain;
using Intive.Patronage2023.Shared.Abstractions.Extensions;
using Intive.Patronage2023.Shared.Infrastructure.ImportExport;
using Microsoft.AspNetCore.Http;

namespace Intive.Patronage2023.Modules.Budget.Application.Budget.Shared.Services;

/// <summary>
/// BudgetDataService class implements the IBudgetDataService interface and provides methods
/// for data operations related to budgets.
/// </summary>
public class BudgetTransactionDataService : IBudgetTransactionDataService
{
	private readonly IValidator<GetBudgetTransactionImportInfo> validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="BudgetTransactionDataService"/> class.
	/// </summary>
	/// <param name="validator">Import info validator.</param>
	public BudgetTransactionDataService(IValidator<GetBudgetTransactionImportInfo> validator)
	{
		this.validator = validator;
	}

	/// <summary>
	/// Converts a collection of budget transaction information from CSV format into a list of BudgetTransactionAggregate objects.
	/// </summary>
	/// <param name="budgetTransactionsToImport">Collection of budget transactions information to be converted, represented as GetBudgetTransactionTransferInfo objects.</param>
	/// <param name="csvConfig">Configuration for reading the CSV file.</param>
	/// <returns>A Task containing a BudgetAggregateList, representing the converted budget information.</returns>
	public Task<BudgetTransactionAggregateList> ConvertBudgetTransactionsFromCsvToBudgetTransactionAggregate(IEnumerable<GetBudgetTransactionImportInfo> budgetTransactionsToImport, CsvConfiguration csvConfig)
	{
		var newBudgetTransactions = new List<BudgetTransactionAggregate>();
		foreach (var transaction in budgetTransactionsToImport)
		{
			var transactionId = new TransactionId(Guid.NewGuid());
			decimal value = decimal.Parse(transaction.Value, CultureInfo.InvariantCulture);
			var transactionType = (TransactionType)Enum.Parse(typeof(TransactionType), transaction.TransactionType);
			var categoryType = (CategoryType)Enum.Parse(typeof(CategoryType), transaction.CategoryType);
			var budgetTransactionDate = DateTime.Parse(transaction.Date);
			var newBudgetTransaction = BudgetTransactionAggregate.Create(transactionId, transaction.BudgetId, transactionType, transaction.Name, value, categoryType, budgetTransactionDate);
			newBudgetTransactions.Add(newBudgetTransaction);
		}

		return Task.FromResult(new BudgetTransactionAggregateList(newBudgetTransactions));
	}

	/// <summary>
	/// Validates the properties of a budget transaction object.
	/// </summary>
	/// <param name="budgetTransaction">The budget transaction object to validate.</param>
	/// <returns>A list of error messages. If the list is empty, the budget object is valid.</returns>
	public async Task<List<string>> BudgetTransactionValidate(GetBudgetTransactionImportInfo budgetTransaction)
	{
		var validationResult = await this.validator.ValidateAsync(budgetTransaction);

		var errors = new List<string>();
		errors.AddErrors(validationResult);

		return errors;
	}

	/// <summary>
	/// Reads budgets from a CSV file, validates them, and returns a list of valid budgets.
	/// Any errors encountered during validation are added to the provided errors list.
	/// </summary>
	/// <param name="budgetId">Import destination budget id.</param>
	/// <param name="file">The CSV file containing the budgets to be read and validated.</param>
	/// <param name="csvConfig">Configuration for reading the CSV file.</param>
	/// <param name="errors">A list to which any validation errors will be added.</param>
	/// <returns>A list of valid budgets read from the CSV file.</returns>
	public async Task<GetTransferList<GetBudgetTransactionImportInfo>> CreateValidBudgetTransactionsList(BudgetId budgetId, IFormFile file, CsvConfiguration csvConfig, List<string> errors)
	{
		var budgetTransactionsInfos = new List<GetBudgetTransactionImportInfo>();
		await using var stream = file.OpenReadStream();
		using var streamReader = new StreamReader(stream);
		using var csv = new CsvReader(streamReader, csvConfig);
		await csv.ReadAsync();
		var budgetTransactions = csv.GetRecords<GetBudgetTransactionTransferInfo>().MapToBudgetTransactionImportInfo(budgetId);
		int rowNumber = 0;

		foreach (var budget in budgetTransactions)
		{
			var results = await this.BudgetTransactionValidate(budget);
			rowNumber++;

			if (results.Any())
			{
				foreach (string result in results)
				{
					errors.Add($"row: {rowNumber}| error: {result}");
				}

				continue;
			}

			budgetTransactionsInfos.Add(budget);
		}

		return new GetTransferList<GetBudgetTransactionImportInfo> { CorrectList = budgetTransactionsInfos };
	}
}