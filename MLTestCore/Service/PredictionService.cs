using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Models;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using MLGeneticAlgorithm.Service;
using MLTestCore.Data;
using MLTestCore.DataCreation;
using NBADailyFantasyOptimizer.DataTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MLTestCore.Service
{
    public interface IPredictionService
    {
        PredictionModel<t, tr> Train<t, tr>(List<t> trainingObjects, string resultColumnName) where t : Player where tr : class, new();
        RegressionMetrics Evaluate(PredictionModel<Player, PointPrediction> model, List<PlayerDto> players);
        void Predict(PredictionModel<Player, PointPrediction> model, List<Player> players);
    }

    public class PredictionService : IPredictionService
    {
        private readonly string _path;
        private readonly string _exportTrainingFileName;
        private readonly string _modelFileName;
        private readonly string _exportTestFileName;

        public PredictionService(string path, string exportTrainingFileName, string modelFileName, string exportTestFileName)
        {
            _path = path;
            _exportTestFileName = exportTestFileName;
            _exportTrainingFileName = exportTrainingFileName;
            _modelFileName = modelFileName;
        }

        public PredictionModel<t, tr> Train<t, tr>(List<t> trainingObjects, string resultColumnName) where t : Player where tr : class, new()
        {
            if (!trainingObjects.Any())
            {
                throw new Exception("Nothing to train!");
            }

            var pipeline = new LearningPipeline();

            pipeline.Add(CollectionDataSource.Create(trainingObjects));

            pipeline.Add(new ColumnCopier((resultColumnName, "Label")));

            var fieldsToConvert = FastDeepCloner.DeepCloner.GetFastDeepClonerFields(trainingObjects.First().GetType()).Where(s => s.PropertyType != typeof(Single) && s.ContainAttribute(typeof(ColumnAttribute)));// trainingObjects.First().GetType().GetFields().Where(s => s.FieldType != typeof(Single));
            if (fieldsToConvert.Any())
            {
                Console.Write("Transforming non-float fields: ");
                fieldsToConvert.ToList().ForEach(z => Console.Write( " | " + z.Name + " " + z.PropertyType));
                var listToConvert = fieldsToConvert.Select(s => s.Name).ToArray();
                pipeline.Add(new CategoricalOneHotVectorizer(listToConvert));
            }

            var fieldsToUse = FastDeepCloner.DeepCloner.GetFastDeepClonerFields(trainingObjects.First().GetType())
                .Where(z => z.ContainAttribute(typeof(ColumnAttribute)) && z.Name != resultColumnName)
                .Select(s => s.Name)
                .ToArray();

            Console.WriteLine();
            fieldsToUse.ToList().ForEach(s => Console.Write(s + " - "));

            pipeline.Add(new ColumnConcatenator("Features", fieldsToUse));

            pipeline.Add(new FastTreeRegressor());

            //await model.WriteAsync(path + modelFileName);
            try
            {
                return pipeline.Train<t, tr>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException.Message);
                Console.WriteLine(e.InnerException.StackTrace);
                throw e;
            }
        }

        public RegressionMetrics Evaluate(PredictionModel<Player, PointPrediction> model, List<PlayerDto> players)
        {
            CsvExporter.Export(_path + _exportTrainingFileName, players);
            var testData = new TextLoader(_path + _exportTestFileName).CreateFrom<Player>(false, ',');

            var evaluator = new RegressionEvaluator();
            return evaluator.Evaluate(model, testData);
        }

        public void Predict(PredictionModel<Player, PointPrediction> model, List<Player> players)
        {
            var predictions = model.Predict(players).ToList();

            for(int i = 0; i < players.Count; i++)
            {
                players[i].Prediction = predictions[i];
            }
        }
    }
}
