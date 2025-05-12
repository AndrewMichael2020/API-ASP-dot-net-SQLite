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
}