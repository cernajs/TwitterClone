using System.Net;
using TwitterClone.Hubs;
using TwitterClone.Data;
using TwitterClone.Models;
using TwitterClone.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.CookiePolicy;
// using tt.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<TwitterContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// strategy for retrieving tweets
builder.Services.AddScoped<ITweetRetrievalStrategy, GetAllTweets>();
builder.Services.AddScoped<IPopularTweetStrategy, PopularTweetStrategy>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITweetService, TweetService>();
builder.Services.AddScoped<IHashtagService, HashtagService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddEntityFrameworkStores<TwitterContext>();
builder.Services.AddSignalR();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy =  SameSiteMode.Lax;
});

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

app.UseCookiePolicy( new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    HttpOnly = HttpOnlyPolicy.None,
    Secure = CookieSecurePolicy.None,
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<NotificationHub>("/notificationHub");
});

app.Run();