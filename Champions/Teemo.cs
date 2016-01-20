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
using SharpDX;
using Color = SharpDX.Color;
using MainMenu = OKTRAIO.Menu_Settings.MainMenu;

namespace OKTRAIO.Champions
{
    class Teemo : AIOChampion
    {
        #region Initialize and Declare

        private static Spell.Targeted _q;
        private static Spell.Active _w, _e;
        private static Spell.Skillshot _r;
        private static readonly int[] Rranges = {300, 600, 900};
        private static bool _rDelay;
        private static readonly Vector2 Offset = new Vector2(1, 0);

        public static Menu Mainmenu
        {
            get { return MainMenu._combo; }
        }

        public override void Init()
        {
            try
            {
                //spells
                _q = new Spell.Targeted(SpellSlot.Q, 680);
                _w = new Spell.Active(SpellSlot.W);
                _e = new Spell.Active(SpellSlot.E);
                _r = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Circular, 250, 1000, 120)
                {
                    AllowedCollisionCount = int.MaxValue
                };

                //menu

                //combo
                MainMenu.ComboKeys(true, true, false, true);
                MainMenu._combo.AddSeparator();
                MainMenu._combo.AddSlider("combo.w.distance", "Use W when enemy is in {0} range", 600, 1, 1200, true);
                MainMenu._combo.AddSlider("combo.r.stacks", "Keep shrooms at {0} stacks", 1, 0, 3, true);
                MainMenu._combo.AddGroupLabel("Prediction", "combo.grouplabel.addonmenu", true);
                MainMenu._combo.AddSlider("combo.r.prediction", "Hitchance Percentage for R", 80, 0, 100, true);
                if (EntityManager.Heroes.Enemies.Count > 0)
                {
                    var addedChamps = new List<string>();
                    foreach (var enemy in EntityManager.Heroes.Enemies.Where(enemy => !addedChamps.Contains(enemy.ChampionName)))
                    {
                        addedChamps.Add(enemy.ChampionName);
                        MainMenu._combo.Add(enemy.ChampionName, new CheckBox(string.Format("{0}", enemy.ChampionName)));
                    }
                }

                //flee
                MainMenu.FleeKeys(true, true, false, true);
                MainMenu._flee.AddSeparator();
                MainMenu._flee.AddSlider("flee.r.stacks", "Keep shrooms at {0} stacks", 1, 0, 3, true);
                MainMenu._flee.AddGroupLabel("Mana Manager:", "flee.grouplabel.addonmenu", true);
                MainMenu.FleeManaManager(true, true, false, true, 20, 20, 0, 20);

                //lasthit
                MainMenu.LastHitKeys(true, false, false, false);
                MainMenu._lasthit.AddSeparator();
                MainMenu._lasthit.AddGroupLabel("Mana Manager:", "lasthit.grouplabel.addonmenu", true);
                MainMenu.LasthitManaManager(true, false, false, false, 20, 0, 0, 0);

                //laneclear
                MainMenu.LaneKeys(true, false, false, true);
                MainMenu._lane.AddSeparator();
                MainMenu._lane.AddSlider("lane.r.min", "Min. {0} minions for R", 3, 1, 7, true);
                MainMenu._lane.AddSlider("lane.r.stacks", "Keep shrooms at {0} stacks", 1, 0, 3, true);
                MainMenu._lane.AddGroupLabel("Mana Manager:", "lane.grouplabel.addonmenu", true);
                MainMenu.LaneManaManager(true, false, false, true, 60, 0, 0, 60);

                //jungleclear
                MainMenu.JungleKeys(true, false, false, true);
                MainMenu._jungle.AddSeparator();
                MainMenu._jungle.AddSlider("jungle.r.min", "Min. {0} minions for R", 3, 1, 7, true);
                MainMenu._jungle.AddSlider("jungle.r.stacks", "Keep shrooms at {0} stacks", 1, 0, 3, true);
                MainMenu._jungle.AddGroupLabel("Mana Manager:", "jungle.grouplabel.addonmenu", true);
                MainMenu.JungleManaManager(true, false, false, true, 60, 0, 0, 60);

                //harass
                MainMenu.HarassKeys(true, false, false, false);
                MainMenu._harass.AddSeparator();
                MainMenu._harass.AddGroupLabel("Mana Manager:", "harass.grouplabel.addonmenu", true);
                MainMenu.HarassManaManager(true, false, false, false, 60, 0, 0, 0);

                //Ks
                MainMenu.KsKeys(true, false, false, false);
                MainMenu._ks.AddSeparator();
                MainMenu._ks.AddGroupLabel("Mana Manager:", "killsteal.grouplabel.addonmenu", true);
                MainMenu.KsManaManager(true, false, false, false, 20, 0, 0, 0);

                //misc
                MainMenu.MiscMenu();
                MainMenu._misc.Add("misc.r.auto", new KeyBind("Auto shroom locations", true, KeyBind.BindTypes.PressToggle, 'H'));
                MainMenu._misc.AddSlider("misc.r.stacks", "Keep shrooms at {0} stacks", 1, 0, 3);
                MainMenu._misc.AddCheckBox("misc.q.gapcloser", "Use Q on gapcloser");
                MainMenu._misc.AddSeparator();
                MainMenu._misc.AddGroupLabel("Auto Q/R Settings", "misc.grouplabel1.addonmenu", true);
                MainMenu._misc.AddSeparator();
                MainMenu._misc.AddCheckBox("misc.q.charm", "Use Q on Charmed Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.stun", "Use Q on Stunned Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.knockup", "Use Q on Knocked Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.snare", "Use Q on Snared Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.suppression", "Use Q on Suppressed Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.taunt", "Use Q on Taunted Enemy", true, true);
                MainMenu._misc.AddSeparator();
                MainMenu._misc.AddCheckBox("misc.r.charm", "Use R on Charmed Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.r.stun", "Use R on Stunned Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.r.knockup", "Use R on Knocked Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.r.snare", "Use R on Snared Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.r.suppression", "Use R on Suppressed Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.r.taunt", "Use R on Taunted Enemy", true, true);

                //draw
                MainMenu.DrawKeys(true, false, false, true);
                MainMenu._draw.AddSeparator();
                MainMenu._draw.AddCheckBox("draw.hp.bar", "Draw Combo Damage", true, true);
                MainMenu._draw.AddCheckBox("draw.r.auto", "Draw Auto Shroom locations", true, true);
                MainMenu._draw.AddCheckBox("draw.status", "Draw Auto Shroom status", true, true);
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code MENU)</font>");
            }

            try
            {
                Value.Init();
                if (MainMenu._menu["useonupdate"].Cast<CheckBox>().CurrentValue)
                {
                    Game.OnUpdate += GameOnUpdate;
                }
                else
                {
                    Game.OnTick += GameOnUpdate;
                }
                Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
                Drawing.OnDraw += GameOnDraw;
                Drawing.OnEndScene += Drawing_OnEndScene;
            }

            catch (Exception e)
            {

                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code INIT)</font>");
            }

        }
        #endregion

        #region Gamerelated Logic

        public override void Combo()
        {
            var targetq = TargetSelector.GetTarget(_q.Range, DamageType.Magical);

            if (_q.IsReady())
            {
                if (Value.Use("combo.q"))
                {
                    if (targetq != null && !Orbwalker.IsAutoAttacking)
                    {
                        _q.Cast(targetq);
                    }

                }
            }

            if (_w.IsReady())
            {
                if (Value.Use("combo.w"))
                {
                    if (Player.Instance.CountEnemiesInRange(Value.Get("combo.w.distance")) > 0)
                    {
                        _w.Cast();
                    }
                }
            }

            if (_r.IsReady() && _r.IsLearned)
            {
                if (Value.Use("combo.r") && Rstacks > Value.Get("combo.r.stacks"))
                {
                    var targetr = TargetSelector.GetTarget(RRange, DamageType.Magical);

                    if (targetr != null)
                    {
                        var rpred = _r.GetPrediction(targetr);
                        if (rpred.HitChancePercent >= Value.Get("combo.r.prediction") && !Orbwalker.IsAutoAttacking)
                        {
                            Rcast(rpred.CastPosition);
                        }
                    }
                }
            }
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Magical);

            if (_q.IsReady())
            {
                if (Value.Use("harass.q") && Player.Instance.ManaPercent >= Value.Get("harass.q.mana"))
                {
                    if (target != null && !Orbwalker.IsAutoAttacking)
                    {
                        _q.Cast(target);
                    }
                }
            }
        }

        public override void Laneclear()
        {
            var minionq =
                EntityManager.MinionsAndMonsters.GetLaneMinions()
                    .OrderBy(a => a.Health)
                    .FirstOrDefault(
                        a => a.IsValidTarget(_q.Range) && a.Health >= Player.Instance.GetSpellDamage(a, SpellSlot.Q));

            if (_q.IsReady())
            {
                if (Value.Use("lane.q") && Player.Instance.ManaPercent >= Value.Get("lane.q.mana"))
                {
                    if (minionq != null && !Orbwalker.IsAutoAttacking)
                    {
                        _q.Cast(minionq);
                    }
                }
            }

            if (_r.IsReady() && _r.IsLearned)
            {
                if (Value.Use("lane.r") && Player.Instance.ManaPercent >= Value.Get("lane.r.mana"))
                {
                    var minionr = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                        Player.Instance.Position, RRange).Where(a => !a.HasBuff("bantamtraptarget"));
                    var farmr = EntityManager.MinionsAndMonsters.GetCircularFarmLocation(minionr, _r.Width + 80,
                        RRange);

                    if (farmr.HitNumber >= Value.Get("lane.r.min") && Rstacks > Value.Get("lane.r.stacks"))
                    {
                        Rcast(farmr.CastPosition);
                    }
                }
            }
        }

        public override void Jungleclear()
        {
            var monsterq =
                EntityManager.MinionsAndMonsters.GetJungleMonsters()
                    .OrderByDescending(a => a.MaxHealth)
                    .FirstOrDefault(
                        a =>
                            a.IsValidTarget(_q.Range) && Variables.SummonerRiftJungleList.Contains(a.BaseSkinName) &&
                            a.Health >= Player.Instance.GetSpellDamage(a, SpellSlot.Q));

            if (_q.IsReady())
            {
                if (Value.Use("jungle.q") && Player.Instance.ManaPercent >= Value.Get("jungle.q.mana"))
                {
                    if (monsterq != null)
                    {
                        _q.Cast(monsterq);
                    }
                }
            }

            if (_r.IsReady() && _r.IsLearned)
            {
                if (Value.Use("jungle.r") && Player.Instance.ManaPercent >= Value.Get("jungle.r.mana"))
                {
                    var monsterr =
                        EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position,
                            RRange).Where(a => !a.HasBuff("bantamtraptarget"));
                    var farmr = EntityManager.MinionsAndMonsters.GetCircularFarmLocation(monsterr, _r.Width + 80,
                        RRange);

                    if (farmr.HitNumber >= Value.Get("jungle.r.min") && Rstacks > Value.Get("jungle.r.stacks"))
                    {
                        Rcast(farmr.CastPosition);
                    }
                }
            }
        }

        public override void Flee()
        {
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Magical);

            if (_w.IsReady())
            {
                if (Value.Use("flee.w") && Player.Instance.ManaPercent >= Value.Get("flee.w.mana"))
                {
                    _w.Cast();
                }
            }

            if (target == null)
            {
                return;
            }

            var tpos = Player.Instance.Position.Extend(target.Position, -300).To3D();

            if (_q.IsReady())
            {
                if (Value.Use("flee.q") && Player.Instance.ManaPercent >= Value.Get("flee.q.mana"))
                {
                    _q.Cast(target);
                }
            }

            if (_r.IsLearned && _r.IsReady())
            {
                if (Value.Use("flee.r") && Player.Instance.ManaPercent >= Value.Get("flee.r.mana"))
                {
                    if (Rstacks > Value.Get("flee.r.stacks"))
                    {
                        Rcast(tpos);
                    }
                }
            }
        }

        public override void LastHit()
        {
            var minion =
                EntityManager.MinionsAndMonsters.GetLaneMinions()
                    .OrderByDescending(a => a.MaxHealth)
                    .FirstOrDefault(
                        a => a.IsValidTarget(_q.Range) && a.Health <= Player.Instance.GetSpellDamage(a, SpellSlot.Q));

            if (_q.IsReady())
            {
                if (Value.Use("lasthit.q") && Player.Instance.ManaPercent >= Value.Get("lasthit.q.mana"))
                {
                    if (minion != null)
                    {
                        _q.Cast(minion);
                    }
                }
            }
        }

        private static void GameOnUpdate(EventArgs args)
        {
            Ks();

            AutoRcc();

            AutoShroom();
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender.IsAlly || !Value.Use("misc.q.gapcloser"))
            {
                return;
            }

            if (_q.IsReady() && _q.IsInRange(sender))
            {
                _q.Cast(sender);
            }
        }

        #endregion

        #region Utils

        private static int Rstacks
        {
            get { return _r.Handle.Ammo; }
        }
        
        private static bool Shroomed(Vector3 castposition)
        {
            return
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(a => a.Name == "Noxious Trap").Any(a => castposition.Distance(a.Position) <= _r.Width + 80);
        }

        private static void Ks()
        {
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Magical);

            if (_q.IsReady())
            {
                if (Value.Use("killsteal.q") && Player.Instance.ManaPercent >= Value.Get("killsteal.q.mana"))
                {
                    if (target != null &&
                        target.TotalShieldHealth() <= Player.Instance.GetSpellDamage(target, SpellSlot.Q) &&
                        !Orbwalker.IsAutoAttacking)
                    {
                        _q.Cast(target);
                    }
                }
            }
        }

        private static void AutoRcc()
        {
            var targetq =
                EntityManager.Heroes.Enemies.FirstOrDefault(
                    a => a.IsValidTarget(_q.Range) &&
                         (a.HasBuffOfType(BuffType.Charm) || a.HasBuffOfType(BuffType.Knockup) ||
                          a.HasBuffOfType(BuffType.Snare) || a.HasBuffOfType(BuffType.Stun) ||
                          a.HasBuffOfType(BuffType.Suppression) || a.HasBuffOfType(BuffType.Taunt)));
            

            if (_q.IsReady())
            {
                if (targetq != null)
                {
                    if (Value.Use("misc.q.charm") && targetq.IsCharmed ||
                        Value.Use("misc.q.knockup") ||
                        Value.Use("misc.q.stun") && targetq.IsStunned ||
                        Value.Use("misc.q.snare") && targetq.IsRooted ||
                        Value.Use("misc.q.suppression") && targetq.IsSuppressCallForHelp ||
                        Value.Use("misc.q.taunt") && targetq.IsTaunted)
                    {
                        _q.Cast(targetq);
                    }
                }
            }

            if (_r.IsReady() && _r.IsLearned)
            {
                var target =
                EntityManager.Heroes.Enemies.FirstOrDefault(
                    a => a.IsValidTarget(RRange) &&
                         (a.HasBuffOfType(BuffType.Charm) || a.HasBuffOfType(BuffType.Knockup) ||
                          a.HasBuffOfType(BuffType.Snare) || a.HasBuffOfType(BuffType.Stun) ||
                          a.HasBuffOfType(BuffType.Suppression) || a.HasBuffOfType(BuffType.Taunt)));

                if (target != null)
                {
                    if (Value.Use("misc.r.charm") && target.IsCharmed ||
                        Value.Use("misc.r.knockup") ||
                        Value.Use("misc.r.stun") && target.IsStunned ||
                        Value.Use("misc.r.snare") && target.IsRooted ||
                        Value.Use("misc.r.suppression") && target.IsSuppressCallForHelp ||
                        Value.Use("misc.r.taunt") && target.IsTaunted)
                    {
                        Rcast(_r.GetPrediction(target).CastPosition);
                    }
                }
            }
        }

        private static void AutoShroom()
        {
            if (_r.IsReady() && _r.IsLearned)
            {
                if (Value.Active("misc.r.auto") && Rstacks > Value.Get("misc.r.stacks") && !Player.Instance.IsRecalling() && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Game.MapId == GameMapId.SummonersRift)
                {
                    var teemo = Player.Instance.Position;

                    //top
                    var topTri = new Vector2(4467, 11853);
                    var topRiver = new Vector2(3099, 10826);
                    var topPixel = new Vector2(5229, 9150);
                    var topEntrance = new Vector2(3916, 12830);
                    var baron = new Vector2(4677, 10075);

                    //mid
                    var midbrushleft = new Vector2(6512, 8368);
                    var midbushright = new Vector2(8411, 6498);
                    var midleftbrush = new Vector2(5029, 8503);
                    var midrighbrush = new Vector2(9856, 6520);

                    //bot
                    var botPixel = new Vector2(9427, 5675);
                    var botTri = new Vector2(10392, 3094);
                    var botRiver = new Vector2(11791, 4108);
                    var botEntrance = new Vector2(10894, 1995);
                    var dragon = new Vector2(10169, 4838);

                    //red jungle
                    var rkrugsBrush = new Vector2(5643, 12789);
                    var rredbrus1 = new Vector2(7994, 11852);
                    var rredbrush2 = new Vector2(6731, 11463);
                    var rredbrush3 = new Vector2(6291, 10150);
                    var rbasebrush = new Vector2(9230, 11503);
                    var rwraitshbrush = new Vector2(8291, 10263);
                    var rwolvesbrush = new Vector2(9964, 7929);
                    var rbluebrush = new Vector2(11486, 7172);
                    var rgrompbrush = new Vector2(12493, 5223);
                    var rtoplanebrush = new Vector2(7160, 14120);
                    var rbotlanebrush = new Vector2(14108, 7031);

                    //blue jungle
                    var bkrugsBrush = new Vector2(9219, 2181);
                    var bredbrus1 = new Vector2(6848, 3120);
                    var bredbrush2 = new Vector2(8051, 3525);
                    var bredbrush3 = new Vector2(8543, 4848);
                    var bbasebrush = new Vector2(5609, 3509);
                    var bwraitshbrush = new Vector2(6545, 4711);
                    var bwolvesbrush = new Vector2(4820, 7149);
                    var bbluebrush = new Vector2(3384, 7787);
                    var bgrompbrush = new Vector2(2311, 9752);
                    var btoplanebrush = new Vector2(826, 8171);
                    var bbotlanebrush = new Vector2(7778, 831);

                    if (Player.Instance.CountEnemiesInRange(_q.Range) == 0)
                    {
                        //top
                        if (teemo.Distance(topTri) < RRange)
                        {
                            Rcast(topTri.To3D());
                        }
                        if (teemo.Distance(topRiver) < RRange)
                        {
                            Rcast(topRiver.To3D());
                        }
                        if (teemo.Distance(topPixel) < RRange)
                        {
                            Rcast(topPixel.To3D());
                        }
                        if (teemo.Distance(topEntrance) < RRange)
                        {
                            Rcast(topEntrance.To3D());
                        }
                        if (teemo.Distance(baron) < RRange)
                        {
                            Rcast(baron.To3D());
                        }

                        //mid
                        if (teemo.Distance(midbrushleft) < RRange)
                        {
                            Rcast(midbrushleft.To3D());
                        }
                        if (teemo.Distance(midbushright) < RRange)
                        {
                            Rcast(midbushright.To3D());
                        }
                        if (teemo.Distance(midleftbrush) < RRange)
                        {
                            Rcast(midleftbrush.To3D());
                        }
                        if (teemo.Distance(midrighbrush) < RRange)
                        {
                            Rcast(midrighbrush.To3D());
                        }

                        //bot
                        if (teemo.Distance(botTri) < RRange)
                        {
                            Rcast(botTri.To3D());
                        }
                        if (teemo.Distance(botPixel) < RRange)
                        {
                            Rcast(botPixel.To3D());
                        }
                        if (teemo.Distance(botEntrance) < RRange)
                        {
                            Rcast(botEntrance.To3D());
                        }
                        if (teemo.Distance(botRiver) < RRange)
                        {
                            Rcast(botRiver.To3D());
                        }
                        if (teemo.Distance(dragon) < RRange)
                        {
                            Rcast(dragon.To3D());
                        }

                        //red
                        if (teemo.Distance(rbasebrush) < RRange)
                        {
                            Rcast(rbasebrush.To3D());
                        }
                        if (teemo.Distance(rbluebrush) < RRange)
                        {
                            Rcast(rbluebrush.To3D());
                        }
                        if (teemo.Distance(rbotlanebrush) < RRange)
                        {
                            Rcast(rbotlanebrush.To3D());
                        }
                        if (teemo.Distance(rgrompbrush) < RRange)
                        {
                            Rcast(rgrompbrush.To3D());
                        }
                        if (teemo.Distance(rkrugsBrush) < RRange)
                        {
                            Rcast(rkrugsBrush.To3D());
                        }
                        if (teemo.Distance(rredbrus1) < RRange)
                        {
                            Rcast(rredbrus1.To3D());
                        }
                        if (teemo.Distance(rredbrush2) < RRange)
                        {
                            Rcast(rredbrush2.To3D());
                        }
                        if (teemo.Distance(rredbrush3) < RRange)
                        {
                            Rcast(rredbrush3.To3D());
                        }
                        if (teemo.Distance(rtoplanebrush) < RRange)
                        {
                            Rcast(rtoplanebrush.To3D());
                        }
                        if (teemo.Distance(rwolvesbrush) < RRange)
                        {
                            Rcast(rwolvesbrush.To3D());
                        }
                        if (teemo.Distance(rwraitshbrush) < RRange)
                        {
                            Rcast(rwraitshbrush.To3D());
                        }

                        //blue
                        if (teemo.Distance(bbasebrush) < RRange)
                        {
                            Rcast(bbasebrush.To3D());
                        }
                        if (teemo.Distance(bbluebrush) < RRange)
                        {
                            Rcast(bbluebrush.To3D());
                        }
                        if (teemo.Distance(bbotlanebrush) < RRange)
                        {
                            Rcast(bbotlanebrush.To3D());
                        }
                        if (teemo.Distance(bgrompbrush) < RRange)
                        {
                            Rcast(bgrompbrush.To3D());
                        }
                        if (teemo.Distance(bkrugsBrush) < RRange)
                        {
                            Rcast(bkrugsBrush.To3D());
                        }
                        if (teemo.Distance(bredbrus1) < RRange)
                        {
                            Rcast(bredbrus1.To3D());
                        }
                        if (teemo.Distance(bredbrush2) < RRange)
                        {
                            Rcast(bredbrush2.To3D());
                        }
                        if (teemo.Distance(bredbrush3) < RRange)
                        {
                            Rcast(bredbrush3.To3D());
                        }
                        if (teemo.Distance(btoplanebrush) < RRange)
                        {
                            Rcast(btoplanebrush.To3D());
                        }
                        if (teemo.Distance(bwolvesbrush) < RRange)
                        {
                            Rcast(bwolvesbrush.To3D());
                        }
                        if (teemo.Distance(bwraitshbrush) < RRange)
                        {
                            Rcast(bwraitshbrush.To3D());
                        }
                    }
                }
            }
        }

        private static void Rcast(Vector3 location)
        {
            if (!Shroomed(location) && !_rDelay)
            {
                _r.Cast(location);
                _rDelay = true; Core.DelayAction(() => _rDelay = false, 1000);
            }
        }

        private static int RRange
        {
            get { return Rranges[_r.Level - 1]; }
        }

        private static float EPassiveTime(Obj_AI_Base target)
        {
            if (target.HasBuff("toxicshotparticle"))
            {
                return Math.Max(0, target.GetBuff("toxicshotparticle").EndTime) - Game.Time;
            }
            return 0;
        }

        private static float RTime(Obj_AI_Base target)
        {
            if (target.HasBuff("bantamtraptarget"))
            {
                return Math.Max(0, target.GetBuff("bantamtraptarget").EndTime) - Game.Time;
            }
            return 0;
        }

        private static float EPassivedamage(Obj_AI_Base target)
        {
            float dmg = 0;
            if (!target.HasBuff("toxicshotparticle") || !_e.IsLearned) return 0;

            if (_e.Level == 1)
            {
                dmg = 6 + (.10f * Player.Instance.TotalMagicalDamage);
            }
            if (_e.Level == 2)
            {
                dmg = 12 + (.10f * Player.Instance.TotalMagicalDamage);
            }
            if (_e.Level == 3)
            {
                dmg = 18 + (.10f * Player.Instance.TotalMagicalDamage);
            }
            if (_e.Level == 4)
            {
                dmg = 24 + (.10f * Player.Instance.TotalMagicalDamage);
            }
            if (_e.Level == 5)
            {
                dmg = 30 + (.10f * Player.Instance.TotalMagicalDamage);
            }
            return dmg * EPassiveTime(target) - target.HPRegenRate;
        }

        private static float Rdamage(Obj_AI_Base target)
        {
            float dmg = 0;
            if (!target.HasBuff("bantamtraptarget") || !_r.IsLearned) return 0;

            if (_r.Level == 1)
            {
                dmg = 50 + (.125f * Player.Instance.TotalMagicalDamage);
            }
            if (_r.Level == 2)
            {
                dmg = 81.25f + (.125f * Player.Instance.TotalMagicalDamage);
            }
            if (_r.Level == 3)
            {
                dmg = 112.5f + (.125f * Player.Instance.TotalMagicalDamage);
            }
            return dmg * RTime(target) - target.HPRegenRate;
        }

        private static float Rdamagecalc(Obj_AI_Base target)
        {
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical, Rdamage(target));
        }

        private static float EPassivedamagecalc(Obj_AI_Base target)
        {
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical, EPassivedamage(target));
        }

        private static float EDamageonhit(Obj_AI_Base target)
        {
            if (_e.IsLearned)
            {
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                    new[] {10, 20, 30, 40, 50}[_e.Level - 1] + (.30f*Player.Instance.TotalMagicalDamage));
            }
            return 0f;
        }

        private static float ComboDamage(Obj_AI_Base target)
        {
            var damage = Player.Instance.GetAutoAttackDamage(target) + EDamageonhit(target);

            if (_q.IsReady())
            {
                damage += Player.Instance.GetSpellDamage(target, SpellSlot.Q);
            }

            if (target.HasBuff("toxicshotparticle"))
            {
                damage += EPassivedamagecalc(target);
            }

            if (target.HasBuff("bantamtraptarget"))
            {
                damage += Rdamagecalc(target);
            }
            return damage;
        }
        #endregion

        #region Drawings
        private static void GameOnDraw(EventArgs args)
        {
            var colorQ = MainMenu._draw.GetColor("color.q");
            var widthQ = MainMenu._draw.GetWidth("width.q");
            var colorR = MainMenu._draw.GetColor("color.r");
            var widthR = MainMenu._draw.GetWidth("width.r");

            if (!Value.Use("draw.disable"))
            {
                if (Value.Use("draw.q") && ((Value.Use("draw.ready") && _q.IsReady()) || !Value.Use("draw.ready")))
                {
                    new Circle
                    {
                        Color = colorQ,
                        Radius = _q.Range,
                        BorderWidth = widthQ
                    }.Draw(Player.Instance.Position);
                }

                if (_r.IsLearned)
                {
                    if (Value.Use("draw.r") && ((Value.Use("draw.ready") && _r.IsReady()) || !Value.Use("draw.ready")))
                    {
                        new Circle
                        {
                            Color = colorR,
                            Radius = RRange,
                            BorderWidth = widthR
                        }.Draw(Player.Instance.Position);
                    }
                }

                if (Value.Use("draw.r.auto") && Game.MapId == GameMapId.SummonersRift)
                {
                    //top
                    var topTri = new Vector2(4467, 11853);
                    var topRiver = new Vector2(3099, 10826);
                    var topPixel = new Vector2(5229, 9150);
                    var topEntrance = new Vector2(3916, 12830);
                    var baron = new Vector2(4677, 10075);

                    //mid
                    var midbrushleft = new Vector2(6512, 8368);
                    var midbushright = new Vector2(8411, 6498);
                    var midleftbrush = new Vector2(5029, 8503);
                    var midrighbrush = new Vector2(9856, 6520);

                    //bot
                    var botPixel = new Vector2(9427, 5675);
                    var botTri = new Vector2(10392, 3094);
                    var botRiver = new Vector2(11791, 4108);
                    var botEntrance = new Vector2(10894, 1995);
                    var dragon = new Vector2(10169, 4838);

                    //red jungle
                    var rkrugsBrush = new Vector2(5643, 12789);
                    var rredbrus1 = new Vector2(7994, 11852);
                    var rredbrush2 = new Vector2(6731, 11463);
                    var rredbrush3 = new Vector2(6291, 10150);
                    var rbasebrush = new Vector2(9230, 11503);
                    var rwraitshbrush = new Vector2(8291, 10263);
                    var rwolvesbrush = new Vector2(9964, 7929);
                    var rbluebrush = new Vector2(11486, 7172);
                    var rgrompbrush = new Vector2(12493, 5223);
                    var rtoplanebrush = new Vector2(7160, 14120);
                    var rbotlanebrush = new Vector2(14108, 7031);

                    //blue jungle
                    var bkrugsBrush = new Vector2(9219, 2181);
                    var bredbrus1 = new Vector2(6848, 3120);
                    var bredbrush2 = new Vector2(8051, 3525);
                    var bredbrush3 = new Vector2(8543, 4848);
                    var bbasebrush = new Vector2(5609, 3509);
                    var bwraitshbrush = new Vector2(6545, 4711);
                    var bwolvesbrush = new Vector2(4820, 7149);
                    var bbluebrush = new Vector2(3384, 7787);
                    var bgrompbrush = new Vector2(2311, 9752);
                    var btoplanebrush = new Vector2(826, 8171);
                    var bbotlanebrush = new Vector2(7778, 831);

                    //top
                    new Circle(Color.Green, 70).Draw(topTri.To3D());
                    new Circle(Color.Green, 70).Draw(topPixel.To3D());
                    new Circle(Color.Green, 70).Draw(topRiver.To3D());
                    new Circle(Color.Green, 70).Draw(topEntrance.To3D());
                    new Circle(Color.Green, 70).Draw(baron.To3D());

                    //mid
                    new Circle(Color.Green, 70).Draw(midrighbrush.To3D());
                    new Circle(Color.Green, 70).Draw(midleftbrush.To3D());
                    new Circle(Color.Green, 70).Draw(midbrushleft.To3D());
                    new Circle(Color.Green, 70).Draw(midbushright.To3D());

                    //bot
                    new Circle(Color.Green, 70).Draw(botTri.To3D());
                    new Circle(Color.Green, 70).Draw(botPixel.To3D());
                    new Circle(Color.Green, 70).Draw(botRiver.To3D());
                    new Circle(Color.Green, 70).Draw(botEntrance.To3D());
                    new Circle(Color.Green, 70).Draw(dragon.To3D());

                    //red
                    new Circle(Color.Green, 70).Draw(rwolvesbrush.To3D());
                    new Circle(Color.Green, 70).Draw(rwraitshbrush.To3D());
                    new Circle(Color.Green, 70).Draw(rbasebrush.To3D());
                    new Circle(Color.Green, 70).Draw(rbluebrush.To3D());
                    new Circle(Color.Green, 70).Draw(rbotlanebrush.To3D());
                    new Circle(Color.Green, 70).Draw(rgrompbrush.To3D());
                    new Circle(Color.Green, 70).Draw(rkrugsBrush.To3D());
                    new Circle(Color.Green, 70).Draw(rredbrus1.To3D());
                    new Circle(Color.Green, 70).Draw(rredbrush2.To3D());
                    new Circle(Color.Green, 70).Draw(rredbrush3.To3D());
                    new Circle(Color.Green, 70).Draw(rtoplanebrush.To3D());

                    //blue
                    new Circle(Color.Green, 70).Draw(bwolvesbrush.To3D());
                    new Circle(Color.Green, 70).Draw(bwraitshbrush.To3D());
                    new Circle(Color.Green, 70).Draw(bbasebrush.To3D());
                    new Circle(Color.Green, 70).Draw(bbluebrush.To3D());
                    new Circle(Color.Green, 70).Draw(bbotlanebrush.To3D());
                    new Circle(Color.Green, 70).Draw(bgrompbrush.To3D());
                    new Circle(Color.Green, 70).Draw(bkrugsBrush.To3D());
                    new Circle(Color.Green, 70).Draw(bredbrus1.To3D());
                    new Circle(Color.Green, 70).Draw(bredbrush2.To3D());
                    new Circle(Color.Green, 70).Draw(bredbrush3.To3D());
                    new Circle(Color.Green, 70).Draw(btoplanebrush.To3D());
                }

                if (Value.Use("draw.status"))
                {
                    if (Value.Active("misc.r.auto"))
                    {
                        Drawing.DrawText(1342, 1019, System.Drawing.Color.Chartreuse, "Auto-Shroom Activated", 20);
                    }
                    else
                    {
                        Drawing.DrawText(1342, 1019, System.Drawing.Color.Red, "Auto-Shroom Disabled", 20);
                    }
                }
            }
        }
        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Value.Use("draw.hp.bar"))
            {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(a => !a.IsDead && a.IsHPBarRendered))
                {
                    var damage = ComboDamage(enemy);
                    var damagepercent = ((enemy.TotalShieldHealth() - damage) > 0 ? (enemy.TotalShieldHealth() - damage) : 0) / (enemy.MaxHealth + enemy.AllShield + enemy.AttackShield + enemy.MagicShield);
                    var hppercent = enemy.TotalShieldHealth() / (enemy.MaxHealth + enemy.AllShield + enemy.AttackShield + enemy.MagicShield);
                    var start = new Vector2((int)(enemy.HPBarPosition.X + Offset.X + damagepercent * 104), (int)(enemy.HPBarPosition.Y + Offset.Y) - 5);
                    var end = new Vector2((int)(enemy.HPBarPosition.X + Offset.X + hppercent * 104) + 2, (int)(enemy.HPBarPosition.Y + Offset.Y) - 5);

                    Drawing.DrawLine(start, end, 9, System.Drawing.Color.Chartreuse);
                }
            }
        }
        #endregion 
    }
}
