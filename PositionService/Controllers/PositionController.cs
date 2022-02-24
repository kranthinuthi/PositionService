using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PositionService.Biz;
using PositionService.Resources;

namespace PositionService.Controllers
{
    [EnableCors("MyPolicy")]
    [ApiController]
    [Route("[controller]")]
    public class PositionController : ControllerBase
    {

        [HttpGet]
        public async Task<List<Position>> GetPositionsAsync()
        {
            PositionManager positionManager = new PositionManager();
            return await positionManager.GetCurrentNetPositionsAsync();
        }
    }
}
