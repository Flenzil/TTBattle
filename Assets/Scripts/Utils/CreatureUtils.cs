using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CreatureUtils{

    public enum CreatureSize {
        tiny,
        small,
        medium,
        large,
        huge,
        gargantuan
    }

    public enum AbilityScore {
        strength,
        dexterity,
        constitution,
        intelligence,  
        wisdom,
        charisma,
        none
    }

    public static class UCreature{


        public static int SizeRanking(CreatureSize creatureSize){
            return creatureSize switch
            {
                CreatureSize.tiny => 0,
                CreatureSize.small => 1,
                CreatureSize.medium => 2,
                CreatureSize.large => 3,
                CreatureSize.huge => 4,
                CreatureSize.gargantuan => 5,
                _ => -1,
            };
        }

        public static int CreatureWidthAsInt(CreatureSize creatureSize){
            return creatureSize switch
            {
                CreatureSize.large => 2,
                CreatureSize.huge => 3,
                CreatureSize.gargantuan => 4,
                _ => 1,
            };
        }

        public static int CreatureSizesDifference(CreatureSize size1, CreatureSize size2){
            return SizeRanking(size1) - SizeRanking(size2);
        }
    }
}