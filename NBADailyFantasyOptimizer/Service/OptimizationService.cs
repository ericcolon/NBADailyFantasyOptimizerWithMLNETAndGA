using NBADailyFantasyOptimizer.DataAccess;
using NBADailyFantasyOptimizer.DataTransfer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBADailyFantasyOptimizer.Service
{
    public class OptimizationService
    {
        private readonly PlayerDao _playerDao;
        private const int PlayersPerRoster = 8;
        private const int MaxSimilarPlayersBetweenRosters = PlayersPerRoster - 3;
        private const int MaxSimilarPlayersAmongThreeRosters = PlayersPerRoster - 6;

        public OptimizationService()
        {
            _playerDao = new PlayerDao();
        }

        public List<RosterDto> Optimize(int day, int numRostersWanted, double percToRemove, double minuteCutoff, int projDivision, int projToRemove, int playerMinMinutesPrevGame, Dictionary<int, List<double>> allTotals, List<PlayerDto> playersToUse = null)
        {
            var numRostersWithSamePlayer = numRostersWanted / 2;
            List<PlayerDto> players = null;
            if (playersToUse != null)
            {
                players = playersToUse;
            }
            else
            {
                players = _playerDao.GetAllPlayers(day);
                //if (players.Select(s => s.Team).Distinct().Count() < 12)
                //    return new List<RosterDto>();
                List<PlayerDto> previousPlayers = Enumerable.Range(day - 4, 4).Select(d => _playerDao.GetAllPlayers(d)).SelectMany(s => s).ToList();


                //var fdsa = previousPlayers.Where(s => s.ActualPoints > 0 && s.Projection > 0).GroupBy(s => new { s.Position, sal = s.Salary / 10 }).OrderBy(d => d.Key.Position).ThenBy(x => x.Key.sal).Select(s => s.Key.Position + "\t" + (s.Key.sal * 10) + "\t" + Math.Round(s.Average(x => x.Projection - x.ActualPoints), 2) + "\t" + s.Count());
                //var dd = "";
                //fdsa.ToList().ForEach(s => dd += "\n" + s);


                UpdateProjection(players, previousPlayers, minuteCutoff, projDivision, playerMinMinutesPrevGame);
            }
            //players.ForEach(z => z.Projection = z.ActualPoints);
            FilterUnneededPlayers(players);

            if (!players.Any())
                return new List<RosterDto>();

            players.ForEach(p => p.TopPlayerForTeam = !players.Any(s => s.Name != p.Name && s.Team == p.Team && s.Projection > p.Projection));

            var pointGuards = players.Where(p => p.Position == "PG").OrderByDescending(p => p.Roi).ToList();
            var shootingGuards = players.Where(p => p.Position == "SG").OrderByDescending(p => p.Roi).ToList();
            var smallForwards = players.Where(p => p.Position == "SF").OrderByDescending(p => p.Roi).ToList();
            var powerForwards = players.Where(p => p.Position == "PF").OrderByDescending(p => p.Roi).ToList();
            var centers = players.Where(p => p.Position == "C").OrderByDescending(p => p.Roi).ToList();

            var guards = pointGuards.Concat(shootingGuards).OrderByDescending(p => p.Roi).ToList();
            var forwards = smallForwards.Concat(powerForwards).OrderByDescending(p => p.Roi).ToList();
            var utilities = players.OrderByDescending(p => p.Roi).ToList();


            var allGuards = (from pg in pointGuards
                             from sg in shootingGuards
                             from g in guards
                             where (g.Position == "PG" && g.Id < pg.Id)
                                    || (g.Position == "SG" && g.Id < sg.Id) 
                             select new
                             {
                                 pg = pg,
                                 sg = sg,
                                 g = g,
                                 proj = pg.Projection + sg.Projection + g.Projection,
                                 sal = pg.Salary + sg.Salary + g.Salary
                             }).GroupBy(s => s.sal).SelectMany(s => s.OrderByDescending(x => x.proj).Take(200)).OrderByDescending(s => s.sal).ToArray();
            var allForwards = (from pf in powerForwards
                             from sf in smallForwards
                               from f in forwards
                               where (f.Position == "PF" && f.Id < pf.Id)
                                      || (f.Position == "SF" && f.Id < sf.Id) 

                                      
                             select new RosterDto
                             {
                                 PowerForward = pf,
                                 SmallForward = sf,
                                 Forward = f,
                                 proj = pf.Projection + sf.Projection + f.Projection,
                                 sal = pf.Salary + sf.Salary + f.Salary
                             }).GroupBy(s => s.sal).SelectMany(s => s.OrderByDescending(x => x.proj).Take(200)).OrderByDescending(s => s.sal).ToArray();
            var allCentersUtils = (from c in centers
                             from u in utilities
                             where u.Position != "C" || (u.Position == "C" && u.Id < c.Id)
                             select new
                             {
                                 c = c,
                                 u = u,
                                 proj = c.Projection + u.Projection,
                                 sal = c.Salary + u.Salary
                             }).GroupBy(s => s.sal).SelectMany(s => s.OrderByDescending(x => x.proj).Take(200)).OrderByDescending(s => s.sal).ToArray();

            var topRosters = new List<RosterDto>(); 
            var fullyUsedPlayers = new HashSet<PlayerDto>();
            var firstRound = true;
            var dict = new ConcurrentDictionary<int, RosterDto[]>();

            var watch = new Stopwatch();
            watch.Start();
            foreach (var num in Enumerable.Range(1, numRostersWanted))
            {
                var tempRosters = new ConcurrentBag<RosterDto>();

                Parallel.ForEach(allCentersUtils, new ParallelOptions { MaxDegreeOfParallelism = 1 }, (cus) =>
                {
                    var sal = 0;
                    double topProj = 200;
                    RosterDto topRoster = new RosterDto();
                    RosterDto newRoster = new RosterDto();
                    RosterDto[] fws;
                    RosterDto fs;
                    int fwsId = 0;

                    foreach (var gs in allGuards)
                    {
                        if (gs.pg.Id == cus.u.Id || gs.sg.Id == cus.u.Id || gs.g.Id == cus.u.Id)
                            continue;
                        //if (num == 5 && ( gs.pg.Name != "RUSSELL WESTBROOK" || gs.sg.Name != "JAMES HARDEN"))
                        //    continue;

                        sal = cus.sal + gs.sal;

                        if (!dict.TryGetValue(sal, out fws))
                        {
                            fws = GetThisManyFws(allForwards, sal, fullyUsedPlayers, num);
                            dict.AddOrUpdate(sal, fws, (a, b) => { return b; });
                        }

                        for (fwsId = 0; fwsId < fws.Length; fwsId++)
                        {
                            fs = fws[fwsId];

                            //if (num == 1 && fs.PowerForward.Name != "GORDON HAYWARD")
                            //    continue;
                            //if (num == 2 && (fs.PowerForward.Name != "GORDON HAYWARD" || cus.c.Name != "RUDY GOBERT"))
                            //    continue;
                            //if (num == 3 && (fs.PowerForward.Name != "GORDON HAYWARD" || cus.c.Name != "RUDY GOBERT" || fs.SmallForward.Name != "KEVIN DURANT"))
                            //    continue;
                            //if (num == 4 && gs.pg.Name != "STEPHEN CURRY")
                            //    continue;
                            //if (num == 5 && (fs.Forward.Name != "KEVIN DURANT" || gs.pg.Name != "SHELVIN MACK" || gs.sg.Name != "JOE INGLES"))
                            //    continue;
                            //if (num == 6 && (fs.Forward.Name != "KEVIN DURANT" || gs.sg.Name != "RODNEY HOOD"))
                            //    continue;
                            //if (num == 7 && (fs.PowerForward.Name != "GORDON HAYWARD" || cus.c.Name != "RUDY GOBERT" || gs.sg.Name != "RODNEY HOOD" || fs.Forward.Name != "ANDRE IGUODALA"))
                            //    continue;
                            //if (num == 8 && ( cus.c.Name != "RUDY GOBERT"))
                            //    continue;
                            //if (num == 9 && (gs.pg.Name != "STEPHEN CURRY" || gs.sg.Name != "JOE INGLES" || gs.g.Name != "RODNEY HOOD" || fs.Forward.Name != "ANDRE IGUODALA"))
                            //    continue;
                            //if (num == 10 && (fs.PowerForward.Name != "GORDON HAYWARD" || gs.sg.Name != "JOE INGLES"))
                            //    continue;

                            if (fs.PowerForward.Id == cus.u.Id || fs.SmallForward.Id == cus.u.Id || fs.Forward.Id == cus.u.Id)
                                continue;

                            var proj = cus.proj + gs.proj + fs.proj;

                            if (proj < topProj)
                                continue;

                            newRoster = new RosterDto
                            {
                                PointGuard = gs.pg,
                                ShootingGuard = gs.sg,
                                PowerForward = fs.PowerForward,
                                SmallForward = fs.SmallForward,
                                Center = cus.c,
                                Utility = cus.u,
                                Guard = gs.g,
                                Forward = fs.Forward
                            };

                            if (!firstRound)
                            {
                                //if (newRoster.Players.Any(s => fullyUsedPlayers.Contains(s)))
                                //    continue;

                                var aaa = topRosters.Select(s => s.Players.Count(x => RosterDto.PlayerExistsOnRoster(x, newRoster)));
                                if (aaa.Any(x => x > MaxSimilarPlayersBetweenRosters))
                                {
                                    continue;
                                }

                                //if (aaa.Count(x => x > MaxSimilarPlayersAmongThreeRosters) > 2)
                                //{
                                //    continue;
                                //}
                            }

                            //var teamGroups = newRoster.Players.GroupBy(s => s.Team);
                            //proj -= teamGroups.Where(s => s.Count() > 1 && s.Sum(x => x.Projection) > 30 * s.Count()).Sum(s => (s.Sum(x => x.Projection) - (30 * s.Count())) * .1);
                            //proj += teamGroups.Where(s => s.Count() > 1).Sum(teamGroup => teamGroup.Sum(player => player.ActualResultsBeforeDay == null || !player.ActualResultsBeforeDay.Any() ? 0 : player.ActualResultsBeforeDay.Average(actual => actual.Assists)) * (teamGroup.Sum(player => player.Projection) / 500));

                            //proj += newRoster.Players.Where(s => newRoster.Players.Any(f => f.Opponent == s.Team)).Sum(s => s.Projection) * .001;

                            //if (proj < topProj)
                            //    continue;

                            topRoster = newRoster;
                            topProj = proj;
                            topRoster.proj = topProj;
                        }

                    }
                    tempRosters.Add(topRoster);
                });

                var topR = tempRosters.OrderByDescending(s => s.proj).First();
                if (!topR.Players.Any( p => p != null))
                    return topRosters;

                topRosters.Add(topR);

                //foreach (var player in topR.Players)
                //{
                //    if (player != null && player.Projection > 0 && topRosters.SelectMany(s => s.Players).Count(s => s.Equals(player)) >= /*player.Roi **/ (player.Projection / 40)* 2 * numRostersWithSamePlayer)
                //        fullyUsedPlayers.Add(player);
                //}
                firstRound = false;

                if (!allTotals.ContainsKey(topRosters.Count))
                    allTotals.Add(topRosters.Count, new List<double>());
                allTotals[topRosters.Count].Add(topR.TotalActual);

                //Console.WriteLine(num + " - " + watch.Elapsed.ToString());
                //Console.WriteLine("Projected Points: " + topR.proj);
               // if (topR.TotalActual > 0)
               //     Console.WriteLine("Actual Points: " + topR.TotalActual);

               // Console.WriteLine("Total Salary: " + topR.TotalSalary);
               // if (topR.Players.All(s => s != null))
                //    topR.Players.ToList().ForEach(s => Console.WriteLine(s.Name + ", " + s.Position + ", " + s.Team + ", " + s.Opponent + ", " + s.Salary + ", PROJ: " + s.Projection + ", ACT: " + s.ActualPoints + (s.InjuryStatus.Length > 0 ? ", ---" + s.InjuryStatus + "---" : "")));
            }

            return topRosters;
        }

        private void UpdateProjection(List<PlayerDto> players, List<PlayerDto> previousPlayers, double minuteCutoff, int projDivision, int playerMinMinutesPrevGame)
        {
            foreach (var player in players)
            {
                var opTeam = player.Opponent;
                var day = player.Day;
                var position = player.Position;

                var pre = previousPlayers.Where(x => x.Opponent == player.Opponent && x.Position == player.Position && x.Day > player.Day - 10 && x.Day < player.Day && x.ActualPoints > 0 && x.Minutes >= minuteCutoff && x.ActualResultsBeforeDay.OrderByDescending(d => d.Day).Take(5).Any(p => p.Minutes >= minuteCutoff)).ToList();
                var prev5 = pre.Select(x => x.Day).Distinct().OrderByDescending(x => x).Take(5);
                if (prev5 == null || prev5.Count() < 2)
                    continue;

                var startDay = prev5.Min();
                var prev5GamesForPositionVsOp = pre.Where(x => x.Day >= startDay).ToList();

                List<double> positionsPrevious5VsOp = new List<double>();
                foreach (var d in prev5)
                {
                    var ava = prev5GamesForPositionVsOp.Where(x => x.Day == d && x.ActualResultsBeforeDay != null && x.ActualResultsBeforeDay.Any()).ToList();
                    var av = ava.Select(x => x.ActualPoints / x.ActualResultsBeforeDay.OrderByDescending(c => c.Day).Take(5).Where(z => z.Minutes >= minuteCutoff).Average(s => s.ActualPoints));
                    if (!av.Any())
                        continue;
                    positionsPrevious5VsOp.Add(av.Average());
                }

                if (!positionsPrevious5VsOp.Any())
                    continue;

                var percToChange = positionsPrevious5VsOp.Average();
                if (percToChange > 1.5)
                {
                    percToChange = 1.5;
                }
                else if (percToChange < .5)
                {
                    percToChange = .5;
                }

                //player.Projection = Math.Round(player.Projection * ((percToChange + projDivision - 1) / projDivision), 2);
            }

            //players.ForEach(p => p.Projection = p.ActualResultsBeforeDay != null && p.ActualResultsBeforeDay.OrderByDescending(x => x.Day).Take(5).Count(x => x.ActualPoints > p.Projection) > 0 ? p.Projection : 0);

            //players.ForEach(p => p.Projection += (p.ActualResultsBeforeDay != null && p.ActualResultsBeforeDay.Count() > 3) ? p.ActualResultsBeforeDay.OrderByDescending(z => z.Day).Take(3).Count(z => z.ActualPoints > p.Projection) : 0);
            //players.Where(p => p.Salary < 40 && p.ActualResultsBeforeDay != null && p.ActualResultsBeforeDay.Count(a => a.Minutes > 25) > 4).ToList().ForEach(p => p.Projection = p.ActualResultsBeforeDay.Where(z => z.Minutes > 25).OrderByDescending(d => d.Day).Take(5).Count(x => x.ActualPoints > p.Projection) > 2 ? p.Projection : 0);


            //players.ForEach(s => s.Projection = s.ActualResultsBeforeDay != null && s.ActualResultsBeforeDay.Any() && s.ActualResultsBeforeDay.OrderByDescending(x => x.Day).First().Minutes > playerMinMinutesPrevGame ? s.Projection : s.Projection - 5);

            //players.ForEach(x => x.Projection = x.InjuryStatus != "" && previousPlayers.Where(z => z.Name == x.Name && z.Team == x.Team).Any() && previousPlayers.Where(z => z.Name == x.Name && z.Team == x.Team).OrderByDescending(z => z.Day).First().InjuryStatus != "" ? 0 : x.Projection);

        }

        private RosterDto[] GetThisManyFws(RosterDto[] fws, int y, HashSet<PlayerDto> fullyUsedPlayers, int num)
        {
            return fws.Where(s => s.sal + y <= 200 /*&& s.Players.All(x => !fullyUsedPlayers.Contains(x))*/).OrderByDescending(x => x.proj).Take(num * 2 * 1 ).ToArray();
        }

        public static List<PlayerDto> FilterUnneededPlayers(List<PlayerDto> players)
        {
            //players.ToList().ForEach(s => s.Projection = s.ActualResultsBeforeDay != null && s.ActualResultsBeforeDay.Count > 1 ? (s.ActualResultsBeforeDay.Average(f => f.ActualPoints) + s.Projection) / 2 : s.Projection);



            players.RemoveAll(p => p.Projection <= 0);
            players.RemoveAll(p => p.Roi < .75);
            players.RemoveAll(p => p.InjuryStatus != null && ConstantDto.RemoveInjuryStatuses.Contains(p.InjuryStatus));
            players.RemoveAll(p => p.Projection < 1 );


            players = players.Where(s => s.InjuryStatus == null || string.IsNullOrWhiteSpace(s.InjuryStatus)).ToList();
            players = players.Where(s => s.ActualPoints > 0 && s.Projection > 0).ToList();

            var teams = new HashSet<string> { };
            var names = new HashSet<string> {};

            players.RemoveAll(s => names.Any(n => n == s.Name));
            players.RemoveAll(s => teams.Any(t => t == s.Team));

            players.ForEach(p => p.TopPlayerForTeam = !players.Any(s => s.Name != p.Name && s.Team == p.Team && s.Projection > p.Projection));
            players.ForEach(p => p.TopPlayerAtPositionForTeam = !players.Any(s => s.Position != p.Position && s.Name != p.Name && s.Team == p.Team && s.Projection > p.Projection));

            return players;
        }
    }
}
