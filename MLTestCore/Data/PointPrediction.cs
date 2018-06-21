using Microsoft.ML.Runtime.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLTestCore.Data
{
    public class PointPrediction
    {
        [ColumnName("Score")]
        public float ActualPoints;
    }
}
