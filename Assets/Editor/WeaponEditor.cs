using UnityEditor;
using UnityEngine;
using CombatUtils;
using System.Collections.Generic;

[CustomEditor(typeof(Weapon))]
public class MyEditorClass : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Weapon weapon = target as Weapon;

        if (weapon.weaponProperties.Contains(WeaponProperty.Range))
        {
            weapon.weaponRange = EditorGUILayout.IntField ("Weapon Short Range", weapon.weaponRange);
            weapon.weaponLongRange = EditorGUILayout.IntField ("Weapon Long Range", weapon.weaponLongRange);
        }
    }
}