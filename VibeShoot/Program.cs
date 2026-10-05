using Microsoft.EntityFrameworkCore;
using System;
using VibeShoot.Data;
using VibeShoot.Services;

var builder = WebApplication.CreateBuilder(args);

// HTTPS: redirect http:// to https://, send HSTS and mark cookies Secure. Controlled by "Https:Enabled".
var httpsEnabled = builder.Configuration.GetValue<bool>("Https:Enabled");

builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(o =>
{
    o.Cookie.SecurePolicy = httpsEnabled ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddScoped<MediaStore>();
builder.Services.AddDatabaseDataProtection("VibeShoot");     // sign-in/form keys survive restarts
builder.Services.AddVibeShootRateLimits();

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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    if (httpsEnabled) app.UseHsts();
}

// If the host's SSL sits in front of the app and forwards plain HTTP, honour its "was HTTPS" header so the
// redirect below doesn't loop. Only the scheme is trusted (not X-Forwarded-For), so rate limits can't be dodged.
var forwarded = new Microsoft.AspNetCore.Builder.ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto,
};
forwarded.KnownIPNetworks.Clear();
forwarded.KnownProxies.Clear();
app.UseForwardedHeaders(forwarded);

if (httpsEnabled) app.UseHttpsRedirection();
app.UseSecurityHeaders();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
