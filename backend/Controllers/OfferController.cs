using Microsoft.AspNetCore.Mvc;
using CARDB_EF.Models.EF;
using CARDB_EF.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CARDB_EF.Controllers
{
    [ApiController]
    [Route("api/offer")]
    [Authorize]
    public class OfferController : ControllerBase
    {
        private readonly OfferService offerService;

        public OfferController(OfferService offerService)
        {
            this.offerService = offerService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOffer([FromBody] Offer offer)
        {
            var result = await offerService.CreateOfferAsync(offer);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpPut("accept/{offerId}")]
        public async Task<IActionResult> AcceptOffer(int offerId)
        {
            var result = await offerService.AcceptOfferAsync(offerId);
            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpPut("reject/{offerId}")]
        public async Task<IActionResult> RejectOffer(int offerId)
        {
            var result = await offerService.RejectOfferAsync(offerId);
            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpGet("sent/{userId}")]
        public async Task<IActionResult> GetSentOffers(int userId)
        {
            var currentUserId = User.FindFirstValue("userId");
            if (userId.ToString() != currentUserId)
            {
                return Forbid();
            }
            var offers = await offerService.GetSentOffersAsync(userId);
            return Ok(offers);
        }

        [HttpGet("received/{userId}")]
        public async Task<IActionResult> GetReceivedOffers(int userId)
        {
            var currentUserId = User.FindFirstValue("userId");
            if (userId.ToString() != currentUserId)
            {
                return Forbid();
            }
            var offers = await offerService.GetReceivedOffersAsync(userId);
            return Ok(offers);
        }

        [HttpGet("received/{userId}/pending")]
        public async Task<IActionResult> GetPendingReceivedOfferCount(int userId)
        {
            var currentUserId = User.FindFirstValue("userId");
            if (userId.ToString() != currentUserId)
            {
                return Forbid();
            }
            var count = await offerService.GetPendingReceivedOfferCountAsync(userId);
            return Ok(count);
        }
    }
}