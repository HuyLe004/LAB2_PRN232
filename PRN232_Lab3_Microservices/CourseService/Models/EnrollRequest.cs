namespace CourseService.Models
{
    public class EnrollRequest
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public string? Status { get; set; }
    }
}
