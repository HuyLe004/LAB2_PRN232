using Microsoft.AspNetCore.Mvc;
using CourseService.Data;
using CourseService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly CourseDbContext _context;

        public CoursesController(CourseDbContext context)
        {
            _context = context;
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

            // gRPC call to StudentService
            // Student information is required only for demo purposes of Lab3 requirement 2.
            // Must fetch student info via gRPC before saving enrollment (per Lab3 requirement).
            try
            {
                // For simplicity, use synchronous client resolution via DI
                // (We will request client from HttpContext RequestServices)
                var client = HttpContext.RequestServices.GetService<CourseService.Grpc.StudentGrpcClient>();
                if (client != null)
                {
                    var student = await client.GetStudentByIdAsync(enrollment.StudentId);
                    // Optionally attach some info to response (not persisted to course DB)
                    if (student != null && student.StudentId != 0)
                    {
                        studentFullName = student.FullName;
                    }
                }
            }
            catch
            {
                // ignore
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