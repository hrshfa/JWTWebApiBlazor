using AuthorizeTest.Shared.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthorizeTest.dntipsAPI.Entities
{
    public class UserRole
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }
        public RolesEnum Role { get; set; }

        public virtual User User { get; set; }
    }
}
