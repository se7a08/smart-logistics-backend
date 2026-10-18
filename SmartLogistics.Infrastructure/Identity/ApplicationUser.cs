using Microsoft.AspNetCore.Identity;
namespace SmartLogistics.Infrastructure.Identity
{
    public class ApplicationUser:IdentityUser
    {
        public string FullName {  get; set; }
        public Guid? DomainUserId { get; set; }
    }
}
