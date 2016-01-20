using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using OKTRAIO.Menu_Settings;

namespace OKTRAIO.Utility
{
    public static class UtilityMenu
    {
        public static Menu _menu, _activator, _baseult, _randomult;

        public static void Init()
        {
            try
            {
                _menu = EloBuddy.SDK.Menu.MainMenu.AddMenu("OKTR Utility", "marks.aio.utility.menu",
                    Player.Instance.ChampionName);
                _menu.AddGroupLabel("OKTR Utilities for " + Player.Instance.ChampionName);
                ActivatorMenu();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code Utility Menu)</font>");
            }
        }

        public static void ActivatorMenu()
        {
            _activator = _menu.AddSubMenu("Activator", "activator");
            _activator.AddGroupLabel("OKTR AIO - Activator for " + Player.Instance.ChampionName,
                "activator.grouplabel.utilitymenu");
            _activator.AddCheckBox("activator.botrk", "Use BOTRK");
            _activator.AddCheckBox("activator.bilgewater", "Use BC");
            _activator.AddCheckBox("activator.youmus", "Use Youmus");
            _activator.AddCheckBox("activator.potions", "Use Potions");
            _activator.AddCheckBox("activator.heal", "Use Heal");
            _activator.AddCheckBox("activator.barrier", "Use Barrier");
            _activator.AddCheckBox("activator.ignite", "Use Ignite");
            _activator.AddSeparator();
            _activator.Add("activator.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange += Value.AdvancedModeChanged;
            _activator.AddSeparator();
            _activator.AddGroupLabel("Blade of The Ruined King Manager:", "activator.label.utilitymenu.botrk", true);
            _activator.AddCheckBox("activator.botrk.combo", "Use BOTRK (COMBO Mode)", true, true);
            _activator.AddCheckBox("activator.botrk.ks", "Use BOTRK (KS Mode)", false, true);
            _activator.AddCheckBox("activator.botrk.lifesave", "Use BOTRK (LifeSave)", false, true);
            _activator.AddSlider("activator.botrk.hp", "Use BOTRK (LifeSave) if HP are under {0}", 20, 0, 100, true);
            _activator.AddSeparator();
            _activator.AddGroupLabel("Bilgewater Cutlass Manager:", "activator.label.utilitymenu.bilgewater", true);
            _activator.AddCheckBox("activator.bilgewater.combo", "Use BC (COMBO Mode)", true, true);
            _activator.AddCheckBox("activator.bilgewater.ks", "Use BC (KS Mode)", false, true);
            _activator.AddSeparator();
            _activator.AddGroupLabel("Youmus Manager:", "activator.label.utilitymenu.youmus", true);
            _activator.AddCheckBox("activator.youmusspellonly", "Use Youmus only on spell cast", false, true);
            _activator.AddSeparator();
            _activator.AddGroupLabel("Potions Manager:", "activator.label.utilitymenu.potions", true);
            _activator.AddSlider("activator.potions.hp", "Use POTIONS if HP are under {0}", 20, 0, 100, true);
            _activator.AddSlider("activator.potions.mana", "Use POTIONS if mana is under {0}", 20, 0, 100, true);
            _activator.AddSeparator();
            _activator.AddGroupLabel("Heal Manager:", "activator.label.utilitymenu.heal", true);
            _activator.AddCheckBox("activator.heal.lifesave", "Use Heal for Allies", false, true);
            _activator.AddSlider("activator.heal.hp", "Use Heal if HP are under {0}", 15, 0, 100, true);
            _activator.AddSeparator();
            _activator.AddGroupLabel("Barrier Manager:", "activator.label.utilitymenu.barrier", true);
            _activator.AddSlider("activator.barrier.hp", "Use Heal if HP are under {0}", 15, 0, 100, true);
            _activator.AddSeparator();
            _activator.AddGroupLabel("Ignite Manager:", "activator.label.utilitymenu.ignite", true);
            _activator.AddCheckBox("activator.ignite.progressive", "Use Ignite for Progressive Damage", false, true);
            _activator.AddCheckBox("activator.ignite.burst", "Use Ignite for Burst Damage", false, true);
            _activator.AddCheckBox("activator.ignite.killsteal", "Use Ignite for Killsteal", true, true);
        }
        public static void BaseUltMenu()
        {
            _baseult = _menu.AddSubMenu("BaseUlt Menu", "baseult");
            _baseult.AddGroupLabel("OKTR AIO - BaseULT for " + Player.Instance.ChampionName,
                "baseult.grouplabel.utilitymenu");
            _baseult.AddCheckBox("baseult.use", "Use BaseUlt");
            _baseult.Add("baseult.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange += Value.AdvancedModeChanged;
            _baseult.AddCheckBox("baseult.recallsEnemy", "Show enemy recalls", true, true);
            _baseult.AddCheckBox("baseult.recallsAlly", "Show ally recalls", true, true);
            _baseult.AddSlider("baseult.x", "Recall location X", (int)(Drawing.Width * 0.4), 0, Drawing.Width, true);
            _baseult.AddSlider("baseult.y", "Recall location Y", (int)(Drawing.Height * 0.75), 0, Drawing.Height, true);
            _baseult.AddSlider("baseult.width", "Recall width", 300, 200, 500, true);
            _baseult.AddSeparator();
            _baseult.AddLabel("Use BaseULT for:", 25, "baseult.label", true);
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                _baseult.AddCheckBox("baseult." + enemy.ChampionName, enemy.ChampionName, true, true);
            }
        }

        public static void RandomUltMenu()
        {
            _randomult = _menu.AddSubMenu("RandomUlt Menu", "randomult");
            _randomult.AddGroupLabel("OKTR AIO - RandomUlt for " + Player.Instance.ChampionName,
                "randomult.grouplabel.utilitymenu");
            _randomult.AddCheckBox("randomult.use", "Use RandomUlt");
            _randomult.Add("randomult.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange += Value.AdvancedModeChanged;
            _randomult.AddCheckBox("randomult.recallsEnemy", "Show enemy recalls", true, true);
            _randomult.AddSlider("randomult.x", "Recall location X", (int)(Drawing.Width * 0.4), 0, Drawing.Width, true);
            _randomult.AddSlider("randomult.y", "Recall location Y", (int)(Drawing.Height * 0.75), 0, Drawing.Height, true);
            _randomult.AddSlider("randomult.width", "Recall width", 300, 200, 500, true);
            _randomult.AddSeparator();
            _randomult.AddLabel("Use RandomULT for:", 25, "randomult.label", true);
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                _baseult.AddCheckBox("randomult." + enemy.ChampionName, enemy.ChampionName, true, true);
            }
        }
        }
}

