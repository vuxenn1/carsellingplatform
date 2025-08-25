using CARDB_EF.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CARDB_EF.Controllers
{
    [ApiController]
    [Route("api/log")]
    public class LogController : ControllerBase
    {
        private readonly CarDbContext cdb;

        public LogController(CarDbContext cdb)
        {
            this.cdb = cdb;
        }

        [HttpGet("user")]
        public IActionResult GetUserLogs()
        {
            var logs = cdb.UserLogs.OrderByDescending(l => l.ActionTime).ToList();

            return Ok(logs);
        }

        [HttpGet("car")]
        public IActionResult GetCarLogs()
        {
            var logs = cdb.CarLogs.OrderByDescending(l => l.ActionTime).ToList();

            return Ok(logs);
        }

        [HttpGet("offer")]
        public IActionResult GetOfferLogs()
        {
            var logs = cdb.OfferLogs.OrderByDescending(l => l.ActionTime).ToList();

            return Ok(logs);
        }
    }
}