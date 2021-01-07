using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MyTennisPartner.Web.Data;
using Microsoft.EntityFrameworkCore;
using MyTennisPartner.Data.Context;
using Microsoft.AspNetCore.Identity;
using MyTennisPartner.Web.Models;
using MyTennisPartner.Models.Users;
using MyTennisPartner.Web.Data.Seeding;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Options;
using MyTennisPartner.Utilities;
using MyTennisPartner.Web;

namespace MyTennisPartner
{
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
    public class Program
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        public static async Task Main(string[] args)
        {
            var host = BuildWebHost(args);

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                try
                {
                    var userContext = services.GetRequiredService<UserDbContext>();

                    // migrate database if necessary
                    logger.LogInformation("migrating user database");
                    await userContext.Database.MigrateAsync();

                    var appContext = services.GetRequiredService<TennisContext>();

                    // migrate database if necessary
                    logger.LogInformation("migrating tennis database");
                    //await appContext.Database.MigrateAsync();

                    //// seed database if necessary
                    var environment = services.GetRequiredService<IHostingEnvironment>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var httpClient = services.GetRequiredService<SeedHttpClient>();
                    var adminUserOption = services.GetRequiredService<IOptions<AdminUser>>();
                    if (!(adminUserOption.Value is AdminUser adminUser) || adminUser.email == null)
                    {
                        throw new Exception("Missing info for Admin User in appSettings.json file");
                    }

                    // WARNING: force seeding will occur, all users dropped, etc. if forceSeeding option is set here
                    // default behavior: check if any users.  If none, then create admin and, for non-Prod, create seed users
                    // forceSeeding behavior: wipe all users, re-create admin and, for non-Prod, all seed users
                    // returns: list of seedUsers that it populated.  If < 2 users in this list, then we didn't create seed users beyond the admin
                    logger.LogInformation("seeding databases");
                    var userDbInitializer = new UserDbInitializer(userManager, roleManager, environment);
                    var seedUsers = await userDbInitializer.SeedUsers(adminUser, forceSeeding: false);

                    // create admin role, and assign our admin user to that role
                    await userDbInitializer.CreateAdminRole();
                    await userDbInitializer.AssignAdminUser(adminUser.email);

                    // WARNING: all matches, leagues, etc. will be dropped and re-seeded if this line is not commented out, and users were force-seeded 
                    // exception for PROD, because we don't want to ever seed in PROD
                    var dbInitializer = new DbInitializer(appContext, userManager, httpClient, seedUsers, environment);
                    dbInitializer.Initialize();

                    // dispose of managed resources
                    userManager.Dispose();
                    roleManager.Dispose();
                    httpClient.Dispose();
                }
                catch (Exception ex)
                {
                    var message = $"{ex.Message} - {ex.InnerException?.Message}";
                    logger.LogError(ex, $"An error occurred while migrating or seeding the database: {message}");

                    // if this happens, we want to know about it - crash here
                    throw;
                }
            }
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();
    }
}
