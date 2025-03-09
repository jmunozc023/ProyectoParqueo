using ParqueoApp3.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Retrieve Key Vault endpoint from configuration

if (builder.Environment.IsProduction())
{
    var keyVaultEndpoint = new Uri(builder.Configuration["VaultKey:KeyVaultURL"]!);
    var secretClient = new SecretClient(keyVaultEndpoint, new DefaultAzureCredential());
    KeyVaultSecret kvs = secretClient.GetSecret("ParqueoAppSecret2");
    builder.Services.AddDbContext<AppDBContext>(o => o.UseSqlServer(kvs.Value));
}

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<AppDBContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Add services to the container.
builder.Services.AddControllersWithViews();





builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Acceso/LogIn";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.LogoutPath = "/Acceso/LogOut";
    options.AccessDeniedPath = "/Acceso/LogIn";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=LogIn}/{id?}");

app.Run();
