using ChatWithEncryption.Data;
using ChatWithEncryption.Hubs;
using ChatWithEncryption.Models;
using ChatWithEncryption.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatWithEncryption
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
  
           

            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;          
                options.Password.RequireLowercase = false;        
                options.Password.RequireUppercase = false;       
                options.Password.RequireNonAlphanumeric = false;  
                options.Password.RequiredLength = 1;               
                options.User.RequireUniqueEmail = false;
            })  .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<AuthServices>(); 

            builder.Services.AddSignalR();


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login"; 
                    options.LogoutPath = "/Auth/Logout"; 
                    options.SlidingExpiration = true;  
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);  
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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

            app.MapHub<ChatHub>("/chatHub");

            app.Run();
        }
    }
}
