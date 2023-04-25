using FluentValidation;
using Intive.Patronage2023.Modules.Budget.Application.Budget;
using Intive.Patronage2023.Modules.Budget.Application.Budget.CreatingBudget;
using Intive.Patronage2023.Modules.Budget.Application.Budget.CreatingBudgetTransaction;
using Intive.Patronage2023.Modules.Budget.Application.Budget.GettingBudgets;
using Intive.Patronage2023.Modules.Budget.Application.Budget.GettingBudgetTransactions;
using Intive.Patronage2023.Modules.Budget.Contracts.ValueObjects;
using Intive.Patronage2023.Shared.Abstractions;
using Intive.Patronage2023.Shared.Abstractions.Commands;
using Intive.Patronage2023.Shared.Abstractions.Errors;
using Intive.Patronage2023.Shared.Abstractions.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Intive.Patronage2023.Modules.Budget.Api.Controllers;

/// <summary>
/// Budget controller.
/// </summary>
[ApiController]
[Route("budgets")]
public class BudgetController : ControllerBase
{
	private readonly ICommandBus commandBus;
	private readonly IQueryBus queryBus;
	private readonly IValidator<CreateBudget> createBudgetValidator;
	private readonly IValidator<GetBudgets> getBudgetsValidator;
	private readonly IValidator<CreateBudgetTransaction> createTransactionValidator;
	private readonly IValidator<GetBudgetTransaction> getBudgetTransactionValidator;

	/// <summary>
	/// Initializes a new instance of the <see cref="BudgetController"/> class.
	/// </summary>
	/// <param name="commandBus">Command bus.</param>
	/// <param name="queryBus">Query bus.</param>
	/// <param name="createBudgetValidator">Create Budget validator.</param>
	/// <param name="getBudgetsValidator">Get Budgets validator.</param>
	/// <param name="createTransactionValidator">Create Transaction validator.</param>
	/// <param name="getBudgetTransactionValidator">Get Budget Transaction validator.</param>
	public BudgetController(
		ICommandBus commandBus,
		IQueryBus queryBus,
		IValidator<CreateBudget> createBudgetValidator,
		IValidator<GetBudgets> getBudgetsValidator,
		IValidator<CreateBudgetTransaction> createTransactionValidator,
		IValidator<GetBudgetTransaction> getBudgetTransactionValidator)
	{
		this.createBudgetValidator = createBudgetValidator;
		this.getBudgetsValidator = getBudgetsValidator;
		this.commandBus = commandBus;
		this.queryBus = queryBus;
		this.createTransactionValidator = createTransactionValidator;
		this.getBudgetTransactionValidator = getBudgetTransactionValidator;
	}

	/// <summary>
	/// Get Budgets.
	/// </summary>
	/// <param name="request">Query parameters.</param>
	/// <returns>Paged and sorted list of budgets.</returns>
	/// <remarks>
	/// Sample request:
	///
	///     POST
	///     {
	///         "pageSize": 10,
	///         "pageIndex": 1,
	///         "search": "in",
	///         "sortDescriptors": [
	///         {
	///             "columnName": "name",
	///             "sortAscending": true
	///         }
	///       ]
	///     }
	/// .</remarks>
	/// <response code="200">Returns the list of budgets corresponding to the query.</response>
	/// <response code="400">If the query is not valid.</response>
	[HttpPost]
	[Route("list")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> GetBudgets([FromBody] GetBudgets request)
	{
		var validationResult = await this.getBudgetsValidator.ValidateAsync(request);
		if (validationResult.IsValid)
		{
			var pagedList = await this.queryBus.Query<GetBudgets, PagedList<BudgetInfo>>(request);
			return this.Ok(pagedList);
		}

		throw new AppException("One or more error occured when trying to get Budgets.", validationResult.Errors);
	}

	/// <summary>
	/// Creates Budget.
	/// </summary>
	/// <param name="request">Request.</param>
	/// <returns>Created Result.</returns>
	/// <remarks>
	/// Sample request:
	///
	///     {
	///        "id": {
	///           "value": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
	///        },
	///        "name": "string"
	///     }
	/// .</remarks>
	/// <response code="201">Returns the newly created item.</response>
	/// <response code="400">If the body is not valid.</response>
	/// <response code="401">If the user is unauthorized.</response>
	[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(ErrorExample), StatusCodes.Status400BadRequest)]
	[HttpPost]
	public async Task<IActionResult> CreateBudget([FromBody] CreateBudget request)
	{
		var validationResult = await this.createBudgetValidator.ValidateAsync(request);
		if (validationResult.IsValid)
		{
			await this.commandBus.Send(request);
			return this.Created(string.Empty, request.Id);
		}

		throw new AppException("One or more error occured when trying to create Budget.", validationResult.Errors);
	}

	/// <summary>
	/// Creates Income / Expense Budget Transaction.
	/// </summary>
	/// <param name="budgetId">Budget Id.</param>
	/// <param name="command">Command that creates Transaction to specific budget.</param>
	/// <returns>Created Result.</returns>
	/// <remarks>
	/// Sample request:
	///
	/// Types: "Income" , "Expense"
	///
	/// Value must be positive for income or negative for expense.
	///
	/// Categories: "HomeSpendings" ,  "Subscriptions" , "Car" , "Grocery" ,
	///
	///     POST
	///     {
	///         "type": "Income",
	///         "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
	///         "name": "string",
	///         "value": 1,
	///         "category": "HomeSpendings",
	///         "transactionDate": "2023-06-20T14:15:47.392Z"
	///     }
	/// .</remarks>
	/// <response code="201">Returns the newly created item.</response>
	/// <response code="400">If the body is not valid.</response>
	/// <response code="401">If the user is unauthorized.</response>
	[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(ErrorExample), StatusCodes.Status400BadRequest)]
	[HttpPost("{budgetId:guid}/transaction")]
	public async Task<IActionResult> CreateNewTransaction([FromRoute] Guid budgetId, [FromBody] CreateTransaction command)
	{
		Guid transactionId = command.Id == default ? Guid.NewGuid() : command.Id;
		var transactionDate = command.TransactionDate == DateTime.MinValue ? DateTime.UtcNow : command.TransactionDate;

		var newBudgetTransaction = new CreateBudgetTransaction(command.Type, transactionId, budgetId, command.Name, command.Value, command.Category, transactionDate);

		var validationResult = await this.createTransactionValidator.ValidateAsync(newBudgetTransaction);
		if (validationResult.IsValid)
		{
			await this.commandBus.Send(newBudgetTransaction);
			return this.Created(string.Empty, newBudgetTransaction.Id);
		}

		throw new AppException("One or more error occured when trying to create Budget Transaction.", validationResult.Errors);
	}

	/// <summary>
	/// Get Budget by id with transactions.
	/// </summary>
	/// <param name="budgetId">Budget Id.</param>
	/// <param name="request">Query parameters.</param>
	/// <returns>Budget details, list of incomes and Expenses.</returns>
	/// <remarks>
	/// Sample request:
	///
	///     {
	///         "pageSize": 1,
	///         "pageIndex": 1,
	///     }
	/// .</remarks>
	/// <response code="200">Returns the list of Budget details, list of incomes and Expenses corresponding to the query.</response>
	/// <response code="400">If the query is not valid.</response>
	/// <response code="401">If the user is unauthorized.</response>
	[HttpPost("{budgetId:guid}/transactions")]
	[ProducesResponseType(typeof(PagedList<BudgetInfo>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ErrorExample), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> GetTransactionByBudgetId([FromRoute] Guid budgetId, [FromBody] GetPageSizeAndPageIndex request)
	{
		var getBudgetTransactions = new GetBudgetTransaction
		{
			BudgetId = new BudgetId(budgetId),
			PageSize = request.PageSize,
			PageIndex = request.PageIndex,
		};
		var validationResult = await this.getBudgetTransactionValidator.ValidateAsync(getBudgetTransactions);
		if (validationResult.IsValid)
		{
			var pagedList = await this.queryBus.Query<GetBudgetTransaction, PagedList<BudgetTransactionInfo>>(getBudgetTransactions);
			return this.Ok(pagedList);
		}

		throw new AppException("One or more error occured when trying to get Transactions.", validationResult.Errors);
	}
}