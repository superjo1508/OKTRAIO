using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace OKTRAIO.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Enumerations;
    using EloBuddy.SDK.Events;
    using EloBuddy.SDK.Rendering;

    using OKTRAIO.Menu_Settings;

    using SharpDX;
    using SharpDX.Direct3D9;

    using Color = System.Drawing.Color;

    public class RecallTracker : UtilityAddon
    {
        private const int Length = 260;

        private const int Height = 25;

        private readonly List<Recall> Recalls = new List<Recall>();

        private Text Text;

        private readonly int BarHeight = 10;

        private int BarX
        {
            get
            {
                return Value.Get("recall.x");
            }
        }

        private int BarY
        {
            get
            {
                return Value.Get("recall.y");
            }
        }

        private int BarWidth
        {
            get
            {
                return Value.Get("recall.width");
            }
        }

        public override UtilityInfo GetUtilityInfo()
        {
            return new UtilityInfo(this, "RecallTracker", "recalltracker", "Unknown");
        }

        protected override void InitializeMenu()
        {
            Menu.AddGroupLabel("OKTR AIO - RecallTracker for " + Player.Instance.ChampionName, "recall.grouplabel.utilitymenu");
            Menu.AddCheckBox("recall.use", "Use RecallTracker");
            Menu.Add("recall.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange += Value.AdvancedModeChanged;
            Menu.AddCheckBox("recall.recallsEnemy", "Show enemy recalls", true, true);
            Menu.AddCheckBox("recall.recallsAlly", "Show ally recalls", true, true);
            Menu.AddSlider("recall.x", "Recall location X", (int)(Drawing.Width * 0.4), 0, Drawing.Width, true);
            Menu.AddSlider("recall.y", "Recall location Y", (int)(Drawing.Height * 0.75), 0, Drawing.Height, true);
            Menu.AddSlider("recall.width", "Recall width", 300, 200, 500, true);
        }

        public override void Initialize()
        {
            Teleport.OnTeleport += OnTeleport;

            Text = new Text(
                "",
                new FontDescription
                    {
                        FaceName = "Calibri", Height = Height / 30 * 23, OutputPrecision = FontPrecision.Default,
                        Quality = FontQuality.ClearType
                    });

            foreach (var hero in ObjectManager.Get<AIHeroClient>())
            {
                Recalls.Add(new Recall(hero, RecallStatus.Inactive));
            }
        }
        protected override void Game_OnTick(EventArgs args)
        {
            foreach (var recall in Recalls)
            {
                if (recall.Status != RecallStatus.Inactive)
                {
                    var recallDuration = recall.Duration;
                    var cd = recall.Started + recallDuration - Game.Time;
                    var percent = cd > 0 && Math.Abs(recallDuration) > float.Epsilon ? 1f - cd / recallDuration : 1f;
                    var textLength = (recall.Unit.ChampionName.Length + 6) * 7;
                    var myLength = percent * Length;
                    var freeSpaceLength = myLength + textLength;
                    var freeSpacePercent = freeSpaceLength / Length > 1 ? 1 : freeSpaceLength / Length;
                    if (
                        Recalls.Any(
                            h =>
                            GetRecallPercent(h) > percent && GetRecallPercent(h) < freeSpacePercent
                            && h.TextPos == recall.TextPos && recall.Started > h.Started))
                    {
                        recall.TextPos += 1;
                    }

                    if (recall.Status == RecallStatus.Finished
                        && Recalls.Any(
                            h =>
                            h.Started > recall.Started && h.TextPos == recall.TextPos
                            && recall.Started + 3 < h.Started + recall.Duration))
                    {
                        recall.TextPos += 1;
                    }
                }
            }
        }

        public void DrawRect(float x, float y, float width, float height, float thickness, Color color)
        {
            for (var i = 0; i < height; i++)
            {
                Drawing.DrawLine(x, y + i, x + width, y + i, thickness, color);
            }
        }

        protected override void Drawing_OnEndScene(EventArgs args)
        {
            if (Value.Use("recall.use"))
            {
                var recalls =
                    Recalls.Where(
                        x =>
                        (x.Unit.IsAlly && Value.Use("recall.recallsAlly"))
                        || (x.Unit.IsEnemy && Value.Use("recall.recallsEnemy"))).OrderBy(x => x.Started);

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
                    if (recall.Status == RecallStatus.Active)
                    {
                        var percent = GetRecallPercent(recall);
                        var colorBar = recall.Unit.IsAlly ? Color.DarkGreen : Color.DarkOrange;

                        DrawRect(BarX, BarY, BarWidth * (float)percent, BarHeight, 1, colorBar);

                        Text.Color = Color.White;
                        Text.TextValue = "(" + (int)(percent * 100) + "%) " + recall.Unit.ChampionName;
                        Text.Position = new Vector2(
                            BarWidth * (float)percent + BarX - 20,
                            BarY + 10 + recall.TextPos * 20);
                        Text.Draw();
                    }
                }
            }
        }

        private double GetRecallPercent(Recall recall)
        {
            var recallDuration = recall.Duration;
            var cd = recall.Started + recallDuration - Game.Time;
            var percent = cd > 0 && Math.Abs(recallDuration) > float.Epsilon ? 1f - cd / recallDuration : 1f;
            return percent;
        }

        public void OnTeleport(Obj_AI_Base sender, Teleport.TeleportEventArgs args)
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

        public RecallTracker(Menu menu, Champion? champion = null) : base(menu, champion)
        {
        }
    }
}