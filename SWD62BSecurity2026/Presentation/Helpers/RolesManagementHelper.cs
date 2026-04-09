using Microsoft.AspNetCore.Identity;

namespace Presentation.Helpers
{
    public class RolesManagementHelper
    {
        private RoleManager<IdentityRole> _roleManager;
        private UserManager<IdentityUser> _userManager;

        public RolesManagementHelper(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager)
        {
            this._roleManager = roleManager;
            this._userManager = userManager;
        }

        public void AllocateRoleToUser(string userId, string role)
        {
            //Logic to allocate a role to a user.
            Task<IdentityUser?> task = this._userManager.FindByIdAsync(userId);
            task.Wait();

            if(task.Result != null)
                this._userManager.AddToRoleAsync(task.Result, role).Wait();
        }

        public void DellocateRoleFromUser(string userId, string role)
        {
            //Logic to deallocate a role from a user.
            Task<IdentityUser?> task = this._userManager.FindByIdAsync(userId);
            task.Wait();

            if (task.Result != null)
                this._userManager.RemoveFromRoleAsync(task.Result, role).Wait();
        }

        //It sets the database with a pre-set list of roles. Allocate all users the default role of "user".
        public void DefaultRolesSetup()
        {
            string[] defaultRoles = new string[] { "admin", "user", "moderator", "organiser" };

            foreach(string defaultRole in defaultRoles)
            {
                if(!this._roleManager.RoleExistsAsync(defaultRole).Result)
                {
                    this._roleManager.CreateAsync(new IdentityRole(defaultRole)).Wait();
                }
            }

            List<IdentityUser> users = this._userManager.Users.ToList();

            foreach(IdentityUser user in users)
            {
                if(!this._userManager.IsInRoleAsync(user, "user").Result)
                {
                    this._userManager.AddToRoleAsync(user, "user").Wait();
                }
            }
        }
    }
}
