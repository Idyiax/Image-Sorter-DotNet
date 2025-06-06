using Image_Sorter_DotNet.Controllers;
using Image_Sorter_DotNet.Models;

namespace Image_Sorter_DotNet.Services.Interfaces
{
    public interface ITagService
    {
        public Task<Tags?> GetTag(int id);
        public Task<List<Tags>?> GetAllTags();
        public Task<List<Tags>?> GetChildren(int id);
        public Task<List<Tags>?> GetAllChildren(int id);
        public List<int> TagsToId(List<Tags> tags);
    }
}
