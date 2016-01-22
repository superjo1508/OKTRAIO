using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;
using OKTRAIO;
using OKTRAIO.Champions;
using OKTRAIO.Menu_Settings;
using OKTRAIO.Utility;
using SharpDX;

namespace MarksmanAIO.Champions
{
    class Vayne : AIOChampion
    {
        #region Initialize and Declare

        private static Spell.Skillshot _q;
        private static Spell.Targeted _e;
        private static int _randomizerOne, _randomizerTwo;
        private static AttackableUnit _target;

        public override void Init()
        {
            _q = new Spell.Skillshot(SpellSlot.Q, 300, SkillShotType.Linear);
            _e = new Spell.Targeted(SpellSlot.E, 550);

            MainMenu.ComboKeys(true, true, true, false);
            MainMenu._combo.AddGroupLabel("Combo Preferences", "combo.gl.pref", true);

            MainMenu.HarassKeys(true, true, true, false);
            MainMenu._harass.AddGroupLabel("Harass Preferences", "harass.gl.pref", true);

            MainMenu.JungleKeys(true, true, true, false);
            MainMenu._jungle.AddSlider("jungle.mana", "ManaSlider", 80, 0, 100, true);

            MainMenu.LaneKeys(true, true, true, false);
            MainMenu._lane.AddGroupLabel("LaneClear Preferences", "lane.gl.pref", true);
            MainMenu._lane.AddSlider("lane.mana", "ManaSlider", 80, 0, 100, true);

            MainMenu.KsKeys(true, false, true, false);

            MainMenu.DrawKeys(true, false, false, true);
            MainMenu._draw.AddSeparator();
            MainMenu._draw.AddLabel("W/Extended Q Settings");
            MainMenu._draw.Add("draw.qw", new CheckBox("Draw W/Extended Q"));
            MainMenu._draw.AddColorItem("color.qw");
            MainMenu._draw.AddWidthItem("width.qw");
            MainMenu.DamageIndicator(true);

            MainMenu.FleeKeys(false, false, true, false);

            Value.Init();

            DamageIndicator.DamageToUnit += GetRawDamage;
            Drawing.OnDraw += Draw;
            if (MainMenu._menu["useonupdate"].Cast<EloBuddy.SDK.Menu.Values.CheckBox>().CurrentValue)
            {
                Game.OnUpdate += Game_OnTick;
            }
            else
            {
                Game.OnTick += Game_OnTick;
            }
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Orbwalker.OnPostAttack += OrbwalkerOnPostAttack;
        }

        #endregion

        #region Gamerelated Logic

        public override void Combo()
        {
        }

        public override void Harass()
        {
        }

        public override void Laneclear()
        {
        }

        public override void Jungleclear()
        {
        }

        public override void Flee()
        {
        }

        public override void LastHit()
        {
        }

        private void OrbwalkerOnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (target.IsValidTarget())
            {
                if (Value.Mode(Orbwalker.ActiveModes.Combo) ||
                    Value.Mode(Orbwalker.ActiveModes.Harass) ||
                    Value.Mode(Orbwalker.ActiveModes.LaneClear) ||
                    Value.Mode(Orbwalker.ActiveModes.JungleClear))
                {
                    _target = target;
                    Core.DelayAction(SpellLogic, GetSpellDelay);
                }
            }
        }

        private void SpellLogic()
        {
            if (_target is AIHeroClient)
            {
                if (Value.Mode(Orbwalker.ActiveModes.Combo))
                {
                    var qPos = OKTRGeometry.SafeDashPos(_q.Range);
                    if (qPos != Vector3.Zero)
                        _q.Cast(qPos);
                    else
                    {
                        if (InsideCone())
                        {
                            _q.Cast(
                                OKTRGeometry.Deviation(Player.Instance.Position.To2D(), _target.Position.To2D(), -65)
                                    .To3D());
                        }
                        else
                        {
                            _q.Cast(
                                OKTRGeometry.Deviation(Player.Instance.Position.To2D(), _target.Position.To2D(), 65)
                                    .To3D());
                        }
                    }
                }
            }
            if (_target is Obj_AI_Base)
            {
                var targets = new List<Obj_AI_Minion>();
                if ((_target as Obj_AI_Base).IsMonster && Value.Mode(Orbwalker.ActiveModes.JungleClear) &&
                    Value.Get("jungle.mana") < Player.Instance.ManaPercent)
                {
                    targets =
                        EntityManager.MinionsAndMonsters.Monsters.Where(
                            m => m.Distance(Player.Instance) < Player.Instance.AttackRange)
                            .ToList();
                    if (!targets.Any()) return;
                    if (_q.IsReady() && Value.Use("jungle.q"))
                    {
                        Chat.Print("2");
                        var qPos = OKTRGeometry.SafeDashPos(_q.Range);
                        if (qPos != Vector3.Zero)
                            _q.Cast(qPos);
                        else
                        {
                            if (InsideCone())
                            {
                                _q.Cast(
                                    OKTRGeometry.Deviation(Player.Instance.Position.To2D(), _target.Position.To2D(), -65)
                                        .To3D());
                            }
                            else
                            {
                                _q.Cast(
                                    OKTRGeometry.Deviation(Player.Instance.Position.To2D(), _target.Position.To2D(), 65)
                                        .To3D());
                            }
                        }
                    }
                }
                else if ((_target as Obj_AI_Base).IsMinion && Value.Get("lane.mana") < Player.Instance.ManaPercent &&
                         Value.Mode(Orbwalker.ActiveModes.LaneClear))
                {
                    targets =
                        EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                            m => m.Distance(Player.Instance) < Player.Instance.AttackRange).ToList();
                    if (!targets.Any()) return;
                    if (_q.IsReady() && Value.Use("lane.q"))
                    {
                        var qPos = OKTRGeometry.SafeDashPos(_q.Range);
                        if (qPos != Vector3.Zero)
                            _q.Cast(qPos);
                        else
                        {
                            if (InsideCone())
                            {
                                _q.Cast(
                                    OKTRGeometry.Deviation(Player.Instance.Position.To2D(), _target.Position.To2D(), -65)
                                        .To3D());
                            }
                            else
                            {
                                _q.Cast(
                                    OKTRGeometry.Deviation(Player.Instance.Position.To2D(), _target.Position.To2D(), 65)
                                        .To3D());
                            }
                        }
                    }
                }
            }
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Slot == SpellSlot.Q) Orbwalker.ResetAutoAttack();
        }

        private void Game_OnTick(EventArgs args)
        {
        }

        #endregion

        #region Utils

        private static bool InsideCone()
        {
            if (_target == null) return false;
            var cone = new Geometry.Polygon.Sector(Player.Instance.Position,
                            OKTRGeometry.Deviation(Player.Instance.Position.To2D(), _target.Position.To2D(), 90).To3D(), Geometry.DegreeToRadian(180),
                            (Player.Instance.AttackRange + Player.Instance.BoundingRadius * 2));
            return cone.IsInside(Game.CursorPos.To2D());
        }

        private float GetRawDamage(Obj_AI_Base minion)
        {
            return 0;
        }

        public static int GetSpellDelay
        {
            get { return Game.Ping * (new Random().Next(_randomizerOne, _randomizerTwo) / 10); }
        }

        #endregion

        private void Draw(EventArgs args)
        {
        }
    }
}
