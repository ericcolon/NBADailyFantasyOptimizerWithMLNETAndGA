using MLGeneticAlgorithm.Domain;
using MLTestCore.Data;
using MLTestCore.Service;
using NBADailyFantasyOptimizer.DataTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MLGeneticAlgorithm.Service
{
    public interface IGenerationService
    {
        PlayerInfo GetBestFit(List<PlayerDto> playersToTrain, List<PlayerDto> playersToTest, int generations, int populationSize, List<PlayerInfo> seededParents = null);
    }

    public class GenerationService : IGenerationService
    {
        private readonly IPredictionService _predictionService;

        public GenerationService(IPredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        public PlayerInfo GetBestFit(List<PlayerDto> playersToTrain, List<PlayerDto> playersToTest, int generations, int populationSize, List<PlayerInfo> seededParents = null)
        {
            //Generate
            var gen = CreateRandomGeneration(populationSize - (seededParents?.Count ?? 0));

            if (seededParents != null)
            {
                gen.PlayerInfos.AddRange(seededParents);
            }

            //GetFitness
            gen.PlayerInfos.ForEach(s => GetFitness(s, playersToTrain, playersToTest));

            List<Generation> allGenerations = new List<Generation>();
            HashSet<PlayerInfo> allAncestorHashes = new HashSet<PlayerInfo>();

            try
            {
                for (int i = 0; i < generations; i++)
                {
                    LogGeneration(gen);
                    allGenerations.Add(gen);

                    //Select
                    var breeders = Select(gen.PlayerInfos);

                    //Breed
                    var offspring = Breed(breeders, allAncestorHashes);

                    //Mutate
                    Mutate(offspring, allAncestorHashes);

                    offspring.AddRange(breeders); //50% Elitism.
                    offspring.ForEach(s => allAncestorHashes.Add(s));

                    gen = new Generation { GenerationAge = i + 1, PlayerInfos = offspring };

                    //GetFitness
                    gen.PlayerInfos.ForEach(s => GetFitness(s, playersToTrain, playersToTest));
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }

            //Display results
            allGenerations.SelectMany(s => s.PlayerInfos).Distinct().OrderByDescending(s => s.TotalPredictionDifference).ToList().ForEach(s =>
            {
                Console.WriteLine();
                Console.WriteLine($"{s.Id}: {s.TotalPredictionDifference}");
                s.Fields.ForEach(z => Console.Write(z.Name + " - "));
            });

            //Return best
            return allGenerations.SelectMany(s => s.PlayerInfos).OrderBy(s => s.TotalPredictionDifference).First();
        }

        private void LogGeneration(Generation gen)
        {
            gen.PlayerInfos.OrderBy(s => s.TotalPredictionDifference).ToList().ForEach(s =>
            {
                Console.WriteLine();
                Console.WriteLine($"{s.Id}: {s.TotalPredictionDifference}");
                s.Fields.ForEach(z => Console.Write(z.Name + " - "));
            });
        }

        private void Mutate(PlayerInfo playerInfo, Random rand)
        {
            var unrepresentedGenes = typeof(Player).GetFields().Where(s => !playerInfo.Fields.Any(z => z.Name == s.Name) && s.Name != "ActualPoints").ToList();
            var unrepCount = unrepresentedGenes.Count;
            List<FieldInfo> fieldsToAdd = new List<FieldInfo>();
            for (var i = 0; i < unrepCount; i++)
            {
                if (rand.Next(1, 100) <= 5)
                {
                    fieldsToAdd.Add(unrepresentedGenes[i]);
                }
            }

            var fieldCount = playerInfo.Fields.Count;
            List<FieldInfo> fieldsToRemove = new List<FieldInfo>();
            for (var i = 0; i < fieldCount; i++)
            {
                if (playerInfo.Fields[i].Name != "ActualPoints" && rand.Next(1, 100) <= 5)
                {
                    fieldsToRemove.Add(playerInfo.Fields[i]);
                }
            }

            if (fieldsToAdd.Any())
            {
                playerInfo.Fields.InsertRange(0, fieldsToAdd);
                playerInfo.TotalPredictionDifference = 0;
            }

            if (fieldsToRemove.Any())
            {
                fieldsToRemove.ForEach(s => playerInfo.Fields.Remove(s));
                playerInfo.TotalPredictionDifference = 0;

                if (!playerInfo.Fields.Any(s => s.Name != "ActualPoints"))
                {
                    playerInfo.Fields.InsertRange(0, GetRandomProperties(typeof(Player), new Random()));
                }
            }
        }

        private void Mutate(List<PlayerInfo> playerInfos, HashSet<PlayerInfo> allAncestorHashes)
        {
            var rand = new Random();

            foreach (var playerInfo in playerInfos)
            {
                do
                {
                    Mutate(playerInfo, rand);
                }
                while (allAncestorHashes.Contains(playerInfo));
            }
        }

        private List<PlayerInfo> Breed(List<PlayerInfo> playerInfos, HashSet<PlayerInfo> allAncestorHashes)
        {
            List<PlayerInfo> offspring = new List<PlayerInfo>();
            for(var i = 0; i < playerInfos.Count; i++)
            {
                PlayerInfo result = null;
                int z = 0;
                do
                {
                    var ind1 = i % (playerInfos.Count - 1);
                    var ind2 = (i + 1 + z) % (playerInfos.Count - 1);
                    result = Breed(playerInfos[ind1], playerInfos[ind2], i);
                    z++;
                }
                while (allAncestorHashes.Contains(result));

                offspring.Add(result);
            }
            return offspring;
        }

        private PlayerInfo Breed(PlayerInfo parent1, PlayerInfo parent2, int newId)
        {
            var rand = new Random();
            var minSize = Math.Min(parent1.Fields.Count, parent2.Fields.Count);
            var crossoverPoint = rand.Next(1, minSize);
            var newFields = parent1.Fields.GetRange(0, crossoverPoint);
            newFields.AddRange(parent2.Fields.GetRange(crossoverPoint, parent2.Fields.Count - crossoverPoint));
            var actPoints = newFields.Find(a => a.Name == "ActualPoints");
            newFields.RemoveAll(s => s.Name == "ActualPoints");
            newFields.Add(actPoints);
            return new PlayerInfo { Id = newId, Fields = newFields.Distinct().ToList() };
        }

        private List<PlayerInfo> Select(List<PlayerInfo> playerInfos)
        {
            return playerInfos.OrderBy(s => s.TotalPredictionDifference).Take(playerInfos.Count / 2).ToList();
        }

        private void GetFitness(PlayerInfo playerInfo, List<PlayerDto> playersToTrain, List<PlayerDto> playersToTest)
        {
            if(playerInfo.TotalPredictionDifference != 0)
                return;

            var populationToTrain = TypeService.CloneObjectsToNewObjectType(playersToTrain.Select(Player.From).ToList(), playerInfo.Fields);

            playerInfo.PredictionModel = _predictionService.Train<Player, PointPrediction>(populationToTrain.Select(s => (Player)s).ToList(), "ActualPoints");

            var testPlayers = playersToTest.Select(Player.From).ToList();

            _predictionService.Predict(playerInfo.PredictionModel, testPlayers);

            Console.WriteLine();
            playerInfo.Fields.ForEach(s => Console.Write(s.Name + ", "));
            //testPlayers.ForEach(s => Console.WriteLine("Pred: " + s.Pred + ". Proj: " + s.Projection + ". Pred Diff: " + Math.Abs(s.Pred - s.ActualPoints) + ". Proj Diff: " + Math.Abs(s.Projection - s.ActualPoints)));
            playerInfo.TotalPredictionDifference = testPlayers.Sum(s => Math.Abs(s.Pred - s.ActualPoints));
            Console.WriteLine(playerInfo.TotalPredictionDifference);
        }

        public Generation CreateRandomGeneration(int populationSize)
        {
            var generation = new Generation { GenerationAge = 1 };
            var rand = new Random();

            for(int i = 0; i < populationSize; i++)
            {
                var props = GetRandomProperties(typeof(Player), rand).Where(s => s.Name != "ActualPoints").ToList();
                props.Add(typeof(Player).GetField("ActualPoints"));
                generation.PlayerInfos.Add(new PlayerInfo { Id = i, Fields = props });
            }

            return generation;
        }

        private List<FieldInfo> GetRandomProperties(Type type, Random rand)
        {
            return type
                .GetFields()
                .Where(s => rand.Next(0, 2) == 1)
                .ToList();
        }
    }
}
