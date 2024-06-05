using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevPortalen.Models
{
    public class StudentRecoveryModel
    {
        public Guid UserId { get; set; }

        [Key]
        public int HistoryId { get; set; }
        public int StudentId { get; set; }
        public int senderId { get; set; }
        public int receiverId { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? Title { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? FirstName { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? MiddleName { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? LastName { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string? Address { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public string? Description { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string? profileImage { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? Speciality { get; set; }
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Indtast kun et dansk telefonnummer af 8 cifre")]
        public int PhoneNumber { get; set; }
        [Required]
        public DateTime RegisteredDate { get; set; }
        [Required]
        public DateTime RecoveryCreationDate { get; set; }
    }
}
