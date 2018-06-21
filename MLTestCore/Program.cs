using NBADailyFantasyOptimizer.DataAccess;
using System;
using System.Linq;
using NBADailyFantasyOptimizer.DataTransfer;
using MLTestCore.Data;
using System.Collections.Generic;
using NBADailyFantasyOptimizer.Service;
using MLTestCore.Service;
using Microsoft.Extensions.DependencyInjection;
using MLGeneticAlgorithm.Service;

namespace MLTestCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var serviceProvider = StartUp.ConfigureService(new ServiceCollection());

            var startTrainingDay = 2;
            var startTestingDay = 49;
            var numTestingDays = 51;
            var endTestingDay = startTestingDay + numTestingDays;

            var playerDao = serviceProvider.GetService<IPlayerDao>();
            var players = OptimizationService.FilterUnneededPlayers(Enumerable.Range(startTrainingDay, startTestingDay - startTrainingDay).ToList().SelectMany(s => playerDao.GetAllPlayers(s)).ToList());
            var testPlayerDtos = OptimizationService.FilterUnneededPlayers(Enumerable.Range(startTestingDay, numTestingDays).ToList().SelectMany(s => playerDao.GetAllPlayers(s)).ToList()).Where(s => players.Any(z => z.Id == s.Id));
            var testPlayers = testPlayerDtos.Select(Player.From).ToList();
            var playersToTrain = players.ToList();
            var playersToTest = testPlayers.ToList();
            var playerDtosToTest = testPlayerDtos.ToList();
            var testPs = testPlayers.ToList();
            var testPDtos = testPlayerDtos.ToList();
            var trainingService = serviceProvider.GetService<IPredictionService>();

            var threePointerLabels = new List<string> { "LastGameProjDiffRatio", "LastGameMinutes", "IsHome", "LastGameAssists", "LastGameFreethrowsMade", "Salary", "LastGameOffensiveRebounds", "FantasyPointsPerGame", "Id", "Team", "Position", "TopPlayerForTeam", "LastGameThreePointersAttempted", "LastGamePlusMinus", "LastGamePoints", "ActualPoints" };
            var pointsLabels = new List<string> { "LastGameSteals", "LastGameProjDiffRatio", "Roi", "LastGamePersonalFouls", "ProjectionRg", "FantasyPointsPerGame", "Id", "Opponent", "Position", "LastGameProjection", "LastGameFieldGoalsAttempted", "LastGameFreethrowsAttempted", "IsBench", "LastGameOffensiveRebounds", "LastGameDefensiveRebounds", "LastGameAssists", "ActualPoints" };
            var defensiveReboundsLabels = new List<string> { "LastGameThreePointersMade", "LastGameAct", "LastGameBlocks", "SeasonCeiling", "Id", "Team", "Opponent", "Position", "Salary", "LastGameDiff", "Roi", "LastGameMinutes", "LastGameFieldGoalsMade", "LastGameTurnovers", "ProjectionRg", "ActualPoints" };
            var offensiveReboundsLabels = new List<string> { "LastGameFieldGoalsMade", "LastGameFieldGoalsAttempted", "Salary", "Id", "Team", "Position", "Projection", "TopPlayerForTeam", "LastGameThreePointersAttempted", "LastGameSteals", "LastGameTurnovers", "ActualPoints" };
            var assistsLabels = new List<string> { "FantasyPointsPerGame", "SeasonCeiling", "LastGameDefensiveRebounds", "Id", "Team", "Position", "Projection", "IsHome", "LastGameAssists", "ActualPoints" };
            var stealsLabels = new List<string> { "LastGameTurnovers", "LastGamePoints", "Salary", "LastGameAssists", "LastGameDefensiveRebounds", "SeasonFloor", "LastGameMinutes", "Id", "Opponent", "Position", "Projection", "LastGameProjection", "FantasyPointsPerGame", "IsBench", "LastGameThreePointersAttempted", "LastGameOffensiveRebounds", "LastGameBlocks", "LastGamePersonalFouls", "ProjectionRg", "SeasonCeiling", "ActualPoints" };
            var blocksLabels = new List<string> { "LastGameDefensiveRebounds", "LastGameSteals", "FantasyPointsPerGame", "LastGameThreePointersAttempted", "TopPlayerAtPositionForTeam", "LastGameFieldGoalsAttempted", "LastGamePersonalFouls", "ProjectionRg", "Roi", "Id", "Team", "Opponent", "Position", "LastGameAct", "LastGameProjDiffRatio", "LastGameBlocks", "SeasonFloor", "ActualPoints" };
            var turnoversLabels = new List<string> { "IsBench", "LastGameAssists", "LastGameProjDiffRatio", "FantasyPointsPerGame", "LastGameFieldGoalsAttempted", "LastGameBlocks", "LastGameFreethrowsAttempted", "Team", "Position", "LastGameFreethrowsMade", "LastGameOffensiveRebounds", "LastGameTurnovers", "ProjectionRg", "ActualPoints" };

            var threePointerFields = typeof(Player).GetFields().Where(f => threePointerLabels.Contains(f.Name)).ToList();
            if (threePointerFields.Count != threePointerLabels.Count)
                throw new Exception("Misspelled field probably!");
            playersToTrain.ForEach(s => s.ActualPoints = s.ActualResults.OrderByDescending(z => z.Day).FirstOrDefault()?.ThreePointersMade ?? 0);
            var threePointerObjs = TypeService.CloneObjectsToNewObjectType(playersToTrain.Select(Player.From).ToList(), threePointerFields);
            var threePointerModel = trainingService.Train<Player, PointPrediction>(threePointerObjs.Select(s => ((Player)s)).ToList(), "ActualPoints");
            trainingService.Predict(threePointerModel, testPs);
            testPDtos.ForEach(s => s.ThreePointersMade = testPs.First(z => z.Id == s.Id && z.Day == s.Day)?.Pred ?? 0);


            var pointsFields = typeof(Player).GetFields().Where(f => pointsLabels.Contains(f.Name)).ToList();
            if (pointsFields.Count != pointsLabels.Count)
                throw new Exception("Misspelled field probably!");
            playersToTrain.ForEach(s => s.ActualPoints = s.ActualResults.OrderByDescending(z => z.Day).FirstOrDefault()?.Points ?? 0);
            var pointsObjs = TypeService.CloneObjectsToNewObjectType(playersToTrain.Select(Player.From).ToList(), pointsFields);
            var pointsModel = trainingService.Train<Player, PointPrediction>(pointsObjs.Select(s => ((Player)s)).ToList(), "ActualPoints");
            trainingService.Predict(pointsModel, testPs);
            testPDtos.ForEach(s => s.Points = testPs.First(z => z.Id == s.Id && z.Day == s.Day)?.Pred ?? 0);


            var defensiveReboundsFields = typeof(Player).GetFields().Where(f => defensiveReboundsLabels.Contains(f.Name)).ToList();
            if (defensiveReboundsFields.Count != defensiveReboundsLabels.Count)
                throw new Exception("Misspelled field probably!");
            playersToTrain.ForEach(s => s.ActualPoints = s.ActualResults.OrderByDescending(z => z.Day).FirstOrDefault()?.DefensiveRebounds ?? 0);
            var defensiveReboundsObjs = TypeService.CloneObjectsToNewObjectType(playersToTrain.Select(Player.From).ToList(), defensiveReboundsFields);
            var defensiveReboundsModel = trainingService.Train<Player, PointPrediction>(defensiveReboundsObjs.Select(s => ((Player)s)).ToList(), "ActualPoints");
            trainingService.Predict(defensiveReboundsModel, testPs);
            testPDtos.ForEach(s => s.DefensiveRebounds = testPs.First(z => z.Id == s.Id && z.Day == s.Day)?.Pred ?? 0);

            var offensiveReboundsFields = typeof(Player).GetFields().Where(f => offensiveReboundsLabels.Contains(f.Name)).ToList();
            if (offensiveReboundsFields.Count != offensiveReboundsLabels.Count)
                throw new Exception("Misspelled field probably!");
            playersToTrain.ForEach(s => s.ActualPoints = s.ActualResults.OrderByDescending(z => z.Day).FirstOrDefault()?.OffensiveRebounds ?? 0);
            var offensiveReboundsObjs = TypeService.CloneObjectsToNewObjectType(playersToTrain.Select(Player.From).ToList(), offensiveReboundsFields);
            var offensiveReboundsModel = trainingService.Train<Player, PointPrediction>(offensiveReboundsObjs.Select(s => ((Player)s)).ToList(), "ActualPoints");
            trainingService.Predict(offensiveReboundsModel, testPs);
            testPDtos.ForEach(s => s.OffensiveRebounds = testPs.First(z => z.Id == s.Id && z.Day == s.Day)?.Pred ?? 0);


            var assistsFields = typeof(Player).GetFields().Where(f => assistsLabels.Contains(f.Name)).ToList();
            if (assistsFields.Count != assistsLabels.Count)
                throw new Exception("Misspelled field probably!");
            playersToTrain.ForEach(s => s.ActualPoints = s.ActualResults.OrderByDescending(z => z.Day).FirstOrDefault()?.Assists ?? 0);
            var assistsObjs = TypeService.CloneObjectsToNewObjectType(playersToTrain.Select(Player.From).ToList(), assistsFields);
            var assistsModel = trainingService.Train<Player, PointPrediction>(assistsObjs.Select(s => ((Player)s)).ToList(), "ActualPoints");
            trainingService.Predict(assistsModel, testPs);
            testPDtos.ForEach(s => s.Assists = testPs.First(z => z.Id == s.Id && z.Day == s.Day)?.Pred ?? 0);


            var stealsFields = typeof(Player).GetFields().Where(f => stealsLabels.Contains(f.Name)).ToList();
            if (stealsFields.Count != stealsLabels.Count)
                throw new Exception("Misspelled field probably!");
            playersToTrain.ForEach(s => s.ActualPoints = s.ActualResults.OrderByDescending(z => z.Day).FirstOrDefault()?.Steals ?? 0);
            var stealsObjs = TypeService.CloneObjectsToNewObjectType(playersToTrain.Select(Player.From).ToList(), stealsFields);
            var stealsModel = trainingService.Train<Player, PointPrediction>(stealsObjs.Select(s => ((Player)s)).ToList(), "ActualPoints");
            trainingService.Predict(stealsModel, testPs);
            testPDtos.ForEach(s => s.Steals = testPs.First(z => z.Id == s.Id && z.Day == s.Day)?.Pred ?? 0);


            var blocksFields = typeof(Player).GetFields().Where(f => blocksLabels.Contains(f.Name)).ToList();
            if (blocksFields.Count != blocksLabels.Count)
                throw new Exception("Misspelled field probably!");
            playersToTrain.ForEach(s => s.ActualPoints = s.ActualResults.OrderByDescending(z => z.Day).FirstOrDefault()?.Blocks ?? 0);
            var blocksObjs = TypeService.CloneObjectsToNewObjectType(playersToTrain.Select(Player.From).ToList(), blocksFields);
            var blocksModel = trainingService.Train<Player, PointPrediction>(blocksObjs.Select(s => ((Player)s)).ToList(), "ActualPoints");
            trainingService.Predict(blocksModel, testPs);
            testPDtos.ForEach(s => s.Blocks = testPs.First(z => z.Id == s.Id && z.Day == s.Day)?.Pred ?? 0);


            var turnoversFields = typeof(Player).GetFields().Where(f => turnoversLabels.Contains(f.Name)).ToList();
            if (turnoversFields.Count != turnoversLabels.Count)
                throw new Exception("Misspelled field probably!");
            playersToTrain.ForEach(s => s.ActualPoints = s.ActualResults.OrderByDescending(z => z.Day).FirstOrDefault()?.Turnovers ?? 0);
            var turnoversObjs = TypeService.CloneObjectsToNewObjectType(playersToTrain.Select(Player.From).ToList(), turnoversFields);
            var turnoversModel = trainingService.Train<Player, PointPrediction>(turnoversObjs.Select(s => ((Player)s)).ToList(), "ActualPoints");
            trainingService.Predict(turnoversModel, testPs);
            testPDtos.ForEach(s => s.Turnovers = testPs.First(z => z.Id == s.Id && z.Day == s.Day)?.Pred ?? 0);

            var totalProj = testPDtos.Sum(s => Math.Abs(s.Projection - s.ActualPoints));
            Console.WriteLine("Proj: " + totalProj);

            testPDtos.ForEach(s => s.SeasonAverage = (float)s.Projection);
            testPDtos.ForEach(s =>
            {
                s.Projection = Math.Round(s.ThreePointersMade * ConstantDto.ThreePointPoints
               + s.Points * ConstantDto.PointPoints
               + (s.OffensiveRebounds + s.DefensiveRebounds) * ConstantDto.ReboundPoints
               + s.Assists * ConstantDto.AssistPoints
               + s.Steals * ConstantDto.StealPoints
               + s.Blocks * ConstantDto.BlockPoints
               + s.Turnovers * ConstantDto.TurnoverPoints, 2);
            });


            //var model = trainingService.Train<Player, PointPrediction>(playersToTrain.Select(Player.From).ToList(), "ActualPoints");
            //trainingService.Predict(model, testPs);
            testPDtos = testPDtos.OrderByDescending(s => Math.Abs(s.SeasonAverage - s.ActualPoints) - Math.Abs(s.Projection - s.ActualPoints)).ToList();
            testPDtos.ForEach(s => Console.WriteLine(s.Name + " Pred=" + s.Projection + " Proj=" + s.SeasonAverage + " Act:" + s.ActualPoints + "-" + s.ThreePointersMade + "-" + s.Points + "-" + s.OffensiveRebounds + "-" + s.DefensiveRebounds + "-" + s.Assists + "-" + s.Steals + "-" + s.Blocks + "-" + s.Turnovers));
            ProjMain(startTestingDay, endTestingDay, testPDtos);


            //var asdf = testPDtos.Where(s => s.Pred == 0).ToList();
            //var aaa = testPDtos.Where(s => s.Pred != 0).ToList();
            // var asdfads = aaa.Max(s => Math.Abs(s.Pred - s.ActualPoints) - Math.Abs(s.Projection - s.ActualPoints));
            //var adfasdf = aaa.OrderByDescending(s => Math.Abs(s.Pred - s.ActualPoints) - Math.Abs(s.Projection - s.ActualPoints)).ToList();

            Console.WriteLine("Proj: " + totalProj);
            var totalPred = testPDtos.Sum(s => Math.Abs(s.Projection - s.ActualPoints));
            Console.WriteLine("Pred: " + totalPred);

            var totalAct = testPDtos.Sum(s => Math.Abs(s.ActualPoints));
            Console.WriteLine("Act: " + totalAct);
            //LogResults(www);

            //LogResults(testPlayers);
            ////ProjMain(playerDtos);
            ////ProjMain(null);
        }

        //private static void LogResults(List<PlayerDto> players)
        //{
        //    double totalProjDiff = 0;

        //    foreach(var player in players)
        //    {
        //        totalProjDiff += Math.Abs((float)player.Projection - player.ActualPoints);
        //    }

        //    Console.WriteLine($"Total Proj difference: {totalProjDiff}.  Total ML Diff: {totalPredDiff}.  ML Diff better by {totalProjDiff - totalPredDiff}");
        //}

        private static void ProjMain(int startDay, int endDay, List<PlayerDto> playersToUse = null)
        {
            var service = new OptimizationService();
            var dao = new PlayerDao();

            Dictionary<int, List<double>> allTotals = new Dictionary<int, List<double>>();

            var numRostersWanted = 6;
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
                var weeksTopRosters = service.Optimize(i, numRostersWanted, removalRoi, minuteCutoff, projDivision, projMin, playerMinMinutesPrevGame, allTotals, playersToUse?.Where(p => p.Day == i).ToList());
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
                Console.WriteLine(string.Format("Final Totals: "));
                Console.WriteLine(string.Format("Count 280+: {0}", allTopRosters.Select(s => s.TotalActual).Count(s => s >= 280)));
                Console.WriteLine(string.Format("Count 300+: {0}", allTopRosters.Select(s => s.TotalActual).Count(s => s >= 300)));
                Console.WriteLine(string.Format("Average Total: {0}", allTopRosters.Select(s => s.TotalActual).Average()));
                Console.WriteLine(string.Format("Highest Total: {0}", allTopRosters.Select(s => s.TotalActual).Max()));
                //}            Console.WriteLine();
                //foreach (var iteration in allTotals)
                //{
                //    Console.WriteLine();
                //    Console.WriteLine("Iteration " + iteration.Key + ": Count: " + iteration.Value.Count());
                //    Console.WriteLine("Average: " + iteration.Value.Average());
                //    Console.WriteLine("Count 280+: " + iteration.Value.Count(x => x >= 280));
                //    Console.WriteLine("Count 300+: " + iteration.Value.Count(x => x >= 300));
                //}
            }

            Console.WriteLine("End.");
            Console.ReadLine();
        }
    }
}