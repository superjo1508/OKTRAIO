using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using OKTRAIO.Database.Spell_Library;
using OKTRAIO.Menu_Settings;
using SharpDX;
using Color = System.Drawing.Color;
using Spell = EloBuddy.SDK.Spell;

namespace OKTRAIO.Champions
{
    class Sivir : AIOChampion
    {
        #region Initialize and Declare

        private static Spell.Active _r, _w, _e;
        private static Spell.Skillshot _q;
        private static readonly Vector2 Offset = new Vector2(1, 0);
        private static float _qmana, _wmana, _emana, _rmana;


        public override void Init()
        {
            try
            {
                //spells
                _q = new Spell.Skillshot(SpellSlot.Q, 1200, SkillShotType.Linear, 250, 1350, 90)
                {
                    AllowedCollisionCount = int.MaxValue
                };
                _w = new Spell.Active(SpellSlot.W, (uint)Player.Instance.GetAutoAttackRange());
                _e = new Spell.Active(SpellSlot.E);
                _r = new Spell.Active(SpellSlot.R, 1000);


                //menu

                //combo
                MainMenu.ComboKeys(true, true, false, true);
                MainMenu._combo.AddSeparator();
                MainMenu._combo.AddGroupLabel("Combo Preferences", "combo.grouplabel.addonmenu", true);
                MainMenu._combo.AddSlider("combo.r.enemy", "Min. {0} Enemies in Range for R", 3, 0, 5, true);
                MainMenu._combo.AddSlider("combo.r.ally", "Min. {0} Allys in Range for R", 3, 0, 5, true);
                MainMenu._combo.AddCheckBox("combo.mana.management", "Smart Mana Management", true, true);

                //flee
                MainMenu.FleeKeys(true, true, false, true);
                MainMenu._flee.AddSeparator();
                MainMenu._flee.AddGroupLabel("Mana Manager:", "flee.grouplabel.addonmenu", true);
                MainMenu.FleeManaManager(true, true, false, true, 20, 20, 0, 20);

                //lasthit
                MainMenu.LastHitKeys(true, false, false, false);
                MainMenu._lasthit.AddSeparator();
                MainMenu._lasthit.AddGroupLabel("Mana Manager:", "lasthit.grouplabel.addonmenu", true);
                MainMenu.LasthitManaManager(true, false, false, false, 20, 0, 0, 0);

                //laneclear
                MainMenu.LaneKeys(true, true, false, false);
                MainMenu._lane.AddSeparator();
                MainMenu._lane.AddSlider("lane.q.min", "Min. {0} Minions for Q", 3, 1, 7, true);
                MainMenu._lane.AddSlider("lane.w.min", "Min. {0} Minions for W", 3, 1, 7, true);
                MainMenu._lane.AddGroupLabel("Mana Manager:", "lane.grouplabel.addonmenu", true);
                MainMenu.LaneManaManager(true, true, false, false, 60, 60, 0, 0);

                //jungleclear
                MainMenu.JungleKeys(true, true, false, false);
                MainMenu._jungle.AddSeparator();
                MainMenu._jungle.AddSlider("jungle.q.min", "Min. {0} Minions for Q", 3, 1, 4, true);
                MainMenu._jungle.AddSlider("jungle.w.min", "Min. {0} Minions for W", 3, 1, 4, true);
                MainMenu._jungle.AddGroupLabel("Mana Manager:", "jungle.grouplabel.addonmenu", true);
                MainMenu.JungleManaManager(true, true, false, false, 60, 60, 0, 0);

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
                MainMenu._misc.AddGroupLabel("Auto E - Spell Settings", "misc.grouplabel.addonmenu", true);
                MainMenu._misc.AddSeparator();
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(a => a.Team != Player.Instance.Team))
                {
                    foreach (var spell in enemy.Spellbook.Spells.Where(a => (a.SData.TargettingType == SpellDataTargetType.Unit || a.SData.TargettingType == SpellDataTargetType.Cone || a.SData.TargettingType == SpellDataTargetType.Location || a.SData.TargettingType == SpellDataTargetType.Location2 || a.SData.TargettingType == SpellDataTargetType.Location3 || a.SData.TargettingType == SpellDataTargetType.LocationAoe || a.SData.TargettingType == SpellDataTargetType.LocationVector || a.SData.TargettingType == SpellDataTargetType.LocationSummon || a.SData.TargettingType == SpellDataTargetType.LocationTunnel) && !a.Name.Contains("summoner")))
                    {
                        if (spell.Slot == SpellSlot.Q)
                        {
                            MainMenu._misc.Add("spell" + spell.SData.Name, new CheckBox(enemy.BaseSkinName + " - Q - " + spell.SData.Name));
                        }
                        else if (spell.Slot == SpellSlot.W)
                        {
                            MainMenu._misc.Add("spell" + spell.SData.Name, new CheckBox(enemy.BaseSkinName + " - W - " + spell.SData.Name));
                        }
                        else if (spell.Slot == SpellSlot.E)
                        {
                            MainMenu._misc.Add("spell" + spell.SData.Name, new CheckBox(enemy.BaseSkinName + " - E - " + spell.SData.Name));
                        }
                        else if (spell.Slot == SpellSlot.R)
                        {
                            MainMenu._misc.Add("spell" + spell.SData.Name, new CheckBox(enemy.BaseSkinName + " - R - " + spell.SData.Name));
                        }
                    }
                }
                MainMenu._misc.AddSeparator();
                MainMenu._misc.AddSlider("misc.e.delay", "E Cast Delay", 500, 0, 4000, true);
                MainMenu._misc.AddSeparator();
                MainMenu._misc.AddGroupLabel("Auto Q Settings", "misc.grouplabel1.addonmenu", true);
                MainMenu._misc.AddSeparator();
                MainMenu._misc.AddCheckBox("misc.q.charm", "Use Q on Charmed Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.stun", "Use Q on Stunned Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.knockup", "Use Q on Knocked Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.snare", "Use Q on Snared Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.suppression", "Use Q on Suppressed Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.taunt", "Use Q on Taunted Enemy", true, true);
                MainMenu._misc.AddSeparator();
                MainMenu._misc.AddGroupLabel("Prediction", "combo.grouplabel2.addonmenu", true);
                MainMenu._misc.AddSlider("misc.Q.prediction", "Hitchance Percentage for Q", 80, 0, 100, true);
                

                //draw
                MainMenu.DrawKeys(true, false, false, true);
                MainMenu._draw.AddSeparator();
                MainMenu._draw.AddCheckBox("draw.hp.bar", "Draw Combo Damage", true, true);
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
                AIHeroClient.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;
                Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
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
            var targetq = TargetSelector.GetTarget(_q.Range, DamageType.Physical);
            var targetr = EntityManager.Heroes.Enemies.Where(a => a.IsValidTarget(1400));
            var ally = EntityManager.Heroes.Allies.Where(a => a.IsValidTarget(_r.Range - 50));
            if (targetq ==  null) { return;}
            var qpred = _q.GetPrediction(targetq);

            if (_q.IsReady() && Value.Use("combo.q") && Player.Instance.Mana > _qmana + _rmana)
            {
                if (qpred.HitChancePercent >= Value.Get("misc.q.prediction"))
                {
                    _q.Cast(qpred.CastPosition);
                }
            }

            if (_r.IsReady() && Value.Use("combo.r"))
            {
                if (ally.Count() >= Value.Get("combo.r.ally") && targetr.Count() >= Value.Get("combo.r.enemy"))
                {
                    _r.Cast();
                }
            }
        }

        public override void Harass()
        {
            var targetq = TargetSelector.GetTarget(_q.Range, DamageType.Physical);
            if (targetq == null) { return; }
            var qpred = _q.GetPrediction(targetq);

            if (_q.IsReady() && Value.Use("harass.q") && Player.Instance.ManaPercent >= Value.Get("harass.q.mana"))
            {
                if (qpred.HitChancePercent >= Value.Get("misc.q.prediction"))
                {
                    _q.Cast(qpred.CastPosition);
                }
            }
        }

        public override void Laneclear()
        {
            var minionq = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                Player.Instance.Position, _q.Range);
            var qfarm = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minionq, _q.Width, (int) _q.Range);

            if (_q.IsReady() && Value.Use("lane.q") && Player.Instance.ManaPercent >= Value.Get("lane.q.mana"))
            {
                if (qfarm.HitNumber >= Value.Get("lane.q.min"))
                {
                    _q.Cast(qfarm.CastPosition);
                }
            }
        }

        public override void Jungleclear()
        {
            var monsterq = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, _q.Range);
            var qfarm = EntityManager.MinionsAndMonsters.GetLineFarmLocation(monsterq, _q.Width, (int) _q.Range);
            var dragon =
                EntityManager.MinionsAndMonsters.Monsters.FirstOrDefault(
                    a => a.IsValidTarget(_q.Range) && (a.BaseSkinName == "SRU_Dragon" || a.BaseSkinName == "SRU_Dragon"));

            if (_q.IsReady() && Value.Use("jungle.q") && Player.Instance.ManaPercent >= Value.Get("jungle.q.mana"))
            {
                if (qfarm.HitNumber >= Value.Get("jungle.q.min"))
                {
                    _q.Cast(qfarm.CastPosition);
                }
            }

            else if (_q.IsReady() && Value.Use("jungle.q") && Player.Instance.ManaPercent >= Value.Get("jungle.q.mana"))
            {
                if (dragon != null)
                {
                    _q.Cast(dragon);
                }
            }
        }

        public override void Flee()
        {
            
        }

        public override void LastHit()
        {
            var minionq =
                EntityManager.MinionsAndMonsters.GetLaneMinions()
                    .OrderByDescending(a => a.MaxHealth)
                    .FirstOrDefault(a => a.IsValidTarget(_q.Range) && a.Health <= Player.Instance.GetSpellDamage(a, SpellSlot.Q));

            if (_q.IsReady() && Value.Use("lasthit.q") && Player.Instance.ManaPercent >= Value.Get("lasthit.q.mana"))
            {
                if (minionq != null && Player.Instance.IsInAutoAttackRange(minionq) &&
                    minionq.Health > Player.Instance.GetAutoAttackDamage(minionq, true))
                {
                    _q.Cast(minionq);
                }
                else if (minionq != null)
                {
                    _q.Cast(minionq);
                }
            }
        }

        private static void GameOnUpdate(EventArgs args)
        {
            Ks();

            AutoQcc();

            ManaManagement();
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (Player.Instance.IsInAutoAttackRange(target))
            {
                if (_w.IsReady() && Value.Use("combo.w") && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Player.Instance.Mana > _wmana + _rmana)
                {
                    _w.Cast();
                    Orbwalker.ResetAutoAttack();
                }

                if (_w.IsReady() && Value.Use("jungle.w") && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) && Player.Instance.ManaPercent >= Value.Get("jungle.w.mana"))
                {
                    var monsterw = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, _q.Range);
                    var dragon =
                        EntityManager.MinionsAndMonsters.Monsters.FirstOrDefault(
                            a =>
                                a.IsValidTarget(_q.Range) &&
                                (a.BaseSkinName == "SRU_Dragon" || a.BaseSkinName == "SRU_Dragon"));

                    if (monsterw.Count() >= Value.Get("jungle.w.min"))
                    {
                        _w.Cast();
                        Orbwalker.ResetAutoAttack();
                    }

                    else if (dragon != null && target.NetworkId == dragon.NetworkId)
                    {
                        _w.Cast();
                        Orbwalker.ResetAutoAttack();
                    }
                }

                if (_w.IsReady() && Value.Use("lane.w") && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && Player.Instance.ManaPercent >= Value.Get("lane.w.mana"))
                {
                    var minionw = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                        Player.Instance.Position, _q.Range);

                    if (minionw.Count() >= Value.Get("lane.w.min"))
                    {
                        _w.Cast();
                        Orbwalker.ResetAutoAttack();
                    }
                }
            }
        }

        private static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!MainMenu._misc["spell" + args.SData.Name].Cast<CheckBox>().CurrentValue || args.Target == null || sender == null || sender.IsAlly || sender.IsMe || args.SData.IsAutoAttack() || !_e.IsReady())
            {
                return;
            }

            if (args.Target == Player.Instance)
            {
                Core.DelayAction(() => _e.Cast(), Value.Get("misc.e.delay"));
            }
            else if (args.End.Distance(Player.Instance.ServerPosition) < Player.Instance.BoundingRadius * 2)
            {
                Core.DelayAction(() => _e.Cast(), Value.Get("misc.e.delay"));
            }
        }

        #endregion
        #region Utils

        private static float QMaxDamage(Obj_AI_Base target)
        {
            if (_q.IsLearned)
            {
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical,
                    new[] { 46.25f, 83.25f, 120.25f, 159.1f, 194.25f }[_q.Level - 1] + (new[] { 1.295f, 1.48f, 1.665f, 1.85f, 2.035f }[_q.Level - 1] * Player.Instance.TotalAttackDamage));
            }
            return 0f;
        }

        private static float ComboDamage(Obj_AI_Base target)
        {
            var damage = Player.Instance.GetAutoAttackDamage(target);

            if (_q.IsReady())
            {
                damage += QMaxDamage(target);
            }
            return damage;
        }

        private static void AutoQcc()
        {
            var targetq =
                EntityManager.Heroes.Enemies.FirstOrDefault(
                    a => a.IsValidTarget(_q.Range) &&
                         (a.HasBuffOfType(BuffType.Charm) || a.HasBuffOfType(BuffType.Knockup) ||
                          a.HasBuffOfType(BuffType.Snare) || a.HasBuffOfType(BuffType.Stun) ||
                          a.HasBuffOfType(BuffType.Suppression) || a.HasBuffOfType(BuffType.Taunt)));


            if (_q.IsReady() && targetq != null)
            {
                if (Value.Use("misc.q.charm") && targetq.IsCharmed ||
                    Value.Use("misc.q.knockup") ||
                    Value.Use("misc.q.stun") && targetq.IsStunned ||
                    Value.Use("misc.q.snare") && targetq.IsRooted ||
                    Value.Use("misc.q.suppression") && targetq.IsSuppressCallForHelp ||
                    Value.Use("misc.q.taunt") && targetq.IsTaunted)
                {
                    _q.Cast(_q.GetPrediction(targetq).CastPosition);
                }
            }
        }

        private static void Ks()
        {
            var ksq =
                EntityManager.Heroes.Enemies.FirstOrDefault(
                    a =>
                        a.IsValidTarget(_q.Range) && !a.IsZombie && !a.IsDead &&
                        !a.HasBuffOfType(BuffType.Invulnerability) && !a.HasBuff("ChronoShift") && a.TotalShieldHealth() <= QMaxDamage(a));

            if (_q.IsReady() && Value.Use("killsteal.q") && Player.Instance.ManaPercent >= Value.Get("killsteal.q.mana"))
            {
                if (ksq != null)
                {
                    _q.Cast(_q.GetPrediction(ksq).CastPosition);
                }
            }
        }

        private static void ManaManagement()
        {
            if (Value.Use("combo.mana.management") && Player.Instance.HealthPercent > 20)
            {
                _qmana = _q.Handle.SData.Mana;
                _wmana = _w.Handle.SData.Mana;
                _emana = _e.Handle.SData.Mana;
                _rmana = _r.IsReady() ? _r.Handle.SData.Mana : 0;
            }
            else
            {
                _qmana = 0;
                _wmana = 0;
                _emana = 0;
                _rmana = 0;
            }
        }
        #endregion

        #region Drawings
        private static void GameOnDraw(EventArgs args)
        {
            Color colorQ = MainMenu._draw.GetColor("color.q");
            var widthQ = MainMenu._draw.GetWidth("width.q");
            Color colorR = MainMenu._draw.GetColor("color.r");
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

                if (Value.Use("draw.r") && ((Value.Use("draw.ready") && _r.IsReady()) || !Value.Use("draw.ready")))
                {
                    new Circle
                    {
                        Color = colorR,
                        Radius = _r.Range,
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
                    var damagepercent = ((enemy.TotalShieldHealth() - damage) > 0 ? (enemy.TotalShieldHealth() - damage) : 0) / (enemy.MaxHealth + enemy.AllShield + enemy.AttackShield + enemy.MagicShield);
                    var hppercent = enemy.TotalShieldHealth() / (enemy.MaxHealth + enemy.AllShield + enemy.AttackShield + enemy.MagicShield);
                    var start = new Vector2((int)(enemy.HPBarPosition.X + Offset.X + damagepercent * 104), (int)(enemy.HPBarPosition.Y + Offset.Y) - 5);
                    var end = new Vector2((int)(enemy.HPBarPosition.X + Offset.X + hppercent * 104) + 2, (int)(enemy.HPBarPosition.Y + Offset.Y) - 5);

                    Drawing.DrawLine(start, end, 9, Color.Chartreuse);
                }
            }
        }
        #endregion     
    }
}
