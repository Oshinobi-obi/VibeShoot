using Microsoft.EntityFrameworkCore;
using System;
using VibeShoot.Data;
using VibeShoot.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<MediaStore>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

// Create the database (if needed), apply migrations and import existing images/photographers.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DbSeeder.SeedAsync(db, app.Environment, logger);
}

// HTTPS is off until the sites have SSL certificates. Set "Https:Enabled": true in appsettings to turn it back on.
var httpsEnabled = app.Configuration.GetValue<bool>("Https:Enabled");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    if (httpsEnabled) app.UseHsts();
}

if (httpsEnabled) app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
