 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
[RequireComponent(typeof(BoxCollider))]

public class Enemy : MonoBehaviour
{
    [Space(5f)]
    [Header("General Settings")]
    public float health = 100;
    public float speed = 15;
    public Type EnemyType;

    [HideIfEnumValue("EnemyType",HideIf.NotEqual, (int) Type.warrior)]
    public float swordDamage;
    [HideIfEnumValue("EnemyType",HideIf.NotEqual, (int) Type.archer)]
    public float arrowDamage;
    [HideIfEnumValue("EnemyType",HideIf.NotEqual, (int) Type.mage)]
    public float orbDamage;
    
    public bool hideColliderSettings;

    [HideIf("hideColliderSettings", true)]
    public Vector3 center;
    [HideIf("hideColliderSettings", true)]
    public Vector3 size;
    [HideIfNotNull("target")]
    [TextArea(1,2)] public string error = "Please, set enemy's target";
    public GameObject target;
    [HideIfNull("target")]
    float distanceToTarget;
    public CapsuleCollider enemyCollider;

    public enum Type : int
    {
        archer,
        warrior,
        mage
    }
}