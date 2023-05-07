using System.Net;
using FluentValidation;
using Intive.Patronage2023.Api.User;
using Intive.Patronage2023.Api.User.CreatingUser;
using Intive.Patronage2023.Api.User.GettingUsers;
using Intive.Patronage2023.Api.User.SignIn;
using Intive.Patronage2023.Modules.Example.Application.Example;
using Intive.Patronage2023.Shared.Abstractions;
using Intive.Patronage2023.Shared.Abstractions.Commands;
using Intive.Patronage2023.Shared.Abstractions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Intive.Patronage2023.Api.Controllers;

/// <summary>
/// User Controller.
/// </summary>
[Route("user")]
[ApiController]
public class UserController : ControllerBase
{
	private readonly IQueryBus queryBus;
	private readonly ICommandBus commandBus;
	private readonly IValidator<SignInUser> signInUserValidator;
	private readonly IValidator<CreateUser> createUserValidator;

	/// <summary>
	/// Initializes a new instance of the <see cref="UserController"/> class.
	/// </summary>
	/// <param name="queryBus">Query bus.</param>
	/// <param name="commandBus">Command bus.</param>
	/// <param name="signInUserValidator">SignIn User validator.</param>
	/// <param name="createUserValidator">Create User validator.</param>
	public UserController(IQueryBus queryBus, ICommandBus commandBus, IValidator<SignInUser> signInUserValidator, IValidator<CreateUser> createUserValidator)
	{
		this.queryBus = queryBus;
		this.commandBus = commandBus;
		this.signInUserValidator = signInUserValidator;
		this.createUserValidator = createUserValidator;
	}

	/// <summary>
	/// Authenticates a user and returns a JWT token.
	/// </summary>
	/// <param name="command">The sign-in command, which includes the user email and password.</param>
	/// <returns>
	/// An HTTP response containing a JWT token if the sign-in was successful, or an error response
	/// if the sign-in failed.
	/// </returns>
	/// <remarks>
	/// Sample request:
	///
	///     {
	///        "email": "jkowalski@gmail.com",
	///        "password": "Password123!"
	///     }
	/// .</remarks>
	/// <exception cref="AppException">
	/// Thrown if one or more errors occur while trying to authenticate the user.
	/// </exception>
	/// <response code="200">Return Token class object.</response>
	/// <response code="401">Email or password is not valid.</response>
	/// <response code="500">Internal server error.</response>
	[ProducesResponseType(typeof(Token), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
	[AllowAnonymous]
	[HttpPost("sign-in")]
	public async Task<IActionResult> SignInUserAsync([FromBody] SignInUser command)
	{
		var validationResult = await this.signInUserValidator.ValidateAsync(command);
		if (validationResult.IsValid)
		{
			HttpResponseMessage response = await this.queryBus.Query<SignInUser, HttpResponseMessage>(command);

			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				return this.Unauthorized();
			}

			string responseContent = await response.Content.ReadAsStringAsync();
			if (!string.IsNullOrEmpty(responseContent))
			{
				Token? token = JsonConvert.DeserializeObject<Token>(responseContent);
				return this.Ok(token);
			}
		}

		throw new AppException("One or more error occured when trying to get token.", validationResult.Errors);
	}

	/// <summary>
	/// Creates user.
	/// </summary>
	/// <param name="command">Command.</param>
	/// <returns>Created command.</returns>
	/// <remarks>
	/// Sample request:
	///
	///     {
	///        "avatar": "1",
	///        "firstName": "Jan",
	///        "lastName": "Kowalski",
	///        "password": "Password123!",
	///        "email": "jkowalski@gmail.com"
	///     }
	/// .</remarks>
	/// <response code="200">Indicates if the request to create user was done correctly.</response>
	/// <response code="400">If the body is not valid.</response>
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[AllowAnonymous]
	[HttpPost("sign-up")]
	public async Task<IActionResult> SignUp([FromBody] CreateUser command)
	{
		var validationResult = await this.createUserValidator.ValidateAsync(command);
		if (validationResult.IsValid)
		{
			await this.commandBus.Send(command);
			return this.Ok();
		}

		throw new AppException("One or more error occured while trying to create user.", validationResult.Errors);
	}

	/// <summary>
	/// Retrieves list of users filtered by query.
	/// TODO: ADD AUTHORIZATION.
	/// </summary>
	/// <param name="query">Query.</param>
	/// <returns>Created command.</returns>
	/// <response code="200">Indicates if the request to create user was done correctly.</response>
	/// <response code="400">If the body is not valid.</response>
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[HttpGet("list")]
	public async Task<IActionResult> GetUsers([FromQuery] GetUsers query)
	{
		var result = await this.queryBus.Query<GetUsers, PagedList<UserInfo>>(query);
		return this.Ok(result);
	}
}