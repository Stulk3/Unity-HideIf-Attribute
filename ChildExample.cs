using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildExample : ParentExample
{
    [HideIf("_isHidden", true)]
    [SerializeField] private int _number;
}
