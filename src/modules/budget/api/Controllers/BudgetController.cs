using FluentValidation;

using Intive.Patronage2023.Modules.Budget.Application.Budget;
using Intive.Patronage2023.Modules.Budget.Application.Budget.CreatingBudget;
using Intive.Patronage2023.Modules.Budget.Application.Budget.GettingBudgets;
using Intive.Patronage2023.Shared.Abstractions;
using Intive.Patronage2023.Shared.Abstractions.Commands;
using Intive.Patronage2023.Shared.Abstractions.Queries;

using Microsoft.AspNetCore.Mvc;

namespace Intive.Patronage2023.Modules.Budget.Api.Controllers;

/// <summary>
/// Budget controller.
/// </summary>
[ApiController]
[Route("[controller]")]
public class BudgetController : ControllerBase
{
	private readonly ICommandBus commandBus;
	private readonly IQueryBus queryBus;
	private readonly IValidator<CreateBudget> createBudgetValidator;
	private readonly IValidator<GetBudgets> getBudgetsValidator;

	/// <summary>
	/// Initializes a new instance of the <see cref="BudgetController"/> class.
	/// </summary>
	/// <param name="commandBus">Command bus.</param>
	/// <param name="queryBus">Query bus.</param>
	/// <param name="createBudgetValidator">Create Budget validator.</param>
	/// <param name="getBudgetsValidator">Get Budgets validator.</param>
	public BudgetController(ICommandBus commandBus, IQueryBus queryBus, IValidator<CreateBudget> createBudgetValidator, IValidator<GetBudgets> getBudgetsValidator)
	{
		this.createBudgetValidator = createBudgetValidator;
		this.getBudgetsValidator = getBudgetsValidator;
		this.commandBus = commandBus;
		this.queryBus = queryBus;
	}

	/// <summary>
	/// Get Budgets.
	/// </summary>
	/// <param name="request">Query parameters.</param>
	/// <returns>Paged list of Budgets.</returns>
	/// <response code="200">Returns the list of Budgets corresponding to the query.</response>
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
	///     POST
	///     {
	///        "Name": "Budget"
	///     }
	/// .</remarks>
	/// <response code="201">Returns the newly created item.</response>
	/// <response code="400">If the body is not valid.</response>
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[HttpPost]
	public async Task<IActionResult> CreateBudget([FromBody] CreateBudget request)
	{
		var validationResult = await this.createBudgetValidator.ValidateAsync(request);
		if (validationResult.IsValid)
		{
			await this.commandBus.Send(request);
			return this.Created($"Budget/{request.Id}", request.Id);
		}

		throw new AppException("One or more error occured when trying to create Budget.", validationResult.Errors);
	}
}