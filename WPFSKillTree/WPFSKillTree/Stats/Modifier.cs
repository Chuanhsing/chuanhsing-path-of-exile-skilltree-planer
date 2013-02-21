using System;

namespace POESKillTree.Stats
{
    public enum ModifierType
    {
        Increase,
        Reduce,
        More,
        Less,
        FlatIncrease
    }

    public class Modifier
    {
        public ModifierType type { get; private set; }
        public float additive { get; private set; }
        public float multiplicative { get; private set; }

        // --------------------------------------------------------------------

        public Modifier( ModifierType type, float inputValue )
        {
            this.type = type;
            float value = inputValue / 100.0f;

            switch (type)
            {
                case ModifierType.Increase:
                    additive = value;
                    break;
                case ModifierType.Reduce:
                    additive = -value;
                    break;
                case ModifierType.More:
                    multiplicative = 1.0f + value;
                    break;
                case ModifierType.Less:
                    multiplicative = 1.0f - Math.Min(value, 1.0f);
                    break;
                case ModifierType.FlatIncrease:
                    additive = inputValue;
                    break;
            }
        }
    }

    public class StatModifier
    {
        public StatType type { get; private set; }
        public Modifier modifier { get; private set; }
        public Keystone keystone { get; private set; }

        public StatModifier( StatType type, ModifierType modType, float valuePercentage )
        {
            this.type = type;
            modifier = new Modifier( modType, valuePercentage );
            keystone = Keystone.None;
        }

        public StatModifier( Keystone keystone )
        {
            type = StatType.Keystone;
            modifier = null;
            this.keystone = keystone;
        }
    }
}