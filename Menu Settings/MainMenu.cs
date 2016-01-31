using System;
using EloBuddy;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace OKTRAIO.Menu_Settings
{
    internal class MainMenu
    {
        public static Menu _menu, _combo, _lane, _jungle, _lasthit, _harass, _flee, _misc, _ks, _draw, _baseult;

        public static void Init()
        {
            try
            {
                /*
                    How does it work?

                    Just Call The method in Champion script when you need to use, remember some menus need variables for generating
                 * Test
                */

                //              
                //  Main Menu   
                //              
                _menu = EloBuddy.SDK.Menu.MainMenu.AddMenu("OKTR AIO ", "marks.aio", Player.Instance.ChampionName);
                _menu.AddGroupLabel("One Key To Report AIO - " + Player.Instance.ChampionName);
                _menu.AddLabel("Hope you guys enjoy the ONE KEY TO RAXE AIO!");
                _menu.AddLabel("Doh! is One Key To Report >_< \n Or OneKeyToRape (your enemies) ಠ_ಠ");
                _menu.AddSeparator();
                _menu.AddGroupLabel("Main Settings:");
                _menu.Add("playsound", new CheckBox("Play Welcome Sound"));
                _menu.AddSeparator();
                _menu.AddGroupLabel("Performance Settings:");
                _menu.Add("useonupdate", new CheckBox("Improve PERFORMANCE*", false));
                _menu.AddSeparator();
                _menu.AddLabel("*=Please take care, if your improve your PERFORMANCE the AIO will have less FPS");

                //Todo Improve Menu Design

                //
                //  Combo Menu
                //
                _combo = _menu.AddSubMenu("Combo Menu", "combo");
                _combo.AddGroupLabel("Combo Settings");
                //If wanna to add more function use MainMenu._combo.Add... Call in Champion Script

                //
                //  Lane Clear
                //
                _lane = _menu.AddSubMenu("LaneClear Menu", "lane");

                //
                //  Jungle Clear
                //
                _jungle = _menu.AddSubMenu("JungleClear Menu", "jungle");

                //
                //  Harrass
                //
                _harass = _menu.AddSubMenu("Harass Menu", "harass");
                _harass.AddGroupLabel("Harass Settings");

                //
                //  Flee Menu
                //
                //Use Initiator FleeKeys(bool,bool,bool,bool)

                //
                //  KS
                //
                _ks = _menu.AddSubMenu("Kill Steal", "killsteal");
                _ks.AddGroupLabel("Kill Steal Settings");

                //
                // Misc Menu
                //MiscMenuInit();
                //
                //Call it on Champion Script... need to configurate manually

                //
                //  Draw Menu
                //
                _draw = _menu.AddSubMenu("Drawing Menu", "draw");
                _draw.AddGroupLabel("Drawing Settings");
                _draw.AddSeparator();
                _draw.AddCheckBox("draw.disable", "Disable Drawings", false);
                _draw.AddCheckBox("draw.ready", "Display only ready skills");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Chat.Print(
                    "<font color='#23ADDB'>Marksman AIO:</font><font color='#E81A0C'> an error ocurred. (Code 2)</font>");
            }
            //TODO: Main menu stuff n things
        }

        public static void MiscMenu()
        {
            _misc = _menu.AddSubMenu("Misc Menu", "misc");
            _misc.AddGroupLabel("Misc Settings");
            _misc.Add("misc.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange +=
                Value.AdvancedModeChanged;
        }

        public static void DamageIndicator(bool jungle = false, string draw = "")
        {
            _draw.AddSeparator();
            _draw.AddGroupLabel("Enemy Damage Indicator Settings");
            _draw.AddLabel("Drawing: " + (draw == "" ? "Combo damage" : draw));
            _draw.AddCheckBox("draw.enemyDmg", "Draw damage on enemy healthbar");
            _draw.AddColorItem("draw.color.enemyDmg", 3);

            if (jungle)
            {
                _draw.AddSeparator();
                _draw.AddGroupLabel("Jungle Damage Indicator Settings");
                _draw.AddCheckBox("draw.jungleDmg", "Draw damage on jungle healthbar");
                _draw.AddColorItem("draw.color.jungleDmg", 15);
            }
        }

        public static void DrawKeys(
            bool useQ = true,
            bool defaultQ = true,
            bool useW = true,
            bool defaultW = true,
            bool useE = true,
            bool defaultE = true,
            bool useR = true,
            bool defaultR = true)
        {
            if (useQ)
            {
                _draw.AddSeparator();
                _draw.AddGroupLabel("Q Settings");
                _draw.AddCheckBox("draw.q", "Draw Q", defaultQ);
                _draw.AddColorItem("color.q");
                _draw.AddWidthItem("width.q");
            }
            if (useW)
            {
                _draw.AddSeparator();
                _draw.AddGroupLabel("W Settings");
                _draw.AddCheckBox("draw.w", "Draw W", defaultW);
                _draw.AddColorItem("color.w");
                _draw.AddWidthItem("width.w");
            }
            if (useE)
            {
                _draw.AddSeparator();
                _draw.AddGroupLabel("E Settings");
                _draw.AddCheckBox("draw.e", "Draw E", defaultE);
                _draw.AddColorItem("color.e");
                _draw.AddWidthItem("width.e");
            }
            if (useR)
            {
                _draw.AddSeparator();
                _draw.AddGroupLabel("R Settings");
                _draw.AddCheckBox("draw.r", "Draw R", defaultR);
                _draw.AddColorItem("color.r");
                _draw.AddWidthItem("width.r");
            }
            _draw.Add("draw.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange +=
                Value.AdvancedModeChanged;
        }

        public static void ComboManaManager(bool q, bool w, bool e, bool r, int qmana, int wmana, int emana, int rmana)
        {
            if (q)
            {
                _combo.AddSlider("combo.q.mana", "Use Q if Mana is above {0}%", qmana, 0, 100, true);
            }
            if (w)
            {
                _combo.AddSlider("combo.w.mana", "Use W if Mana is above {0}%", wmana, 0, 100, true);
            }
            if (w)
            {
                _combo.AddSlider("combo.e.mana", "Use E if Mana is above {0}%", emana, 0, 100, true);
            }
            if (r)
            {
                _combo.AddSlider("combo.r.mana", "Use R if Mana is above {0}%", rmana, 0, 100, true);
            }
        }

        public static void LaneManaManager(bool q, bool w, bool e, bool r, int qmana, int wmana, int emana, int rmana)
        {
            if (q)
            {
                _lane.AddSlider("lane.q.mana", "Use Q if Mana is above {0}%", qmana, 0, 100, true);
            }
            if (w)
            {
                _lane.AddSlider("lane.w.mana", "Use W if Mana is above {0}%", wmana, 0, 100, true);
            }
            if (e)
            {
                _lane.AddSlider("lane.e.mana", "Use E if Mana is above {0}%", emana, 0, 100, true);
            }
            if (r)
            {
                _lane.AddSlider("lane.r.mana", "Use R if Mana is above {0}%", rmana, 0, 100, true);
            }
        }

        public static void JungleManaManager(bool q, bool w, bool e, bool r, int qmana, int wmana, int emana, int rmana)
        {
            if (q)
            {
                _jungle.AddSlider("jungle.q.mana", "Use Q if Mana is above {0}%", qmana, 0, 100, true);
            }
            if (w)
            {
                _jungle.AddSlider("jungle.w.mana", "Use W if Mana is above {0}%", wmana, 0, 100, true);
            }
            if (e)
            {
                _jungle.AddSlider("jungle.e.mana", "Use E if Mana is above {0}%", emana, 0, 100, true);
            }
            if (r)
            {
                _jungle.AddSlider("jungle.r.mana", "Use R if Mana is above {0}%", rmana, 0, 100, true);
            }
        }

        public static void LasthitManaManager(
            bool q,
            bool w,
            bool e,
            bool r,
            int qmana,
            int wmana,
            int emana,
            int rmana)
        {
            if (q)
            {
                _lasthit.AddSlider("lasthit.q.mana", "Use Q if Mana is above {0}%", qmana, 0, 100, true);
            }
            if (w)
            {
                _lasthit.AddSlider("lasthit.w.mana", "Use W if Mana is above {0}%", wmana, 0, 100, true);
            }
            if (e)
            {
                _lasthit.AddSlider("lasthit.e.mana", "Use E if Mana is above {0}%", emana, 0, 100, true);
            }
            if (r)
            {
                _lasthit.AddSlider("lasthit.r.mana", "Use R if Mana is above {0}%", rmana, 0, 100, true);
            }
        }

        public static void HarassManaManager(bool q, bool w, bool e, bool r, int qmana, int wmana, int emana, int rmana)
        {
            if (q)
            {
                _harass.AddSlider("harass.q.mana", "Use Q if Mana is aboven {0}%", qmana, 0, 100, true);
            }
            if (w)
            {
                _harass.AddSlider("harass.w.mana", "Use W if Mana is above {0}%", wmana, 0, 100, true);
            }
            if (e)
            {
                _harass.AddSlider("harass.e.mana", "Use E if Mana is above {0}%", emana, 0, 100, true);
            }
            if (r)
            {
                _harass.AddSlider("harass.r.mana", "Use R if Mana is above {0}%", rmana, 0, 100, true);
            }
        }

        public static void FleeManaManager(bool q, bool w, bool e, bool r, int qmana, int wmana, int emana, int rmana)
        {
            if (q)
            {
                _flee.AddSlider("flee.q.mana", "Use Q if Mana is above {0}%", qmana, 0, 100, true);
            }
            if (w)
            {
                _flee.AddSlider("flee.w.mana", "Use W if Mana is above {0}%", wmana, 0, 100, true);
            }
            if (e)
            {
                _flee.AddSlider("flee.e.mana", "Use E if Mana is above {0}%", emana, 0, 100, true);
            }
            if (r)
            {
                _flee.AddSlider("flee.r.mana", "Use R if Mana is above {0}%", rmana, 0, 100, true);
            }
        }

        public static void KsManaManager(bool q, bool w, bool e, bool r, int qmana, int wmana, int emana, int rmana)
        {
            if (q)
            {
                _ks.AddSlider("killsteal.q.mana", "Use Q if Mana is above {0}%", qmana, 0, 100, true);
            }
            if (w)
            {
                _ks.AddSlider("killsteal.w.mana", "Use W if Mana is above {0}%", wmana, 0, 100, true);
            }
            if (e)
            {
                _ks.AddSlider("killsteal.e.mana", "Use E if Mana is above {0}%", emana, 0, 100, true);
            }
            if (r)
            {
                _ks.AddSlider("killsteal.r.mana", "Use R if Mana is above {0}%", rmana, 0, 100, true);
            }
        }

        /// <summary>
        ///     Combo Keys
        /// </summary>
        /// <param name="useQ">Create Q COMBO menu</param>
        /// <param name="defaultQ">Determinate toggle status</param>
        /// <param name="useW">Create W COMBO menu</param>
        /// <param name="defaultW">Determinate toggle status</param>
        /// <param name="useE">Create E COMBO menu</param>
        /// <param name="defaultE">Determinate toggle status</param>
        /// <param name="useR">Create R COMBO menu</param>
        /// <param name="defaultR">Determinate toggle status</param>
        public static void ComboKeys(
            bool useQ = true,
            bool defaultQ = true,
            bool useW = true,
            bool defaultW = true,
            bool useE = true,
            bool defaultE = true,
            bool useR = true,
            bool defaultR = true)
        {
            if (useQ)
            {
                _combo.AddCheckBox("combo.q", "Use Q", defaultQ);
            }
            if (useW)
            {
                _combo.AddCheckBox("combo.w", "Use W", defaultW);
            }
            if (useE)
            {
                _combo.AddCheckBox("combo.e", "Use E", defaultE);
            }
            if (useR)
            {
                _combo.AddCheckBox("combo.r", "Use R", defaultR);
            }
            _combo.AddSeparator();
            _combo.Add("combo.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange +=
                Value.AdvancedModeChanged;
        }

        /// <summary>
        ///     KS Keys
        /// </summary>
        /// <param name="useQ">Create Q KS menu</param>
        /// <param name="defaultQ">Determinate toggle status</param>
        /// <param name="useW">Create W KS menu</param>
        /// <param name="defaultW">Determinate toggle status</param>
        /// <param name="useE">Create E KS menu</param>
        /// <param name="defaultE">Determinate toggle status</param>
        /// <param name="useR">Create R KS menu</param>
        /// <param name="defaultR">Determinate toggle status</param>
        public static void KsKeys(
            bool useQ = true,
            bool defaultQ = true,
            bool useW = true,
            bool defaultW = true,
            bool useE = true,
            bool defaultE = true,
            bool useR = true,
            bool defaultR = true)
        {
            if (useQ)
            {
                _ks.AddCheckBox("killsteal.q", "Use Q", defaultQ);
            }
            if (useW)
            {
                _ks.AddCheckBox("killsteal.w", "Use W", defaultW);
            }
            if (useE)
            {
                _ks.AddCheckBox("killsteal.e", "Use E", defaultE);
            }
            if (useR)
            {
                _ks.AddCheckBox("killsteal.r", "Use R", defaultR);
            }
            _ks.AddSeparator();
            _ks.Add("killsteal.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange +=
                Value.AdvancedModeChanged;
        }

        /// <summary>
        ///     Lane Clear Keys
        /// </summary>
        /// <param name="useQ">Create Q LC menu</param>
        /// <param name="defaultQ">Determinate toggle status</param>
        /// <param name="useW">Create W LC menu</param>
        /// <param name="defaultW">Determinate toggle status</param>
        /// <param name="useE">Create E LC menu</param>
        /// <param name="defaultE">Determinate toggle status</param>
        /// <param name="useR">Create R LC menu</param>
        /// <param name="defaultR">Determinate toggle status</param>
        public static void LaneKeys(
            bool useQ = true,
            bool defaultQ = true,
            bool useW = true,
            bool defaultW = true,
            bool useE = true,
            bool defaultE = true,
            bool useR = true,
            bool defaultR = true)
        {
            _lane.AddGroupLabel("LaneClear Settings");
            if (useQ)
            {
                _lane.AddCheckBox("lane.q", "Use Q", false);
            }
            if (useW)
            {
                _lane.AddCheckBox("lane.w", "Use W", false);
            }
            if (useE)
            {
                _lane.AddCheckBox("lane.e", "Use E", false);
            }
            if (useR)
            {
                _lane.AddCheckBox("lane.r", "Use R", false);
            }
            _lane.AddSeparator();
            _lane.Add("lane.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange +=
                Value.AdvancedModeChanged;
        }

        /// <summary>
        ///     Jungle Clear Keys
        /// </summary>
        /// <param name="useQ">Create Q JC menu</param>
        /// <param name="defaultQ">Determinate toggle status</param>
        /// <param name="useW">Create W JC menu</param>
        /// <param name="defaultW">Determinate toggle status</param>
        /// <param name="useE">Create E JC menu</param>
        /// <param name="defaultE">Determinate toggle status</param>
        /// <param name="useR">Create R JC menu</param>
        /// <param name="defaultR">Determinate toggle status</param>
        /// <param name="junglesteal">Determinate toggle status for JungleSteal</param>
        public static void JungleKeys(
            bool useQ = true,
            bool defaultQ = true,
            bool useW = true,
            bool defaultW = true,
            bool useE = true,
            bool defaultE = true,
            bool useR = true,
            bool defaultR = true,
            bool junglesteal = false)
        {
            _jungle.AddGroupLabel("JungleClear Settings");
            if (useQ)
            {
                _jungle.AddCheckBox("jungle.q", "Use Q", false);
            }
            if (useW)
            {
                _jungle.AddCheckBox("jungle.w", "Use W", false);
            }
            if (useE)
            {
                _jungle.AddCheckBox("jungle.e", "Use E", false);
            }
            if (useR)
            {
                _jungle.AddCheckBox("jungle.r", "Use R", false);
            }
            _jungle.AddSeparator();
            _jungle.Add("jungle.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange +=
                Value.AdvancedModeChanged;
            if (junglesteal)
            {
                JungleSteal();
            }
        }

        private static void JungleSteal()
        {
            _jungle.AddSeparator();
            _jungle.AddGroupLabel("Jungle Steal Settings", "jungle.grouplabel", true);
            _jungle.AddCheckBox("jungle.stealenabled", "Enable Jungle Steal", true, true);
            if (Game.MapId == GameMapId.SummonersRift)
            {
                _jungle.AddLabel("Epics", 25, "jungle.label", true);
                _jungle.AddCheckBox("jungle.SRU_Baron", "Baron", true, true);
                _jungle.AddCheckBox("jungle.SRU_Dragon", "Dragon", true, true);
                _jungle.AddLabel("Buffs", 25, "jungle.label.1", true);
                _jungle.AddCheckBox("jungle.SRU_Blue", "Blue", false, true);
                _jungle.AddCheckBox("jungle.SRU_Red", "Red", false, true);
                _jungle.AddLabel("Small Camps", 25, "jungle.label.2", true);
                _jungle.AddCheckBox("jungle.SRU_Gromp", "Gromp", false, true);
                _jungle.AddCheckBox("jungle.SRU_Murkwolf", "Murkwolf", false, true);
                _jungle.AddCheckBox("jungle.SRU_Krug", "Krug", false, true);
                _jungle.AddCheckBox("jungle.SRU_Razorbeak", "Razerbeak", false, true);
                _jungle.AddCheckBox("jungle.Sru_Crab", "Skuttles", false, true);
            }

            if (Game.MapId == GameMapId.TwistedTreeline)
            {
                _jungle.AddLabel("Epics", 25, "jungle.label.3", true);
                _jungle.AddCheckBox("TT_Spiderboss8.1", "Vilemaw", true, true);
                _jungle.AddLabel("Camps", 25, "jungle.label.4", true);
                _jungle.AddCheckBox("TT_NWraith1.1", "Wraith", false, true);
                _jungle.AddCheckBox("TT_NWraith4.1", "Wraith", false, true);
                _jungle.AddCheckBox("TT_NGolem2.1", "Golem", false, true);
                _jungle.AddCheckBox("TT_NGolem5.1", "Golem", false, true);
                _jungle.AddCheckBox("TT_NWolf3.1", "Wolf", false, true);
                _jungle.AddCheckBox("TT_NWolf6.1", "Wolf", false, true);
            }
        }

        /// <summary>
        ///     Last Hit Keys
        /// </summary>
        /// <param name="useQ">Create Q LH menu</param>
        /// <param name="defaultQ">Determinate toggle status</param>
        /// <param name="useW">Create W LH menu</param>
        /// <param name="defaultW">Determinate toggle status</param>
        /// <param name="useE">Create E LH menu</param>
        /// <param name="defaultE">Determinate toggle status</param>
        /// <param name="useR">Create R LH menu</param>
        /// <param name="defaultR">Determinate toggle status</param>
        public static void LastHitKeys(
            bool useQ = true,
            bool defaultQ = true,
            bool useW = true,
            bool defaultW = true,
            bool useE = true,
            bool defaultE = true,
            bool useR = true,
            bool defaultR = true)
        {
            //
            //  LastHit
            //
            _lasthit = _menu.AddSubMenu("LastHit Menu", "lasthit");
            _lasthit.AddGroupLabel("Last Hit Settings");
            if (useQ)
            {
                _lasthit.AddCheckBox("lasthit.q", "Use Q", false);
            }
            if (useW)
            {
                _lasthit.AddCheckBox("lasthit.w", "Use W", false);
            }
            if (useE)
            {
                _lasthit.AddCheckBox("lasthit.e", "Use E", false);
            }
            if (useR)
            {
                _lasthit.AddCheckBox("lasthit.r", "Use R", false);
            }
            _lasthit.AddSeparator();
            _lasthit.Add("lasthit.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange +=
                Value.AdvancedModeChanged;
        }

        /// <summary>
        ///     Harass Keys
        /// </summary>
        /// <param name="useQ">Create Q Harass menu</param>
        /// <param name="defaultQ">Determinate toggle status</param>
        /// <param name="useW">Create W Harass menu</param>
        /// <param name="defaultW">Determinate toggle status</param>
        /// <param name="useE">Create E Harass menu</param>
        /// <param name="defaultE">Determinate toggle status</param>
        /// <param name="useR">Create R Harass menu</param>
        /// <param name="defaultR">Determinate toggle status</param>
        public static void HarassKeys(
            bool useQ = true,
            bool defaultQ = true,
            bool useW = true,
            bool defaultW = true,
            bool useE = true,
            bool defaultE = true,
            bool useR = true,
            bool defaultR = true)
        {
            if (useQ)
            {
                _harass.AddCheckBox("harass.q", "Use Q");
            }
            if (useW)
            {
                _harass.AddCheckBox("harass.w", "Use W");
            }
            if (useE)
            {
                _harass.AddCheckBox("harass.e", "Use E");
            }
            if (useR)
            {
                _harass.AddCheckBox("harass.r", "Use R", false);
            }
            _harass.AddSeparator();
            _harass.Add("harass.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange +=
                Value.AdvancedModeChanged;
        }

        public static void FleeKeys(
            bool useQ = true,
            bool defaultQ = true,
            bool useW = true,
            bool defaultW = true,
            bool useE = true,
            bool defaultE = true,
            bool useR = true,
            bool defaultR = true)
        {
            _flee = _menu.AddSubMenu("Flee Menu", "flee");
            _flee.AddGroupLabel("Flee Settings");
            if (useQ)
            {
                _flee.AddCheckBox("flee.q", "Use Q");
            }
            if (useW)
            {
                _flee.AddCheckBox("flee.w", "Use W");
            }
            if (useE)
            {
                _flee.AddCheckBox("flee.e", "Use E");
            }
            if (useR)
            {
                _flee.AddCheckBox("flee.r", "Use R ", false);
            }
            _flee.AddSeparator();
            _flee.Add("flee.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange +=
                Value.AdvancedModeChanged;
        }
    }
}