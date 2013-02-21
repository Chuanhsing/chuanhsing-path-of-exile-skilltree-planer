using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using POESKillTree.Stats;

namespace POESKillTree.Parser
{
    public abstract class ParserBase
    {
        protected bool matchRegex( List<StatModifier> outModifiers, Regex regex, string nodeText, ModifierType modifierType, uint matchCount = 3u )
        {
            float value;

            Match match = regex.Match( nodeText );
            if (match.Success)
            {
                if (match.Groups.Count == matchCount && float.TryParse( match.Groups[ 1 ].Value, out value ))
                {
                    if (internalMatch( outModifiers, regex, match, matchCount, modifierType, value))
                    {
                        return true;
                    }
                }
#if(DEBUG)
                throw new FormatException( nodeText );
#endif
            }

            return false;
        }

        protected abstract bool internalMatch( List<StatModifier> outModifiers, Regex regex, Match match, uint matchCount, ModifierType modifierType, float value );

        protected static Keystone parseKeystone( string nodeName )
        {
            Keystone keystone = Keystone.None;

            switch (nodeName)
            {
                case "Acrobatics":
                    keystone = Keystone.Acrobatics;
                    break;
                case "Adder's Touch":
                    keystone = Keystone.AddersTouch;
                    break;
                case "Ancestral Bond":
                    keystone = Keystone.AncestralBond;
                    break;
                case "Armour Master":
                    keystone = Keystone.ArmorMaster;
                    break;
                case "Arrow Dodging":
                    keystone = Keystone.ArrowDodging;
                    break;
                case "Bloodless":
                    keystone = Keystone.Bloodless;
                    break;
                case "Blood Magic":
                    keystone = Keystone.BloodMagic;
                    break;
                case "Breath of Rime":
                    keystone = Keystone.BreathOfRime;
                    break;
                case "Cannot be Frozen":
                    keystone = Keystone.FreezeImmunity;
                    break;
                case "Cannot be Shocked":
                    keystone = Keystone.ShockImmunity;
                    break;
                case "Chaos Inoculation":
                    keystone = Keystone.ChaosInoculation;
                    break;
                case "Conduit":
                    keystone = Keystone.Conduit;
                    break;
                case "Eldritch Battery":
                    keystone = Keystone.EldritchBattery;
                    break;
                case "Elemental Equilibrium":
                    keystone = Keystone.ElementalEquilibrium;
                    break;
                case "Ghost Reaver":
                    keystone = Keystone.GhostReaver;
                    break;
                case "Hammer Blows":
                    keystone = Keystone.HammerBlows;
                    break;
                case "Hex Master":
                    keystone = Keystone.HexMaster;
                    break;
                case "Iron Grip":
                    keystone = Keystone.IronGrip;
                    break;
                case "Iron Reflexes":
                    keystone = Keystone.IronReflexes;
                    break;
                case "King of the Hill":
                    keystone = Keystone.KingOfTheHill;
                    break;
                case "Minion Instability":
                    keystone = Keystone.MinionInstability;
                    break;
                case "Necromantic Aegis":
                    keystone = Keystone.NecromanticAegis;
                    break;
                case "Pain Attunement":
                    keystone = Keystone.PainAttunement;
                    break;
                case "Phase Acrobatics":
                    keystone = Keystone.PhaseAcrobatics;
                    break;
                case "Piercing Shots":
                    keystone = Keystone.PiercingShots;
                    break;
                case "Point Blank":
                    keystone = Keystone.PointBlank;
                    break;
                case "Powerful Blast":
                    keystone = Keystone.PowerfulBlast;
                    break;
                case "Purity of Essence":
                    keystone = Keystone.PurityOfEssence;
                    break;
                case "Resolute Technique":
                    keystone = Keystone.ResoluteTechnique;
                    break;
                case "Unwavering Stance":
                    keystone = Keystone.UnwaveringStance;
                    break;
                case "Vaal Pact":
                    keystone = Keystone.VaalPact;
                    break;
                case "Whispers of Doom":
                    keystone = Keystone.WhispersOfDoom;
                    break;
                case "Zealot's Oath":
                    keystone = Keystone.ZealotsOath;
                    break;
            }

            return keystone;
        }

        protected static bool parseStatType( string value, out StatType type )
        {
            string lower = value.ToLowerInvariant();

            switch (lower)
            {
                case "armour and energy shield":
                case "energy shield":
                case "evasion and energy shield":
                    // Item-only mods we don't need as attributes
                    type = StatType.None;
                    return true;

                case "accuracy rating":
                    type = StatType.AccuracyRating;
                    return true;
                case "all elemental resistances":
                    type = StatType.AllResist;
                    return true;
                case "area damage":
                    type = StatType.AoEDamage;
                    return true;
                case "armour":
                    type = StatType.Armor;
                    return true;
                case "armour and evasion":
                    type = StatType.EvasionAndArmorRating;
                    return true;
                case "arrow speed":
                    type = StatType.ArrowSpeed;
                    return true;
                case "attack speed":
                    type = StatType.AttackSpeed;
                    return true;
                case "avoid being chilled":
                    type = StatType.ChillAvoidance;
                    return true;
                case "avoid being frozen":
                    type = StatType.FreezeAvoidance;
                    return true;
                case "avoid being ignited":
                    type = StatType.BurnAvoidance;
                    return true;
                case "avoid being shocked":
                    type = StatType.ShockAvoidance;
                    return true;
                case "avoid being stunned":
                    type = StatType.StunAvoidance;
                    return true;
                case "avoid elemental status ailments":
                    type = StatType.ElementalStatusAvoidance;
                    return true;
                case "avoid interruption while casting":
                    type = StatType.InterruptAvoidance;
                    return true;
                case "block and stun recovery":
                    type = StatType.BlockAndStunRecovery;
                    return true;
                case "block chance":
                    type = StatType.ShieldBlockChance;
                    return true;
                case "block chance applied to spells":
                    type = StatType.SpellBlockChanceFromPhysicalBlockChance;
                    return true;
                case "block chance while dual wielding":
                    type = StatType.DualWieldBlockChance;
                    return true;
                case "block chance while dual wielding or holding a shield":
                    type = StatType.DualWieldShieldBlockChance;
                    return true;
                case "block chance with staves":
                    type = StatType.StaffBlockChance;
                    return true;
                case "block recovery":
                    type = StatType.BlockRecovery;
                    return true;
                case "bow gems":
                    type = StatType.BowGemLevel;
                    return true;
                case "burn duration on enemies":
                    type = StatType.BurnDurationOnEnemies;
                    return true;
                case "burning damage":
                    type = StatType.BurnDamageOnEnemies;
                    return true;
                case "cast speed":
                    type = StatType.CastSpeed;
                    return true;
                case "cast speed for curses":
                    type = StatType.CurseCastSpeed;
                    return true;
                case "chance to dodge attacks":
                    type = StatType.DodgeChance;
                    return true;
                case "chaos resistance":
                    type = StatType.ChaosResist;
                    return true;
                case "chill duration on you":
                    type = StatType.ChillDuration;
                    return true;
                case "chill duration on enemies":
                    type = StatType.ChillDurationOnEnemies;
                    return true;
                case "cold damage":
                    type = StatType.ColdDamage;
                    return true;
                case "cold gems":
                    type = StatType.ColdGemLevel;
                    return true;
                case "cold resistance":
                    type = StatType.ColdResist;
                    return true;
                case "critical strike chance":
                    type = StatType.CriticalStrikeChance;
                    return true;
                case "critical strike chance for spells":
                    type = StatType.SpellCriticalStrikeChance;
                    return true;
                case "critical strike multiplier for spells":
                    type = StatType.SpellCriticalStrikeMultiplier;
                    return true;
                case "critical strike multiplier":
                    type = StatType.CriticalStrikeMultiplier;
                    return true;
                case "defences from equipped shield":
                    type = StatType.ShieldDefences;
                    return true;
                case "dexterity":
                    type = StatType.Dexterity;
                    return true;
                case "duration of buffs and debuffs you create from skills":
                    type = StatType.BuffDebuffDuration;
                    return true;
                case "effect of buffs on you":
                    type = StatType.InnerForce;
                    return true;
                case "elemental damage":
                    type = StatType.ElementalDamage;
                    return true;
                case "elemental damage with weapons":
                    type = StatType.WeaponElementalDamage;
                    return true;
                case "elemental resistances while holding a shield":
                    type = StatType.ShieldAllResist;
                    return true;
                case "endurance charge":
                    type = StatType.EnduranceChargeCount;
                    return true;
                case "endurance charge duration":
                    type = StatType.EnduranceChargeDuration;
                    return true;
                case "enemy chance to block sword attacks":
                    type = StatType.SwordEnemyBlockChance;
                    return true;
                case "enemy stun threshold":
                    type = StatType.EnemyStunThreshold;
                    return true;
                case "enemy stun threshold with maces":
                    type = StatType.MaceEnemyStunThreshold;
                    return true;
                case "energy shield cooldown recovery":
                    type = StatType.EnergyShieldCooldownRecovery;
                    return true;
                case "energy shield from equipped shield":
                    type = StatType.EnergyShieldFromShield;
                    return true;
                case "evasion rating":
                    type = StatType.EvasionRating;
                    return true;
                case "evasion rating and armour":
                    type = StatType.EvasionAndArmorRating;
                    return true;
                case "evasion rating per frenzy charge":
                    type = StatType.FrenzyChargeEvasionRating;
                    return true;
                case "extra damage from critical strikes":
                    type = StatType.CriticalDamageTaken;
                    return true;
                case "flask charges used":
                    type = StatType.FlaskChargesUsed;
                    return true;
                case "flask life recovery rate":
                    type = StatType.FlaskLifeRecoveryRate;
                    return true;
                case "flask recovery speed":
                    type = StatType.FlaskRecoverySpeed;
                    return true;
                case "fire damage":
                    type = StatType.FireDamage;
                    return true;
                case "fire gems":
                    type = StatType.FireGemLevel;
                    return true;
                case "fire resistance":
                    type = StatType.FireResist;
                    return true;
                case "freeze duration on enemies":
                    type = StatType.FreezeDurationOnEnemies;
                    return true;
                case "freeze enemies on hit with cold damage":
                    type = StatType.FreezeChanceOnEnemies;
                    return true;
                case "frenzy charge":
                    type = StatType.FrenzyChargeCount;
                    return true;
                case "frenzy charge duration":
                    type = StatType.FrenzyChargeDuration;
                    return true;
                case "gems":
                    type = StatType.GemLevel;
                    return true;
                case "global critical strike chance":
                    type = StatType.CriticalStrikeChance;
                    return true;
                case "global critical strike multiplier":
                    type = StatType.CriticalStrikeMultiplier;
                    return true;
                case "ignite the enemy on hit with fire damage":
                    type = StatType.BurnChanceOnEnemies;
                    return true;
                case "intelligence":
                    type = StatType.Intelligence;
                    return true;
                case "knockback distance":
                    type = StatType.KnockbackDistance;
                    return true;
                case "knock enemies back on hit":
                    type = StatType.KnockbackChance;
                    return true;
                case "life gained for each enemy hit by your attacks":
                    type = StatType.LifeGainOnHit;
                    return true;
                case "life gained when you kill an enemy":
                    type = StatType.LifeGainOnKill;
                    return true;
                case "life recovery from flasks":
                    type = StatType.FlaskLifeRecovery;
                    return true;
                case "life regenerated per second":
                    type = StatType.LifeReg;
                    return true;
                case "lightning damage":
                    type = StatType.LightningDamage;
                    return true;
                case "lightning gems":
                    type = StatType.LightningGemLevel;
                    return true;
                case "lightning resistance":
                    type = StatType.LightningResist;
                    return true;
                case "mana cost of skills":
                    type = StatType.ManaCostOfSkills;
                    return true;
                case "mana gained when you kill an enemy":
                    type = StatType.ManaGainOnKill;
                    return true;
                case "mana recovery from flasks":
                    type = StatType.FlaskManaRecovery;
                    return true;
                case "mana regeneration rate":
                    type = StatType.ManaReg;
                    return true;
                case "mana reserved":
                    type = StatType.AuraManaReservation;
                    return true;
                case "maximum energy shield":
                    type = StatType.MaxEnergyShield;
                    return true;
                case "maximum cold resistance":
                    type = StatType.MaxColdResist;
                    return true;
                case "maximum fire resistance":
                    type = StatType.MaxFireResist;
                    return true;
                case "maximum lightning resistance":
                    type = StatType.MaxLightningResist;
                    return true;
                case "maximum life":
                    type = StatType.MaxLife;
                    return true;
                case "maximum life regenerated per second per endurance charge":
                    type = StatType.EnduranceChargeLifeReg;
                    return true;
                case "maximum mana":
                    type = StatType.MaxMana;
                    return true;
                case "maximum number of skeletons":
                    type = StatType.MaxSkeletons;
                    return true;
                case "maximum number of spectres":
                    type = StatType.MaxSpectres;
                    return true;
                case "maximum number of zombies":
                    type = StatType.MaxZombies;
                    return true;
                case "melee gems":
                    type = StatType.MeleeGemLevel;
                    return true;
                case "melee physical damage":
                    type = StatType.MeleePhysicalDamage;
                    return true;
                case "minion gems":
                    type = StatType.MinionGemLevel;
                    return true;
                case "movement speed":
                    type = StatType.MovementSpeed;
                    return true;
                case "physical attack damage leeched back as life":
                    type = StatType.PhysicalLifeLeech;
                    return true;
                case "physical attack damage leeched back as mana":
                    type = StatType.PhysicalManaLeech;
                    return true;
                case "physical damage":
                    type = StatType.PhysicalDamage;
                    return true;
                case "physical damage dealt with claws leeched back as life":
                    type = StatType.ClawLifeLeech;
                    return true;
                case "physical damage to melee attackers":
                    type = StatType.PhysicalReflect;
                    return true;
                case "power charge":
                    type = StatType.PowerChargeCount;
                    return true;
                case "power charge duration":
                    type = StatType.PowerChargeDuration;
                    return true;
                case "projectile damage":
                    type = StatType.ProjectileDamage;
                    return true;
                case "quantity of items found":
                    type = StatType.ItemQuantity;
                    return true;
                case "radius of area skills":
                    type = StatType.AoERadius;
                    return true;
                case "radius of auras":
                    type = StatType.AuraRadius;
                    return true;
                case "rarity of items found":
                    type = StatType.ItemRarity;
                    return true;
                case "shield block chance":
                    type = StatType.ShieldBlockChance;
                    return true;
                case "shock duration on enemies":
                    type = StatType.ShockDurationOnEnemies;
                    return true;
                case "shock the enemy on hit with lightning damage":
                    type = StatType.ShockChanceOnEnemies;
                    return true;
                case "skill effect duration":
                    type = StatType.SkillDuration;
                    return true;
                case "spell damage":
                    type = StatType.SpellDamage;
                    return true;
                case "spell damage per power charge":
                    type = StatType.PowerChargeSpellDamage;
                    return true;
                case "strength":
                    type = StatType.Strength;
                    return true;
                case "stun duration on enemies":
                    type = StatType.StunDurationOnEnemies;
                    return true;
                case "totem duration":
                    type = StatType.TotemDuration;
                    return true;
                case "totem life":
                    type = StatType.TotemLife;
                    return true;
                case "totem range":
                    type = StatType.TotemRange;
                    return true;
                case "trap and mine duration":
                    type = StatType.TrapMineDuration;
                    return true;
                case "trap and mine laying speed":
                    type = StatType.TrapMineLayingSpeed;
                    return true;
                case "trap and mine trigger radius":
                    type = StatType.TrapMineTriggerRadius;
                    return true;
                case "wand damage per power charge":
                    type = StatType.WandDamagePerPowerCharge;
                    return true;
            }

            type = StatType.None;
#if(DEBUG)
            return false;
#endif
            return true;
        }

        protected const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
        protected static readonly Regex GenericIncrease = new Regex( @"^(\d+)% increased ([A-Za-z\s]+)", Options );
        protected static readonly Regex AttributeIncrease = new Regex( @"^\+(\d+)%? t?o?\s?([A-Za-z\s]+)", Options );
        protected static readonly Regex RegenLeech = new Regex( @"^([\d\.]+)% of ([A-Za-z\s]+)", Options );
        protected static readonly Regex FlatIncrease = new Regex( @"^(\d+)% additional ([A-Za-z\s]+)", Options );
        protected static readonly Regex GenericReduce = new Regex( @"(\d+)% reduced ([A-Za-z\s]+)", Options );
    }
}
