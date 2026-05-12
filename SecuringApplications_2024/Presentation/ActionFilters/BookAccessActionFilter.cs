using Common.Models;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Presentation.Utilities;
using System.Text;
using System.Web;

namespace Presentation.ActionFilters
{
    public class BookAccessActionFilterAttribute : ActionFilterAttribute
    {
        private readonly bool _read;
        private readonly bool _write;

        public BookAccessActionFilterAttribute(bool read, bool write = false)
        {
            this._read = read;
            this._write = write;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string bookId = context.HttpContext.Request.Query["bookId"];

            if (string.IsNullOrEmpty(bookId) || !context.HttpContext.User.Identity.IsAuthenticated) 
            {
                context.Result = new ForbidResult();
            }

            string safeInput = HttpUtility.UrlDecode(bookId);
            byte[] cipherAsBytes = Convert.FromBase64String(safeInput);

            var userManager = context.HttpContext.RequestServices.GetService<UserManager<CustomUser>>();
            var user = userManager?.GetUserAsync(context.HttpContext.User).GetAwaiter().GetResult();

            if (user == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            string passwordHash = user.Id.ToString(); // You may want to hash this in production
            var encryptionTool = new Encryption();

            byte[] originalDataAsBytes = encryptionTool.SymmetricDecrypt(cipherAsBytes, passwordHash);
            int originalBookId = Convert.ToInt32(UTF32Encoding.UTF32.GetString(originalDataAsBytes));

            // Store decrypted ID for use in controller
            context.HttpContext.Items["DecryptedBookId"] = originalBookId;

            PermissionsRepository permissionsRepository = context.HttpContext.RequestServices.GetService<PermissionsRepository>();
            
            if (permissionsRepository != null)
            {
                IQueryable<Permission> permissions = permissionsRepository.GetPermissions(originalBookId);
                
                if(permissions.Count(permission => permission.User.Email.Equals(context.HttpContext.User.Identity.Name) && permission.Read == _read && permission.Write == _write) > 0)
                {
                }
                else
                {
                    context.Result = new ForbidResult();
                }
            }
            else 
            {
                context.Result = new NotFoundResult();
            }

            base.OnActionExecuting(context);
        }
    }
}