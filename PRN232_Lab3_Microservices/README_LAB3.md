# LAB 3 - Microservices + gRPC + JWT + Docker

## What is implemented
- Identity Service: login, refresh-token, JWT validation, role-based auth, Swagger
- Student Service: student CRUD, gRPC server, Swagger, JWT auth
- Course Service: course CRUD, course enrollment with gRPC student validation, Swagger
- API Gateway: YARP reverse proxy, JWT validation before forwarding
- Docker Compose: all services + SQL Server databases

## Run locally
1. Build and run with Docker Compose:
   - docker compose up --build
2. Access services:
   - Gateway: http://localhost:5000
   - Identity: http://localhost:5001
   - Student: http://localhost:5002
   - Course: http://localhost:5003

## Test flow
- Login: POST /api/auth/login
- Protected API: GET /api/students
- Unauthorized request: call protected endpoint without token
- gRPC flow: POST /api/courses/enroll with a valid student ID

## Notes
- Default seed users: admin/admin123, student/student123
- Default seed student: SV001 / SV002
