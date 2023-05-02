using Intive.Patronage2023.Modules.Budget.Contracts.Events;
using Intive.Patronage2023.Modules.Budget.Contracts.TransactionEnums;
using Intive.Patronage2023.Shared.Infrastructure.Domain;
using Intive.Patronage2023.Modules.Budget.Contracts.ValueObjects;
using Intive.Patronage2023.Modules.Budget.Domain.Rules;

namespace Intive.Patronage2023.Modules.Budget.Domain;

/// <summary>
/// Budget Transaction of aggregate root.
/// </summary>
public class BudgetTransactionAggregate : Aggregate
{
	private BudgetTransactionAggregate()
	{
	}

	private BudgetTransactionAggregate(TransactionId id, BudgetId budgetId, TransactionType transactionType, string name, decimal value, CategoryType categoryType, DateTime budgetTransactionDate)
	{
		var budgetTransactionCreated = new BudgetTransactionCreatedDomainEvent(id, budgetId, transactionType, name, value, categoryType, budgetTransactionDate);
		this.Apply(budgetTransactionCreated, this.Handle);
	}

	/// <summary>
	/// Reference to budget ID.
	/// </summary>
	public BudgetId BudgetId { get; private set; }

	/// <summary>
	/// Reference to transaction ID.
	/// </summary>
	public TransactionId Id { get; private set; }

	/// <summary>
	/// Budget Transaction eg. income/Expense.
	/// </summary>
	public TransactionType TransactionType { get; private set; }

	/// <summary>
	/// Budget Transaction name.
	/// </summary>
	public string Name { get; private set; } = default!;

	/// <summary>
	/// Value of new created income/Expense.
	/// </summary>
	public decimal Value { get; private set; }

	/// <summary>
	/// Category eg. "Home Spendings," "Subscriptions," "Car," "Grocery".
	/// </summary>
	public CategoryType CategoryType { get; private set; }

	/// <summary>
	/// Budget Transaction creation date.
	/// </summary>
	public DateTime BudgetTransactionDate { get; private set; }

	/// <summary>
	/// Budget Transaction creation date.
	/// </summary>
	public DateTime CreatedOn { get; private set; }

	/// <summary>
	/// Status of budget.
	/// </summary>
	public Status Status { get; private set; } = default;

	/// <summary>
	/// Create Budget Transaction.
	/// </summary>
	/// <param name="id">Transaction Id.</param>
	/// <param name="budgetId">Budget Id.</param>
	/// <param name="transactionType">Enum of Income or Expense.</param>
	/// <param name="name">Name of income or Expense.</param>
	/// <param name="value">Value of income or Expense.</param>
	/// <param name="categoryType">Enum of income/Expense Categories.</param>
	/// <param name="budgetTransactionDate">Date of Creating Transaction.</param>
	/// <returns>New aggregate.</returns>
	public static BudgetTransactionAggregate Create(TransactionId id, BudgetId budgetId, TransactionType transactionType, string name, decimal value, CategoryType categoryType, DateTime budgetTransactionDate)
	{
		return new BudgetTransactionAggregate(id, budgetId, transactionType, name, value, categoryType, budgetTransactionDate);
	}

	/// <summary>
	/// This method updates the "soft delete" flag for budget transactions.
	/// </summary>
	/// <param name="status">Soft Delete Status.</param>
	public void SoftRemove(Status status)
	{
		this.CheckRule(new SuperImportantBudgetBusinessRuleForStatus(status));

		var evt = new BudgetTransactionSoftDeleteDomainEvent(this.Id, status);

		this.Apply(evt, this.Handle);
	}

	private void Handle(BudgetTransactionSoftDeleteDomainEvent @event)
	{
		this.Status = @event.Status;
	}

	private void Handle(BudgetTransactionCreatedDomainEvent @event)
	{
		this.Id = @event.Id;
		this.BudgetId = @event.BudgetId;
		this.TransactionType = @event.TransactionType;
		this.Name = @event.Name;
		this.Value = @event.Value;
		this.CategoryType = @event.CategoryType;
		this.BudgetTransactionDate = @event.BudgetTransactionDate;
		this.CreatedOn = @event.Timestamp;
	}
}