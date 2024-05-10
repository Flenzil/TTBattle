using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore;

public enum Condition {
    blinded,
    charmed,
    deafened,
    frightened,
    grappled,
    incapacitated,
    invisible,
    paralyzed,
    petrified,
    poisoned,
    prone,
    restrained,
    stunned,
    unconcious
}

public static class Conditions{

    public static void ApplyCondition(Condition condition, CreatureStats creatureStats){

        Dictionary<Condition, Action<CreatureStats>> map = new() {
            {Condition.blinded, ApplyBlinded},
            {Condition.charmed, ApplyCharmed},
            {Condition.deafened, ApplyDeafened},
            {Condition.frightened, ApplyFrightened},
            {Condition.grappled, ApplyGrappled},
            {Condition.incapacitated, ApplyIncapacitated},
            {Condition.invisible, ApplyInvisible},
            {Condition.paralyzed, ApplyParalyzed},
            {Condition.petrified, ApplyPetrified},
            {Condition.poisoned, ApplyPoisoned},
            {Condition.prone, ApplyProne},
            {Condition.restrained, ApplyRestrained},
            {Condition.stunned, ApplyStunned},
            {Condition.unconcious, ApplyUnconcious},
        };

        map[condition].Invoke(creatureStats);

    }

    private static void ApplyBlinded(CreatureStats creatureStats){
        creatureStats.hasDisadvantageToHit = true;
        creatureStats.hasAdvantageToBeHit = true;
    }

    private static void ApplyCharmed(CreatureStats creatureStats){
    }

    private static void ApplyDeafened(CreatureStats creatureStats){
    }

    private static void ApplyFrightened(CreatureStats creatureStats){
        creatureStats.hasDisadvantageToHit = true;
    }

    private static void ApplyGrappled(CreatureStats creatureStats){
    }

    private static void ApplyIncapacitated(CreatureStats creatureStats){
    }

    private static void ApplyInvisible(CreatureStats creatureStats){
        creatureStats.hasAdvantageToHit= true;
        creatureStats.hasDisadvantageToBeHit= true;
    }

    private static void ApplyParalyzed(CreatureStats creatureStats){
        ApplyIncapacitated(creatureStats);
    }

    private static void ApplyPetrified(CreatureStats creatureStats){
        ApplyIncapacitated(creatureStats);
    }

    private static void ApplyPoisoned(CreatureStats creatureStats){
    }

    private static void ApplyProne(CreatureStats creatureStats){
    }

    private static void ApplyRestrained(CreatureStats creatureStats){
        ApplyGrappled(creatureStats);
        creatureStats.hasAdvantageToBeHit = true;
        creatureStats.hasDisadvantageToHit = true;
        
    }
    
    private static void ApplyStunned(CreatureStats creatureStats){
        ApplyIncapacitated(creatureStats);
    }

    private static void ApplyUnconcious(CreatureStats creatureStats){
        ApplyIncapacitated(creatureStats);
    }
}
