using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PhotoGallery_AspDotNet8_MVC.Enums;
using PhotoGallery_AspDotNet8_MVC.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"))
    options.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection"))
    );
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true; 
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI();

builder.Services.AddControllersWithViews();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // Identity UI

// Seed Roles and Admin
using (var scope = app.Services.CreateScope())
{
    var svc = scope.ServiceProvider;
    var userManager = svc.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = svc.GetRequiredService<RoleManager<IdentityRole>>();

    string UserRole = RoleEnum.User.ToString();
    string AdminRole = RoleEnum.Admin.ToString();
    string[] roles = new[] { AdminRole , UserRole };
    foreach (var r in roles)
    {
        if (!await roleManager.RoleExistsAsync(r))
            await roleManager.CreateAsync(new IdentityRole(r));
    }

    var user = await userManager.FindByEmailAsync("user@site.com");
    if (user == null)
    {
        user = new ApplicationUser { UserName = "user@site.com", Email = "user@site.com" };
        await userManager.CreateAsync(user, "User@123");
        await userManager.AddToRoleAsync(user, UserRole);
    }

    var admin = await userManager.FindByEmailAsync("admin@site.com");
    if (admin == null)
    {
        admin = new ApplicationUser { UserName = "admin@site.com", Email = "admin@site.com" };
        await userManager.CreateAsync(admin, "Admin@123");
        await userManager.AddToRoleAsync(admin, AdminRole);
    }
}

app.Run();
