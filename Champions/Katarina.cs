using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using OKTRAIO.Menu_Settings;
using OKTRAIO.Utility;
using Color = System.Drawing.Color;
using MainMenu = OKTRAIO.Menu_Settings.MainMenu;

namespace OKTRAIO.Champions
{
    class Katarina : AIOChampion
    {
        #region Initialization
        #region SpellsDefine
        private static Spell.Targeted _q;
        private static Spell.Active _w;
        private static Spell.Targeted _e;
        private static Spell.Active _r;
        #endregion

        private bool _isUlting;
        private static Menu _humanizerMenu;
        public override void Init()
        {
            try
            {
                try
                {
                    #region Spells
                    // Defining Spells
                    _q = new Spell.Targeted(SpellSlot.Q, 675);
                    _w = new Spell.Active(SpellSlot.W, 375);
                    _e = new Spell.Targeted(SpellSlot.E, 700);
                    _r = new Spell.Active(SpellSlot.R, 550);
                    #endregion
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code SPELL)</font>");
                }

                try
                {
                    #region Menu
                    var combo = MainMenu._combo;
                    string[] s = { "QEWR", "EQWR" };

                    combo.AddStringList("combo.mode", "Mode: ", s, 1);
                    MainMenu.ComboKeys(true, true, true, true);
                    MainMenu.HarassKeys(true, true, true, true);
                    MainMenu._harass.Add("harass.autow", new CheckBox("Use Auto W"));

                    MainMenu.FleeKeys(false, false, true, false);
                    MainMenu._flee.Add("flee.ward", new CheckBox("Use Wardjump"));

                    MainMenu.LaneKeys(true, true, true, false);
                    MainMenu.LastHitKeys(true, true, true, false);
                    MainMenu.KsKeys(true, true, true, true);
                    MainMenu._ks.Add("killsteal.ignite", new CheckBox("Use Ignite"));

                    MainMenu.DamageIndicator();
                    MainMenu.DrawKeys(true, true, true, true);
                    MainMenu._draw.AddSeparator();

                    MainMenu._draw.AddGroupLabel("Flash Settings");
                    MainMenu._draw.Add("draw.flash", new CheckBox("Draw flash"));
                    MainMenu._draw.AddColorItem("color.flash");
                    MainMenu._draw.AddWidthItem("width.flash");
                    MainMenu._draw.AddSeparator();

                    MainMenu._draw.AddGroupLabel("Ignite Settings");
                    MainMenu._draw.Add("draw.ignite", new CheckBox("Draw ignite"));
                    MainMenu._draw.AddColorItem("color.ignite");
                    MainMenu._draw.AddWidthItem("width.ignite");

                    _humanizerMenu = MainMenu._menu.AddSubMenu("Humanizer Menu");
                    _humanizerMenu.AddGroupLabel("Q Settings");
                    _humanizerMenu.Add("min.q", new Slider("Min Q Delay", 0, 0, 50));
                    _humanizerMenu.Add("max.q", new Slider("Max Q Delay", 0, 0, 50));
                    _humanizerMenu.AddSeparator(10);

                    _humanizerMenu.AddGroupLabel("W Settings");
                    _humanizerMenu.Add("min.w", new Slider("Min W Delay", 0, 0, 50));
                    _humanizerMenu.Add("max.w", new Slider("Max W Delay", 0, 0, 50));
                    _humanizerMenu.AddSeparator(10);

                    _humanizerMenu.AddGroupLabel("E Settings");
                    _humanizerMenu.Add("min.e", new Slider("Min E Delay", 0, 0, 50));
                    _humanizerMenu.Add("max.e", new Slider("Max E Delay", 0, 0, 50));
                    _humanizerMenu.AddSeparator(10);

                    _humanizerMenu.AddGroupLabel("R Settings");
                    _humanizerMenu.Add("min.r", new Slider("Min R Delay", 4, 0, 50));
                    _humanizerMenu.Add("max.r", new Slider("Max R Delay", 4, 0, 50));
                    _humanizerMenu.AddSeparator(10);

                    #endregion
                }

                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code MENU)</font>");
                }

                #region UtilityInit
                DamageIndicator.DamageToUnit = GetActualRawComboDamage;
                Value.Init();
                Value.MenuList.Add(_humanizerMenu);
                Drawing.OnDraw += DrawRanges;

                #region MenuValueChange
                _humanizerMenu["min.q"].Cast<Slider>().OnValueChange += delegate
                {
                    if (_humanizerMenu["min.q"].Cast<Slider>().CurrentValue > _humanizerMenu["max.q"].Cast<Slider>().CurrentValue)
                        _humanizerMenu["min.q"].Cast<Slider>().CurrentValue = _humanizerMenu["max.q"].Cast<Slider>().CurrentValue;
                };
                _humanizerMenu["max.q"].Cast<Slider>().OnValueChange += delegate
                {
                    if (_humanizerMenu["max.q"].Cast<Slider>().CurrentValue < _humanizerMenu["min.q"].Cast<Slider>().CurrentValue)
                        _humanizerMenu["max.q"].Cast<Slider>().CurrentValue = _humanizerMenu["min.q"].Cast<Slider>().CurrentValue;
                };
                _humanizerMenu["min.w"].Cast<Slider>().OnValueChange += delegate
                {
                    if (_humanizerMenu["min.w"].Cast<Slider>().CurrentValue > _humanizerMenu["max.w"].Cast<Slider>().CurrentValue)
                        _humanizerMenu["min.w"].Cast<Slider>().CurrentValue = _humanizerMenu["max.w"].Cast<Slider>().CurrentValue;
                };
                _humanizerMenu["max.w"].Cast<Slider>().OnValueChange += delegate
                {
                    if (_humanizerMenu["max.w"].Cast<Slider>().CurrentValue < _humanizerMenu["min.w"].Cast<Slider>().CurrentValue)
                        _humanizerMenu["max.w"].Cast<Slider>().CurrentValue = _humanizerMenu["min.w"].Cast<Slider>().CurrentValue;
                };
                _humanizerMenu["min.e"].Cast<Slider>().OnValueChange += delegate
                {
                    if (_humanizerMenu["min.e"].Cast<Slider>().CurrentValue > _humanizerMenu["max.e"].Cast<Slider>().CurrentValue)
                        _humanizerMenu["min.e"].Cast<Slider>().CurrentValue = _humanizerMenu["max.e"].Cast<Slider>().CurrentValue;
                };
                _humanizerMenu["min.e"].Cast<Slider>().OnValueChange += delegate
                {
                    if (_humanizerMenu["max.e"].Cast<Slider>().CurrentValue < _humanizerMenu["min.e"].Cast<Slider>().CurrentValue)
                        _humanizerMenu["max.e"].Cast<Slider>().CurrentValue = _humanizerMenu["min.e"].Cast<Slider>().CurrentValue;
                };
                _humanizerMenu["min.r"].Cast<Slider>().OnValueChange += delegate
                {
                    if (_humanizerMenu["min.r"].Cast<Slider>().CurrentValue > _humanizerMenu["max.r"].Cast<Slider>().CurrentValue)
                        _humanizerMenu["min.r"].Cast<Slider>().CurrentValue = _humanizerMenu["max.r"].Cast<Slider>().CurrentValue;
                };
                _humanizerMenu["min.r"].Cast<Slider>().OnValueChange += delegate
                {
                    if (_humanizerMenu["max.r"].Cast<Slider>().CurrentValue < _humanizerMenu["min.r"].Cast<Slider>().CurrentValue)
                        _humanizerMenu["max.r"].Cast<Slider>().CurrentValue = _humanizerMenu["min.r"].Cast<Slider>().CurrentValue;
                };
                #endregion
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code 503)</font>");
            }

            Game.OnUpdate += delegate
            {
                try
                {
                    #region IsUlting
                    _isUlting = Player.Instance.HasBuff("katarinarsound");

                    Orbwalker.DisableAttacking = _isUlting;
                    Orbwalker.DisableMovement = _isUlting;
                    #endregion
                    #region AutoW
                    if (MainMenu._harass["harass.autow"].Cast<CheckBox>().CurrentValue)
                    {
                        var e = EntityManager.Heroes.Enemies.Where(ee => !ee.IsDead && ee.IsValid);
                        foreach (var enemy in e)
                        {
                            if (_w.IsInRange(enemy) && _w.IsReady() && !_isUlting)
                            {
                                _w.Cast();
                            }
                        }
                    }
                    #endregion

                    KillSteal();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> san error ocurred. (Code 5)</font>");
                }
                //KillSteal();
            };
        }
        #endregion

        #region _humanizerMenuIndex
        private static int MinQDelay
        {
            get
            {
                return _humanizerMenu["min.q"].Cast<Slider>().CurrentValue;
            }
        }
        private static int MaxQDelay
        {
            get
            {
                return _humanizerMenu["max.q"].Cast<Slider>().CurrentValue;
            }
        }
        private static int MinWDelay
        {
            get
            {
                return _humanizerMenu["min.w"].Cast<Slider>().CurrentValue;
            }
        }
        private static int MaxWDelay
        {
            get
            {
                return _humanizerMenu["max.w"].Cast<Slider>().CurrentValue;
            }
        }
        private static int MinEDelay
        {
            get
            {
                return _humanizerMenu["min.e"].Cast<Slider>().CurrentValue;
            }
        }
        private static int MaxEDelay
        {
            get
            {
                return _humanizerMenu["max.e"].Cast<Slider>().CurrentValue;
            }
        }
        private static int MinRDelay
        {
            get
            {
                return _humanizerMenu["min.e"].Cast<Slider>().CurrentValue;
            }
        }
        private static int MaxRDelay
        {
            get
            {
                return _humanizerMenu["max.r"].Cast<Slider>().CurrentValue;
            }
        }
        #endregion

        #region ComboMain
        public override void Combo()
        {
            try
            {
                var target = TargetSelector.GetTarget(1000, DamageType.Magical);
                if (target == null || !target.IsValid)
                    return;

                switch (Value.Get("combo.mode"))
                {
                    #region Mode QEWR
                    case 0:
                        if (_q.IsReady() && _q.IsInRange(target) && Value.Use("combo.q"))
                        {
                            Core.DelayAction(() => _q.Cast(target), new Random().Next(MinQDelay, MaxQDelay));
                        }
                        if (_e.IsReady() && _e.IsInRange(target) && Value.Use("combo.e"))
                        {
                            Core.DelayAction(() => _e.Cast(target), new Random().Next(MinWDelay, MaxWDelay));
                        }
                        if (_w.IsReady() && _w.IsInRange(target) && Value.Use("combo.w"))
                        {
                            Core.DelayAction(() => _w.Cast(), new Random().Next(MinEDelay, MaxEDelay));
                        }
                        if (_r.IsReady() && _r.IsInRange(target) && Value.Use("combo.r"))
                        {
                            Core.DelayAction(() => _r.Cast(), new Random().Next(MinRDelay, MaxRDelay));
                        }
                        break;
                    #endregion
                    #region Mode EQWR
                    case 1:
                        if (_e.IsReady() && _e.IsInRange(target) && Value.Use("combo.e"))
                        {
                            Core.DelayAction(() => _e.Cast(target), new Random().Next(MinWDelay, MaxWDelay));
                        }
                        if (_q.IsReady() && _q.IsInRange(target) && Value.Use("combo.q"))
                        {
                            Core.DelayAction(() => _q.Cast(target), new Random().Next(MinQDelay, MaxQDelay));
                        }
                        if (_w.IsReady() && _w.IsInRange(target) && Value.Use("combo.w"))
                        {
                            Core.DelayAction(() => _w.Cast(), new Random().Next(MinEDelay, MaxEDelay));
                        }
                        if (_r.IsReady() && _r.IsInRange(target) && Value.Use("combo.r"))
                        {
                            Core.DelayAction(() => _r.Cast(), new Random().Next(MinRDelay, MaxRDelay));
                        }
                        break;
                }
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code COMBO)</font>");
            }
        }
        #endregion

        #region HarassMain
        public override void Harass()
        {
            try
            {
                var target = TargetSelector.GetTarget(1000, DamageType.Magical);
                if (target == null || !target.IsValid)
                    return;



                if (_q.IsReady() && _q.IsInRange(target) && Value.Use("harass.q"))
                {
                    Core.DelayAction(() => _q.Cast(target), new Random().Next(MinQDelay, MaxQDelay));
                }
                if (_w.IsReady() && _w.IsInRange(target) && Value.Use("harass.w"))
                {
                    Core.DelayAction(() => _w.Cast(), new Random().Next(MinEDelay, MaxEDelay));
                }
                if (_e.IsReady() && _e.IsInRange(target) && Value.Use("harass.e"))
                {
                    Core.DelayAction(() => _e.Cast(target), new Random().Next(MinWDelay, MaxWDelay));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code HARASS)</font>");
            }
        }
        #endregion

       #region LastHitMain
        public override void LastHit()
        {
            try
            {
                foreach (var minion in EntityManager.MinionsAndMonsters.EnemyMinions)
                {
                    if (minion == null || !minion.IsValid)
                        return;

                    #region Q
                    try
                    {
                        if (Prediction.Health.GetPrediction(minion, _q.CastDelay + (Game.Ping / 4)) <= Player.Instance.GetSpellDamage(minion, SpellSlot.Q))
                        {
                            if (_q.IsInRange(minion) && _q.IsReady() && Value.Use("lasthit.q"))
                            {
                                Core.DelayAction(() => _q.Cast(minion), new Random().Next(MinQDelay, MaxQDelay));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code LASTHIT.Q)</font>");
                    }
                    #endregion
                    #region W
                    try
                    {
                        if (Prediction.Health.GetPrediction(minion, _w.CastDelay + (Game.Ping / 4)) <= Player.Instance.GetSpellDamage(minion, SpellSlot.W))
                        {
                            if (_w.IsInRange(minion) && _w.IsReady() && Value.Use("lasthit.w"))
                            {
                                Core.DelayAction(() => _w.Cast(), new Random().Next(MinEDelay, MaxEDelay));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code LASTHIT.W)</font>");
                    }
                    #endregion
                    #region E
                    try
                    {
                        if (Prediction.Health.GetPrediction(minion, _e.CastDelay + (Game.Ping / 4)) <= Player.Instance.GetSpellDamage(minion, SpellSlot.E))
                        {
                            if (_e.IsInRange(minion) && _e.IsReady() && Value.Use("lasthit.e"))
                            {
                                Core.DelayAction(() => _e.Cast(minion), new Random().Next(MinWDelay, MaxWDelay));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code LASTHIT.W)</font>");
                    }
                    #endregion
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code LASTHIT)</font>");
            }
        }
        #endregion

        #region FleeMain
        public override void Flee()
        {
            try
            {

                foreach (var minion in ObjectManager.Get<Obj_AI_Base>().Where(o => o.IsTargetable && o.IsValid && !o.IsDead && o.IsHPBarRendered && (o.IsMinion || o.IsMonster || (o is AIHeroClient && !o.IsMe) || o.IsWard())).OrderBy(o => o.Distance(Game.CursorPos)))
                {
                    if (minion == null)
                    {
                        return;
                    }
                    if (Value.Use("flee.e") && _e.IsReady() && _e.IsInRange(minion))
                    {
                        if (minion.IsInRange(Game.CursorPos, 200))
                        {
                            Core.DelayAction(() => _e.Cast(minion), new Random().Next(MinWDelay, MaxWDelay));
                        }
                    }
                    else
                    {
                        try
                        {
                            if (Utility.Extensions.GetWardSlot() == null || !Utility.Extensions.GetWardSlot().IsWard)
                                return;

                            if (Value.Use("flee.ward") && Utility.Extensions.GetWardSlot().CanUseItem() && _e.IsReady() && Value.Use("flee.e"))
                            {
                                var pos = Player.Instance.Position.Extend(Game.CursorPos, 600);
                                Utility.Extensions.GetWardSlot().Cast(pos.To3D());
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code FLE_e.WARDJUMP)</font>");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code FLEE)</font>");
            }
        }
        #endregion

        #region LaneClearMain
        public override void Laneclear()
        {
            try
            {
                foreach (var minion in EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m.IsInRange(Player.Instance, 1200)))
                {
                    if (minion == null || !minion.IsValid)
                        return;

                    if (_q.IsInRange(minion) && _q.IsReady() && Value.Use("lane.q"))
                    {
                        Core.DelayAction(() => _q.Cast(minion), new Random().Next(MinQDelay, MaxQDelay));
                    }

                    if (_w.IsInRange(minion) && _w.IsReady() && Value.Use("lane.w"))
                    {
                        Core.DelayAction(() => _w.Cast(), new Random().Next(MinEDelay, MaxEDelay));
                    }

                    if (_e.IsInRange(minion) && _e.IsReady() && Value.Use("lane.e"))
                    {
                        Core.DelayAction(() => _e.Cast(minion), new Random().Next(MinWDelay, MaxWDelay));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code LANECLEAR)</font>");
            }
        }
        #endregion

        #region KillStealMain
        private static void KillSteal()
        {
            try
            {
                var e = EntityManager.Heroes.Enemies.Where(ee => !ee.IsDead && ee.IsValid);

                foreach (var enemy in e)
                {
                    var damage = Player.Instance.CalculateDamageOnUnit(enemy, DamageType.Magical, GetActualRawComboDamage(enemy), true, true);
                    if (enemy.Health <= damage)
                    {
                        if (_q.IsReady() && _q.IsInRange(enemy) && Value.Use("killsteal.q"))
                        {
                            Core.DelayAction(() => _q.Cast(enemy), new Random().Next(MinQDelay, MaxQDelay));
                        }
                        if (_w.IsReady() && _w.IsInRange(enemy) && Value.Use("killsteal.w"))
                        {
                            Core.DelayAction(() => _w.Cast(), new Random().Next(MinEDelay, MaxEDelay));
                        }
                        if (_e.IsReady() && _e.IsInRange(enemy) && Value.Use("killsteal.e"))
                        {
                            Core.DelayAction(() => _e.Cast(enemy), new Random().Next(MinWDelay, MaxWDelay));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code KILLSTEAL)</font>");
            }
        }
        #endregion

        #region Damages
        #region BaseDamages
        private static float[] QDamage = { 0, 60, 85, 110, 135, 160 };
        private static float[] BonusQDamage = { 0, 15, 30, 45, 60, 75 };
        private static float[] WDamage = { 0, 40, 75, 110, 145, 180 };
        private static float[] EDamage = { 0, 40, 70, 100, 130, 160 };
        private static float[] RDamage = { 0, 350, 550, 750 };
        #endregion
        #region GetSpellDamage
        private static float GetSpellDamage(SpellSlot slot)
        {
            try
            {
                var qbasedamage = QDamage[_q.Level];
                var wbasedamage = WDamage[_w.Level];
                var ebasedamage = EDamage[_e.Level];
                var rbasedamage = RDamage[_r.Level];

                var qbonusdamage = (45f / 100f * Player.Instance.FlatMagicDamageMod);
                var wbonusdamage = (25f / 100f * Player.Instance.FlatMagicDamageMod);
                var ebonusdamage = (25f / 100f * Player.Instance.FlatMagicDamageMod);
                var rbonusdamage = (25f / 100f * Player.Instance.FlatMagicDamageMod);

                if (slot == SpellSlot.Q)
                    return qbasedamage + qbonusdamage + (BonusQDamage[_q.Level] + (15f / 100f * Player.Instance.FlatMagicDamageMod));
                if (slot == SpellSlot.W)
                    return wbasedamage + wbonusdamage + (60f / 100f * Player.Instance.FlatPhysicalDamageMod);
                if (slot == SpellSlot.E)
                    return ebasedamage + ebonusdamage;
                if (slot == SpellSlot.R)
                    return rbasedamage + rbonusdamage + (375f / 1000f * Player.Instance.FlatPhysicalDamageMod);

                //if (raw)
                //return Player.Instance.CalculateDamageOnUnit(target, DamageTyp_e.Magical, damage, true, true);
                return 0f;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code GETSPELLDAMAGE)</font>");
                return 0f;
            }
        }
        #endregion
        #region RawComboDamage
        private static float GetActualRawComboDamage(Obj_AI_Base enemy)
        {
            try
            {
                var damage = 0f;

                var spells = new List<SpellSlot>();
                spells.Add(SpellSlot.Q);
                spells.Add(SpellSlot.W);
                spells.Add(SpellSlot.E);
                spells.Add(SpellSlot.R);
                foreach (var spell in spells.Where(s => Player.Instance.Spellbook.CanUseSpell(s) == SpellState.Ready && s != SpellSlot.R))
                {
                    if (Player.Instance.Spellbook.CanUseSpell(spell) == SpellState.Ready)
                        damage += GetSpellDamage(spell);
                }
                if (Player.Instance.Spellbook.CanUseSpell(GetIgniteSpellSlot()) != SpellState.Cooldown && Player.Instance.Spellbook.CanUseSpell(GetIgniteSpellSlot()) != SpellState.NotLearned && Player.Instance.Spellbook.CanUseSpell(GetIgniteSpellSlot()) == SpellState.Ready)
                    damage += Player.Instance.GetSummonerSpellDamage(enemy, DamageLibrary.SummonerSpells.Ignite);
                if (Player.Instance.Spellbook.CanUseSpell(SpellSlot.R) != SpellState.Cooldown && Player.Instance.Spellbook.CanUseSpell(SpellSlot.R) != SpellState.NotLearned)
                    damage += GetSpellDamage(SpellSlot.R);
                return damage;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code GETACTUALRAWCOMBODAMAGE)</font>");
                return 0f;
            }
        }
        #endregion

        #region DrawColorIndex
        private static Color ColorQ
        {
            get { return MainMenu._draw.GetColor("color.q"); }
        }
        private static Color ColorW
        {
            get { return MainMenu._draw.GetColor("color.w"); }
        }
        private static Color ColorE
        {
            get { return MainMenu._draw.GetColor("color.e"); }
        }
        private static Color ColorR
        {
            get { return MainMenu._draw.GetColor("color.r"); }
        }
        private static Color ColorIgnite
        {
            get { return MainMenu._draw.GetColor("color.ignite"); }
        }
        private static Color ColorFlash
        {
            get { return MainMenu._draw.GetColor("color.flash"); }
        }
        #endregion
        #region Value.Use("draw.w")idthIndex
        private static float WidthQ
        {
            get { return MainMenu._draw.GetWidth("width.q"); }
        }
        private static float WidthW
        {
            get { return MainMenu._draw.GetWidth("width.w"); }
        }
        private static float WidthE
        {
            get { return MainMenu._draw.GetWidth("width.e"); }
        }
        private static float WidthR
        {
            get { return MainMenu._draw.GetWidth("width.r"); }
        }
        private static float WidthIgnite
        {
            get { return MainMenu._draw.GetWidth("width.ignite"); }
        }
        private static float WidthFlash
        {
            get { return MainMenu._draw.GetWidth("width.flash"); }
        }
        #endregion
        #region DrawSummonersIndex
        private static bool DrawFlash
        {
            get
            {
                return Value.Use("dra_w.flash");
            }
        }
        private static bool DrawIgnite
        {
            get
            {
                return Value.Use("dra_w.ignite");
            }
        }
        #endregion
        #region SummonersRanges
        private static float flashrange = 425;
        private static float igniterange = 600;
        #endregion
        #region Value.Use("draw.r")angesMain
        private static void DrawRanges(EventArgs args)
        {
            try
            {
                if (Value.Use("draw.disable"))
                    return;

                try
                {
                    #region Q
                    if (Value.Use("draw.ready"))
                    {
                        if (_q.IsReady())
                            new Circle{ BorderWidth = WidthQ, Color = ColorQ, Radius = _q.Range }.Draw(Player.Instance.Position);
                    }
                    else
                    {
                        new Circle { BorderWidth = WidthQ, Color = ColorQ, Radius = _q.Range }.Draw(Player.Instance.Position);
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code DRAW</font>");
                }

                try
                {
                    #region W
                    if (Value.Use("draw.ready"))
                    {
                        if (_w.IsReady())
                            new Circle { BorderWidth = WidthW, Color = ColorW, Radius = _w.Range }.Draw(Player.Instance.Position);
                    }
                    else
                    {
                        new Circle { BorderWidth = WidthW, Color = ColorW, Radius = _w.Range }.Draw(Player.Instance.Position);
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code DRAW</font>");
                }

                try
                {
                    #region E
                    if (Value.Use("draw.ready"))
                    {
                        if (_e.IsReady())
                            new Circle { BorderWidth = WidthE, Color = ColorE, Radius = _e.Range }.Draw(Player.Instance.Position);
                    }
                    else
                    {
                        new Circle{ BorderWidth = WidthE, Color = ColorE, Radius = _e.Range }.Draw(Player.Instance.Position);
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code DRAW</font>");
                }

                try
                {
                    #region R
                    if (Value.Use("draw.r"))
                    {
                        if (!_r.IsOnCooldown)
                            new Circle() { BorderWidth = WidthR, Color = ColorR, Radius = _r.Range }.Draw(Player.Instance.Position);
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code DRAW)</font>");
                }

                #region Summoners
                try
                {
                    #region Flash
                    if (DrawFlash)
                    {
                        if (Player.CanUseSpell(GetFlashSpellSlot()) == SpellState.Ready)
                            new Circle() { BorderWidth = WidthFlash, Color = ColorFlash, Radius = flashrange }.Draw(Player.Instance.Position);
                        if (Player.CanUseSpell(GetFlashSpellSlot()) == SpellState.Cooldown)
                            new Circle() { BorderWidth = WidthFlash, Color = ColorFlash, Radius = flashrange }.Draw(Player.Instance.Position);
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code DRAW)</font>");
                }

                try
                {
                    #region Ignite
                    if (DrawIgnite)
                    {
                        if (Player.CanUseSpell(GetIgniteSpellSlot()) == SpellState.Ready)
                            new Circle() { BorderWidth = WidthIgnite, Color = ColorIgnite, Radius = igniterange }.Draw(Player.Instance.Position);
                        if (Player.CanUseSpell(GetIgniteSpellSlot()) == SpellState.Cooldown)
                            new Circle() { BorderWidth = WidthIgnite, Color = ColorIgnite, Radius = igniterange }.Draw(Player.Instance.Position);
                    }
                    #endregion
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code DRAW)</font>");
                }

                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (DRAW)</font>");
            }
        }
        #endregion
        #endregion
        #region GetSpellSlots
        private static SpellSlot GetFlashSpellSlot()
        {
            try
            {
                if (Player.GetSpell(SpellSlot.Summoner1).Name == "summonerflash")
                    return SpellSlot.Summoner1;
                if (Player.GetSpell(SpellSlot.Summoner2).Name == "summonerflash")
                    return SpellSlot.Summoner2;
                return SpellSlot.Unknown;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code GETFLASHSPELLSLOT)</font>");
                return SpellSlot.Unknown;
            }
        }

        private static SpellSlot GetIgniteSpellSlot()
        {
            try
            {
                if (Player.GetSpell(SpellSlot.Summoner1).Name.ToLower() == "summonerignite")
                    return SpellSlot.Summoner1;
                if (Player.GetSpell(SpellSlot.Summoner2).Name.ToLower() == "summonerignite")
                    return SpellSlot.Summoner2;
                return SpellSlot.Unknown;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code GETIGNITESPELLSLOT)</font>");
                return SpellSlot.Unknown;
            }
        }
        #endregion
    }

    #region Misc
    static class KataExtensions
    {
        public static void AddStringList(this Menu m, string uniqueId, string displayName, string[] values, int defaultValue)
        {
            try
            {
                var mode = m.Add(uniqueId, new Slider(displayName, defaultValue, 0, values.Length - 1));
                mode.DisplayName = displayName + ": " + values[mode.CurrentValue];
                mode.OnValueChange += delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                {
                    sender.DisplayName = displayName + ": " + values[args.NewValue];
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print("<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code ADDSTRINGLIST)</font>");
            }
        }

    }
    #endregion
}
