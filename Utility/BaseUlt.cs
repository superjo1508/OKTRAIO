using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using OKTRAIO.Menu_Settings;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Font = EloBuddy.SDK.Rendering.Text;

namespace OKTRAIO.Utility
{
    public class BaseUlt
    {
        private const int LineThickness = 4;
        private const int Length = 260;
        private const int Height = 25;
        private static readonly List<Recall> Recalls = new List<Recall>();
        private static readonly List<BaseUltUnit> BaseUltUnits = new List<BaseUltUnit>();
        private static readonly List<BaseUltSpell> BaseUltSpells = new List<BaseUltSpell>();
        private static Font Text;

        public static void Initialize()
        {
            if (UtilityMenu._baseult == null) return;

            Game.OnTick += OnTick;
            Drawing.OnEndScene += OnEndScene;
            Teleport.OnTeleport += OnTeleport;

            Text = new Font("", new FontDescription
            {
                FaceName = "Calibri",
                Height = (Height / 30) * 23,
                OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.ClearType
            });

            foreach (var hero in ObjectManager.Get<AIHeroClient>())
            {
                Recalls.Add(new Recall(hero, RecallStatus.Inactive));
            }

            #region Spells

            BaseUltSpells.Add(new BaseUltSpell("Ashe", SpellSlot.R, 250, 1600, 130, true));
            BaseUltSpells.Add(new BaseUltSpell("Draven", SpellSlot.R, 400, 2000, 160, true));
            BaseUltSpells.Add(new BaseUltSpell("Ezreal", SpellSlot.R, 1000, 2000, 160, false));
            BaseUltSpells.Add(new BaseUltSpell("Jinx", SpellSlot.R, 600, 1700, 140, true));

            #endregion
        }

        public static void OnTick(EventArgs args)
        {
            foreach (var recall in Recalls)
            {
                if (recall.Status != RecallStatus.Inactive)
                {
                    var recallDuration = recall.Duration;
                    var cd = recall.Started + recallDuration - Game.Time;
                    var percent = (cd > 0 && Math.Abs(recallDuration) > float.Epsilon) ? 1f - (cd / recallDuration) : 1f;
                    var textLength = (recall.Unit.ChampionName.Length + 6) * 7;
                    var myLength = percent * Length;
                    var freeSpaceLength = myLength + textLength;
                    var freeSpacePercent = freeSpaceLength / Length > 1 ? 1 : freeSpaceLength / Length;
                    if (
                        Recalls.Any(
                            h =>
                                GetRecallPercent(h) > percent && GetRecallPercent(h) < freeSpacePercent &&
                                h.TextPos == recall.TextPos && recall.Started > h.Started))
                    {
                        recall.TextPos += 1;
                    }

                    if (recall.Status == RecallStatus.Finished &&
                        Recalls.Any(
                            h =>
                                h.Started > recall.Started && h.TextPos == recall.TextPos &&
                                recall.Started + 3 < h.Started + recall.Duration))
                    {
                        recall.TextPos += 1;
                    }
                }

                if (recall.Status == RecallStatus.Active)
                {
                    var compatibleChamps = new[] { "Ashe", "Draven", "Ezreal",  "Jinx" };
                    if (recall.Unit.IsEnemy && compatibleChamps.Any(h => h == Player.Instance.ChampionName) &&
                        BaseUltUnits.All(h => h.Unit.NetworkId != recall.Unit.NetworkId))
                    {
                        var spell = BaseUltSpells.Find(h => h.Name == Player.Instance.ChampionName);
                        if (Player.Instance.Spellbook.GetSpell(spell.Slot).IsReady &&
                            Player.Instance.Spellbook.GetSpell(spell.Slot).Level > 0)
                        {
                            BaseUltCalcs(recall);
                        }
                    }
                }

                if (recall.Status != RecallStatus.Active)
                {
                    var baseultUnit = BaseUltUnits.Find(h => h.Unit.NetworkId == recall.Unit.NetworkId);
                    if (baseultUnit != null)
                    {
                        BaseUltUnits.Remove(baseultUnit);
                    }
                }
            }

            foreach (var unit in BaseUltUnits)
            {
                if (unit.Collision)
                {
                    continue;
                }

                if (unit.Unit.IsVisible)
                {
                    unit.LastSeen = Game.Time;
                }

                if (Math.Round(unit.FireTime, 1) == Math.Round(Game.Time, 1) && Game.Time >= unit.LastSeen)
                {
                    var spell = Player.Instance.Spellbook.GetSpell(BaseUltSpells.Find(h => h.Name == Player.Instance.ChampionName).Slot);
                    if (spell.IsReady)
                    {
                        Player.Instance.Spellbook.CastSpell(spell.Slot, GetFountainPos());
                    }
                }
            }
        }

        static int BarX
        {
            get { return Value.Get("baseult.x"); }
        }

        static int BarY
        {
            get { return Value.Get("baseult.y"); }
        }

        static int BarWidth
        {
            get { return Value.Get("baseult.width"); }
        } 

        static int BarHeight = 10;

        public static void DrawRect(float x, float y, float width, float height, float thickness, System.Drawing.Color color)
        {
            for (int i = 0; i < height; i++)
                Drawing.DrawLine(x, y + i, x + width, y + i, thickness, color);
        }

        public static void OnEndScene(EventArgs args)
        {
            var recalls = Recalls.Where(
                x => 
                    (x.Unit.IsAlly && Value.Use("baseult.recallsAlly"))
                    || (x.Unit.IsEnemy && Value.Use("baseult.recallsEnemy"))).OrderBy(x => x.Started);

            if (recalls.Any(x => x.Status == RecallStatus.Active))
            {
                DrawRect(BarX, BarY, BarWidth, BarHeight, 1, Color.DarkGray);
            }
            else
            {
                return;
            }

            foreach (var recall in recalls)
            {
                var recallDuration = recall.Duration;
                if (recall.Status == RecallStatus.Active)
                {
                    var isBaseUlt = BaseUltUnits.Any(h => h.Unit.NetworkId == recall.Unit.NetworkId);
                    var percent = GetRecallPercent(recall);
                    var colorBar = isBaseUlt ? Color.Red : (recall.Unit.IsAlly ? Color.DarkGreen : Color.DarkOrange);

                    DrawRect(BarX, BarY, BarWidth * (float)percent, BarHeight, 1, colorBar);

                    Text.Color = Color.White;
                    Text.TextValue = "(" + (int)(percent * 100) + "%) " + recall.Unit.ChampionName;
                    Text.Position = new Vector2(BarWidth * (float)percent + BarX - 20, BarY + 10 + (recall.TextPos*20));
                    Text.Draw();
                }

                if (recall.Status == RecallStatus.Abort || recall.Status == RecallStatus.Finished)
                {
                    //const int fadeoutTime = 3;
                    //var colorIndicator = recall.Status == RecallStatus.Abort ? Color.OrangeRed : Color.GreenYellow;
                    //var colorText = recall.Status == RecallStatus.Abort ? Color.Orange : Color.GreenYellow;
                    //var colorBar = recall.Status == RecallStatus.Abort ? Color.Yellow : Color.LawnGreen;
                    //var fadeOutPercent = (recall.Ended + fadeoutTime - Game.Time) / fadeoutTime;
                    //if (recall.Ended + fadeoutTime > Game.Time)
                    //{
                    //    var timeUsed = recall.Ended - recall.Started;
                    //    var percent = timeUsed > recallDuration ? 1 : timeUsed / recallDuration;
                    //}
                    //else
                    //{
                    //    recall.Status = RecallStatus.Inactive;
                    //    recall.TextPos = 0;
                    //}
                }
            }

            foreach (var unit in BaseUltUnits)
            {
                var duration = Recalls.Find(h => h.Unit.NetworkId == unit.Unit.NetworkId).Duration;
                var barPos = (unit.FireTime - Recalls.Find(h => unit.Unit.NetworkId == h.Unit.NetworkId).Started) /
                             duration;

                Drawing.DrawLine(
                    (int)(barPos * BarWidth) + BarX, BarY - 15,
                    (int)(barPos * BarWidth) + BarX, BarY - 5, 
                    LineThickness, Color.Lime);
            }
        }

        private static Vector3 GetFountainPos()
        {
            switch (Game.MapId)
            {
                case GameMapId.SummonersRift:
                    {
                        return Player.Instance.Team == GameObjectTeam.Order
                            ? new Vector3(14296, 14362, 171)
                            : new Vector3(408, 414, 182);
                    }
            }

            return new Vector3();
        }

        private static double GetRecallPercent(Recall recall)
        {
            var recallDuration = recall.Duration;
            var cd = recall.Started + recallDuration - Game.Time;
            var percent = (cd > 0 && Math.Abs(recallDuration) > float.Epsilon) ? 1f - (cd / recallDuration) : 1f;
            return percent;
        }

        private static float GetBaseUltTravelTime(float delay, float speed)
        {
            if (Player.Instance.ChampionName == "Karthus")
            {
                return delay / 1000;
            }

            var distance = Vector3.Distance(Player.Instance.ServerPosition, GetFountainPos());
            var missilespeed = speed;
            if (Player.Instance.ChampionName == "Jinx" && distance > 1350)
            {
                const float accelerationrate = 0.3f;
                var acceldifference = distance - 1350f;
                if (acceldifference > 150f)
                {
                    acceldifference = 150f;
                }

                var difference = distance - 1500f;
                missilespeed = (1350f * speed + acceldifference * (speed + accelerationrate * acceldifference) +
                                difference * 2200f) / distance;
            }

            return (distance / missilespeed + ((delay - 65) / 1000));
        }

        private static double GetBaseUltSpellDamage(BaseUltSpell spell, AIHeroClient target)
        {
            var level = Player.Instance.Spellbook.GetSpell(spell.Slot).Level - 1;
            switch (spell.Name)
            {
                case "Ashe":
                    {
                        var damage = new float[] { 250, 425, 600 }[level] + 1 * Player.Instance.FlatMagicDamageMod;
                        return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical, damage);
                    }

                case "Draven":
                    {
                        var damage = new float[] { 175, 275, 375 }[level] + 1.1f * Player.Instance.FlatPhysicalDamageMod;
                        return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, damage) * 0.7;
                    }

                case "Ezreal":
                    {
                        var damage = new float[] { 350, 500, 650 }[level] + 0.9f * Player.Instance.FlatMagicDamageMod +
                                     1 * Player.Instance.FlatPhysicalDamageMod;
                        return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical, damage) * 0.7;
                    }

                case "Jinx":
                    {
                        var damage = new float[] { 250, 350, 450 }[level] +
                                     new float[] { 25, 30, 35 }[level] / 100 * (target.MaxHealth - target.Health) +
                                     1 * Player.Instance.FlatPhysicalDamageMod;
                        return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, damage);
                    }
            }

            return 0;
        }

        private static void BaseUltCalcs(Recall recall)
        {
            var finishedRecall = recall.Started + recall.Duration;
            var spellData = BaseUltSpells.Find(h => h.Name == Player.Instance.ChampionName);
            var timeNeeded = GetBaseUltTravelTime(spellData.Delay, spellData.Speed);
            var fireTime = finishedRecall - timeNeeded;
            var spellDmg = GetBaseUltSpellDamage(spellData, recall.Unit);
            var collision = GetCollision(spellData.Radius, spellData).Any();
            if (fireTime > Game.Time && fireTime < recall.Started + recall.Duration && recall.Unit.Health < spellDmg
                && Value.Use("baseult." + recall.Unit.ChampionName) && Value.Use("baseult.use"))
            {
                BaseUltUnits.Add(new BaseUltUnit(recall.Unit, fireTime, collision));
            }
            else if (BaseUltUnits.Any(h => h.Unit.NetworkId == recall.Unit.NetworkId))
            {
                BaseUltUnits.Remove(BaseUltUnits.Find(h => h.Unit.NetworkId == recall.Unit.NetworkId));
            }
        }

        public static void OnTeleport(Obj_AI_Base sender, Teleport.TeleportEventArgs args)
        {
            var unit = Recalls.Find(h => h.Unit.NetworkId == sender.NetworkId);
            if (unit == null || args.Type != TeleportType.Recall)
            {
                return;
            }

            switch (args.Status)
            {
                case TeleportStatus.Start:
                    {
                        unit.Status = RecallStatus.Active;
                        unit.Started = Game.Time;
                        unit.TextPos = 0;
                        unit.Duration = (float)args.Duration / 1000;
                        break;
                    }

                case TeleportStatus.Abort:
                    {
                        unit.Status = RecallStatus.Abort;
                        unit.Ended = Game.Time;
                        break;
                    }

                case TeleportStatus.Finish:
                    {
                        unit.Status = RecallStatus.Finished;
                        unit.Ended = Game.Time;
                        break;
                    }
            }
        }

        private static IEnumerable<Obj_AI_Base> GetCollision(float spellwidth, BaseUltSpell spell)
        {
            return (from unit in EntityManager.Heroes.Enemies.Where(h => Player.Instance.Distance(h) < 2000)
                    let pred =
                        Prediction.Position.PredictLinearMissile(unit, 2000, (int)spell.Radius, (int)spell.Delay,
                            spell.Speed, -1)
                    let endpos = Player.Instance.ServerPosition.Extend(GetFountainPos(), 2000)
                    let projectOn = pred.UnitPosition.To2D().ProjectOn(Player.Instance.ServerPosition.To2D(), endpos)
                    where projectOn.SegmentPoint.Distance(endpos) < spellwidth + unit.BoundingRadius
                    select unit).Cast<Obj_AI_Base>().ToList();
        }
    }

    public class Recall
    {
        public int TextPos;

        public Recall(AIHeroClient unit, RecallStatus status)
        {
            Unit = unit;
            Status = status;
        }

        public AIHeroClient Unit { get; set; }
        public RecallStatus Status { get; set; }
        public float Started { get; set; }
        public float Ended { get; set; }
        public float Duration { get; set; }
    }

    public class BaseUltUnit
    {
        public BaseUltUnit(AIHeroClient unit, float fireTime, bool collision)
        {
            Unit = unit;
            FireTime = fireTime;
            Collision = collision;
        }

        public AIHeroClient Unit { get; set; }
        public float FireTime { get; set; }
        public bool Collision { get; set; }
        public float LastSeen { get; set; }
    }

    public class BaseUltSpell
    {
        public BaseUltSpell(string name, SpellSlot slot, float delay, float speed, float radius, bool collision)
        {
            Name = name;
            Slot = slot;
            Delay = delay;
            Speed = speed;
            Radius = radius;
            Collision = collision;
        }

        public string Name { get; set; }
        public SpellSlot Slot { get; set; }
        public float Delay { get; set; }
        public float Speed { get; set; }
        public float Radius { get; set; }
        public bool Collision { get; set; }
    }

    public enum RecallStatus
    {
        Active,
        Inactive,
        Finished,
        Abort
    }
}
