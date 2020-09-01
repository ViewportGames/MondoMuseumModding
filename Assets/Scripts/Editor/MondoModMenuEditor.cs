using UnityEditor;
using UnityEngine;
using System.IO;

public class MondoModMenuEditor : MonoBehaviour
{
    [MenuItem("Mondo Museum/Setup New Mod Folder", false, 0)]
    public static void CreateNewMod(){
        if(!AssetDatabase.IsValidFolder("Assets/Mods")){
             AssetDatabase.CreateFolder("Assets", "Mods");
        }

        string guid = AssetDatabase.CreateFolder("Assets/Mods", "New Mod");
        string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);

        guid = AssetDatabase.CreateFolder(newFolderPath, "BundledDataContent");
        string bundlePath = AssetDatabase.GUIDToAssetPath(guid);
        AssetDatabase.CreateFolder(bundlePath, "Windows");
        AssetDatabase.CreateFolder(bundlePath, "OSX Universal");

        AssetDatabase.CreateFolder(newFolderPath, "Assets");

        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
    }

    [MenuItem("Mondo Museum/Documentation", false, 15)]
    public static void OpenDocumentation(){
        Application.OpenURL("https://github.com/ViewportGames/MondoMuseumModding/wiki");
    }
    [MenuItem("Mondo Museum/Get Latest Release (Using 0.1)", false, 16)]
    public static void OpenReleases(){
        Application.OpenURL("https://github.com/ViewportGames/MondoMuseumModding/releases");
    }

    [MenuItem ("Assets/Mondo Museum Helpers/Cleanup Bundles")]
    static void CleanBundles(MenuCommand command){
        var path = "";
        foreach(UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets)){
            path = AssetDatabase.GetAssetPath(obj);
            if(!string.IsNullOrEmpty(path) && File.Exists(path)){
                if(IsGarbageAssetBundle(Path.GetFileName(path))){
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }
    }

    static bool IsGarbageAssetBundle(string filename){
        if(filename == "Windows" ||
            filename == "Windows.manifest" ||
            filename == "OSX Universal" ||
            filename == "OSX Universal.manifest" ||
            filename == "common.data" ||
            filename == "common.data.manifest"){
            
            return true;
        }

        return false;
    }

}
