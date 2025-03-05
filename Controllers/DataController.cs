using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Image_Sorter_DotNet.Models;
using Image_Sorter_DotNet.Data;

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
    public async Task<ActionResult<Images>> PostImage(IFormFile file, string name = "")
    {
        // Create unique filename
        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";

        // Create full path
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", uniqueFileName);

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

        // Create database entry
        var image = new Images
        {
            FileName = name == "" ? uniqueFileName : name,
            FilePathName = $"/img/{uniqueFileName}",
            CreatedDate = DateTime.UtcNow
        };

        _context.Images.Add(image);
        await _context.SaveChangesAsync();

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

        // Return the image URL
        return Ok(new { url = image.FilePathName });
    }
}
