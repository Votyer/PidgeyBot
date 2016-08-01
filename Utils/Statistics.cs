using PidgeyBot.Logic;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PidgeyBot.Utils
{
    public class Statistics
    {
        private readonly DateTime _initSessionDateTime;
        
        public int TotalExperience;
        public int TotalItemsRemoved;
        public int TotalPokemons;
        public int TotalPokemonsTransfered;
        public int TotalStardust;

        public DateTime getDateTime()
        {
            return _initSessionDateTime;
        }

        public Statistics(Statistics old)
        {
            if (old != null)
            {
                TotalExperience = old.TotalExperience;
                TotalItemsRemoved = old.TotalItemsRemoved;
                TotalPokemons = old.TotalPokemons;
                TotalPokemonsTransfered = old.TotalPokemonsTransfered;
                TotalStardust = old.TotalStardust;
                TotalExperience = old.TotalExperience;
                _initSessionDateTime = old._initSessionDateTime;
            }
            else
                _initSessionDateTime = DateTime.Now;
        }

        private string FormatRuntime()
        {
            return (DateTime.Now - _initSessionDateTime).ToString(@"dd\.hh\:mm\:ss");
        }

        public string GetCurrentInfo(Inventory inventory)
        {
            var stats = inventory.GetPlayerStats().Result;
            var output = string.Empty;
            var stat = stats.FirstOrDefault();
            if (stat != null)
            {
                var ep = stat.NextLevelXp - stat.PrevLevelXp - (stat.Experience - stat.PrevLevelXp);
                var time = Math.Round(ep / (TotalExperience / GetRuntime()), 2);
                var hours = 0.00;
                var minutes = 0.00;
                if (double.IsInfinity(time) == false && time > 0)
                {
                    time = Convert.ToDouble(TimeSpan.FromHours(time).ToString("h\\.mm"), CultureInfo.InvariantCulture);
                    hours = Math.Truncate(time);
                    minutes = Math.Round((time - hours) * 100);
                }

                output =
                    $"{stat.Level} (next level in {hours}h {minutes}m | {stat.Experience - stat.PrevLevelXp - GetXpDiff(stat.Level)}/{stat.NextLevelXp - stat.PrevLevelXp - GetXpDiff(stat.Level)} XP)";
            }
            return output;
        }

        public double GetRuntime()
        {
            return (DateTime.Now - _initSessionDateTime).TotalSeconds / 3600;
        }

        public static int GetXpDiff(int level)
        {
            if (level > 0 && level <= 40)
            {
                int[] xpTable = { 0, 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000,
                    10000, 10000, 10000, 10000, 15000, 20000, 20000, 20000, 25000, 25000,
                    50000, 75000, 100000, 125000, 150000, 190000, 200000, 250000, 300000, 350000,
                    500000, 500000, 750000, 1000000, 1250000, 1500000, 2000000, 2500000, 1000000, 1000000};
                return xpTable[level - 1];
            }
            return 0;
        }

        public string ToStrings(Inventory _currentLevelInfos)
        {
            var stats = _currentLevelInfos.GetPlayerStats().Result;
            var stat = stats.FirstOrDefault();
            if (stat != null)
            {
                return
                $"Runtime {FormatRuntime()} - Lvl: { stat.Level } | EXP / H: { TotalExperience / GetRuntime():0} | P / H: { TotalPokemons / GetRuntime():0} | Stardust: { TotalStardust: 0} | Transfered: { TotalPokemonsTransfered: 0} | Items Recycled: { TotalItemsRemoved: 0} ";
            }
            else
                return string.Empty;
        }
    }
}