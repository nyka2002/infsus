using Microsoft.EntityFrameworkCore;
using RentIO.Data;
using RentIO.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRezervacijaService, RezervacijaService>();

builder.Services.AddControllersWithViews()
    .AddViewLocalization();

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("hr-HR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("hr-HR");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
