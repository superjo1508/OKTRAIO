using System;
using System.Collections.Generic;
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
    class Draven : AIOChampion
    {

        private static int AxeCount
        {
            get

            {
                return (Player.Instance.HasBuff("dravenspinningattack")
                    ? Player.Instance.Buffs.First(x => x.Name == "dravenspinningattack").Count
                    : 0) + Axes.Count;
            }
        }

        //Learning spells
        private static Spell.Active _q, _w;
        private static Spell.Skillshot _e, _r;
        private static readonly Vector2 Offset = new Vector2(1, 0);

        //LunarBlue method
        private static List<Axe> Axes { get; set; }
        private static Circle _axeLocation;


        public override void Init()
        {
            try
            {
                //Creating Spells

                _q = new Spell.Active(SpellSlot.Q);
                _w = new Spell.Active(SpellSlot.W);
                _e = new Spell.Skillshot(SpellSlot.E, 1100, SkillShotType.Linear, 250, 1400, 130)
                {
                    AllowedCollisionCount = int.MaxValue
                };
                _r = new Spell.Skillshot(SpellSlot.R, 20000, SkillShotType.Linear, 250, 2000, 160)
                {
                    AllowedCollisionCount = int.MaxValue
                };

                //LunarBlue method
                _axeLocation = new Circle();
                Axes = new List<Axe>();
                try
                {
                    //Combo Menu Settings
                    MainMenu.ComboKeys(true, true, true, true);
                    MainMenu._combo.AddSeparator();
                    MainMenu._combo.AddGroupLabel("Prediction Settings", "combo.grouplabel.2", true);
                    MainMenu._combo.AddSlider("combo.e.prediction", "Use E if Hitchance > {0}%", 80, 0, 100, true);
                    MainMenu._combo.AddSeparator();
                    MainMenu._combo.AddGroupLabel("Mana Manager:", "combo.grouplabel.3", true);
                    MainMenu.ComboManaManager(true, true, true, true, 10, 5, 10, 10);

                    //Lane Clear Menu Settings
                    MainMenu.LaneKeys(true, true, true, false);
                    MainMenu._lane.AddSeparator();
                    MainMenu._lane.AddGroupLabel("Mana Manager:", "lane.grouplabel.2", true);
                    MainMenu.LaneManaManager(true, true, true, false, 60, 50, 40, 50);

                    //Jungle Clear Menu Settings
                    MainMenu.JungleKeys(true, true, true, false);
                    MainMenu._jungle.AddSeparator();
                    MainMenu._jungle.AddGroupLabel("Jungleclear Preferences", "jungle.grouplabel.1", true);
                    MainMenu._jungle.AddCheckBox("jungle.monsters.spell", "Use Abilities on Big Monster", true, true);
                    MainMenu._jungle.AddCheckBox("jungle.minimonsters.spell", "Use Abilities on Mini Monsters", false,
                        true);
                    MainMenu._jungle.AddSeparator();
                    MainMenu._jungle.AddGroupLabel("Mana Manager:", "jungle.grouplabel.2", true);
                    MainMenu.JungleManaManager(true, true, true, false, 60, 50, 40, 50);

                    //Last hit Menu Settings
                    MainMenu.LastHitKeys(false, false, true, false);
                    MainMenu._lasthit.AddSeparator();
                    MainMenu._lasthit.AddGroupLabel("Mana Manager:", "lasthit.grouplabel.1", true);
                    MainMenu.LasthitManaManager(false, false, true, false, 60, 50, 40, 50);

                    //Harras
                    MainMenu.HarassKeys(true, true, true, false);
                    MainMenu._harass.AddSeparator();
                    MainMenu._harass.AddGroupLabel("Mana Manager:", "harass.grouplabel.1", true);
                    MainMenu.HarassManaManager(true, true, true, false, 60, 50, 40, 50);

                    //Flee Menu
                    MainMenu.FleeKeys(false, true, true, false);
                    MainMenu._flee.AddSeparator();
                    MainMenu._flee.AddGroupLabel("Mana Manager:", "flee.grouplabel.1", true);
                    MainMenu.FleeManaManager(false, true, true, false, 0, 20, 30, 0);

                    //Ks
                    MainMenu.KsKeys(false, false, true, true);
                    MainMenu._ks.AddSeparator();
                    MainMenu._ks.AddGroupLabel("Mana Manager:", "killsteal.grouplabel.5", true);
                    MainMenu.KsManaManager(false, false, true, true, 60, 50, 10, 25);

                    //Misc Menu
                    MainMenu.MiscMenu();
                    MainMenu._misc.AddCheckBox("misc.e.interrupter", "Use E for Interrupt");
                    MainMenu._misc.AddCheckBox("misc.e.gapcloser", "Use E for Anti-GapCloser");
                    MainMenu._misc.AddCheckBox("misc.e", "Use Auto E");
                    MainMenu._misc.AddSeparator();
                    MainMenu._misc.AddGroupLabel("Interrupter - AntiGapCloser settings", "misc.grouplabel.6", true);
                    MainMenu._misc.AddSlider("misc.e.interrupter.mana", "Min. Mana to interrupt", 30, 0, 100, true);
                    MainMenu._misc.AddSlider("misc.e.gapcloser.mana", "Min. Mana% to use R for Anti-GapCloser", 30, 0,
                        100, true);
                    MainMenu._misc.AddSeparator();
                    MainMenu._misc.AddGroupLabel("Auto E Settings", "misc.grouplabel.3", true);
                    MainMenu._misc.AddCheckBox("misc.e.tower", "Use E on enemies taking tower hits", true, true);
                    MainMenu._misc.AddCheckBox("misc.e.stun", "Use E on Stunned Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.e.charm", "Use E on Charmed Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.e.taunt", "Use E on Taunted Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.e.fear", "Use E on Feared Enemy", true, true);
                    MainMenu._misc.AddCheckBox("misc.e.snare", "Use E on Snared Enemy", true, true);
                    MainMenu._misc.AddSlider("misc.e.mana", "Use E on CC Enemy if Mana is above than {0}%", 10, 0, 100,
                        true);
                    MainMenu._misc.AddSeparator();
                    MainMenu._misc.AddGroupLabel("Axe Settings", "misc.grouplabel.4", true);
                    MainMenu._misc.AddCheckBox("misc.w.axe", "Use W if necessary", true, true);
                    MainMenu._misc.AddCheckBox("misc.axe.tower", "Do not Catch under Tower", true, true);
                    MainMenu._misc.AddCheckBox("misc.axe.arf", "I WANT THAT AXE!!", false, true);
                    MainMenu._misc.AddCheckBox("misc.axe.check", "Be safe while Catching", true, true);
                    MainMenu._misc.AddSlider("misc.axe.check.enemies", "Maximum Enemies for Catch", 2, 1, 5, true);
                    MainMenu._misc.AddSlider("misc.axe.count", "Maximum Axes", 2, 1, 3, true);
                    MainMenu._misc.AddSlider("misc.axe.range", "Catch Range", 800, 120, 1500, true);
                    MainMenu._misc.AddGroupLabel("Catch Axes if:", "misc.grouplabel.5", true);
                    MainMenu._misc.Add("misc.axe.mode", new Slider("Axe Catch Mode", 1, 1, 2)).OnValueChange +=

                        delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                        {
                            if (sender.Cast<Slider>().CurrentValue == 1)
                            {
                                sender.DisplayName = "Catch Method: Combo";
                            }
                            else if (sender.Cast<Slider>().CurrentValue == 2)
                            {
                                sender.DisplayName = "Catch Method: Any";
                            }
                            else if (sender.Cast<Slider>().CurrentValue == 3)
                            {
                                sender.DisplayName = "Catch Method: Orbwalking";
                            }
                        };

                    Value.AdvancedMenuItemUiDs.Add("misc.axe.mode");
                    MainMenu._misc["misc.axe.mode"].IsVisible =
                        MainMenu._misc["misc.advanced"].Cast<CheckBox>().CurrentValue;

                    //Need to start the Axe Manager code

                    //Draw Menu
                    MainMenu.DrawKeys(true, true, true, true);
                    MainMenu._draw.AddCheckBox("draw.axe.catch", "Draw Catche Range");
                    MainMenu._draw.AddSeparator();
                    MainMenu._draw.AddCheckBox("draw.hp.bar", "Draw Combo Damage", true, true);

                    //BaseUlt
                    UtilityMenu.BaseUltMenu();

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
                GameObject.OnCreate += GameObjectOnCreate;
                GameObject.OnDelete += GameObjectOnDelete;
                Interrupter.OnInterruptableSpell += InterrupterOnInterruptableSpell;
                Orbwalker.OnPreAttack += OrbwalkerOnPreAttack;
                Gapcloser.OnGapcloser += AntiGapCloser;
                if (MainMenu._menu["useonupdate"].Cast<CheckBox>().CurrentValue)
                {
                    Game.OnUpdate += GameOnUpdate;
                }
                else
                {
                    Game.OnTick += GameOnUpdate;
                }
                Drawing.OnDraw += GameOnDraw;
                Drawing.OnEndScene += Drawing_OnEndScene;
                BaseUlt.Initialize();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code INIT)</font>");
            }
        }

        private static void GameObjectOnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Draven_Base_Q_reticle_self.troy"))
            {
                Axes.Add(new Axe { Object = sender, ExpireTime = Game.Time + 1.8});

                Core.DelayAction(() => Axes.RemoveAll(x => x.Object.NetworkId == sender.NetworkId), 1800);
            }
        }

        private static void GameObjectOnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Draven_Base_Q_reticle_self.troy"))
            {
                Axes.RemoveAll(x => x.Object.NetworkId == sender.NetworkId);
            }
        }

        private static void InterrupterOnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            try
            {
                if (!sender.IsValidTarget(_e.Range) || e.DangerLevel != DangerLevel.Medium ||
                    e.Sender.Type != Player.Instance.Type || !e.Sender.IsEnemy ||
                    Player.Instance.ManaPercent <= Value.Get("misc.e.interrupter.mana"))
                    return;

                if (_e.IsReady() && _e.IsInRange(sender) && Value.Use("misc.e.interrupter"))
                {
                    var pred = _e.GetPrediction(sender);

                    _e.Cast(pred.CastPosition);
                }
            }

            catch (Exception a)
            {
                Console.WriteLine(a);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code INTERRUPTER)</font>");
            }
        }

        private static void OrbwalkerOnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (_q.IsReady())
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) ||
                    Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                    Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {
                    if (target is AIHeroClient)
                    {
                        if (Value.Use("combo.q") && AxeCount < Value.Get("misc.axe.count"))
                        {
                            _q.Cast();
                        }
                    }

                    if ((target as Obj_AI_Base).IsMinion)
                    {
                        if (Value.Use("lane.q") && AxeCount < Value.Get("misc.axe.count"))
                        {
                            _q.Cast();
                        }
                    }

                    if ((target as Obj_AI_Base).IsMonster)
                    {
                        if (Value.Use("jungle.q") && AxeCount < Value.Get("misc.axe.count"))
                        {
                            _q.Cast();
                        }
                    }
                }
            }
        }

        private static void AntiGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {

            try
            {
                if (!e.Sender.IsValidTarget() || !Value.Use("misc.e.gapcloser") || e.Sender.Type != Player.Instance.Type ||
                    !e.Sender.IsEnemy ||
                    Player.Instance.ManaPercent <= Value.Get("misc.e.gapcloser.mana"))
                    return;

                if (_e.IsReady() && _e.IsInRange(sender) && Value.Use("misc.e.gapcloser"))
                {
                    var pred = _e.GetPrediction(sender);

                    _e.Cast(pred.CastPosition);
                }
            }

            catch (Exception a)
            {
                Console.WriteLine(a);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code ANTIGAP)</font>");
            }
        }

        private static void GameOnUpdate(EventArgs args)
        {
            if (Player.Instance.IsDead || Player.Instance.HasBuff("Recall")
                || Player.Instance.IsStunned || Player.Instance.IsRooted || Player.Instance.IsCharmed ||
                Orbwalker.IsAutoAttacking)
                return;

            if (Value.Use("misc.e") && _e.IsReady())
            {
                AutoE();
            }

            if (Value.Use("misc.axe.tower") && _e.IsReady())
            {
                AutoETurret();
            }

            if (Value.Use("killsteal.e") && _e.IsReady() ||
                Value.Use("killsteal.r") && _r.IsReady())
            {
                KillSteal();
            }

            CatchAxe();
        }

        private static void AutoE()
        {

            var enemy =
                EntityManager.Heroes.Enemies.FirstOrDefault(
                    x =>
                        x.HasBuffOfType(BuffType.Charm) || x.HasBuffOfType(BuffType.Knockup) ||
                        x.HasBuffOfType(BuffType.Stun) || x.HasBuffOfType(BuffType.Suppression) ||
                        x.HasBuffOfType(BuffType.Snare));

            if (!_e.IsLearned || !_e.IsReady() || Orbwalker.IsAutoAttacking ||
                EntityManager.Turrets.Enemies.Count(t => t.IsValidTarget() && t.IsAttackingPlayer) > 0)
                return;

            if (enemy == null || !enemy.IsValidTarget(_e.Range)) return;

            try
            {
                if (Player.Instance.ManaPercent >= Value.Get("misc.e.mana"))
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


            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code AUTOE)</font>");
            }
        }

        private static void AutoETurret()
        {
            if (!Variables.CloseEnemies(_e.Range).Any() || !_e.IsReady()) return;
            foreach (
                AIHeroClient fuccboi in
                    EntityManager.Heroes.Enemies.Where(fuccboi => fuccboi.IsValidTarget(_e.Range)))
            {
                if (
                    !EntityManager.Turrets.Allies.Any(
                        turret => turret.LastTarget() != null && turret.LastTarget().NetworkId == fuccboi.NetworkId && fuccboi.Distance(turret) <= 1200)) continue;
                foreach (
                    Obj_AI_Turret turret in
                        EntityManager.Turrets.Allies.Where(
                            turret => turret.LastTarget().NetworkId == fuccboi.NetworkId && fuccboi.Distance(turret) <= 1200))
                {
                    AIHeroClient target = TargetSelector.GetTarget(_e.Range, DamageType.Physical);
                    if (!target.IsValidTarget(_e.Range)) return;
                    PredictionResult prediction = _e.GetPrediction(target);
                    if (prediction.HitChance >= HitChance.Medium)
                    {
                        _e.Cast(prediction.CastPosition);
                    }
                }
            }
        }

        private static void KillSteal()
        {
            try
            {
                foreach (
                    var target in
                        EntityManager.Heroes.Enemies.Where(
                            hero =>
                                hero.IsValidTarget(_e.Range) && !hero.IsDead && !hero.IsZombie &&
                                hero.HealthPercent <= 25))
                {
                    if (Player.Instance.ManaPercent >= Value.Get("killsteal.e.mana"))
                    {
                        if (target.Health + target.AttackShield <
                            Player.Instance.GetSpellDamage(target, SpellSlot.E))

                        {
                            _e.Cast(_e.GetPrediction(target).CastPosition);
                        }
                    }

                    if (Player.Instance.ManaPercent >= Value.Get("killsteal.r.mana"))
                    {
                        if (target.Health + target.AttackShield <
                            Player.Instance.GetSpellDamage(target, SpellSlot.R))

                        {
                            _r.Cast(_r.GetPrediction(target).CastPosition);
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

        private static void CatchAxe()
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) return;
            if (Value.Get("misc.axe.mode") == 1 && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) ||
                Value.Get("misc.axe.mode") == 2 && Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.None ||
                Value.Get("misc.axe.mode") == 3)
                return;

            var bestAxe = GetBestAxe;

            if (bestAxe != null && bestAxe.Object.Position.Distance(Player.Instance.ServerPosition) > 110)
            {
                if (Value.Use("misc.axe.check") &&
                    bestAxe.Object.CountEnemiesInRange(300) > Value.Get("misc.axe.check.enemies")) return;

                var catchTime = 1000*(Player.Instance.Distance(bestAxe.Object.Position)/Player.Instance.MoveSpeed);
                var expireTime = bestAxe.ExpireTime - Game.Time;

                if (catchTime >= expireTime && Value.Use("misc.w.axe"))
                {
                    _w.Cast();
                }

                if (Value.Use("misc.axe.tower"))
                {
                    if (IsUnderTurret(bestAxe.Object.Position) || !IsUnderTurret(bestAxe.Object.Position))
                    {
                        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.None))
                        {
                            Orbwalker.OrbwalkTo(bestAxe.Object.Position);
                        }

                        else
                        {
                            Orbwalker.OrbwalkTo(bestAxe.Object.Position);
                        }
                    }
                }

                else
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.None))
                    {
                        Orbwalker.OrbwalkTo(bestAxe.Object.Position);
                    }

                    else
                    {
                        Orbwalker.OrbwalkTo(bestAxe.Object.Position);
                    }
                }
            }
        }



        public override void Combo()
        {
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Physical);

            if (target == null || target.IsZombie) return;

            if (Player.Instance.ManaPercent >= Value.Get("combo.e.mana"))
            {
                if (Value.Use("combo.e") && _e.IsReady())
                {
                    _e.Cast(_e.GetPrediction(target).CastPosition);
                }
            }

            if (Player.Instance.ManaPercent >= Value.Get("combo.w.mana"))
            {
                    if (!Player.Instance.HasBuff("dravenfurybuff") && _w.IsReady())
                    {
                        _w.Cast();
                    }
            }

            if (Player.Instance.ManaPercent >= Value.Get("combo.r.mana"))
            {
                if (Value.Use("combo.r") && _r.IsReady())
                {
                    _r.Cast(_r.GetPrediction(target).CastPosition);
                }
            }
            
        }

        public override void Harass()
        {
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Physical);

            if (target == null || target.IsZombie) return;

            if (Player.Instance.ManaPercent >= Value.Get("harass.q.mana"))
            {
                if (Value.Use("harass.q") && _q.IsReady())
                {
                    _q.Cast();
                }
            }
            if (Player.Instance.ManaPercent >= Value.Get("harass.w.mana"))
            {
                if (Value.Use("harass.w"))
                {
                    if (!Player.Instance.HasBuff("dravenfurybuff") && _w.IsReady())
                    {
                        _w.Cast();
                    }
                }
            }
            if (Player.Instance.ManaPercent >= Value.Get("harass.e.mana"))
            {
                if (Value.Use("harass.e") && _e.IsReady())
                {
                    _e.Cast(_e.GetPrediction(target).CastPosition);
                }
            }
        }

        public override void Laneclear()
        {
            var count =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                    Player.Instance.ServerPosition,
                    Player.Instance.AttackRange, false).Count();

            var source =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                    Player.Instance.ServerPosition,
                    Player.Instance.AttackRange).OrderByDescending(a => a.MaxHealth).FirstOrDefault();

            if (count == 0) return;

            if (Player.Instance.ManaPercent >= Value.Get("lane.q.mana"))
            {
                if (Value.Use("lane.q") && _q.IsReady())
                {
                    _q.Cast();
                }
            }
            if (Player.Instance.ManaPercent >= Value.Get("lane.w.mana"))
            {
                if (Value.Use("lane.w") && _w.IsReady())
                {
                    if (!Player.Instance.HasBuff("dravenfurybuff") && _w.IsReady())
                    {
                        _w.Cast();
                    }
                }
            }
            if (Player.Instance.ManaPercent >= Value.Get("lane.e.mana"))
            {
                if (Value.Use("lane.e") && _e.IsReady())
                {
                    _e.Cast(source);
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
            if (monsters != null)
            {
                if (Value.Use("jungle.monsters.spell"))
                {
                    if (Player.Instance.ManaPercent >= Value.Get("jungle.q.mana"))
                    {
                        if (Value.Use("jungle.q") && _q.IsReady())
                        {
                            _q.Cast();
                        }
                    }
                    if (Player.Instance.ManaPercent >= Value.Get("jungle.w.mana"))
                    {
                        if (Value.Use("jungle.w") && _w.IsReady())
                        {
                            if (!Player.Instance.HasBuff("dravenfurybuff") && _w.IsReady())
                            {
                                _w.Cast();
                            }
                        }
                    }
                    if (Player.Instance.ManaPercent >= Value.Get("jungle.e.mana"))
                    {
                        if (Value.Use("jungle.e") && _e.IsReady())
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

                    if (Player.Instance.ManaPercent >= Value.Get("jungle.q.mana"))
                    {
                        if (Value.Use("jungle.q") && _q.IsReady())
                        {
                            _q.Cast();
                        }
                    }
                    if (Player.Instance.ManaPercent >= Value.Get("jungle.w.mana"))
                    {
                        if (Value.Use("jungle.w") && _w.IsReady())
                        {
                            if (!Player.Instance.HasBuff("dravenfurybuff") && _w.IsReady())
                            {
                                _w.Cast();
                            }
                        }
                    }
                    if (Player.Instance.ManaPercent >= Value.Get("jungle.e.mana"))
                    {
                        if (Value.Use("jungle.e") && _e.IsReady())
                        {
                            _e.Cast(fappamonsters);
                        }
                    }
                }
            }
        }

        public override void Flee()
        {
            var target = TargetSelector.GetTarget(_e.Range, DamageType.Physical);

            if (Value.Use("flee.w"))
            {
                if (!Player.Instance.HasBuff("dravenfurybuff") && _w.IsReady())
                {
                    _w.Cast();
                }
            }

            if (Value.Use("flee.e"))
            {
                if (target != null && _e.IsReady())
                {
                    _e.Cast(_e.GetPrediction(target).CastPosition);
                }
            }
        }

        public override void LastHit()
        {
            var source =
                EntityManager.MinionsAndMonsters.EnemyMinions
                    .FirstOrDefault(m => m.IsValidTarget(_e.Range) && Player.Instance.GetSpellDamage(m, SpellSlot.E) > m.TotalShieldHealth());

            if (source == null) return;

            if (Player.Instance.ManaPercent >= Value.Get("lasthit.e.mana"))
            {
                if (Value.Use("lasthit.e") && _q.IsReady())
                {
                    _e.Cast(source);
                }
            }
        }

        private static float ComboDamage(Obj_AI_Base enemy)
        {
            float damage = Player.Instance.GetAutoAttackDamage(enemy);

            if (AxeCount > 0)
            {
                damage += Player.Instance.GetSpellDamage(enemy, SpellSlot.Q);
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

        private static Axe GetBestAxe
        {
            get
            {
                return Axes.Where(h =>h.Object.Position.Distance(Game.CursorPos) <=Value.Get("misc.axe.range")).
                        OrderBy(h => h.Object.Position.Distance(Player.Instance.ServerPosition)).
                        ThenBy(x => x.Object.Distance(Game.CursorPos)).
                        FirstOrDefault();
            }
        }

        public static bool IsUnderTurret(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.IsValidTarget(950) && turret.IsEnemy);
        }

        public class Axe
        {
            public double ExpireTime { get; set; }

            public GameObject Object { get; set; }
        }
        

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
                    if (GetBestAxe != null)
                    {
                        _axeLocation.Color = colorQ;
                        _axeLocation.Radius = widthQ;
                        _axeLocation.Draw(GetBestAxe.Object.Position);
                    }

                    foreach (
                        var axe in
                            Axes.Where(x => x.Object.NetworkId != (GetBestAxe != null ? GetBestAxe.Object.NetworkId : 0)))
                    {
                        _axeLocation.Color = colorQ;
                        _axeLocation.Radius = widthQ;
                        _axeLocation.Draw(axe.Object.Position);
                    }
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
                if (Value.Use("draw.axe.catch") && ((Value.Use("draw.ready") && _q.IsReady()) || !Value.Use("draw.ready")))
                {
                    new Circle
                    {
                        Color = colorQ,
                        Radius = Value.Get("misc.axe.range"),
                        BorderWidth = widthQ
                    }.Draw(Game.CursorPos);
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
    }
}
