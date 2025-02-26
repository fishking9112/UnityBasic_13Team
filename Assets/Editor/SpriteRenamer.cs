using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteImporterRenamer : EditorWindow
{
    [MenuItem("Tools/Sprite Importer Renamer")]
    public static void ShowWindow()
    {
        GetWindow<SpriteImporterRenamer>("Sprite Importer Renamer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Rename Sprites Automatically", EditorStyles.boldLabel);

        if (GUILayout.Button("Rename Sprites"))
        {
            RenameSprites();
        }
    }

    private static void RenameSprites()
    {
        string resourcePath = "Assets/Resources/item"; // 스프라이트 폴더 경로
        string[] spriteSheets = Directory.GetFiles(resourcePath, "*.png");

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

            string baseName = Path.GetFileNameWithoutExtension(path); // 예: "1001"
            int baseNumber;
            if (!int.TryParse(baseName, out baseNumber))
            {
                Debug.LogWarning($"Skipping {path} because the filename is not a valid number.");
                continue;
            }

            for (int i = 0; i < spriteSheetProperty.arraySize; i++)
            {
                SerializedProperty element = spriteSheetProperty.GetArrayElementAtIndex(i);
                SerializedProperty nameProperty = element.FindPropertyRelative("m_Name");

                if (nameProperty != null)
                {
                    nameProperty.stringValue = (baseNumber + i).ToString(); // "1001", "1002", "1003"...
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

        Debug.Log("All sprite names updated successfully!");
    }
}
