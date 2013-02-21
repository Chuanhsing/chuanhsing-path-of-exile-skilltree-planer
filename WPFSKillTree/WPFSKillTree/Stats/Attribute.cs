using System;
using System.Collections.Generic;

namespace POESKillTree.Stats
{
    class Attribute
    {
        public uint baseValue
        {
            get { return m_baseValue; }
            set
            {
                m_baseValue = value; 
                updateValue();
            }
        }

        public uint modifiedValue { get; private set; }

        // --------------------------------------------------------------------

        public Attribute()
        {
            m_modifiers = new List<Modifier>();
        }

        public void addModifier( Modifier modifier )
        {
            m_modifiers.Add( modifier );
            updateValue();
        }

        public void reset()
        {
            m_modifiers.Clear();
            updateValue();
        }

        private void updateValue()
        {
            float additive = 0;
            float multiplicative = 1;

            foreach (var modifier in m_modifiers)
            {
                additive += modifier.additive;
                multiplicative *= modifier.multiplicative;
            }

            modifiedValue = (uint) Math.Round( baseValue * (1.0f + additive) * multiplicative );
        }

        // --------------------------------------------------------------------

        private uint m_baseValue;
        private readonly List<Modifier> m_modifiers;
    }
}
