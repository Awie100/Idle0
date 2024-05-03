using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif




[System.Serializable]
public class FacObj
{
    public Factory fac;
    public float baseValue, baseMult;

    public enum Types { Amount, Cap, Multiplier, Timer, TimerCap };
    public Types type;
    public bool isMult, hide;
    public int target;

    public float value, mult;

    public void Init()
    {
        value = baseValue;
        mult = baseMult;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FacObj))]
public class FacObjGui : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        position.height /= 4;
        position.y += position.height;

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var fac = property.FindPropertyRelative("fac");
        var facRect = new Rect(position.x, position.y, position.width, position.height);
        EditorGUI.PropertyField(facRect, fac, GUIContent.none);
        EditorGUI.LabelField(facRect, new GUIContent("", "Factory To Exchange With"));

        var value = property.FindPropertyRelative("baseValue");
        var valueRect = new Rect(position.x, position.y + position.height, position.width / 3, position.height);
        EditorGUI.PropertyField(valueRect, value, GUIContent.none);
        EditorGUI.LabelField(valueRect, new GUIContent("", "Base Amount To Exchange"));

        var mult = property.FindPropertyRelative("baseMult");
        var multRect = new Rect(position.x + position.width / 3, position.y + position.height, position.width / 3, position.height);
        EditorGUI.PropertyField(multRect, mult, GUIContent.none);
        EditorGUI.LabelField(multRect, new GUIContent("", "Base Multipler Of Amount"));

        var isMult = property.FindPropertyRelative("isMult");
        var isMultRect = new Rect(position.x + 9 * position.width / 12, position.y + position.height, position.height, position.height);
        EditorGUI.PropertyField(isMultRect, isMult, GUIContent.none);
        EditorGUI.LabelField(isMultRect, new GUIContent("", "Is Exchange Additive Or Multiplicative"));

        var hide = property.FindPropertyRelative("hide");
        var hideRect = new Rect(position.x + 10 * position.width / 12, position.y + position.height, position.height, position.height);
        EditorGUI.PropertyField(hideRect, hide, GUIContent.none);
        EditorGUI.LabelField(hideRect, new GUIContent("", "Hide Exchange In Game"));


        var type = property.FindPropertyRelative("type");
        var typeRect = new Rect(position.x, position.y + 2 * position.height, position.width / 2, position.height);
        EditorGUI.PropertyField(typeRect, type, GUIContent.none);
        EditorGUI.LabelField(typeRect, new GUIContent("", "Type Of Exchange Resource"));

        if (type.enumValueIndex == 2)
        {
            var target = property.FindPropertyRelative("target");
            var targetRect = new Rect(position.x + position.width / 2, position.y + 2 * position.height, position.width / 2, position.height);
            EditorGUI.PropertyField(targetRect, target, GUIContent.none);
            EditorGUI.LabelField(targetRect, new GUIContent("", "Exchange Target \n0 = All, \n1+ = Specific Index"));
        }



        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * 4;
    }
}

#endif
