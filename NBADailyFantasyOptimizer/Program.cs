using NBADailyFantasyOptimizer.DataAccess;
using NBADailyFantasyOptimizer.DataTransfer;
using NBADailyFantasyOptimizer.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBADailyFantasyOptimizer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var service = new OptimizationService();
            var dao = new PlayerDao();

            Dictionary<int, List<double>> allTotals = new Dictionary<int, List<double>>();

            var numRostersWanted = 13;
            var startDay = 100;
            var endDay = 100;
            var removalRoi = .75;
            double minuteCutoff = 22.0;
            int projDivision = 10;
            int projMin = 19;
            int playerMinMinutesPrevGame = 19;

            var allTopRosters = new List<RosterDto>();

            for (var i = startDay; i <= endDay; i++)
            {
                Console.WriteLine();
                Console.WriteLine(string.Format("Week {0}", i));
                var weeksTopRosters = service.Optimize(i, numRostersWanted, removalRoi, minuteCutoff, projDivision, projMin, playerMinMinutesPrevGame, allTotals);
                if (weeksTopRosters.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine(string.Format("Count 280+: {0}", weeksTopRosters.Select(s => s.TotalActual).Count(s => s >= 280)));
                    Console.WriteLine(string.Format("Average Total: {0}", weeksTopRosters.Select(s => s.TotalActual).Average()));
                    Console.WriteLine(string.Format("Highest Total: {0}", weeksTopRosters.Select(s => s.TotalActual).Max()));
                    allTopRosters.AddRange(weeksTopRosters);
                }
            }
            if (allTopRosters.Any(s => s.TotalActual > 0))
            {
                Console.WriteLine();
                Console.WriteLine(string.Format("Final Totals: playerMinMinutesPrevGame {0}", playerMinMinutesPrevGame));
                Console.WriteLine(string.Format("Count 280+: {0}", allTopRosters.Select(s => s.TotalActual).Count(s => s >= 280)));
                Console.WriteLine(string.Format("Count 300+: {0}", allTopRosters.Select(s => s.TotalActual).Count(s => s >= 300)));
                Console.WriteLine(string.Format("Average Total: {0}", allTopRosters.Select(s => s.TotalActual).Average()));
                Console.WriteLine(string.Format("Highest Total: {0}", allTopRosters.Select(s => s.TotalActual).Max()));
                //}            Console.WriteLine();
                foreach (var iteration in allTotals)
                {
                    Console.WriteLine();
                    Console.WriteLine("Iteration " + iteration.Key + ": Count: " + iteration.Value.Count());
                    Console.WriteLine("Average: " + iteration.Value.Average());
                    Console.WriteLine("Count 280+: " + iteration.Value.Count(x => x >= 280));
                    Console.WriteLine("Count 300+: " + iteration.Value.Count(x => x >= 300));
                }
            }

            Console.WriteLine("End.");
            Console.ReadLine();
        }
    }
}
