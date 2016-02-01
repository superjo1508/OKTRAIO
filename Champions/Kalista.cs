using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using OKTRAIO.Menu_Settings;
using SharpDX;
using Activator = OKTRAIO.Utility.Activator;

namespace OKTRAIO.Champions
{
    internal class Kalista : AIOChampion
    {
        #region Initialize and Declare

        private static Spell.Skillshot _q;
        private static Spell.Targeted _w;
        private static Spell.Active _e, _r;
        private static readonly Vector2 Offset = new Vector2(1, 0);

        public override void Init()
        {
            try
            {
                //Creating Spells
                _q = new Spell.Skillshot(SpellSlot.Q, 1150, SkillShotType.Linear, 250, 2500, 40);
                _w = new Spell.Targeted(SpellSlot.W, 5000);
                _e = new Spell.Active(SpellSlot.E, 950);
                _r = new Spell.Active(SpellSlot.R, 1400);


                try
                {
                    //Combo Menu Settings
                    MainMenu.ComboKeys(useW: false, useR: false);
                    MainMenu._combo.AddSeparator();
                    MainMenu._combo.AddGroupLabel("Combo Preferences", "combo.grouplabel.addonmenu", true);
                    MainMenu._combo.AddCheckBox("combo.q.aa", "Use Q after autoattack", true, true);
                    MainMenu._combo.AddCheckBox("combo.q.collision", "Use Q checking collision", true, true);
                    MainMenu._combo.AddCheckBox("combo.q.minions", "Use Q through minions for hit the target", true,
                        true);
                    MainMenu._combo.AddCheckBox("combo.e.auto", "Use E if target leave the range", false, true);
                    MainMenu._combo.AddCheckBox("combo.e.death", "Use E if u are nearly to death", false, true);
                    MainMenu._combo.AddSlider("combo.e.range", "Min range for Use E {0} ", 600, 100, 600, true);
                    MainMenu._combo.AddSlider("combo.e.damage", "E Overkill", 60, 0, 500, true);
                    MainMenu._combo.AddSlider("combo.e.damage.me", "Use E if HP goes under {0} ", 10, 0, 100, true);
                    MainMenu._combo.AddCheckBox("combo.r.allin.prevent", "Use R if the ally can use R too", false, true);
                    MainMenu._combo.AddCheckBox("combo.r.allin", "Use R if u guys can kill those faggs", false, true);
                    MainMenu._combo.AddSeparator();
                    MainMenu._combo.AddGroupLabel("Prediction Settings", "combo.grouplabel.addonmenu.2", true);
                    MainMenu._combo.AddSlider("combo.q.prediction", "Use Q if Hitchance > {0}%", 80, 0, 100, true);
                    MainMenu._combo.AddSeparator();
                    MainMenu._combo.AddGroupLabel("Mana Manager:", "combo.grouplabel.addonmenu.3", true);
                    MainMenu._combo.AddCheckBox("combo.e.save.mana", "Save mana for Use E", true, true);
                    MainMenu._combo.AddCheckBox("combo.r.save.mana", "Save mana for Use R", true, true);
                    MainMenu.ComboManaManager(true, false, true, true, 20, 10, 10, 5);

                    //Lane Clear Menu Settings
                    MainMenu.LaneKeys(useW: false, useE: false, useR: false);
                    MainMenu._lane.AddSeparator();
                    MainMenu._lane.AddGroupLabel("Mana Manager:", "lane.grouplabel.addonmenu", true);
                    MainMenu.LaneManaManager(true, false, false, false, 80, 80, 80, 50);

                    //Jungle Clear Menu Settings
                    MainMenu.JungleKeys(useW: false, useR: false);
                    MainMenu._jungle.AddSeparator();
                    MainMenu._jungle.AddGroupLabel("Jungleclear Preferences", "jungle.grouplabel.addonmenu", true);
                    MainMenu._jungle.AddCheckBox("jungle.monsters.spell", "Use Abilities on Big Monster", true, true);
                    MainMenu._jungle.AddCheckBox("jungle.minimonsters.spell", "Use Abilities on Mini Monsters", false,
                        true);
                    MainMenu._jungle.AddCheckBox("jungle.monsters.baron", "Execute the Baron", true, true);
                    MainMenu._jungle.AddCheckBox("jungle.monsters.dragon", "Execute the Dragon", true, true);
                    MainMenu._jungle.AddCheckBox("jungle.monsters.blue", "Execute the Blue", true, true);
                    MainMenu._jungle.AddCheckBox("jungle.monsters.red", "Execute the Red", true, true);
                    MainMenu._jungle.AddCheckBox("jungle.monsters.gromp", "Execute the Gromp", true, true);
                    MainMenu._jungle.AddCheckBox("jungle.monsters.murkwolf", "Execute the Wolf", true, true);
                    MainMenu._jungle.AddCheckBox("jungle.monsters.razorbreak", "Execute the Bird", true, true);
                    MainMenu._jungle.AddCheckBox("jungle.monsters.krug", "Execute the Golem", true, true);
                    MainMenu._jungle.AddCheckBox("jungle.monsters.crab", "Execute the Crab", true, true);
                    MainMenu._jungle.AddSeparator();
                    MainMenu._jungle.AddGroupLabel("Mana Manager:", "jungle.grouplabel.addonmenu.2", true);
                    MainMenu.JungleManaManager(true, false, true, false, 40, 80, 50, 40);

                    //Last hit Menu Settings
                    MainMenu.LastHitKeys(useW: false, useR: false);
                    MainMenu._lasthit.AddSeparator();
                    MainMenu._lasthit.AddGroupLabel("LastHit Preferences", "lasthit.grouplabel.addonmenu", true);
                    MainMenu._lasthit.AddSlider("lasthit.q.count", "Execute {0} minions with Q", 2, 1, 10, true);
                    MainMenu._lasthit.AddSlider("lasthit.e.count", "Execute {0} minions with E", 4, 1, 10, true);
                    MainMenu._lasthit.AddGroupLabel("Mana Manager:", "lasthit.grouplabel.addonmenu.2", true);
                    MainMenu.LasthitManaManager(true, false, false, true, 70, 90, 60, 50);

                    //Harras
                    MainMenu.HarassKeys(useW: false, useR: false);
                    MainMenu._harass.AddSeparator();
                    MainMenu._harass.AddGroupLabel("Harass Preferences", "harass.grouplabel.addonmenu", true);
                    MainMenu._harass.AddCheckBox("harass.q.trough", "Use Q for transfer the E Stacks", true, true);
                    MainMenu._harass.AddCheckBox("harass.e.minions", "Use E on minions for Harass", true, true);
                    MainMenu._harass.AddCheckBox("harass.aa.mark", "Force AA to target with W Passive", true, true);
                    MainMenu._harass.AddSeparator();
                    MainMenu._harass.AddGroupLabel("Mana Manager:", "harass.grouplabel.addonmenu.2", true);
                    MainMenu.HarassManaManager(true, true, true, true, 60, 80, 50, 40);

                    //Flee Menu
                    MainMenu.FleeKeys(useW: false, useR: false);
                    MainMenu._flee.AddSeparator();
                    MainMenu._flee.AddGroupLabel("Flee Preferences", "flee.grouplabel.addonmenu", true);
                    MainMenu._flee.AddCheckBox("flee.q.wall", "Use Q for jump the wall", true, true);
                    MainMenu._flee.AddCheckBox("flee.aa.gapcloser", "Use AA for reach the target / run away", true, true);

                    //Ks
                    MainMenu.KsKeys(useW: false);
                    MainMenu._ks.AddSeparator();
                    MainMenu._ks.AddGroupLabel("Mana Manager:", "killsteal.grouplabel.addonmenu", true);
                    MainMenu.KsManaManager(true, false, true, false, 20, 30, 10, 5);

                    //Misc Menu
                    MainMenu.MiscMenu();
                    MainMenu._misc.AddCheckBox("misc.q.gapcloser", "Use Auto Q on GapCloser", false);
                    MainMenu._misc.AddCheckBox("misc.r.gapcloser", "Use Auto R on GapCloser if not braindead", false);
                    MainMenu._misc.AddCheckBox("misc.q", "Use Auto Q on CC");
                    MainMenu._misc.AddCheckBox("misc.w.auto", "Use Auto W", false);
                    MainMenu._misc.AddCheckBox("misc.r.save", "Use Auto R for save Ally");
                    MainMenu._misc.AddCheckBox("misc.aa.exploit", "Use DoubleJump Exploit");
                    MainMenu._misc.AddCheckBox("misc.passive.pact", "Use AutoPact", false);
                    MainMenu._misc.AddSeparator();
                    MainMenu._misc.AddGroupLabel("Auto Q Settings", "misc.grouplabel.addonmenu", true);
                    MainMenu._misc.AddCheckBox("misc.q.stun", "Use Q on Stunned Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.q.charm", "Use Q on Charmed Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.q.taunt", "Use Q on Taunted Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.q.fear", "Use Q on Feared Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.q.snare", "Use Q on Snared Enemy", true, true);
                    MainMenu._misc.AddSeparator();
                    MainMenu._misc.AddGroupLabel("W Settings", "misc.grouplabel.addonmenu.2", true);
                    MainMenu._misc.AddCheckBox("misc.w.alarm", "Alert if Sentinel is receiving damage", true, true);
                    MainMenu._misc.AddCheckBox("misc.w.alarm.sound", "Active Sentinel notification Sound", true, true);
                    MainMenu._misc.AddCheckBox("misc.w.interrupt", "Interrupt W if Combo Key pressed", true, true);

                    MainMenu._misc.Add("misc.w.dragon",
                        new KeyBind("Send sentinel to Drake", false, KeyBind.BindTypes.HoldActive, 'P'))
                        .OnValueChange += OnDrake;
                    Value.AdvancedMenuItemUiDs.Add("misc.w.dragon");
                    MainMenu._misc["misc.w.baron"].IsVisible =
                        MainMenu._misc["misc.advanced"].Cast<CheckBox>().CurrentValue;

                    MainMenu._misc.Add("misc.w.baron",
                        new KeyBind("Send sentinel to Baron", false, KeyBind.BindTypes.HoldActive, 'O'))
                        .OnValueChange += OnBaron;
                    Value.AdvancedMenuItemUiDs.Add("misc.w.baron");
                    MainMenu._misc["misc.w.baron"].IsVisible =
                        MainMenu._misc["misc.advanced"].Cast<CheckBox>().CurrentValue;

                    MainMenu._misc.AddSeparator();
                    MainMenu._misc.AddCheckBox("misc.r.save.prevent", "Prevent to cast R if ally is channeling", true,
                        true);
                    MainMenu._misc.AddSlider("misc.r.save.ally", "Use R if ally is under {0}  HP", 15, 0, 100, true);
                    MainMenu._misc.AddSeparator();
                    MainMenu._misc.AddGroupLabel("Mana Manager:", "misc.grouplabel.addonmenu.3", true);
                    MainMenu._misc.AddSlider("misc.q.mana", "Use Q on CC Enemy if Mana is above than {0}%", 30, 0, 100,
                        true);
                    MainMenu._misc.AddSlider("misc.w.mana", "Use W if Mana is above than {0}%", 70, 0, 100,
                        true);
                    MainMenu._misc.AddSlider("misc.r.mana", "Use R if Mana is above than {0}%", 30, 0, 100,
                        true);

                    //Draw Menu
                    MainMenu.DrawKeys();
                    MainMenu._draw.AddSeparator();
                    MainMenu._draw.AddCheckBox("draw.hp.bar", "Draw Combo Damage", true, true);
                    MainMenu._draw.AddCheckBox("draw.w.alarm.text", "Active Sentinel notification Text", true, true);
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

                Obj_AI_Base.OnBuffGain += BuffGain;
                Gapcloser.OnGapcloser += AntiGapCloser;
                Obj_AI_Base.OnProcessSpellCast += AaReset;
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
            throw new NotImplementedException();
        }

        #endregion

        #region Harass

        public override void Harass()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Laneclear

        public override void Laneclear()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Jungleclear

        public override void Jungleclear()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Flee

        public override void Flee()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Lasthit

        public override void LastHit()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Exploit

        private static void Exploit()
        {
            if (Value.Use("misc.aa.exploit") && Player.Instance.AttackDelay/1 > 1.70)
            {
                if (Value.Mode(Orbwalker.ActiveModes.Combo) || Value.Mode(Orbwalker.ActiveModes.Harass) ||
                    Value.Mode(Orbwalker.ActiveModes.LaneClear) || Value.Mode(Orbwalker.ActiveModes.JungleClear) ||
                    Value.Mode(Orbwalker.ActiveModes.Flee) || Value.Mode(Orbwalker.ActiveModes.LastHit))
                {
                    var target = TargetSelector.GetTarget(_q.Range + Player.Instance.GetAutoAttackRange() + 50,
                        DamageType.Physical);

                    if (Game.Time*1000 >= Orbwalker.LastAutoAttack + 1)
                    {
                        Orbwalker.OrbwalkTo(OKTRGeometry.SafeDashPosRework(_q.Range, target, 190));
                    }

                    if (Game.Time*1000 > Orbwalker.LastAutoAttack + Player.Instance.AttackDelay*1000 - 150)
                    {
                        Orbwalker.OrbwalkTo(OKTRGeometry.SafeDashPosRework(_q.Range, target, 190));
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Utils

        #region OnUpdate

        private void GameOnUpdate(EventArgs args)
        {
            if (Player.Instance.IsDead || Player.Instance.HasBuff("Recall")
                || Player.Instance.IsStunned || Player.Instance.IsRooted || Player.Instance.IsCharmed ||
                Orbwalker.IsAutoAttacking)
                return;

            Balista();
            //Tahmlista();
            Pact();
            AutoSentinel();
            AutoSave();
            Exploit();
        }

        #endregion

        #region Pact

        private static void Pact()
        {
            var ally = Variables.CloseAllies(Activator.KalistaSpear.Range).FirstOrDefault(a => Variables.IsSupport(a)
                                                                                               &&
                                                                                               !a.HasBuff(
                                                                                                   "kalistacoopstrikeally"));

            if (ally == null) return;

            if (Value.Use("misc.passive.pact")
                && Activator.KalistaSpear.IsOwned()
                && Shop.CanShop)
            {
                Activator.KalistaSpear.Cast(ally);
            }
        }

        #endregion

        #region AutoSave

        private static void AutoSave()
        {
            if (Value.Use("misc.r.save") && Player.Instance.ManaPercent >= Value.Get("misc.r.mana"))
            {
                var ally = Variables.CloseAllies(1000).FirstOrDefault(a => Variables.IsSupport(a)
                                                                           && a.HasBuff("kalistacoopstrikeally"));

                if (ally == null) return;

                var target = TargetSelector.GetTarget(_r.Range, DamageType.Mixed, ally.ServerPosition);

                if (Value.Get("misc.r.save.ally") >= ally.HealthPercent && ally.IsFacing(target))
                {
                    if (Value.Use("misc.r.save.prevent") && !ally.Spellbook.IsChanneling)
                    {
                        _r.Cast();
                    }
                    else
                    {
                        _r.Cast();
                    }
                }
            }
        }

        #endregion

        #region OnAntiGap

        private static void AntiGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            var ally = Variables.CloseAllies(1000).FirstOrDefault(a => Variables.IsSupport(a)
                                                                       && a.HasBuff("kalistacoopstrikeally"));

            if (!e.Sender.IsValidTarget() || e.Sender.Type != Player.Instance.Type || !e.Sender.IsEnemy)
                return;

            if (_r.IsReady() && Value.Use("misc.r.gapcloser") &&
                Player.Instance.ManaPercent <= Value.Get("misc.r.mana") &&
                (ally != null))
            {
                if (Value.Use("misc.r.save.prevent") && !ally.Spellbook.IsChanneling)
                {
                    _r.Cast();
                }
                else
                {
                    _r.Cast();
                }
            }

            if (_q.IsReady() && _q.IsInRange(sender) && Value.Use("misc.q.gapcloser"))
            {
                var pred = e.End;

                _q.Cast(_q.GetPrediction(sender).CastPosition);
                Orbwalker.OrbwalkTo(pred + 5*(Player.Instance.Position - e.End));
            }
        }

        #endregion

        #region Buffgain

        private static void BuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            if (Value.Use("misc.r.balista") && _r.IsReady())
            {
                var blizzzyboz =
                    EntityManager.Heroes.Enemies.Find(
                        a => a.ChampionName.Equals("Blitzcrank") && a.HasBuff("kalistacoopstrikeally"));

                if (blizzzyboz != null)
                {
                    if ((Player.Instance.Distance(blizzzyboz) >= Value.Get("misc.r.balista.range")) &&
                        _r.IsInRange(blizzzyboz)
                        && sender.HasBuff("rocketgrab2") && sender.IsEnemy)
                    {
                        _r.Cast();
                    }
                }
            }

            if (Value.Use("misc.q") && Player.Instance.ManaPercent >= Value.Get("misc.q.mana") && _q.IsInRange(sender))
            {
                if (Value.Use("misc.q.stun") && sender.IsStunned)
                {
                    _q.Cast(sender);
                }
                if (Value.Use("misc.q.snare") && sender.IsRooted)
                {
                    _q.Cast(sender);
                }
                if (Value.Use("misc.q.charm") && sender.IsCharmed)
                {
                    _q.Cast(sender);
                }
                if (Value.Use("misc.q.taunt") && sender.IsTaunted)
                {
                    _q.Cast(sender);
                }
                if (Value.Use("misc.q.fear") && sender.IsFeared)
                {
                    _q.Cast(sender);
                }
            }
        }

        #endregion

        #region AAReset

        private static void AaReset(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.Instance.IsInAutoAttackRange(sender) && sender.HasBuff("KalistaExpungeWrapper"))
            {
                Orbwalker.ResetAutoAttack();
            }
        }

        #endregion

        #region Balista

        private static
            void Balista()
        {
            if (EntityManager.Heroes.Allies.Any(a => a.ChampionName == "Blitzcrank"))
            {
                MainMenu._misc.AddGroupLabel("Balista Settings", "misc.grouplabel.addonmenu.3", true);
                MainMenu._misc.AddCheckBox("misc.r.balista", "Use Balista", true, true);
                MainMenu._misc.AddSlider("misc.r.balista.range", "Use Balista if the range is more then {0}", 600, 100,
                    1200, true);
                MainMenu._misc.AddSeparator();
            }
        }

        #endregion

        #region Tahmlista

        private static void Tahmlista()
        {
            if (EntityManager.Heroes.Allies.Any(a => a.ChampionName == "TahmKench"))
            {
                MainMenu._misc.AddGroupLabel("TahmLista Settings", "misc.grouplabel.addonmenu.4", true);
                MainMenu._misc.AddCheckBox("misc.r.tahmlista", "Use Tahmlista", true, true);
                MainMenu._misc.AddSlider("misc.r.balista.range", "Use Tahmlista if the range is more then {0}", 600, 100,
                    1200, true);
                MainMenu._misc.AddSeparator();
            }
        }

        #endregion

        #region Sentinel

        #region Vector Check

        private static bool HasSentinel(Vector3 castposition)
        {
            return
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(a => a.Name == "KalistaSentinel").Any(a => castposition.Distance(a.Position) <= 300);
        }

        private static void Wcast(Vector3 location)
        {
            if (!HasSentinel(location))
            {
                _w.Cast(location);
            }
        }

        #endregion

        private static void AutoSentinel()
        {
            if (Player.Instance.IsRecalling() && Game.MapId != GameMapId.SummonersRift) return;
            if (Value.Use("misc.w.interrupt")) return;
            if (Value.Use("misc.w.auto") && _w.IsReady() && Player.Instance.ManaPercent >= Value.Get("misc.w.mana"))
            {
                var kalista = Player.Instance.Position;
                var baron = new Vector2(5007.124f, 10471.45f);
                var dragon = new Vector2(9866.148f, 4414.014f);

                if (Player.Instance.CountEnemiesInRange(_q.Range) == 0)
                {
                    //Baron
                    if (kalista.Distance(baron) < _w.Range)
                    {
                        Wcast(baron.To3D());
                    }
                    //Dragon
                    else if (kalista.Distance(dragon) < _w.Range)
                    {
                        Wcast(dragon.To3D());
                    }
                }
            }
        }

        #region Drakebutton

        private static void OnDrake(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            var dragon = new Vector2(9866.148f, 4414.014f);

            if (args.NewValue && Player.Instance.Position.Distance(dragon) < _w.Range)
            {
                Wcast(dragon.To3D());
            }
        }

        #endregion

        #region Baronbutton

        private static void OnBaron(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            var baron = new Vector2(5007.124f, 10471.45f);

            if (args.NewValue && Player.Instance.Position.Distance(baron) < _w.Range)
            {
                Wcast(baron.To3D());
            }
        }

        #endregion

        #endregion

        #endregion
    }
}