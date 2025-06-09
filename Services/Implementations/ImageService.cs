using Microsoft.AspNetCore.Mvc;
using Image_Sorter_DotNet.Data;
using Image_Sorter_DotNet.Models;
using Image_Sorter_DotNet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Image_Sorter_DotNet.Services.Implementations
{
    public class ImageService : IImageService
    {
        private readonly ApplicationDbContext _context;

        public ImageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Images?> GetImage(int id)
        {
            return await _context.Images.FindAsync(id);
        }
    }
}
