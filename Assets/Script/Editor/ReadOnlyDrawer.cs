using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 이전 GUI 상태 저장
        bool previousGUIState = GUI.enabled;
        
        // GUI를 비활성화하여 읽기 전용으로 만듦
        GUI.enabled = false;
        
        // 프로퍼티 필드 그리기
        EditorGUI.PropertyField(position, property, label);
        
        // GUI 상태 복원
        GUI.enabled = previousGUIState;
    }
} 