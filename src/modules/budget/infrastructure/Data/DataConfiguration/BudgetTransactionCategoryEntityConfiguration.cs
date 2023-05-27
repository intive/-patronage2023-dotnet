using Intive.Patronage2023.Modules.Budget.Contracts.ValueObjects;
using Intive.Patronage2023.Modules.Budget.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intive.Patronage2023.Modules.Budget.Infrastructure.Data.DataConfiguration;

/// <summary>
/// Transaction Category Aggregate Configuration.
/// </summary>
internal class BudgetTransactionCategoryEntityConfiguration : IEntityTypeConfiguration<TransactionCategoryAggregate>
{
	/// <inheritdoc/>
	public void Configure(EntityTypeBuilder<TransactionCategoryAggregate> builder)
	{
		builder.ToTable("BudgetTransactionCategory", "Budgets");
		builder.HasKey(x => x.Id);
		builder.HasIndex(x => x.Id);

		builder.HasOne<BudgetAggregate>().WithMany().HasForeignKey(k => k.BudgetId);

		builder.Property(e => e.Id)
			.HasConversion(BudgetConverters.TransactionCategoryId());
		builder.Property(e => e.BudgetId)
			.HasConversion(BudgetConverters.BudgetIdConverter());

		builder.Property(x => x.Id).HasColumnName("Id").HasDefaultValueSql("newsequentialid()").IsRequired();
		builder.Property(x => x.BudgetId).HasColumnName("BudgetId").IsRequired();
		builder.Property(x => x.Icon).HasColumnName("Icon").IsRequired();
		builder.Property(x => x.Name).HasColumnName("Name").IsRequired();
	}
}