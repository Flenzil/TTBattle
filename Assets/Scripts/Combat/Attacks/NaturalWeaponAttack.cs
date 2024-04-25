using CombatUtils;
using CreatureUtils;

public class NaturalWeaponAttack : Attack, IAttack {


    private NaturalWeapon naturalWeapon;

    public NaturalWeaponAttack(NaturalWeapon naturalWeapon) {
        this.naturalWeapon = naturalWeapon;
    }


    public override DamageType GetDamageType()
    {
        return naturalWeapon.damageType; 
    }

    public override int GetDamageRoll(){
        int damage = 0; 

        for (int i = 0; i < naturalWeapon.damageDieAmount; i++){
            damage += UCombat.RollDice(naturalWeapon.damageDieSize);
        }

        return damage;
    }

    public override string GetAttackName()
    {
        return naturalWeapon.attackName;
    }

    public override AbilityScore GetDamageModifier()
    {
        return naturalWeapon.damageModifier;
    }

    public override int GetBonusToHit(){
        return naturalWeapon.bonusToHit;
    }

    public override int GetBonusToDamage(){
        return naturalWeapon.bonusToDamage;
    }

    public override WeaponType GetWeaponType()
    {
        return WeaponType.natural;
    }

    public override void Override(out bool overrideToHit, out bool overrideDamage)
    {
        overrideToHit = naturalWeapon.overrideToHit;
        overrideDamage = naturalWeapon.overrideDamage;
    }
}