using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevPortalen.Models
{
    //Lavet af Jozsef
    public class CompanyModel
    {
        public Guid UserId { get; set; }

        [Key]
        public int CompanyId { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? CompanyName { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string? CompanyAddress { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string? Region { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [Url]
        public string? Link { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? Preferences { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public string? Description { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string? profileImage { get; set; }
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Indtast kun et dansk telefonnummer af 8 cifre")]
        public int PhoneNumber { get; set; }
        public bool IsHiring { get; set; }
        public bool IsVisible { get; set; }
        public bool IsVerified { get; set; }
        [Required]
        public DateTime RegisteredDate { get; set; }
        [Required]
        public DateTime UpdatedDate { get; set; }

    }
}
