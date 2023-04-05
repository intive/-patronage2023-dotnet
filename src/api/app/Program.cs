using Intive.Patronage2023.Modules.Budget.Api;
using Intive.Patronage2023.Modules.Example.Api;
using Intive.Patronage2023.Api.Configuration;
using Intive.Patronage2023.Shared.Abstractions;
using Intive.Patronage2023.Shared.Abstractions.Commands;
using Intive.Patronage2023.Shared.Abstractions.Queries;
using Intive.Patronage2023.Shared.Infrastructure;
using Intive.Patronage2023.Shared.Infrastructure.Commands.CommandBus;
using Intive.Patronage2023.Shared.Infrastructure.EventDispachers;
using Intive.Patronage2023.Shared.Infrastructure.EventHandlers;
using Intive.Patronage2023.Shared.Infrastructure.Queries.QueryBus;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

string corsPolicyName = "CorsPolicy";

builder.Services.AddCors(builder.Configuration, corsPolicyName);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddExampleModule(builder.Configuration);
builder.Services.AddBudgetModule(builder.Configuration);
builder.Services.AddHttpLogging(logging =>
{
	logging.LoggingFields = HttpLoggingFields.All;
	logging.RequestBodyLogLimit = 4096;
	logging.ResponseBodyLogLimit = 4096;
});

builder.Services.AddSharedModule();

builder.Services.AddMediatR(cfg =>
{
	cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Version = "v1",
		Title = "Intive Patronage2023 Some Title Api",
		Description = "An ASP.NET Core Web API for managing bills and more",
	});

	// Searching for all files with ".Api.xml" suffix, which should be api docs,
	// in build directory and attach them to swagger
	var xmlFiles = Directory.GetFiles(
		AppContext.BaseDirectory,
		"*.Api.xml",
		SearchOption.TopDirectoryOnly).ToList();
	xmlFiles.ForEach(xmlFile => options.IncludeXmlComments(xmlFile));
});

builder.Services.AddControllers();

builder.Services.AddFromAssemblies(typeof(IDomainEventHandler<>));
builder.Services.AddFromAssemblies(typeof(IEventDispatcher<>));
builder.Services.AddFromAssemblies(typeof(ICommandHandler<>));
builder.Services.AddFromAssemblies(typeof(IQueryHandler<,>));
builder.Services.AddScoped<ICommandBus, CommandBus>();
builder.Services.AddScoped<IQueryBus, QueryBus>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors(corsPolicyName);

app.UseHttpLogging();
app.UseHttpsRedirection();

app.MapControllers();

app.UseExampleModule();

app.Run();