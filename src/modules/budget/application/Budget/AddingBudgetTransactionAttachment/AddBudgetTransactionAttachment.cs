using Intive.Patronage2023.Modules.Budget.Application.Budget.Shared.Services;
using Intive.Patronage2023.Modules.Budget.Contracts.ValueObjects;
using Intive.Patronage2023.Modules.Budget.Domain;
using Intive.Patronage2023.Modules.Budget.Infrastructure.Data;
using Intive.Patronage2023.Shared.Abstractions.Commands;
using Intive.Patronage2023.Shared.Abstractions.Domain;
using Intive.Patronage2023.Shared.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Intive.Patronage2023.Modules.Budget.Application.Budget.AddingBudgetTransactionAttachment;

/// <summary>
/// Command adding budget transaction attachment.
/// </summary>
/// <param name="File">File.</param>
/// <param name="TransactionId">Budget transaction Id.</param>
public record AddBudgetTransactionAttachment(IFormFile File, TransactionId TransactionId) : ICommand;

/// <summary>
/// Method that handles adding attachment to budget transaction.
/// </summary>
public class HandleAddBudgetTransactionAttachment : ICommandHandler<AddBudgetTransactionAttachment>
{
	private readonly IAddTransactionAttachmentService addTransactionAttachmentService;
	private readonly IRepository<BudgetTransactionAggregate, TransactionId> budgetTransactionRepository;
	private readonly BudgetDbContext budgetDbContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="HandleAddBudgetTransactionAttachment"/> class.
	/// Constructor.
	/// </summary>
	/// <param name="addTransactionAttachmentService">Blob storage service.</param>
	/// <param name="budgetTransactionRepository">Budget transaction repository.</param>
	/// <param name="budgetDbContext">Budget Db Context.</param>
	public HandleAddBudgetTransactionAttachment(IAddTransactionAttachmentService addTransactionAttachmentService, IRepository<BudgetTransactionAggregate, TransactionId> budgetTransactionRepository, BudgetDbContext budgetDbContext)
	{
		this.addTransactionAttachmentService = addTransactionAttachmentService;
		this.budgetTransactionRepository = budgetTransactionRepository;
		this.budgetDbContext = budgetDbContext;
	}

	/// <inheritdoc/>
	public async Task Handle(AddBudgetTransactionAttachment command, CancellationToken cancellationToken)
	{
		var file = command.File;

		var attachmentFile = new FileModel()
		{
			FileName = command.TransactionId.Value.ToString(),
			Content = file.OpenReadStream(),
		};

		var transaction = await this.budgetTransactionRepository.GetById(command.TransactionId);

		if (transaction == null)
		{
			throw new InvalidOperationException("Transaction not found.");
		}

		var userBudget = await this.budgetDbContext.UserBudget.FirstOrDefaultAsync(b => b.BudgetId == transaction.BudgetId);

		string userId = userBudget!.UserId.Value.ToString();
		Uri fileUrl = await this.addTransactionAttachmentService.UploadFileAsync(userId, attachmentFile.FileName, attachmentFile.Content);

		transaction.AddAttachment(fileUrl);

		await this.budgetTransactionRepository.Persist(transaction ?? throw new InvalidOperationException());
	}
}