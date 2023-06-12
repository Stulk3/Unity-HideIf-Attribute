using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEditor;
using HideIfUtilities;

public abstract class HidingAttributeDrawer : PropertyDrawer {

    public static bool CheckShouldHide(SerializedProperty property) {
        try {
            bool shouldHide = false;

            var targetObject = Utilities.GetTargetObjectOfProperty(property);
            var type = targetObject.GetType();

            FieldInfo field;
            do
            {
                field = type.GetField(property.name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                type = type.BaseType;
            } while (field == null && type != null);
            
            var customAttributes = field.GetCustomAttributes(typeof (HidingAttribute), false);
            
            // 'property' may be a property of a serialized class or collection inside the property.serializedObject.
            // In that case we get the serialized property just above 'property' so the ShouldDraw method can test the
            // HideIf attribute using FindPropertyRelative on the 'propertyParent'.
            SerializedProperty propertyParent = null;
            var propertyPath = property.propertyPath;
            var lastDot = propertyPath.LastIndexOf('.');
            if (lastDot > 0)
            {
                var parentPath = propertyPath.Substring(0, lastDot);
                propertyParent = property.serializedObject.FindProperty(parentPath);
            }
            
            HidingAttribute[] attachedAttributes = (HidingAttribute[]) customAttributes;
            foreach (var hider in attachedAttributes) {
                if (!ShouldDraw(property.serializedObject, propertyParent, hider)) {
                    shouldHide = true;
                }
            }

            return shouldHide;
        }
        catch {
            return false;
        }
    }

  
    private static Dictionary<Type, Type> typeToDrawerType;

    private static Dictionary<Type, PropertyDrawer> drawerTypeToDrawerInstance;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (!CheckShouldHide(property)) {
            if (typeToDrawerType == null)
                PopulateTypeToDrawer();

            Type drawerType;
            var typeOfProp = Utilities.GetTargetObjectOfProperty(property).GetType();
            if (typeToDrawerType.TryGetValue(typeOfProp, out drawerType)) {
                var drawer = drawerTypeToDrawerInstance.GetOrAdd(drawerType, () => CreateDrawerInstance(drawerType));
                drawer.OnGUI(position, property, label);
            }
            else {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (CheckShouldHide(property))
            return -2;

        if (typeToDrawerType == null)
            PopulateTypeToDrawer();

        Type drawerType;
        var typeOfProp = Utilities.GetTargetObjectOfProperty(property).GetType();
        if (typeToDrawerType.TryGetValue(typeOfProp, out drawerType)) {
            var drawer = drawerTypeToDrawerInstance.GetOrAdd(drawerType, () => CreateDrawerInstance(drawerType));
            return drawer.GetPropertyHeight(property, label);
        }
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    private PropertyDrawer CreateDrawerInstance(Type drawerType) {
        return (PropertyDrawer) Activator.CreateInstance(drawerType);
    }

    private void PopulateTypeToDrawer() {
        typeToDrawerType = new Dictionary<Type, Type>();
        drawerTypeToDrawerInstance = new Dictionary<Type, PropertyDrawer>();
        var propertyDrawerType = typeof (PropertyDrawer);
        var targetType = typeof (CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
        var useForChildren = typeof (CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);

        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());

        foreach (Type type in types) {
            if (propertyDrawerType.IsAssignableFrom(type)) {
                var customPropertyDrawers = type.GetCustomAttributes(true).OfType<CustomPropertyDrawer>().ToList();
                foreach (var propertyDrawer in customPropertyDrawers) {
                    var targetedType = (Type) targetType.GetValue(propertyDrawer);
                    typeToDrawerType[targetedType] = type;

                    var usingForChildren = (bool) useForChildren.GetValue(propertyDrawer);
                    if (usingForChildren) {
                        var childTypes = types.Where(t => targetedType.IsAssignableFrom(t) && t != targetedType);
                        foreach (var childType in childTypes) {
                            typeToDrawerType[childType] = type;
                        }
                    }
                }

            }
        }
    }

    private static bool ShouldDraw(SerializedObject hidingobject, SerializedProperty serializedProperty, HidingAttribute hider) {
        var hideIf = hider as HideIfAttribute;
        if (hideIf != null) {
            return HideIfAttributeDrawer.ShouldDraw(hidingobject, serializedProperty, hideIf);
        }

        var hideIfNull = hider as HideIfNullAttribute;
        if (hideIfNull != null) {
            return HideIfNullAttributeDrawer.ShouldDraw(hidingobject, serializedProperty, hideIfNull);
        }

        var hideIfNotNull = hider as HideIfNotNullAttribute;
        if (hideIfNotNull != null) {
            return HideIfNotNullAttributeDrawer.ShouldDraw(hidingobject, serializedProperty, hideIfNotNull);
        }

        var hideIfEnum = hider as HideIfEnumValueAttribute;
        if (hideIfEnum != null) {
            return HideIfEnumValueAttributeDrawer.ShouldDraw(hidingobject, serializedProperty, hideIfEnum);
        }

        var hideIfCompare = hider as HideIfCompareValueAttribute;
        if (hideIfCompare != null) {
            return HideIfCompareValueAttributeDrawer.ShouldDraw(hidingobject, serializedProperty, hideIfCompare);
        }

        Debug.LogWarning("Trying to check unknown hider loadingType: " + hider.GetType().Name);
        return false;
    }

}

[CustomPropertyDrawer(typeof (HideIfAttribute))]
public class HideIfAttributeDrawer : HidingAttributeDrawer {
    public static bool ShouldDraw(SerializedObject hidingObject, SerializedProperty serializedProperty, HideIfAttribute attribute) {
        var prop = serializedProperty == null ? hidingObject.FindProperty(attribute.variable) : serializedProperty.FindPropertyRelative(attribute.variable);
        if (prop == null) {
            return true;
        }
        return prop.boolValue != attribute.state;
    }
}

[CustomPropertyDrawer(typeof (HideIfNullAttribute))]
public class HideIfNullAttributeDrawer : HidingAttributeDrawer {
    public static bool ShouldDraw(SerializedObject hidingObject, SerializedProperty serializedProperty, HideIfNullAttribute attribute) {
        var prop = serializedProperty == null ? hidingObject.FindProperty(attribute.variable) : serializedProperty.FindPropertyRelative(attribute.variable);
        if (prop == null) {
            return true;
        }

        return prop.objectReferenceValue != null;
    }
}

[CustomPropertyDrawer(typeof (HideIfNotNullAttribute))]
public class HideIfNotNullAttributeDrawer : HidingAttributeDrawer {
    public static bool ShouldDraw(SerializedObject hidingObject, SerializedProperty serializedProperty, HideIfNotNullAttribute attribute) {
        var prop = serializedProperty == null ? hidingObject.FindProperty(attribute.variable) : serializedProperty.FindPropertyRelative(attribute.variable);
        if (prop == null) {
            return true;
        }

        return prop.objectReferenceValue == null;
    }
}

[CustomPropertyDrawer(typeof (HideIfEnumValueAttribute))]
public class HideIfEnumValueAttributeDrawer : HidingAttributeDrawer {
    public static bool ShouldDraw(SerializedObject hidingObject, SerializedProperty serializedProperty, HideIfEnumValueAttribute hideIfEnumValueAttribute) {
        var enumProp = serializedProperty == null ? hidingObject.FindProperty(hideIfEnumValueAttribute.variable) : serializedProperty.FindPropertyRelative(hideIfEnumValueAttribute.variable);
        var states = hideIfEnumValueAttribute.states;

        //enumProp.enumValueIndex gives the order in the enum list, not the actual enum value
        bool equal = states.Contains(enumProp.intValue);

        return equal != hideIfEnumValueAttribute.hideIfEqual;
    }
}

[CustomPropertyDrawer(typeof (HideIfCompareValueAttribute))]
public class HideIfCompareValueAttributeDrawer : HidingAttributeDrawer {
    public static bool ShouldDraw(SerializedObject hidingObject, SerializedProperty serializedProperty, HideIfCompareValueAttribute hideIfCompareValueAttribute) {
        var variable = serializedProperty == null ? hidingObject.FindProperty(hideIfCompareValueAttribute.variable) : serializedProperty.FindPropertyRelative(hideIfCompareValueAttribute.variable);
        var compareValue = hideIfCompareValueAttribute.value;
        
        switch (hideIfCompareValueAttribute.hideIf)
        {
            case HideIf.Equal: return variable.intValue != compareValue;
            case HideIf.NotEqual: return variable.intValue == compareValue;
            case HideIf.Greater: return variable.intValue <= compareValue;
            default: /*case HideIf.Lower:*/ return variable.intValue >= compareValue;
        }
    }
}
