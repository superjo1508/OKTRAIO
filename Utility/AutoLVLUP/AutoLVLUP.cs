using EloBuddy;
using EloBuddy.SDK.Menu.Values;
using OKTRAIO.Menu_Settings;

namespace OKTRAIO.Utility.AutoLVLUP
{
    class Leveler
    {
        #region Bool
        public static
            bool AbilityPower;

        public static 
            bool Jungler;

        public static
            bool Laner;

        public static
            bool Support;

        public static int[] 
            AbilitySequence;

        public static int 
            QOff = 0, 
            WOff = 0, 
            EOff = 0, 
            ROff;
        #endregion

        #region UtilityMenu related - Build Line

        public static void BuildLineSlider(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            UpdateSlider(1);
        }

        #endregion

        public static void UpdateSlider(int id)
        {
            switch (id)
            {
                    #region Switch related - Build Line 

                case 1:
                    buildline();
                    break;

                    #endregion
            }
        }

        #region Switch case 1 - Build Line

        public static void buildline()
        {
            string buildName;

            buildName = "Build Line: ";

            var value = Value.Get("autolvlup.build.line.mode");

            switch (value)
            {
                case 0:
                    buildName = buildName + "Mobafire";
                    break;
                case 1:
                    buildName = buildName + "LolKing";
                    break;
                case 2:
                    buildName = buildName + "ProBuilds";
                    break;
                case 3:
                    buildName = buildName + "LolPro";
                    break;
                case 4:
                    buildName = buildName + "SoloMid";
                    break;
                case 5:
                    buildName = buildName + "LolClass";
                    break;
                case 6:
                    buildName = buildName + "ChampionGG";
                    break;
                case 7:
                    buildName = buildName + "LeagueCraft";
                    break;
                case 8:
                    buildName = buildName + "MetaLol";
                    break;
            }

            UtilityMenu.Autolvlup["autolvlup.build.line.mode"].Cast<Slider>().DisplayName = buildName;
        }

        #endregion

        #region Recognize Damage
        public static void Masteries()
        {
            if (Player.Instance.PercentMagicDamageMod >= Player.Instance.PercentPhysicalDamageMod)
            {
                AbilityPower = true;
            }
        }
        #endregion

        #region Recognize Spells
        public static void Spells()
        {
            if (Activator.Barrier != null || Activator.Heal != null || Activator.Ignite != null)
            {
                Laner = true;
            }
            else if (Activator.Smite != null)
            {
                Jungler = true;
            }
            else if (Activator.Exhaust != null)
            {
                Support = true;
            }
        }
        #endregion

        public static void Recognize()
        {

        }

    }
}


