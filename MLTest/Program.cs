using NBADailyFantasyOptimizer.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;

namespace MLTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var dao = new PlayerDao();

            var allPlayers = Enumerable.Range(50, 50).ToList().SelectMany(s => dao.GetAllPlayers(s)).ToList();

            var ffd = allPlayers.Where(s => s.ActualPoints > 0 && s.Projection > 0).ToList();

            var pipeline = new LearningPipeline();
        }
    }
}
