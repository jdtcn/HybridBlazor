using Microsoft.AspNetCore.Identity;

namespace HybridBlazor.Server.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? CounterId { get; set; }

        public virtual Counter Counter { get; set; }
    }
}
