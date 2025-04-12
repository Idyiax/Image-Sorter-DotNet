using Microsoft.AspNetCore.Mvc;
using Image_Sorter_DotNet.Models;
using Image_Sorter_DotNet.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;

namespace Image_Sorter_DotNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ImagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Images>>> GetImages()
    {
        List<Images> images = await _context.Images.ToListAsync();

        if (images == null || images.Count == 0)
        {
            return NoContent();
        }

        else
        {
            return Ok(images);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetImage(int id)
    {
        var image = await _context.Images.FindAsync(id);

        if (image == null)
        {
            return NotFound();
        }

        return Ok(image);
    }

    [HttpPost]
    public async Task<IActionResult> AddImage([FromForm] IFormFile file, [FromForm] string? name)
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
        };

        try
        {
            // Add image entry to the database
            _context.Images.Add(image);
            await _context.SaveChangesAsync();
        }

        catch(Exception e)
        {
            // Delete the created image if adding the database entry fails
            System.IO.File.Delete(filePath);
            return BadRequest($"Could not add the image {name} because of the following error: {e}");
        }

        return CreatedAtAction(nameof(GetImage), new { id = image.Id }, image);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchImage(int id, [FromBody] JsonPatchDocument<Images> patchDoc)
    {
        if (patchDoc == null)
        {
            return BadRequest("No patch document sent.");
        }

        var image = await _context.Images.FindAsync(id);

        if (image == null)
        {
            return NotFound($"The image with the id '{id}' could not be found.");
        }

        patchDoc.ApplyTo(image);

        await _context.SaveChangesAsync();
        return Ok(image);
    }

    [HttpDelete("{id}")]
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


    [HttpGet("{id}/tags")]
    public async Task<ActionResult<IEnumerable<Tags>>> GetTags(int id)
    {
        List<Tags> tags =  await _context.TagConnections
            .Where(tagConnection => tagConnection.ImageId == id)
            .Join(_context.Tags,
                tagConnection => tagConnection.TagId,
                tag => tag.Id,
                (tagConnection, tag) => tag)
            .ToListAsync();

        if (tags == null || tags.Count == 0)
        {
            return NoContent();
        }

        else
        {
            return Ok(tags);
        }
    }

    [HttpPost("{id}/tags/{tagId}")]
    public async Task<IActionResult> AddTag(int id, int tagId)
    {
        var connection = new TagConnections
        {
            ImageId = id,
            TagId = tagId,
        };
        
        _context.TagConnections.Add(connection);
        await _context.SaveChangesAsync();

        return Ok(connection);
    }

    [HttpDelete("{id}/tags/{tagId}")]
    public async Task<IActionResult> RemoveTag(int id, int tagId)
    {
        TagConnections? connection = await _context.TagConnections.FirstOrDefaultAsync((c) => c.TagId == tagId);

        if (connection == null)
        {
            return NotFound();
        }

        _context.TagConnections.Remove(connection);
        await _context.SaveChangesAsync();

        return Ok();
    }
}