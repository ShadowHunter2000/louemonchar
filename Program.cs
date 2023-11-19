using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Projet_Final.Areas.Identity.Data;
using Projet_Final.Hubs;
using Projet_Final.Models;
using Projet_Final.Services;
using Projet_Final.Settings;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUtilisateur>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("TwilioSettings"));
builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration.GetSection("GoogleAuthSettings")
    .GetValue<string>("ClientId");
    googleOptions.ClientSecret = builder.Configuration.GetSection("GoogleAuthSettings")
    .GetValue<string>("ClientSecret");
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireUppercase = false;
});

builder.Services.AddSingleton<TestData>();
builder.Services.AddScoped<ISMSSenderService, SMSSenderService>();

var app = builder.Build();

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
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    ApplicationDbContext.SeedData(dbContext);
}

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chatHub");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
