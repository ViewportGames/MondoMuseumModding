#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

public class TransparentScreenCapture : MonoBehaviour{

    RecorderController m_RecorderController;

    [SerializeField] private string _imageName = "image";
    [SerializeField] private Vector2 _resolution = new Vector2(1920, 1080);

    private bool _isBusy = false;
    public bool IsBusy{
        get {return _isBusy; }
    }
             
    private void OnEnable(){
        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        m_RecorderController = new RecorderController(controllerSettings);

        var mediaOutputFolder = Path.Combine(Application.streamingAssetsPath + "\\ScreenCaptures");

        // // Image
        var imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
        imageRecorder.name = "My Image Recorder";
        imageRecorder.Enabled = true;
        imageRecorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
        imageRecorder.CaptureAlpha = true;
        
        imageRecorder.OutputFile = string.Concat(mediaOutputFolder, "\\" + _imageName + "_" + DefaultWildcard.Take);

        imageRecorder.imageInputSettings = new GameViewInputSettings
        {
            OutputWidth = (int)_resolution.x,
            OutputHeight = (int)_resolution.y,
        };

        // Setup Recording
        controllerSettings.AddRecorderSettings(imageRecorder);
        controllerSettings.SetRecordModeToSingleFrame(0);
    }

    public void Capture(){
        StartCoroutine(CaptureCoroutine());
    }

    private IEnumerator CaptureCoroutine(){
        _isBusy = true;
        m_RecorderController.PrepareRecording();
        m_RecorderController.StartRecording();
        while(m_RecorderController.IsRecording()){
            yield return 0;
        }
        _isBusy = false;
    }
}

 #endif