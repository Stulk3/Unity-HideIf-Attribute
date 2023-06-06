using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Example", menuName = "ScriptableObject", order = 1)]
public class HideIfScriptable : ScriptableObject
{
    public bool isHidden;

    [HideIf("isHidden", true)]
    public int data;
}
