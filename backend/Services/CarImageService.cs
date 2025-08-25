using CARDB_EF.Data;
using CARDB_EF.Models.EF;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CARDB_EF.Services
{
    /// <summary>
    /// Manages the storage and database records for car images.
    /// </summary>
    public class CarImageService
    {
        private readonly CarDbContext cdb;
        private readonly IWebHostEnvironment env;

        /// <summary>
        /// Initializes a new instance of the <see cref="CarImageService"/>.
        /// </summary>
        /// <param name="cdb">The database context.</param>
        /// <param name="env">The web hosting environment, used for file path resolution.</param>
        public CarImageService(CarDbContext cdb, IWebHostEnvironment env)
        {
            this.cdb = cdb;
            this.env = env;
        }

        /// <summary>
        /// Asynchronously retrieves the database records for all images associated with a specific car.
        /// </summary>
        /// <param name="carId">The ID of the car.</param>
        /// <returns>A list of <see cref="CarImage"/> objects. Returns an empty list if no images are found or the car ID is invalid.</returns>
        public async Task<List<CarImage>> GetCarImagesAsync(int carId)
        {
            if (carId <= 0)
                return new List<CarImage>();

            return await cdb.CarImages
                .AsNoTracking()
                .Where(i => i.CarId == carId)
                .OrderBy(i => i.ImageId)
                .ToListAsync();
        }

        /// <summary>
        /// Asynchronously saves a list of uploaded image files to the server's file system.
        /// </summary>
        /// <param name="images">A list of <see cref="IFormFile"/> objects representing the uploaded images.</param>
        /// <returns>A list of URL strings for the images that were successfully saved.</returns>
        /// <remarks>If an individual file fails to save, the error will be logged to the console, and the process will continue with the next file.</remarks>
        public async Task<List<string>> SaveImagesAndGetUrlsAsync(List<IFormFile> images)
        {
            var savedUrls = new List<string>();
            if (images == null || !images.Any())
                return savedUrls;

            var targetFolderPath = Path.Combine(env.WebRootPath, "images");

            if (!Directory.Exists(targetFolderPath))
            {
                Directory.CreateDirectory(targetFolderPath);
            }

            foreach (var imageFile in images)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                        var filePath = Path.Combine(targetFolderPath, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        // Use forward slashes for web URLs
                        var imageUrl = $"/images/{uniqueFileName}";
                        savedUrls.Add(imageUrl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving image file {imageFile.FileName}: {ex.Message}");
                    }
                }
            }
            return savedUrls;
        }

        /// <summary>
        /// Asynchronously adds image metadata (URLs and alt texts) to the database for a specific car.
        /// </summary>
        /// <param name="carId">The ID of the car to associate the images with.</param>
        /// <param name="urls">A list of image URL strings.</param>
        /// <param name="altTexts">A list of alternative text strings corresponding to the URLs.</param>
        /// <returns>A tuple indicating the success status and a descriptive message.</returns>
        public async Task<(bool Success, string Message)> AddImagesToDbAsync(int carId, List<string> urls, List<string> altTexts)
        {
            if (carId <= 0)
                return (false, "Invalid Car ID.");

            if (urls == null || !urls.Any())
                return (true, "No image URLs provided to save.");

            var carImages = new List<CarImage>();
            for (int i = 0; i < urls.Count; i++)
            {
                var carImage = new CarImage
                {
                    CarId = carId,
                    ImageUrl = urls[i],
                    AltText = (altTexts != null && i < altTexts.Count && !string.IsNullOrWhiteSpace(altTexts[i])) ? altTexts[i] : null,
                };
                carImages.Add(carImage);
            }

            await cdb.CarImages.AddRangeAsync(carImages);
            await cdb.SaveChangesAsync();

            return (true, "Image records saved to the database successfully.");
        }
    }
}