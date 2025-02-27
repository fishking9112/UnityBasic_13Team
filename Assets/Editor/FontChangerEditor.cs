using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FontChangerEditor : EditorWindow
{
    private Font uiFont;
    private TMP_FontAsset tmpFont;
    private bool includeInactive = true;
    private bool preserveFontSize = true;
    private bool preserveFontStyle = true;
    private bool preserveColor = true;
    private bool showDebugLogs = true;
    
    private Vector2 scrollPosition;
    private List<Text> previewUITexts = new List<Text>();
    private List<TextMeshProUGUI> previewTMPTexts = new List<TextMeshProUGUI>();
    private bool showPreview = false;
    
    [MenuItem("Tools/Font Changer")]
    public static void ShowWindow()
    {
        GetWindow<FontChangerEditor>("Font Changer");
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("Font Changer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Font Settings", EditorStyles.boldLabel);
        uiFont = (Font)EditorGUILayout.ObjectField("UI Font", uiFont, typeof(Font), false);
        tmpFont = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP Font", tmpFont, typeof(TMP_FontAsset), false);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        includeInactive = EditorGUILayout.Toggle("Include Inactive Objects", includeInactive);
        preserveFontSize = EditorGUILayout.Toggle("Preserve Font Size", preserveFontSize);
        preserveFontStyle = EditorGUILayout.Toggle("Preserve Font Style", preserveFontStyle);
        preserveColor = EditorGUILayout.Toggle("Preserve Color", preserveColor);
        showDebugLogs = EditorGUILayout.Toggle("Show Debug Logs", showDebugLogs);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Find Texts in Scene"))
        {
            FindTextsInScene();
        }
        
        EditorGUILayout.Space();
        
        GUI.enabled = (uiFont != null || tmpFont != null);
        
        if (GUILayout.Button("Change All Fonts in Scene"))
        {
            ChangeAllFonts();
        }
        
        if (GUILayout.Button("Change Fonts in Selected Canvas"))
        {
            ChangeSelectedCanvasFonts();
        }
        
        if (GUILayout.Button("Change Fonts in Selected Objects"))
        {
            ChangeSelectedObjectsFonts();
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        
        showPreview = EditorGUILayout.Foldout(showPreview, "Preview Found Texts");
        
        if (showPreview)
        {
            EditorGUI.indentLevel++;
            
            if (previewUITexts.Count > 0 || previewTMPTexts.Count > 0)
            {
                if (previewUITexts.Count > 0)
                {
                    EditorGUILayout.LabelField($"UI Texts ({previewUITexts.Count}):", EditorStyles.boldLabel);
                    foreach (var text in previewUITexts)
                    {
                        if (text != null)
                        {
                            EditorGUILayout.ObjectField(text.gameObject.name, text, typeof(Text), true);
                        }
                    }
                }
                
                if (previewTMPTexts.Count > 0)
                {
                    EditorGUILayout.LabelField($"TextMeshPro Texts ({previewTMPTexts.Count}):", EditorStyles.boldLabel);
                    foreach (var text in previewTMPTexts)
                    {
                        if (text != null)
                        {
                            EditorGUILayout.ObjectField(text.gameObject.name, text, typeof(TextMeshProUGUI), true);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No texts found. Click 'Find Texts in Scene' to locate texts.", MessageType.Info);
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void FindTextsInScene()
    {
        previewUITexts.Clear();
        previewTMPTexts.Clear();
        
        // Unity UI Text 컴포넌트 찾기
        Text[] texts = includeInactive 
            ? Resources.FindObjectsOfTypeAll<Text>() 
            : Object.FindObjectsOfType<Text>();
        
        foreach (Text text in texts)
        {
            if (text.gameObject.scene.isLoaded)
            {
                previewUITexts.Add(text);
            }
        }
        
        // TextMeshPro 컴포넌트 찾기
        TextMeshProUGUI[] tmpTexts = includeInactive 
            ? Resources.FindObjectsOfTypeAll<TextMeshProUGUI>() 
            : Object.FindObjectsOfType<TextMeshProUGUI>();
        
        foreach (TextMeshProUGUI tmpText in tmpTexts)
        {
            if (tmpText.gameObject.scene.isLoaded)
            {
                previewTMPTexts.Add(tmpText);
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"Found {previewUITexts.Count} UI Texts and {previewTMPTexts.Count} TextMeshPro Texts in the scene.");
        }
        
        showPreview = true;
    }
    
    private void ChangeAllFonts()
    {
        int uiTextCount = 0;
        int tmpTextCount = 0;
        
        // Unity UI Text 컴포넌트 변경
        if (uiFont != null)
        {
            Text[] texts = includeInactive 
                ? Resources.FindObjectsOfTypeAll<Text>() 
                : Object.FindObjectsOfType<Text>();
            
            foreach (Text text in texts)
            {
                if (text.gameObject.scene.isLoaded)
                {
                    Undo.RecordObject(text, "Change Font");
                    ChangeUITextFont(text);
                    uiTextCount++;
                    EditorUtility.SetDirty(text);
                }
            }
        }
        
        // TextMeshPro 컴포넌트 변경
        if (tmpFont != null)
        {
            TextMeshProUGUI[] tmpTexts = includeInactive 
                ? Resources.FindObjectsOfTypeAll<TextMeshProUGUI>() 
                : Object.FindObjectsOfType<TextMeshProUGUI>();
            
            foreach (TextMeshProUGUI tmpText in tmpTexts)
            {
                if (tmpText.gameObject.scene.isLoaded)
                {
                    Undo.RecordObject(tmpText, "Change Font");
                    ChangeTMPTextFont(tmpText);
                    tmpTextCount++;
                    EditorUtility.SetDirty(tmpText);
                }
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"폰트 변경 완료: UI Text {uiTextCount}개, TextMeshPro {tmpTextCount}개");
        }
    }
    
    private void ChangeSelectedCanvasFonts()
    {
        Canvas canvas = Selection.activeGameObject?.GetComponent<Canvas>();
        
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Font Changer", "Please select a GameObject with a Canvas component.", "OK");
            return;
        }
        
        int uiTextCount = 0;
        int tmpTextCount = 0;
        
        // Unity UI Text 컴포넌트 변경
        if (uiFont != null)
        {
            Text[] texts = canvas.GetComponentsInChildren<Text>(includeInactive);
            foreach (Text text in texts)
            {
                Undo.RecordObject(text, "Change Font");
                ChangeUITextFont(text);
                uiTextCount++;
                EditorUtility.SetDirty(text);
            }
        }
        
        // TextMeshPro 컴포넌트 변경
        if (tmpFont != null)
        {
            TextMeshProUGUI[] tmpTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>(includeInactive);
            foreach (TextMeshProUGUI tmpText in tmpTexts)
            {
                Undo.RecordObject(tmpText, "Change Font");
                ChangeTMPTextFont(tmpText);
                tmpTextCount++;
                EditorUtility.SetDirty(tmpText);
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"캔버스 '{canvas.name}'의 폰트 변경 완료: UI Text {uiTextCount}개, TextMeshPro {tmpTextCount}개");
        }
    }
    
    private void ChangeSelectedObjectsFonts()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Font Changer", "Please select at least one GameObject.", "OK");
            return;
        }
        
        int uiTextCount = 0;
        int tmpTextCount = 0;
        
        foreach (GameObject obj in selectedObjects)
        {
            // Unity UI Text 컴포넌트 변경
            if (uiFont != null)
            {
                Text[] texts = obj.GetComponentsInChildren<Text>(includeInactive);
                foreach (Text text in texts)
                {
                    Undo.RecordObject(text, "Change Font");
                    ChangeUITextFont(text);
                    uiTextCount++;
                    EditorUtility.SetDirty(text);
                }
            }
            
            // TextMeshPro 컴포넌트 변경
            if (tmpFont != null)
            {
                TextMeshProUGUI[] tmpTexts = obj.GetComponentsInChildren<TextMeshProUGUI>(includeInactive);
                foreach (TextMeshProUGUI tmpText in tmpTexts)
                {
                    Undo.RecordObject(tmpText, "Change Font");
                    ChangeTMPTextFont(tmpText);
                    tmpTextCount++;
                    EditorUtility.SetDirty(tmpText);
                }
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"선택한 오브젝트의 폰트 변경 완료: UI Text {uiTextCount}개, TextMeshPro {tmpTextCount}개");
        }
    }
    
    private void ChangeUITextFont(Text text)
    {
        if (text == null || uiFont == null) return;
        
        // 기존 속성 저장
        int fontSize = text.fontSize;
        FontStyle fontStyle = text.fontStyle;
        Color color = text.color;
        
        // 폰트 변경
        text.font = uiFont;
        
        // 속성 복원
        if (preserveFontSize) text.fontSize = fontSize;
        if (preserveFontStyle) text.fontStyle = fontStyle;
        if (preserveColor) text.color = color;
        
        if (showDebugLogs)
        {
            Debug.Log($"UI Text 폰트 변경: {text.gameObject.name}");
        }
    }
    
    private void ChangeTMPTextFont(TextMeshProUGUI tmpText)
    {
        if (tmpText == null || tmpFont == null) return;
        
        // 기존 속성 저장
        float fontSize = tmpText.fontSize;
        FontStyles fontStyle = tmpText.fontStyle;
        Color color = tmpText.color;
        
        // 폰트 변경
        tmpText.font = tmpFont;
        
        // 속성 복원
        if (preserveFontSize) tmpText.fontSize = fontSize;
        if (preserveFontStyle) tmpText.fontStyle = fontStyle;
        if (preserveColor) tmpText.color = color;
        
        if (showDebugLogs)
        {
            Debug.Log($"TMP Text 폰트 변경: {tmpText.gameObject.name}");
        }
    }
} 