using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevPortalen.Models
{
    //Lavet af Jozsef
    public class StudentModel
    {
        public Guid UserId { get; set; }

        [Key]
        public int StudentId { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? Title { get; set; } // jeg tror vi skal ikke have title bruger vi specialisering i stedet for
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
        [Column(TypeName = "nvarchar(5000)")]
        public string? Description { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string? profileImage { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? Speciality { get; set; }
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Indtast kun et dansk telefonnummer af 8 cifre")]
        public int PhoneNumber { get; set; }
        [Url]
        public string? FacebookLink { get; set; }
        [Url]
        public string? LinkedInLink { get; set; }
        [Url]
        public string? InstagramLink { get; set; }
        [Url]
        public string? GitHubLink { get; set; }
        [Required]
        public DateTime RegisteredDate { get; set; }
        [Required]
        public DateTime UpdatedDate { get; set; }


        public virtual SkillModel? Skills { get; set; }

    }
}
