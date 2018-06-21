using Microsoft.ML;
using MLTestCore.Data;
using NBADailyFantasyOptimizer.DataTransfer;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MLGeneticAlgorithm.Domain
{
    public class Generation
    {
        public int GenerationAge { get; set; }
        public List<PlayerInfo> PlayerInfos { get; set; } = new List<PlayerInfo>();
    }
}
