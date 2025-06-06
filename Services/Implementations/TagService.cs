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

            var visitedIds = new HashSet<int>();
            var result = new List<Tags>();

            await TraverseForChildren(id, visitedIds, result);

            return (List<Tags>?)result.Where(tag => tag.Id != id); // Exclude parent
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
    }
}
