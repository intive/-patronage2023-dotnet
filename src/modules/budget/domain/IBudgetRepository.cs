namespace Intive.Patronage2023.Modules.Budget.Domain;

/// <summary>
/// Interface of repository for BudgetAggregate.
/// </summary>
public interface IBudgetRepository : IRepository<BudgetAggregate, Guid>
{
    /// <summary>
    /// Checks if budget of given name exists.
    /// </summary>
    /// <param name="userId">Budget owner id.</param>
    /// <param name="name">Budget name.</param>
    /// <returns>True if exists.</returns>
    bool ExistsByName(Guid? userId, string name);
}