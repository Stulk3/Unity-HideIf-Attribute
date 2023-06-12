using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(BoxCollider))]

public class HideIfExample : MonoBehaviour
{
    [Space(5f)]
    [Header("General Settings")]
    public float health = 100;
    public float speed = 15;
    public Type EnemyType;

    [HideIfEnumValue("EnemyType",HideIf.NotEqual, (int) Type.warrior)]
    [SerializeField] private float _swordDamage;
    [HideIfEnumValue("EnemyType",HideIf.NotEqual, (int) Type.archer)]
    [SerializeField] private float _arrowDamage;
    [HideIfEnumValue("EnemyType",HideIf.NotEqual, (int) Type.mage)]
    [SerializeField] private float _orbDamage;

    [HideIfEnumValue("EnemyType",HideIf.Equal, (int) Type.warrior)]
    public float projectileSpeed {get; private set;}
    
    public bool hideColliderSettings;

    [HideIf("hideColliderSettings", true)]
    public Vector3 center;
    [HideIf("hideColliderSettings", true)]
    public Vector3 size;
    [HideIfNotNull("target")]
    [TextArea(1,2)] public string error = "Please, set enemy's target";
    public GameObject target;
    [HideIfEnumValue("EnemyType", HideIf.NotEqual, (int) Type.warrior)]
    [HideIfNull("target")]
    float distanceToTarget;
    public CapsuleCollider enemyCollider;

	public int attackComboCount = 1;
    [HideIfCompareValue("attackComboCount", HideIf.Lower, 2)]
    public float timeBetweenAttacks = 1;

    public enum Type : int
    {
        archer,
        warrior,
        mage
    }
}