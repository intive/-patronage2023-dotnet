using Intive.Patronage2023.Api.Configuration;
using Intive.Patronage2023.Modules.Example.Api;
using Intive.Patronage2023.Shared.Abstractions;
using Intive.Patronage2023.Shared.Abstractions.Commands;
using Intive.Patronage2023.Shared.Abstractions.Queries;
using Intive.Patronage2023.Shared.Infrastructure;
using Intive.Patronage2023.Shared.Infrastructure.EventDispachers;
using Intive.Patronage2023.Shared.Infrastructure.EventHandlers;
using Intive.Patronage2023.Shared.Infrastructure.Queries.QueryBus;
using Keycloak.AuthServices.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.AspNetCore.Mvc.Authorization;
using Intive.Patronage2023.Shared.Infrastructure.Commands.CommandBus;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpLogging(logging =>
{
	logging.LoggingFields = HttpLoggingFields.All;
	logging.RequestBodyLogLimit = 4096;
	logging.ResponseBodyLogLimit = 4096;
});

builder.Services.AddSharedModule();
builder.Services.AddExampleModule(builder.Configuration);
builder.Services.AddBudgetModule(builder.Configuration);

builder.Services.AddMediatR(cfg =>
{
	cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddControllers(options =>
	options.Filters.Add(new AuthorizeFilter(
		new AuthorizationPolicyBuilder()
			.RequireAuthenticatedUser()
			.Build())));

builder.Services.AddFromAssemblies(typeof(IDomainEventHandler<>));
builder.Services.AddFromAssemblies(typeof(IEventDispatcher<>));
builder.Services.AddFromAssemblies(typeof(ICommandHandler<>));
builder.Services.AddFromAssemblies(typeof(IQueryHandler<,>));

using var loggerFactory = LoggerFactory.Create(builder =>
{
	builder.AddSimpleConsole(i => i.ColorBehavior = LoggerColorBehavior.Disabled);
});

var logger = loggerFactory.CreateLogger<Program>();

builder.Logging.AddApplicationInsights(
		configureTelemetryConfiguration: (config) =>
			config.ConnectionString = builder.Configuration.GetConnectionString("APPLICATIONINSIGHTS_CONNECTION_STRING"),
		configureApplicationInsightsLoggerOptions: (options) => { });

builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("category", Microsoft.Extensions.Logging.LogLevel.Trace);

using var channel = new InMemoryChannel();

var app = builder.Build();

builder.Services.AddScoped<ICommandBus, CommandBus>();
builder.Services.AddScoped<IQueryBus, QueryBus>();

app.MapGet("/Test", async context =>
{
	logger.LogInformation("Testing logging in Program.cs");
	await context.Response.WriteAsync("Testing");
});
builder.Services.AddKeycloakAuthentication(builder.Configuration, configureOptions =>
{
	// turning off issuer validation and https
	configureOptions.RequireHttpsMetadata = false;
	configureOptions.TokenValidationParameters.ValidateIssuer = false;
});
builder.Services.AddAuthorization();

builder.Services.AddSwagger();

app.UseHttpLogging();
app.UseHttpsRedirection();

app.MapControllers();

app.UseExampleModule();
app.UseBudgetModule();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();