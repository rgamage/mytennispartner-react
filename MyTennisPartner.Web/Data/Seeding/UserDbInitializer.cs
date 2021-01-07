using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using MyTennisPartner.Models.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyTennisPartner.Web.Data.Seeding
{
    public class UserDbInitializer
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IHostingEnvironment environment;

        // Creates a TextInfo based on the "en-US" culture.
        private readonly TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

        public UserDbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IHostingEnvironment environment)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.environment = environment;
        }

        /// <summary>
        /// initialize user database, returns true if succeeded / no errors
        /// </summary>
        public async Task CreateAdminRole()
        {
            // check if admin role exists, if not, create it
            var adminRole = await roleManager.FindByNameAsync("Admin");
            if (adminRole == null)
            {
                var identityResult = await roleManager.CreateAsync(new IdentityRole("Admin"));
                var result = identityResult.Succeeded;
                if (!result)
                {
                    throw new Exception($"CreateAdminRole ERROR: {identityResult.GetAllErrorMessages()}");
                }
            }
        }

        /// <summary>
        /// assign initial admin users
        /// </summary>
        public async Task AssignAdminUser(string username)
        {
            var adminUser = await userManager.FindByNameAsync(username);
            if (adminUser != null)
            {
                var isAdmin = await userManager.IsInRoleAsync(adminUser, "Admin");
                if (!isAdmin)
                {
                    var identityResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                    var success = identityResult.Succeeded;
                    if (!success)
                    {
                        throw new Exception($"AssignAdminUsers ERROR: {identityResult.GetAllErrorMessages()}");
                    }
                }
            }
        }

        public async Task<List<RandomUser>> SeedUsers(RandomUser adminUser, bool forceSeeding = false)
        {
            // set flag = true if it's a new database (no users)
            var newDatabase = !userManager.Users.Any();

            if (forceSeeding)
            {
                // in case of force seeding, we first delete all users
                var users = userManager.Users.ToList();
                foreach (var user in users)
                {
                    await userManager.DeleteAsync(user);
                }
            }

            var randomUsers = new List<RandomUser>();

            // if admin user doesn't exist, then add it to randomUser list
            var existingAdminUser = await userManager.FindByNameAsync(adminUser?.email);
            if (existingAdminUser == null)
            {
                randomUsers.Insert(0, adminUser);
            }

            // if not in PROD, and we have a new database, then create a bunch of dummy users
            if (!environment.IsProduction() && (newDatabase || forceSeeding))
            {
                // Get random user data
                RandomUserResults randomUserResult = JsonConvert.DeserializeObject<RandomUserResults>(File.ReadAllText($"Data/Seeding/random-users.json"));
                randomUsers.AddRange(randomUserResult.Results.ToList());
            }

            // insert any random users in the list into the db
            foreach (var randomUser in randomUsers)
            {
                var user = new ApplicationUser
                {
                    UserName = randomUser.email,
                    Email = randomUser.email,
                    FirstName = myTI.ToTitleCase(randomUser.name.first),
                    LastName = myTI.ToTitleCase(randomUser.name.last),
                    EmailConfirmed = true
                };

                const string password = "P@ssw0rd";
                await userManager.CreateAsync(user, password);
            }
            return randomUsers;
        }
    }

    public static class IdentityExtensions
    {
        public static string GetAllErrorMessages(this IdentityResult result)
        {
            if (result?.Succeeded == true) return string.Empty;
            var messages = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description).ToArray());
            return messages;
        }
    }
}
