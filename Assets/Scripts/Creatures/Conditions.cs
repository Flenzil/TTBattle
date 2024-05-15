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
    immobilised,
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
            {Condition.immobilised, ApplyImmobilised},
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
            {Condition.immobilised, ClearImmobilised},
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
        creature.currentConditions.Add(Condition.blinded);
    }

    private static void ClearBlinded(Creature creature) {
        creature.hasDisadvantageToHit = false;
        creature.hasAdvantageToBeHit = false;
        creature.currentConditions.Remove(Condition.blinded);
    }

    private static void ApplyCharmed(Creature creature){
        creature.currentConditions.Add(Condition.charmed);
    }

    private static void ClearCharmed(Creature creature){
        creature.currentConditions.Remove(Condition.charmed);
    }

    private static void ApplyDeafened(Creature creature){
        creature.currentConditions.Add(Condition.deafened);
    }

    private static void ClearDeafened(Creature creature){
        creature.currentConditions.Remove(Condition.deafened);
    }

    private static void ApplyFrightened(Creature creature){
        creature.currentConditions.Add(Condition.frightened);
        creature.hasDisadvantageToHit = true;
    }

    private static void ClearFrightened(Creature creature){
        creature.currentConditions.Remove(Condition.frightened);
        creature.hasDisadvantageToHit = false;
    }

    private static void ApplyGrappled(Creature creature){
        ApplyImmobilised(creature);
        creature.currentConditions.Add(Condition.grappled);
    }

    private static void ClearGrappled(Creature creature){
        creature.currentConditions.Remove(Condition.grappled);
        ClearImmobilised(creature);
    }

    private static void ApplyImmobilised(Creature creature){
        creature.SetReaminingMovement(0);
        creature.currentConditions.Add(Condition.immobilised);
    }

    private static void ClearImmobilised(Creature creature){
        creature.SetReaminingMovement(creature.GetMovementSpeed());
        creature.currentConditions.Remove(Condition.immobilised);
    }

    private static void ApplyIncapacitated(Creature creature){
        creature.currentConditions.Add(Condition.incapacitated);
    }

    private static void ClearIncapacitated(Creature creature){
        creature.currentConditions.Remove(Condition.incapacitated);
    }

    private static void ApplyInvisible(Creature creature){
        creature.currentConditions.Add(Condition.invisible);
        creature.hasAdvantageToHit= true;
        creature.hasDisadvantageToBeHit= true;
    }

    private static void ClearInvisible(Creature creature){
        creature.currentConditions.Remove(Condition.invisible);
        creature.hasAdvantageToHit= false;
        creature.hasDisadvantageToBeHit= false;
    }

    private static void ApplyParalyzed(Creature creature){
        creature.hasAdvantageToBeHit = true;
        creature.currentConditions.Add(Condition.paralyzed);
        ApplyIncapacitated(creature);
    }

    private static void ClearParalyzed(Creature creature){
        creature.hasAdvantageToBeHit = false;
        creature.currentConditions.Remove(Condition.paralyzed);
        ClearIncapacitated(creature);
    }

    private static void ApplyPetrified(Creature creature){
        creature.currentConditions.Add(Condition.petrified);
        ApplyIncapacitated(creature);
    }

    private static void ClearPetrified(Creature creature){
        creature.currentConditions.Remove(Condition.petrified);
        ClearIncapacitated(creature);
    }

    private static void ApplyPoisoned(Creature creature){
        creature.hasDisadvantageToHit = true;
        creature.currentConditions.Add(Condition.poisoned);
    }

    private static void ClearPoisoned(Creature creature){
        creature.hasDisadvantageToHit = false;
        creature.currentConditions.Remove(Condition.poisoned);
    }

    private static void ApplyProne(Creature creature){
        creature.currentConditions.Add(Condition.prone);
    }

    private static void ClearProne(Creature creature){
        creature.currentConditions.Remove(Condition.prone);
    }

    private static void ApplyRestrained(Creature creature){
        creature.currentConditions.Add(Condition.restrained);
        ApplyImmobilised(creature);
        creature.hasAdvantageToBeHit = true;
        creature.hasDisadvantageToHit = true;
        
    }

    private static void ClearRestrained(Creature creature){
        creature.currentConditions.Remove(Condition.restrained);
        ClearImmobilised(creature);
        creature.hasAdvantageToBeHit = false;
        creature.hasDisadvantageToHit = false;
    }
    
    private static void ApplyStunned(Creature creature){
        creature.hasAdvantageToBeHit = true;
        creature.currentConditions.Add(Condition.stunned);
        ApplyIncapacitated(creature);
    }

    private static void ClearStunned(Creature creature){
        creature.hasAdvantageToBeHit = false;
        creature.currentConditions.Remove(Condition.stunned);
        ClearIncapacitated(creature);
    }

    private static void ApplyUnconcious(Creature creature){
        creature.hasAdvantageToBeHit = true;
        creature.currentConditions.Add(Condition.unconcious);
        ApplyProne(creature);
        ApplyIncapacitated(creature);
    }

    private static void ClearUnconcious(Creature creature){
        creature.hasAdvantageToBeHit = false;
        creature.currentConditions.Remove(Condition.unconcious);
        ClearIncapacitated(creature);
    }
}
