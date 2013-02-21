namespace POESKillTree.Stats
{
    public enum CharacterType
    {
        Marauder,
        Ranger,
        Witch,
        Duelist,
        Templar,
        Shadow
    }

    public class CharacterBaseConfig
    {
        public CharacterType type { get; private set; }
        public uint strength { get; private set; }
        public uint intelligence { get; private set; }
        public uint dexterity { get; private set; }

        public CharacterBaseConfig( CharacterType type )
        {
            this.type = type;
            strength = 14;
            intelligence = 14;
            dexterity = 14;

            switch (type)
            {
                case CharacterType.Marauder:
                    strength = 30;
                    break;
                case CharacterType.Ranger:
                    dexterity = 30;
                    break;
                case CharacterType.Witch:
                    intelligence = 30;
                    break;
                case CharacterType.Duelist:
                    strength = 22;
                    dexterity = 22;
                    break;
                case CharacterType.Templar:
                    strength = 22;
                    intelligence = 22;
                    break;
                case CharacterType.Shadow:
                    intelligence = 22;
                    dexterity = 22;
                    break;
            }
        }
    }
}
