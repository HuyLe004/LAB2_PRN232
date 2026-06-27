using Microsoft.AspNetCore.Mvc;
using StudentService.Data;
using StudentService.Entities;
using Microsoft.EntityFrameworkCore;

namespace StudentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.Authorize] // protected APIs (require JWT)
    public class StudentsController : ControllerBase
    {
        private readonly StudentDbContext _context;

        public StudentsController(StudentDbContext context)
        {
            _context = context;
        }

        // 1. API Lấy danh sách toàn bộ sinh viên
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Students.ToListAsync());
        }

        // 2. API Lấy chi tiết 1 sinh viên theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound("Không tìm thấy sinh viên.");
            return Ok(student);
        }

        // 3. API Thêm mới sinh viên
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = student.StudentId }, student);
        }
    }
}