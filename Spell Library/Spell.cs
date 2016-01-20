namespace OKTRAIO.Spell_Library
{
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
            this.CharName = charName;
            this.SpellKey = spellKey;
            this.Damage = damage;
        }

        public string charName { get; set; }
        public string spellKey { get; set; }
        public float damage { get; set; }
    }
    
}
