using System.ComponentModel.DataAnnotations;

namespace StudentService.Entities
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        public string? PhoneNumber { get; set; }
        public string? StudentCode { get; set; }
    }
}