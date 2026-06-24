using Lms.Shared.Protos;
using Grpc.Net.Client;

namespace CourseService.Grpc;

public class StudentGrpcClient
{
    private readonly StudentService.StudentServiceClient _client;

    public StudentGrpcClient(IConfiguration configuration)
    {
        // e.g. StudentService:GrpcBaseUrl = "https://localhost:5002" or "http://localhost:5002"
        var baseUrl = configuration["StudentService:GrpcBaseUrl"] ?? "http://localhost:5002";
        var channel = GrpcChannel.ForAddress(baseUrl);
        _client = new StudentService.StudentServiceClient(channel);
    }

    public async Task<GetStudentByIdResponse> GetStudentByIdAsync(int studentId, CancellationToken ct = default)
    {
        var request = new GetStudentByIdRequest { StudentId = studentId };
        return await _client.GetStudentByIdAsync(request, cancellationToken: ct);
    }
}

