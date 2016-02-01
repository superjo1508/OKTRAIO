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
        #region ItemLogic

        public static Item

            #region AD Items

            Machete = new Item((int)ItemId.Hunters_Machete),
            Potion = new Item((int)ItemId.Health_Potion),
            Biscuit = new Item(2010),
            Refillable = new Item(2031),
            Hunter = new Item(2032),
            Corrupting = new Item(2033),
            Botrk = new Item((int)ItemId.Blade_of_the_Ruined_King, 550f),
            Cutlass = new Item((int)ItemId.Bilgewater_Cutlass, 550f),
            Youmuus = new Item((int)ItemId.Youmuus_Ghostblade, 650f),
            Qss = new Item((int) ItemId.Quicksilver_Sash),
            DervishBlade = new Item((int)ItemId.Dervish_Blade),
            Mercurial = new Item((int) ItemId.Mercurial_Scimitar),
            KalistaSpear = new Item((int)ItemId.The_Black_Spear),

            #endregion

            #region AP Items

            Zhonya = new Item((int)ItemId.Zhonyas_Hourglass),
            Seraph = new Item(3048),

            #endregion

            #region Support Items

            Talisman = new Item((int) ItemId.Talisman_of_Ascension),
            Randuin = new Item((int) ItemId.Randuins_Omen),
            Glory = new Item((int) ItemId.Righteous_Glory),
            Fotmountain = new Item((int) ItemId.Face_of_the_Mountain),
            Mikael = new Item((int) ItemId.Mikaels_Crucible),
            Ironsolari = new Item((int) ItemId.Locket_of_the_Iron_Solari),
            Coin = new Item((int)ItemId.Ancient_Coin),
            Relic = new Item((int)ItemId.Relic_Shield),
            Edge = new Item((int)ItemId.Spellthiefs_Edge),

            #endregion

            #region Vision

            PinkVision = new Item(2043, 1000f),
            GreaterStealthTotem = new Item((int)ItemId.Greater_Stealth_Totem_Trinket, 1000f),
            GreaterVisionTotem = new Item((int)ItemId.Greater_Vision_Totem_Trinket, 1000f),
            FarsightAlteration = new Item(3363, 1000f),
            WardingTotem = new Item((int)ItemId.Warding_Totem_Trinket, 1000f);

        #endregion

        #endregion

        #region Offensive Items

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

            if (Youmuus.IsReady() && Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo &&
                Value.Use("activator.youmus"))
            {
                var t = Orbwalker.LastTarget;

                if (t.IsValidTarget(Player.Instance.AttackRange) && t is AIHeroClient)
                {
                    Youmuus.Cast();
                }
            }
        }

        #endregion

        #region Initialize

        public static void Init()
        {
            if (Botrk.IsOwned())
            {
                UtilityMenu.Activator.AddCheckBox("activator.botrk", "Use BOTRK");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Blade of The Ruined King Manager:",
                    "activator.label.utilitymenu.botrk", true);
                UtilityMenu.Activator.AddCheckBox("activator.botrk.combo", "Use BOTRK (COMBO Mode)", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.botrk.ks", "Use BOTRK (KS Mode)", false, true);
                UtilityMenu.Activator.AddCheckBox("activator.botrk.lifesave", "Use BOTRK (LifeSave)", false, true);
                UtilityMenu.Activator.AddSlider("activator.botrk.hp", "Use BOTRK (LifeSave) if HP are under {0}", 20, 0,
                    100, true);
                UtilityMenu.Activator.AddSeparator();
            }
            if (Cutlass.IsOwned())
            {
                UtilityMenu.Activator.AddCheckBox("activator.bilgewater", "Use BC");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Bilgewater Cutlass Manager:",
                    "activator.label.utilitymenu.bilgewater", true);
                UtilityMenu.Activator.AddCheckBox("activator.bilgewater.combo", "Use BC (COMBO Mode)", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.bilgewater.ks", "Use BC (KS Mode)", false, true);
                UtilityMenu.Activator.AddSeparator();
            }

            if (Youmuus.IsOwned())
            {
                UtilityMenu.Activator.AddCheckBox("activator.youmus", "Use Youmus");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Youmus Manager:", "activator.label.utilitymenu.youmus", true);
                UtilityMenu.Activator.AddCheckBox("activator.youmusspellonly", "Use Youmus only on spell cast", false,
                    true);
                UtilityMenu.Activator.AddSeparator();
            }

            if (Hunter.IsOwned() || Refillable.IsOwned() || Potion.IsOwned() || Biscuit.IsOwned() ||
                Corrupting.IsOwned())
            {
                UtilityMenu.Activator.AddCheckBox("activator.potions", "Use Potions");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Potions Manager:", "activator.label.utilitymenu.potions", true);
                UtilityMenu.Activator.AddSlider("activator.potions.hp", "Use POTIONS if HP are under {0}", 20, 0, 100,
                    true);
                UtilityMenu.Activator.AddSlider("activator.potions.mana", "Use POTIONS if mana is under {0}", 20, 0, 100,
                    true);
                UtilityMenu.Activator.AddSeparator();
            }

            if (Mercurial.IsOwned() || Qss.IsOwned())
            {
                UtilityMenu.Activator.AddCheckBox("activator.qss", "Use QSS - Mercurial");
                UtilityMenu.Activator.AddCheckBox("activator.qss.ulti", "Prevent ultimates");
                UtilityMenu.Activator.AddCheckBox("activator.qss.bonus", "Use on bonus");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Anti Cloud-Control Manager:", "activator.label.utilitymenu.qss",
                    true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.1", "Use it on Airborne", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.2", "Use it on Blind", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.3", "Use it on Disarm", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.4", "Use it on Forced Action", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.5", "Use it on Root", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.6", "Use it on Silence", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.7", "Use it on Slow", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.8", "Use it on Stasis", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.9", "Use it on Stun", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.10", "Use it on Suppression", true, true);
                UtilityMenu.Activator.AddSeparator();

                if (Value.Use("activator.qss.ulti"))
                {
                    UtilityMenu.Activator.AddGroupLabel("Anti Ultimate Manager:",
                        "activator.label.utilitymenu.qss.antiulti", true);

                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Fiora"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.1", "Prevent Fiora Ultimate", true, true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Fizz"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.2", "Prevent Fizz Ultimate", true, true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Lissandra"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.4", "Prevent Lissandra Ultimate", true,
                            true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Mordekaiser"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.5", "Prevent Mordekaiser Ultimate", true,
                            true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Thresh"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.7", "Prevent Thresh Q", true, true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Vladimir"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.8", "Prevent Vladimir", true, true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Zed"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.9", "Prevent Zed Ultimate", true, true);
                    }
                }

                UtilityMenu.Activator.AddSeparator();

                if (Value.Use("activator.qss.bonus"))
                {
                    UtilityMenu.Activator.AddGroupLabel("Anti Cloud-Control Bonus Manager:",
                        "activator.label.utilitymenu.qss.bonus", true);

                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Vayne"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.bonus.1", "Prevent Vayne Stacks", false, true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Darius"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.bonus.2", "Prevent Darius BloodStacks", false,
                            true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Kalista"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.bonus.3", "Prevent Kalista EStacks", false,
                            true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Tristana"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.bonus.4", "Prevent Tristana EStacks", false,
                            true);
                    }
                }

                UtilityMenu.Activator.AddSlider("activator.qss.prevent.enemies",
                    "Prevent to use QSS if there are less then {0} enemies", 3, 0, 5, true);
                UtilityMenu.Activator.AddSlider("activator.qss.hp", "Use QSS if HP are under {0}", 20, 0, 100, true);
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
            Obj_AI_Base.OnBuffGain += BuffGain;
        }

        private static void Shop_OnBuyItem(AIHeroClient sender, ShopActionEventArgs args)
        {
            if (args.Id == (int) Botrk.Id && UtilityMenu.Activator["activator.botrk"] == null)
            {
                UtilityMenu.Activator.AddCheckBox("activator.botrk", "Use BOTRK");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Blade of The Ruined King Manager:",
                    "activator.label.utilitymenu.botrk", true);
                UtilityMenu.Activator.AddCheckBox("activator.botrk.combo", "Use BOTRK (COMBO Mode)", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.botrk.ks", "Use BOTRK (KS Mode)", false, true);
                UtilityMenu.Activator.AddCheckBox("activator.botrk.lifesave", "Use BOTRK (LifeSave)", false, true);
                UtilityMenu.Activator.AddSlider("activator.botrk.hp", "Use BOTRK (LifeSave) if HP are under {0}", 20, 0,
                    100, true);
                UtilityMenu.Activator.AddSeparator();
            }
            if (args.Id == (int) Cutlass.Id && UtilityMenu.Activator["activator.bilgewater"] == null)
            {
                UtilityMenu.Activator.AddCheckBox("activator.bilgewater", "Use BC");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Bilgewater Cutlass Manager:",
                    "activator.label.utilitymenu.bilgewater", true);
                UtilityMenu.Activator.AddCheckBox("activator.bilgewater.combo", "Use BC (COMBO Mode)", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.bilgewater.ks", "Use BC (KS Mode)", false, true);
                UtilityMenu.Activator.AddSeparator();
            }

            if (args.Id == (int) Youmuus.Id && UtilityMenu.Activator["activator.youmus"] == null)
            {
                UtilityMenu.Activator.AddCheckBox("activator.youmus", "Use Youmus");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Youmus Manager:", "activator.label.utilitymenu.youmus", true);
                UtilityMenu.Activator.AddCheckBox("activator.youmusspellonly", "Use Youmus only on spell cast", false,
                    true);
                UtilityMenu.Activator.AddSeparator();
            }

            if (args.Id == (int) Zhonya.Id && UtilityMenu.Activator["activator.zhonya"] == null)
            {
                UtilityMenu.Activator.AddCheckBox("activator.zhonya", "Use Zhonya - WIP");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Zhonya Manager:", "activator.label.utilitymenu.zhonya", true);
                UtilityMenu.Activator.AddCheckBox("activator.zhonya.prevent", "Prevent to use Zhonya", true, true);
                UtilityMenu.Activator.AddSlider("activator.zhonya.prevent.enemies",
                    "Prevent to use Zhonya if there are more then {0} enemies", 3, 0, 5, true);
                UtilityMenu.Activator.AddSlider("activator.zhonya.hp", "Use Zhonys if HP are under {0}", 20, 0, 100,
                    true);
                UtilityMenu.Activator.AddSeparator();
            }

            if (args.Id == (int) Seraph.Id && UtilityMenu.Activator["activator.seraph"] == null)
            {
                UtilityMenu.Activator.AddCheckBox("activator.seraph", "Use Seraph - WIP");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Seraph Manager:", "activator.label.utilitymenu.zhonya", true);
                UtilityMenu.Activator.AddCheckBox("activator.seraph.prevent", "Prevent to use Seraph", true, true);
                UtilityMenu.Activator.AddSlider("activator.seraph.prevent.enemies",
                    "Prevent to use Seraph if there are more then {0} enemies", 3, 0, 5, true);
                UtilityMenu.Activator.AddSlider("activator.seraph.hp", "Use Seraph if HP are under {0}", 20, 0, 100,
                    true);
                UtilityMenu.Activator.AddSeparator();
            }

            if ((args.Id == (int) Mercurial.Id || args.Id == (int) Qss.Id) &&
                UtilityMenu.Activator["activator.qss"] == null)
            {
                UtilityMenu.Activator.AddCheckBox("activator.qss", "Use QSS - Mercurial");
                UtilityMenu.Activator.AddCheckBox("activator.qss.ulti", "Prevent ultimates");
                UtilityMenu.Activator.AddCheckBox("activator.qss.bonus", "Use on bonus");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Anti Cloud-Control Manager:", "activator.label.utilitymenu.qss",
                    true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.1", "Use it on Airborne", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.2", "Use it on Blind", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.3", "Use it on Disarm", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.4", "Use it on Forced Action", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.5", "Use it on Root", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.6", "Use it on Silence", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.7", "Use it on Slow", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.8", "Use it on Stasis", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.9", "Use it on Stun", true, true);
                UtilityMenu.Activator.AddCheckBox("activator.qss.cc.10", "Use it on Suppression", true, true);
                UtilityMenu.Activator.AddSeparator();

                if (Value.Use("activator.qss.ulti"))
                {
                    UtilityMenu.Activator.AddGroupLabel("Anti Ultimate Manager:",
                        "activator.label.utilitymenu.qss.antiulti", true);

                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Fiora"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.1", "Prevent Fiora Ultimate", true, true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Fizz"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.2", "Prevent Fizz Ultimate", true, true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Lissandra"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.4", "Prevent Lissandra Ultimate", true,
                            true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Mordekaiser"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.5", "Prevent Mordekaiser Ultimate", true,
                            true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Thresh"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.7", "Prevent Thresh Q", true, true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Vladimir"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.8", "Prevent Vladimir", true, true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Zed"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.ulti.9", "Prevent Zed Ultimate", true, true);
                    }
                }

                UtilityMenu.Activator.AddSeparator();

                if (Value.Use("activator.qss.bonus"))
                {
                    UtilityMenu.Activator.AddGroupLabel("Anti Cloud-Control Bonus Manager:",
                        "activator.label.utilitymenu.qss.bonus", true);

                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Vayne"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.bonus.1", "Prevent Vayne Stacks", false, true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Darius"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.bonus.2", "Prevent Darius BloodStacks", false,
                            true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Kalista"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.bonus.3", "Prevent Kalista EStacks", false,
                            true);
                    }
                    if (EntityManager.Heroes.Enemies.Any(a => a.ChampionName == "Tristana"))
                    {
                        UtilityMenu.Activator.AddCheckBox("activator.qss.bonus.4", "Prevent Tristana EStacks", false,
                            true);
                    }
                }

                UtilityMenu.Activator.AddSlider("activator.qss.prevent.enemies",
                    "Prevent to use QSS if there are less then {0} enemies", 3, 0, 5, true);
                UtilityMenu.Activator.AddSlider("activator.qss.hp", "Use QSS if HP are under {0}", 20, 0, 100, true);
                UtilityMenu.Activator.AddSeparator();
            }

            if ((args.Id == (int) Hunter.Id || args.Id == (int) Refillable.Id || args.Id == (int) Potion.Id ||
                 args.Id == (int) Biscuit.Id || args.Id == (int) Corrupting.Id) &&
                UtilityMenu.Activator["activator.potions"] == null)
            {
                UtilityMenu.Activator.AddCheckBox("activator.potions", "Use Potions");
                UtilityMenu.Activator.AddSeparator();
                UtilityMenu.Activator.AddGroupLabel("Potions Manager:", "activator.label.utilitymenu.potions", true);
                UtilityMenu.Activator.AddSlider("activator.potions.hp", "Use POTIONS if HP are under {0}", 20, 0, 100,
                    true);
                UtilityMenu.Activator.AddSlider("activator.potions.mana", "Use POTIONS if mana is under {0}", 20, 0, 100,
                    true);
                UtilityMenu.Activator.AddSeparator();
            }
        }

        #endregion

        #region Gamerelated Events

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && Youmuus.IsOwned() && Value.Use("activator.youmusspellonly"))
            {
                if (((Player.Instance.ChampionName == "Lucian" ||
                      Player.Instance.ChampionName == "Twitch" ||
                      Player.Instance.ChampionName == "Zed" ||
                      Player.Instance.ChampionName == "Varus") && args.Slot == SpellSlot.R) ||
                    (Player.Instance.ChampionName == "Ashe" && args.Slot == SpellSlot.Q))

                {
                    Youmuus.Cast();
                }
            }
        }

        private static void GameOnUpdate(EventArgs args)
        {
            OffensiveItems();
            DefensiveItems();
            PlayerSpells();
        }

        #endregion

        #region Defensive Items

        private static void DefensiveItems()
        {
            Potions();
        }

        private static void Potions()
        {
            if (Player.Instance.IsInFountain() || Player.Instance.IsRecalling() ||
                Player.Instance.IsUsingPotion() ||
                (!Hunter.IsOwned() && !Refillable.IsOwned() && !Potion.IsOwned() && !Biscuit.IsOwned() &&
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

        private static void BuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            if (!sender.IsMe) return;

            if ((Qss.IsOwned() || Mercurial.IsOwned()) && Value.Use("activator.qss"))
            {
                if (Value.Get("activator.qss.prevent.enemies") >=
                    Player.Instance.CountEnemiesInRange(Player.Instance.GetAutoAttackRange()) &&
                    Value.Get("activator.qss.hp") >= Player.Instance.HealthPercent)
                {
                    if (Value.Use("activator.qss.cc.1") &&
                        (args.Buff.Type == BuffType.Knockback || args.Buff.Type == BuffType.Knockup))
                    {
                        AutoRemove();
                    }
                    if (Value.Use("activator.qss.cc.2") && args.Buff.Type == BuffType.Blind)
                    {
                        AutoRemove();
                    }
                    if (Value.Use("activator.qss.cc.3") && args.Buff.Type == BuffType.Polymorph)
                    {
                        AutoRemove();
                    }
                    if (Value.Use("activator.qss.cc.4") &&
                        (args.Buff.Type == BuffType.Charm || args.Buff.Type == BuffType.Fear ||
                         args.Buff.Type == BuffType.Flee || args.Buff.Type == BuffType.Taunt))
                    {
                        AutoRemove();
                    }
                    if (Value.Use("activator.qss.cc.5") && args.Buff.Type == BuffType.Snare)
                    {
                        AutoRemove();
                    }
                    if (Value.Use("activator.qss.cc.6") && args.Buff.Type == BuffType.Silence)
                    {
                        AutoRemove();
                    }
                    if (Value.Use("activator.qss.cc.7") && args.Buff.Type == BuffType.Slow)
                    {
                        AutoRemove();
                    }
                    if (Value.Use("activator.qss.cc.8") && args.Buff.Type == BuffType.Sleep)
                    {
                        AutoRemove();
                    }
                    if (Value.Use("activator.qss.cc.9") && args.Buff.Type == BuffType.Stun)
                    {
                        AutoRemove();
                    }
                    if (Value.Use("activator.qss.cc.10") && args.Buff.Type == BuffType.Suppression)
                    {
                        AutoRemove();
                    }

                    if (Value.Use("activator.qss.ulti"))
                    {
                        if (Value.Use("activator.qss.ulti.1") && args.Buff.Name == "FioraRMark")
                        {
                            AutoRemove();
                        }
                        if (Value.Use("activator.qss.ulti.2") && args.Buff.Name == "FizzMarinerDoom")
                        {
                            AutoRemove();
                        }
                        if (Value.Use("activator.qss.ulti.4") && args.Buff.Name == "LissandraR")
                        {
                            AutoRemove();
                        }
                        if (Value.Use("activator.qss.ulti.5") && args.Buff.Name == "MordekaiserChildrenOfTheGrave")
                        {
                            AutoRemove();
                        }
                        if (Value.Use("activator.qss.ulti.7") && args.Buff.Name == "ThreshQ")
                        {
                            AutoRemove();
                        }
                        if (Value.Use("activator.qss.ulti.8") && args.Buff.Name == "VladimirHemoplague")
                        {
                            AutoRemove();
                        }
                        if (Value.Use("activator.qss.ulti.9") && args.Buff.Name == "zedulttargetmark")
                        {
                            AutoRemove();
                        }
                    }

                    if (Value.Use("activator.qss.bonus"))
                    {
                        if (Value.Use("activator.qss.bonus.1") && args.Buff.Name == "vaynesilvereddebuff")
                        {
                            if (Player.Instance.GetBuffCount("vaynesilvereddebuff") >= 2)
                            {
                                AutoRemove();
                            }
                        }
                        if (Value.Use("activator.qss.bonus.2") && args.Buff.Name == "dariushemo")
                        {
                            if (Player.Instance.GetBuffCount("dariushemo") >= 4)
                            {
                                AutoRemove();
                            }
                        }
                        if (Value.Use("activator.qss.bonus.3") && args.Buff.DisplayName == "KalistaExpungeMarker")
                        {
                            if (Player.Instance.GetBuffCount("KalistaExpungeMarker") >= 6)
                            {
                                AutoRemove();
                            }
                        }
                        if (Value.Use("activator.qss.bonus.4") && args.Buff.Name == "tristanaecharge")
                        {
                            if (Player.Instance.GetBuffCount("tristanaecharge") >= 3)
                            {
                                AutoRemove();
                            }
                        }
                    }
                }
            }
        }

        private static void AutoRemove()
        {
            if (Game.MapId == GameMapId.SummonersRift)
            {
                if (Qss.IsOwned() && Qss.IsReady())
                {
                    Qss.Cast();
                }

                else if (Mercurial.IsOwned() && Mercurial.IsReady())
                {
                    Mercurial.Cast();
                }
            }

            else if (Game.MapId == GameMapId.TwistedTreeline)
            {
                if (Qss.IsOwned() && Qss.IsReady())
                {
                    Qss.Cast();
                }

                else if (DervishBlade.IsOwned() && DervishBlade.IsReady())
                {
                    DervishBlade.Cast();
                }
            }
        }

        #endregion

        #region Spells

        public static
            Spell.Targeted Ignite, Smite, Exhaust;

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

        private static void PlayerSpells()
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
            var vec3 = hero.Team == GameObjectTeam.Order
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