using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevPortalen.Models
{
    public class JobOfferModel
    {
        [Key]
        public int JobOfferId { get; set; }
        public int CompanyId { get; set; }
        public int senderId { get; set; }
        public int receiverId { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? CompanyName { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string? ContactPerson { get; set; }
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Indtast kun et dansk telefonnummer af 8 cifre")]
        public int? PhoneNumber { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? Title { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string? JobAddress { get; set; }
        [Url]
        public string? JobLink { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public string? JobDetails { get; set; }
        public int NumberOfPositionsAvailable { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string? Speciality { get; set; }
        [Required]
        public DateTime DateOfPublish { get; set; }
        [Required]
        public DateTime Deadline { get; set; }
    }
}
