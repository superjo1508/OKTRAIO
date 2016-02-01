using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Media;
using System.Net;
using EloBuddy;
using EloBuddy.Sandbox;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using OKTRAIO.Champions;
using OKTRAIO.Menu_Settings;
using OKTRAIO.Utility;
using OKTRAIO.Utility.SkinManager;
using Activator = System.Activator;

namespace OKTRAIO
{
    internal class Brain
    {
        public static AIOChampion Champion;

        private static SoundPlayer _welcomeSound;

        private static void Main(string[] args)
        {
            try
            {
                Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code 1)</font>");
            }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            var champion = Type.GetType("OKTRAIO.Champions." + Player.Instance.ChampionName);
            if (champion != null)
            {
                Console.Write("[MarksmanAIO] " + Player.Instance.ChampionName + " Loaded");
                Champion = (AIOChampion) Activator.CreateInstance(champion);
                Events.Init();
                MainMenu.Init();
                UtilityMenu.Init();
                Champion.Init();
                JsonSettings.Init();
                Utility.Activator.LoadSpells();
                Utility.Activator.Init();
                Humanizer.Init();
                SkinManagement.Init();
                if (MainMenu._menu["playsound"].Cast<CheckBox>().CurrentValue)
                {
                    PlayWelcome();
                }
                Chat.Print("MarksmanAIO: " + Player.Instance.ChampionName + " Loaded", Color.CornflowerBlue);
            }
            else
            {
                Chat.Print("MarksmanAIO doesn't support: " + Player.Instance.ChampionName);
            }

            if (RandomUlt.IsCompatibleChamp() && champion == null)
            {
                UtilityMenu.Init();
            }
            if (BaseUlt.IsCompatibleChamp())
            {
                UtilityMenu.BaseUltMenu();
                BaseUlt.Initialize();
            }
            if (RandomUlt.IsCompatibleChamp())
            {
                UtilityMenu.RandomUltMenu();
                RandomUlt.Initialize();
            }
            Value.Init();
        }

        private static void PlayWelcome()
        {
            try
            {
                var sandBox = SandboxConfig.DataDirectory + @"\OKTR\";

                if (!Directory.Exists(sandBox))
                {
                    Directory.CreateDirectory(sandBox);
                }

                if (!File.Exists(sandBox + Player.Instance.ChampionName + ".wav"))
                {
                    var client = new WebClient();
                    client.DownloadFile(
                        "http://oktraio.com/VoiceAssistant/" + Player.Instance.ChampionName + ".wav",
                        sandBox + Player.Instance.ChampionName + ".wav");
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                }

                if (File.Exists(sandBox + Player.Instance.ChampionName + ".wav"))
                {
                    _welcomeSound = new SoundPlayer
                    {
                        SoundLocation =
                            SandboxConfig.DataDirectory + @"\OKTR\" + Player.Instance.ChampionName
                            + ".wav"
                    };

                    _welcomeSound.Load();
                    if (_welcomeSound == null || !_welcomeSound.IsLoadCompleted)
                        return;
                    _welcomeSound.Play();
                }
            }
            catch (Exception e)
            {
                Chat.Print("Failed to load Sound File: \"" + Player.Instance.ChampionName + ".wav\": " + e);
            }
        }

        private static void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _welcomeSound = new SoundPlayer
            {
                SoundLocation =
                    SandboxConfig.DataDirectory + @"\OKTR\" + Player.Instance.ChampionName + ".wav"
            };
            _welcomeSound.Load();
            _welcomeSound.Play();
        }
    }
}