using Microsoft.AspNetCore.Mvc;
using CourseService.Data;
using CourseService.Entities;
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
            var courses = await _context.Courses.Include(c => c.Semester).ToListAsync();
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
        public async Task<IActionResult> EnrollStudent([FromBody] Enrollment enrollment)
        {
            var studentFullName = "";

            // Basic validation
            if (enrollment == null)
                return BadRequest("Enrollment is required.");

            if (enrollment.StudentId <= 0)
                return BadRequest("Invalid StudentId.");

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
            if (string.IsNullOrEmpty(enrollment.Status))
            {
                enrollment.Status = "Active";
            }

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // Return student info if available
            return Ok(new { message = "Đăng ký môn học thành công!", enrollment, studentFullName });
        }
    }
}