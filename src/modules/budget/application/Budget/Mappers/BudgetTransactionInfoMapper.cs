using Intive.Patronage2023.Modules.Budget.Application.Budget.GettingBudgetTransactions;
using Intive.Patronage2023.Modules.Budget.Domain;

namespace Intive.Patronage2023.Modules.Budget.Application.Budget.Mappers;

/// <summary>
/// Mapper class.
/// </summary>
public static class BudgetTransactionInfoMapper
{
	/// <summary>
	/// Mapping method.
	/// </summary>
	/// <param name="query">Query.</param>
	/// <returns>Returns <ref name="TransactionInfo"/>BudgetInfo.</returns>
	public static IEnumerable<BudgetTransactionInfo> MapToTransactionInfo(this IEnumerable<BudgetTransactionAggregate> query) =>
		query.Select(x => new BudgetTransactionInfo
		{
			TransactionType = x.TransactionType,
			BudgetId = x.BudgetId,
			TransactionId = x.Id,
			Name = x.Name,
			Value = x.Value,
			BudgetTransactionDate = x.BudgetTransactionDate,
			CategoryType = x.CategoryType,
			Username = x.Username,
		});
}