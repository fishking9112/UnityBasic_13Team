using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteImporterRenamer : EditorWindow
{
    private string resourcePath = "Assets/Resources/item"; // 기본 스프라이트 폴더 경로
    private string namePrefix = ""; // 이름 접두사 (예: "ID_")
    private bool useNumbering = true; // 번호 사용 여부
    private int startNumber = 1; // 시작 번호
    private int digitCount = 3; // 자릿수 (예: 3 -> 001, 002, ...)
    private string separator = "_"; // 구분자 (예: "_")

    [MenuItem("Tools/Sprite Importer Renamer")]
    public static void ShowWindow()
    {
        GetWindow<SpriteImporterRenamer>("Sprite Importer Renamer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Rename Sprites Automatically", EditorStyles.boldLabel);

        // 폴더 경로 입력 필드
        EditorGUILayout.BeginHorizontal();
        resourcePath = EditorGUILayout.TextField("Sprite Folder Path", resourcePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Sprite Folder", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // 프로젝트 경로에 상대적인 경로로 변환
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    resourcePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    Debug.LogWarning("Please select a folder inside the Assets directory.");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        // 이름 형식 설정
        namePrefix = EditorGUILayout.TextField("Name Prefix", namePrefix);
        useNumbering = EditorGUILayout.Toggle("Use Numbering", useNumbering);
        
        if (useNumbering)
        {
            EditorGUI.indentLevel++;
            startNumber = EditorGUILayout.IntField("Start Number", startNumber);
            digitCount = EditorGUILayout.IntSlider("Digit Count", digitCount, 1, 6);
            separator = EditorGUILayout.TextField("Separator", separator);
            EditorGUI.indentLevel--;
        }

        // 미리보기 표시
        GUILayout.Space(10);
        GUILayout.Label("Preview: " + GetNamePreview(0), EditorStyles.boldLabel);
        GUILayout.Label("Preview: " + GetNamePreview(1), EditorStyles.boldLabel);
        GUILayout.Label("Preview: " + GetNamePreview(2), EditorStyles.boldLabel);

        GUILayout.Space(10);
        if (GUILayout.Button("Rename Sprites"))
        {
            RenameSprites();
        }
    }

    private string GetNamePreview(int index)
    {
        if (useNumbering)
        {
            string format = new string('0', digitCount);
            return namePrefix + (string.IsNullOrEmpty(separator) ? "" : separator) + (startNumber + index).ToString(format);
        }
        else
        {
            return namePrefix + (string.IsNullOrEmpty(namePrefix) ? "" : "_") + Path.GetFileNameWithoutExtension(resourcePath) + "_" + index;
        }
    }

    private void RenameSprites()
    {
        if (!Directory.Exists(resourcePath))
        {
            EditorUtility.DisplayDialog("Error", "The specified folder does not exist: " + resourcePath, "OK");
            return;
        }

        string[] spriteSheets = Directory.GetFiles(resourcePath, "*.png");

        if (spriteSheets.Length == 0)
        {
            EditorUtility.DisplayDialog("Warning", "No PNG files found in the specified folder.", "OK");
            return;
        }

        int totalRenamed = 0;

        foreach (string path in spriteSheets)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            SerializedObject serializedImporter = new SerializedObject(importer);
            SerializedProperty spriteSheetProperty = serializedImporter.FindProperty("m_SpriteSheet.m_Sprites");

            if (spriteSheetProperty == null || spriteSheetProperty.arraySize == 0)
            {
                Debug.LogWarning($"No sprites found in {path}");
                continue;
            }

            string baseName = Path.GetFileNameWithoutExtension(path);
            
            for (int i = 0; i < spriteSheetProperty.arraySize; i++)
            {
                SerializedProperty element = spriteSheetProperty.GetArrayElementAtIndex(i);
                SerializedProperty nameProperty = element.FindPropertyRelative("m_Name");

                if (nameProperty != null)
                {
                    if (useNumbering)
                    {
                        string format = new string('0', digitCount);
                        nameProperty.stringValue = namePrefix + (string.IsNullOrEmpty(separator) ? "" : separator) + (startNumber + i).ToString(format);
                    }
                    else
                    {
                        nameProperty.stringValue = namePrefix + (string.IsNullOrEmpty(namePrefix) ? "" : "_") + baseName + "_" + i;
                    }
                    totalRenamed++;
                }
            }

            serializedImporter.ApplyModifiedProperties();

            // 강제 적용
            AssetDatabase.WriteImportSettingsIfDirty(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            Debug.Log($"Renamed {spriteSheetProperty.arraySize} sprites in {baseName}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Successfully renamed {totalRenamed} sprites!", "OK");
    }
}
