using MES.Core.Interfaces;
using MES.Infrastructure.Data;
using MES.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using MES.API.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// validators workorders gasingkron
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateWorkOrderValidator>();

builder.Services.AddDbContext<MesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// workorder
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
// stepservice
builder.Services.AddScoped<IStepService, StepService>();

// builder
var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();