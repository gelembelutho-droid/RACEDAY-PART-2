using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public PaymentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all payments for the logged-in user
        /// </summary>
        [HttpGet("my-payments")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = GetCurrentUserId();

            var payments = await _context.Payments
                .Include(p => p.Registration)
                    .ThenInclude(r => r!.Race)
                        .ThenInclude(ra => ra!.Event)
                .Where(p => p.Registration != null && p.Registration.UserId == userId)
                .Select(p => new
                {
                    p.PaymentId,
                    p.RegistrationId,
                    RaceName = p.Registration != null && p.Registration.Race != null
                        ? p.Registration.Race.RaceName : "",
                    EventName = p.Registration != null && p.Registration.Race != null
                        && p.Registration.Race.Event != null
                        ? p.Registration.Race.Event.EventName : "",
                    p.Amount,
                    p.PaymentDate,
                    p.PaymentMethod,
                    p.TransactionId
                })
                .ToListAsync();

            return Ok(payments);
        }

        /// <summary>
        /// Get a specific payment by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPayment(int id)
        {
            var userId = GetCurrentUserId();

            var payment = await _context.Payments
                .Include(p => p.Registration)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            if (payment.Registration?.UserId != userId)
                return Forbid();

            return Ok(new
            {
                payment.PaymentId,
                payment.RegistrationId,
                payment.Amount,
                payment.PaymentDate,
                payment.PaymentMethod,
                payment.TransactionId
            });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? "0";

            return int.Parse(userIdClaim);
        }
    }
}