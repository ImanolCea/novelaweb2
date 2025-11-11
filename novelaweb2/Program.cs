using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using novelaweb2.Data;
using novelaweb2.Models;

var builder = WebApplication.CreateBuilder(args);

// === Configuración DB: lee DefaultConnection de appsettings.json ===
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<WebNovelasDbContext>(options =>
    options.UseSqlServer(connectionString));

// === Sesiones ===
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromDays(7);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WebNovelasDbContext>();
    DbInitializer.SeedRoles(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // <-- importante

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
