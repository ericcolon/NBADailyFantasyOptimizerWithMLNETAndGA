using NBADailyFantasyOptimizer.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBADailyFantasyOptimizer.DataTransfer
{
    public class ProjectionDto
    {
        public string Name { get; set; }
        public string Team { get; set; }
        public string Position { get; set; }
        public double Minutes { get; set; }
        public double FieldGoals { get; set; }
        public double ThreePointers { get; set; }
        public double Rebounds { get; set; }
        public double Assists { get; set; }
        public double Steals { get; set; }
        public double Blocks { get; set; }
        public double Turnovers { get; set; }
        public double Points { get; set; }
        public double Projection { get; set; }
        public double ProjectionRg { get; set; }
        public int Day { get; set; }
        public int OpponentRanking { get; set; }
        public double Floor { get; set; }
        public double Ceiling { get; set; }
        public double AverageMinutes { get; set; }
        public int Consistency { get; set; }
        public double Last5Average { get; set; }
        public double SeasonAverage { get; set; }
        public double SeasonFloor { get; set; }
        public double SeasonCeiling { get; set; }
        public double OtherProj { get; set; }

        public double Dvp { get; set; }

        public void ComputeProjection()
        {
            Projection = Math.Round(ThreePointers * ConstantDto.ThreePointPoints
               + Points * ConstantDto.PointPoints
               + Rebounds * ConstantDto.ReboundPoints
               + Assists * ConstantDto.AssistPoints
               + Steals * ConstantDto.StealPoints
               + Blocks * ConstantDto.BlockPoints
               + Turnovers * ConstantDto.TurnoverPoints, 2);
        }
    }
}
