using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PositionService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
       
        public HealthController()
        {
       
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("healthy");
        }
    }
}
