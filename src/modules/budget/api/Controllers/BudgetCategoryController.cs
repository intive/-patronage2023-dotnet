using Intive.Patronage2023.Modules.Budget.Application.TransactionCategories.AddingTransactionCategory;
using Intive.Patronage2023.Modules.Budget.Application.TransactionCategories.DeletingTransactionCategory;
using Intive.Patronage2023.Modules.Budget.Application.TransactionCategories.GettingTransactionCategories;
using Intive.Patronage2023.Modules.Budget.Contracts.ValueObjects;
using Intive.Patronage2023.Shared.Abstractions;
using Intive.Patronage2023.Shared.Abstractions.Commands;
using Intive.Patronage2023.Shared.Abstractions.Queries;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intive.Patronage2023.Modules.Budget.Api.Controllers;

/// <summary>
/// Controller that contains endpoints managing Budget Categories.
/// </summary>
[ApiController]
[Route("categories")]
public class BudgetCategoryController : ControllerBase
{
	private readonly ICommandBus commandBus;
	private readonly IQueryBus queryBus;
	private readonly IExecutionContextAccessor contextAccessor;
	private readonly IAuthorizationService authorizationService;

	/// <summary>
	/// Initializes a new instance of the <see cref="BudgetCategoryController"/> class.
	/// </summary>
	/// <param name="commandBus">Bus that managed persisting changes in database.</param>
	/// <param name="queryBus">Bus that get data from database.</param>
	/// <param name="contextAccessor">An instance of the ContextAccessor class that provides access to the current context.</param>
	/// <param name="authorizationService">An instance of the AuthorizationService class that provides authorization functionality.</param>
	public BudgetCategoryController(ICommandBus commandBus, IQueryBus queryBus, IExecutionContextAccessor contextAccessor, IAuthorizationService authorizationService)
	{
		this.commandBus = commandBus;
		this.queryBus = queryBus;
		this.contextAccessor = contextAccessor;
		this.authorizationService = authorizationService;
	}

	/// <summary>
	/// Retrieves the budget transaction categories list.
	/// </summary>
	/// <param name="budgetId">The ID of the budget for which to retrieve the transaction categories.</param>
	/// <returns>A Task representing the asynchronous operation that returns an IActionResult.</returns>
	[HttpGet]
	[Route("{budgetId:guid}/list")]
	public async Task<IActionResult> GetBudgetCategories([FromRoute]Guid budgetId)
	{
		var query = new GetTransactionCategories(new BudgetId(budgetId));
		var categories = await this.queryBus.Query<GetTransactionCategories, TransactionCategoriesInfo>(query);
		return this.Ok(categories);
	}

	/// <summary>
	/// Adds a transaction category to a budget.
	/// </summary>
	/// <param name="budgetId">The ID of the budget to which the category will be added.</param>
	/// <param name="categoryName">The transaction category name.</param>
	/// <returns>A Task representing the asynchronous operation that returns an IActionResult.</returns>
	[HttpPost]
	[Route("{budgetId:guid}/add/{categoryName}")]
	public async Task<IActionResult> AddCategoryToBudget([FromRoute]Guid budgetId, [FromRoute]string categoryName)
	{
		var command = new AddCategory(new BudgetId(budgetId), categoryName);
		await this.commandBus.Send(command);
		return this.Ok();
	}

	/// <summary>
	/// Deletes a transaction category from a budget.
	/// </summary>
	/// <param name="budgetId">The ID of the budget from which to delete the transaction category.</param>
	/// <param name="categoryName">The name of the category to delete.</param>
	/// <returns>A Task representing the asynchronous operation that returns an IActionResult.</returns>
	[HttpDelete]
	[Route("{budgetId:guid}/delete/{categoryName}")]
	public async Task<IActionResult> DeleteTransactionCategoryFromBudget([FromRoute]Guid budgetId, [FromRoute]string categoryName)
	{
		var command = new DeleteTransactionCategory(new BudgetId(budgetId), categoryName);
		await this.commandBus.Send(command);
		return this.Ok();
	}
}