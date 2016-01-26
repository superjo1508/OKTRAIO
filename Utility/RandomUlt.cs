namespace OKTRAIO.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Enumerations;
    using EloBuddy.SDK.Events;

    using OKTRAIO.Menu_Settings;

    using SharpDX;

    public class RandomUlt
    {
        private static readonly Vector3 spawnPos =
            ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy).Position;

        private static readonly List<RandomUltUnit> RandomUltUnits = new List<RandomUltUnit>();

        private static readonly List<RandomUltSpell> RandomUltSpells = new List<RandomUltSpell>();

        private static readonly List<Champion> CompatibleChampions = new List<Champion>
                                                                         {
                                                                             Champion.Ashe, Champion.Draven,
                                                                             Champion.Ezreal, Champion.Jinx,
                                                                             Champion.Gangplank, Champion.Lux,
                                                                             Champion.Ziggs
                                                                         };

        public static void Initialize()
        {
            if (UtilityMenu.Randomult == null) { return; }

            Game.OnUpdate += OnUpdate;
            Teleport.OnTeleport += OnTeleport;

            foreach (var hero in EntityManager.Heroes.Enemies)
            {
                RandomUltUnits.Add(new RandomUltUnit(hero));
            }

            #region Spells

            RandomUltSpells.Add(new RandomUltSpell("Ashe", SpellSlot.R, 250, 1600, 130, int.MaxValue, true));
            RandomUltSpells.Add(new RandomUltSpell("Draven", SpellSlot.R, 400, 2000, 160, int.MaxValue, true));
            RandomUltSpells.Add(new RandomUltSpell("Ezreal", SpellSlot.R, 1000, 2000, 160, int.MaxValue, false));
            RandomUltSpells.Add(new RandomUltSpell("Jinx", SpellSlot.R, 600, 1700, 140, int.MaxValue, true));
            RandomUltSpells.Add(
                new RandomUltSpell("Gangplank", SpellSlot.R, 100, int.MaxValue, 600, int.MaxValue, false));
            RandomUltSpells.Add(new RandomUltSpell("Lux", SpellSlot.R, 500, int.MaxValue, 190, 3000, false));
            RandomUltSpells.Add(new RandomUltSpell("Ziggs", SpellSlot.R, 100, 1750, 525, 5000, false));

            #endregion
        }

        public static bool IsCompatibleChamp()
        {
            return CompatibleChampions.Any(x => x.Equals(Player.Instance.Hero));
        }

        public static void OnUpdate(EventArgs args)
        {
            foreach (var enemy in
                RandomUltUnits.Where(x => x.Unit.IsHPBarRendered && !x.Unit.IsDead && x.Unit.IsValidTarget()))
            {
                enemy.LastSeen = Game.Time;
                enemy.OldPos = enemy.Unit.Position;
                enemy.PredictedPos = Prediction.Position.PredictUnitPosition(enemy.Unit, 10).To3DWorld();
            }

            if (!IsCompatibleChamp() || !Player.Instance.Spellbook.GetSpell(SpellSlot.R).IsReady
                || Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None
                || Player.Instance.CountEnemiesInRange(Value.Get("randomult.range")) >= 1 || !Value.Use("randomult.use")
                || (Player.Instance.ChampionName == "Draven"
                    && Player.Instance.Spellbook.GetSpell(SpellSlot.R).Name != "DravenRCast"))
            {
                return;
            }
            foreach (var enemy in
                RandomUltUnits.Where(
                    x =>
                    x.Unit.IsValid() && !x.Unit.IsDead && Value.Use("randomult." + x.Unit.ChampionName)
                    && x.RecallData.Status == RecallStatus.Active).OrderByDescending(x => x.RecallData.Started))
            {
                if (CheckResurrect(enemy.Unit) || !(Game.Time - enemy.RecallData.Started > Value.Get("randomult.delay")))
                {
                    continue;
                }
                Chat.Print(enemy.RecallData.Started - enemy.LastSeen);
                var dist = ((enemy.RecallData.Started - enemy.LastSeen) / 1000 * enemy.Unit.MoveSpeed);
                          // - enemy.Unit.MoveSpeed / 4;
                if (dist > 1500 || (dist * 2 > 1000 && !enemy.Unit.IsHPBarRendered))
                {
                    continue;
                }
                var pos = enemy.Unit.IsHPBarRendered
                              ? enemy.Unit.Position
                              : enemy.OldPos.Extend(enemy.PredictedPos, dist).To3D();
                if (Player.Instance.Distance(pos)
                    > RandomUltSpells.Find(x => x.Name == Player.Instance.ChampionName).Range)
                        {
                            continue;
                        }
                CastRandomUlt(enemy, pos);
            }
        }

        private static void CastRandomUlt(RandomUltUnit target, Vector3 pos)
        {
            var ult = Player.Instance.Spellbook.GetSpell(SpellSlot.R);
            if (ult.IsReady)
            {
                if (GetRandomUltSpellDamage(
                    RandomUltSpells.Find(x => x.Name == Player.Instance.ChampionName),
                    target.Unit) && UltTime(pos) < target.RecallData.GetRecallTime())
                {
                    Player.Instance.Spellbook.CastSpell(SpellSlot.R, pos);
                }
            }
        }

        public static float GetPath(AIHeroClient hero, Vector3 b)
        {
            var path = hero.GetPath(b);
            var lastPoint = path[0];
            var distance = 0f;
            foreach (var point in path.Where(point => !point.Equals(lastPoint)))
            {
                distance += lastPoint.Distance(point);
                lastPoint = point;
            }
            return distance;
        }

        private static bool CheckResurrect(AIHeroClient enemy)
        {
            switch (enemy.ChampionName)
            {
                case "Anivia":
                    return !enemy.HasBuff("rebirthcooldown");

                case "Aatrox":
                    return enemy.HasBuff("aatroxpassiveready");
            }
            return enemy.HasUndyingBuff() || enemy.HasBuffOfType(BuffType.SpellShield) || enemy.IsInvulnerable;
        }

        private static float UltTime(Vector3 pos)
        {
            var distance = Player.Instance.Distance(pos);
            var spell = RandomUltSpells.Find(x => x.Name == Player.Instance.ChampionName);
            var delay = spell.Delay;
            var speed = spell.Speed;
            if (Player.Instance.ChampionName == "Jinx" && distance > 1350)
            {
                var accelDif = distance - 1350;
                if (accelDif > 150) { accelDif = 150; }
                var difference = distance - 1500;
                return (distance / ((1350 * 1700 + accelDif * (1700 + 0.3f * accelDif) + difference * 1700f) / distance))
                       * 1000 + 250;
            }
            return (distance / speed) * 1000 + delay;
        }

        private static Vector2 PredictPos(RandomUltUnit target, float distance)
        {
            var recallTime = (target.RecallData.Started - target.LastSeen);
            var line = target.Unit.Position.Extend(target.PredictedPos, distance);
            if (target.Unit.Position.Distance(target.PredictedPos) < distance
                && ((recallTime < 2000
                     || target.Unit.Position.Distance(target.PredictedPos) > target.Unit.Position.Distance(line) * 0.70f)))
            {
                line = target.PredictedPos.To2D();
            }
            return line;
        }

        private static bool GetRandomUltSpellDamage(RandomUltSpell spell, Obj_AI_Base target)
        {
            var level = Player.Instance.Spellbook.GetSpell(spell.Slot).Level - 1;
            var damage = 0f;
            var damageType = DamageType.Mixed;
            switch (spell.Name)
            {
                case "Ashe":
                    {
                        damage = new float[] { 250, 425, 600 }[level] + 1 * Player.Instance.FlatMagicDamageMod;
                        damageType = DamageType.Magical;
                        break;
                    }

                case "Draven":
                    {
                        damage = (new float[] { 175, 275, 375 }[level] + 1.1f * Player.Instance.FlatPhysicalDamageMod)
                                 * 2;
                        damageType = DamageType.Physical;
                        break;
                    }

                case "Ezreal":
                    {
                        damage = new float[] { 350, 500, 650 }[level] + 0.9f * Player.Instance.FlatMagicDamageMod
                                 + 1 * Player.Instance.FlatPhysicalDamageMod;
                        damageType = DamageType.Magical;
                        break;
                    }

                case "Gangplank":
                    {
                        damage = (new float[] { 50, 70, 90 }[level] + 0.1f * Player.Instance.FlatMagicDamageMod) * 3;
                        damageType = DamageType.Magical;
                        break;
                    }

                case "Jinx":
                    {
                        damage = new float[] { 250, 350, 450 }[level]
                                 + new float[] { 25, 30, 35 }[level] / 100 * (target.MaxHealth - target.Health)
                                 + 1 * Player.Instance.FlatPhysicalDamageMod;
                        damageType = DamageType.Physical;
                        break;
                    }

                case "Lux":
                    {
                        damage = new float[] { 300, 400, 500 }[level] + 0.75f * Player.Instance.FlatMagicDamageMod;
                        damageType = DamageType.Magical;
                        break;
                    }

                case "Ziggs":
                    {
                        damage = new float[] { 200, 300, 400 }[level] + 0.72f * Player.Instance.FlatMagicDamageMod;
                        damageType = DamageType.Magical;
                        break;
                    }
            }
            return Player.Instance.CalculateDamageOnUnit(target, damageType, damage) * 0.7 > target.Health;
        }

        public static void OnTeleport(Obj_AI_Base sender, Teleport.TeleportEventArgs args)
        {
            var recall = RandomUltUnits.Find(h => h.Unit.NetworkId == sender.NetworkId).RecallData;
            if (!sender.IsEnemy || recall == null || args.Type != TeleportType.Recall)
            {
                return;
            }

            switch (args.Status)
            {
                case TeleportStatus.Start:
                    {
                        recall.Status = RecallStatus.Active;
                        recall.Started = Game.Time;
                        recall.Duration = args.Duration;
                        recall.Ended = recall.Started + recall.Duration;
                        break;
                    }

                case TeleportStatus.Abort:
                    {
                        recall.Status = RecallStatus.Abort;
                        recall.Ended = Game.Time;
                        break;
                    }

                case TeleportStatus.Finish:
                    {
                        recall.Status = RecallStatus.Finished;
                        recall.Ended = Game.Time;
                        break;
                    }
            }
        }
    }

    internal class RandomUltUnit
    {
        public AIHeroClient Unit;

        public float LastSeen;

        public Vector3 PredictedPos;

        public Vector3 OldPos;

        public RandomUltRecall RecallData;

        public RandomUltUnit(AIHeroClient unit)
        {
            Unit = unit;
            RecallData = new RandomUltRecall(unit, RecallStatus.Inactive);
        }
    }

    public class RandomUltSpell
    {
        public RandomUltSpell(
            string name,
            SpellSlot slot,
            float delay,
            float speed,
            float radius,
            float range,
            bool collision)
        {
            Name = name;
            Slot = slot;
            Delay = delay;
            Speed = speed;
            Radius = radius;
            Range = range;
            Collision = collision;
        }

        public string Name { get; set; }

        public SpellSlot Slot { get; set; }

        public float Delay { get; set; }

        public float Speed { get; set; }

        public float Radius { get; set; }

        public float Range { get; set; }

        public bool Collision { get; set; }
    }

    public class RandomUltRecall
    {
        public RandomUltRecall(AIHeroClient unit, RecallStatus status)
        {
            Unit = unit;
            Status = status;
        }

        public AIHeroClient Unit { get; set; }

        public RecallStatus Status { get; set; }

        public float Started { get; set; }

        public float Ended { get; set; }

        public float Duration { get; set; }

        public float GetRecallTime()
        {
            return (Started + Duration - Game.Time) * 1000;
        }
    }
}