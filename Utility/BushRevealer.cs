using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using OKTRAIO.Menu_Settings;

namespace OKTRAIO.Utility
{
    public static class BushRevealer
    {
        private static int lastWarded;

        public static void Init()
        {
            Game.OnUpdate += GameOnUpdate;
        }

        private static void GameOnUpdate(EventArgs args)
        {
            if (Value.Use("bushreveal.use"))
            {
                Random rnd = new Random();

                var random = Value.Use("bushreveal.humanize") ? rnd.Next(200, 500) : 0;

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) != true) return;

                foreach (
                    var heros in
                        EntityManager.Heroes.Enemies.Where(x => !x.IsDead && x.Distance(Player.Instance) < 1000))
                {
                    var path = heros.Path.LastOrDefault();

                    if (NavMesh.IsWallOfGrass(path, 1))
                    {
                        if (heros.Distance(path) > 200) return;
                        if (NavMesh.IsWallOfGrass(Player.Instance.Position, 1) && Player.Instance.Distance(path) < 200)
                            return;

                        if (Player.Instance.Distance(path) < 500)
                        {
                            foreach (
                                var obj in
                                    ObjectManager.Get<AIHeroClient>()
                                        .Where(
                                            x => x.Name.ToLower().Contains("ward") && x.IsAlly && x.Distance(path) < 300)
                                )
                            {
                                if (NavMesh.IsWallOfGrass(obj.Position, 1)) return;
                            }

                            var wardslot = GetWardSlot();
                            if (wardslot != null && Environment.TickCount - lastWarded > 1000)
                            {
                                Core.DelayAction(() => wardslot.Cast(path), 100);
                                lastWarded = Environment.TickCount;
                            }
                        }
                    }
                }
            }
        }

        public static ItemId[] WardIds =
            {

            ItemId.Warding_Totem_Trinket,
            ItemId.Greater_Stealth_Totem_Trinket,
            ItemId.Greater_Vision_Totem_Trinket,
            ItemId.Sightstone, ItemId.Ruby_Sightstone,
            (ItemId) 2301,
            (ItemId)2302,
            (ItemId)2303,
            (ItemId) 3711,
            (ItemId) 1411,
            (ItemId) 1410,
            (ItemId) 1408,
            (ItemId) 1409

            };

        public static InventorySlot GetWardSlot()
        {
            return
                WardIds.Select(wardId => Player.Instance.InventoryItems.FirstOrDefault(a => a.Id == wardId))
                    .FirstOrDefault(slot => slot != null && slot.CanUseItem());
        }
    }

}
