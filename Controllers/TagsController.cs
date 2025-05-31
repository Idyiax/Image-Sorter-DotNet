using Microsoft.AspNetCore.Mvc;
using Image_Sorter_DotNet.Models;
using Image_Sorter_DotNet.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
using System.Threading.Tasks;
using Azure;

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
    public record TagsAddRequest(string name, string colourHex);

    [HttpPost("{id}/children/{childId}")]
    public async Task<IActionResult> AddChild(int id, int childId)
    {
        if (id == childId) return BadRequest($"Parent and child tag both have ID {id}. Tags cannot be children of themselves.");

        if (await GetTag(id) is not OkObjectResult) return NotFound($"Parent tag with ID {id} not found");
        if (await GetTag(childId) is not OkObjectResult) return NotFound($"Child tag with ID {childId} not found");

        if (await GetChildren(id) is OkObjectResult okResult &&
            okResult.Value is List<Tags?> children &&
            children.Any(c => c?.Id == childId))
        {
            return BadRequest($"The tag with ID {id} already contains the tag with ID {childId} as a child.");
        }

        // Maybe add a check here to avoid cyclcle loops. For example: A -> B -> C -> A.
        // In this case, you wouldn't be allowed to make tag A a child of tag C because
        // tag C already inherits from A.

        TagRelations relation = new TagRelations 
        { 
            ParentTagId = id,
            ChildTagId = childId
        };

        await _context.TagRelations.AddAsync(relation);
        await _context.SaveChangesAsync();

        return Ok(relation);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTags()
    {
        List<Tags> tags = await _context.Tags.ToListAsync();

        if (tags == null || tags.Count == 0)
        {
            return NoContent();
        }

        return Ok(tags);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTag(int id)
    {
        var tag = await _context.Tags.FindAsync(id);

        if (tag == null) return NotFound();

        return Ok(tag);
    }

    [HttpGet("find")]
    public async Task<IActionResult> FindTag([FromQuery] string name)
    {
        Tags? tag = await _context.Tags.FirstOrDefaultAsync(t => t.TagName == name);

        if (tag == null)
        {
            return NotFound();
        }

        return Ok(tag);
    }

    [HttpGet("{id}/children")]
    public async Task<IActionResult> GetChildren(int id)
    {
        if (await GetTag(id) is not OkObjectResult) return NotFound($"The tag with the ID {id} does not exist.");

        List<Tags?> children = _context.TagRelations.Where(tr => tr.ParentTagId == id).Select(tr => tr.ChildTag).ToList();

        if (children.Count == 0 || children.All(c => c == null))
        {
            return NoContent();
        }

        return Ok(children);
    }

    [HttpGet("{id}/parents")]
    public async Task<IActionResult> GetParents(int id)
    {
        if (await GetTag(id) is not OkObjectResult) return NotFound($"The tag with the ID {id} does not exist.");

        List<Tags?> parents = _context.TagRelations.Where(tr => tr.ChildTagId == id).Select(tr => tr.ParentTag).ToList();

        if (parents.Count == 0 || parents.All(p => p == null))
        {
            return NoContent();
        }

        return Ok(parents);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Tags>> DeleteTag(int id)
    {
        var tag = await _context.Tags.FindAsync(id);

        if (tag == null)
        {
            return NotFound();
        }

        var relations = _context.TagRelations.Where(tr => tr.ParentTagId == id || tr.ChildTagId == id);
        var connections = _context.TagConnections.Where(tc => tc.TagId == id);

        try
        {
            _context.Tags.Remove(tag);
            _context.TagRelations.RemoveRange(relations);
            _context.TagConnections.RemoveRange(connections);

            await _context.SaveChangesAsync();
        }
        
        catch (Exception e)
        {
            return BadRequest($"Failed to delete tag with id {id}: {e}");
        }

        return Ok(tag);
    }

    [HttpDelete("{id}/children/{childId}")]
    public async Task<ActionResult<Tags>> DeleteChild(int id, int childId)
    {
        if (await GetTag(id) is not OkObjectResult) return NotFound($"Parent tag with ID {id} not found");
        if (await GetTag(childId) is not OkObjectResult) return NotFound($"Child tag with ID {childId} not found");

        TagRelations? relation = await _context.TagRelations.FirstOrDefaultAsync(tr => tr.ParentTagId == id && tr.ChildTagId == childId);

        if (relation == null) return NotFound($"Could not find a relationship between the parent tag with ID {id} and child tag with ID {childId}");
        
        _context.TagRelations.Remove(relation);
        await _context.SaveChangesAsync();

        return Ok();
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
                source = relation.ParentTagId,
                target = relation.ChildTagId
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
}