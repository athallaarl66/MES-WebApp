using MES.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// SERVICES REGISTRATION 
builder.Services.AddControllersWithViews();
// untuk web biar  handle JSON dari API dengan bener
builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

// HttpClient ke API
builder.Services.AddHttpClient("MesApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5096"
    );
});

// CORS Policy  
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IWorkOrderApiService, WorkOrderApiService>();

var app = builder.Build();

//MIDDLEWARE PIPELINE 

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

 app.UseRouting(); //menentukan rute dulu

app.UseCors();    //Cek izin akses (CORS harus setelah Routing)

app.UseAuthorization(); //Cek siapa yang akses

//ENDPOINTS
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=WorkOrder}/{action=Index}/{id?}");

app.MapControllers(); //untuk Controller API di dalam Web bisa jalan

app.Run();