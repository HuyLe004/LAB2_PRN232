# TODO - Hoàn thiện yêu cầu 6 (Docker Deployment) — Lab3 Microservices

## Mục tiêu
- Có `Dockerfile` cho từng service: `api-gateway`, `identity-service`, `student-service`, `course-service`.
- Có `docker-compose.yml` triển khai toàn bộ system.
- Tối thiểu các container theo requirement: 
  - api-gateway
  - identity-service
  - student-service
  - course-service
  - identity-db
  - student-db
  - course-db

## Checklist
- [ ] Tạo `Dockerfile` cho `ApiGateway`.
- [ ] Tạo `Dockerfile` cho `IdentityService`.
- [ ] Tạo `Dockerfile` cho `StudentService`.
- [ ] Tạo `Dockerfile` cho `CourseService`.
- [ ] Tạo `docker-compose.yml` ở root `PRN232_Lab3_Microservices`.
- [ ] Cấu hình networks + environment variables cho connection strings tới SQL Server.
- [ ] Exposure ports hợp lý (gateway + swagger).
- [ ] Đảm bảo JWT secret/issuer/audience đồng bộ giữa các service (dùng cùng value đang hardcode trong code).
- [x] Cấu hình thứ tự khởi chạy phụ thuộc `depends_on`.
- [x] Thêm healthcheck cho SQL Server.
- [ ] (Optional nếu pass yêu cầu) Thêm seed/migration chạy tự động: dùng `dotnet ef database update` hoặc gọi `EnsureCreated` (nếu project có logic).


