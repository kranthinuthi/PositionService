using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PositionService.Resources;

namespace PositionService.Biz
{
    public interface IPositionManager
    {
        public Task<List<Position>> GetCurrentNetPositionsAsync();
    }
}
