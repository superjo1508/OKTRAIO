using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using OKTRAIO.Menu_Settings;
using OKTRAIO.Utility.AutoLVLUP;
using OKTRAIO.Utility.SkinManager;
using MainMenu = EloBuddy.SDK.Menu.MainMenu;

namespace OKTRAIO.Utility
{
    public static class UtilityMenu
    {
        public static Menu Menu, Activator, Autolvlup, Skinmanager, Baseult, Randomult, Bushreveal;

        public static void Init()
        {
            try
            {
                Menu = MainMenu.AddMenu("OKTR Utility", "marks.aio.utility.menu",
                    Player.Instance.ChampionName);
                Menu.AddGroupLabel("OKTR Utilities for " + Player.Instance.ChampionName);
                ActivatorMenu();
                SkinManager();
                BushRevealerMenu();
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
            Activator = Menu.AddSubMenu("Activator", "activator");
            Activator.AddGroupLabel("OKTR AIO - Activator for " + Player.Instance.ChampionName,
                "activator.grouplabel.utilitymenu");
            Activator.Add("activator.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange +=
                Value.AdvancedModeChanged;

            if (Utility.Activator.Heal != null)
            {
                Activator.AddCheckBox("activator.heal", "Use Heal");
                Activator.AddSeparator();
                Activator.AddGroupLabel("Heal Manager:", "activator.label.utilitymenu.heal", true);
                Activator.AddCheckBox("activator.heal.lifesave", "Use Heal for Allies", false, true);
                Activator.AddSlider("activator.heal.hp", "Use Heal if HP are under {0}", 15, 0, 100, true);
                Activator.AddSeparator();
            }

            if (Utility.Activator.Barrier != null)
            {
                Activator.AddCheckBox("activator.barrier", "Use Barrier");
                Activator.AddSeparator();
                Activator.AddGroupLabel("Barrier Manager:", "activator.label.utilitymenu.barrier", true);
                Activator.AddSlider("activator.barrier.hp", "Use Heal if HP are under {0}", 15, 0, 100, true);
                Activator.AddSeparator();
            }

            if (Utility.Activator.Ignite != null)
            {
                Activator.AddCheckBox("activator.ignite", "Use Ignite");
                Activator.AddSeparator();
                Activator.AddGroupLabel("Ignite Manager:", "activator.label.utilitymenu.ignite", true);
                Activator.AddCheckBox("activator.ignite.progressive", "Use Ignite for Progressive Damage", false, true);
                Activator.AddCheckBox("activator.ignite.burst", "Use Ignite for Burst Damage", false, true);
                Activator.AddCheckBox("activator.ignite.killsteal", "Use Ignite for Killsteal", true, true);
                Activator.AddSeparator();
            }
        }

        public static void LvlUpMenu()
        {
            Autolvlup = Menu.AddSubMenu("AutoLVLUP Menu", "autolvlup");
            Autolvlup.AddGroupLabel("OKTR AIO - AutoLVLUP for " + Player.Instance.ChampionName,
                "autolvlup.grouplabel.utilitymenu");
            Autolvlup.AddCheckBox("autolvlup.use", "Use AutoLVLUP");
            Autolvlup.Add("autolvlup.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange +=
                Value.AdvancedModeChanged;
            Autolvlup.AddCheckBox("autolvlup.recognize.spells", "Recognize your Activator Spells", true, true);
            Autolvlup.AddCheckBox("autolvlup.recognize.mode", "Recognize your Masteries", true, true);
            Autolvlup.Add("autolvlup.build.line.mode", new Slider("Build Line: ", 0, 0, 8)).OnValueChange +=
                Leveler.BuildLineSlider;
        }

        public static void SkinManager()
        {
            Skinmanager = Menu.AddSubMenu("SkinManager Menu", "skinmanager");
            Skinmanager.AddGroupLabel("OKTR AIO - Skinmanager for " + Player.Instance.ChampionName,
                "skinmanager.grouplabel.utilitymenu");
            Skinmanager.AddLabel("PSA: Changing your Model might in rare cases crash the game." + Environment.NewLine +
                                  "This does not apply to changing skin.");
            Skinmanager.AddSeparator();
            Skinmanager.Add("skinmanager.models", new Slider("Model - ", 0, 0, 0)).OnValueChange +=
                SkinManagement.SkinManager_OnModelSliderChange;
            Skinmanager.Add("skinmanager.skins", new Slider("Skin - Classic", 0, 0, 0)).OnValueChange +=
                SkinManagement.SkinManager_OnSkinSliderChange;
            Skinmanager.Add("skinmanager.resetModel", new CheckBox("Reset Model", false)).OnValueChange +=
                SkinManagement.SkinManager_OnResetModel;
            Skinmanager.Add("skinmanager.resetSkin", new CheckBox("Reset Skin", false)).OnValueChange +=
                SkinManagement.SkinManager_OnResetSkin;
            Skinmanager.AddSeparator();
        }

        private static void BushRevealerMenu()
        {
            Bushreveal = Menu.AddSubMenu("Bush Reveal", "bushreveal");
            Bushreveal.AddGroupLabel("OKTR AIO - Bush Revealer for " + Player.Instance.ChampionName,
                "bushreveal.grouplabel.utilitymenu");
            Bushreveal.AddCheckBox("bushreveal.use", "Use Bush Revealer");
            Bushreveal.AddCheckBox("bushreveal.humanize", "Humanize the Bush Reveal");
        }

        public static void BaseUltMenu()
        {
            Baseult = Menu.AddSubMenu("BaseUlt Menu", "baseult");
            Baseult.AddGroupLabel("OKTR AIO - BaseULT for " + Player.Instance.ChampionName,
                "baseult.grouplabel.utilitymenu");
            Baseult.AddCheckBox("baseult.use", "Use BaseUlt");
            Baseult.Add("baseult.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange += Value.AdvancedModeChanged;
            Baseult.AddCheckBox("baseult.recallsEnemy", "Show enemy recalls", true, true);
            Baseult.AddCheckBox("baseult.recallsAlly", "Show ally recalls", true, true);
            Baseult.AddSlider("baseult.x", "Recall location X", (int)(Drawing.Width * 0.4), 0, Drawing.Width, true);
            Baseult.AddSlider("baseult.y", "Recall location Y", (int)(Drawing.Height * 0.75), 0, Drawing.Height, true);
            Baseult.AddSlider("baseult.width", "Recall width", 300, 200, 500, true);
            Baseult.AddSeparator();
            Baseult.AddLabel("Use BaseULT for:", 25, "baseult.label", true);
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                Baseult.AddCheckBox("baseult." + enemy.ChampionName, enemy.ChampionName, true, true);
            }
        }

        public static void RandomUltMenu()
        {
            Randomult = Menu.AddSubMenu("RandomUlt Menu", "randomult");
            Randomult.AddGroupLabel("OKTR AIO - RandomUlt for " + Player.Instance.ChampionName,
                "randomult.grouplabel.utilitymenu");
            Randomult.AddCheckBox("randomult.use", "Use RandomUlt");
            Randomult.Add("randomult.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange += Value.AdvancedModeChanged;
            Randomult.AddCheckBox("randomult.recallsEnemy", "Show enemy recalls", true, true);
            Randomult.AddSlider("randomult.x", "Recall location X", (int)(Drawing.Width * 0.4), 0, Drawing.Width, true);
            Randomult.AddSlider("randomult.y", "Recall location Y", (int)(Drawing.Height * 0.75), 0, Drawing.Height, true);
            Randomult.AddSlider("randomult.width", "Recall width", 300, 200, 500, true);
            Randomult.AddSeparator();
            Randomult.AddLabel("Use RandomULT for:", 25, "randomult.label", true);
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                Baseult.AddCheckBox("randomult." + enemy.ChampionName, enemy.ChampionName, true, true);
            }
        }
        }
}

