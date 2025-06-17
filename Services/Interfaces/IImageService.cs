using Image_Sorter_DotNet.Controllers;
using Image_Sorter_DotNet.Models;

namespace Image_Sorter_DotNet.Services.Interfaces
{
    public interface IImageService
    {
        public Task<Images?> GetImage(int id);
        public Task<bool> DeleteImage(int id);
    }
}
