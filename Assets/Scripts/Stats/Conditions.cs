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

    public static void ApplyCondition(Condition condition, Creature creature){

        Dictionary<Condition, Action<Creature>> map = new() {
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

        map[condition].Invoke(creature);

    }

    public static void ClearCondition(Condition condition, Creature creature){

        Dictionary<Condition, Action<Creature>> map = new() {
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

        map[condition].Invoke(creature);

    }
    private static void ApplyBlinded(Creature creature){
        creature.hasDisadvantageToHit = true;
        creature.hasAdvantageToBeHit = true;
    }

    private static void ClearBlinded(Creature creature) {
        creature.hasDisadvantageToHit = false;
        creature.hasAdvantageToBeHit = false;
    }

    private static void ApplyCharmed(Creature creature){
    }

    private static void ClearCharmed(Creature creature){
    }

    private static void ApplyDeafened(Creature creature){
    }

    private static void ClearDeafened(Creature creature){
    }

    private static void ApplyFrightened(Creature creature){
        creature.hasDisadvantageToHit = true;
    }

    private static void ClearFrightened(Creature creature){
        creature.hasDisadvantageToHit = false;
    }

    private static void ApplyGrappled(Creature creature){
    }

    private static void ClearGrappled(Creature creature){
    }

    private static void ApplyIncapacitated(Creature creature){
    }

    private static void ClearIncapacitated(Creature creature){
    }

    private static void ApplyInvisible(Creature creature){
        creature.hasAdvantageToHit= true;
        creature.hasDisadvantageToBeHit= true;
    }

    private static void ClearInvisible(Creature creature){
        creature.hasAdvantageToHit= false;
        creature.hasDisadvantageToBeHit= false;
    }

    private static void ApplyParalyzed(Creature creature){
        ApplyIncapacitated(creature);
    }

    private static void ClearParalyzed(Creature creature){
        ClearIncapacitated(creature);
    }

    private static void ApplyPetrified(Creature creature){
        ApplyIncapacitated(creature);
    }

    private static void ClearPetrified(Creature creature){
        ClearIncapacitated(creature);
    }

    private static void ApplyPoisoned(Creature creature){
    }

    private static void ClearPoisoned(Creature creature){
    }

    private static void ApplyProne(Creature creature){
    }

    private static void ClearProne(Creature creature){
    }

    private static void ApplyRestrained(Creature creature){
        ApplyGrappled(creature);
        creature.hasAdvantageToBeHit = true;
        creature.hasDisadvantageToHit = true;
        
    }

    private static void ClearRestrained(Creature creature){
        ClearGrappled(creature);
        creature.hasAdvantageToBeHit = false;
        creature.hasDisadvantageToHit = false;
    }
    
    private static void ApplyStunned(Creature creature){
        ApplyIncapacitated(creature);
    }

    private static void ClearStunned(Creature creature){
        ClearIncapacitated(creature);
    }

    private static void ApplyUnconcious(Creature creature){
        ApplyIncapacitated(creature);
    }

    private static void ClearUnconcious(Creature creature){
        ClearIncapacitated(creature);
    }
}
