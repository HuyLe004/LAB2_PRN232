using IdentityService.Controllers;
using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_AcceptsPasswordField_AndReturnsToken()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new IdentityDbContext(options);
        context.Users.Add(new User { Username = "admin", PasswordHash = "admin123", Role = "Admin" });
        await context.SaveChangesAsync();

        var controller = new AuthController(context);
        var result = await controller.Login(new LoginRequest { Username = "admin", Password = "admin123" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var value = ok.Value;
        Assert.NotNull(value);
    }
}
