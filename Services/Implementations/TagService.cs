using Microsoft.AspNetCore.Mvc;
using Image_Sorter_DotNet.Data;
using Image_Sorter_DotNet.Models;
using Image_Sorter_DotNet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Image_Sorter_DotNet.Services.Implementations
{
    public class TagService : ITagService
    {
        private readonly ApplicationDbContext _context;

        public TagService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a tag from its ID.
        /// </summary>
        /// <param name="id"> The ID of the tag to get </param>
        public async Task<Tags?> GetTag(int id)
        {
            return await _context.Tags.FindAsync(id);
        }

        /// <summary>
        /// Gets all tags.
        /// </summary>
        public async Task<List<Tags>?> GetAllTags()
        {
            return await _context.Tags.ToListAsync();
        }

        /// <summary>
        /// Gets the immediate child tags of a given tag.
        /// </summary>
        /// <param name="id"> The ID of the tag who's children to get. </param>
        public async Task<List<Tags>?> GetChildren(int id)
        {
            if(await GetTag(id) == null) return null;
            List<Tags?>? children = _context.TagRelations.Where(tr => tr.ParentTagId == id).Select(tr => tr.ChildTag).ToList();
            children?.RemoveAll(child => child == null);
            return children as List<Tags>;
        }

        /// <summary>
        /// Gets all child tags of a given tag.
        /// </summary>
        /// <param name="id"> The ID of the tag who's children to get. </param>
        public async Task<List<Tags>?> GetAllChildren(int id)
        {
            if (await GetTag(id) == null) return null;

            HashSet<int> visitedIds = new();
            List<Tags> result = new();

            await TraverseForChildren(id, visitedIds, result);

            result = result.Where(tag => tag.Id != id).ToList(); // Exclude parent

            /* TODO: Make sure duplicate entries are removed. 
             * They should have been with the hashet but sometimes aren't for some reason.

            Console.WriteLine($"Found {result.Count} children in total");

            foreach (Tags child in result)
            {
                Console.WriteLine($"Found the tag with name: {child.TagName}");
            }
            */

            return result;
        }

        /// <summary>
        /// Recursively traverses the tag relation tree to get all the children of a given tag.
        /// </summary>
        /// <param name="currentId"> The ID of the current tag to check. </param>
        /// <param name="visitedIds"> The set of already visited tag IDs. </param>
        /// <param name="result"> The list of child tags currently found. </param>
        /// <returns> Does not directly return anything, but modifies the given results list of tags. </returns>
        private async Task TraverseForChildren(int currentId, HashSet<int> visitedIds, List<Tags> result)
        {
            if (!visitedIds.Add(currentId)) return;

            List<Tags>? children = await GetChildren(currentId);
            if (children == null) return;

            foreach (Tags child in children)
            {
                result.Add(child);
                await TraverseForChildren(child.Id, visitedIds, result);
            }
        }

        /// <summary>
        /// Converts a list of tags to a list of tag IDs.
        /// </summary>
        /// <param name="tags"> The list of tags to convert. </param>
        public List<int> TagsToId(List<Tags> tags)
        {
            return tags.ConvertAll(t => t.Id).ToList();
        }
    }
}
