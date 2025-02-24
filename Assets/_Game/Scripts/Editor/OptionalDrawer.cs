using UnityEditor;
using UnityEngine;

//  OutlineFx © NullTale - https://twitter.com/NullTale/
namespace Utils.Outline.Editor
{
    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalDrawer : PropertyDrawer
    {
        private const float k_ToggleWidth = 18;

        // =======================================================================
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var valueProperty = property.FindPropertyRelative("value");
            return EditorGUI.GetPropertyHeight(valueProperty);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProperty   = property.FindPropertyRelative("value");
            var enabledProperty = property.FindPropertyRelative("enabled");

            OnGui(position, label, enabledProperty, valueProperty);
        }

        public static void OnGui(Rect position, GUIContent label, SerializedProperty enabledProperty, SerializedProperty valueProperty)
        {
            position.width -= k_ToggleWidth;
            using (new EditorGUI.DisabledGroupScope(!enabledProperty.boolValue))
                EditorGUI.PropertyField(position, valueProperty, label, true);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var togglePos = new Rect(position.x + position.width + EditorGUIUtility.standardVerticalSpacing, position.y, k_ToggleWidth, EditorGUIUtility.singleLineHeight);
            enabledProperty.boolValue = EditorGUI.Toggle(togglePos, GUIContent.none, enabledProperty.boolValue);

            EditorGUI.indentLevel = indent;
        }
    }
}