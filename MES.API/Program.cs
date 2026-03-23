using MES.Core.Interfaces;
using MES.Infrastructure.Data;
using MES.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using MES.API.Validators;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return new BadRequestObjectResult(new
            {
                success = false,
                message = "Validasi gagal",
                errors
            });
        };
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateWorkOrderValidator>();

builder.Services.AddDbContext<MesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
builder.Services.AddScoped<IStepService, StepService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();