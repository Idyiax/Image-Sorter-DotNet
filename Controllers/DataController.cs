using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Image_Sorter_DotNet.Models;
using Image_Sorter_DotNet.Data;
using Microsoft.EntityFrameworkCore;

namespace Image_Sorter_DotNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DataController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<Images>> PostImage([FromForm] IFormFile file, [FromForm] string? name)
    {
        // Create unique filename
        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";

        // Create full path
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", uniqueFileName);

        try
        {
            // Create directory if it doesn't exist
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Save the file
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
        }

        catch
        {
            return BadRequest();
        }

        // Create database entry
        var image = new Images
        {
            FileName = name == null ? "" : name,
            FilePathName = $"/img/{uniqueFileName}",
            CreatedDate = DateTime.UtcNow
        };

        try
        {
            // Add image entry to the database
            _context.Images.Add(image);
            await _context.SaveChangesAsync();
        }

        catch
        {
            // Delete the created image if adding the database entry fails
            System.IO.File.Delete(filePath);
            return BadRequest();
        }

        return CreatedAtAction(nameof(GetImage), new { id = image.Id }, image);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Images>> GetImage(int id)
    {
        var image = await _context.Images.FindAsync(id);

        if (image == null)
        {
            return NotFound();
        }

        return Ok(image);
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<Images>>> GetAllImages()
    {
        return await _context.Images.ToListAsync();
    }

    [HttpPost("{id}/{name}")]
    public async Task<IActionResult> UpdateImageName(int id, string name)
    {
        var image = await _context.Images.FindAsync(id);

        if (image == null)
        {
            return NotFound();
        }

        image.FileName = name;

        await _context.SaveChangesAsync();

        return Ok(image.FileName);
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> DeleteImage(int id)
    {
        var image = await _context.Images.FindAsync(id);

        if (image == null)
        {
            return NotFound();
        }

        var imagePath = image.FilePathName;

        _context.Images.Remove(image);

        await _context.SaveChangesAsync();

        return Ok();
    }
}
