using TwitterClone.Hubs;
using TwitterClone.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
// using tt.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ttIdentityDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ttIdentityDbContextConnection' not found.");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<TwitterContext>(options => options.UseInMemoryDatabase("TwitterClone"));
// strategy for retrieving tweets
builder.Services.AddScoped<ITweetRetrievalStrategy, GetAllTweets>();
builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddEntityFrameworkStores<TwitterContext>();
builder.Services.AddSignalR();

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

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<NotificationHub>("/notificationHub");
});

app.Run();
