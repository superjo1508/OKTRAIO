using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using OKTRAIO.Menu_Settings;
using OKTRAIO.Utility;
using SharpDX;
using Color = System.Drawing.Color;

namespace OKTRAIO.Champions
{
    class Ezreal : AIOChampion
    {
        //Spells
        private static Spell.Skillshot _q, _w, _r;
        private static Spell.Targeted _e;
        private int _minionId;
        private static readonly Vector2 Offset = new Vector2(1, 0);

        public override void Init()
        {
            //Spells

            _q = new Spell.Skillshot(SpellSlot.Q, 1150, SkillShotType.Linear, 250, 2000, (int) 60f);
            _w = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Linear, 250, 1600, (int) 80f)
            {
                AllowedCollisionCount = int.MaxValue
            };
            _e = new Spell.Targeted(SpellSlot.E, 475);
            _r = new Spell.Skillshot(SpellSlot.R, 3000, SkillShotType.Linear, 1000, 2000, (int) 160f);


            try
            {
                //Menu Init
                //Combo
                MainMenu.ComboKeys(true, true, true, false);
                MainMenu._combo.AddSeparator();
                MainMenu._combo.AddGroupLabel("Combo Preferences", "combo.grouplabel.mode", true);
                MainMenu._combo.Add("combo.mode", new Slider("Combo Mode", 0, 0, 1)).OnValueChange += ModeSlider;
                Value.AdvancedMenuItemUiDs.Add("combo.mode");
                MainMenu._combo["combo.mode"].IsVisible = MainMenu._combo["combo.advanced"].Cast<CheckBox>().CurrentValue;
                MainMenu._combo.Add("combo.emode", new Slider("E Mode: ", 0, 0, 2)).OnValueChange += ComboEModeSlider;
                Value.AdvancedMenuItemUiDs.Add("combo.emode");
                MainMenu._combo["combo.emode"].Cast<Slider>().IsVisible = MainMenu._combo["combo.advanced"].Cast<CheckBox>().CurrentValue;
                MainMenu._combo.Add("combo.rbind",
                new KeyBind("Semi-Auto R (No Lock)", false, KeyBind.BindTypes.HoldActive, 'T'))
                .OnValueChange += OnUltButton;
                Value.AdvancedMenuItemUiDs.Add("combo.rbind");
                MainMenu._combo["combo.rbind"].IsVisible = MainMenu._combo["combo.advanced"].Cast<CheckBox>().CurrentValue;
                MainMenu._combo.AddSeparator();
                MainMenu._combo.AddGroupLabel("Prediction Settings", "combo.advanced.predctionlabel", true);
                MainMenu._combo.AddSlider("combo.q.pred", "Use Q if HitChance is above than {0}%", 45, 0, 100, true);
                MainMenu._combo.AddSlider("combo.w.pred", "Use W if HitChance is above than {0}%", 30, 0, 100, true);
                MainMenu._combo.AddSlider("combo.r.pred", "Use R if HitChance is above than {0}%", 70, 0, 100, true);
                MainMenu._combo.AddSeparator();
                MainMenu._combo.AddGroupLabel("Mana Manager:", "combo.advanced.manamanagerlabel", true);
                MainMenu.ComboManaManager(true, true, true, true, 10, 10, 10, 10);

                //Harass
                MainMenu.HarassKeys(true, true, true, false);
                MainMenu._harass.AddSeparator();
                MainMenu._harass.AddGroupLabel("Harass Preferences", "harass.grouplabel.mode", true);
                MainMenu._harass.AddCheckBox("harass.auto", "Use AUTO HARASS", false, true);
                MainMenu._harass.Add("harass.emode", new Slider("E Mode: ", 0, 0, 2)).OnValueChange += HarassEModeSlider;
                Value.AdvancedMenuItemUiDs.Add("harass.emode");
                MainMenu._harass["harass.emode"].IsVisible = MainMenu._harass["harass.advanced"].Cast<CheckBox>().CurrentValue;
                MainMenu._harass.AddSeparator();
                MainMenu._harass.AddGroupLabel("Prediction Settings", "harass.advanced.predctionlabel", true);
                MainMenu._harass.AddSlider("harass.q.pred", "Use Q if HitChance is above than {0}%", 45, 0, 100, true);
                MainMenu._harass.AddSlider("harass.w.pred", "Use W if HitChance is above than {0}%", 30, 0, 100, true);
                MainMenu._harass.AddSeparator();
                MainMenu._harass.AddGroupLabel("Mana Manager:", "harass.advanced.manamanagerlabel", true);
                MainMenu.HarassManaManager(true, true, true, false, 60, 60, 0, 0);
                
                //Farm
                MainMenu.LaneKeys(true, true, false, false);
                MainMenu._lane.AddSeparator();
                MainMenu._lane.AddGroupLabel("Q Settings", "lane.advanced.qsettingslabel", true);
                MainMenu._lane.AddCheckBox("lane.q.aa", "Use Q only when can't AA", true, true);
                MainMenu._lane.AddSeparator();
                MainMenu._lane.AddGroupLabel("Mana Manager:", "harass.advanced.manamanagerlabel", true);
                MainMenu.LaneManaManager(true, true, false, false, 65, 0, 0, 0);

                //Jungle Clear Menu Settings
                MainMenu.JungleKeys(true, true, true, false);
                MainMenu._jungle.AddSeparator();
                MainMenu._jungle.AddGroupLabel("Jungleclear Preferences", "jungle.grouplabel.1", true);
                MainMenu._jungle.AddCheckBox("jungle.monsters.spell", "Use Abilities on Big Monster", true, true);
                MainMenu._jungle.AddCheckBox("jungle.minimonsters.spell", "Use Abilities on Mini Monsters", false, true);
                MainMenu._jungle.AddSeparator();
                MainMenu._jungle.AddGroupLabel("Mana Manager:", "jungle.grouplabel.2", true);
                MainMenu.JungleManaManager(true, true, true, false, 60, 50, 40, 50);

                //Last hit Menu Settings
                MainMenu.LastHitKeys(true, false, false, false);
                MainMenu._lasthit.AddSeparator();
                MainMenu._lasthit.AddGroupLabel("Mana Manager:", "lasthit.grouplabel.1", true);
                MainMenu.LasthitManaManager(true, false, false, false, 60, 50, 40, 50);

                //Ks
                MainMenu.KsKeys(true, true, true, true);
                MainMenu._ks.AddSeparator();
                MainMenu._ks.AddGroupLabel("Mana Manager:", "killsteal.grouplabel.5", true);
                MainMenu.KsManaManager(true, true, true, true, 60, 50, 40, 50);

                //Flee Menu
                MainMenu.FleeKeys(false, false, true, false);

                //Misc
                MainMenu.MiscMenu();
                MainMenu._misc.AddSeparator();
                MainMenu._misc.AddCheckBox("misc.q", "Use Auto Q");
                MainMenu._misc.AddCheckBox("misc.w", "Use Auto W");
                MainMenu._misc.AddCheckBox("misc.e.gapcloser", "Use Auto E on GapCloser", false);
                MainMenu._misc.AddCheckBox("misc.e.gapcloser.wall", "Safe GapCloser E (Wall)");
                MainMenu._misc.AddSeparator();
                MainMenu._misc.AddGroupLabel("Auto Q-W Settings", "misc.grouplabel.addonmenu", true);
                MainMenu._misc.AddCheckBox("misc.w.ally", "Use W on Allies/Yourself", false, true);
                MainMenu._misc.AddCheckBox("misc.q.stun", "Use Q on Stunned Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.w.stun", "Use W on Stunned Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.charm", "Use Q on Charmed Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.w.charm", "Use W on Charmed Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.taunt", "Use Q on Taunted Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.w.taunt", "Use W on Taunted Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.fear", "Use Q on Feared Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.w.fear", "Use W on Feared Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.q.snare", "Use Q on Snared Enemy", true, true);
                MainMenu._misc.AddCheckBox("misc.w.snare", "Use W on Snared Enemy", true, true);
                MainMenu._misc.AddSeparator();
                MainMenu._misc.AddGroupLabel("Mana Manager:", "misc.grouplabel.addonmenu.1", true);
                MainMenu._misc.AddSlider("misc.q.mana", "Use Q on CC Enemy if Mana is above than {0}%", 30, 0, 100,
                    true);
                MainMenu._misc.AddSlider("misc.w.mana", "Use W on CC Enemy if Mana is above than {0}%", 30, 0, 100,
                    true);
                MainMenu.DrawKeys(true, true, true, true);
                MainMenu._draw.AddCheckBox("draw.hp.bar", "Draw Combo Damage", true, true);
                MainMenu.DamageIndicator(true);
                UtilityMenu.BaseUltMenu();
                


                //Value
                Value.Init();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code MENU)</font>");
            }
            try
            {
                Drawing.OnDraw += GameOnDraw;
                Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
                Orbwalker.OnPreAttack += OrbwalkerOnOnPreAttack;
                Orbwalker.OnPostAttack += OrbwalkerOnOnPostAttack;
                if (MainMenu._menu["useonupdate"].Cast<CheckBox>().CurrentValue)
                {
                    Game.OnUpdate += GameOnUpdate;
                }
                else
                {
                    Game.OnTick += GameOnUpdate;
                }
                Drawing.OnEndScene += Drawing_OnEndScene;
                BaseUlt.Initialize();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code 503)</font>");
            }
        }


      

        private void OrbwalkerOnOnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (_minionId != target.NetworkId) return;
            _minionId = target.NetworkId;
        }

        private void OrbwalkerOnOnPreAttack(AttackableUnit target, EventArgs args)
        {
            if (!target.IsMe) return;
            if (_minionId != target.NetworkId) return;
            _minionId = target.NetworkId;
            if (_w.IsReady() && Value.Use("lane.w") && (target.Type == GameObjectType.obj_AI_Turret) &&
                target.IsValid && (Player.Instance.ManaPercent >= Value.Get("lane.w.mana")))
            {
                foreach (var allies in EntityManager.Heroes.Allies)
                {
                    if ((allies.Distance(Player.Instance.Position) < 600) && !allies.IsMe && allies.IsAlly )
                    {
                        _w.Cast(allies);
                    }
                }
            }
        }

        private void GameOnUpdate(EventArgs args)
        {
            if (Player.Instance.IsDead || Player.Instance.HasBuff("Recall")
                || Player.Instance.IsStunned || Player.Instance.IsRooted || Player.Instance.IsCharmed ||
                Orbwalker.IsAutoAttacking)
                return;

            if (Value.Use("harass.auto"))
            {
                AutoHarass();
            }

            if ((Value.Use("misc.q") && _q.IsReady()) ||
                (Value.Use("misc.w") && _w.IsReady()))
            {
                AutoQwCc();
            }

            if ((Value.Use("killsteal.q") && _q.IsReady()) ||
                (Value.Use("killsteal.w") && _w.IsReady()) ||
                (Value.Use("killsteal.e") && _e.IsReady()) ||
                (Value.Use("killsteal.r") && _r.IsReady()))
            {
                KillSteal();
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

        private void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!e.Sender.IsValidTarget() || (e.Sender.Type != Player.Instance.Type) || !e.Sender.IsEnemy)
                return;

            if (Value.Use("misc.e.gapcloser"))
            {
                if (Value.Use("misc.e.gapcloser.wall"))
                {
                    _e.Cast(DetectWall());
                }
                else
                {
                    _e.Cast(Player.Instance.Position.Extend(Game.CursorPos, _e.Range).To3D());
                }
            }
        }
        
        #region Drawings

        private static void GameOnDraw(EventArgs args)
        {
            Color colorW = MainMenu._draw.GetColor("color.w");
            var widthW = MainMenu._draw.GetWidth("width.w");
            Color colorE = MainMenu._draw.GetColor("color.e");
            var widthE = MainMenu._draw.GetWidth("width.e");
            Color colorR = MainMenu._draw.GetColor("color.r");
            var widthR = MainMenu._draw.GetWidth("width.r");

            if (!Value.Use("draw.disable"))
            {
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

        #endregion
        private static bool InsideCone()
        {
            var target = TargetSelector.GetTarget(_e.Range, DamageType.Mixed);
            if (target == null) return false;
            var cone = new Geometry.Polygon.Sector(Player.Instance.Position,
                            OKTRGeometry.Deviation(Player.Instance.Position.To2D(), target.Position.To2D(), 90).To3D(), Geometry.DegreeToRadian(180),
                            (Player.Instance.AttackRange + Player.Instance.BoundingRadius * 2));
            return cone.IsInside(Game.CursorPos.To2D());
        }

        private static void ComboEModeSlider(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            UpdateSlider(2);
        }

        private static void ModeSlider(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            UpdateSlider(3);
        }

        private static void HarassEModeSlider(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            UpdateSlider(4);
        }

        private static void UpdateSlider(int id)
        {
            try
            {
                string displayName;
                if (id == 2)
                {
                    displayName = "E Mode: ";

                    if (Value.Get("combo.emode") == 0)
                    {
                        displayName = displayName + "Safe";
                    }
                    else if (Value.Get("combo.emode") == 1)
                    {
                        displayName = displayName + "Burst";
                    }
                    else if (Value.Get("combo.emode") == 2)
                    {
                        displayName = displayName + "To Mouse";
                    }
                    MainMenu._combo["combo.emode"].Cast<Slider>().DisplayName = displayName;
                }
                else if (id == 3)
                {
                    displayName = "Combo Mode: ";

                    if (Value.Get("combo.mode") == 0)
                    {
                        displayName = displayName + "Burst (E->Q->W->R)";
                    }
                    else if (Value.Get("combo.mode") == 1)
                    {
                        displayName = displayName + "Normal (Q->W->E->R)";
                    }
                    MainMenu._combo["combo.mode"].Cast<Slider>().DisplayName = displayName;
                }
                else if (id == 4)
                {
                    displayName = "E Mode: ";

                    if (Value.Get("harass.emode") == 0)
                    {
                        displayName = displayName + "Safe";
                    }
                    else if (Value.Get("harass.emode") == 1)
                    {
                        displayName = displayName + "Burst";
                    }
                    else if (Value.Get("harass.emode") == 2)
                    {
                        displayName = displayName + "To Mouse";
                    }
                    MainMenu._harass["harass.emode"].Cast<Slider>().DisplayName = displayName;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code anal)</font>");
            }
        }

        private static void OnUltButton(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            if (args.NewValue && (TargetSelector.GetTarget(_r.Range, DamageType.Mixed) != null))
                _r.Cast(_r.GetPrediction(TargetSelector.GetTarget(_r.Range, DamageType.Mixed)).CastPosition);
        }
        private static void ELogic()
        {
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Mixed);
            if (target == null) return;
            
            if (Value.Mode(Orbwalker.ActiveModes.Combo))
            {
                if (Value.Get("combo.emode") == 0)
                {
                    var ePos = OKTRGeometry.SafeDashLogic(_e.Range);
                    if (ePos != Vector3.Zero)
                        _e.Cast(ePos);
                }
                else if (Value.Get("combo.emode") == 1)
                {
                    if ((Game.CursorPos.Distance(Player.Instance.Position) >
                        Player.Instance.AttackRange + Player.Instance.BoundingRadius * 2) &&
                        !Player.Instance.Position.Extend(Game.CursorPos, _e.Range).IsUnderTurret())
                    {
                        _e.Cast(Player.Instance.Position.Extend(Game.CursorPos, _e.Range).To3D());
                    }
                    else
                    {
                        if (InsideCone())
                        {
                            _e.Cast(
                                OKTRGeometry.Deviation(Player.Instance.Position.To2D(), target.Position.To2D(), -65)
                                    .To3D());
                        }
                        else
                        {
                            _e.Cast(
                                OKTRGeometry.Deviation(Player.Instance.Position.To2D(), target.Position.To2D(), 65)
                                    .To3D());
                        }
                    }
                }
                else if (Value.Get("combo.emode") == 2)
                {
                    _e.Cast(Game.CursorPos);
                }
            }
            else
            {
                if (Value.Get("harass.emode") == 0)
                {
                    var ePos = OKTRGeometry.SafeDashLogic(_e.Range);
                    if (ePos != Vector3.Zero)
                        _e.Cast(ePos);
                }
                else if (Value.Get("harass.emode") == 1)
                {
                    if ((Game.CursorPos.Distance(Player.Instance.Position) >
                        Player.Instance.AttackRange + Player.Instance.BoundingRadius * 2) &&
                        !Player.Instance.Position.Extend(Game.CursorPos, _e.Range).IsUnderTurret())
                    {
                        _e.Cast(Player.Instance.Position.Extend(Game.CursorPos, _e.Range).To3D());
                    }
                    else
                    {
                        if (InsideCone())
                        {
                            _e.Cast(
                                OKTRGeometry.Deviation(Player.Instance.Position.To2D(), target.Position.To2D(), 255)
                                    .To3D());
                        }
                        else
                        {
                            _e.Cast(
                                OKTRGeometry.Deviation(Player.Instance.Position.To2D(), target.Position.To2D(), 65)
                                    .To3D());
                        }
                    }
                }
                else if (Value.Get("harass.emode") == 2)
                {
                    _e.Cast(Game.CursorPos);
                }
            }
        }

        public override void Combo()
        {
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Mixed);

            if (target == null) return;

            if (Value.Get("combo.mode") == 0)
            {
                if (_e.IsReady() && Value.Use("combo.e"))
                {
                    if (Player.Instance.ManaPercent >= Value.Get("combo.e.mana"))
                    {
                        ELogic();
                    }
                }
                else if (_q.IsReady() && Value.Use("combo.q"))
                {
                    if ((_q.GetPrediction(target).HitChancePercent >= Value.Get("combo.q.pred")) &&
                        (Player.Instance.ManaPercent >= Value.Get("combo.q.mana")))
                    {
                        _q.Cast(_q.GetPrediction(target).CastPosition);
                    }
                }
                else if (_w.IsReady() && Value.Use("combo.w"))
                {
                    var ally =
                       EntityManager.Heroes.Allies
                           .FirstOrDefault(x => x.IsValidTarget(_w.Range));

                    if (Value.Use("misc.w.ally"))
                    {
                        if (Player.Instance.Distance(ally) <= _w.Range)
                        {
                            _w.Cast(_w.GetPrediction(ally).CastPosition);
                        }
                        else
                        {
                            _w.Cast(Game.CursorPos);
                        }
                    }
                    else if ((_w.GetPrediction(target).HitChancePercent >= Value.Get("combo.w.pred")) &&
                        (Player.Instance.ManaPercent >= Value.Get("combo.w.mana")))
                    {
                        _w.Cast(_w.GetPrediction(target).CastPosition);
                    }
                }
            }
            else
            {
                if (_q.IsReady() && Value.Use("combo.q"))
                {
                    if ((_q.GetPrediction(target).HitChancePercent >= Value.Get("combo.q.pred")) &&
                        (Player.Instance.ManaPercent >= Value.Get("combo.q.mana")))
                    {
                        _q.Cast(_q.GetPrediction(target).CastPosition);
                    }
                }
                else if (_w.IsReady() && Value.Use("combo.w"))
                {
                    if ((_w.GetPrediction(target).HitChancePercent >= Value.Get("combo.w.pred")) &&
                        (Player.Instance.ManaPercent >= Value.Get("combo.w.mana")))
                    {
                        _w.Cast(_w.GetPrediction(target).CastPosition);
                    }
                }
                else if (_e.IsReady() && Value.Use("combo.e"))
                {
                    if (Player.Instance.ManaPercent >= Value.Get("combo.e.mana"))
                    {
                        ELogic();
                    }
                }
            }
        }


        public override void Harass()
        {
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Mixed);

            if (target == null) return;

            if (_q.IsReady() && Value.Use("harass.q"))
            {
                if ((_q.GetPrediction(target).HitChancePercent >= Value.Get("harass.q.pred")) &&
                    (Player.Instance.ManaPercent >= Value.Get("harass.q.mana")))
                {
                    _q.Cast(_q.GetPrediction(target).CastPosition);
                }
            }
            else if (_w.IsReady() && Value.Use("harass.w"))
            {
                var ally =
                       EntityManager.Heroes.Allies
                           .FirstOrDefault(x => x.IsValidTarget(_w.Range));

                if (Value.Use("misc.w.ally"))
                {
                    if (Player.Instance.Distance(ally) <= _w.Range)
                    {
                        _w.Cast(_w.GetPrediction(ally).CastPosition);
                    }
                    else
                    {
                        _w.Cast(Game.CursorPos);
                    }
                }

                else if ((_w.GetPrediction(target).HitChancePercent >= Value.Get("harass.w.pred")) &&
                    (Player.Instance.ManaPercent >= Value.Get("harass.w.mana")))
                {
                    _w.Cast(_w.GetPrediction(target).CastPosition);
                }
            }
            else if (_e.IsReady() && Value.Use("harass.e"))
            {
                if (Player.Instance.ManaPercent >= Value.Get("harass.e.mana"))
                {
                    ELogic();
                }
            }
        }

        public override void Laneclear()
        {
            //only q when cant aa
            var source =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                    Player.Instance.ServerPosition,
                    _q.Range).OrderByDescending(a => a.MaxHealth).FirstOrDefault();

            if (source == null) return;

            if (_q.IsReady() && Value.Use("lane.q"))
            {
                if (Player.Instance.ManaPercent >= Value.Get("lane.q.mana"))
                {
                    if (Value.Use("lane.q.aa"))
                    {
                        if (Player.Instance.GetAutoAttackRange() >= source.Distance(Player.Instance))
                        {
                            _q.Cast(source);
                        }
                    }
                    else
                    {
                        _q.Cast(source);
                    }
                }
            }
        }

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

            if ((monsters == null) || (fappamonsters == null)) return;

            if (Value.Use("jungle.monsters.spell"))
            {
                if (Player.Instance.ManaPercent >= Value.Get("jungle.q.mana"))
                {
                    if (Value.Use("jungle.q") && _q.IsReady())
                    {
                        _q.Cast(monsters);
                    }
                }

                else if (_w.IsReady() && Value.Use("jungle.w"))
                {
                    if (Player.Instance.ManaPercent >= Value.Get("jungle.w.mana"))
                    {
                        _w.Cast(monsters);
                    }
                }

                else if (Player.Instance.ManaPercent >= Value.Get("jungle.e.mana"))
                {
                    if (Value.Use("jungle.e") && _e.IsReady())
                    {
                        _e.Cast(DetectWall());
                    }
                }
            }

            if (Value.Use("jungle.minimonsters.spell"))
            {
                if (Player.Instance.ManaPercent >= Value.Get("jungle.q.mana"))
                {
                    if (Value.Use("jungle.q") && _q.IsReady())
                    {
                        _q.Cast();
                    }
                }

                else if (_w.IsReady() && Value.Use("jungle.w"))
                {
                    if (Player.Instance.ManaPercent >= Value.Get("jungle.w.mana"))
                    {
                        _w.Cast(fappamonsters);
                    }
                }

                if (Player.Instance.ManaPercent >= Value.Get("jungle.e.mana"))
                {
                    if (Value.Use("jungle.e") && _w.IsReady())
                    {
                        _e.Cast(DetectWall());
                    }
                }

            }
        }

        public override void Flee()
        {
            if (_e.IsReady() && Value.Use("flee.e") && _e.IsLearned)
            {
                _e.Cast(Player.Instance.ServerPosition.Extend(Game.CursorPos, _e.Range).To3D());
            }
        }

        public override void LastHit()
        {
            var source =
                EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault
                (m =>
                        m.IsValidTarget(_q.Range) &&
                        (Player.Instance.GetSpellDamage(m, SpellSlot.Q) > m.TotalShieldHealth()));

            if (source == null) return;

            if (Player.Instance.ManaPercent >= Value.Get("lasthit.q.mana"))
            {
                if (Value.Use("lasthit.q") && _q.IsReady())
                {
                    _q.Cast(source);
                }
            }
        }

        private static void AutoHarass()
        {
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Mixed);

            if (_q.IsReady() && Value.Use("harass.q"))
            {
                if ((_q.GetPrediction(target).HitChancePercent >= Value.Get("harass.q.pred")) &&
                    (Player.Instance.ManaPercent >= Value.Get("harass.q.mana")))
                {
                    _q.Cast(_q.GetPrediction(target).CastPosition);
                }
            }
            else if (_w.IsReady() && Value.Use("harass.w"))
            {
                var ally =
                    EntityManager.Heroes.Allies
                        .FirstOrDefault(x => x.IsValidTarget(_w.Range));

                if (Value.Use("misc.w.ally"))
                {
                    if (Player.Instance.Distance(ally) <= _w.Range)
                    {
                        _w.Cast(_w.GetPrediction(ally).CastPosition);
                    }
                    else
                    {
                        _w.Cast(Game.CursorPos);
                    }
                }

                else if ((_w.GetPrediction(target).HitChancePercent >= Value.Get("harass.w.pred")) &&
                         (Player.Instance.ManaPercent >= Value.Get("harass.w.mana")))
                {
                    _w.Cast(_w.GetPrediction(target).CastPosition);
                }
            }
        }


        private static void AutoQwCc()
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

            if (enemy == null || enemy.IsValidTarget(_q.Range)) return;

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

        private static void KillSteal()
        {
            foreach (
                    var target in
                        EntityManager.Heroes.Enemies.Where(
                            hero =>
                                hero.IsValidTarget(1200) && !hero.IsDead && !hero.IsZombie &&
                                (hero.HealthPercent <= 25)))
            {
                if (Player.Instance.ManaPercent >= Value.Get("killsteal.q.mana"))
                {
                    if (target.Health + target.AttackShield <
                        Player.Instance.GetSpellDamage(target, SpellSlot.Q))

                    {
                        _q.Cast(_q.GetPrediction(target).CastPosition);
                    }
                }

                if (Player.Instance.ManaPercent >= Value.Get("killsteal.w.mana"))
                {
                    if (target.Health + target.AttackShield <
                        Player.Instance.GetSpellDamage(target, SpellSlot.W))

                    {
                        _w.Cast(_w.GetPrediction(target).CastPosition);
                    }
                }

                if (Player.Instance.ManaPercent >= Value.Get("killsteal.e.mana"))
                {
                    var tawah =
                        EntityManager.Turrets.Enemies.FirstOrDefault(
                            a =>
                                !a.IsDead && (a.Distance(target) <= 775 + Player.Instance.BoundingRadius +
                                target.BoundingRadius / 2 + 44.2));

                    if ((target.Health + target.AttackShield <
                        Player.Instance.GetSpellDamage(target, SpellSlot.E)) &&
                        (target.Position.CountEnemiesInRange(800) == 1) && (tawah == null) &&
                       (Player.Instance.Mana >= 120))
                    {
                        _e.Cast(target);
                    }
                }
                if (Player.Instance.ManaPercent >= Value.Get("killsteal.r.mana"))
                {
                    if (target.Distance(Player.Instance) <= 1200)
                    if (target.Health + target.AttackShield <
                        Player.Instance.GetSpellDamage(target, SpellSlot.R))
                    { 
                        _r.Cast(_r.GetPrediction(target).CastPosition);
                    }
                }
            }
        }

        private Vector3 DetectWall()
        {

            const int circleLineSegmentN = 20;

            var outRadius = 700 / (float)Math.Cos(2 * Math.PI / circleLineSegmentN);
            var inRadius = 300 / (float)Math.Cos(2 * Math.PI / circleLineSegmentN);
            var bestPoint = ObjectManager.Player.Position;
            for (var i = 1; i <= circleLineSegmentN; i++)
            {
                var angle = i * 2 * Math.PI / circleLineSegmentN;
                var point = new Vector2(ObjectManager.Player.Position.X + outRadius * (float)Math.Cos(angle), ObjectManager.Player.Position.Y + outRadius * (float)Math.Sin(angle)).To3D();
                var point2 = new Vector2(ObjectManager.Player.Position.X + inRadius * (float)Math.Cos(angle), ObjectManager.Player.Position.Y + inRadius * (float)Math.Sin(angle)).To3D();
                if (((point.ToNavMeshCell().CollFlags & CollisionFlags.Wall) != 0) &&
                    ((point2.ToNavMeshCell().CollFlags & CollisionFlags.Wall) != 0) &&
                    (Game.CursorPos.Distance(point) < Game.CursorPos.Distance(bestPoint)))
                {
                    bestPoint = point;
                    return bestPoint;
                } 
            }

            return new Vector3();
        }
    }
}