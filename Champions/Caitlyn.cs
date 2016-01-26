using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using OKTRAIO.Menu_Settings;
using SharpDX;
using Color = System.Drawing.Color;

namespace OKTRAIO.Champions
{
    class Caitlyn : AIOChampion
    {

        #region Initialize and Declare

        private static Spell.Skillshot _q;
        private static Spell.Skillshot _w;
        private static Spell.Skillshot _e;
        private static Spell.Targeted _r;
        private static readonly Vector2 Offset = new Vector2(1, 0);

        public override void Init()
        {
            try
            {
                //Creating Spells
                _q = new Spell.Skillshot(SpellSlot.Q, 1240, SkillShotType.Linear, 250, 2000, 60);
                _w = new Spell.Skillshot(SpellSlot.W, 820, SkillShotType.Circular, 500, int.MaxValue, 80);
                _e = new Spell.Skillshot(SpellSlot.E, 800, SkillShotType.Linear, 250, 1600, 80);
                _r = new Spell.Targeted(SpellSlot.R, 2000);

                try
                {
                    //Combo Menu Settings
                    MainMenu.ComboKeys(true, true, true, false);
                    MainMenu._combo.AddSeparator();
                    MainMenu._combo.AddGroupLabel("Combo Preferences", "combo.grouplabel.1", true);
                    MainMenu._combo.AddCheckBox("combo.melee", "Use abilities also in melee range", false, true);
                    MainMenu._combo.AddCheckBox("combo.force.target", "Force target with Passive ", true, true);
                    MainMenu._combo.AddCheckBox("combo.e.gapcloser", "Use E for Reach the target", false, true);
                    MainMenu._combo.AddSlider("combo.e.range",
                        "Don't Use E if in Range from Target there are more than {0} enemies", 1, 1, 5, true);
                    MainMenu._combo.Add("combo.e.q.bind",
                     new KeyBind("Use E + Q Spells", false, KeyBind.BindTypes.HoldActive, 'Z'))
                     .OnValueChange += OnEqButton;
                    Value.AdvancedMenuItemUiDs.Add("combo.e.q.bind");
                    MainMenu._combo["combo.e.q.bind"].IsVisible = MainMenu._combo["combo.advanced"].Cast<CheckBox>().CurrentValue;
                    MainMenu._combo.AddSeparator();
                    MainMenu._combo.AddGroupLabel("Prediction Settings", "combo.grouplabel.2", true);
                    MainMenu._combo.AddSlider("combo.q.prediction", "Use Q if Hitchance > {0}%", 80, 0, 100, true);
                    MainMenu._combo.AddSlider("combo.w.prediction", "Use W if Hitchance > {0}%", 80, 0, 100, true);
                    MainMenu._combo.AddSlider("combo.e.prediction", "Use E if Hitchance > {0}%", 80, 0, 100, true);
                    MainMenu._combo.AddSeparator();
                    MainMenu._combo.AddGroupLabel("Mana Manager:", "combo.grouplabel.3", true);
                    MainMenu.ComboManaManager(true, true, true, true, 60, 50, 40, 20);

                    //Lane Clear Menu Settings
                    MainMenu.LaneKeys(true, false, false, false);
                    MainMenu._lane.AddSeparator();
                    MainMenu._lane.AddGroupLabel("LaneClear Preferences", "lane.grouplabel.1", true);
                    MainMenu._lane.AddSlider("lane.farm", "Use Q if minions are more then {0} ", 3, 0, 6, true);
                    MainMenu._lane.AddSeparator();
                    MainMenu._lane.AddGroupLabel("Mana Manager:", "lane.grouplabel.2", true);
                    MainMenu.LaneManaManager(true, false, false, false, 60, 0, 0, 0);

                    //Jungle Clear Menu Settings
                    MainMenu.JungleKeys(true, false, true, false);
                    MainMenu._jungle.AddSeparator();
                    MainMenu._jungle.AddGroupLabel("Jungleclear Preferences", "jungle.grouplabel.addonmenu", true);
                    MainMenu._jungle.AddCheckBox("jungle.monsters.spell", "Use Abilities on Big Monster", true, true);
                    MainMenu._jungle.AddCheckBox("jungle.minimonsters.spell", "Use Abilities on Mini Monsters", false,
                        true);
                    MainMenu._jungle.AddSeparator();
                    MainMenu._jungle.AddGroupLabel("Mana Manager:", "jungle.grouplabel.1", true);
                    MainMenu.JungleManaManager(true, false, false, false, 60, 0, 0, 0);

                    //Last hit Menu Settings
                    MainMenu.LastHitKeys(true, false, true, false);
                    MainMenu._lasthit.AddSeparator();
                    MainMenu._lasthit.AddGroupLabel("Mana Manager:", "lasthit.grouplabel.1", true);
                    MainMenu.LasthitManaManager(true, false, true, false, 40, 0, 60, 0);

                    //Harras
                    MainMenu.HarassKeys(true, true, true, false);
                    MainMenu._harass.AddSeparator();
                    MainMenu._harass.AddGroupLabel("Harass Preferences:", "harass.grouplabel.1", true);
                    MainMenu._harass.AddCheckBox("harass.auto", "Use AutoHarass", false, true);
                    MainMenu._harass.AddSlider("harass.traps", "Limit traps to", 2, 1, 5, true);
                    MainMenu._harass.AddCheckBox("harass.force.target", "Force target with Passsive", true, true);
                    MainMenu._harass.AddSeparator();
                    MainMenu._harass.AddGroupLabel("Prediction Settings", "harass.grouplabel.2", true);
                    MainMenu._harass.AddSlider("harass.q.prediction", "Use Q if Hitchance > {0}%", 80, 0, 100, true);
                    MainMenu._harass.AddSlider("harass.w.prediction", "Use W if Hitchance > {0}%", 80, 0, 100, true);
                    MainMenu._harass.AddSlider("harass.e.prediction", "Use E if Hitchance > {0}%", 80, 0, 100, true);
                    MainMenu._harass.AddSeparator();
                    MainMenu._harass.AddGroupLabel("Mana Manager:", "harass.grouplabel.3", true);
                    MainMenu.HarassManaManager(true, true, true, false, 50, 50, 50, 0);

                    //Flee Menu
                    MainMenu.FleeKeys(false, true, true, false);
                    MainMenu._flee.AddSeparator();
                    MainMenu._flee.AddGroupLabel("Mana Manager:", "flee.grouplabel.1", true);
                    MainMenu.FleeManaManager(false, true, true, false, 0, 10, 20, 0);

                    //Ks
                    MainMenu.KsKeys(true, true, true, true);
                    MainMenu._ks.AddSeparator();
                    MainMenu._ks.AddGroupLabel("KillSteal Preferences: ", "killsteal.groulabel.1", true);
                    MainMenu._ks.AddCheckBox("killsteal.spells.flee", "Prevent to killsteal with spells if fleeing", false, true);
                    MainMenu._ks.AddSlider("killsteal.r.enemies", "Prevent to killsteal with R if there are more then {0} enemies", 3, 0, 5, true);
                    MainMenu._ks.AddSlider("killsteal.r.safe", "Use R only if the target leaves {0} range", 1500, 800, 3000, true);
                    MainMenu._ks.AddSeparator();
                    MainMenu._ks.AddGroupLabel("Prediction Settings", "killsteal.grouplabel.2", true);
                    MainMenu._ks.AddSlider("killsteal.q.prediction", "Use Q if Hitchance > {0}%", 80, 0, 100, true);
                    MainMenu._ks.AddSlider("killsteal.w.prediction", "Use W if Hitchance > {0}%", 80, 0, 100, true);
                    MainMenu._ks.AddSlider("killsteal.e.prediction", "Use E if Hitchance > {0}%", 80, 0, 100, true);
                    MainMenu._ks.AddGroupLabel("Mana Manager:", "killsteal.grouplabel.3", true);
                    MainMenu.KsManaManager(true, true, true, false, 10, 10, 20, 0);

                    //Misc Menu
                    MainMenu.MiscMenu();
                    MainMenu._misc.AddCheckBox("misc.q", "Use Auto Q");
                    MainMenu._misc.AddCheckBox("misc.w", "Use Auto W");
                    MainMenu._misc.AddCheckBox("misc.e", "Use Auto E", false);
                    MainMenu._misc.AddCheckBox("misc.w.gapcloser", "Use Auto W on Gapcloser", true, true);
                    MainMenu._misc.AddCheckBox("misc.e.gapcloser", "Use Auto E on Gapcloser", true, true);
                    MainMenu._misc.AddSeparator();
                    MainMenu._misc.AddGroupLabel("Auto Spell Settings", "misc.grouplabel.1", true);
                    MainMenu._misc.AddCheckBox("misc.q.stun", "Use Q on Stunned Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.w.stun", "Use W on Stunned Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.e.stun", "Use E on Stunned Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.q.charm", "Use Q on Charmed Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.w.charm", "Use W on Charmed Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.e.charm", "Use E on Charmed Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.q.taunt", "Use Q on Taunted Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.w.taunt", "Use W on Taunted Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.e.taunt", "Use E on Taunted Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.q.fear", "Use Q on Feared Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.w.fear", "Use W on Feared Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.e.fear", "Use E on Feared Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.q.snare", "Use Q on Snared Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.w.snare", "Use W on Snared Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.e.snare", "Use E on Snared Enemy", true, true);
                    MainMenu._misc.AddSlider("misc.traps", "Limit traps to", 2, 1, 5, true);
                    MainMenu._misc.AddSeparator();
                    MainMenu._misc.AddGroupLabel("Mana Manager:", "misc.grouplabel.2", true);
                    MainMenu._misc.AddSlider("misc.q.mana", "Use Q on CC Enemy if Mana is above than {0}%", 30, 0, 100,
                        true);
                    MainMenu._misc.AddSlider("misc.w.mana", "Use W on CC Enemy if Mana is above than {0}%", 30, 0, 100,
                        true);
                    MainMenu._misc.AddSlider("misc.e.mana", "Use E on CC Enemy if Mana is above than {0}%", 30, 0, 100,
                        true);

                    //Draw Menu
                    MainMenu.DrawKeys(true, true, true, true);
                    MainMenu._draw.AddSeparator();
                    MainMenu._draw.AddCheckBox("draw.hp.bar", "Draw Combo Damage", true, true);

                    Value.Init();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Chat.Print(
                        "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code MENU)</font>");
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code 503)</font>");
            }

            try
            {
                if (MainMenu._menu["useonupdate"].Cast<CheckBox>().CurrentValue)
                {
                    Game.OnUpdate += GameOnUpdate;
                }
                else
                {
                    Game.OnTick += GameOnUpdate;
                }
                Gapcloser.OnGapcloser += AntiGapCloser;
                GameObject.OnCreate += OnCreate;
                Obj_AI_Base.OnBuffGain += OnBuffGain;
                Obj_AI_Base.OnBuffLose += OnBuffLose;
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

        #region Combo
        public override void Combo()
        {
            var target = TargetSelector.GetTarget(_r.Range, DamageType.Physical);

            if (target == null) return;

            if (Value.Use("combo.melee"))
            {
                if (Value.Use("combo.q"))
                {
                    if (Player.Instance.ManaPercent >= Value.Get("combo.q.mana"))
                    {
                        if (_q.GetPrediction(target).HitChancePercent >= Value.Get("combo.q.prediction"))
                        {
                            _q.Cast(_q.GetPrediction(target).CastPosition);
                        }
                    }
                }

                if (Value.Use("combo.w"))
                {
                    if (Player.Instance.ManaPercent >= Value.Get("combo.w.mana"))
                    {
                        if (_w.GetPrediction(target).HitChancePercent >= Value.Get("combo.w.prediction") &&
                            _w.Handle.Ammo >= 1)
                        {
                            _w.Cast(_w.GetPrediction(target).CastPosition);
                        }
                    }
                }
                if (Value.Use("combo.e"))
                {
                    if (Player.Instance.ManaPercent >= Value.Get("combo.e.mana") &&
                        target.CountEnemiesInRange(600) <= Value.Get("combo.e.range"))
                    {
                        if (Value.Use("combo.e.gapcloser"))
                        {
                            Vector3 pred = 
                            _e.GetPrediction(target).CastPosition + 2 * (Player.Instance.ServerPosition - _e.GetPrediction(target).CastPosition);

                            {
                                _e.Cast(pred);
                            }
                        }
                        else if (_e.GetPrediction(target).HitChancePercent >= Value.Get("combo.e.prediction"))
                        {
                            _e.Cast(_e.GetPrediction(target).CastPosition);
                        }
                    }
                }
            }

            else if (Player.Instance.Distance(target) >= 400)
            {
                if (Value.Use("combo.q"))
                {
                    if (Player.Instance.ManaPercent >= Value.Get("combo.q.mana"))
                    {
                        if (_q.GetPrediction(target).HitChancePercent >= Value.Get("combo.q.prediction"))
                        {
                            _q.Cast(_q.GetPrediction(target).CastPosition);
                        }
                    }
                }

                if (Value.Use("combo.w"))
                {
                    if (Player.Instance.ManaPercent >= Value.Get("combo.w.mana"))
                    {
                        if (_w.GetPrediction(target).HitChancePercent >= Value.Get("combo.w.prediction") &&
                            _w.Handle.Ammo >= 1)
                        {
                            _w.Cast(_w.GetPrediction(target).CastPosition);
                        }
                    }
                }
                if (Value.Use("combo.e"))
                {
                    if (Player.Instance.ManaPercent >= Value.Get("combo.e.mana") &&
                        target.CountEnemiesInRange(600) <= Value.Get("combo.e.range"))
                    {
                        if (Value.Use("combo.e.gapcloser"))
                        {
                            Vector3 pred =
                            _e.GetPrediction(target).CastPosition + 2 * (Player.Instance.ServerPosition - _e.GetPrediction(target).CastPosition);

                            {
                                _e.Cast(pred);
                            }
                        }
                        else if (_e.GetPrediction(target).HitChancePercent >= Value.Get("combo.e.prediction"))
                        {
                            _e.Cast(_e.GetPrediction(target).CastPosition);
                        }
                    }
                }
            }
        }
        #endregion 

        #region Harass
        public override void Harass()
        {
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Physical);

            if (target == null) return;

            if (Value.Use("harass.q"))
            {
                if (Player.Instance.ManaPercent >= Value.Get("harass.q.mana"))
                {
                    if (_q.GetPrediction(target).HitChancePercent >= Value.Get("harass.q.prediction"))
                    {
                        _q.Cast(_q.GetPrediction(target).CastPosition);
                    }
                }
            }

            if (Value.Use("harass.w"))
            {
                if (Player.Instance.ManaPercent >= Value.Get("harass.w.mana"))
                {
                    if (_w.GetPrediction(target).HitChancePercent >= Value.Get("harass.w.prediction") &&
                        _w.Handle.Ammo >= Value.Get("harass.traps"))
                    {
                        _w.Cast(_w.GetPrediction(target).CastPosition);
                    }
                }
            }
            if (Value.Use("harass.e"))
            {
                if (Player.Instance.ManaPercent >= Value.Get("harass.e.mana"))
                {
                    if (_e.GetPrediction(target).HitChancePercent >= Value.Get("harass.e.prediction"))
                    {
                        _e.Cast(_e.GetPrediction(target).CastPosition);
                    }
                }
            }
        }
        #endregion 

        #region Laneclear
        public override void Laneclear()
        {
            var count =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Instance.ServerPosition,
                    Player.Instance.AttackRange, false).Count();

            var source =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Instance.ServerPosition,
                    Player.Instance.AttackRange).OrderBy(a => a.MaxHealth).FirstOrDefault();
            if (count != 0 && source != null)
            {
                if (Value.Use("lane.q") && _q.IsReady())
                {
                    if (Player.Instance.ManaPercent >= Value.Get("lane.q.mana") && 
                        count >= Value.Get("lane.farm"))
                    {
                        _q.Cast(source);
                    }
                }
            }
        }
        #endregion 

        #region Jungleclear
        public override void Jungleclear()
        {
            var monsters =
                EntityManager.MinionsAndMonsters.GetJungleMonsters(ObjectManager.Player.ServerPosition,
                    _q.Range)
                    .FirstOrDefault(x => x.IsValidTarget(_q.Range));

            var fappamonsters =
                EntityManager.MinionsAndMonsters.GetJungleMonsters(ObjectManager.Player.ServerPosition,
                    _q.Range)
                    .LastOrDefault(x => x.IsValidTarget(_q.Range));

            if (monsters != null)
            {
                if (Value.Use("jungle.monsters.spell"))
                {
                    if (Value.Use("jungle.q") && _q.IsReady())
                    {
                        if (Player.Instance.ManaPercent >= Value.Get("jungle.q.mana"))
                        {
                            _q.Cast(monsters);
                        }
                    }

                    if (Value.Use("jungle.e") && _e.IsReady())
                    {
                        if (Player.Instance.ManaPercent >= Value.Get("jungle.e.mana"))
                        {
                            _e.Cast(monsters);
                        }
                    }
                }
            }

            if (fappamonsters != null)
            {
                if (Value.Use("jungle.minimonsters.spell"))
                {

                    if (Value.Use("jungle.q") && _q.IsReady())
                    {
                        if (Player.Instance.ManaPercent >= Value.Get("jungle.q.mana"))
                        {
                            _q.Cast(fappamonsters);
                        }
                    }


                    if (Value.Use("jungle.e") && _e.IsReady())
                    {
                        if (Player.Instance.ManaPercent >= Value.Get("jungle.e.mana"))
                        {
                            _e.Cast(fappamonsters);
                        }
                    }
                }
            }
        }
        #endregion 

        #region Flee

        public override void Flee()
        {
            Vector3 pred = Game.CursorPos + (Player.Instance.Position - Game.CursorPos);

            if (Value.Use("flee.e") && _e.IsReady())
            {
                if (Player.Instance.ManaPercent >= Value.Get("flee.e.mana"))
                {
                    _e.Cast(pred);
                }
            }

            if (Value.Use("flee.w") && _w.IsReady())
            {
                var target = TargetSelector.GetTarget(_w.Range, DamageType.Magical);

                if (target == null) return;

                if (Player.Instance.ManaPercent >= Value.Get("flee.w.mana"))
                {
                    _w.Cast(_w.GetPrediction(target).CastPosition);
                }
            }
        }



        #endregion 

        #region Lasthit

        public override void LastHit()
        {
            var source =
                EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(
                    m => m.IsValidTarget(_q.Range));

            if (source == null) return;

            if (Value.Use("lasthit.q") && _q.IsReady())
            {
                if (Player.Instance.ManaPercent >= Value.Get("lasthit.q.mana") &&
                    Player.Instance.GetSpellDamage(source, SpellSlot.Q) > source.TotalShieldHealth())
                {
                    _q.Cast(source);
                }
            }

            if (Value.Use("lasthit.e") && _e.IsReady())
            {
                if (Player.Instance.ManaPercent >= Value.Get("lasthit.e.mana") &&
                    Player.Instance.GetSpellDamage(source, SpellSlot.E) > source.TotalShieldHealth())
                {
                    _e.Cast(source);
                }
            }
        }

        #endregion

        #endregion

        #region Utils

        #region AntiGapCloser

        private static void AntiGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            try
            {
                if (!e.Sender.IsValidTarget() || e.Sender.Type != Player.Instance.Type ||
                    !e.Sender.IsEnemy)
                    return;
                Vector3 pred = e.End;

                if (_e.IsReady() && _e.IsInRange(sender) && Value.Use("misc.e.gapcloser"))
                {
                    _e.Cast(pred + 5 * (Player.Instance.Position - e.End));
                }
                if (_w.IsReady() && _w.IsInRange(sender) && Value.Use("misc.w.gapcloser"))
                {
                    _w.Cast(pred + 5 * (Player.Instance.Position - e.End));
                }
            }

            catch (Exception a)
            {
                Console.WriteLine(a);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code ANTIGAP)</font>");
            }
        }

        #endregion

        #region OnCreate
        private static void OnCreate(GameObject sender, EventArgs args)
        {
            var rengar = EntityManager.Heroes.Enemies.Find(r => r.ChampionName.Equals("Rengar"));
            var khazix = EntityManager.Heroes.Enemies.Find(z => z.ChampionName.Equals("Khazix"));

            try
            {
                if (Value.Use("misc.e.gapcloser"))
                {
                    if (khazix != null)
                    {
                        if (sender.Name == ("Khazix_Base_E_Tar.troy") &&
                            sender.Position.Distance(Player.Instance) <= 400) _e.Cast(_e.GetPrediction(khazix).CastPosition);
                    }
                    if (rengar != null)
                    {
                        if (sender.Name == ("Rengar_LeapSound.troy") &&
                            sender.Position.Distance(Player.Instance) < _e.Range) _e.Cast(_e.GetPrediction(rengar).CastPosition);
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code ONCREATE)</font>");
            }
        }
        #endregion

        #region OnUpdate
        public static void GameOnUpdate(EventArgs args)
        {
            RaiseRLevel();
            AutoSpells();
            KillSteal();
            AutoHarass();
        }
        #endregion
    
        #region RaiseRLevel
        public static void RaiseRLevel()
        {
            if (_r.IsLearned)
            {
                _r = new Spell.Targeted(SpellSlot.R, (uint) new[] { 2000, 2500, 3000 }[_r.Level - 1]);
            }
        }
        #endregion

        #region AutoHarass
        public static void AutoHarass()
        {
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Physical);

            if (target == null) return;

            if (Value.Use("harass.auto"))
            {
                if (Value.Use("harass.q") && _q.IsReady())
                {
                    if (Value.Get("harass.q.mana") >= Player.Instance.ManaPercent)
                    {
                        if (_q.GetPrediction(target).HitChancePercent >= Value.Get("harass.q.prediction"))
                        {
                            _q.Cast(_q.GetPrediction(target).CastPosition);
                        }
                    }
                }

                if (Value.Use("harass.w") && _w.IsReady())
                {
                    if (Value.Get("harass.w.mana") >= Player.Instance.ManaPercent)
                    {
                        if (_w.GetPrediction(target).HitChancePercent >= Value.Get("harass.w.prediction") &&
                            _w.Handle.Ammo >= Value.Get("harass.traps"))
                        {
                            _w.Cast(_w.GetPrediction(target).CastPosition);
                        }
                    }
                }
                if (Value.Use("harass.e") && _e.IsReady())
                {
                    if (Value.Get("harass.e.mana") >= Player.Instance.ManaPercent)
                    {
                        if (_e.GetPrediction(target).HitChancePercent >= Value.Get("harass.e.prediction"))
                        {
                            _e.Cast(_e.GetPrediction(target).CastPosition);
                        }
                    }
                }
            }
        }
        #endregion 

        #region AutoSpells   
        private static void AutoSpells()
        {

            var enemy =
                EntityManager.Heroes.Enemies.FirstOrDefault(
                    x =>
                        x.HasBuffOfType(BuffType.Charm) || x.HasBuffOfType(BuffType.Knockup) ||
                        x.HasBuffOfType(BuffType.Stun) || x.HasBuffOfType(BuffType.Suppression) ||
                        x.HasBuffOfType(BuffType.Snare));

            if (Orbwalker.IsAutoAttacking ||
                EntityManager.Turrets.Enemies.Count(t => t.IsValidTarget(_q.Range) && t.IsAttackingPlayer) > 0)
                return;

            try
            {
                if (enemy == null || enemy.IsValidTarget(_q.Range)) return;

                if (Value.Use("misc.q"))
                {
                    if (Value.Get("misc.q.mana") >= Player.Instance.ManaPercent)
                    {
                        if (Value.Use("misc.q.stun") && enemy.IsStunned)
                        {
                            _q.Cast(enemy);
                        }
                        if (Value.Use("misc.q.snare") && enemy.IsRooted)
                        {
                            _q.Cast(enemy);
                        }
                        if (Value.Use("misc.q.charm") && enemy.IsCharmed)
                        {
                            _q.Cast(enemy);
                        }
                        if (Value.Use("misc.q.taunt") && enemy.IsTaunted)
                        {
                            _q.Cast(enemy);
                        }
                        if (Value.Use("misc.q.fear") && enemy.IsFeared)
                        {
                            _q.Cast(enemy);
                        }
                    }
                }

                if (Value.Use("misc.w") && _w.Handle.Ammo >= Value.Get("misc.traps"))
                {
                    if (Value.Get("misc.w.mana") >= Player.Instance.ManaPercent)
                    {
                        if (Value.Use("misc.w.stun") && enemy.IsStunned)
                        {
                            _w.Cast(enemy);
                        }
                        if (Value.Use("misc.w.snare") && enemy.IsRooted)
                        {
                            _w.Cast(enemy);
                        }
                        if (Value.Use("misc.w.charm") && enemy.IsCharmed)
                        {
                            _w.Cast(enemy);
                        }
                        if (Value.Use("misc.w.taunt") && enemy.IsTaunted)
                        {
                            _w.Cast(enemy);
                        }
                        if (Value.Use("misc.w.fear") && enemy.IsFeared)
                        {
                            _w.Cast(enemy);
                        }
                    }
                }

                if (Value.Use("misc.e"))
                {
                    if (Value.Get("misc.e.mana") >= Player.Instance.ManaPercent)
                    {
                        if (Value.Use("misc.e.stun") && enemy.IsStunned)
                        {
                            _e.Cast(enemy);
                        }
                        if (Value.Use("misc.e.snare") && enemy.IsRooted)
                        {
                            _e.Cast(enemy);
                        }
                        if (Value.Use("misc.e.charm") && enemy.IsCharmed)
                        {
                            _e.Cast(enemy);
                        }
                        if (Value.Use("misc.e.taunt") && enemy.IsTaunted)
                        {
                            _e.Cast(enemy);
                        }
                        if (Value.Use("misc.e.fear") && enemy.IsFeared)
                        {
                            _e.Cast(enemy);
                        }
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code AUTOQ)</font>");
            }
        }

        #endregion

        #region KillSteal
        private static void KillSteal()
        {
            try
            {
                if (Value.Use("killsteal.spells.flee") && Value.Mode(Orbwalker.ActiveModes.Flee)) return;

                foreach (
                    var target in
                        EntityManager.Heroes.Enemies.Where(
                            hero =>
                                hero.IsValidTarget(_r.Range) && !hero.IsDead && !hero.IsZombie &&
                                hero.HealthPercent <= 25))
                {
                    if (Value.Use("killsteal.q") && _q.IsReady())
                    {
                        if (Player.Instance.ManaPercent >= Value.Get("killsteal.q.mana") &&
                            _q.GetPrediction(target).HitChancePercent >= Value.Get("killsteal.q.prediction"))
                        {
                            if (target.Health + target.AttackShield <
                                Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                            {
                                _q.Cast(_q.GetPrediction(target).CastPosition);
                            }
                        }
                    }

                    if (Value.Use("killsteal.w") && _w.IsReady())
                    {
                        if (Player.Instance.ManaPercent >= Value.Get("killsteal.w.mana") &&
                            _w.GetPrediction(target).HitChancePercent >= Value.Get("killsteal.w.prediction"))
                        {
                            if (target.Health + target.AttackShield <
                                Player.Instance.GetSpellDamage(target, SpellSlot.W))
                            {
                                _w.Cast(_w.GetPrediction(target).CastPosition);
                            }
                        }
                    }

                    if (Value.Use("killsteal.e") && _e.IsReady())
                    {
                        if (Player.Instance.ManaPercent >= Value.Get("killsteal.e.mana") &&
                            _e.GetPrediction(target).HitChancePercent >= Value.Get("killsteal.e.prediction"))
                        {
                            if (target.Health + target.AttackShield <
                                Player.Instance.GetSpellDamage(target, SpellSlot.E))
                            {
                                _e.Cast(_e.GetPrediction(target).CastPosition);
                            }
                        }
                    }

                    if (Value.Use("killsteal.r") && _r.IsReady())
                    {
                        if (Player.Instance.CountEnemiesInRange(800) >= Value.Get("killsteal.r.enemies") &&
                            Player.Instance.Distance(target) >= Value.Get("killsteal.r.safe"))
                        {
                            if (Player.Instance.ManaPercent >= Value.Get("killsteal.r.mana"))
                            {
                                if (target.Health + target.AttackShield <
                                    Player.Instance.GetSpellDamage(target, SpellSlot.R))

                                {
                                    _r.Cast(target);
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code KILLSTEAL)</font>");
            }

        }

        #endregion 

        #region EQButton
        private static void OnEqButton(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            if (args.NewValue && (TargetSelector.GetTarget(_q.Range, DamageType.Mixed) != null))
            {
                if (_q.IsReady())
                {
                    _q.Cast(_q.GetPrediction(TargetSelector.GetTarget(_q.Range, DamageType.Mixed)).CastPosition);
                }
                if (_e.IsReady())
                {
                    _e.Cast(_e.GetPrediction(TargetSelector.GetTarget(_e.Range, DamageType.Mixed)).CastPosition);
                }
            }
        }
        #endregion

        #region OnBuffGain

        public static void OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            if (Value.Use("harass.force.target") || Value.Use("combo.force.target"))
            {
                if (!sender.IsMe &&
                    args.Buff.DisplayName == "caitlynyordletrapsight" ||
                    args.Buff.DisplayName == "CaitlynEntrapmentMissile" ||
                    args.Buff.DisplayName == "caitlynyordletrapinternal")
                {
                    Orbwalker.ForcedTarget = sender;
                }
            }
        }

        #endregion

        #region OnBuffLose

        public static void OnBuffLose(Obj_AI_Base sender, Obj_AI_BaseBuffLoseEventArgs args)
        {
            if (Value.Use("harass.force.target") || Value.Use("combo.force.target"))
            {
                if (!sender.IsMe &&
                    args.Buff.DisplayName == "caitlynyordletrapsight" ||
                    args.Buff.DisplayName == "CaitlynEntrapmentMissile" ||
                    args.Buff.DisplayName == "caitlynyordletrapinternal")
                {
                    if (Orbwalker.ForcedTarget != null)
                    {
                        Orbwalker.ForcedTarget = null;
                    }
                }
            }
        }

        #endregion

        #region Damage

        private static float ComboDamage(Obj_AI_Base enemy)
        {
            float damage = Player.Instance.GetAutoAttackDamage(enemy);

            if (_q.IsReady())
            {
                damage += Player.Instance.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (_w.IsReady())
            {
                damage += Player.Instance.GetSpellDamage(enemy, SpellSlot.W);
            }

            if (_e.IsReady())
            {
                damage += Player.Instance.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (_r.IsReady())
            {
                damage += Player.Instance.GetSpellDamage(enemy, SpellSlot.R);
            }

            return damage;
        }

        #endregion

        #endregion

        #region Drawings
        private static void GameOnDraw(EventArgs args)
        {
            var colorQ = MainMenu._draw.GetColor("color.q");
            var widthQ = MainMenu._draw.GetWidth("width.q");
            var colorW = MainMenu._draw.GetColor("color.w");
            var widthW = MainMenu._draw.GetWidth("width.w");
            var colorE = MainMenu._draw.GetColor("color.e");
            var widthE = MainMenu._draw.GetWidth("width.e");
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
                if (Value.Use("draw.w") && ((Value.Use("draw.ready") && _w.IsReady()) || !Value.Use("draw.ready")))
                {
                    new Circle
                    {
                        Color = colorW,
                        Radius = _w.Range,
                        BorderWidth = widthW
                    }.Draw(Player.Instance.Position);
                }
                if (Value.Use("draw.e") && ((Value.Use("draw.ready") && _e.IsReady()) || !Value.Use("draw.ready")))
                {
                    new Circle
                    {
                        Color = colorE,
                        Radius = _e.Range,
                        BorderWidth = widthE
                    }.Draw(Player.Instance.Position);
                }
                if (Value.Use("draw.r") && ((Value.Use("draw.ready") && _r.IsReady()) || !Value.Use("draw.ready")))
                {
                    new Circle
                    {
                        Color = colorR,
                        Radius = _q.Range,
                        BorderWidth = widthR
                    }.Draw(Player.Instance.Position);
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
                    var damagepercent = ((enemy.TotalShieldHealth() - damage) > 0
                        ? (enemy.TotalShieldHealth() - damage)
                        : 0) / (enemy.MaxHealth + enemy.AllShield + enemy.AttackShield + enemy.MagicShield);
                    var hppercent = enemy.TotalShieldHealth() /
                                    (enemy.MaxHealth + enemy.AllShield + enemy.AttackShield + enemy.MagicShield);
                    var start = new Vector2((int)(enemy.HPBarPosition.X + Offset.X + damagepercent * 104),
                        (int)(enemy.HPBarPosition.Y + Offset.Y) - 5);
                    var end = new Vector2((int)(enemy.HPBarPosition.X + Offset.X + hppercent * 104) + 2,
                        (int)(enemy.HPBarPosition.Y + Offset.Y) - 5);

                    Drawing.DrawLine(start, end, 9, Color.Chartreuse);
                }
            }
        }
        #endregion

    }
}
