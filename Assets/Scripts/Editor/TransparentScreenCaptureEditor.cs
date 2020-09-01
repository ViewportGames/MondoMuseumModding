using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TransparentScreenCapture), true)]
public class TransparentScreenCaptureEditor : Editor{
    SerializedProperty _resolution;

    TransparentScreenCapture _script;

    private void OnEnable(){
        _script = target as TransparentScreenCapture;

        _resolution = serializedObject.FindProperty("_resolution");
    }

    public override void OnInspectorGUI(){
        serializedObject.Update();

        EditorGUILayout.HelpBox("Save a screenshot of the game view as a .png with transparent background.\n\nImages are in StreamingAssets/ScreenCaptures folder.", MessageType.None);

        if(Application.isPlaying){
            EditorGUI.BeginDisabledGroup(true);
        }
        EditorGUILayout.PropertyField(_resolution);
        if(Application.isPlaying){
            EditorGUI.EndDisabledGroup();
        }

        if(_script.IsBusy || !Application.isPlaying){
            EditorGUI.BeginDisabledGroup(true);
        }
        if(GUILayout.Button("Capture")){
            _script.Capture();
        }
        if(_script.IsBusy || !Application.isPlaying){
            EditorGUI.EndDisabledGroup();
        }

        if(!Application.isPlaying){
            EditorGUILayout.HelpBox("Enter Play Mode to use capture button.", MessageType.Info);
        }
        
        
        serializedObject.ApplyModifiedProperties();

        if(GUI.changed){
            EditorUtility.SetDirty(_script);
        }
    }
}
