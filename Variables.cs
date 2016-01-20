using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using MarksmanAIO.Champions;

namespace MarksmanAIO
{
    public class Variables
    {

        public static AIHeroClient GetBestExplosionRange(float distance, int radius)
        {
            var enemies =
                        EntityManager.Heroes.Enemies.Where(
                            o => Player.Instance.Distance(o) < distance && o.IsValidTarget())
                            .OrderBy(o => o.CountEnemiesInRange(radius))
                            .FirstOrDefault();
            return enemies;
        }

        public static List<AIHeroClient> CloseEnemies(float range = 1500)
        {
            return EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(range)).ToList();
        }

        public static List<AIHeroClient> CloseAllies(float range = 1500)
        {
            return EntityManager.Heroes.Allies.Where(a => a.IsValidTarget(range) && !a.IsMe).ToList();
        }

        public static readonly string[] SummonerRiftJungleList =
        {
            "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron", "SRU_Gromp",
            "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Krug", "Sru_Crab"
        };

        public static readonly string[] TwistedJungleList =
        {
            "TT_NWraith1.1", "TT_NWraith4.1",
            "TT_NGolem2.1", "TT_NGolem5.1", "TT_NWolf3.1", "TT_NWolf6.1", "TT_Spiderboss8.1"
        };

        public static float GetChampionDamage(Obj_AI_Base target)
        {
            if (Player.Instance.ChampionName == "Lucian") return Lucian.GetRawDamage(target);
            return 0;
        }
    }


}
