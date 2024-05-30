using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using ReviewService.Bus;
using ReviewService.Data;
using ReviewService.Interfaces;
using ReviewService.Services;

var builder = WebApplication.CreateBuilder(args);

// Read environment variables and trim any extraneous quotes
var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST")?.Trim('"');
var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT")?.Trim('"');
var postgresDb = Environment.GetEnvironmentVariable("POSTGRES_DB")?.Trim('"');
var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER")?.Trim('"');
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")?.Trim('"');
var serviceBusConnectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTION_STRING")?.Trim('"');

// Replace placeholders in the configuration
builder.Configuration["ConnectionStrings:DefaultConnection"] = $"Host={postgresHost};Port={postgresPort};Database={postgresDb};Username={postgresUser};Password={postgresPassword};";
builder.Configuration["ServiceBus:ConnectionString"] = serviceBusConnectionString;

// Log the connection string for debugging
Console.WriteLine($"ServiceBus Connection String: {serviceBusConnectionString}");

// Add services to the container.
builder.Services.AddScoped<IReviewService, ReviewComService>();
builder.Services.AddControllers();

// Add DbContext
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the Azure Service Bus MessageSender and MessageReceiverService
var topicName = builder.Configuration["ServiceBus:TopicName"];
var subscriptionName = builder.Configuration["ServiceBus:SubscriptionName"];

builder.Services.AddSingleton(new MessageSender(serviceBusConnectionString, topicName));
builder.Services.AddHostedService(provider =>
    new MessageReceiverService(serviceBusConnectionString, topicName, subscriptionName));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Automatically apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
