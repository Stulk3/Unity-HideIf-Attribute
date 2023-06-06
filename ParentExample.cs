using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentExample : MonoBehaviour
{
    [SerializeField] private bool _isHidden;
    [HideIf("_isHidden",true)]
    [SerializeField] private int _age;
}
