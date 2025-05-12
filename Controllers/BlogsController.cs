using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Data;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BlogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBlogs()
        {
            var blogs = await _context.Blogs.ToListAsync();
            return Ok(blogs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlog(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return NotFound(new { error = "Blog not found" });
            return Ok(blog);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlog(Blog blog)
        {
            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBlog), new { id = blog.Id }, blog);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlog(int id, Blog blog)
        {
            if (id != blog.Id) return BadRequest(new { error = "ID mismatch" });

            _context.Entry(blog).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Blogs.Any(b => b.Id == id))
                    return NotFound(new { error = "Blog not found" });
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null) return NotFound(new { error = "Blog not found" });

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}