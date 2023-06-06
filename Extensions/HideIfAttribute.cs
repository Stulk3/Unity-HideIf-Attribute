using System;
using System.Collections;

using UnityEngine;

public abstract class HidingAttribute : PropertyAttribute {}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public class HideIfAttribute: HidingAttribute
{
    public readonly string variable;
    public readonly bool state;

    public HideIfAttribute(string variable, bool state, int order = 0)
    {
        this.variable = variable;
        this.state = state;
        this.order = order;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public class HideIfNullAttribute: HidingAttribute 
{
    public readonly string variable;
    
    public HideIfNullAttribute(string variable, int order = 0)
    {
        this.variable = variable;
        this.order = order;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public class HideIfNotNullAttribute : HidingAttribute
{
    public readonly string variable;
    public HideIfNotNullAttribute(string variable, int order = 0)
    {
        this.variable = variable;
        this.order = order;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public class HideIfEnumValueAttribute : HidingAttribute
{
    public readonly string variable;
    public readonly bool hideIfEqual;
    public readonly int[] states;
    
    public HideIfEnumValueAttribute(string variable, HideIf hideIf, params int[] states)
    {
        this.variable = variable;
        this.hideIfEqual = hideIf == HideIf.Equal;
        this.states = states;  
        this.order = -1;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public class HideIfCompareValueAttribute : HidingAttribute
{
    public readonly string variable;
    public readonly HideIf hideIf;
    public readonly int value;
    
    public HideIfCompareValueAttribute(string variable, HideIf hideIf, int value)
    {
        this.variable = variable;
        this.hideIf = hideIf;
        this.value = value;  
        this.order = -1;
    }
}

public enum HideIf
{
    Equal,
    NotEqual,
    Greater,
    Lower
}