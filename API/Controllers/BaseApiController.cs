using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    // the behavior of [ApiController] is overrided. 
    // Look at inside ApplicationServiceExtensions class > services.Configure<ApiBehaviorOptions>().
    // Before overriding we have {type:"", title:"", status:, traceId:"", errors{id:[]}}
    // After overriding we have {errors:[], statusCode:400, message:""}
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {

    }
}