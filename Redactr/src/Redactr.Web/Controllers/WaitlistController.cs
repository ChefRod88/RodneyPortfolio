using Microsoft.AspNetCore.Mvc;
using Redactr.Web.Models;

namespace Redactr.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaitlistController : ControllerBase
{
    private readonly ILogger<WaitlistController> _logger;

    public WaitlistController(ILogger<WaitlistController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Post([FromBody] WaitlistRequest request)
    {
        _logger.LogInformation("Waitlist signup: Email={Email}, Role={Role}", request.Email, request.Role);

        return Ok(new
        {
            success = true,
            message = "You're on the list!",
            position = 312
        });
    }
}
