using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBADailyFantasyOptimizer.DataTransfer
{
    public class RosterDto
    {
        public HashSet<PlayerDto> Players { get { return new HashSet<PlayerDto> { PointGuard, ShootingGuard, Guard, SmallForward, PowerForward, Forward, Center, Utility }; } }
        public PlayerDto PointGuard { get; set; }
        public PlayerDto ShootingGuard { get; set; }
        public PlayerDto Guard { get; set; }
        public PlayerDto SmallForward { get; set; }
        public PlayerDto PowerForward { get; set; }
        public PlayerDto Forward { get; set; }
        public PlayerDto Center { get; set; }
        public PlayerDto Utility { get; set; }
        
        public double TotalProjection { get { return Players.Sum(s => s == null ? 0 : s.Projection); } }
        public double TotalActual { get { return Players.Sum(s => s == null ? 0 : s.ActualPoints); } }
        public int TotalSalary { get { return Players.Sum(s => s == null ? 0 : s.Salary); } }

        public int sal { get; set; }
        public double proj { get; set; }

        public static bool PlayerExistsOnRoster(PlayerDto player, RosterDto roster)
        {
            if (player == null)
                return false;
            return roster.Players.Contains(player);
        }

        public RosterDto CloneRoster()
        {
            return new RosterDto
            {
                PointGuard = PointGuard,
                ShootingGuard = ShootingGuard,
                Guard = Guard,
                SmallForward = SmallForward,
                PowerForward = PowerForward,
                Forward = Forward,
                Center = Center,
                Utility = Utility
            };
        }
    }
}
