using Xunit;
using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Controllers;
using UserManagementAPI.Data;
using UserManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BlogAPI.Tests;

public class UsersControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("UsersTestDb")
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task CreateUser_ReturnsCreatedUser()
    {
        var db = GetDbContext();
        var controller = new UsersController(db);
        var user = new User { Name = "Test", Email = "test@example.com" };

        var result = await controller.CreateUser(user) as CreatedAtActionResult;
        Assert.NotNull(result);
        var created = result?.Value as User;
        Assert.NotNull(created);
        Assert.Equal("Test", created!.Name);
    }

    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        var db = GetDbContext();
        db.Users.Add(new User { Name = "A", Email = "a@a.com" });
        db.SaveChanges();
        var controller = new UsersController(db);

        var result = await controller.GetUsers() as OkObjectResult;
        Assert.NotNull(result);
        var users = result?.Value as System.Collections.Generic.List<User>;
        Assert.NotNull(users);
        Assert.Single(users!);
    }

    [Fact]
    public async Task GetUser_ReturnsUser_WhenExists()
    {
        var db = GetDbContext();
        var user = new User { Name = "B", Email = "b@b.com" };
        db.Users.Add(user);
        db.SaveChanges();
        var controller = new UsersController(db);

        var result = await controller.GetUser(user.Id) as OkObjectResult;
        Assert.NotNull(result);
        var found = result?.Value as User;
        Assert.NotNull(found);
        Assert.Equal("B", found!.Name);
    }

    [Fact]
    public async Task GetUser_ReturnsNotFound_WhenMissing()
    {
        var db = GetDbContext();
        var controller = new UsersController(db);

        var result = await controller.GetUser(999);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNoContent_WhenSuccess()
    {
        var db = GetDbContext();
        var user = new User { Name = "C", Email = "c@c.com" };
        db.Users.Add(user);
        db.SaveChanges();
        var controller = new UsersController(db);

        user.Name = "C Updated";
        var result = await controller.UpdateUser(user.Id, user);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateUser_ReturnsBadRequest_WhenIdMismatch()
    {
        var db = GetDbContext();
        var user = new User { Name = "D", Email = "d@d.com" };
        db.Users.Add(user);
        db.SaveChanges();
        var controller = new UsersController(db);

        var result = await controller.UpdateUser(user.Id + 1, user);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var db = GetDbContext();
        var controller = new UsersController(db);
        var user = new User { Id = 999, Name = "Missing", Email = "missing@example.com" };

        var result = await controller.UpdateUser(999, user);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateUser_Handles_DbUpdateConcurrencyException()
    {
        var db = GetDbContext();
        var user = new User { Name = "Test", Email = "test@example.com" };
        db.Users.Add(user);
        db.SaveChanges();

        var controller = new UsersController(db);

        // Simulate concurrency exception by removing the user before update
        db.Users.Remove(user);
        db.SaveChanges();

        var result = await controller.UpdateUser(user.Id, user);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteUser_RemovesUser()
    {
        var db = GetDbContext();
        var user = new User { Name = "E", Email = "e@e.com" };
        db.Users.Add(user);
        db.SaveChanges();
        var controller = new UsersController(db);

        var result = await controller.DeleteUser(user.Id);
        Assert.IsType<NoContentResult>(result);
        Assert.Empty(db.Users);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNotFound_WhenMissing()
    {
        var db = GetDbContext();
        var controller = new UsersController(db);

        var result = await controller.DeleteUser(12345);
        Assert.IsType<NotFoundObjectResult>(result);
    }
}