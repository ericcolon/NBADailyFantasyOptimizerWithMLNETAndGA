using Microsoft.Extensions.DependencyInjection;
using MLGeneticAlgorithm.Domain;
using MLGeneticAlgorithm.Service;
using MLTestCore.Data;
using NBADailyFantasyOptimizer.DataAccess;
using NBADailyFantasyOptimizer.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MLGeneticAlgorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = StartUp.ConfigureService(new ServiceCollection());

            var generations = 100;
            var popSize = 20;
            var generationService = serviceProvider.GetService<IGenerationService>();
            var playerDao = serviceProvider.GetService<IPlayerDao>();
            var players = OptimizationService.FilterUnneededPlayers(Enumerable.Range(2, 48).ToList().SelectMany(s => playerDao.GetAllPlayers(s)).ToList());
            var testPlayerDtos = OptimizationService.FilterUnneededPlayers(Enumerable.Range(49, 13).ToList().SelectMany(s => playerDao.GetAllPlayers(s)).ToList());

            players.RemoveAll(s => s.ActualResults.Find(z => z.Day == s.Day) == null);
            players.RemoveAll(s => !s.ActualResultsBeforeDay.Any() || !s.Projections.Any(z => z.Day < s.Day));

            //Update BOTH of these!!!!!!!!!!!!!!!!!!!!
            //players.ForEach(s => s.ActualPoints = s.ActualResults.Find(z => z.Day == s.Day)?.Turnovers ?? 0);
            //testPlayerDtos.ForEach(s => s.ActualPoints = s.ActualResults.Find(z => z.Day == s.Day)?.Turnovers ?? 0);

            List<PlayerInfo> playerInfosToSeed = new List<PlayerInfo>();
            var seed1FieldNames = new List<string> { "LastGameProjDiffRatio", "LastGameMinutes", "IsHome", "LastGameAssists", "LastGameFreethrowsMade", "Salary", "LastGameOffensiveRebounds", "FantasyPointsPerGame", "Id", "Team", "Position", "TopPlayerForTeam", "LastGameThreePointersAttempted", "LastGamePlusMinus", "LastGamePoints", "ActualPoints" };
            var seed1 = typeof(Player).GetFields().Where(s => seed1FieldNames.Contains(s.Name)).ToList();
            var seed2FieldNames = new List<string> { "LastGameThreePointersMade", "LastGameProjDiffRatio", "LastGameMinutes", "IsHome", "LastGameAssists", "LastGameFreethrowsMade", "Salary", "LastGameOffensiveRebounds", "FantasyPointsPerGame", "Id", "Team", "Position", "TopPlayerForTeam", "LastGameThreePointersAttempted", "LastGamePlusMinus", "LastGamePoints", "ActualPoints" };
            var seed2 = typeof(Player).GetFields().Where(s => seed1FieldNames.Contains(s.Name)).ToList();
            if (seed1.Select(s => s.Name).Any(z => !seed1FieldNames.Contains(z)) || seed1FieldNames.Any(z => !seed1.Any(s => s.Name == z)))
            {
                throw new Exception("Not here");
            }
           // playerInfosToSeed.Add(new PlayerInfo { Fields = seed1, Id = popSize - 1 });
           // playerInfosToSeed.Add(new PlayerInfo { Fields = seed2, Id = popSize - 2 });
            List<string> results = new List<string>();
            //foreach (var pos in players.Select(s => s.Position).Distinct().ToList())
            //{
                var bestFit = generationService.GetBestFit(players/*.Where(s => s.Position == pos)*/.ToList(), testPlayerDtos.Where(s => players.Any(z => z.Id == s.Id)).ToList()/*.Where(s => s.Position == pos)*/.ToList(), generations, popSize, playerInfosToSeed);
                var totalProjDiff = testPlayerDtos/*.Where(s => s.Position == pos)*/.Sum(s => Math.Abs(s.Projection - s.ActualPoints));
                var totalProjRgDiff = testPlayerDtos/*.Where(s => s.Position == pos)*/.Sum(s => Math.Abs((s.Projections.FirstOrDefault(z => z.Day == s.Day)?.ProjectionRg ?? 0) - s.ActualPoints));
                var totalAct = testPlayerDtos/*.Where(s => s.Position == pos)*/.Sum(s => s.ActualPoints);
                results.Add(": ML:" + bestFit.TotalPredictionDifference + " DFN Proj:" + totalProjDiff + " RG Proj:" + totalProjRgDiff + " Act:" + totalAct + "     -    " + String.Join(", ", bestFit.Fields.Select(s => s.Name).ToArray()));
            //}

            foreach(var result in results)
            {
                Console.WriteLine(result);
            }
        }
    }
}
