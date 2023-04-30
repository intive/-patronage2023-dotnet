using Intive.Patronage2023.Modules.Budget.Contracts.ValueObjects;
using Intive.Patronage2023.Modules.Budget.Domain;
using Intive.Patronage2023.Modules.Budget.Infrastructure.Data;
using Intive.Patronage2023.Shared.Abstractions.Commands;

using Microsoft.EntityFrameworkCore;

namespace Intive.Patronage2023.Modules.Budget.Application.Budget.RemovingBudgetTransactions;

/// <summary>
/// Remove Budget Transactions command.
/// </summary>
/// <param name="Id">Budget identifier.</param>
public record RemoveBudgetTransactions(Guid Id) : ICommand;

/// <summary>
/// Remove Budget Transactions.
/// </summary>
public class HandleRemoveBudgetTransactions : ICommandHandler<RemoveBudgetTransactions>
{
	private readonly IBudgetTransactionRepository budgetTransactionRepository;
	private readonly BudgetDbContext budgetDbContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="HandleRemoveBudgetTransactions"/> class.
	/// </summary>
	/// <param name="budgetTransactionRepository">Repository that manages Budget Transaction aggregate root.</param>
	/// <param name="budgetDbContext">Repository that manages Budget aggregate root.</param>
	public HandleRemoveBudgetTransactions(IBudgetTransactionRepository budgetTransactionRepository, BudgetDbContext budgetDbContext)
	{
		this.budgetTransactionRepository = budgetTransactionRepository;
		this.budgetDbContext = budgetDbContext;
	}

	/// <inheritdoc/>
	public async Task Handle(RemoveBudgetTransactions command, CancellationToken cancellationToken)
	{
		var budgetId = new BudgetId(command.Id);
		bool isDeleted = true;
		//// var budget = await this.budgetRepository.GetById(budgetId);
		var transactions = await this.budgetDbContext.Transaction
			.Where(x => x.BudgetId == budgetId)
			.ToListAsync(cancellationToken: cancellationToken);

		foreach (var transaction in transactions)
		{
			transaction.UpdateIsRemoved(isDeleted);
			await this.budgetTransactionRepository.Update(transaction);
		}
	}
}