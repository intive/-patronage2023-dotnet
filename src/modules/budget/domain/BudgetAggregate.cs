namespace Intive.Patronage2023.Modules.Budget.Domain;

using Intive.Patronage2023.Modules.Budget.Contracts.Events;
using Intive.Patronage2023.Modules.Budget.Domain.Rules;
using Intive.Patronage2023.Shared.Infrastructure.Domain;

/// <summary>
/// Budget of aggregate root.
/// </summary>
public class BudgetAggregate : Aggregate
{
	private BudgetAggregate(Guid id, string name)
	{
		var vudgetCreated = new BudgetCreatedDomainEvent(id, name);
		this.Apply(vudgetCreated, this.Handle);
	}

	/// <summary>
	/// Budget identifier.
	/// </summary>
	public Guid Id { get; private set; }

	/// <summary>
	/// Budget name.
	/// </summary>
	public string Name { get; private set; } = default!;

	/// <summary>
	/// Budget creation date.
	/// </summary>
	public DateTime CreatedOn { get; private set; }

	/// <summary>
	/// Create Budget.
	/// </summary>
	/// <param name="id">Unique identifier.</param>
	/// <param name="name">Budget name.</param>
	/// <returns>New aggregate.</returns>
	public static BudgetAggregate Create(Guid id, string name)
	{
		return new BudgetAggregate(id, name);
	}

	/// <summary>
	/// Update Budget name.
	/// </summary>
	/// <param name="name">New name.</param>
	public void UpdateName(string name)
	{
		this.CheckRule(new SuperImportantBudgetBusinessRule(name));

		var evt = new BudgetNameUpdatedDomainEvent(this.Id, name);

		this.Apply(evt, this.Handle);
	}

	private void Handle(BudgetNameUpdatedDomainEvent @event)
	{
		this.Name = @event.NewName;
	}

	private void Handle(BudgetCreatedDomainEvent @event)
	{
		this.Id = @event.Id;
		this.Name = @event.Name;
		this.CreatedOn = @event.Timestamp;
	}
}