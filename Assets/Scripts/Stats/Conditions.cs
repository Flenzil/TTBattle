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

    public static void ClearCondition(Condition condition, CreatureStats creatureStats){

        Dictionary<Condition, Action<CreatureStats>> map = new() {
            {Condition.blinded, ClearBlinded},
            {Condition.charmed, ClearCharmed},
            {Condition.deafened, ClearDeafened},
            {Condition.frightened, ClearFrightened},
            {Condition.grappled, ClearGrappled},
            {Condition.incapacitated, ClearIncapacitated},
            {Condition.invisible, ClearInvisible},
            {Condition.paralyzed, ClearParalyzed},
            {Condition.petrified, ClearPetrified},
            {Condition.poisoned, ClearPoisoned},
            {Condition.prone, ClearProne},
            {Condition.restrained, ClearRestrained},
            {Condition.stunned, ClearStunned},
            {Condition.unconcious, ClearUnconcious},
        };

        map[condition].Invoke(creatureStats);

    }
    private static void ApplyBlinded(CreatureStats creatureStats){
        creatureStats.hasDisadvantageToHit = true;
        creatureStats.hasAdvantageToBeHit = true;
    }

    private static void ClearBlinded(CreatureStats creatureStats) {
        creatureStats.hasDisadvantageToHit = false;
        creatureStats.hasAdvantageToBeHit = false;
    }

    private static void ApplyCharmed(CreatureStats creatureStats){
    }

    private static void ClearCharmed(CreatureStats creatureStats){
    }

    private static void ApplyDeafened(CreatureStats creatureStats){
    }

    private static void ClearDeafened(CreatureStats creatureStats){
    }

    private static void ApplyFrightened(CreatureStats creatureStats){
        creatureStats.hasDisadvantageToHit = true;
    }

    private static void ClearFrightened(CreatureStats creatureStats){
        creatureStats.hasDisadvantageToHit = false;
    }

    private static void ApplyGrappled(CreatureStats creatureStats){
    }

    private static void ClearGrappled(CreatureStats creatureStats){
    }

    private static void ApplyIncapacitated(CreatureStats creatureStats){
    }

    private static void ClearIncapacitated(CreatureStats creatureStats){
    }

    private static void ApplyInvisible(CreatureStats creatureStats){
        creatureStats.hasAdvantageToHit= true;
        creatureStats.hasDisadvantageToBeHit= true;
    }

    private static void ClearInvisible(CreatureStats creatureStats){
        creatureStats.hasAdvantageToHit= false;
        creatureStats.hasDisadvantageToBeHit= false;
    }

    private static void ApplyParalyzed(CreatureStats creatureStats){
        ApplyIncapacitated(creatureStats);
    }

    private static void ClearParalyzed(CreatureStats creatureStats){
        ClearIncapacitated(creatureStats);
    }

    private static void ApplyPetrified(CreatureStats creatureStats){
        ApplyIncapacitated(creatureStats);
    }

    private static void ClearPetrified(CreatureStats creatureStats){
        ClearIncapacitated(creatureStats);
    }

    private static void ApplyPoisoned(CreatureStats creatureStats){
    }

    private static void ClearPoisoned(CreatureStats creatureStats){
    }

    private static void ApplyProne(CreatureStats creatureStats){
    }

    private static void ClearProne(CreatureStats creatureStats){
    }

    private static void ApplyRestrained(CreatureStats creatureStats){
        ApplyGrappled(creatureStats);
        creatureStats.hasAdvantageToBeHit = true;
        creatureStats.hasDisadvantageToHit = true;
        
    }

    private static void ClearRestrained(CreatureStats creatureStats){
        ClearGrappled(creatureStats);
        creatureStats.hasAdvantageToBeHit = false;
        creatureStats.hasDisadvantageToHit = false;
    }
    
    private static void ApplyStunned(CreatureStats creatureStats){
        ApplyIncapacitated(creatureStats);
    }

    private static void ClearStunned(CreatureStats creatureStats){
        ClearIncapacitated(creatureStats);
    }

    private static void ApplyUnconcious(CreatureStats creatureStats){
        ApplyIncapacitated(creatureStats);
    }

    private static void ClearUnconcious(CreatureStats creatureStats){
        ClearIncapacitated(creatureStats);
    }
}
