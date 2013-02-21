using System;
using System.Collections.Generic;

namespace POESKillTree.Stats
{
    public class Character
    {
        public Character( CharacterBaseConfig config )
        {
            m_attributes = new Dictionary<StatType, Attribute>( Enum.GetNames( typeof(StatType) ).Length );
            m_items = new Dictionary<ItemType, Item>( Enum.GetNames( typeof(ItemType) ).Length );
            m_keystones = Keystone.None;

            initBaseAttributes( config );
        }

        public void addStatModifier( StatModifier modifier )
        {
            if (modifier.keystone != Keystone.None)
            {
                m_keystones |= modifier.keystone;
            }
            else
            {
                m_attributes[ modifier.type ].addModifier( modifier.modifier );
            }
        }

        public void resetStatModifiers()
        {
            m_keystones = Keystone.None;

            foreach (KeyValuePair<StatType, Attribute> attribute in m_attributes)
            {
                attribute.Value.reset();
            }
        }

        public void addItem( Item item )
        {
            m_items[ item.type ] = item;
        }

        // --------------------------------------------------------------------
        
        private void initBaseAttributes( CharacterBaseConfig charBaseConfig )
        {
            foreach (StatType value in Enum.GetValues( typeof(StatType) ))
            {
                m_attributes.Add( value, new Attribute() );
            }

            m_attributes[ StatType.Strength ].baseValue = charBaseConfig.strength;
            m_attributes[ StatType.Intelligence ].baseValue = charBaseConfig.intelligence;
            m_attributes[ StatType.Dexterity ].baseValue = charBaseConfig.dexterity;
        }

        // --------------------------------------------------------------------

        private readonly Dictionary<StatType, Attribute> m_attributes;
        private readonly Dictionary<ItemType, Item> m_items;
        private Keystone m_keystones;
    }
}
