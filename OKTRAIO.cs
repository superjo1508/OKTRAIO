using System;
using System.ComponentModel;
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
using Activator = OKTRAIO.Utility.Activator;
using Color = System.Drawing.Color;

namespace OKTRAIO
{
    internal class MarksmanAIO
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
            switch (Player.Instance.ChampionName)
            {
                case "Ashe":
                    Champion = new Ashe();
                    break;
                case "Caitlyn":
                    Champion = new Caitlyn();
                    break;
                case "Corki":
                    Champion = new Corki();
                    break;
                case "Draven":
                    Champion = new Draven();
                    break;
                case "Ezreal":
                    Champion = new Ezreal();
                    break;
                case "Graves":
                    Champion = new Graves();
                    break;
                case "Jinx":
                    Champion = new Jinx();
                    break;
                case "Kalista":
                    Champion = new Kalista();
                    break;
                case "Kindred":
                    Champion = new Kindred();
                    break;
                case "Kog'Maw":
                    Champion = new KogMaw();
                    break;
                case "Lucian":
                    Champion = new Lucian();
                    break;
                case "MissFortune":
                    Champion = new MissFortune();
                    break;
                case "Quinn":
                    Champion = new Quinn();
                    break;
                case "Sivir":
                    Champion = new Sivir();
                    break;
                case "Teemo":
                    Champion = new Teemo();
                    break;
                case "Tristana":
                    Champion = new Tristana();
                    break;
                case "Twitch":
                    Champion = new Twitch();
                    break;
                case "Urgot":
                    Champion = new Urgot();
                    break;
                case "Varus":
                    Champion = new Varus();
                    break;
                case "Vayne":
                    Champion = new Vayne();
                    break;
                case "Katarina":
                    Champion = new Katarina();
                    break;
                    //case "Vayne": champion = new Vayne(); break; //etc
            }
            if (Champion != null)
            {
                Events.Init();
                MainMenu.Init();
                UtilityMenu.Init();
                Champion.Init();
                Activator.LoadSpells();
                Activator.Init();
                Humanizer.Init();
                if (MainMenu._menu["playsound"].Cast<CheckBox>().CurrentValue) PlayWelcome();
                Chat.Print("MarksmanAIO: " + Player.Instance.ChampionName + " Loaded", Color.CornflowerBlue);
            }
            else
            {
                Chat.Print("MarksmanAIO doesn't support: " + Player.Instance.ChampionName);
            }
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
                    client.DownloadFile("http://oktraio.com/VoiceAssistant/" + Player.Instance.ChampionName + ".wav",
                        sandBox + Player.Instance.ChampionName + ".wav");
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                }

                if (File.Exists(sandBox + Player.Instance.ChampionName + ".wav"))
                {
                    _welcomeSound = new SoundPlayer
                    {
                        SoundLocation = SandboxConfig.DataDirectory + @"\OKTR\" + Player.Instance.ChampionName + ".wav"
                    };
                    _welcomeSound.Load();

                    _welcomeSound.Play();
                }
            }
            catch (Exception e)
            {
                Chat.Print("Failed to load Sound File: \"" + Player.Instance.ChampionName + ".wav\": " + e.ToString());
            }

        }

        private static void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _welcomeSound = new SoundPlayer
            {
                SoundLocation = SandboxConfig.DataDirectory + @"\OKTR\" + Player.Instance.ChampionName + ".wav"
            };
            _welcomeSound.Load();
            _welcomeSound.Play();
        }
    }
}
