using Microsoft.AspNetCore.Identity;

namespace Capitec.DAL.Data.Entities
{

    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public bool IsDeleted { get; set; }
    }
}
