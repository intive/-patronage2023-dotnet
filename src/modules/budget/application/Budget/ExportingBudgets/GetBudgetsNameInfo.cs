namespace Intive.Patronage2023.Modules.Budget.Application.Budget.ExportingBudgets;

/// <summary>
///  The record is used to store information about the names of budgets associated with a specific user.
/// </summary>
public record GetBudgetsNameInfo()
{
	/// <summary>
	/// Represents the names of budgets.
	/// </summary>
	public List<string>? BudgetName { get; init; }
}