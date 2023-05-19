using System.Text.Json.Serialization;
using Intive.Patronage2023.Modules.User.Contracts;

namespace Intive.Patronage2023.Modules.User.Application.GettingUsers;

/// <summary>
/// User information.
/// </summary>
public record UserInfo()
{
	/// <summary>
	/// User Id.
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// User email.
	/// </summary>
	public string Email { get; set; } = null!;

	/// <summary>
	/// User first name.
	/// </summary>
	public string FirstName { get; set; } = null!;

	/// <summary>
	/// User last name.
	/// </summary>
	public string LastName { get; set; } = null!;

	/// <summary>
	/// User additional information.
	/// </summary>
	[JsonIgnore]
	public UserAttributes Attributes { get; set; } = null!;

	/// <summary>
	/// User Avatar.
	/// </summary>
	public string Avatar => this.Attributes?.Avatar?.FirstOrDefault() ?? string.Empty;

	/// <summary>
	/// User created timestamp.
	/// </summary>
	public long CreatedTimestamp { get; set; }

	/// <summary>
	/// User created with email or via 3rd party services.
	/// </summary>
	public string CreatedVia { get; set; } = "Email";
}