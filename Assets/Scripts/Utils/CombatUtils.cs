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

    [Serializable] public struct NaturalWeapon {

        public string attackName;
        public int damageDieAmount;
        public Die damageDieSize;
        public DamageType damageType;
        public AbilityScore damageModifier;
        public int bonusToHit;
        public int bonusToDamage;

    }

    public static class UCombat {

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