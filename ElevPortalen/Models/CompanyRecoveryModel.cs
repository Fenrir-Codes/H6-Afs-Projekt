using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevPortalen.Models
{
    public class CompanyRecoveryModel
    {
        public Guid UserId { get; set; }

        [Key]
        public int HistoryId { get; set; }
        public int CompanyId { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? CompanyName { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? CompanyAddress { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string? Region { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [Url]
        public string? Link { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? Preferences { get; set; }
        [Column(TypeName = "nvarchar(5000)")]
        public string? Description { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string? profileImage { get; set; }
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Indtast kun et dansk telefonnummer af 8 cifre")]
        public int PhoneNumber { get; set; }
        public bool IsHiring { get; set; }
        public bool IsVisible { get; set; }
        [Required]
        public DateTime RegisteredDate { get; set; }
        [Required]
        public DateTime RecoveryCreatedDate{ get; set; }
    }
}
