using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookResearchApp.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        private string GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return userIdClaim != null ? (userIdClaim.Value) : string.Empty;
        }
    }
}
