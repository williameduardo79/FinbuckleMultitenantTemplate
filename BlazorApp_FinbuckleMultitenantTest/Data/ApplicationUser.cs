using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity;

namespace BlazorApp_FinbuckleMultitenantTest.Data
{
 
   
    public class ApplicationUser : IdentityUser
    {
       
        public virtual ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
    }

}
