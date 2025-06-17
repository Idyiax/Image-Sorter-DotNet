using Image_Sorter_DotNet.Data;
using Image_Sorter_DotNet.Models;
using Image_Sorter_DotNet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Image_Sorter_DotNet.Services.Implementations
{
    public class ImageService : IImageService
    {
        private readonly ApplicationDbContext _context;

        public ImageService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets an image from it's ID.
        /// </summary>
        /// <param name="id"> The ID of the image to get </param>
        public async Task<Images?> GetImage(int id)
        {
            return await _context.Images.FindAsync(id);
        }

        /// <summary>
        /// Deletes an image from it's ID.
        /// </summary>
        /// <param name="id"> The ID of the image to delete </param>
        /// <returns> Whether the image was successfully deleted </returns>
        public async Task<bool> DeleteImage(int id)
        {
            Images? image = await _context.Images.FindAsync(id);
            if (image == null) return false;

            string filePathEnd = image.FilePathName.Replace('/', '\\');
            filePathEnd = filePathEnd.TrimStart('\\');
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePathEnd);

            try
            {
                File.Delete(filePath);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
