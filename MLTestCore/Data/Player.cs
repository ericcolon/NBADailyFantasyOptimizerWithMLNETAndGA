using Microsoft.ML.Runtime.Api;
using NBADailyFantasyOptimizer.DataTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MLTestCore.Data
{
    public class Player
    {
        public float Id;
        public string Team;
        public string Opponent;
        public string Position;
        //[Column(ordinal: "4")]
        public float Projection;
        //[Column(ordinal: "5")]
        public float Salary;
        //[Column(ordinal: "10")]
        public float TopPlayerForTeam;
        //[Column(ordinal: "6")]
        public float LastGameDiff;
        //[Column(ordinal: "11")]
        public float LastGameProjDiff;
        public float IsHome;

        public float Day { get; set; }

        public float LastGameProjection;
        public float LastGameProjDiffRatio;
        //[Column(ordinal: "7")]
        public float FantasyPointsPerGame;
        public float IsBench;
        //[Column(ordinal: "8")]
        public float Roi;
        public float TopPlayerAtPositionForTeam;
        public float LastGameAct;
        public float LastGameMinutes;
        public float LastGameFieldGoalsMade;
        //[Column(ordinal: "9")]
        public float LastGameFieldGoalsAttempted;
        public float LastGameThreePointersMade ;
        public float LastGameThreePointersAttempted ;
        //[Column(ordinal: "0")]
        public float LastGameFreethrowsMade ;
        public float LastGameFreethrowsAttempted ;
        //[Column(ordinal: "1")]
        public float LastGameOffensiveRebounds ;
        public float LastGameDefensiveRebounds ;
        public float LastGameAssists;
        //[Column(ordinal: "12")]
        public float LastGameSteals;
        //[Column(ordinal: "13")]
        public float LastGameBlocks ;
        public float LastGameTurnovers;
        //[Column(ordinal: "14")]
        public float LastGamePersonalFouls ;
        //[Column(ordinal: "2")]
        public float LastGamePlusMinus ;
        //[Column(ordinal: "3")]
        public float LastGamePoints ;
        //public float ProjMinutes;
        //[Column(ordinal: "15")]
        public float ProjectionRg;

        public double Last5Average;
        public double SeasonAverage;
        public double SeasonFloor;
        public double SeasonCeiling;
        public double OtherProj;


        //[Column(ordinal: "16")]
        public float ActualPoints;







        public PointPrediction Prediction { get; set; }
        public float Pred => Prediction?.ActualPoints ?? 0;

        public static Player From(PlayerDto dto)
        {
            var descGames = dto.ActualResultsBeforeDay.OrderByDescending(s => s.Day);
            ActualResultsDto gameBeforeToday = null;
            if(descGames.Count() > 1)
            {
                gameBeforeToday = descGames.ToList()[0];
            }

            var todaysGame = dto.ActualResults.Find(s => s.Day == dto.Day);

            var projection = dto.Projections.Find(p => p.Day == dto.Day);

            return new Player
            {
                Day = dto.Day,
                Id = dto.Id,
                Team = dto.Team,
                Opponent = dto.Opponent,
                Position = dto.Position,
                Projection = (float)dto.Projection,
                Salary = dto.Salary,
                TopPlayerForTeam = dto.TopPlayerForTeam ? 1 : 0,
                ActualPoints = (float)dto.ActualPoints,
                LastGameDiff = (float)((gameBeforeToday?.ActualPoints - gameBeforeToday?.Projection) ?? 0),
                LastGameProjDiff = (float)((dto.Projection - gameBeforeToday?.Projection) ?? 0),
                LastGameProjDiffRatio = (float)((dto.Projection / gameBeforeToday?.Projection) ?? 0),
                IsHome = dto.IsHome ? 1 : 0,
                FantasyPointsPerGame = (float)dto.FantasyPointsPerGame,
                IsBench = dto.IsBench ? 1 : 0,
                Roi = (float)dto.Roi,
                TopPlayerAtPositionForTeam = dto.TopPlayerAtPositionForTeam ? 1 : 0,
                LastGameAct = (float)(gameBeforeToday?.ActualPoints ?? 0),
                LastGameAssists = (float)(gameBeforeToday?.Assists ?? 0),
                LastGameBlocks = (float)(gameBeforeToday?.Blocks ?? 0),
                LastGameDefensiveRebounds = (float)(gameBeforeToday?.DefensiveRebounds ?? 0),
                LastGameFieldGoalsAttempted = (float)(gameBeforeToday?.FieldGoalsAttempted ?? 0),
                LastGameFieldGoalsMade = (float)(gameBeforeToday?.FieldGoalsMade ?? 0),
                LastGameFreethrowsAttempted = (float)(gameBeforeToday?.FreethrowsAttempted ?? 0),
                LastGameFreethrowsMade = (float)(gameBeforeToday?.FreethrowsMade ?? 0),
                LastGameMinutes = (float)(gameBeforeToday?.Minutes ?? 0),
                LastGameOffensiveRebounds = (float)(gameBeforeToday?.OffensiveRebounds ?? 0),
                LastGamePersonalFouls = (float)(gameBeforeToday?.PersonalFouls ?? 0),
                LastGamePlusMinus = (float)(gameBeforeToday?.PlusMinus ?? 0),
                LastGamePoints = (float)(gameBeforeToday?.Points ?? 0),
                LastGameSteals = (float)(gameBeforeToday?.Steals ?? 0),
                LastGameThreePointersAttempted = (float)(gameBeforeToday?.ThreePointersAttempted ?? 0),
                LastGameThreePointersMade = (float)(gameBeforeToday?.ThreePointersMade ?? 0),
                LastGameTurnovers = (float)(gameBeforeToday?.Turnovers ?? 0),
                LastGameProjection = (float)(gameBeforeToday?.Projection ?? 0),
                //ProjMinutes = (float)(projection?.Minutes ?? 0),
                ProjectionRg = (float)(projection?.ProjectionRg ?? 0),
                Last5Average = (float)(projection?.Last5Average ?? 0),
                SeasonAverage = (float)(projection?.SeasonAverage ?? 0),
                SeasonCeiling = (float)(projection?.SeasonCeiling ?? 0),
                SeasonFloor = (float)(projection?.SeasonFloor ?? 0),
                OtherProj = (float)(projection?.OtherProj ?? 0),
            };
        }
    }
}
