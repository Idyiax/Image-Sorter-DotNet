using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Image_Sorter_DotNet.Models;
using Image_Sorter_DotNet.Data;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

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

    #region Gets
    /// <summary>
    /// Get the row of a specified image from the Images table.
    /// </summary>
    /// <param name="id"> The id of the image. </param>
    /// <returns> The row of the specified image. </returns>
    [HttpGet("getimage/{id}")]
    public async Task<ActionResult<Images>> GetImage(int id)
    {
        var image = await _context.Images.FindAsync(id);

        if (image == null)
        {
            return NotFound();
        }

        return Ok(image);
    }
    
    /// <summary>
    /// Find an image by it's name.
    /// </summary>
    /// <param name="name"> The name of the image to find. </param>
    /// <returns> The row of the specified image. </returns>
    [HttpGet("findimage/{name}")]
    public async Task<ActionResult<Images>> FindImage(string name)
    {
        var image = await _context.Images.FirstOrDefaultAsync(i => i.FileName == name);

        if (image == null)
        {
            return NotFound();
        }

        return Ok(image);
    }
    
    /// <summary>
    /// Get the row of a specified tag from the Tags table.
    /// </summary>
    /// <param name="id"> The id of the tag. </param>
    /// <returns> The row of the specified tag. </returns>
    [HttpGet("gettag/{id}")]
    public async Task<ActionResult<Tags>> GetTag(int id)
    {
        var tag = await _context.Tags.FindAsync(id);

        if (tag == null)
        {
            return NotFound();
        }

        return Ok(tag);
    }

    /// <summary>
    /// Find a tag by it's name.
    /// </summary>
    /// <param name="name"> The name of the tag to find. </param>
    /// <returns> The row of the specified tag. </returns>
    [HttpGet("findtag/{name}")]
    public async Task<ActionResult<Tags>> FindTag(string name)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.TagName == name);

        if (tag == null)
        {
            return NotFound();
        }

        return Ok(tag);
    }

    /// <summary>
    /// Get all of the rows currently within the Images table.
    /// </summary>
    /// <returns> A list of all rows in the Images table. </returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Images>>> GetAllImages()
    {
        return await _context.Images.ToListAsync();
    }

    #endregion Gets
    #region Posts
    /// <summary>
    /// Add an image to the Images table.
    /// </summary>
    /// <param name="file"> The image file from a form. </param>
    /// <param name="name"> The image name from a form. </param>
    /// <returns> The row of the new image. </returns>
    [HttpPost]
    public async Task<ActionResult<Images>> AddImage([FromForm] IFormFile file, [FromForm] string? name)
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

        catch(Exception e)
        {
            // Delete the created image if adding the database entry fails
            System.IO.File.Delete(filePath);
            return BadRequest($"Could not add the image {name} because of the following error: {e}");
        }

        return CreatedAtAction(nameof(GetImage), new { id = image.Id }, image);
    }

    /// <summary>
    /// Updates the name of a given image in the Images table.
    /// </summary>
    /// <param name="id"> The id of the image to rename. </param>
    /// <param name="name"> The new name of the image. </param>
    /// <returns> The new file name. </returns>
    [HttpPost("{id}/{name}")]
    public async Task<IActionResult> RenameImage(int id, string name)
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

    /// <summary>
    /// Deletes an image from the Images table.
    /// </summary>
    /// <param name="id"> The id of the image to delete. </param>
    /// <returns></returns>
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

    [HttpPost("{name}/{colour}")]
    public async Task<IActionResult> AddTag(string name, Color colour)
    {
        var tag = new Tags
        {
            TagName = name,
            ColourR = colour.R,
            ColourG = colour.G,
            ColourB = colour.B,
            CreatedDate = DateTime.UtcNow
        };

        try
        {
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
        }

        catch(Exception e)
        {
            return BadRequest($"Could not add the tag {name} because of the following error: {e}");
        }

        return CreatedAtAction(nameof(FindTag), new { id = tag.Id }, tag);
    }
    #endregion Posts
}