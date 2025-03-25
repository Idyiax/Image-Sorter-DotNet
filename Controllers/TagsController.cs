using Microsoft.AspNetCore.Mvc;
using Image_Sorter_DotNet.Models;
using Image_Sorter_DotNet.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;

namespace Image_Sorter_DotNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TagsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddTag([FromBody] TagsAddRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var tag = new Tags
        {
            TagName = request.name,
            ColourHex = request.colourHex,
            CreatedDate = DateTime.UtcNow
        };

        try
        {
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
        }
        catch (Exception e)
        {
            return BadRequest($"Could not add the tag: {e.Message}");
        }
    }

    [HttpGet]
    public async Task<ActionResult<Tags>> GetAllTags()
    {
        List<Tags> tags = await _context.Tags.ToListAsync();

        if (tags == null || tags.Count == 0)
        {
            return NoContent();
        }

        return Ok(tags);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tags>> GetTag(int id)
    {
        var tag = await _context.Tags.FindAsync(id);

        if (tag == null)
        {
            return NotFound();
        }

        return Ok(tag);
    }

    [HttpGet("find")]
    public async Task<ActionResult<Tags>> FindTag([FromQuery] string name)
    {
        Tags? tag = await _context.Tags.FirstOrDefaultAsync(t => t.TagName == name);

        if (tag == null)
        {
            return NotFound();
        }

        return Ok(tag);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Tags>> DeleteTag(int id)
    {
        var tag = await _context.Tags.FindAsync(id);

        if (tag == null)
        {
            return NotFound();
        }

        try
        {
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
        
        catch (Exception e)
        {
            return BadRequest($"Failed to delete tag with id {id}: {e}");
        }

        return Ok(tag);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<Tags>> PatchTag(int id, [FromBody] JsonPatchDocument<Tags> patchDoc)
    {
        if (patchDoc == null)
        {
            return BadRequest("No patch document sent.");
        }

        var tag = await _context.Tags.FindAsync(id);

        if (tag == null)
        {
            return NotFound($"The tag with the id '{id}' could not be found.");
        }

        patchDoc.ApplyTo(tag);

        await _context.SaveChangesAsync();
        return Ok(tag);
    }

    [HttpGet("manager-data")]
    public async Task<IActionResult> GetTagManagerData()
    {
        List<object> nodes = new();
        List<object> links = new();

        await _context.Tags.ForEachAsync((tag) =>
        {
            nodes.Add(new
            {
                id = tag.Id,
                name = tag.TagName,
                color = tag.ColourHex
            });
        });

        await _context.TagRelations.ForEachAsync((relation) =>
        {
            links.Add(new
            {
                source = relation.ParentTag,
                target = relation.ChildTag
            });
        });

        object data = new
        {
            nodes = nodes.ToArray(),
            links = links.ToArray()
        };

        var json = JsonSerializer.Serialize(data);

        return Ok(json);
    }

    public record TagsAddRequest(string name, string colourHex);
}