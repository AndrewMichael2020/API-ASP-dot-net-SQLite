using Xunit;
using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Controllers;
using UserManagementAPI.Data;
using UserManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BlogAPI.Tests;

public class BlogsControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("BlogsTestDb")
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task CreateBlog_ReturnsCreatedBlog()
    {
        var db = GetDbContext();
        var controller = new BlogsController(db);
        var blog = new Blog { Title = "Blog1", Content = "Content1" };

        var result = await controller.CreateBlog(blog) as CreatedAtActionResult;
        Assert.NotNull(result);
        var created = result?.Value as Blog;
        Assert.NotNull(created);
        Assert.Equal("Blog1", created!.Title);
    }

    [Fact]
    public async Task GetBlogs_ReturnsAllBlogs()
    {
        var db = GetDbContext();
        db.Blogs.Add(new Blog { Title = "A", Content = "A" });
        db.SaveChanges();
        var controller = new BlogsController(db);

        var result = await controller.GetBlogs() as OkObjectResult;
        Assert.NotNull(result);
        var blogs = result?.Value as System.Collections.Generic.List<Blog>;
        Assert.NotNull(blogs);
        Assert.Single(blogs!);
    }

    [Fact]
    public async Task GetBlog_ReturnsBlog_WhenExists()
    {
        var db = GetDbContext();
        var blog = new Blog { Title = "B", Content = "B" };
        db.Blogs.Add(blog);
        db.SaveChanges();
        var controller = new BlogsController(db);

        var result = await controller.GetBlog(blog.Id) as OkObjectResult;
        Assert.NotNull(result);
        var found = result?.Value as Blog;
        Assert.NotNull(found);
        Assert.Equal("B", found!.Title);
    }

    [Fact]
    public async Task GetBlog_ReturnsNotFound_WhenMissing()
    {
        var db = GetDbContext();
        var controller = new BlogsController(db);

        var result = await controller.GetBlog(999);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateBlog_ReturnsNoContent_WhenSuccess()
    {
        var db = GetDbContext();
        var blog = new Blog { Title = "C", Content = "C" };
        db.Blogs.Add(blog);
        db.SaveChanges();
        var controller = new BlogsController(db);

        blog.Title = "C Updated";
        var result = await controller.UpdateBlog(blog.Id, blog);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateBlog_ReturnsBadRequest_WhenIdMismatch()
    {
        var db = GetDbContext();
        var blog = new Blog { Title = "D", Content = "D" };
        db.Blogs.Add(blog);
        db.SaveChanges();
        var controller = new BlogsController(db);

        var result = await controller.UpdateBlog(blog.Id + 1, blog);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateBlog_ReturnsNotFound_WhenBlogDoesNotExist()
    {
        var db = GetDbContext();
        var controller = new BlogsController(db);
        var blog = new Blog { Id = 999, Title = "Missing", Content = "Missing" };

        var result = await controller.UpdateBlog(999, blog);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateBlog_Handles_DbUpdateConcurrencyException()
    {
        var db = GetDbContext();
        var blog = new Blog { Title = "Test", Content = "Test" };
        db.Blogs.Add(blog);
        db.SaveChanges();

        var controller = new BlogsController(db);

        // Simulate concurrency exception by removing the blog before update
        db.Blogs.Remove(blog);
        db.SaveChanges();

        var result = await controller.UpdateBlog(blog.Id, blog);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteBlog_RemovesBlog()
    {
        var db = GetDbContext();
        var blog = new Blog { Title = "E", Content = "E" };
        db.Blogs.Add(blog);
        db.SaveChanges();
        var controller = new BlogsController(db);

        var result = await controller.DeleteBlog(blog.Id);
        Assert.IsType<NoContentResult>(result);
        Assert.Empty(db.Blogs);
    }

    [Fact]
    public async Task DeleteBlog_ReturnsNotFound_WhenMissing()
    {
        var db = GetDbContext();
        var controller = new BlogsController(db);

        var result = await controller.DeleteBlog(12345);
        Assert.IsType<NotFoundObjectResult>(result);
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
}