using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using OKTRAIO.Menu_Settings;
using SharpDX;
using Color = System.Drawing.Color;

namespace OKTRAIO.Champions
{
    class Sivir : AIOChampion
    {
        #region Initialize and Declare

        private static Spell.Targeted _w;
        private static Spell.Active _r, _e;
        private static Spell.Skillshot _q;
        private static readonly Vector2 Offset = new Vector2(1, 0);
        private static float _qmana, _wmana, _emana, _rmana;


        public override void Init()
        {
            try
            {
                //spells
                _q = new Spell.Skillshot(SpellSlot.Q, 1200, SkillShotType.Linear, 250, 1350, 70);
                _w = new Spell.Targeted(SpellSlot.W, (uint)Player.Instance.GetAutoAttackRange());
                _e = new Spell.Active(SpellSlot.E);
                _r = new Spell.Active(SpellSlot.R, 1000);


                //menu

                //combo
                MainMenu.ComboKeys(true, true, false, true);
                MainMenu._combo.AddSeparator();
                MainMenu._combo.AddGroupLabel("Combo Preferences", "combo.grouplabel.addonmenu", true);
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
                MainMenu._lane.AddSlider("lane.q.min", "Min. {0} minions for Q", 3, 1, 7, true);
                MainMenu._lane.AddGroupLabel("Mana Manager:", "lane.grouplabel.addonmenu", true);
                MainMenu.LaneManaManager(true, true, false, false, 60, 60, 0, 0);

                //jungleclear
                MainMenu.JungleKeys(true, true, false, false);
                MainMenu._jungle.AddSeparator();
                MainMenu._jungle.AddSlider("jungle.q.min", "Min. {0} minions for Q", 3, 1, 4, true);
                MainMenu._jungle.AddGroupLabel("Mana Manager:", "jungle.grouplabel.addonmenu", true);
                MainMenu.JungleManaManager(true, false, false, false, 60, 60, 0, 0);

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
                MainMenu._misc.AddGroupLabel("Auto E - Targetted Spells Settings", "misc.grouplabel.addonmenu", true);
                MainMenu._misc.AddSeparator();
                foreach (var enemy in EntityManager.Heroes.Enemies)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var spell = enemy.Spellbook.Spells[i];
                        if (spell.SData.TargettingType == SpellDataTargetType.Unit && !spell.Name.Contains("summoner"))
                        {
                            MainMenu._misc.AddCheckBox(spell.SData.Name, enemy.ChampionName + " - " + spell.Name, false, true);
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
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
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
            throw new NotImplementedException();
        }

        public override void Harass()
        {
            throw new NotImplementedException();
        }

        public override void Laneclear()
        {
            throw new NotImplementedException();
        }

        public override void Jungleclear()
        {
            throw new NotImplementedException();
        }

        public override void Flee()
        {
            throw new NotImplementedException();
        }

        public override void LastHit()
        {
            throw new NotImplementedException();
        }

        private static void GameOnUpdate(EventArgs args)
        {
            AutoQcc();

            ManaManagement();
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs spell)
        {
            if (sender.IsAlly || sender.IsMe) { return; }

            if (MainMenu._misc[spell.SData.Name].Cast<CheckBox>().CurrentValue && spell.Target.IsMe && spell.SData.TargettingType == SpellDataTargetType.Unit && !spell.IsAutoAttack())
            {
                Core.DelayAction(() => _e.Cast(), Value.Get("misc.e.delay"));
            }
        }

        #endregion
        #region Utils

        private static float QDamage(Obj_AI_Base target)
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
                damage += QDamage(target);
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
