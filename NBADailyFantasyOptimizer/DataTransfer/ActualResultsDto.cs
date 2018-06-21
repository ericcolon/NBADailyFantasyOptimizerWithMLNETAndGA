using NBADailyFantasyOptimizer.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBADailyFantasyOptimizer.DataTransfer
{
    public class ActualResultsDto
    {
        public int Day { get; set; }

        public string Name { get; set; }
        public string Position { get; set; }
        public string Team { get; set; }
        public bool IsHome { get; set; }
        public bool IsBench { get; set; }

        public float Minutes { get; set; }
        public float FieldGoalsMade { get; set; }
        public float FieldGoalsAttempted { get; set; }
        public float ThreePointersMade { get; set; }
        public float ThreePointersAttempted { get; set; }
        public float FreethrowsMade { get; set; }
        public float FreethrowsAttempted { get; set; }
        public float OffensiveRebounds { get; set; }
        public float DefensiveRebounds { get; set; }
        public float Assists { get; set; }
        public float Steals { get; set; }
        public float Blocks { get; set; }
        public float Turnovers { get; set; }
        public float PersonalFouls { get; set; }
        public float PlusMinus { get; set; }
        public float Points { get; set; }

        public double Projection { get; set; }

        public double ActualPoints { get; set; }

        public ActualResultsDto() { }

        public ActualResultsDto(PlayerDto actuals)
        {
            if (actuals == null)
                return;

            Day = actuals.Day;
            Minutes = actuals.Minutes;
            FieldGoalsMade = actuals.FieldGoalsMade;
            FieldGoalsAttempted = actuals.FieldGoalsAttempted;
            ThreePointersMade = actuals.ThreePointersMade;
            ThreePointersAttempted = actuals.ThreePointersAttempted;
            FreethrowsMade = actuals.FreethrowsMade;
            FreethrowsAttempted = actuals.FreethrowsAttempted;
            OffensiveRebounds = actuals.OffensiveRebounds;
            DefensiveRebounds = actuals.DefensiveRebounds;
            Assists = actuals.Assists;
            Steals = actuals.Steals;
            Blocks = actuals.Blocks;
            Turnovers = actuals.Turnovers;
            PersonalFouls = actuals.PersonalFouls;
            PlusMinus = actuals.PlusMinus;
            Points = actuals.Points;

            ComputeActuals();
        }

        public void ComputeActuals()
        {
            ActualPoints = Math.Round(ThreePointersMade * ConstantDto.ThreePointPoints
               + Points * ConstantDto.PointPoints
               + (DefensiveRebounds + OffensiveRebounds) * ConstantDto.ReboundPoints
               + Assists * ConstantDto.AssistPoints
               + Steals * ConstantDto.StealPoints
               + Blocks * ConstantDto.BlockPoints
               + Turnovers * ConstantDto.TurnoverPoints, 2);
        }
    }
}
