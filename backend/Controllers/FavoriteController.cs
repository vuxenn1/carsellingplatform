using Microsoft.AspNetCore.Mvc;
using CARDB_EF.Services;
using CARDB_EF.Models.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CARDB_EF.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user/favorite")]
    public class FavoriteController : ControllerBase
    {
        private readonly FavoriteService favoriteService;
        public FavoriteController(FavoriteService favoriteService) => this.favoriteService = favoriteService;

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] FavoriteDto dto)
        {
            if (dto == null || dto.UserId <= 0 || dto.CarId <= 0)
                return BadRequest(new { message = "User ID and Car ID must be provided." });

            var result = await favoriteService.AddFavoriteAsync(dto.UserId, dto.CarId);

            return result.Success
                ? Ok(new { message = result.Message })
                : BadRequest(new { message = result.Message });
        }

        [HttpPost("remove")]
        public async Task<IActionResult> Remove([FromBody] FavoriteDto dto)
        {
            if (dto == null || dto.UserId <= 0 || dto.CarId <= 0)
                return BadRequest(new { message = "User ID and Car ID must be provided." });

            var result = await favoriteService.RemoveFavoriteAsync(dto.UserId, dto.CarId);

            return result.Success
                ? Ok(new { message = result.Message })
                : NotFound(new { message = result.Message });
        }

        [HttpGet("check")]
        public async Task<IActionResult> Check([FromQuery] int userId, [FromQuery] int carId)
        {
            if (userId <= 0 || carId <= 0)
                return BadRequest();

            var isFav = await favoriteService.IsCarFavoritedAsync(userId, carId);
            return Ok(new { isFavorited = isFav });
        }

        [HttpGet("count/{userId}")]
        public async Task<IActionResult> GetFavoriteCount(int userId)
        {
            if (userId <= 0)
                return BadRequest();

            var count = await favoriteService.GetFavoriteCountAsync(userId);
            return Ok(count);
        }
    }
}