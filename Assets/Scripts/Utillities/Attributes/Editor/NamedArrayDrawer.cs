using Phoder1.Reflection;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Phoder1.Editor
{
    [CustomPropertyDrawer(typeof(NamedArrayAttribute))]
    public class NamedArrayDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            //if (!property.isArray)

            if (!property.IsArrayElement())
            {
                EditorGUI.PropertyField(rect, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(rect, label, property);

            var att = attribute as NamedArrayAttribute;
            int index = property.GetArrayPropertyIndex();
            var targets = this.GetTarget(property) as IList;

            if (targets == null)
            {
                EditorGUI.PropertyField(rect, property, label, true);
            }
            else
            {
                var target = targets[index];
                if (target == null || !target.TryGetMemberValue(att.ElementName, out string value, false) || string.IsNullOrWhiteSpace(value))
                {
                    EditorGUI.PropertyField(rect, property, label, true);
                }
                else
                {
                    EditorGUI.PropertyField(rect, property, new GUIContent(value), true);
                }
            }
            EditorGUI.EndProperty();
        }
    }
}