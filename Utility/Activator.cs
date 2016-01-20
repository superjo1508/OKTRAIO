using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using OKTRAIO.Menu_Settings;
using SharpDX;

namespace OKTRAIO.Utility
{
    public static class Activator
    {

        #region Initialize

        public static void Init()
        {
            if (MainMenu._menu["useonupdate"].Cast<EloBuddy.SDK.Menu.Values.CheckBox>().CurrentValue)
            {
                Game.OnUpdate += GameOnUpdate;
            }
            else
            {
                Game.OnTick += GameOnUpdate;
            }
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        #endregion

        #region Gamerelated Events

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {

            if (sender.IsMe && Value.Use("activator.youmusspellonly"))
            {
                if (((Player.Instance.ChampionName == "Lucian" || Player.Instance.ChampionName == "Twitch") &&
                     args.Slot == SpellSlot.R) || (Player.Instance.ChampionName == "Ashe" && args.Slot == SpellSlot.Q))
                {
                    Youmuus.Cast();
                }
            }
        }

        private static void GameOnUpdate(EventArgs args)
        {
            OffensiveItems();
            Potions();
            ActivatorSpells();
        }

        #endregion

        #region ItemLogic

        public static Item

            Potion = new Item(2003),
            Biscuit = new Item(2010),
            Refillable = new Item(2031),
            Hunter = new Item(2032),
            Corrupting = new Item(2033),
            Botrk = new Item(3153, 550f),
            Cutlass = new Item(3144, 550f),
            Youmuus = new Item(3142, 650f);

        private static void OffensiveItems()
        {
            if (Botrk.IsReady() && Value.Use("activator.botrk"))
            {
                var t = TargetSelector.GetTarget(Botrk.Range, DamageType.Physical);
                if (t.IsValidTarget())
                {
                    if (Value.Use("activator.botrk.ks") &&
                        Player.Instance.CalculateDamageOnUnit(t, DamageType.Physical, t.MaxHealth*(float) 0.1) >
                        t.Health)
                        Botrk.Cast(t);
                    if (Value.Use("activator.botrk.lifesave") && 
                        Player.Instance.HealthPercent < Value.Get("activator.botrk.hp") )
                        Botrk.Cast(t);
                    if (Value.Use("activator.botrk.combo") &&
                        Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
                        Botrk.Cast(t);
                }
            }

            if (Cutlass.IsReady() && Value.Use("activator.bilgewater"))
            {
                var t = TargetSelector.GetTarget(Botrk.Range, DamageType.Physical);
                if (t.IsValidTarget())
                {
                    if (Value.Use("activator.bilgewater.ks") &&
                        Player.Instance.CalculateDamageOnUnit(t, DamageType.Physical, 100) > t.Health)
                        Botrk.Cast(t);
                    if (Value.Use("activator.bilgewater.combo") &&
                        Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
                        Botrk.Cast(t);
                }
            }

            if (Youmuus.IsReady() && Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo && Value.Use("activator.youmus") )
            {
                var t = Orbwalker.LastTarget;

                if (t.IsValidTarget(Player.Instance.AttackRange) && t is AIHeroClient)
                {
                    Youmuus.Cast();
                }
            }
        }

        private static void Potions()
        {
            if (Player.Instance.IsInFountain() || Player.Instance.IsRecalling() ||
                Player.Instance.IsUsingPotion()) return;
            if (Value.Use("activator.potions"))
            {
                if (Player.Instance.HealthPercent < Value.Get("activator.potions.hp"))
                {
                    if (Hunter.IsReady())
                    {
                        Hunter.Cast();
                    }
                    else if (Corrupting.IsReady())
                    {
                        Corrupting.Cast();
                    }
                    else if (Refillable.IsReady())
                    {
                        Refillable.Cast();
                    }
                    else if (Potion.IsReady())
                    {
                        Potion.Cast();
                    }
                    else if (Biscuit.IsReady())
                    {
                        Biscuit.Cast();
                    }
                }
                if (Player.Instance.ManaPercent < Value.Get("activator.potions.mana"))
                {
                    if (Hunter.IsReady())
                    {
                        Hunter.Cast();
                    }
                    else if (Corrupting.IsReady())
                    {
                        Corrupting.Cast();
                    }
                }
            }
        }

        #endregion
        #region Spells

        public static Spell.Targeted Ignite;

        public static Spell.Active Heal, Barrier;

        public static void LoadSpells()
        {
            if (Player.Instance.Spellbook.GetSpell(SpellSlot.Summoner1).Name.Contains("dot"))
                Ignite = new Spell.Targeted(SpellSlot.Summoner1, 580);
            else if (Player.Instance.Spellbook.GetSpell(SpellSlot.Summoner2).Name.Contains("dot"))
                Ignite = new Spell.Targeted(SpellSlot.Summoner2, 580);
            if (Player.Instance.Spellbook.GetSpell(SpellSlot.Summoner1).Name.Contains("barrier"))
                Barrier = new Spell.Active(SpellSlot.Summoner1);
            else if (Player.Instance.Spellbook.GetSpell(SpellSlot.Summoner2).Name.Contains("barrier"))
                Barrier = new Spell.Active(SpellSlot.Summoner2);
            if (Player.Instance.Spellbook.GetSpell(SpellSlot.Summoner1).Name.Contains("heal"))
                Heal = new Spell.Active(SpellSlot.Summoner1);
            else if (Player.Instance.Spellbook.GetSpell(SpellSlot.Summoner2).Name.Contains("heal"))
                Heal = new Spell.Active(SpellSlot.Summoner2);
        }

        private static void ActivatorSpells()
        {
            var target = TargetSelector.GetTarget(1000, DamageType.Mixed );
            
            var spell = Player.Instance.Spellbook;

            if (Player.Instance.IsInFountain() || Player.Instance.IsRecalling() || target == null) return;

            if (Value.Use("activator.barrier"))
            {
                if (Barrier != null)
                {
                    if (Value.Get("activator.barrier.hp") > Player.Instance.HealthPercent)
                    {
                        if (Barrier.IsReady())
                        {
                            Barrier.Cast();
                        }
                    }
                }
            }

            if (Value.Use("activator.heal"))
            {
                if (Heal != null && Heal.IsReady())
                {
                    if (Value.Use("activator.heal.lifesave"))
                    {
                        var ally =
                            EntityManager.Heroes.Allies.FirstOrDefault(
                                x =>
                                    x.IsValidTarget(Player.Instance.GetAutoAttackRange()) &&
                                    x.HealthPercent <= Value.Get("activator.barrier.hp"));

                        if (ally != null)
                        {

                            if (ally.IsFacing(target))
                            {
                                Heal.Cast();
                            }
                        }
                    }

                    if (Player.Instance.HealthPercent <= Value.Get("activator.heal.hp"))
                    {

                        if (Player.Instance.IsFacing(target))
                        {
                            Heal.Cast();
                        }
                    }
                }
            }

            if (Value.Use("activator.ignite"))
            {
                if (Ignite != null && Ignite.IsReady())
                {
                    if (Value.Use("activator.ignite.killsteal"))
                    {
                        if (target.Health <= DamageLibrary.GetSpellDamage(Player.Instance, target, Ignite.Slot))
                        {
                            Ignite.Cast(target);
                        }
                    }

                    if (Value.Use("activator.ignite.burst"))
                    {
                        if (spell.GetSpell(SpellSlot.Q).IsReady && spell.GetSpell(SpellSlot.W).IsReady && spell.GetSpell(SpellSlot.E).IsReady && spell.GetSpell(SpellSlot.R).IsReady)
                        { 
                            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                            {
                                Ignite.Cast(target);
                            }
                        }
                    }
                    if (Value.Use("activator.ignite.progressive"))
                    {
                        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                        {
                            Ignite.Cast(target);
                        }
                    }
                }
            }



        }

        #endregion


        #region Util

        public static bool IsInFountain(this AIHeroClient hero)
        {
            float fountainRange = 562500; //750 * 750
            var vec3 = (hero.Team == GameObjectTeam.Order)
                ? new Vector3(363, 426, 182)
                : new Vector3(14340, 14390, 172);
            var map = Game.MapId;
            if (map == GameMapId.SummonersRift)
            {
                fountainRange = 1102500; //1050 * 1050
            }
            return hero.IsVisible && hero.Distance(vec3, true) < fountainRange;
        }

        public static bool IsUsingPotion(this AIHeroClient hero)
        {
            return hero.HasBuff("RegenerationPotion") || hero.HasBuff("ItemMiniRegenPotion") ||
                   hero.HasBuff("ItemCrystalFlask") || hero.HasBuff("ItemCrystalFlaskJungle") ||
                   hero.HasBuff("ItemDarkCrystalFlask");
        }
        #endregion
    }
}