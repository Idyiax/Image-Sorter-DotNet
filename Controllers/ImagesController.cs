using Microsoft.AspNetCore.Mvc;
using Image_Sorter_DotNet.Models;
using Image_Sorter_DotNet.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
using Image_Sorter_DotNet.Services.Interfaces;

namespace Image_Sorter_DotNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IImageService _imageService;
    private readonly ITagService _tagService;

    public ImagesController(ApplicationDbContext context, IImageService imageService, ITagService tagService)
    {
        _context = context;
        _imageService = imageService;
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Images>>> GetImages([FromQuery] string? sortingMode, [FromQuery] string? tagFilters, [FromQuery] string? filterMode)
    {
        List<Images> images = new();

        int[]? filters = filters = tagFilters?.Split(',').Select(int.Parse).ToArray();

        if (filters == null || filters.Length == 0)
        {
            images = await _context.Images.ToListAsync();
        }

        else
        {
            if (filterMode == null || filterMode == "all")
            {
                List<List<int>> filterLists = new();

                for(int i = 0; i < filters.Count(); i++)
                {
                    var children = await _tagService.GetAllChildren(filters[i]);
                    if (children == null) continue;
                    filterLists.Add(_tagService.TagsToId(children));
                    filterLists[i].Add(filters[i]);
                }

                // Entity Framework can't handle complex nested queries like this so this has to be
                // converted to a list first and done natively. It's not particularly efficient but
                // it works so it'll have to do for now.
                images = _context.Images
                    .Include(i => i.TagConnections)
                    .Where(i => i.TagConnections != null && i.TagConnections.Any())
                    .ToList()  // Materialize the base query first
                    .Where(image => 
                        filterLists.All(filterList => image.TagConnections != null && 
                            image.TagConnections.Any(tc => 
                                filterList.Contains(tc.TagId))))
                    .ToList();
            }

            else
            {
                var childFilters = new List<Tags>();

                foreach (var filter in filters)
                {
                    var children = await _tagService.GetAllChildren(filter);
                    if (children != null) childFilters = childFilters.Concat(children).ToList();
                }

                filters = filters.Concat(_tagService.TagsToId(childFilters)).ToArray();

                images = _context.Images.Where(i => 
                    i.TagConnections != null &&
                    i.TagConnections.Count > 0 &&
                    i.TagConnections.Any(tc => filters.Contains(tc.TagId)))
                    .ToList();
            }
        }

        if (sortingMode != null && images.Count != 0)
        {
            sortingMode = sortingMode.ToLower();

            switch (sortingMode)
            {
                case "alphabetical":
                    images = images.OrderBy(i => i.FileName).ToList();
                    break;
                case "date-created":
                    images = images.OrderBy(i => i.CreatedDate).ToList();
                    break;
                default:
                    break;
            }
        }

        if (images.Count == 0)
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
        Images? image = await _imageService.GetImage(id);

        if (image == null) return NotFound();

        return Ok(image);
    }

    [HttpPost]
    public async Task<IActionResult> AddImage([FromForm] IFormFile file, [FromForm] string? name)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", uniqueFileName);

        try
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
        }

        catch
        {
            return BadRequest($"Could not create image at {filePath}.");
        }

        var image = new Images
        {
            FileName = name == null ? "" : name,
            FilePathName = $"/img/{uniqueFileName}",
        };

        try
        {
            _context.Images.Add(image);
            await _context.SaveChangesAsync();
        }

        catch(Exception e)
        {
            // Delete the created image if adding the database entry fails
            System.IO.File.Delete(filePath);
            return BadRequest($"Could not add image {name} because of the following error: {e}");
        }

        return CreatedAtAction(nameof(GetImage), new { id = image.Id }, image);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchImage(int id, [FromBody] JsonPatchDocument<Images> patchDoc)
    {
        if (patchDoc == null) return BadRequest("No patch document sent.");

        Images? image = await _imageService.GetImage(id);

        if (image == null) return NotFound($"Image with ID {id} not found.");

        patchDoc.ApplyTo(image);

        await _context.SaveChangesAsync();
        return Ok(image);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImage(int id)
    {
        if (await _imageService.GetImage(id) == null) return NotFound($"Image with ID {id} not found.");

        bool result = await _imageService.DeleteImage(id);

        if (result) return Ok();
        else return Problem("Image could not be deleted.");
    }


    [HttpGet("{id}/tags")]
    public async Task<ActionResult<IEnumerable<Tags>>> GetTags(int id)
    {
        if (await _imageService.GetImage(id) == null) return NotFound($"Image with ID {id} not found");

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
        if (await _imageService.GetImage(id) == null) return NotFound($"Image with ID {id} not found");

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
        if (await _imageService.GetImage(id) == null) return NotFound($"Image with ID {id} not found");
        if (await _tagService.GetTag(tagId) == null) return NotFound($"Tag with ID {tagId} not found");

        TagConnections? connection = await _context.TagConnections.FirstOrDefaultAsync((c) => c.TagId == tagId);

        if (connection == null) return NotFound();

        _context.TagConnections.Remove(connection);
        await _context.SaveChangesAsync();

        return Ok();
    }
}