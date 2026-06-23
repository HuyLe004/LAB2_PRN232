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
        [HttpPost("enroll")]
        public async Task<IActionResult> EnrollStudent([FromBody] Enrollment enrollment)
        {
            enrollment.EnrollDate = DateTime.Now;
            if (string.IsNullOrEmpty(enrollment.Status))
            {
                enrollment.Status = "Active";
            }

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đăng ký môn học thành công!", enrollment });
        }
    }
}