using NBADailyFantasyOptimizer.DataTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBADailyFantasyOptimizer.DataAccess
{
    public interface IPlayerDao
    {
        List<PlayerDto> GetAllPlayers(int day);
    }

    public class PlayerDao : IPlayerDao
    {
        public List<PlayerDto> GetAllPlayers(int day)
        {
            var players = GetAllPlayerSalaries(day).ToList();

            var projections = GetAllPlayerProjections(day).ToList();
            var previousProjections = new List<ProjectionDto>();
            Enumerable.Range(day - 15, 16).ToList().ForEach(s => previousProjections.AddRange(GetAllPlayerProjections(s))); /*Just current day for ML. If no ML than uncomment*/
            //previousProjections.AddRange(GetAllPlayerProjections(day, new List<PlayerDto>()));

            List<ActualResultsDto> actuals = new List<ActualResultsDto>();
            Enumerable.Range(day - 6, 7).ToList().ForEach(s => actuals.AddRange(GetAllPlayerActuals(s))); /*Just current day for ML. If no ML than uncomment*/
            //actuals.AddRange(GetAllPlayerActuals(day));

            foreach (var p in players)
            {
                var proj = projections.FirstOrDefault(pro => pro.Name == p.Name);
                var prevProjs = previousProjections.Where(pre => pre.Name == p.Name).ToList();

                if (proj != null)
                {
                    p.AddProjection(proj);
                }

                if (prevProjs.Any())
                {
                    p.AddPreviousProjection(prevProjs);
                }

            }

            if (actuals.Any())
            {
                var fff = players.Where(s => actuals.Count(a => a.Day == s.Day && a.Team == s.Team && a.Name.Substring(0, 1) == s.Name.Substring(0, 1) && s.Name.Contains(a.Name.Substring(a.Name.IndexOf(" ")))) > 1).ToList();
                //if (players.Any(s => s.Projection > 20 && actuals.Count(a => a.Day == s.Day && a.Team == s.Team && a.Name.Substring(0, 1) == s.Name.Substring(0, 1) && s.Name.Contains(a.Name.Substring(a.Name.IndexOf(" ")))) > 1))
                //    throw new Exception("Found more than 1 actual for a player!");

                foreach (var player in players)
                {
                    var acts = actuals.Where(a => a.Team == player.Team && a.Name.Substring(0, 1) == player.Name.Substring(0, 1) && player.Name.Contains(a.Name.Substring(a.Name.IndexOf(" ")))).ToList();
                    if (player.Projection > 15 && !acts.Any())
                    {
                        acts = actuals.Where(a => a.Name.Substring(0, 1) == player.Name.Substring(0, 1) && player.Name.Contains(a.Name.Substring(a.Name.IndexOf(" ")))).ToList();
                        try
                        {
                            throw new Exception("No actuals and proj > 15");
                        }
                        catch (Exception e)
                        {
                            var ffd = 8;
                        }
                    }

                    acts.ForEach(a => player.AddActuals(a, day == a.Day));
                }
            }

            return players;
        }

        public IEnumerable<PlayerDto> GetAllPlayerSalaries(int day)
        {
            var filePath = string.Format(@"C:\Users\Jake\Desktop\NBADailyFantasyOptimizer\Inputsj\NBASalariesDay{0}.csv", day); 
            if (!System.IO.File.Exists(filePath))
                return new List<PlayerDto>();
            var lines = System.IO.File.ReadAllLines(filePath).ToList();
            List<PlayerDto> players = new List<PlayerDto>();

            foreach (var line in lines)
            {
                var vals = line.Split(',');
                if (vals[0] == "Id")
                    continue;

                var player = new PlayerDto
                {
                    Id = int.Parse(ParseString(vals[0].Replace("nba.p.", ""))),
                    Name = ParseString(vals[1] + " " + vals[2]),
                    Position = ParseString(vals[3]),
                    Team = ParseString(vals[4]),
                    Opponent = ParseString(vals[5]),
                    Salary = int.Parse(vals[8]),
                    IsHome = ParseString(vals[6]).Split('@')[1] == ParseString(vals[4]),
                    FantasyPointsPerGame = double.Parse(vals[9].Trim()),
                    InjuryStatus = ParseString(vals[10]),
                    Day = day
                };
                players.Add(player);
            }
            return players;
        }

        public IEnumerable<ProjectionDto> GetDFNProjections(int day)
        {
            var filePath = string.Format(@"C:\Users\Jake\Desktop\NBADailyFantasyOptimizer\Inputsj\NBAProjectionsDay{0}.csv", day);
            if (!System.IO.File.Exists(filePath))
                return new List<ProjectionDto>();
            var lines = System.IO.File.ReadAllLines(filePath).ToList();
            List<ProjectionDto> dfnProjs = new List<ProjectionDto>();

            foreach (var line in lines)
            {
                var vals = line.Split(',');
                if (vals[0] == "Id")
                    continue;

                var projection = new ProjectionDto
                {
                    Name = ParseString(vals[0]),
                    Projection = double.Parse(vals[5]),
                    OtherProj = double.Parse(vals[6]),
                    Last5Average = double.Parse(vals[1]),
                    SeasonAverage = double.Parse(vals[2]),
                    SeasonCeiling = double.Parse(vals[4]),
                    SeasonFloor = double.Parse(vals[3]),
                    Day = day
                };

                dfnProjs.RemoveAll(s => s.Name == projection.Name);
                dfnProjs.Add(projection);
            }

            return dfnProjs;
        }

        public IEnumerable<ProjectionDto> GetRGProjections(int day)
        {
            var RGProjs = new List<ProjectionDto>();
            var filePath5 = string.Format(@"C:\Users\Jake\Desktop\NBADailyFantasyOptimizer\Inputsj\NBAProjectionsDay{0}-rg.csv", day);
            if (System.IO.File.Exists(filePath5))
            {
                foreach (var line in System.IO.File.ReadAllLines(filePath5).ToList())
                {
                    var vals = line.Split(',');
                    var projection = new ProjectionDto
                    {
                        Name = ParseString(vals[0]),
                        Projection = double.Parse(vals[1].Trim()),
                        Position = vals.Length > 3 ? ParseString(vals[3]) : null,
                        Dvp = vals.Length > 3 ? double.Parse(vals[5].Trim()) : 1,
                        Minutes = vals.Length > 13 ? double.Parse(vals[14].Trim()) : 0,
                        Day = day
                    };
                    double temp;
                    if (vals.Length > 3 && double.TryParse(vals[15].Trim(), out temp))
                    {
                        projection.Ceiling = temp;
                    }

                    RGProjs.RemoveAll(s => s.Name == projection.Name);

                    RGProjs.Add(projection);
                }
            }

            return RGProjs;
        }

        public IEnumerable<ProjectionDto> GetAllPlayerProjections(int day)
        {
            List<ProjectionDto> dfnProjs = GetDFNProjections(day).ToList();
            List<ProjectionDto> RGProjs = GetRGProjections(day).ToList();

            var numberFireProjs = new List<ProjectionDto>();
            var filePath2 = string.Format(@"C:\Users\Jake\Desktop\NBADailyFantasyOptimizer\Inputsj\NBAProjectionsDay{0}-NumberFire.csv", day);
            if (System.IO.File.Exists(filePath2))
            {
                var lines2 = System.IO.File.ReadAllLines(filePath2).ToList();

                foreach (var line in lines2)
                {
                    var vals = line.Split(',');
                    if (vals[0] == "Id")
                        continue;

                    var projection = new ProjectionDto
                    {
                        Name = ParseString(vals[0]),
                        Projection = double.Parse(vals[1].Trim()),
                        Day = day,
                    };
                    numberFireProjs.Add(projection);
                }
            }

            var saberProjs = new List<ProjectionDto>();
            var filePath3 = string.Format(@"C:\Users\Jake\Desktop\NBADailyFantasyOptimizer\Inputsj\NBAProjectionsDay{0}-SaberSim.csv", day);
            if (System.IO.File.Exists(filePath3))
            {
                var lines2 = System.IO.File.ReadAllLines(filePath3).ToList();

                foreach (var line in lines2)
                {
                    var vals = line.Split(',');
                    var projection = new ProjectionDto
                    {
                        Name = ParseString(vals[0]),
                        Team = ParseString(vals[1]),
                        Position = ParseString(vals[2]),
                        Minutes = double.Parse(vals[3].Trim()),
                        FieldGoals = double.Parse(vals[4].Trim()),
                        ThreePointers = double.Parse(vals[5].Trim()),
                        Rebounds = double.Parse(vals[6].Trim()),
                        Assists = double.Parse(vals[7].Trim()),
                        Steals = double.Parse(vals[8].Trim()),
                        Blocks = double.Parse(vals[9].Trim()),
                        Turnovers = double.Parse(vals[10].Trim()),
                        Points = double.Parse(vals[11].Trim()),
                        Day = day
                    };

                    projection.ComputeProjection();
                    saberProjs.Add(projection);
                }
            }

            var fcProjs = new List<ProjectionDto>();
            var filePath4 = string.Format(@"C:\Users\Jake\Desktop\NBADailyFantasyOptimizer\Inputsj\NBAProjectionsDay{0}-FC.csv", day);
            if (System.IO.File.Exists(filePath4))
            {
                var lines2 = System.IO.File.ReadAllLines(filePath4).ToList();

                foreach (var line in lines2)
                {
                    var vals = line.Split(',');
                    var projection = new ProjectionDto
                    {
                        Name = ParseString(vals[0]),
                        Team = ParseString(vals[1]),
                        Position = ParseString(vals[2]),
                        OpponentRanking = int.Parse(ParseString(vals[3]).Replace("TH", "").Replace("ND", "").Replace("ST", "").Replace("RD", "")),
                        Floor = double.Parse(vals[4].Trim()),
                        Ceiling = double.Parse(vals[5].Trim()),
                        AverageMinutes = vals[6] != null && vals[6].Length > 0 ? double.Parse(vals[6].Trim()) : 0,
                        Minutes = double.Parse(vals[7].Trim()),
                        Projection = double.Parse(vals[8].Trim()),
                        Consistency = int.Parse(vals[9].Trim()),
                        Day = day
                    };

                    fcProjs.Add(projection);
                }
            }

            foreach (var proj in dfnProjs)
            {
                List<ProjectionDto> all = new List<ProjectionDto>();

                List<ProjectionDto> bads = new List<ProjectionDto>();

                var saberMatch = saberProjs.FirstOrDefault(p => p.Name == proj.Name);
                var nfMatch = numberFireProjs.FirstOrDefault(p => p.Name == proj.Name);
                var fcMatch = fcProjs.FirstOrDefault(p => p.Name == proj.Name);
                var rgMatch = RGProjs.FirstOrDefault(p => p.Name == proj.Name);

                all.Add(proj);

                if (rgMatch != null)
                {
                    all.Add(rgMatch);
                    proj.Dvp = rgMatch.Dvp;
                    proj.Position = rgMatch.Position;
                }

                proj.Minutes = rgMatch?.Minutes ?? 0;
                proj.ProjectionRg = rgMatch?.Projection ?? 0;
                //proj.Projection = all.Any() ? all.Average(x => x.Projection) : proj.Projection;
            }
            //foreach (var proj in RGProjs)
            //{
            //    if (dfnProjs.All(s => s.Name != proj.Name))
            //        dfnProjs.Add(proj);
            //}

            return dfnProjs;
        }

        public IEnumerable<ActualResultsDto> GetAllPlayerActuals(int day)
        {
            var filePath = string.Format(@"C:\Users\Jake\Desktop\NBADailyFantasyOptimizer\Inputsj\NBAActualsDay{0}.csv", day);
            if (!System.IO.File.Exists(filePath))
                return new List<ActualResultsDto>();

            var lines = System.IO.File.ReadAllLines(filePath).ToList();
            List<ActualResultsDto> players = new List<ActualResultsDto>();

            foreach (var line in lines)
            {
                var vals = line.Split(',');
                if (vals[0] == "Id")
                    continue;

                var player = new ActualResultsDto
                {
                    Name = ParseString(vals[0]),
                    Day = day,
                    Position = ParseString(vals[1]),
                    Team = ConstantDto.TeamNames.First(s => s.Item2 == ParseString(vals[2])).Item1,
                    IsHome = bool.Parse(vals[3]),
                    Minutes = int.Parse(vals[4]),
                    FieldGoalsMade = int.Parse(vals[5]),
                    FieldGoalsAttempted = int.Parse(vals[6]),
                    ThreePointersMade = int.Parse(vals[7]),
                    ThreePointersAttempted = int.Parse(vals[8]),
                    FreethrowsMade = int.Parse(vals[9]),
                    FreethrowsAttempted = int.Parse(vals[10]),
                    OffensiveRebounds = int.Parse(vals[11]),
                    DefensiveRebounds = int.Parse(vals[12]),
                    Assists = int.Parse(vals[13]),
                    Steals = int.Parse(vals[14]),
                    Blocks = int.Parse(vals[15]),
                    Turnovers = int.Parse(vals[16]),
                    PersonalFouls = int.Parse(vals[17]),
                    PlusMinus = int.Parse(vals[18]),
                    Points = int.Parse(vals[19]),
                    IsBench = bool.Parse(vals[20])
                };
                players.Add(player);
            }
            return players;
        }

        private string ParseString(string str)
        {
            str = str.Trim().ToUpper();
            ConstantDto.RemovableStrings.ForEach(s => str = str.Replace(s, string.Empty));
            str = str.Replace("GUILLERMO", "WILLY");
            str = str.Replace("MOE ", "MAURICE ");
            str = str.Replace("JUANCHO ", "JUAN ");
            str = str.Replace("JOSEPH ", "JOE ");
            str = str.Replace("ENNIS I", "ENNIS");
            str = str.Replace("KAMINSKY I", "KAMINSKY");
            str = str.Replace("LOU ", "LOUIS ");

            if (str.EndsWith(" IV"))
            {
                str = str.Replace(" IV", string.Empty);
            }

            str = str.Trim();
            return str;
        }
    }
}
