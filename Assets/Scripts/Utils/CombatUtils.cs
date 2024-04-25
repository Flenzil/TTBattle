using System;
using CreatureUtils;

namespace CombatUtils {

    public enum DamageType 
    {
        bludgeoning,
        piercing,
        slashing,
        acid,
        cold,
        fire,
        force,
        lightning,
        necrotic,
        poison,
        psychic,
        radiant,
        thunder
    }

    public enum Die{
        d4,
        d6,
        d8,
        d10,
        d20,
        d100
    }

    public enum WeaponCatagory{
        simple,
        martial,
    }

    public enum WeaponType{
        battleaxe,
        blowgun,
        club,
        dagger,
        dart,
        flail,
        glaive,
        greataxe,
        greatclub,
        greatsword,
        halberd,
        hand_crossbow,
        handaxe,
        heavy_crossbow,
        javelin,
        lance,
        light_crossbow,
        light_hammer,
        longbow,
        longsword,
        mace,
        natural,
        net,
        maul,
        morningstar,
        pike,
        quarterstaff,
        rapier,
        scimitar,
        shortsword,
        sickle,
        spear,
        shortbow,
        sling,
        trident,
        war_pick,
        warhammer,
        whip,
    }

    [Serializable] public struct NaturalWeapon {

        public string attackName;
        public int damageDieAmount;
        public Die damageDieSize;
        public DamageType damageType;
        public AbilityScore damageModifier;
        public int bonusToHit;
        public bool overrideToHitBonus;
        public int bonusToDamage;
        public bool overrideDamageBonus;

    }

    public static class UCombat {

        public static bool IsSimpleWeapon(WeaponType weaponType) {
            switch (weaponType) {
                case WeaponType.club:
                case WeaponType.dagger:
                case WeaponType.greatclub:
                case WeaponType.javelin:
                case WeaponType.light_hammer:
                case WeaponType.mace:
                case WeaponType.quarterstaff:
                case WeaponType.sickle:
                case WeaponType.spear:
                case WeaponType.light_crossbow:
                case WeaponType.dart:
                case WeaponType.shortbow:
                case WeaponType.sling:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsMartialWeapon(WeaponType weaponType) {
            if (!IsSimpleWeapon(weaponType)){
                return true;
            } else {
                return false;
            }
        }
        public static int RollDice(Die die){
            int dieMax = 0;
            switch (die){
                case Die.d4:
                    dieMax = 4;
                    break;
                case Die.d6:
                    dieMax = 6;
                    break;
                case Die.d8:
                    dieMax = 8;
                    break;
                case Die.d10:
                    dieMax = 10;
                    break;
                case Die.d20:
                    dieMax = 20;
                    break;
                case Die.d100:
                    dieMax = 100;
                    break;

            }
            System.Random random = new();
            return random.Next(1, dieMax + 1);
        }
    }
}