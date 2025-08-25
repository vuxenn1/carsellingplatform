using Microsoft.AspNetCore.Mvc;
using CARDB_EF.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace CARDB_EF.Controllers
{
    [ApiController]
    [Route("api/carimage")]
    public class CarImageController : ControllerBase
    {
        private readonly CarImageService carImageService;

        public CarImageController(CarImageService carImageService)
        {
            this.carImageService = carImageService;
        }

        [HttpGet("{carId}")]
        public async Task<IActionResult> GetCarImages(int carId)
        {
            var images = await carImageService.GetCarImagesAsync(carId);
            return Ok(images);
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] int carId, [FromForm] List<string> altTexts, List<IFormFile> images)
        {
            if (images == null || !images.Any())
                return BadRequest(new { message = "No images provided." });

            try
            {
                var urls = await carImageService.SaveImagesAndGetUrlsAsync(images);
                if (urls == null || !urls.Any())
                {
                    return StatusCode(500, new { message = "Failed to save any images to the file system." });
                }

                var result = await carImageService.AddImagesToDbAsync(carId, urls, altTexts);

                if (result.Success)
                {
                    return Ok(new { message = $"{urls.Count} images uploaded successfully." });
                }

                return StatusCode(500, new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An unexpected error occurred: {ex.Message}" });
            }
        }
    }
}