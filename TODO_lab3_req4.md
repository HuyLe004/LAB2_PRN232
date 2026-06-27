# TODO - Hoàn thiện yêu cầu 4 Lab3 (Microservices) — JWT + Refresh + Authorization

## Mục tiêu
Hoàn thiện đầy đủ requirement 4:
- Login API
- JWT Validation
- Refresh Token
- Protected APIs
- Role-based Authorization

## Các bước cần làm (microservices)

### B1) IdentityService (Auth)
- [ ] Thêm entity/model lưu refresh token trong DB (RefreshToken) + migrate.
- [ ] Sửa `POST /api/auth/login` để trả: accessToken + refreshToken.
- [ ] Thêm `POST /api/auth/refresh-token`:
  - validate refresh token (tồn tại, chưa hết hạn, chưa revoke)
  - rotate refresh token (tạo refresh token mới + revoke cái cũ) nếu yêu cầu lab.
  - tạo access token mới có đủ claims (bao gồm role).
- [ ] Bảo đảm JWT claims role luôn set `ClaimTypes.Role`.

### B2) API Gateway
- [ ] Đảm bảo gateway forward header `Authorization` nguyên vẹn sang các service.
- [ ] Nếu cần, thêm cấu hình authorization/policies hoặc Swagger JWT support.

### B3) CourseService & StudentService (Protected APIs)
- [ ] Gắn `[Authorize]` lên controller hoặc action tương ứng.
- [ ] Gắn `[Authorize(Roles = "Admin")]` cho ít nhất 1 endpoint mutating (POST/PUT/DELETE) để đúng ví dụ lab.
- [ ] Kiểm tra JWT role claim mapping hoạt động.

### B4) Swagger/OpenAPI
- [ ] Bật Swagger JWT support cho các service có protected endpoints.

### B5) Test
- [ ] Test Login -> nhận accessToken + refreshToken.
- [ ] Test Protected API không gửi token -> 401.
- [ ] Test Protected API có token -> 200.
- [ ] Test Refresh Token -> nhận accessToken mới.
- [ ] Test Role-based Authorization: non-Admin -> 403, Admin -> 200.

