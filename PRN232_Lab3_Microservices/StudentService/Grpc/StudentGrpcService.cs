using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using StudentService.Data;
using StudentService.Entities;
using Lms.Shared.Protos;

namespace StudentService.Grpc;

public class StudentGrpcService : Lms.Shared.Protos.StudentService.StudentServiceBase
{
    private readonly StudentDbContext _context;

    public StudentGrpcService(StudentDbContext context)
    {
        _context = context;
    }

    public override async Task<GetStudentByIdResponse> GetStudentById(GetStudentByIdRequest request, ServerCallContext context)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == request.StudentId);
        if (student == null)
        {
            // Returning empty response; alternatively throw RpcException(NotFound)
            return new GetStudentByIdResponse();
        }

        return new GetStudentByIdResponse
        {
            StudentId = student.StudentId,
            FullName = student.FullName ?? "",
            Email = student.Email ?? "",
            PhoneNumber = student.PhoneNumber ?? "",
            StudentCode = student.StudentCode ?? ""
        };
    }
}

