using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InGamePlayerManager))]
public class InGamePlayerManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        InGamePlayerManager manager = (InGamePlayerManager)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("테스트 데미지 버튼", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("데미지 5"))
        {
            manager.ApplyDamage5();
        }
        
        if (GUILayout.Button("데미지 10"))
        {
            manager.ApplyDamage10();
        }
        
        if (GUILayout.Button("데미지 50"))
        {
            manager.ApplyDamage50();
        }
        EditorGUILayout.EndHorizontal();
    }
} 