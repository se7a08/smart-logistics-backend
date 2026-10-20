

// Add services to the container.
using Microsoft.AspNetCore.Identity;
using SmartLogistics.API.Extensions;
using SmartLogistics.Infrastructure.Data;
using SmartLogistics.Infrastructure.Data.Seeding;
using SmartLogistics.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddApplication();                          // MediatR, AutoMapper, Validators
builder.Services.AddInfrastructure(builder.Configuration);  // DbContext, Repositories, Services
builder.Services.AddSignalR();                              // «·‹ TrackingService ›Ì «·‹ Infrastructure €«·»« »Ì⁄ „œ ⁄·ÌÂ
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(o =>
    {
        o.Password.RequiredLength = 8;
        o.Password.RequireNonAlphanumeric = false;
        o.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/AccessDenied";
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await ShipmentSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
