using FluentAssertions;
using Intive.Patronage2023.Modules.Example.Application.Example.GettingExamples;
using Intive.Patronage2023.Modules.Example.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Intive.Patronage2023.Modules.Example.Application.IntegrationsTests;

public class ExampleAggregate
{
	public int Id { get; set; }
	public string Name { get; set; }

	public ExampleAggregate(int id, string name)
	{
		this.Id = id;
		this.Name = name;
	}
}

public class GetExampleQueryHandlerTests
{
	[Fact]
	public async Task Handle_ValidQuery_ReturnsExamples()
	{
		// Arrange
		string connectionString = "Server=db;Database=ExampleDatabase;User Id=sa;Password=S3cur3P@ssW0rd!;MultipleActiveResultSets=true";
		var options = new DbContextOptionsBuilder<ExampleDbContext>()
			.UseSqlServer(connectionString)
			.Options;
		var dbContext = new ExampleDbContext(options);
		var examples = new List<ExampleAggregate>
			{
				new ExampleAggregate(1, "Example 1"),
				new ExampleAggregate(2, "Example 2"),
				new ExampleAggregate(3, "Example 3")
			};

		//dbContext.Examples.AddRange(examples);
		await dbContext.SaveChangesAsync();
		dbContext.SaveChanges();
		var query = new GetExamples();
		var handler = new GetExampleQueryHandler(dbContext);

		// Act
		var result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Items.Should().HaveCount(examples.Count);
		//AndConstraint<FluentAssertions.Collections.GenericCollectionAssertions<ExampleInfo>> andConstraint = result.Items.Should().BeEquivalentTo(examples, options => options.ExcludingMissingMembers());
	}
}

