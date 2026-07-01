using Microsoft.AspNetCore.Mvc;
using CourseService.Data;
using CourseService.Entities;
using CourseService.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.Authorize] // protected APIs (require JWT)
    public class CoursesController : ControllerBase
    {
        private readonly CourseDbContext _context;
        private readonly CourseService.Grpc.StudentGrpcClient _studentGrpcClient;

        public CoursesController(CourseDbContext context, CourseService.Grpc.StudentGrpcClient studentGrpcClient)
        {
            _context = context;
            _studentGrpcClient = studentGrpcClient;
        }

        // 1. API Xem danh sách môn học kèm học kỳ
        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Semester)
                .Select(c => new CourseResponse
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    SemesterId = c.SemesterId,
                    SemesterName = c.Semester.SemesterName
                })
                .ToListAsync();

            return Ok(courses);
        }


        // 2. API Tạo môn học mới
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCourse([FromBody] Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return Ok(course);
        }

        // 3. API Đăng ký học (Enrollment) - Chuẩn Microservices chỉ lưu StudentId
        // Requirement 2 (gRPC): CourseService retrieve student info from StudentService via gRPC before saving.
        [HttpPost("enroll")]
        public async Task<IActionResult> EnrollStudent([FromBody] EnrollRequest request)
        {
            var studentFullName = "";

            if (request == null)
                return BadRequest("Enrollment is required.");

            if (request.StudentId <= 0)
                return BadRequest("Invalid StudentId.");

            if (request.CourseId <= 0)
                return BadRequest("Invalid CourseId.");

            var enrollment = new Enrollment
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status
            };

            // gRPC call to StudentService
            // Student information is required only for demo purposes of Lab3 requirement 2.
            // Must fetch student info via gRPC before saving enrollment (per Lab3 requirement).
            try
            {
                // Student validation must be done via gRPC (service-to-service flow)
                // Use DI-injected client to keep the implementation stable and testable.
                var student = await _studentGrpcClient.GetStudentByIdAsync(enrollment.StudentId);
                if (student == null || student.StudentId == 0)
                {
                    return NotFound($"Student with id {enrollment.StudentId} not found.");
                }

                studentFullName = student.FullName ?? "";
            }
            catch
            {
                // gRPC failure -> treat as unable to validate student
                return StatusCode(502, "Unable to validate student via gRPC.");
            }

            enrollment.EnrollDate = DateTime.Now;

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // Return student info if available
            return Ok(new { message = "Đăng ký môn học thành công!", enrollment, studentFullName });
        }
    }
}