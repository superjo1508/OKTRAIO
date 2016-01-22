using EloBuddy;
using EloBuddy.SDK.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using OKTRAIO.Utility;
using EloBuddy.SDK;

namespace OKTRAIO.Database.Spell_Library
{
    #region Spelldamage values

    public class SpellDb
    {
        public string CharName;
        public string SpellKey;
        public float Damage;

        public SpellDb()
        {

        }

        public SpellDb(
            string charName,
            string spellKey,
            float damage
            )
        {
            CharName = charName;
            SpellKey = spellKey;
            Damage = damage;
        }

        public string charName { get; set; }
        public string spellKey { get; set; }
        public float damage { get; set; }
    }

    #endregion

    #region SkillShot values

    public class Spell
    {
        public enum CollisionObjectTypes
        {
            Minion,
            Champions,
            YasuoWall,
        }

        public enum InterruptableDangerLevel
        {
            Low,
            Medium,
            High,
        }

        public Spell.CollisionObjectTypes[] CollisionObjects = { };
        public bool AddHitbox;
        public bool CanBeRemoved = false;
        public bool Centered;
        public string ChampionName;
        public int DangerValue;
        public int Delay;
        public bool DisabledByDefault = false;
        public bool DisableFowDetection = false;
        public bool DontAddExtraDuration;
        public bool DontCheckForDuplicates = false;
        public bool DontCross = false;
        public bool DontRemove = false;
        public int ExtraDuration;
        public string[] ExtraMissileNames = { };
        public int ExtraRange = -1;
        public string[] ExtraSpellNames = { };
        public bool FixedRange;
        public bool ForceRemove = false;
        public bool FollowCaster = false;
        public string FromObject = "";
        public string[] FromObjects = { };
        public int Id = -1;
        public bool Invert;
        public bool IsDangerous = false;
        public int MissileAccel = 0;
        public bool MissileDelayed;
        public bool MissileFollowsUnit;
        public int MissileMaxSpeed;
        public int MissileMinSpeed;
        public int MissileSpeed;
        public string MissileSpellName = "";
        public float MultipleAngle;
        public int MultipleNumber = -1;
        public int RingRadius;
        public SpellSlot Slot;
        public string SpellName;
        public bool TakeClosestPath = false;
        public string ToggleParticleName = "";
        public SkillShotType Type;
        private int _radius;
        private int _range;
        public string BuffName;
        public Spell.InterruptableDangerLevel DangerLevel;
        public bool IsInterruptableSpell;

        public Spell()
        {
        }

        public Spell(string championName,
            string spellName,
            SpellSlot slot,
            SkillShotType type,
            int delay,
            int range,
            int radius,
            int missileSpeed,
            bool addHitbox,
            bool fixedRange,
            int defaultDangerValue)
        {
            ChampionName = championName;
            SpellName = spellName;
            Slot = slot;
            Type = type;
            Delay = delay;
            Range = range;
            _radius = radius;
            MissileSpeed = missileSpeed;
            AddHitbox = addHitbox;
            FixedRange = fixedRange;
            DangerValue = defaultDangerValue;
        }

        public string MenuItemName
        {
            get { return ChampionName + " - " + SpellName; }
        }

        public int Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        public int RawRadius
        {
            get { return _radius; }
        }

        public int RawRange
        {
            get { return _range; }
        }

        public int Range
        {
            get { return _range; }
            set { _range = value; }
        }

        public bool Collisionable
        {
            get
            {
                for (int i = 0; i < CollisionObjects.Length; i++)
                {
                    if (CollisionObjects[i] == Spell.CollisionObjectTypes.Champions ||
                        CollisionObjects[i] == Spell.CollisionObjectTypes.Minion)
                        return true;
                }
                return false;
            }
        }
    }

    internal class InterrupterExtensions
    {
        private List<Spell> Spells = new List<Spell>();

        public bool IsChannelingImportantSpell(AIHeroClient unit)
        {
            return
                Spells.Any(
                    spell =>
                        spell.ChampionName == unit.ChampionName &&
                        spell.IsInterruptableSpell == true &&
                        ((unit.LastCastedspell() != null &&
                            String.Equals(
                                unit.LastCastedspell().Name, spell.SpellName, StringComparison.CurrentCultureIgnoreCase) &&
                            Core.GameTickCount - unit.LastCastedSpellT() < 350 + spell.ExtraDuration) ||
                        (spell.BuffName != null && unit.HasBuff(spell.BuffName)) ||
                        (unit.IsMe &&
                            LastCastedSpell.LastCastPacketSent != null &&
                            LastCastedSpell.LastCastPacketSent.Slot == spell.Slot &&
                            Core.GameTickCount - LastCastedSpell.LastCastPacketSent.Tick < 150 + Game.Ping)));
        }
    }
    #endregion
}
