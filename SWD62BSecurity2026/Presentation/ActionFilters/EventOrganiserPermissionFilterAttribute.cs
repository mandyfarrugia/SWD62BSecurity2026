using Microsoft.AspNetCore.Mvc;

namespace Presentation.ActionFilters
{
    public class HasEventOrganiserPermissionAttribute : TypeFilterAttribute
    {
        public HasEventOrganiserPermissionAttribute() : base(typeof(EventOrganiserPermissionFilter))
        {
        }
    }
}
