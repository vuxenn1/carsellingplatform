using CARDB_EF.Models.DTOs;
using CARDB_EF.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CARDB_EF.Controllers
{
    [ApiController]
    [Route("api/car")]
    public class CarController : ControllerBase
    {
        private readonly CarService carService;

        public CarController(CarService carService)
        {
            this.carService = carService;
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetCarDetails(int id)
        {
            var car = await carService.GetCarDetailsAsync(id);
            if (car == null)
                return NotFound(new { message = "Car not found." });

            return Ok(car);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCars()
        {
            var cars = await carService.GetAllCarListAsync();
            return Ok(cars);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableCars(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string brand = "all",
            [FromQuery] string sortBy = "listDate",
            [FromQuery] string sortDirection = "desc")
        {
            var pagedResult = await carService.GetAllAvailableCarListAsync(pageNumber, pageSize, brand, sortBy, sortDirection);
            return Ok(pagedResult);
        }

        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserCars(int userId)
        {
            var cars = await carService.GetUserCarListAsync(userId);
            return Ok(cars);
        }

        [Authorize]
        [HttpGet("favorites/{userId}")]
        public async Task<IActionResult> GetFavoriteCars(int userId)
        {
            var cars = await carService.GetFavoriteCarListAsync(userId);
            return Ok(cars);
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadCar([FromBody] UploadCarModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid data provided." });

            var result = await carService.UploadCarAsync(model);
            if (!result.Success)
                return StatusCode(500, new { message = result.Message });

            return Ok(new { message = result.Message, carId = result.CarId });
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] CarUpdateDto carUpdateDto)
        {
            if (carUpdateDto == null)
                return BadRequest(new { message = "Invalid car data provided." });

            var result = await carService.UpdateCarAsync(id, carUpdateDto);
            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [Authorize]
        [HttpPut("mark/sold/{carId}")]
        public async Task<IActionResult> MarkAsSold(int carId)
        {
            var result = await carService.MarkCarSoldAsync(carId);
            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [Authorize]
        [HttpPut("mark/available/{carId}")]
        public async Task<IActionResult> MarkAsAvailable(int carId)
        {
            var result = await carService.MarkCarAvailableAsync(carId);
            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}