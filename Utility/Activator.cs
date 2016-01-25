using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using OKTRAIO.Menu_Settings;
using SharpDX;

namespace OKTRAIO.Utility
{
    public static class Activator
    {

        #region Initialize

        public static void Init()
        {
            if (Botrk.IsOwned())
            {
                UtilityMenu.Activator.AddCheckBox("activator.botrk", "Use BOTRK");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Blade of The Ruined King Manager:", "activator.label.utilitymenu.botrk", true);
                UtilityMenu.Activator.AddCheckBox("activator.botrk.combo", "Use BOTRK (COMBO Mode)", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.botrk.ks", "Use BOTRK (KS Mode)", false, true);
                UtilityMenu.Activator.AddCheckBox("activator.botrk.lifesave", "Use BOTRK (LifeSave)", false, true);
                UtilityMenu.Activator.AddSlider("activator.botrk.hp", "Use BOTRK (LifeSave) if HP are under {0}", 20, 0, 100, true);
                UtilityMenu.Activator.AddSeparator();
            }
            if (Cutlass.IsOwned())
            {
                UtilityMenu.Activator.AddCheckBox("activator.bilgewater", "Use BC");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Bilgewater Cutlass Manager:", "activator.label.utilitymenu.bilgewater", true);
                UtilityMenu.Activator.AddCheckBox("activator.bilgewater.combo", "Use BC (COMBO Mode)", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.bilgewater.ks", "Use BC (KS Mode)", false, true);
                UtilityMenu.Activator.AddSeparator();
            }

            if (Youmuus.IsOwned())
            {
                UtilityMenu.Activator.AddCheckBox("activator.youmus", "Use Youmus");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Youmus Manager:", "activator.label.utilitymenu.youmus", true);
                UtilityMenu.Activator.AddCheckBox("activator.youmusspellonly", "Use Youmus only on spell cast", false, true);
                UtilityMenu.Activator.AddSeparator();
            }

            if (Hunter.IsOwned() || Refillable.IsOwned() || Potion.IsOwned() || Biscuit.IsOwned() || Corrupting.IsOwned())
            {
                UtilityMenu.Activator.AddCheckBox("activator.potions", "Use Potions");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Potions Manager:", "activator.label.utilitymenu.potions", true);
                UtilityMenu.Activator.AddSlider("activator.potions.hp", "Use POTIONS if HP are under {0}", 20, 0, 100, true);
                UtilityMenu.Activator.AddSlider("activator.potions.mana", "Use POTIONS if mana is under {0}", 20, 0, 100, true);
                UtilityMenu.Activator.AddSeparator();
            }

            if (MainMenu._menu["useonupdate"].Cast<CheckBox>().CurrentValue)
            {
                Game.OnUpdate += GameOnUpdate;
            }
            else
            {
                Game.OnTick += GameOnUpdate;
            }

            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;

            Shop.OnBuyItem += Shop_OnBuyItem;
        }

        private static void Shop_OnBuyItem(AIHeroClient sender, ShopActionEventArgs args)
        {
            if (args.Id == (int)Botrk.Id && UtilityMenu.Activator["activator.botrk"] == null)
            {
                UtilityMenu.Activator.AddCheckBox("activator.botrk", "Use BOTRK");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Blade of The Ruined King Manager:", "activator.label.utilitymenu.botrk", true);
                UtilityMenu.Activator.AddCheckBox("activator.botrk.combo", "Use BOTRK (COMBO Mode)", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.botrk.ks", "Use BOTRK (KS Mode)", false, true);
                UtilityMenu.Activator.AddCheckBox("activator.botrk.lifesave", "Use BOTRK (LifeSave)", false, true);
                UtilityMenu.Activator.AddSlider("activator.botrk.hp", "Use BOTRK (LifeSave) if HP are under {0}", 20, 0, 100, true);
                UtilityMenu.Activator.AddSeparator();
            }
            if (args.Id == (int)Cutlass.Id && UtilityMenu.Activator["activator.bilgewater"] == null)
            {
                UtilityMenu.Activator.AddCheckBox("activator.bilgewater", "Use BC");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Bilgewater Cutlass Manager:", "activator.label.utilitymenu.bilgewater", true);
                UtilityMenu.Activator.AddCheckBox("activator.bilgewater.combo", "Use BC (COMBO Mode)", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.bilgewater.ks", "Use BC (KS Mode)", false, true);
                UtilityMenu.Activator.AddSeparator();
            }

            if (args.Id == (int)Youmuus.Id && UtilityMenu.Activator["activator.youmus"] == null)
            {
                UtilityMenu.Activator.AddCheckBox("activator.youmus", "Use Youmus");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Youmus Manager:", "activator.label.utilitymenu.youmus", true);
                UtilityMenu.Activator.AddCheckBox("activator.youmusspellonly", "Use Youmus only on spell cast", false, true);
                UtilityMenu.Activator.AddSeparator();
            }

            if ((args.Id == (int)Hunter.Id || args.Id == (int)Refillable.Id || args.Id == (int)Potion.Id || args.Id == (int)Biscuit.Id || args.Id == (int)Corrupting.Id) && UtilityMenu.Activator["activator.potions"] == null)
            {
                UtilityMenu.Activator.AddCheckBox("activator.potions", "Use Potions");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Potions Manager:", "activator.label.utilitymenu.potions", true);
                UtilityMenu.Activator.AddSlider("activator.potions.hp", "Use POTIONS if HP are under {0}", 20, 0, 100, true);
                UtilityMenu.Activator.AddSlider("activator.potions.mana", "Use POTIONS if mana is under {0}", 20, 0, 100, true);
                UtilityMenu.Activator.AddSeparator();
            }
        }

        #endregion

        #region Gamerelated Events

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {

            if (sender.IsMe && Youmuus.IsOwned() && Value.Use("activator.youmusspellonly"))
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
                        Player.Instance.CalculateDamageOnUnit(t, DamageType.Physical, t.MaxHealth * (float)0.1) >
                        t.Health)
                        Botrk.Cast(t);
                    if (Value.Use("activator.botrk.lifesave") &&
                        Player.Instance.HealthPercent < Value.Get("activator.botrk.hp"))
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

            if (Youmuus.IsReady() && Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo && Value.Use("activator.youmus"))
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
                Player.Instance.IsUsingPotion() || (!Hunter.IsOwned() && !Refillable.IsOwned() && !Potion.IsOwned() && !Biscuit.IsOwned() &&
                 !Corrupting.IsOwned()) || !Value.Use("activator.potions"))
                return;

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

        #endregion

        #region Spells

        public static Spell.Targeted Ignite, Smite, Exhaust;

        public static Spell.Active Heal, Barrier;

        public static void LoadSpells()
        {
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner1).Name.Contains("exhaust"))
                Exhaust = new Spell.Targeted(SpellSlot.Summoner1, 650);
            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner2).Name.Contains("exhaust"))
                Exhaust = new Spell.Targeted(SpellSlot.Summoner2, 650);
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner1).Name.Contains("smite"))
                Smite = new Spell.Targeted(SpellSlot.Summoner1, 570);
            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner2).Name.Contains("smite"))
                Smite = new Spell.Targeted(SpellSlot.Summoner2, 570);
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
            var target = TargetSelector.GetTarget(1000, DamageType.Mixed);

            var spell = Player.Instance.Spellbook;

            if (Player.Instance.IsInFountain() || Player.Instance.IsRecalling() || target == null) return;

            if (Barrier != null)
            {
                if (Value.Use("activator.barrier"))
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

            if (Heal != null && Heal.IsReady())
            {
                if (Value.Use("activator.heal"))
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

            if (Ignite != null && Ignite.IsReady())
            {
                if (Value.Use("activator.ignite"))
                {

                    if (Value.Use("activator.ignite.killsteal"))
                    {
                        if (target.Health <= Player.Instance.GetSpellDamage(target, Ignite.Slot))
                        {
                            Ignite.Cast(target);
                        }
                    }

                    if (Value.Use("activator.ignite.burst"))
                    {
                        if (spell.GetSpell(SpellSlot.Q).IsReady && spell.GetSpell(SpellSlot.W).IsReady &&
                            spell.GetSpell(SpellSlot.E).IsReady && spell.GetSpell(SpellSlot.R).IsReady)
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

        public static
        bool IsInFountain(this AIHeroClient hero)
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