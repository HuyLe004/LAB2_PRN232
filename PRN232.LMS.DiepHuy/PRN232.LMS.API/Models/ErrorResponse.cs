namespace PRN232.LMS.API.Models
{
    public class ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = null!;
        public List<string>? Errors { get; set; }
        public int? StatusCode { get; set; }
    }

    public class ErrorDetails
    {
        public string? TraceId { get; set; }
        public string? Path { get; set; }
        public string? Method { get; set; }
    }
}
