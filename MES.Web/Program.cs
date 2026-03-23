using MES.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// HttpClient ke API
builder.Services.AddHttpClient("MesApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:BaseUrl"] 
        ?? "http://localhost:5096"
    );
});

// Register service
builder.Services.AddScoped<IWorkOrderApiService, WorkOrderApiService>();

var app = builder.Build();

// Error handling (production)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=WorkOrder}/{action=Index}/{id?}");

app.Run();