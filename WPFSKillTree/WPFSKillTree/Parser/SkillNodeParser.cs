using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using POESKillTree.Stats;

namespace POESKillTree.Parser
{
    public class SkillNodeParser : ParserBase
    {
        public List<StatModifier> parseNode( string nodeName, string[] nodeTexts )
        {
            List<StatModifier> result = new List<StatModifier>( nodeTexts.Length );

            Keystone keystone = parseKeystone( nodeName );
            if (keystone != Keystone.None)
            {
                result.Add( new StatModifier( keystone ) );
                return result;
            }

            foreach (string nodeText in nodeTexts)
            {
#if(!DEBUG)
                try
                {
#endif
                result.AddRange( parseNode( nodeText ) );
#if(!DEBUG)
                }
                catch (FormatException)
                {
                    result.Add( new StatModifier( StatType.None, ModifierType.FlatIncrease, 0 ) );
                }
#endif
            }

            return result;
        }

        private IEnumerable<StatModifier> parseNode( string nodeText )
        {
            List<StatModifier> outModifiers = new List<StatModifier>();

            if (matchRegex( outModifiers, Charges, nodeText, ModifierType.FlatIncrease ))
            {
                return outModifiers;
            }
            
            if (matchRegex( outModifiers, AttributeIncrease, nodeText, ModifierType.FlatIncrease ))
            {
                return outModifiers;
            }

            if (matchRegex( outModifiers, WeaponIncrease, nodeText, ModifierType.Increase, 4u ))
            {
                return outModifiers;
            }

            if (matchRegex( outModifiers, StanceIncrease, nodeText, ModifierType.Increase, 4u ))
            {
                return outModifiers;
            }

            if (matchRegex( outModifiers, GenericIncrease, nodeText, ModifierType.Increase ))
            {
                return outModifiers;
            }

            if (matchRegex( outModifiers, GenericReduce, nodeText, ModifierType.Reduce ))
            {
                return outModifiers;
            }

            if (matchRegex( outModifiers, FlatIncrease, nodeText, ModifierType.FlatIncrease ))
            {
                return outModifiers;
            }

            if (matchRegex( outModifiers, Chance, nodeText, ModifierType.FlatIncrease ))
            {
                return outModifiers;
            }

            if (matchRegex( outModifiers, WandDamage, nodeText, ModifierType.Increase ))
            {
                return outModifiers;
            }

            if (matchRegex( outModifiers, RegenLeech, nodeText, ModifierType.FlatIncrease ))
            {
                return outModifiers;
            }

            if (matchRegex( outModifiers, Minions, nodeText, ModifierType.Increase ))
            {
                return outModifiers;
            }

            if (matchRegex( outModifiers, Traps, nodeText, ModifierType.FlatIncrease, 2 ))
            {
                return outModifiers;
            }

            throw new FormatException( nodeText );
        }

        protected override bool internalMatch( List<StatModifier> outModifiers, Regex regex, Match match, uint matchCount, ModifierType modifierType, float value )
        {
            StatType type;
            switch (matchCount)
            {
                case 2:
                    outModifiers.Add( new StatModifier( StatType.TrapsAllowed, ModifierType.FlatIncrease, value ) );
                    return true;

                case 3:
                    if (( regex == Minions && parseMinionStatType( match.Groups[ 2 ].Value, out type ) ) ||
                        ( regex == WandDamage && parseWandDamageStatType( match.Groups[ 2 ].Value, out type ) ) ||
                        ( regex != Minions && regex != WandDamage &&
                          parseStatType( match.Groups[ 2 ].Value, out type ) ))
                    {
                        outModifiers.Add( new StatModifier( type, modifierType, value ) );
                        return true;
                    }
                    break;

                case 4:
                    if (parseWeaponStatType( match.Groups[ 2 ].Value, match.Groups[ 3 ].Value, out type ))
                    {
                        outModifiers.Add( new StatModifier( type, modifierType, value ) );
                        return true;
                    }
                    break;
            }
            
            return false;
        }

        private static bool parseWeaponStatType( string stat, string weapon, out StatType type )
        {
            string prefix = weapon.Substring( 0, weapon.Length - 1 );

            string lowerWeapon = weapon.ToLowerInvariant();
            switch (lowerWeapon)
            {
                case "one handed melee weapons":
                    prefix = "OneHand";
                    break;
                case "two handed melee weapons":
                case "two handed melee weapons on enemies":
                    prefix = "TwoHand";
                    break;
                case "dual wielding":
                    prefix = "DualWield";
                    break;
                case "wielding a staff":
                case "staves":
                case "staves on enemies":
                    prefix = "Staff";
                    break;
                case "bows on enemies":
                    prefix = "Bow";
                    break;
            }

            string lowerStat = stat.ToLowerInvariant().Replace( " ", string.Empty );

            if (Enum.TryParse( prefix + lowerStat, true, out type ))
            {
                return true;
            }
            throw new FormatException( prefix + lowerStat );
        }

        private static bool parseMinionStatType( string value, out StatType type )
        {
            string lowerValue = value.ToLowerInvariant();
            switch (lowerValue)
            {
                case "damage":
                    type = StatType.MinionDamage;
                    return true;
                case "maximum life":
                    type = StatType.MinionLife;
                    return true;
            }

            type = StatType.None;
            return false;
        }

        private static bool parseWandDamageStatType( string value, out StatType type )
        {
            string lowerValue = value.ToLowerInvariant();
            switch (lowerValue)
            {
                case "cold":
                    type = StatType.WandAddedColdDamage;
                    return true;
                case "fire":
                    type = StatType.WandAddedFireDamage;
                    return true;
                case "lightning":
                    type = StatType.WandAddedLightningDamage;
                    return true;
            }

            type = StatType.None;
            return false;
        }

        private static readonly Regex Charges = new Regex( @"^\+(\d+) Maximum ([A-Za-z\s]+)", Options );
        private static readonly Regex WeaponIncrease = new Regex( @"^(\d+)% increased ([A-Za-z\s]+) with ([A-Za-z\s]+)", Options );
        private static readonly Regex StanceIncrease = new Regex( @"^(\d+)% increased ([A-Za-z\s]+) while ([A-Za-z\s]+)", Options );
        private static readonly Regex Chance = new Regex( @"^(\d+)% Chance [t|o][o|f] ([A-Za-z\s]+)", Options );
        private static readonly Regex WandDamage = new Regex( @"^(\d+)% of Wand Physical Damage Added as ([A-Za-z]+) Damage", Options );
        private static readonly Regex Minions = new Regex( @"^Minions [A-Za-z]+ (\d+)% increased ([A-Za-z\s]+)", Options );
        private static readonly Regex Traps = new Regex( @"^Can set up to (\d+) additional trap", Options );
    }
}