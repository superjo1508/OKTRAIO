using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using OKTRAIO.Utility;

namespace OKTRAIO.Menu_Settings
{
    public static class Value
    {
        public static void Init()
        {
            if (MainMenu._combo != null) MenuList.Add(MainMenu._combo);
            if (MainMenu._lane != null) MenuList.Add(MainMenu._lane);
            if (MainMenu._jungle != null) MenuList.Add(MainMenu._jungle);
            if (MainMenu._lasthit != null) MenuList.Add(MainMenu._lasthit);
            if (MainMenu._harass != null) MenuList.Add(MainMenu._harass);
            if (MainMenu._flee != null) MenuList.Add(MainMenu._flee);
            if (MainMenu._misc != null) MenuList.Add(MainMenu._misc);
            if (MainMenu._ks != null) MenuList.Add(MainMenu._ks);
            if (MainMenu._draw != null) MenuList.Add(MainMenu._draw);
            if (UtilityMenu._activator != null) MenuList.Add(UtilityMenu._activator);
            if (UtilityMenu._baseult != null) MenuList.Add(UtilityMenu._baseult);
            if (UtilityMenu._randomult != null) MenuList.Add(UtilityMenu._randomult);
        }

        public static readonly string[] MenuStrings = { "combo", "lane", "jungle", "lasthit", "harass", "flee", "misc", "killsteal", "draw", "activator", "baseult", "randomult" };
        public static List<string> AdvancedMenuItemUiDs = new List<string>();
        public static List<Menu> MenuList = new List<Menu>();

        public static bool Use(string id)
        {
            return MenuList.Find(m => m.UniqueMenuId.Contains(MenuSubString(id)))[id].Cast<CheckBox>().CurrentValue;
        }

        public static string MenuSubString(string id)
        {
            return id.Substring(0, id.IndexOf(".", StringComparison.OrdinalIgnoreCase));
        }

        public static bool Mode(Orbwalker.ActiveModes id)
        {
            return Orbwalker.ActiveModesFlags.HasFlag(id);
        }

        public static int Get(string id)
        {
            return MenuList.Find(m => m.UniqueMenuId.Contains(MenuSubString(id)))[id].Cast<Slider>().CurrentValue;
        }

        public static bool Active(string id)
        {
            return MenuList.Find(m => m.UniqueMenuId.Contains(MenuSubString(id)))[id].Cast<KeyBind>().CurrentValue;
        }

        public static void AddCheckBox(this Menu menu, string uid, string displayname, bool defaultvalue = true,
            bool advanced = false)
        {
            menu.Add(uid, new CheckBox(displayname, defaultvalue));
            if (!advanced) return;
            AdvancedMenuItemUiDs.Add(uid);
            menu[uid].Cast<CheckBox>().IsVisible = menu[GetMenuString(menu) + ".advanced"].Cast<CheckBox>().CurrentValue;
        }

        public static void AddSlider(this Menu menu, string uid, string displayName, int defaultValue = 0, int minValue = 0, int maxValue = 100, bool advanced = false)
        {
            menu.Add(uid, new Slider(displayName, defaultValue, minValue, maxValue));
            if (!advanced) return;
            AdvancedMenuItemUiDs.Add(uid);
            menu[uid].Cast<Slider>().IsVisible = menu[GetMenuString(menu) + ".advanced"].Cast<CheckBox>().CurrentValue;
        }

        public static void AddLabel(this Menu menu, string text, int size = 25, string uid = null, bool advanced = false)
        {
            if (uid != null)
            {
                menu.Add(uid, new Label(text));
                if (!advanced) return;
                AdvancedMenuItemUiDs.Add(uid);
                menu[uid].Cast<Label>().IsVisible = menu[GetMenuString(menu) + ".advanced"].Cast<CheckBox>().CurrentValue;
            }
            else
            {
                menu.AddLabel(text, size);
            }
        }

        public static void AddGroupLabel(this Menu menu, string text, string uid = null, bool advanced = false)
        {
            if (uid != null)
            {
                menu.Add(uid, new GroupLabel(text));
                if (!advanced) return;
                AdvancedMenuItemUiDs.Add(uid);
                menu[uid].Cast<GroupLabel>().IsVisible = menu[GetMenuString(menu) + ".advanced"].Cast<CheckBox>().CurrentValue;
            }
            else
            {
                menu.AddGroupLabel(text);
            }
        }

        private static string GetMenuString(Menu menu)
        {
            return MenuStrings.First(m => menu.UniqueMenuId.Contains(m));
        }

        public static void AdvancedModeChanged(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            if (AdvancedMenuItemUiDs.All(uid => MenuSubString(sender.SerializationId) != MenuSubString(uid))) return;
            foreach (var box in AdvancedMenuItemUiDs.Where(uid=>MenuSubString(sender.SerializationId) == MenuSubString(uid)))
            {
                MenuList.Find(m => m.UniqueMenuId.Contains(MenuSubString(box)))[box].IsVisible = args.NewValue;
            }
        }
    }
}
