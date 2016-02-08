using System;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Utils;
using OKTRAIO.Database.Icons;
using OKTRAIO.Menu_Settings;
using OKTRAIO.Properties;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Line = EloBuddy.SDK.Rendering.Line;
using MainMenu = EloBuddy.SDK.Menu.MainMenu;
using Sprite = EloBuddy.SDK.Rendering.Sprite;

namespace OKTRAIO.Utility.Tracker
{
    public class Tracker : UtilityAddon
    {
        public Dictionary<AIHeroClient, SpellAvaliblity> HeroSpellAvaliblitys { get; set; }
        private Dictionary<AIHeroClient, SpellSprites> HeroSpellSprites { get; set; }
        private Sprite TrackerHud { get; set; } 
        private class SpellSprites
        {
            public readonly Dictionary<SpellSlot, Sprite> CooldownSprites;
            public readonly Dictionary<SpellSlot, Sprite> AvailableSprites;
            

            public SpellSprites()
            {
                CooldownSprites = new Dictionary<SpellSlot, Sprite>();
                AvailableSprites = new Dictionary<SpellSlot, Sprite>();
            }
            public void AddSpell(SpellSlot slot, Sprite onCooldown, Sprite available)
            {
                CooldownSprites[slot] = onCooldown;
                AvailableSprites[slot] = available;
            }
        }

        public Tracker(Menu menu) : base(menu)
        {

        }

        public override UtilityInfo GetUtilityInfo()
        {
            return new UtilityInfo(this, "Tracker", "tracker", "coman3");
        }

        protected override void InitializeMenu()
        {
            Menu.AddGroupLabel("OKTR AIO - Tracker", "tracker.grouplabel.utilitymenu");
            Menu.AddLabel("Developed By: Coman3");
            Menu.AddCheckBox("tracker.enable", "Enable", false);
            Menu.AddLabel("Please note that the tracker is still not complete but is working.");
            Menu.AddCheckBox("tracker.show.spells.summoner", "Show Summoner Spells");
            Menu.AddCheckBox("tracker.show.spells.normal", "Show Normal Spells");
            Menu.AddCheckBox("tracker.show.player.allies", "Track Allies");
            Menu.AddCheckBox("tracker.show.player.enemies", "Track Enemies");
            Menu.AddLabel("Compact mode, turns the tracker into a slim lined bar under the players health.\n Can be useful as it will show cooldowns. ");
            Menu.AddCheckBox("tracker.visual.compact", "Compact Mode (Not Implemented)", false);
            Menu.Add("tracker.reload", new CheckBox("Reload Tracker", false)).OnValueChange += Reload;
            Menu.AddSeparator();
            Menu.Add("tracker.advanced", new CheckBox("Show Advanced Menu", false)).OnValueChange += Value.AdvancedModeChanged;
            Menu.AddSlider("tracker.visual.offset.x", "X Offset", 1, -100, 100, true);
            Menu.AddSlider("tracker.visual.offset.y", "Y Offset", 20, -100, 100, true);
        }

        public override void Initialize()
        {
            if (!Value.Use("tracker.enable"))
            {
                Logger.Warn("Tracker Disabled!");
                return;
            }
            HeroSpellAvaliblitys = new Dictionary<AIHeroClient, SpellAvaliblity>(EntityManager.Heroes.AllHeroes.Count);
            HeroSpellSprites = new Dictionary<AIHeroClient, SpellSprites>();
            TrackerHud = new Sprite(TextureLoader.BitmapToTexture(Resources.SpellLayout));
            using (new TimeMeasure("Tracker Sprite Generation"))
            {
                foreach (var hero in EntityManager.Heroes.AllHeroes)
                {
                    var spellAvaliblity = new SpellAvaliblity(hero);
                    HeroSpellAvaliblitys[hero] = spellAvaliblity;
                    //var spellSprites = new SpellSprites();
                    //foreach (SpellSlot slot in SpellAvaliblity.TrackedSpellSlots)
                    //{
                    //    var spell = spellAvaliblity.GetSpell(slot);
                    //    spellSprites.AddSpell(slot,
                    //        IconManager.GetSpellSprite(spell, IconGenerator.IconType.Square, Value.Get("tracker.visual.size"), Color.Red, Value.Get("tracker.visual.border.width")),
                    //        IconManager.GetSpellSprite(spell, IconGenerator.IconType.Square, Value.Get("tracker.visual.size"), Color.Green, Value.Get("tracker.visual.border.width")));

                    //}
                    //HeroSpellSprites[hero] = spellSprites;
                }
            }
        }

        public void Reload(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            if (!args.OldValue && args.NewValue)
            {
                Chat.Print("Reloaded Tracker!", Color.Green);
                
                sender.CurrentValue = false;
            }

        }
        protected override void Drawing_OnEndScene(EventArgs args)
        {

            //PLEASE IGNORE HOW SHIT THIS CODE IS I AM TIRED AND WANT TO GO TO SLEEP! I WILL FIX IT WHEN I GET TO IT
            if (!Value.Use("tracker.enable")) return;
            if(MainMenu.IsOpen) return;
            if(Shop.IsOpen) return;
            foreach (var avaliblity in HeroSpellAvaliblitys)
            {
                if(!avaliblity.Key.IsHPBarRendered) return;
                if ((!avaliblity.Key.IsEnemy || !Value.Use("tracker.show.player.enemies")) && (!avaliblity.Key.IsAlly || !Value.Use("tracker.show.player.allies"))) continue; // || avaliblity.Key.IsMe) continue;
                var drawPos = new Vector2(avaliblity.Key.HPBarPosition.X + avaliblity.Key.HPBarXOffset + Value.Get("tracker.visual.offset.x"), avaliblity.Key.HPBarPosition.Y + avaliblity.Key.HPBarYOffset + Value.Get("tracker.visual.offset.y"));
                var spells = avaliblity.Value;
                var lineWidth = 112 /4;
                var startPos = new Vector2(drawPos.X + 3, drawPos.Y + 10);
                for (int slotIndex = 0; slotIndex < SpellAvaliblity.TrackedSpellSlots.Length; slotIndex++)
                {
                    var slot = SpellAvaliblity.TrackedSpellSlots[slotIndex];
                    if (slot == SpellSlot.Summoner1 || slot == SpellSlot.Summoner2) continue;
                    Line.DrawLine(spells.IsAvailable(slot) ? Color.Green : Color.Red, 4, new Vector2(startPos.X + lineWidth * slotIndex, startPos.Y),
                        new Vector2(startPos.X + lineWidth * slotIndex + lineWidth * spells.CoolDownPercent(slot), startPos.Y));

                }
                TrackerHud.Draw(drawPos);
            }

        }
        
    }
}