using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using OKTRAIO.Menu_Settings;
using OKTRAIO.Utility;
using SharpDX;
using MainMenu = OKTRAIO.Menu_Settings.MainMenu;

namespace OKTRAIO.Champions
{
    class Kalista : AIOChampion
    {
        private static Spell.Skillshot _q;
        private static Spell.Targeted _w;
        private static Spell.Active _e;
        private static Spell.Active _r;

        private static Menu _sentinel;
        private static Menu _balista;

        public static AIHeroClient Soulbound
        {
            get { return EntityManager.Heroes.Allies.FirstOrDefault(x => x.HasBuff("kalistacoopstrikeally")); }
        }
        public static bool BalistaPossible
        {
            get { return Soulbound != null && Soulbound.ChampionName == "Blitzcrank"; }
        }
        private static bool _fleeActivated;

        private static readonly Vector2 Baron = new Vector2(5007.124f, 10471.45f);
        private static readonly Vector2 Dragon = new Vector2(9866.148f, 4414.014f);

        public override void Init()
        {
            try
            {
                //Creating Spells
                _q = new Spell.Skillshot(SpellSlot.Q, 1150, SkillShotType.Linear, 250, 1200, 40);
                _w = new Spell.Targeted(SpellSlot.W, 5000);
                _e = new Spell.Active(SpellSlot.E, 950);
                _r = new Spell.Active(SpellSlot.R, 1500);

                try
                {
                    #region Create menu
                    //Combo Menu Settings
                    MainMenu.ComboKeys(true, false, true, false);
                    MainMenu._combo.AddSeparator();
                    MainMenu._combo.AddGroupLabel("Combo Preferences", "combo.grouplabel.addonmenu", true);
                    MainMenu._combo.AddCheckBox("combo.gapcloser", "Use minions/jungle for GapCloser", true, true);
                    MainMenu._combo.AddCheckBox("combo.e.enemy", "Harass enemy with E when minion can die", false, true);
                    MainMenu._combo.AddSeparator();
                    MainMenu.ComboManaManager(true, false, false, false, 40, 0, 0, 0);

                    //Lane Clear Menu Settings
                    MainMenu.LaneKeys(true, false, true, false);
                    MainMenu._lane.AddSeparator();
                    MainMenu._lane.AddGroupLabel("Laneclear Preferences", "lane.grouplabel.addonmenu", true);
                    MainMenu._lane.AddCheckBox("lane.e.enemy", "Harass enemy with E when minion can die", true, true);
                    MainMenu._lane.AddSlider("lane.q.min", "Mininum {0} minions to use Q", 3, 2, 10, true);
                    MainMenu._lane.AddSlider("lane.e.min", "Mininum {0} minions to use E", 3, 2, 10, true);
                    MainMenu._lane.AddSeparator();//30 q - 30 e
                    MainMenu._lane.AddGroupLabel("Mana Manager:", "lane.grouplabel.addonmenu.1", true);
                    MainMenu.LaneManaManager(true, false, true, false, 30, 0, 30, 0);

                    //Jungle Clear Menu Settings
                    MainMenu._jungle.AddGroupLabel("JungleClear Settings");
                    MainMenu._jungle.AddCheckBox("jungle.e", "Kill jungle camps with E");
                    MainMenu._jungle.AddCheckBox("jungle.e.mini", "Kill mini jungle monsters with E", false);

                    //Harras Menu Settings
                    MainMenu.HarassKeys(true, false, false, false);
                    MainMenu._harass.AddGroupLabel("Harass Preferences", "harass.grouplabel.addonmenu", true);
                    MainMenu._harass.AddCheckBox("harass.e.enemy", "Harass enemy with E when minion can die", true, true);
                    MainMenu._harass.AddSeparator();
                    MainMenu._harass.AddGroupLabel("Mana Manager:", "harass.grouplabel.addonmenu.1", true);
                    MainMenu.HarassManaManager(true, false, false, false, 60, 0, 0, 0);

                    //Flee Menu
                    MainMenu._flee = MainMenu._menu.AddSubMenu("Flee Menu", "flee");
                    MainMenu._flee.AddGroupLabel("Flee Settings");
                    MainMenu._flee.AddCheckBox("flee.attack", "Attack champions/minions/monsters");
                    MainMenu._flee.AddCheckBox("flee.q.jump", "Jump walls with Q on jump spots");

                    //Killsteal Menu
                    MainMenu.KsKeys(false, false, true, false);

                    //Sentinel Manager Menu
                    _sentinel = MainMenu._menu.AddSubMenu("Sentinel Manager", "sentinel");
                    
                    _sentinel.AddGroupLabel("Sentinel Settings");
                    _sentinel.Add("sentinel.castDragon", new KeyBind("Send sentinel to Dragon", false, KeyBind.BindTypes.HoldActive, 'U'));
                    _sentinel.Add("sentinel.castBaron", new KeyBind("Send sentinel to Baron/Rift Herald", false, KeyBind.BindTypes.HoldActive, 'I'));

                    _sentinel.AddSeparator();
                    _sentinel.AddCheckBox("sentinel.enable", "Auto send sentinels", false);
                    _sentinel.AddCheckBox("sentinel.noMode", "Only when no modes are active");
                    _sentinel.AddCheckBox("sentinel.alert", "Alert when sentinel is taking damage");
                    _sentinel.AddSlider("sentinel.mana", "Minimum {0}% mana to auto send W", 40);

                    _sentinel.AddSeparator();
                    _sentinel.Add("sentinel.locationLabel", new Label("Send sentinels to:"));
                    (_sentinel.Add("sentinel.baron", new CheckBox("Baron / Rift Herald"))).OnValueChange += SentinelLocationsChanged;
                    (_sentinel.Add("sentinel.dragon", new CheckBox("Dragon"))).OnValueChange += SentinelLocationsChanged;
                    (_sentinel.Add("sentinel.mid", new CheckBox("Mid brush"))).OnValueChange += SentinelLocationsChanged;
                    (_sentinel.Add("sentinel.blue", new CheckBox("Blue"))).OnValueChange += SentinelLocationsChanged;
                    (_sentinel.Add("sentinel.red", new CheckBox("Red"))).OnValueChange += SentinelLocationsChanged;

                    //Misc Menu
                    MainMenu.MiscMenu();
                    MainMenu._misc.AddCheckBox("misc.junglestealE", "Junglesteal with E");
                    MainMenu._misc.AddSeparator();
                    MainMenu._misc.AddCheckBox("misc.autoE", "Auto use E");
                    MainMenu._misc.AddSlider("misc.autoEHealth", "Health below {0}% to auto use E", 10, 5, 25);
                    MainMenu._misc.AddSeparator();
                    MainMenu._misc.AddCheckBox("misc.unkillableE", "Kill unkillable minions with E");
                    MainMenu._misc.AddSeparator();
                    MainMenu._misc.AddCheckBox("misc.useR", "Use R to save ally");
                    MainMenu._misc.AddSlider("misc.healthR", "{0}% Health to save ally", 15, 5, 25);

                    //Balista Menu
                    if (EntityManager.Heroes.Allies.Any(x => x.ChampionName == "Blitzcrank"))
                    {
                        _balista = MainMenu._menu.AddSubMenu("Balista", "balista");
                        {
                            _balista.AddCheckBox("balista.comboOnly", "Only use Balista in combo mode");
                            _balista.AddSlider("balista.distance", "Minimum distance between you and Blitzcrank: {0}", 400, 0, 1200);
                            _balista.AddSeparator();
                            _balista.Add("balista.label", new Label("Use Balista for:"));
                            foreach (var enemy in EntityManager.Heroes.Enemies)
                            {
                                _balista.AddCheckBox("balista." + enemy.ChampionName, enemy.ChampionName);
                            }
                        }
                    }

                    //Draw Menu
                    MainMenu.DrawKeys(true, true, true, true);
                    MainMenu.DamageIndicator(true);

                    MainMenu._draw.AddSeparator();
                    MainMenu._draw.AddCheckBox("draw.percentage", "Draw E damage percentage enemy");
                    MainMenu._draw.AddCheckBox("draw.killableMinions", "Draw E killable minions");
                    MainMenu._draw.AddCheckBox("draw.stacks", "Draw E stacks enemy", false);
                    MainMenu._draw.AddCheckBox("draw.jumpSpots", "Draw jump spots");
                    if (EntityManager.Heroes.Allies.Any(x => x.ChampionName == "Blitzcrank"))
                        MainMenu._draw.AddCheckBox("draw.balista", "Draw Balista range");

                    Value.Init();
                    Value.MenuList.Add(_sentinel);
                    if (_balista != null) Value.MenuList.Add(_balista);

                    InitSentinelLocations();
                    RecalculateOpenLocations();

                    #endregion
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code MENU)</font>");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code 503)</font>");
            }

            try
            {
                DamageIndicator.DamageToUnit = GetActualDamage;
                if (MainMenu._menu["useonupdate"].Cast<EloBuddy.SDK.Menu.Values.CheckBox>().CurrentValue)
                {
                    Game.OnUpdate += GameOnUpdate;
                }
                else
                {
                    Game.OnTick += GameOnUpdate;
                }
                Drawing.OnDraw += OnDraw;
                Obj_AI_Base.OnBuffGain += Obj_AI_Base_OnBuffGain;
                Orbwalker.OnUnkillableMinion += OnUnkillableMinion;

                if (Game.MapId == GameMapId.SummonersRift)
                {
                    //Load walljump spots
                    InitSpots();

                    // Listen to required events
                    GameObject.OnCreate += OnCreate;

                    // Recalculate open sentinel locations
                    RecalculateOpenLocations();
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code INIT)</font>");
            }
        }

        private static void GameOnUpdate(EventArgs args)
        {
            if (Player.Instance.IsDead) return;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                _fleeActivated = true;
            }

            if (_fleeActivated && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                _fleeActivated = false;
                Orbwalker.DisableAttacking = false;
                Orbwalker.DisableMovement = false;
                Orbwalker.ForcedTarget = null;
            }

            PermaActive();
            SentinelTick();
        }

        #region Drawings
        private static void OnDraw(EventArgs args)
        {
            if (Value.Use("draw.disable") || Player.Instance.IsDead) return;

            if (Value.Use("draw.q"))
            {
                if (!(Value.Use("draw.ready") && !_q.IsReady()))
                {
                    new Circle
                    {
                        Radius = _q.Range,
                        Color = MainMenu._draw.GetColor("color.q"),
                        BorderWidth = MainMenu._draw.GetWidth("width.q")
                    }.Draw(Player.Instance.Position);
                }
            }

            if (Value.Use("draw.w"))
            {
                if (!(Value.Use("draw.ready") && !_w.IsReady()))
                {
                    new Circle
                    {
                        Radius = _w.Range,
                        Color = MainMenu._draw.GetColor("color.w"),
                        BorderWidth = MainMenu._draw.GetWidth("width.w")
                    }.Draw(Player.Instance.Position);
                }
            }

            if (Value.Use("draw.e"))
            {
                if (!(Value.Use("draw.ready") && !_e.IsReady()))
                {
                    new Circle
                    {
                        Radius = _e.Range,
                        Color = MainMenu._draw.GetColor("color.e"),
                        BorderWidth = MainMenu._draw.GetWidth("width.e")
                    }.Draw(Player.Instance.Position);
                }
            }

            if (Value.Use("draw.r"))
            {
                if (!(Value.Use("draw.ready") && !_r.IsReady()))
                {
                    new Circle
                    {
                        Radius = _r.Range,
                        Color = MainMenu._draw.GetColor("color.r"),
                        BorderWidth = MainMenu._draw.GetWidth("width.r")
                    }.Draw(Player.Instance.Position);
                }
            }

            if (Value.Use("draw.killableMinions"))
            {
                foreach (var killableMinion in
                    EntityManager.MinionsAndMonsters.GetLaneMinions(
                        EntityManager.UnitTeam.Enemy, Player.Instance.Position, _e.Range)
                        .Where(x => IsRendKillable(x)))
                {
                    Circle.Draw(Color.GreenYellow, 25f, 2f, killableMinion.Position);
                }
            }

            if (Value.Use("draw.percentage"))
            {
                foreach (var enemy in
                    EntityManager.Heroes.Enemies
                    .Where(x => Player.Instance.Distance(x) <= 2000f && !x.IsDead && x.IsVisible && HasRendBuff(x)))
                {
                    var percent = Math.Floor((GetActualDamage(enemy) / GetTotalHealth(enemy)) * 100);

                    if (percent >= 100 && !IsRendKillable(enemy))
                    {
                        Drawing.DrawText(enemy.HPBarPosition.X + 140, enemy.HPBarPosition.Y + 5, System.Drawing.Color.Red, "Can't kill!", 20);
                    }
                    else
                    {
                        Drawing.DrawText(enemy.HPBarPosition.X + 140, enemy.HPBarPosition.Y + 5, System.Drawing.Color.White,
                        IsRendKillable(enemy) ? "Killable!" : percent + "%", 20);
                    }
                }
            }

            if (Value.Use("draw.stacks"))
            {
                foreach (var enemy in
                    EntityManager.Heroes.Enemies
                    .Where(x => Player.Instance.Distance(x) <= 2000f && !x.IsDead && x.IsVisible && HasRendBuff(x)))
                {
                    var stacks = GetRendBuff(enemy).Count;
                    Drawing.DrawText(enemy.Position.X, enemy.Position.Y, System.Drawing.Color.White, "Stacks: " + stacks);
                }
            }

            if (Value.Use("draw.jumpSpots"))
            {
                foreach (var spot in JumpSpots.Where(s => Player.Instance.Distance(s[0]) <= 2000))
                {
                    Circle.Draw(Color.DarkGray, 30f, spot[0]);
                }
            }

            if (BalistaPossible && Value.Use("draw.balista"))
            {
                Circle.Draw(Color.Aqua, Value.Get("balista.distance"), Player.Instance.Position);
            }
        }
        #endregion

        #region Combo
        public override void Combo()
        {
            if (Value.Use("combo.q")
                && _q.IsReady()
                && !Player.Instance.IsDashing()
                && Player.Instance.ManaPercent > Value.Get("combo.q.mana"))
            {
                var target = TargetSelector.GetTarget(_q.Range, DamageType.Physical);
                if (target != null && target.IsValidTarget())
                {
                    _q.Cast(target);
                }
            }

            if (Value.Use("combo.e") && !Value.Use("killsteal.e"))
            {
                EnemyKill();
            }

            if (Value.Use("combo.gapcloser"))
            {
                var gapCloseTarget =
                    EntityManager.MinionsAndMonsters.CombinedAttackable
                        .FirstOrDefault(x => x.IsValidTarget(Player.Instance.GetAutoAttackRange()));

                Orbwalker.ForcedTarget = EntityManager.Heroes.Enemies.Any(x => Player.Instance.IsInAutoAttackRange(x)) ? null : gapCloseTarget;
            }

            if (Value.Use("combo.e.enemy"))
            {
                MinionHarass();
            }
        }
        #endregion

        #region Harass

        public override void Harass()
        {
            if (Value.Use("harass.q")
                && _q.IsReady()
                && !Player.Instance.IsDashing()
                && Player.Instance.ManaPercent > Value.Get("harass.q.mana"))
            {
                var target = TargetSelector.GetTarget(_q.Range, DamageType.Physical);
                if (target != null && target.IsValidTarget())
                {
                    _q.Cast(target);
                }
            }

            if (Value.Use("harass.e.enemy"))
            {
                MinionHarass();
            }
        }

        #endregion

        #region LaneClear

        public override void Laneclear()
        {
            if (Player.HasBuff("summonerexhaust")) return;

            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(
                EntityManager.UnitTeam.Enemy, Player.Instance.Position, _q.Range).ToArray();

            if (!minions.Any()) return;

            if (Value.Use("lane.q")
                && Player.Instance.ManaPercent > Value.Get("lane.q.mana"))
            {
                var qKillableMinions = minions.Where(x => GetTotalHealth(x) < GetQDamage(x)).ToArray();
                if (!qKillableMinions.Any()) return;

                var predictionResult =
                    (from minion in qKillableMinions
                     let pred = _q.GetPrediction(minion)
                     let count = pred.GetCollisionObjects<Obj_AI_Minion>().Count(x =>
                                    GetTotalHealth(x) < GetQDamage(x)
                                    && x.IsValidTarget() && x.IsEnemy)
                     where count >= Value.Get("lane.q.min")
                     where !Player.Instance.IsDashing()
                     select pred).FirstOrDefault();

                if (_q.IsReady() && predictionResult != null)
                {
                    _q.Cast(predictionResult.CastPosition);
                }
            }

            if (Value.Use("lane.e")
                && Player.Instance.ManaPercent > Value.Get("lane.e.mana")
                && _e.IsReady())
            {
                var count = minions.Count(x => IsRendKillable(x) && x.Health > 10);
                if (count >= Value.Get("lane.e.min"))
                {
                    _e.Cast();
                }
            }

            if (Value.Use("lane.e.enemy"))
            {
                MinionHarass();
            }
        }

        #endregion

        #region JungleClear

        public override void Jungleclear()
        {
            if (Value.Use("jungle.e")
                && !Value.Use("misc.junglestealE"))
            {
                JungleKill();
            }
        }

        #endregion

        #region Flee
        public override void Flee()
        {
            if (Value.Use("flee.q.jump") && Game.MapId == GameMapId.SummonersRift)
            {
                var spot = GetJumpSpot();
                if (spot != null && _q.IsReady())
                {
                    Orbwalker.DisableAttacking = true;
                    Orbwalker.DisableMovement = true;

                    JumpWall();
                    return;
                }
            }

            if (Value.Use("flee.attack"))
            {
                Orbwalker.DisableAttacking = false;
                Orbwalker.DisableMovement = false;

                var target =
                    ObjectManager.Get<Obj_AI_Base>()
                    .FirstOrDefault(
                        x =>
                            x.IsValidTarget(Player.Instance.GetAutoAttackRange())
                            && !x.IsMe
                            && !x.IsAlly);

                Orbwalker.ForcedTarget = target;
            }
        }
        #endregion

        #region WallJump Logic

        private static List<Vector3[]> JumpSpots = new List<Vector3[]>();
        private static float _lastMoveClick;
        private static float _lastDistance;
        private static float _currentDistance;

        private static Vector3[] GetJumpSpot()
        {
            return JumpSpots
                .Where(x => Player.Instance.Distance(x[0]) <= 150)
                .OrderBy(x => Player.Instance.Distance(x[0]))
                .FirstOrDefault();
        }

        private static void JumpWall()
        {
            var spot = GetJumpSpot();
            if (_q.IsReady()
                && spot != null
                && Environment.TickCount - _lastMoveClick > 100)
            {
                if (Player.Instance.Distance(spot[0]) <= 4)
                {
                    _q.Cast(spot[1]);
                    Player.IssueOrder(GameObjectOrder.MoveTo, spot[1]);
                    _lastMoveClick = Environment.TickCount;
                }
                else if (Player.Instance.Distance(spot[0]) > 4
                    && Player.Instance.Distance(spot[0]) < 60)
                {
                    _lastDistance = _currentDistance;
                    _currentDistance = Player.Instance.Distance(spot[0]);
                    if (_lastDistance == _currentDistance)
                    {
                        Player.IssueOrder(GameObjectOrder.MoveTo, spot[1]);
                        _lastMoveClick = Environment.TickCount;
                    }
                }
                else
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, spot[0]);
                    _lastMoveClick = Environment.TickCount;
                }
            }
        }

        #region WallJump Spots  - BIG LIST BE CAREFUL :D:D

        public static void InitSpots()
        {
            //blue side wolves - left wall (top)
            JumpSpots.Add(new[] { new Vector3(2848, 6942, 53), new Vector3(3058, 6960, 52) });
            JumpSpots.Add(new[] { new Vector3(3064, 6962, 52), new Vector3(2809, 6936, 53) });

            //blue side wolves - left wall (middle)
            JumpSpots.Add(new[] { new Vector3(2774, 6558, 57), new Vector3(3072, 6607, 51) });
            JumpSpots.Add(new[] { new Vector3(3074, 6608, 51), new Vector3(2755, 6523, 57) });

            //blue side wolves - left wall (bottom)
            JumpSpots.Add(new[] { new Vector3(3024, 6108, 57), new Vector3(3195, 6307, 52) });
            JumpSpots.Add(new[] { new Vector3(3200, 6243, 52), new Vector3(3022, 6111, 57) });

            //red side wolves - right wall (top)
            JumpSpots.Add(new[] { new Vector3(11772, 8856, 50), new Vector3(11513, 8762, 65) });
            JumpSpots.Add(new[] { new Vector3(11572, 8706, 64), new Vector3(11817, 8903, 50) });

            //red side wolves - right wall (middle)
            JumpSpots.Add(new[] { new Vector3(11772, 8206, 55), new Vector3(12095, 8281, 52) });
            JumpSpots.Add(new[] { new Vector3(12072, 8256, 52), new Vector3(11755, 8206, 55) });

            //red side wolves - right wall (bottom)
            JumpSpots.Add(new[] { new Vector3(11772, 7906, 52), new Vector3(12110, 7980, 53) });
            JumpSpots.Add(new[] { new Vector3(12072, 7906, 53), new Vector3(11767, 7900, 52) });

            //bottom bush wall (top)
            JumpSpots.Add(new[] { new Vector3(11410, 5526, 23), new Vector3(11647, 5452, 54) });
            JumpSpots.Add(new[] { new Vector3(11646, 5452, 54), new Vector3(11354, 5511, 8) });

            //bottom bush wall (middle)
            JumpSpots.Add(new[] { new Vector3(11722, 5058, 52), new Vector3(11345, 4813, -71) });
            JumpSpots.Add(new[] { new Vector3(11428, 4984, -71), new Vector3(11725, 5120, 52) });

            //bot bush wall (bottom)
            JumpSpots.Add(new[] { new Vector3(11772, 4608, -71), new Vector3(11960, 4802, 51) });
            JumpSpots.Add(new[] { new Vector3(11922, 4758, 51), new Vector3(11697, 4614, -71) });

            //top bush wall (top)
            JumpSpots.Add(new[] { new Vector3(3074, 10056, 54), new Vector3(3437, 10186, -66) });
            JumpSpots.Add(new[] { new Vector3(3324, 10206, -65), new Vector3(2964, 10012, 54) });

            //top bush wall (middle)
            JumpSpots.Add(new[] { new Vector3(3474, 9856, -65), new Vector3(3104, 9701, 52) });
            JumpSpots.Add(new[] { new Vector3(3226, 9752, 52), new Vector3(3519, 9833, -65) });

            //top bush wall (bottom)
            JumpSpots.Add(new[] { new Vector3(3488, 9414, 13), new Vector3(3224, 9440, 51) });
            JumpSpots.Add(new[] { new Vector3(3226, 9438, 51), new Vector3(3478, 9422, 16) });

            //mid wall - top side (top)
            JumpSpots.Add(new[] { new Vector3(6524, 8856, -71), new Vector3(6685, 9116, 49) });
            JumpSpots.Add(new[] { new Vector3(6664, 9002, 43), new Vector3(6484, 8804, -71) });

            //mid wall - top side (middle)
            JumpSpots.Add(new[] { new Vector3(6874, 8606, -69), new Vector3(7095, 8727, 52) });
            JumpSpots.Add(new[] { new Vector3(7074, 8706, 52), new Vector3(6857, 8517, -71) });

            //mid wall - top side (bottom)
            JumpSpots.Add(new[] { new Vector3(7174, 8256, -33), new Vector3(7456, 8539, 53) });
            JumpSpots.Add(new[] { new Vector3(7422, 8406, 53), new Vector3(7100, 8159, -24) });

            //mid wall - bot side (top)
            JumpSpots.Add(new[] { new Vector3(7658, 6512, 5), new Vector3(7378, 6298, 52) });
            JumpSpots.Add(new[] { new Vector3(7470, 6260, 52), new Vector3(7714, 6544, -1) });

            //mid wall - bot side (middle)
            JumpSpots.Add(new[] { new Vector3(8034, 6198, -71), new Vector3(7813, 5938, 52) });
            JumpSpots.Add(new[] { new Vector3(7898, 6004, 51), new Vector3(8139, 6210, -71) });

            //mid wall - bot side (bottom)
            JumpSpots.Add(new[] { new Vector3(8222, 5808, 32), new Vector3(8412, 6081, -71) });
            JumpSpots.Add(new[] { new Vector3(8344, 6022, -71), new Vector3(8194, 5742, 42) });

            //baron wall
            JumpSpots.Add(new[] { new Vector3(5774, 10656, 55), new Vector3(5355, 10657, -71) });
            JumpSpots.Add(new[] { new Vector3(5474, 10656, -71), new Vector3(5812, 10832, 55) });

            //baron entrance wall (left)
            JumpSpots.Add(new[] { new Vector3(4474, 10406, -71), new Vector3(4292, 10199, -71) });
            JumpSpots.Add(new[] { new Vector3(4292, 10270, -71), new Vector3(4480, 10437, -71) });

            //baron entrance wall (right)
            JumpSpots.Add(new[] { new Vector3(5074, 10006, -71), new Vector3(4993, 9706, -70) });
            JumpSpots.Add(new[] { new Vector3(5000, 9754, -71), new Vector3(5083, 9998, -71) });

            //dragon wall
            JumpSpots.Add(new[] { new Vector3(9322, 4358, -71), new Vector3(8971, 4284, 52) });
            JumpSpots.Add(new[] { new Vector3(9072, 4208, 53), new Vector3(9378, 4431, -71) });

            //dragon entrance wall (left)
            JumpSpots.Add(new[] { new Vector3(9812, 4918, -71), new Vector3(9803, 5249, -68) });
            JumpSpots.Add(new[] { new Vector3(9822, 5158, -71), new Vector3(9751, 4884, -71) });

            //dragon entrance wall (right)
            JumpSpots.Add(new[] { new Vector3(10422, 4458, -71), new Vector3(10643, 4641, -68) });
            JumpSpots.Add(new[] { new Vector3(10622, 4558, -71), new Vector3(10375, 4441, -71) });

            //top golllems wall
            JumpSpots.Add(new[] { new Vector3(6524, 12006, 56), new Vector3(6553, 11666, 53) });
            JumpSpots.Add(new[] { new Vector3(6574, 11706, 53), new Vector3(6543, 12054, 56) });

            //bot gollems wall
            JumpSpots.Add(new[] { new Vector3(8250, 2894, 51), new Vector3(8213, 3326, 51) });
            JumpSpots.Add(new[] { new Vector3(8222, 3158, 51), new Vector3(8282, 2741, 51) });

            //blue side bot tribush wall (left)
            JumpSpots.Add(new[] { new Vector3(9482, 2786, 49), new Vector3(9535, 3203, 55) });
            JumpSpots.Add(new[] { new Vector3(9530, 3126, 59), new Vector3(9505, 2756, 49) });

            //blue side bot tribush wall (middle)
            JumpSpots.Add(new[] { new Vector3(9772, 2758, 49), new Vector3(9862, 3111, 58) });
            JumpSpots.Add(new[] { new Vector3(9872, 3066, 58), new Vector3(9815, 2673, 49) });

            //blue side bot tribush wall (right)
            JumpSpots.Add(new[] { new Vector3(10206, 2888, 49), new Vector3(10046, 2675, 49) });
            JumpSpots.Add(new[] { new Vector3(10022, 2658, 49), new Vector3(10259, 2925, 49) });

            //red side toplane tribush wall (right)
            JumpSpots.Add(new[] { new Vector3(5274, 11806, 57), new Vector3(5363, 12185, 56) });
            JumpSpots.Add(new[] { new Vector3(5324, 12106, 56), new Vector3(5269, 11725, 57) });

            //red side toplane tribush wall (middle)
            JumpSpots.Add(new[] { new Vector3(5000, 11874, 57), new Vector3(5110, 12210, 56) });
            JumpSpots.Add(new[] { new Vector3(5072, 12146, 56), new Vector3(4993, 11836, 57) });

            //red side toplane tribush wall (left)
            JumpSpots.Add(new[] { new Vector3(4624, 12006, 57), new Vector3(4825, 12307, 56) });
            JumpSpots.Add(new[] { new Vector3(4776, 12224, 56), new Vector3(4605, 11970, 57) });

            //blue side razorbeak wall
            JumpSpots.Add(new[] { new Vector3(7372, 5858, 52), new Vector3(7115, 5524, 55) });
            JumpSpots.Add(new[] { new Vector3(7174, 5608, 58), new Vector3(7424, 5905, 52) });

            //blue side blue buff wall (right)
            JumpSpots.Add(new[] { new Vector3(3774, 7706, 52), new Vector3(3856, 7412, 51) });
            JumpSpots.Add(new[] { new Vector3(3828, 7428, 51), new Vector3(3802, 7743, 52) });

            //blue side blue buff wall (left)
            JumpSpots.Add(new[] { new Vector3(3424, 7408, 52), new Vector3(3422, 7759, 53) });
            JumpSpots.Add(new[] { new Vector3(3434, 7722, 52), new Vector3(3437, 7398, 52) });

            //blue side blue buff - right wall
            JumpSpots.Add(new[] { new Vector3(4144, 8030, 50), new Vector3(4382, 8149, 48) });
            JumpSpots.Add(new[] { new Vector3(4374, 8156, 48), new Vector3(4124, 8022, 50) });

            //blue side rock between blue buff/baron (left)
            JumpSpots.Add(new[] { new Vector3(4664, 8652, -10), new Vector3(4624, 9010, -68) });
            JumpSpots.Add(new[] { new Vector3(4662, 8896, -69), new Vector3(4672, 8519, 26) });

            //blue side rock between blue buff/baron (right)
            JumpSpots.Add(new[] { new Vector3(3774, 9206, -14), new Vector3(4074, 9322, -67) });
            JumpSpots.Add(new[] { new Vector3(4024, 9306, -68), new Vector3(3737, 9233, -8) });

            //red side blue buff wall (left)
            JumpSpots.Add(new[] { new Vector3(11022, 7208, 51), new Vector3(10904, 7521, 52) });
            JumpSpots.Add(new[] { new Vector3(11022, 7506, 52), new Vector3(11040, 7179, 51) });

            //red side blue buff wall (right)
            JumpSpots.Add(new[] { new Vector3(11440, 7208, 52), new Vector3(11449, 7517, 52) });
            JumpSpots.Add(new[] { new Vector3(11470, 7486, 52), new Vector3(11458, 7155, 52) });

            //red side rock between blue buff/dragon (left)
            JumpSpots.Add(new[] { new Vector3(10172, 6208, 16), new Vector3(10189, 5922, -71) });
            JumpSpots.Add(new[] { new Vector3(10172, 5958, -71), new Vector3(10185, 6286, 29) });

            //red side rock between blue buff/dragon (right)
            JumpSpots.Add(new[] { new Vector3(10722, 5658, -66), new Vector3(11049, 5660, -22) });
            JumpSpots.Add(new[] { new Vector3(11022, 5658, -30), new Vector3(10665, 5662, -68) });

            //blue side top tribush wall (top)
            JumpSpots.Add(new[] { new Vector3(2574, 9656, 54), new Vector3(2800, 9596, 52) });
            JumpSpots.Add(new[] { new Vector3(2774, 9656, 53), new Vector3(2537, 9674, 54) });

            //blue side top tribush wall (bottom)
            JumpSpots.Add(new[] { new Vector3(2874, 9306, 51), new Vector3(2500, 9262, 52) });
            JumpSpots.Add(new[] { new Vector3(2598, 9272, 52), new Vector3(2884, 9291, 51) });

            //blue side wolves - right wall (bottom)
            JumpSpots.Add(new[] { new Vector3(4624, 5858, 51), new Vector3(4772, 5636, 50) });
            JumpSpots.Add(new[] { new Vector3(4774, 5658, 50), new Vector3(4644, 5876, 51) });

            //blue side wolves - right wall (top)
            JumpSpots.Add(new[] { new Vector3(4924, 6158, 52), new Vector3(4869, 6452, 51) });
            JumpSpots.Add(new[] { new Vector3(4874, 6408, 51), new Vector3(4938, 6062, 51) });

            //blue razorbeak - left wall
            JumpSpots.Add(new[] { new Vector3(6174, 5308, 49), new Vector3(5998, 5536, 52) });
            JumpSpots.Add(new[] { new Vector3(6024, 5508, 52), new Vector3(6199, 5286, 49) });

            //red side bottom tribush wall (bottom)
            JumpSpots.Add(new[] { new Vector3(12260, 5220, 52), new Vector3(12027, 5265, 54) });
            JumpSpots.Add(new[] { new Vector3(12122, 5208, 54), new Vector3(12327, 5243, 52) });

            //red side bottom tribush wall (top)
            JumpSpots.Add(new[] { new Vector3(11972, 5558, 54), new Vector3(12343, 5498, 53) });
            JumpSpots.Add(new[] { new Vector3(12272, 5558, 53), new Vector3(11969, 5480, 55) });

            //red side razorbeak - rightdown wall
            JumpSpots.Add(new[] { new Vector3(8672, 9606, 50), new Vector3(8831, 9384, 52) });
            JumpSpots.Add(new[] { new Vector3(8830, 9382, 52), new Vector3(8646, 9635, 50) });

            //red side wolves - left wall (top)
            JumpSpots.Add(new[] { new Vector3(10222, 9056, 50), new Vector3(10061, 9282, 52) });
            JumpSpots.Add(new[] { new Vector3(10072, 9306, 52), new Vector3(10193, 9052, 50) });

            //red side wolves - left wall (bottom)
            JumpSpots.Add(new[] { new Vector3(9972, 8506, 68), new Vector3(9856, 8831, 50) });
            JumpSpots.Add(new[] { new Vector3(9872, 8756, 50), new Vector3(9967, 8429, 65) });

            //red size razorbeak - right wall
            JumpSpots.Add(new[] { new Vector3(8072, 9806, 51), new Vector3(8369, 9807, 50) });
            JumpSpots.Add(new[] { new Vector3(8372, 9806, 50), new Vector3(8066, 9796, 51) });

            //blue side base wall (right)
            JumpSpots.Add(new[] { new Vector3(4524, 3258, 96), new Vector3(4780, 3460, 51) });
            JumpSpots.Add(new[] { new Vector3(4774, 3408, 51), new Vector3(4463, 3260, 96) });

            //blue side base wall (left)
            JumpSpots.Add(new[] { new Vector3(3074, 4558, 96), new Vector3(3182, 4917, 54) });
            JumpSpots.Add(new[] { new Vector3(3174, 4858, 54), new Vector3(3085, 4539, 96) });

            //red side base wall (right)
            JumpSpots.Add(new[] { new Vector3(11712, 10390, 91), new Vector3(11621, 10092, 52) });
            JumpSpots.Add(new[] { new Vector3(11622, 10106, 52), new Vector3(11735, 10430, 91) });

            //red base wall (left)
            JumpSpots.Add(new[] { new Vector3(10308, 11682, 91), new Vector3(9999, 11554, 52) });
            JumpSpots.Add(new[] { new Vector3(10022, 11556, 52), new Vector3(10321, 11664, 91) });
        }

        #endregion

        #endregion

        #region PermaActive

        private static void PermaActive()
        {
            if (Value.Use("killsteal.e"))
            {
                EnemyKill();
            }

            if (Value.Use("misc.junglestealE"))
            {
                JungleKill();
            }

            if (Value.Use("misc.useR") && _r.IsReady())
            {
                if (Soulbound == null) return;

                if (Soulbound.HealthPercent <= Value.Get("misc.healthR")
                    && Soulbound.IsValidTarget(_r.Range)
                    && Soulbound.CountEnemiesInRange(500) > 0)
                {
                    _r.Cast();
                }
            }

            if (Value.Use("misc.autoE") && _e.IsReady())
            {
                if (Player.Instance.HealthPercent < Value.Get("misc.autoEHealth")
                    && EntityManager.Heroes.Enemies.Any(x => x.IsValidTarget(_e.Range) && HasRendBuff(x)))
                {
                    _e.Cast();
                }
            }
        }

        #endregion

        #region Multiple Mode Logic
        private static void EnemyKill()
        {
            if (!_e.IsReady()) return;

            if (EntityManager.Heroes.Enemies.Any(x => IsRendKillable(x)))
            {
                _e.Cast();
            }
        }

        private static void JungleKill()
        {
            if (!_e.IsReady()) return;

            var minions = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, _e.Range).ToArray();
            if (!minions.Any()) return;

            if (!Value.Use("jungle.e.mini"))
            {
                if (minions.Any(x => IsRendKillable(x) && !x.Name.Contains("Mini")))
                {
                    _e.Cast();
                }
            }

            else if (minions.Any(x => IsRendKillable(x)))
            {
                _e.Cast();
            }
        }

        private static void MinionHarass()
        {
            if (!_e.IsReady()
                || Player.HasBuff("summonerexhaust")
                || (Player.Instance.Mana - 40) < 40)
            {
                return;
            }

            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions(
                            EntityManager.UnitTeam.Enemy, Player.Instance.Position, _e.Range)
                            .Any(x => IsRendKillable(x) && x.Health > 10);

            if (!minion) return;

            var hero = EntityManager.Heroes.Enemies.Any(
                x =>
                    HasRendBuff(x)
                    && x.IsValidTarget(_e.Range)
                    && !HasSpellShield(x)
                    && !HasUndyingBuff(x));

            if (hero) _e.Cast();
        }
        #endregion

        #region Balista

        private static void Obj_AI_Base_OnBuffGain(Obj_AI_Base target, Obj_AI_BaseBuffGainEventArgs args)
        {
            if (!BalistaPossible) return;

            if (args.Buff.DisplayName == "RocketGrab" && target.IsEnemy && _r.IsReady())
            {
                var hero = target as AIHeroClient;
                if (hero == null
                    || !Value.Use("balista." + hero.ChampionName)
                    || (Value.Use("balista.comboOnly") && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)))
                {
                    return;
                }

                if (hero.IsValidTarget()
                    && Player.Instance.Distance(Soulbound) >= Value.Get("balista.distance")
                    && _r.IsInRange(Soulbound))
                {
                    _r.Cast();
                }
            }
        }

        #endregion

        #region UnkillableMinion

        private static void OnUnkillableMinion(Obj_AI_Base unit, Orbwalker.UnkillableMinionArgs args)
        {
            if (!_e.IsReady()
                || Player.HasBuff("summonerexhaust")
                || (Player.Instance.Mana - 40) < 40)
            {
                return;
            }

            if (Value.Use("misc.unkillableE") && IsRendKillable(unit))
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)
                    || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                {
                    _e.Cast();
                }
            }
        }

        #endregion

        #region Sentinel Logic
        private static void SentinelLocationsChanged(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            RecalculateOpenLocations();
        }

        private enum SentinelLocations
        {
            Baron,
            Dragon,
            Mid,
            Blue,
            Red
        }

        private const float MaxRandomRadius = 15;
        private static readonly Random Random = new Random(DateTime.Now.Millisecond);
        private static readonly Dictionary<GameObjectTeam, Dictionary<SentinelLocations, Vector2>> Locations = new Dictionary<GameObjectTeam, Dictionary<SentinelLocations, Vector2>>
        {
            {
                GameObjectTeam.Order, new Dictionary<SentinelLocations, Vector2>
                {
                    { SentinelLocations.Mid, new Vector2(8428, 6465) },
                    { SentinelLocations.Blue, new Vector2(3871.489f, 7901.054f) },
                    { SentinelLocations.Red, new Vector2(7862.244f, 4111.187f) }
                }
            },
            {
                GameObjectTeam.Chaos, new Dictionary<SentinelLocations, Vector2>
                {
                    { SentinelLocations.Mid, new Vector2(6545, 8361) },
                    { SentinelLocations.Blue, new Vector2(10931.73f, 6990.844f) },
                    { SentinelLocations.Red, new Vector2(7016.869f, 10775.55f) }
                }
            },
            {
                GameObjectTeam.Neutral, new Dictionary<SentinelLocations, Vector2>
                {
                    { SentinelLocations.Baron, new Vector2(5007.124f, 10471.45f) },
                    { SentinelLocations.Dragon, new Vector2(9866.148f, 4414.014f) }
                }
            }
        };

        private static readonly Dictionary<SentinelLocations, Func<bool>> EnabledLocations = new Dictionary<SentinelLocations, Func<bool>>();

        private static void InitSentinelLocations()
        {
            EnabledLocations.Add(SentinelLocations.Baron, () => Value.Use("sentinel.baron"));
            EnabledLocations.Add(SentinelLocations.Dragon, () => Value.Use("sentinel.dragon"));
            EnabledLocations.Add(SentinelLocations.Mid, () => Value.Use("sentinel.mid"));
            EnabledLocations.Add(SentinelLocations.Blue, () => Value.Use("sentinel.blue"));
            EnabledLocations.Add(SentinelLocations.Red, () => Value.Use("sentinel.red"));
        }

        private static readonly List<Tuple<GameObjectTeam, SentinelLocations>> OpenLocations = new List<Tuple<GameObjectTeam, SentinelLocations>>();
        private static readonly Dictionary<GameObjectTeam, Dictionary<SentinelLocations, Obj_AI_Base>> ActiveSentinels = new Dictionary<GameObjectTeam, Dictionary<SentinelLocations, Obj_AI_Base>>();
        private static Tuple<GameObjectTeam, SentinelLocations> SentLocation { get; set; }

        public static void RecalculateOpenLocations()
        {
            OpenLocations.Clear();
            foreach (var location in Locations)
            {
                if (!ActiveSentinels.ContainsKey(location.Key))
                {
                    OpenLocations.AddRange(location.Value.Where(o => EnabledLocations[o.Key]()).Select(loc => new Tuple<GameObjectTeam, SentinelLocations>(location.Key, loc.Key)));
                }
                else
                {
                    OpenLocations.AddRange(from loc in location.Value
                                           where EnabledLocations[loc.Key]() && !ActiveSentinels[location.Key].ContainsKey(loc.Key)
                                           select new Tuple<GameObjectTeam, SentinelLocations>(location.Key, loc.Key));
                }
            }
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (SentLocation == null)
            {
                return;
            }

            var sentinel = sender as Obj_AI_Minion;
            if (sentinel != null && sentinel.IsAlly && sentinel.MaxHealth == 2 && sentinel.Name == "RobotBuddy")
            {
                Core.DelayAction(() => ValidateSentinel(sentinel), 1000);
            }
        }

        private static void ValidateSentinel(Obj_AI_Base sentinel)
        {
            if (sentinel.Health == 2 && sentinel.GetBuffCount("kalistaw") == 1)
            {
                if (!ActiveSentinels.ContainsKey(SentLocation.Item1))
                {
                    ActiveSentinels.Add(SentLocation.Item1, new Dictionary<SentinelLocations, Obj_AI_Base>());
                }
                ActiveSentinels[SentLocation.Item1].Remove(SentLocation.Item2);
                ActiveSentinels[SentLocation.Item1].Add(SentLocation.Item2, sentinel);

                SentLocation = null;
                RecalculateOpenLocations();
            }
        }

        private static void SentinelTick()
        {
            if (Value.Active("sentinel.castBaron"))
            {
                CastW(Baron);
            }
            if (Value.Active("sentinel.castDragon"))
            {
                CastW(Dragon);
            }

            // Validate all sentinels
            foreach (var entry in ActiveSentinels.ToArray())
            {
                if (Value.Use("sentinel.alert") && entry.Value.Any(o => o.Value.Health == 1))
                {
                    var activeSentinel = entry.Value.First(o => o.Value.Health == 1);
                    Chat.Print("[Kalista] Sentinel at {0} taking damage! (local ping)",
                        string.Concat((entry.Key == GameObjectTeam.Order
                            ? "Blue-Jungle"
                            : entry.Key == GameObjectTeam.Chaos
                                ? "Red-Jungle"
                                : "Lake"), " (", activeSentinel.Key, ")"));
                    TacticalMap.ShowPing(PingCategory.Fallback, activeSentinel.Value.Position, true);
                }

                var invalid = entry.Value.Where(o => !o.Value.IsValid || o.Value.Health < 2 || o.Value.GetBuffCount("kalistaw") == 0).ToArray();
                if (invalid.Length > 0)
                {
                    foreach (var location in invalid)
                    {
                        ActiveSentinels[entry.Key].Remove(location.Key);
                    }
                    RecalculateOpenLocations();
                }
            }

            // Auto sentinel management
            if (Value.Use("sentinel.enable") && _w.IsReady() && Player.Instance.ManaPercent >= Value.Get("sentinel.mana") && !Player.Instance.IsRecalling())
            {
                if (!Value.Use("sentinel.noMode") || Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.None)
                {
                    if (OpenLocations.Count > 0 && SentLocation == null)
                    {
                        var closestLocation = OpenLocations.Where(o => Locations[o.Item1][o.Item2].IsInRange(Player.Instance, _w.Range - MaxRandomRadius / 2))
                            .OrderByDescending(o => Locations[o.Item1][o.Item2].Distance(Player.Instance, true))
                            .FirstOrDefault();
                        if (closestLocation != null)
                        {
                            var position = Locations[closestLocation.Item1][closestLocation.Item2];
                            var randomized = (new Vector2(position.X - MaxRandomRadius / 2 + Random.NextFloat(0, MaxRandomRadius),
                                position.Y - MaxRandomRadius / 2 + Random.NextFloat(0, MaxRandomRadius))).To3DWorld();
                            SentLocation = closestLocation;
                            _w.Cast(randomized);
                            Core.DelayAction(() => SentLocation = null, 2000);
                        }
                    }
                }
            }
        }

        private static void CastW(Vector2 location)
        {
            if (!_w.IsReady()) return;

            if (Player.Instance.Distance(location) <= _w.Range)
            {
                _w.Cast(location.To3DWorld());
            }
        }

        #endregion

        #region Damages

        private static readonly float[] RawRendDamage = { 20, 30, 40, 50, 60 };
        private static readonly float[] RawRendDamageMultiplier = { 0.6f, 0.6f, 0.6f, 0.6f, 0.6f };
        private static readonly float[] RawRendDamagePerSpear = { 10, 14, 19, 25, 32 };
        private static readonly float[] RawRendDamagePerSpearMultiplier = { 0.2f, 0.225f, 0.25f, 0.275f, 0.3f };

        private static float GetQDamage(Obj_AI_Base target)
        {
            if (!_q.IsReady()) return 0f;

            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical,
                    new float[] { 10, 70, 130, 190, 250 }[_q.Level - 1]
                    + 1f * Player.Instance.TotalAttackDamage);
        }

        private static float GetRendDamage(Obj_AI_Base target)
        {
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, GetRawRendDamage(target)) *
                   (Player.Instance.HasBuff("summonerexhaust") ? 0.6f : 1);
        }

        private static float GetRawRendDamage(Obj_AI_Base target)
        {
            var stacks = (HasRendBuff(target) ? GetRendBuff(target).Count : 0) - 1;
            if (stacks > -1)
            {
                var index = _e.Level - 1;
                return RawRendDamage[index] + stacks * RawRendDamagePerSpear[index] +
                       Player.Instance.TotalAttackDamage * (RawRendDamageMultiplier[index] + stacks * RawRendDamagePerSpearMultiplier[index]);
            }

            return 0;
        }

        private static float GetActualDamage(Obj_AI_Base target)
        {
            if (!_e.IsReady() || !HasRendBuff(target)) return 0f;

            var damage = GetRendDamage(target);

            if (target.Name.Contains("Baron"))
            {
                // Buff Name: barontarget or barondebuff
                // Baron's Gaze: Baron Nashor takes 50% reduced damage from champions he's damaged in the last 15 seconds. 
                damage = Player.Instance.HasBuff("barontarget")
                    ? damage * 0.5f
                    : damage;
            }

            else if (target.Name.Contains("Dragon"))
            {
                // DragonSlayer: Reduces damage dealt by 7% per a stack
                damage = Player.Instance.HasBuff("s5test_dragonslayerbuff")
                    ? damage * (1 - (.07f * Player.Instance.GetBuffCount("s5test_dragonslayerbuff")))
                    : damage;
            }

            if (Player.Instance.HasBuff("summonerexhaust"))
            {
                damage = damage * 0.6f;
            }

            if (target.HasBuff("FerociousHowl"))
            {
                damage = damage * 0.7f;
            }

            return damage;
        }

        #endregion

        #region Utility Checks
        private static bool HasRendBuff(Obj_AI_Base target)
        {
            return GetRendBuff(target) != null;
        }

        private static BuffInstance GetRendBuff(Obj_AI_Base target)
        {
            return target.Buffs.Find(b => b.Caster.IsMe && b.IsValid() && b.DisplayName == "KalistaExpungeMarker");
        }

        private static bool IsRendKillable(Obj_AI_Base target)
        {
            if (target == null
                || !target.IsValidTarget(_e.Range + 200)
                || !HasRendBuff(target)
                || target.Health <= 0
                || !_e.IsReady())
            {
                return false;
            }

            var hero = target as AIHeroClient;
            if (hero != null)
            {
                if (HasUndyingBuff(hero) || HasSpellShield(hero))
                {
                    return false;
                }

                if (hero.ChampionName == "Blitzcrank")
                {
                    if (!hero.HasBuff("BlitzcrankManaBarrierCD") && !hero.HasBuff("ManaBarrier"))
                    {
                        return GetActualDamage(target) > (GetTotalHealth(target) + (hero.Mana / 2));
                    }

                    if (hero.HasBuff("ManaBarrier") && !(hero.AllShield > 0))
                    {
                        return false;
                    }
                }
            }

            return GetActualDamage(target) > GetTotalHealth(target);
        }

        private static bool HasUndyingBuff(AIHeroClient target)
        {
            // Tryndamere R
            if (target.Buffs.Any(b => b.IsValid() && b.DisplayName == "UndyingRage"))
            {
                return true;
            }

            // Zilean R
            if (target.Buffs.Any(b => b.IsValid() && b.DisplayName == "ChronoShift"))
            {
                return true;
            }

            // Kayle R
            if (target.Buffs.Any(b => b.IsValid() && b.DisplayName == "JudicatorIntervention"))
            {
                return true;
            }

            // Poppy R
            if (target.ChampionName == "Poppy")
            {
                if (
                    EntityManager.Heroes.Allies.Any(
                        o =>
                        !o.IsMe
                        && o.Buffs.Any(
                            b =>
                            b.Caster.NetworkId == target.NetworkId && b.IsValid()
                            && b.DisplayName == "PoppyDITarget")))
                {
                    return true;
                }
            }

            //Kindred R
            if (target.Buffs.Any(b => b.IsValid() && b.DisplayName == "kindredrnodeathbuff"))
            {
                return true;
            }

            if (target.HasBuffOfType(BuffType.Invulnerability))
            {
                return true;
            }

            return target.IsInvulnerable;
        }

        private static bool HasSpellShield(AIHeroClient target)
        {
            //Banshee's Veil
            if (target.Buffs.Any(b => b.IsValid() && b.DisplayName == "bansheesveil"))
            {
                return true;
            }

            //Sivir E
            if (target.Buffs.Any(b => b.IsValid() && b.DisplayName == "SivirE"))
            {
                return true;
            }

            //Nocturne W
            if (target.Buffs.Any(b => b.IsValid() && b.DisplayName == "NocturneW"))
            {
                return true;
            }

            //Other spellshields
            return target.HasBuffOfType(BuffType.SpellShield) || target.HasBuffOfType(BuffType.SpellImmunity);
        }

        private static float GetTotalHealth(Obj_AI_Base target)
        {
            return target.Health + target.AllShield + target.AttackShield + target.MagicShield + (target.HPRegenRate * 2);
        }
        #endregion
    }
}
