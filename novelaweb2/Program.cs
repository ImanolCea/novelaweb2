using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;

var builder = WebApplication.CreateBuilder(args);

// =======================================
// 🔹 CONFIGURACIÓN BASE DE DATOS
// =======================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<WebNovelasDbContext>(options =>
    options.UseSqlServer(connectionString));

// =======================================
// 🔹 SESIONES
// =======================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromDays(7);
});


// =======================================
// 🔹 MVC
// =======================================
builder.Services.AddControllersWithViews();

var app = builder.Build();

// =======================================
// 🔹 MIDDLEWARE
// =======================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

// =======================================
// 🔹 RUTAS PERSONALIZADAS
// =======================================

// Ruta para novelas (permite acceder con /Novelas/Details/3)
app.MapControllerRoute(
    name: "novelas",
    pattern: "Novelas/{action=Details}/{id?}",
    defaults: new { controller = "Novelas" });

// Ruta para capítulos (permite /Capitulos/Details/5 o /Capituloes/Details/5)
app.MapControllerRoute(
    name: "capitulos",
    pattern: "Capitulos/{action=Details}/{id?}",
    defaults: new { controller = "Capituloes" });

//  Ruta principal
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Use(async (context, next) =>
{
    await next.Invoke();
    if (context.Response.StatusCode == 404)
    {
        context.Request.Path = "/Home/Error";
        await next.Invoke();
    }
});

app.Run();
