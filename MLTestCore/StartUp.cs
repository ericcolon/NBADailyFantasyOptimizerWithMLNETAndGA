using Microsoft.Extensions.DependencyInjection;
using MLTestCore.Service;
using NBADailyFantasyOptimizer.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLTestCore
{
    public static class StartUp
    {
        private const string _path = @"C:\Users\Jake\Desktop\NBADailyFantasyOptimizer\Inputsj\";
        private const string _exportTrainingFileName = "Export.csv";
        private const string _exportTestFileName = "ExportTest.csv";
        private const string _modelFileName = "PredictionModel.zip";

        public static IServiceProvider ConfigureService(IServiceCollection services)
        {
            services.AddTransient<IPredictionService>((s) => new PredictionService(_path, _exportTrainingFileName, _modelFileName, _exportTestFileName));
            services.AddTransient<IPlayerDao, PlayerDao>();

            return services.BuildServiceProvider();
        }
    }
}
