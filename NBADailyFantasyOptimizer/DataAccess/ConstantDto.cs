using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBADailyFantasyOptimizer.DataAccess
{
    public class ConstantDto
    {
        public static double ThreePointPoints = .5;
        public static double PointPoints = 1;
        public static double ReboundPoints = 1.2;
        public static double AssistPoints = 1.5;
        public static double StealPoints = 2;
        public static double BlockPoints = 2;
        public static double TurnoverPoints = -1;

        public static string DFNFileName = "";
        public static string SaberFileName = "-SaberSim";
        public static string NFFIleName = "-NumberFire";

        public static List<string> RemovableStrings = new List<string> { ".", " JR", " SR", "'", "-", "III", "II"/*, " I"*/ };
        public static List<string> RemoveInjuryStatuses = new List<string> { "O", "INJ", "GTD"};

        public static List<Tuple<string, string>> TeamNames = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("HOU", "ROCKETS"),
            new Tuple<string, string>("BOS", "CELTICS"),
            new Tuple<string, string>("PHI", "76ERS"),
            new Tuple<string, string>("MIL", "BUCKS"),
            new Tuple<string, string>("MIA", "HEAT"),
            new Tuple<string, string>("BKN", "NETS"),
            new Tuple<string, string>("ATL", "HAWKS"),
            new Tuple<string, string>("CHI", "BULLS"),
            new Tuple<string, string>("GS", "WARRIORS"),
            new Tuple<string, string>("CHA", "HORNETS"),
            new Tuple<string, string>("TOR", "RAPTORS"),
            new Tuple<string, string>("MEM", "GRIZZLIES"),
            new Tuple<string, string>("SAC", "KINGS"),
            new Tuple<string, string>("CLE", "CAVALIERS"),
            new Tuple<string, string>("NY", "KNICKS"),
            new Tuple<string, string>("DAL", "MAVERICKS"),
            new Tuple<string, string>("OKC", "THUNDER"),
            new Tuple<string, string>("NO", "PELICANS"),
            new Tuple<string, string>("LAL", "LAKERS"),
            new Tuple<string, string>("POR", "TRAIL BLAZERS"),
            new Tuple<string, string>("SA", "SPURS"),
            new Tuple<string, string>("PHO", "SUNS"),
            new Tuple<string, string>("DEN", "NUGGETS"),
            new Tuple<string, string>("IND", "PACERS"),
            new Tuple<string, string>("MIN", "TIMBERWOLVES"),
            new Tuple<string, string>("UTA", "JAZZ"),            
            new Tuple<string, string>("DET", "PISTONS"),
            new Tuple<string, string>("WAS", "WIZARDS"),
            new Tuple<string, string>("ORL", "MAGIC"),
            new Tuple<string, string>("LAC", "CLIPPERS"),
        };

        public static int GetNumPositionsByPosition(string position)
        {
            switch (position)
            {
                case "PG":
                    return 3;
                case "SG":
                    return 3;
                case "SF":
                    return 3;
                case "PF":
                    return 3;
                case "C":
                    return 2;
                default:
                    return 0;
            }
        }

        public static string GetProjToUseBasedOnMostAccurateForGroup(string position, double avgProj)
        {
            if (position == "PG")
            {
                if (avgProj < 20)
                    return DFNFileName;
                if (avgProj < 30)
                    return NFFIleName;
                if (avgProj < 40)
                    return NFFIleName;
                if (avgProj < 50)
                    return SaberFileName;
                return NFFIleName;
            }
            if (position == "SG")
            {
                if (avgProj < 20)
                    return DFNFileName;
                if (avgProj < 30)
                    return DFNFileName;
                if (avgProj < 40)
                    return NFFIleName;
                if (avgProj < 50)
                    return NFFIleName;
                return SaberFileName;
            }
            if (position == "SF")
            {
                if (avgProj < 20)
                    return DFNFileName;
                if (avgProj < 30)
                    return NFFIleName;
                if (avgProj < 40)
                    return NFFIleName;
                if (avgProj < 50)
                    return SaberFileName;
                return SaberFileName;
            }
            if (position == "PF")
            {
                if (avgProj < 20)
                    return DFNFileName;
                if (avgProj < 30)
                    return DFNFileName;
                if (avgProj < 40)
                    return NFFIleName;
                if (avgProj < 50)
                    return DFNFileName;
                return DFNFileName;
            }
            if (position == "C")
            {
                if (avgProj < 20)
                    return NFFIleName;
                if (avgProj < 30)
                    return SaberFileName;
                if (avgProj < 40)
                    return DFNFileName;
                if (avgProj < 50)
                    return DFNFileName;
                return DFNFileName;
            }
            return DFNFileName;
        }
    }
}
