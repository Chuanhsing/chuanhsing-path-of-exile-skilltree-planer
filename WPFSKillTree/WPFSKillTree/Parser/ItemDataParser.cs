using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using POESKillTree.DPS;
using POESKillTree.Stats;
using Raven.Json.Linq;

namespace POESKillTree.Parser
{
    public class ItemDataParser : ParserBase
    {
        public List<Item> parseItems( string itemData )
        {
            List<Item> result = new List<Item>();

            RavenJObject jObject = RavenJObject.Parse( itemData );
            foreach (RavenJObject jobj in (RavenJArray) jObject[ "items" ])
            {
                string id = jobj[ "inventoryId" ].ToString();

                switch (id)
                {
                    case "BodyArmour":
                        result.Add( parseItem( ItemType.Armor, jobj ) );
                        break;
                    case "Ring":
                        result.Add( parseItem( ItemType.Ring1, jobj ) );
                        break;
                    case "Ring2":
                        result.Add( parseItem( ItemType.Ring2, jobj ) );
                        break;
                    case "Gloves":
                        result.Add( parseItem( ItemType.Gloves, jobj ) );
                        break;
                    case "Weapon":
                        result.Add( parseItem( ItemType.MainHand, jobj ) );
                        break;
                    case "Offhand":
                        result.Add( parseItem( ItemType.OffHand, jobj ) );
                        break;
                    case "Helm":
                        result.Add( parseItem( ItemType.Helm, jobj ) );
                        break;
                    case "Boots":
                        result.Add( parseItem( ItemType.Boots, jobj ) );
                        break;
                    case "Amulet":
                        result.Add( parseItem( ItemType.Amulet, jobj ) );
                        break;
                    case "Belt":
                        result.Add( parseItem( ItemType.Belt, jobj ) );
                        break;
                }
            }

            return result;
        }

        private Item parseItem( ItemType itemType, RavenJObject jobj )
        {
            Item result;

            if (itemType == ItemType.MainHand || itemType == ItemType.OffHand)
            {
                // :TODO: if OffHand, test if it was really a weapon!
                Weapon weapon = new Weapon( itemType );
                parseWeaponProperties( weapon, jobj["properties"] );
                result = weapon;
            }
            else
            {
                result = new Item( itemType );
            }

            if (jobj.ContainsKey( "implicitMods" ))
            {
                addMods( result, jobj[ "implicitMods" ] );
            }

            if (jobj.ContainsKey( "explicitMods" ))
            {
                addMods( result, jobj[ "explicitMods" ] );
            }

            return result;
        }

        private void addMods( Item item, RavenJToken token )
        {
            List<StatModifier> modifiers = new List<StatModifier>();
            foreach (string mod in token.Values<string>())
            {
                modifiers.Clear();

                Keystone keystone = parseKeystone( mod );
                if (keystone != Keystone.None)
                {
                    item.modifiers.Add( new StatModifier( keystone ) );
                    continue;
                }
                if (parseModifier( modifiers, mod ))
                {
                    item.modifiers.AddRange( modifiers );
                }
            }

            postProcessModifiers( item );
        }

        private static void postProcessModifiers( Item item )
        {
            for (int i = 0; i < item.modifiers.Count; i++)
            {
                StatModifier modifier = item.modifiers[ i ];

                if (modifier.type == StatType.None)
                {
                    item.modifiers.RemoveAt( i );
                    --i;
                    continue;
                }

                switch (item.type)
                {
                    case ItemType.Armor:
                    case ItemType.Gloves:
                    case ItemType.Boots:
                        {
                            switch (modifier.type)
                            {
                                case StatType.Armor:
                                case StatType.EvasionRating:
                                case StatType.EvasionAndArmorRating:
                                case StatType.MaxEnergyShield:
                                    item.modifiers.RemoveAt( i );
                                    --i;
                                    break;
                            }
                        }
                        break;

                    case ItemType.Helm:
                        {
                            switch (modifier.type)
                            {
                                case StatType.Armor:
                                case StatType.EvasionRating:
                                case StatType.EvasionAndArmorRating:
                                case StatType.MaxEnergyShield:
                                    item.modifiers.RemoveAt( i );
                                    --i;
                                    break;

                                case StatType.MinionGemLevel:
                                    item.modifiers.RemoveAt( i );
                                    item.localModifiers.Add( modifier );
                                    --i;
                                    break;
                            }
                        }
                        break;

                    case ItemType.MainHand:
                    case ItemType.OffHand:
                        {
                            switch (modifier.type)
                            {
                                case StatType.AddedColdMaxDmg:
                                case StatType.AddedColdMinDmg:
                                case StatType.AddedFireMaxDmg:
                                case StatType.AddedFireMinDmg:
                                case StatType.AddedLightningMaxDmg:
                                case StatType.AddedLightningMinDmg:
                                case StatType.AddedPhysicalMaxDmg:
                                case StatType.AddedPhysicalMinDmg:
                                case StatType.AttackSpeed:
                                case StatType.PhysicalDamage:
                                case StatType.CriticalStrikeChance:
                                    item.modifiers.RemoveAt( i );
                                    --i;
                                    break;

                                case StatType.AccuracyRating:
                                case StatType.BowGemLevel:
                                case StatType.ColdGemLevel:
                                case StatType.FireGemLevel:
                                case StatType.GemLevel:
                                case StatType.LightningGemLevel:
                                case StatType.MeleeGemLevel:
                                    item.modifiers.RemoveAt( i );
                                    item.localModifiers.Add( modifier );
                                    --i;
                                    break;
                            }
                        }
                        break;

                    case ItemType.Shield:
                        {
                            switch (modifier.type)
                            {
                                case StatType.Armor:
                                case StatType.EvasionRating:
                                case StatType.EvasionAndArmorRating:
                                case StatType.ShieldBlockChance:
                                    item.modifiers.RemoveAt( i );
                                    --i;
                                    break;

                                case StatType.ColdGemLevel:
                                case StatType.FireGemLevel:
                                case StatType.GemLevel:
                                case StatType.LightningGemLevel:
                                case StatType.MeleeGemLevel:
                                    item.modifiers.RemoveAt( i );
                                    item.localModifiers.Add( modifier );
                                    --i;
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        private static void parseWeaponProperties( Weapon weapon, RavenJToken token )
        {
            foreach (RavenJObject prop in (RavenJArray)token)
            {
                string propName = prop[ "name" ].Value< string >();
                RavenJArray propValues = (RavenJArray) prop[ "values" ];

                if (propValues.Length == 0)
                {
                    // This is the type
                    WeaponType type;
                    if (Enum.TryParse( propName, true, out type ))
                    {
                        weapon.weaponType = type;
                        weapon.isTwoHanded = type == WeaponType.Bow || type == WeaponType.Staff;
                    }
                    else
                    {
                        Match match = WeaponTypeRegex.Match(propName);
                        if (match.Success && match.Groups.Count == 3)
                        {
                            weapon.isTwoHanded = match.Groups[ 1 ].Value == "Two";

                            if (Enum.TryParse( match.Groups[2].Value, true, out type ))
                            {
                                weapon.weaponType = type;
                            }
                        }
                    }
                    continue;
                }

                RavenJArray firstProp = (RavenJArray) propValues[ 0 ];

                DamageSource source;
                switch (propName)
                {
                    case "Physical Damage":
                        if (parseDamageSource( out source, firstProp ))
                        {
                            weapon.damageSources.Add( source );
                        }
                        break;
                    case "Elemental Damage":
                        foreach (RavenJToken dmgToken in propValues)
                        {
                            if (parseDamageSource( out source, dmgToken ))
                            {
                                weapon.damageSources.Add( source );
                            }
                        }
                        break;
                    case "Critical Strike Chance":
                        {
                            string chanceValue = firstProp[ 0 ].Value< string >();
                            Match match = Percentage.Match( chanceValue );
                            if (match.Success && match.Groups.Count == 2)
                            {
                                float chance;
                                if (float.TryParse( match.Groups[1].Value, out chance ))
                                {
                                    weapon.criticalStrikeChance = chance;
                                }
                            }
                        }
                        break;
                    case "Attacks per Second":
                        {
                            string apsValue = firstProp[ 0 ].Value< string >();
                            float aps;
                            if (float.TryParse( apsValue, out aps ))
                            {
                                weapon.attackSpeed = aps;
                            }
                        }
                        break;
                }
            }
        }

        private static bool parseDamageSource( out DamageSource source, RavenJToken propValue )
        {
            source = new DamageSource();

            RavenJArray innerValues = (RavenJArray) propValue;
            if (innerValues.Length != 2)
            {
                return false;
            }

            try
            {
                string damageValues = innerValues[0].Value<string>();
                uint damageType = innerValues[1].Value<uint>();

                Match match = Damage.Match( damageValues );
                if (match.Success && match.Groups.Count == 3)
                {
                    source.min = uint.Parse( match.Groups[ 1 ].Value );
                    source.max = uint.Parse( match.Groups[ 2 ].Value );
                }

                switch (damageType)
                {
                    case 0:
                        source.damageType = DamageType.Physical;
                        break;
                    case 4:
                        source.damageType = DamageType.Fire;
                        break;
                    case 5:
                        source.damageType = DamageType.Cold;
                        break;
                    case 6:
                        source.damageType = DamageType.Lightning;
                        break;
                }
            }
            catch (FormatException)
            {
                return false;
            }

            return true;
        }

        private bool parseModifier( List<StatModifier> outModifiers, string mod )
        {
            if (matchRegex( outModifiers, GenericIncrease, mod, ModifierType.Increase ))
            {
                return true;
            }

            if (matchRegex( outModifiers, AllAttributes, mod, ModifierType.FlatIncrease, 2 ))
            {
                return true;
            }

            if (matchRegex(outModifiers, GemLevel, mod, ModifierType.FlatIncrease))
            {
                return true;
            }

            if (matchRegex(outModifiers, AttributeIncrease, mod, ModifierType.FlatIncrease))
            {
                return true;
            }

            if (matchRegex( outModifiers, Reflect, mod, ModifierType.FlatIncrease ))
            {
                return true;
            }

            if (matchRegex( outModifiers, LifeReg, mod, ModifierType.FlatIncrease, 2 ))
            {
                return true;
            }

            if (matchRegex( outModifiers, AddedDamage, mod, ModifierType.FlatIncrease, 4 ))
            {
                return true;
            }

            if (matchRegex( outModifiers, RegenLeech, mod, ModifierType.FlatIncrease ))
            {
                return true;
            }

            if (matchRegex( outModifiers, FlatIncrease, mod, ModifierType.FlatIncrease ))
            {
                return true;
            }

            if (matchRegex( outModifiers, GenericReduce, mod, ModifierType.Reduce ))
            {
                return true;
            }

#if(DEBUG)
            throw new FormatException( mod );
#else
            return false;
#endif
        }

        protected override bool internalMatch( List<StatModifier> outModifiers, Regex regex, Match match,
                                               uint matchCount, ModifierType modifierType, float value )
        {
            StatType type;

            switch (matchCount)
            {
                case 2:
                    if (regex == LifeReg)
                    {
                        outModifiers.Add(new StatModifier(StatType.LifeReg, modifierType, value));
                    }
                    else
                    {
                        outModifiers.Add( new StatModifier( StatType.Strength, modifierType, value ) );
                        outModifiers.Add( new StatModifier( StatType.Intelligence, modifierType, value ) );
                        outModifiers.Add( new StatModifier( StatType.Dexterity, modifierType, value ) );
                    }
                    return true;

                case 3:
                    if (parseStatType( match.Groups[ 2 ].Value, out type ))
                    {
                        outModifiers.Add( new StatModifier( type, modifierType, value ) );
                        return true;
                    }
                    break;

                case 4:
                    {
                        DamageSource damageSource = parseAddedDamage( value, match.Groups[ 2 ].Value, match.Groups[ 3 ].Value );
                        if (damageSource != null)
                        {
                            switch (damageSource.damageType)
                            {
                                case DamageType.Chaos:
                                    outModifiers.Add( new StatModifier( StatType.AddedChaosMinDmg, modifierType, damageSource.min ) );
                                    outModifiers.Add( new StatModifier( StatType.AddedChaosMaxDmg, modifierType, damageSource.max ) );
                                    break;
                                case DamageType.Cold:
                                    outModifiers.Add( new StatModifier( StatType.AddedColdMinDmg, modifierType, damageSource.min ) );
                                    outModifiers.Add( new StatModifier( StatType.AddedColdMaxDmg, modifierType, damageSource.max ) );
                                    break;
                                case DamageType.Fire:
                                    outModifiers.Add( new StatModifier( StatType.AddedFireMinDmg, modifierType, damageSource.min ) );
                                    outModifiers.Add( new StatModifier( StatType.AddedFireMaxDmg, modifierType, damageSource.max ) );
                                    break;
                                case DamageType.Lightning:
                                    outModifiers.Add( new StatModifier( StatType.AddedLightningMinDmg, modifierType, damageSource.min ) );
                                    outModifiers.Add( new StatModifier( StatType.AddedLightningMaxDmg, modifierType, damageSource.max ) );
                                    break;
                                case DamageType.Physical:
                                    outModifiers.Add( new StatModifier( StatType.AddedPhysicalMinDmg, modifierType, damageSource.min ) );
                                    outModifiers.Add( new StatModifier( StatType.AddedPhysicalMaxDmg, modifierType, damageSource.max ) );
                                    break;
                            }
                            return true;
                        }
                    }
                    break;
            }

#if(DEBUG)
            throw new FormatException( match.ToString() );
#endif
            return false;
        }

        private static DamageSource parseAddedDamage( float minValue, string maxValue, string type )
        {
            DamageSource source = new DamageSource {min = minValue};

            float max;
            if (!float.TryParse( maxValue, out max ))
            {
                return null;
            }

            source.max = max;

            string lowerType = type.ToLowerInvariant();
            switch (lowerType)
            {
                case "chaos":
                    source.damageType = DamageType.Chaos;
                    break;
                case "cold":
                    source.damageType = DamageType.Cold;
                    break;
                case "fire":
                    source.damageType = DamageType.Fire;
                    break;
                case "lightning":
                    source.damageType = DamageType.Lightning;
                    break;
                case "physical":
                    source.damageType = DamageType.Physical;
                    break;
            }

            return source;
        }

        private static readonly Regex Reflect = new Regex( @"^Reflects (\d+) ([A-Za-z\s]+)", Options );
        private static readonly Regex LifeReg = new Regex( @"^([\d\.]+) Life Regenerated per second", Options );
        private static readonly Regex AddedDamage = new Regex( @"^Adds (\d+)-(\d+) ([A-Za-z]+) Damage", Options );
        private static readonly Regex AllAttributes = new Regex( @"^\+(\d+) to all Attributes", Options );
        private static readonly Regex GemLevel = new Regex( @"^\+(\d+) to Level of ([A-Za-z\s]+) in this item", Options );

        private static readonly Regex Damage = new Regex( @"(\d+)-(\d+)", Options );
        private static readonly Regex Percentage = new Regex( @"([\d\.]+)%", Options );
        private static readonly Regex WeaponTypeRegex = new Regex( @"([A-Za-z]+) handed ([A-Za-z]+)", Options );
    }
}