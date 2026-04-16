using DataAccess.Context;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Presentation.Helpers;
using Serilog;

namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

            //Add services to the container.
            string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<TicketDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true; //This may block external authentication (OAuth), use with caution.
                options.Lockout.MaxFailedAccessAttempts = 3; //The idea is to block the account after the third failed consecutive login attempt.
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 5; //Must use at least 5 unique characters.
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<TicketDbContext>()
                /* If you are relying on the built-in/scaffolded Identity views (for login and registrations, as an example), 
                 * you need to add the default token providers, as well as the default user interface otherwise login will fail. */
                .AddDefaultTokenProviders()
                .AddDefaultUI(); 

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages(); //Necessary for scaffolded Identity views to work, as well as for any Razor Pages (Identity and external authentication, for example) in the application.

            builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
            }); //Do not chain AddCookie() to AddAuthentication() when using external authentication as it may interfere with the default cookie authentication scheme used by Identity.

            /* Never log to a database or the console, instead log to a file or the cloud. 
             * Reasons not to log into a database:
             * - 1. Performance (logging to a database can be slower than logging to a file)
             * - 2. Complexity (if the database fails/is unavailable/runs out of space, logs will start failing with every line especially if we are using logs as part of error handling)
             * - 3. Costs (database space costs more than storage for logs space) */
            var logConfiguration = new LoggerConfiguration()
                .MinimumLevel.Warning() //Ignore any levels below Warning which is a Level 3 category (Warning + Error + Critical). Good way to downsize the amount of log entries in a log file.
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            //builder.Host.UseSerilog(logConfiguration);

            builder.Host.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog(logConfiguration, dispose: true);
            });

            builder.Services.AddScoped<EventsRepository>(
                serviceProvider => new EventsRepository(
                    serviceProvider.GetRequiredService<TicketDbContext>(), 
                    serviceProvider.GetRequiredService<IConfiguration>()
            ));

            WebApplication? app = builder.Build();

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

            /* This will catch any unhandled exceptions and the user is redirected to the error page with the error details. 
             * ReExecute - server transfer (user is redirected on the server, the user will not notice any changes on the URL)
             * Redirect - client transfer (use this one in case you want to log on which page the exception happened) */
            app.UseStatusCodePagesWithReExecute("/Home/StatusError", "?=code{0}"); //Attach query string parameter to action which returns the status code generated.

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