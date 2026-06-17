using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "API работает!", time = DateTime.Now });
    }
}
