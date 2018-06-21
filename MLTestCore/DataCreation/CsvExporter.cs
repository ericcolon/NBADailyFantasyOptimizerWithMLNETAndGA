using MLTestCore.Data;
using NBADailyFantasyOptimizer.DataTransfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MLTestCore.DataCreation
{
    public static class CsvExporter
    {
        public static void Export(string path, List<PlayerDto> players)
        {
            var sb = new StringBuilder();
            players.ForEach(p => sb.AppendLine(
                p.Id + "," +
                p.Team + "," +
                p.Opponent + "," +
                p.Position + "," +
                p.Projection + "," +
                p.Salary + "," +
                (p.TopPlayerForTeam ? 1 : 0) + "," +
                Player.From(p).LastGameDiff + "," +
                Player.From(p).LastGameProjDiff + "," +
                (p.IsHome ? 1 : 0) + "," +
                p.ActualPoints
                ));

            File.WriteAllText(path, sb.ToString());
        }
    }
}
