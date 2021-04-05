using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    //create a base controller to inherit
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        
    }
}