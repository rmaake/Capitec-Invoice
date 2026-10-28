using Capitec.Api.Services;
using Capitec.Application.Services;
using Capitec.Application.Services.Abstractions;
using Capitec.Business.Constants;
using Capitec.DAL;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDAL(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(builder.Configuration.GetSection("Notifications").Get<NotificationScheduleOptions>() ?? new NotificationScheduleOptions());
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IOverdueNotificationService, OverdueNotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddHostedService<OverdueNotificationWorker>();
builder.Services.AddHealthChecks();

var app = builder.Build();

using var scope = app.Services.CreateScope();

var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

await RoleSeeder.SeedAsync(roleManager);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
