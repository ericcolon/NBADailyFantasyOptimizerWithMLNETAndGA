using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using MLTestCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ML.Runtime.Api.SchemaDefinition;

namespace MLGeneticAlgorithm.Domain
{
    public class PlayerInfo
    {
        public int Id { get; set; }
        public List<FieldInfo> Fields { get; set; }
        public PredictionModel<Player, PointPrediction> PredictionModel { get; set; }
        public float TotalPredictionDifference { get; set; }

        public override int GetHashCode()
        {
            return String.Join(' ', Fields.Select(s => s.Name).OrderBy(s => s).ToList()).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }
    }
}
