using DataAccess.Context;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Presentation.Helpers;
using static System.Formats.Asn1.AsnWriter;

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<TicketDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Lockout.MaxFailedAccessAttempts = 3; //The idea is to block the account after the third failed consecutive login attempt.
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 5; //Must use at least 5 unique characters.
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<TicketDbContext>();

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            builder.Services.AddAuthentication()
            .AddCookie()
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            });


            builder.Services.AddScoped<EventsRepository>(
                serviceProvider => new EventsRepository(
                    serviceProvider.GetRequiredService<TicketDbContext>(), 
                    serviceProvider.GetRequiredService<IConfigurationManager>()
            ));

            var app = builder.Build();

            /* The incorrect approach: Yields the exception "System.InvalidOperationException: 'The service collection cannot be modified because it is read-only.'"
            builder.Services.AddScoped<RolesManagementHelper>(serviceProvider => new RolesManagementHelper(
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
                serviceProvider.GetRequiredService<UserManager<IdentityUser>>()
            )); */

            /* The correct approach: Call it only once in the beginning.
             * RoleManager and UserManager are scoped service, hence they live per request.
             * This will only execute if:
             * - The application is stopped and re-run again.
             * - The server restarts.
             * - A new version of the application is deployed.
             * - Hot reload triggers a restart.
             * The using block creates a new dependency injection service,
             * ensuring a safe resolution of scoped services and their disposal once done.
             * This will not be triggered by an HTTP request unlike controller logic, 
             * hence there is no scope by default. 
             * Therefore, if this code is not enclosed within a dependency injection scope,
             * it will yield the following exception:
             * System.InvalidOperationException: 'Cannot resolve scoped service 'Microsoft.AspNetCore.Identity.RoleManager`1[Microsoft.AspNetCore.Identity.IdentityRole]' from root provider.'*/
            using (IServiceScope? scope = app.Services.CreateScope())
            {
                RoleManager<IdentityRole>? roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                UserManager<IdentityUser>? userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                RolesManagementHelper rolesManagementHelper = new RolesManagementHelper(roleManager, userManager);
                /* The method DefaultRolesSetup() should be idempotent (safe to run multiple times). 
                 * Therefore, it is in one's best interests to check whether the roles already exist 
                 * and only populate the roles if they are not yet in the database. 
                 * This reduces any duplicate entries or any possible exceptions. */
                rolesManagementHelper.DefaultRolesSetup();
            }

            //Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                //The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

            app.Run();
        }
    }
}
