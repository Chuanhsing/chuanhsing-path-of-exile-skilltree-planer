using System.Collections.Generic;
using POESKillTree.DPS;

namespace POESKillTree.Stats
{
    public enum ItemType
    {
        MainHand,
        OffHand,
        Helm,
        Armor,
        Gloves,
        Boots,
        Belt,
        Amulet,
        Ring1,
        Ring2,
        Shield
    }

    public class Item
    {
        public ItemType type { get; private set; }
        public List< StatModifier > modifiers { get; private set; }
        public List< StatModifier > localModifiers { get; private set; }

        public Item( ItemType type )
        {
            this.type = type;
            modifiers = new List< StatModifier >();
            localModifiers = new List< StatModifier >();
        }
    }

    public enum WeaponType
    {
        Axe,
        Bow,
        Claw,
        Dagger,
        Mace,
        Staff,
        Sword,
        Wand
    }

    public class Weapon : Item
    {
        public WeaponType weaponType { get; set; }
        public bool isTwoHanded { get; set; }
        public List< DamageSource > damageSources { get; private set; }
        public float criticalStrikeChance { get; set; }
        public float attackSpeed { get; set; }

        public Weapon( ItemType type )
            : base( type )
        {
            damageSources = new List< DamageSource >();
        }
    }
}