using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using EloBuddy;
using EloBuddy.SDK.Menu.Values;

namespace OKTRAIO.Utility.SkinManager
{
    class SkinManagement
    {
        public static XmlDocument InfoXml;
        public static Model[] Models;
        public static int OriginalSkinIndex;

        internal struct Model
        {
            public readonly string Name;
            public readonly ModelSkin[] Skins;

            public Model(string name, ModelSkin[] skins)
            {
                Name = name;
                Skins = skins;
            }

            public string[] GetSkinNames()
            {
                return Skins.Select(skin => skin.Name).ToArray();
            }
        }

        internal struct ModelSkin
        {
            public readonly string Name;
            public readonly int Index;

            public ModelSkin(string name, string index)
            {
                Name = name;
                Index = int.Parse(index);
            }
        }

        public static Model GetModelByIndex(int index)
        {
            return Models[index];
        }

        public static Model GetModelByName(string name)
        {
            return
                Models.FirstOrDefault(
                    model => string.Equals(model.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        public static string[] ModelNames;

        public static void Init()
        {
            using (var infoStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OKTRAIO.Utility.SkinManager.SkinInfo.xml"))
               
                if (infoStream != null)

                    using (var infoReader = new StreamReader(infoStream))
                    {
                        Chat.Print(infoStream.Length);
                        InfoXml = new XmlDocument();
                        InfoXml.LoadXml(infoReader.ReadToEnd());
                    }

            if (InfoXml.DocumentElement != null)
                Models =
                    InfoXml.DocumentElement.ChildNodes.Cast<XmlElement>()
                        .Select(
                            model =>
                                new Model(model.Attributes["name"].Value,
                                    model.ChildNodes.Cast<XmlElement>()
                                        .Select(
                                            skin =>
                                                new ModelSkin(skin.Attributes["name"].Value, skin.Attributes["index"].Value))
                                        .ToArray()))
                        .ToArray();
            ModelNames = Models.Select(model => model.Name).ToArray();

            OriginalSkinIndex = Player.Instance.SkinId;
            UtilityMenu.Skinmanager["skinmanager.models"].Cast<Slider>().MaxValue = Models.Length - 1;
            UtilityMenu.Skinmanager["skinmanager.models"].Cast<Slider>().CurrentValue = Array.IndexOf(ModelNames, Player.Instance.ChampionName);
        }

        public static void SkinManager_OnResetSkin(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            UtilityMenu.Skinmanager["skinmanager.skins"].Cast<Slider>().CurrentValue = OriginalSkinIndex;
            if (UtilityMenu.Skinmanager["skinmanager.resetSkin"].Cast<CheckBox>().CurrentValue)
                UtilityMenu.Skinmanager["skinmanager.resetSkin"].Cast<CheckBox>().CurrentValue = false;
        }

        public static void SkinManager_OnResetModel(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            UtilityMenu.Skinmanager["skinmanager.models"].Cast<Slider>().CurrentValue = Array.IndexOf(ModelNames, Player.Instance.ChampionName);

            if (UtilityMenu.Skinmanager["skinmanager.resetModel"].Cast<CheckBox>().CurrentValue)
                UtilityMenu.Skinmanager["skinmanager.resetModel"].Cast<CheckBox>().CurrentValue = false;
        }

        public static void SkinManager_OnSkinSliderChange(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            var model = GetModelByIndex(UtilityMenu.Skinmanager["skinmanager.models"].Cast<Slider>().CurrentValue);
            var skin = model.Skins[UtilityMenu.Skinmanager["skinmanager.skins"].Cast<Slider>().CurrentValue];
            UtilityMenu.Skinmanager["skinmanager.skins"].Cast<Slider>().DisplayName = "Skin - " + skin.Name;
            EloBuddy.Player.SetSkinId(skin.Index);
        }

        public static void SkinManager_OnModelSliderChange(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            var model = GetModelByIndex(UtilityMenu.Skinmanager["skinmanager.models"].Cast<Slider>().CurrentValue);
            UtilityMenu.Skinmanager["skinmanager.models"].Cast<Slider>().DisplayName = "Model - " + model.Name;
            EloBuddy.Player.SetModel(model.Name);
            UtilityMenu.Skinmanager["skinmanager.skins"].Cast<Slider>().CurrentValue = 0;
            UtilityMenu.Skinmanager["skinmanager.skins"].Cast<Slider>().MaxValue = model.Skins.Length - 1;
        }
    }
}
