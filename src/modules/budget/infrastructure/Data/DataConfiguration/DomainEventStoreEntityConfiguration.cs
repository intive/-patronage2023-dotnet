using Intive.Patronage2023.Modules.Budget.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Intive.Patronage2023.Modules.Budget.Infrastructure.Data.DataConfiguration;

/// <summary>
/// DomainEventStore configuration.
/// </summary>
internal class DomainEventStoreEntityConfiguration : IEntityTypeConfiguration<DomainEventStore>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<DomainEventStore> builder)
	{
		builder.HasKey(x => x.Id);
		builder.ToTable("DomainEventStore", "Budgets");
		builder.Property(x => x.Id).HasColumnName("Id");
		builder.Property(x => x.Data).HasColumnName("Data");
		builder.Property(x => x.Type).HasColumnName("Type");
		builder.Property(x => x.CreatedAt).HasColumnName("CreatedAt");
	}
}