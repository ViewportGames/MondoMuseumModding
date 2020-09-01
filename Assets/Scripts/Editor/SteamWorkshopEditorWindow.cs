using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;

public class SteamWorkshopEditorWindow : EditorWindow
{
    /* SETTINGS
    If the Workshop Utility Editor Window is open when you recompile this script
    you'll need to close and reopen it to see certain changes. Don't forget.
    */
    private static int _steamAppId = 1093820;

    /* 
    The only way to get all of the tags that the Workshop Admin has set Visible
    is to create a hidden workshop item that contains those tags. That item is
    then queried and its tags returned as those that the Steam Workshop Utility
    populates as available tag toggles.

    You can add these Visible Tags to your dummy item using its Custom Tags field
    */
    private static int _dummyTagItemId = 2012273427;
    
    /*
    Visibility Dropdown disabled by default because people probably shouldn't
    change this too easily without going to the workshop page.
    */
    private bool _enableVisibilityDropdown = false;

    /*
    When updating a workshop item you will automatically subscribe to it if
    not already, and begin downloading it with high priority.
    */
    private bool _subscribeDownloadOnUpdate = true;

    private static bool _showResultLogs = false;

    /* VARIABLES */
    private static Steamworks.Data.PublishedFileId _dummyTagItemPublishedFileID;
    private static bool _steamClientInitialized = false;
    private static string[] _appIdDefaultTags;
    private int _currentItemIndex;
    private static Dictionary<Steamworks.Ugc.Item, string> _publishedItemsDictionary;
    private string _itemTitle = "";
    private string _itemDescription = "";
    private string _itemChangeNotes = "";
    private static List<string> _itemTagNameList;
    private static List<bool> _itemTagActiveList;
    private string _itemCustomTags = "";
    private Texture _itemPreviewImage = null;
    private string _itemContentPath = "";
    private string[] _itemVisibilityOptions = new string[] {"Private", "Friends Only", "Public"};
    private int _itemVisibilityIndex = 0;

    /* GUI Style Stuff */
    GUIStyle _foldoutStyle;
    GUIStyle _tagStyle;
    GUIStyle _downloadedTextureBoxStyle;
    private Vector2 _editorWindowScrollArea;
    private Vector2 _itemChangeNotesScrollArea;
    private Vector2 _itemDescriptionScrollArea;
    private static bool _deleteItemToggle = false;
    private bool _showTags = true;
    private static bool _canEditItem = false;
    static private bool _isBusy = false;
    private bool _hasUploadRequirementError = false;
    private Texture2D _currentDownloadedPreviewTexture;

    [MenuItem("Window/Steam Workshop Utility")]
    static void Init(){
        _canEditItem = false;

        SteamWorkshopEditorWindow window = EditorWindow.GetWindow<SteamWorkshopEditorWindow>("Steam Workshop Utility", (typeof(SceneView)));
        window.minSize = new Vector2(500f, 500f);
        window.Show();
    }

    protected virtual void OnEnable(){
        _publishedItemsDictionary = new Dictionary<Steamworks.Ugc.Item, string>();
        _itemTagNameList = new List<string>();
        _itemTagActiveList = new List<bool>();
        _dummyTagItemPublishedFileID = new Steamworks.Data.PublishedFileId();
        _dummyTagItemPublishedFileID.Value = (ulong)_dummyTagItemId;
        
        InitializeSteamClient();
    }

    protected virtual void OnDisable(){
        ShutdownSteamClient();
    }

    static void InitializeSteamClient(){
        try{
            SteamClient.Init((uint)_steamAppId, true);
        }
        catch(System.Exception e){
            Debug.LogWarning(e.Message);
            _steamClientInitialized = false;
            return;
        }
        _steamClientInitialized = true;
        Debug.Log("Initialized Steam Client");

        _ = GetDummyTagItem();
        _ = GetWorkshopItems();
    }

    private void ShutdownSteamClient(){
        SteamClient.Shutdown();
        _steamClientInitialized = false;
        CloseEditSection();
        _publishedItemsDictionary.Clear();
        Debug.Log("Shutdown Steam Client");
    }

    void OnInspectorUpdate(){
        Repaint();
    }

    protected virtual void OnGUI(){
        _hasUploadRequirementError = false;

        //EditorStyles wasn't being found in OnEnable so these null check initializations are needed
        if(_downloadedTextureBoxStyle == null){
            _downloadedTextureBoxStyle = new GUIStyle();
            _downloadedTextureBoxStyle.padding = new RectOffset(3, 0, 2, -2);
        }
        if(_foldoutStyle == null){
            _foldoutStyle = new GUIStyle(EditorStyles.foldout);
            _foldoutStyle.stretchWidth = false;
        }
        if(_tagStyle == null){
            _tagStyle = new GUIStyle(EditorStyles.label);
            _tagStyle.padding = new RectOffset(103, 0, 1, 0);
            _tagStyle.stretchWidth = false;
        }
        EditorStyles.textField.wordWrap = true;

        // _editorWindowScrollArea = EditorGUILayout.BeginScrollView (_editorWindowScrollArea,
        //                                     false,
        //                                     false,
        //                                     GUILayout.Width(Screen.width),
        //                                     GUILayout.Height(Screen.height - 20));

        WorkshopInitializationOnGUI();
        WorkshopItemSelectionOnGUI();
        if(_canEditItem){
            WorkshopItemEditOnGUI();
        }

        EditorGUILayout.Separator();
        //EditorGUILayout.EndScrollView();
    }

    private void WorkshopInitializationOnGUI(){
        if(!_steamClientInitialized){
            EditorGUILayout.HelpBox("Unable to connect to Steam Client with the provided App Id.\nIf the App Id is correct, Steam may not be open.", MessageType.Error);
        } 

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("Steam App Id: " + _steamAppId.ToString());

        if(_steamAppId != 0){
            if(GUILayout.Button("Open Steam Workshop", GUILayout.ExpandWidth(false))){
                Application.OpenURL("steam://openurl/https://steamcommunity.com/app/" + _steamAppId.ToString() + "/workshop/");
            }
            if(GUILayout.Button("Refresh", GUILayout.ExpandWidth(false))){
                CloseEditSection();
                if(_steamClientInitialized){
                    _ = GetWorkshopItems();
                }
                else{
                    InitializeSteamClient();
                }
            } 
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
    }

    private void WorkshopItemSelectionOnGUI(){
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Your Workshop Items");

        if(_publishedItemsDictionary.Count == 0 || _canEditItem || _isBusy){
            EditorGUI.BeginDisabledGroup(true);
        }
        _currentItemIndex = EditorGUILayout.Popup(_currentItemIndex, _publishedItemsDictionary.Values.ToArray());        
       
        if(GUILayout.Button("Edit", GUILayout.ExpandWidth(false))){
            SyncItemInfo();
            _canEditItem = true;
            EditorCoroutineUtility.StartCoroutine(DownloadItemPreviewTexture(), this);
        }
        if(_publishedItemsDictionary.Count != 0){
            if(GUILayout.Button("View", GUILayout.ExpandWidth(false))){
                Application.OpenURL("steam://openurl/https://steamcommunity.com/sharedfiles/filedetails/?id=" + _publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Id);
            }
        }
        if(_publishedItemsDictionary.Count == 0 || _canEditItem || _isBusy){
            EditorGUI.EndDisabledGroup();
        }
        
        if(_canEditItem || _isBusy){
            EditorGUI.BeginDisabledGroup(true);
        }
        if(GUILayout.Button(new GUIContent("+", "Create a new item on the Mondo Museum Workshop. It will be hidden by default and empty until you edit and update it."), GUILayout.ExpandWidth(false))){
            _ = CreateNewWorkshopItem();
        }
        if(_canEditItem || _isBusy){
            EditorGUI.EndDisabledGroup();
        }

        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }

    private IEnumerator DownloadItemPreviewTexture(){
        if(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).PreviewImageUrl == ""){
            _currentDownloadedPreviewTexture = null;
            yield break;
        }
        UnityWebRequest request = UnityWebRequest.Get(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).PreviewImageUrl);
        request.downloadHandler = new DownloadHandlerTexture();
        yield return request.SendWebRequest();
        _currentDownloadedPreviewTexture = DownloadHandlerTexture.GetContent(request);
    }

    private void WorkshopItemEditOnGUI(){
        EditorGUILayout.Separator();
        if(_isBusy){
            EditorGUI.BeginDisabledGroup(true);
        }

        EditorGUILayout.Separator();
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Title:\n" + System.Text.Encoding.UTF8.GetBytes(_itemTitle).Count() + "/129", GUILayout.Width(145));
        EditorGUI.BeginChangeCheck();
        _itemTitle = EditorGUILayout.TextField(_itemTitle);
        if(EditorGUI.EndChangeCheck()){
            if(System.Text.Encoding.UTF8.GetBytes(_itemTitle).Count() > 129){
                _itemTitle = _itemTitle.Substring(0, 129);
                GUI.FocusControl(null);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Description:\n" + System.Text.Encoding.UTF8.GetBytes(_itemDescription).Count() + "/8000", GUILayout.Width(143));
        _itemDescriptionScrollArea = EditorGUILayout.BeginScrollView(_itemDescriptionScrollArea, false, false);
        
        EditorGUI.BeginChangeCheck();
        _itemDescription = EditorGUILayout.TextArea(_itemDescription, GUILayout.ExpandHeight(true));
       if(EditorGUI.EndChangeCheck()){
            if(System.Text.Encoding.UTF8.GetBytes(_itemDescription).Count() > 8000){
                _itemDescription = _itemDescription.Substring(0, 8000);
                GUI.FocusControl(null);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        _showTags = EditorGUILayout.Foldout(_showTags, "Tags: ", _foldoutStyle);
        var customTags = "";
        if(_itemCustomTags != ""){
            customTags = ", " + _itemCustomTags;
        }
        GUILayout.Label(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(GetActiveTagsAsLabelString() + customTags), _tagStyle);
        EditorGUILayout.EndHorizontal();

        if(_showTags){
            EditorGUI.indentLevel++;
            for(int i = 0; i < _itemTagNameList.Count; i++){
                _itemTagActiveList[i] = EditorGUILayout.Toggle(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_itemTagNameList[i]), _itemTagActiveList[i]);
            }
            _itemCustomTags = EditorGUILayout.TextField("Custom Tags: ", _itemCustomTags);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Thumbnail Image: ");
        if(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).PreviewImageUrl != ""){
            GUILayout.Box(_currentDownloadedPreviewTexture, _downloadedTextureBoxStyle, GUILayout.Width(64), GUILayout.Height(64));
        }
        _itemPreviewImage = (Texture)EditorGUILayout.ObjectField(_itemPreviewImage, typeof(Texture), false, GUILayout.Width(61), GUILayout.Height(61));

        EditorGUILayout.BeginVertical();
        EditorGUILayout.HelpBox("File Size: Less than 1MB\nFormat: .jpg or .png\nImage Size: 1:1 square aspect ratio", MessageType.Info);
        if(_itemPreviewImage != null){
            if(!File.Exists(AssetDatabase.GetAssetPath(_itemPreviewImage))){
                _itemPreviewImage = null;
            }
            else{
                var fileInfo = new System.IO.FileInfo(Path.GetFullPath(AssetDatabase.GetAssetPath(_itemPreviewImage)));
                var fileSize = System.Math.Round(((float)fileInfo.Length * 0.0009765625f) / 1000f, 2);
                var fileExtension = Path.GetExtension(AssetDatabase.GetAssetPath(_itemPreviewImage));
                if(fileExtension != ".png" && fileExtension != ".jpg"){
                    EditorGUILayout.HelpBox("Preview Image file must be .jpg or .png. Selected image is " + fileExtension, MessageType.Error);
                    _hasUploadRequirementError = true;
                }
                
                if(fileSize > 1f){
                    EditorGUILayout.HelpBox("Thumbnail Image file size must be less than 1MB. Selected image is " + fileSize + "MB", MessageType.Error);
                    _hasUploadRequirementError = true;
                }
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Separator();

        _itemContentPath = EditorGUILayout.TextField("Content Path: ", _itemContentPath);

        if(_enableVisibilityDropdown){
            _itemVisibilityIndex = EditorGUILayout.Popup("Workshop Visibility: ", _itemVisibilityIndex, _itemVisibilityOptions, GUILayout.MaxWidth(260));
        }

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Change Notes: ");
        _itemChangeNotesScrollArea = EditorGUILayout.BeginScrollView(_itemChangeNotesScrollArea, false, false, GUILayout.Height(60));
        _itemChangeNotes = EditorGUILayout.TextArea(_itemChangeNotes, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        GUIStyle textLinkGUIStyle = new GUIStyle(GUI.skin.label);
        textLinkGUIStyle.padding = new RectOffset(-6, -6, 1, 0);
        textLinkGUIStyle.richText = true;
        GUILayout.Label("By submitting this item, you agree to the ", GUILayout.ExpandWidth(false));
        if(GUILayout.Button("<b><color=blue>workshop terms of service</color></b>", textLinkGUIStyle, GUILayout.ExpandWidth(false))){
            Application.OpenURL("steam://openurl/http://steamcommunity.com/sharedfiles/workshoplegalagreement");
        }
        GUILayout.Label(".", GUILayout.ExpandWidth(false));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if(_hasUploadRequirementError){
            EditorGUI.BeginDisabledGroup(true);
        }
        if(GUILayout.Button("Update Workshop Item", GUILayout.ExpandWidth(false))){
            _ = UpdateCurrentWorkshopItem();
        }
        if(_hasUploadRequirementError){
            EditorGUI.EndDisabledGroup();
        }
        if(GUILayout.Button("Cancel", GUILayout.ExpandWidth(false))){
            CloseEditSection();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        _deleteItemToggle = EditorGUILayout.ToggleLeft("Delete?", _deleteItemToggle);
        if(_deleteItemToggle){
            EditorGUILayout.HelpBox("No way! This can't be undone, so you should do it on the item's Steam Workshop page if you're serious (Click the \"View\" button)", MessageType.Warning);
        }

        if(_isBusy){
            EditorGUI.EndDisabledGroup();
        }
    }

    private void SyncItemInfo(){
        _itemTitle = _publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Title;
        _itemDescription = _publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Description;
        _itemCustomTags = "";

        var tags = _publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Tags;
        for(int i = 0; i < tags.Length; i++){
            for(int j = 0; j < _itemTagNameList.Count; j++){
                if(tags[i] == _itemTagNameList[j].ToLower()){
                    _itemTagActiveList[j] = true;
                    break;
                }
            }
        }

        foreach(string tag in tags){
            var isCustomTag = true;
            foreach(string defaultTag in _itemTagNameList){
                if(tag == defaultTag.ToLower()){
                    isCustomTag = false;
                    break;
                }
            }
            if(!isCustomTag){
                continue;
            }
            _itemCustomTags += tag + ", ";
        }
        if(_itemCustomTags != ""){
            _itemCustomTags = _itemCustomTags.Substring(0, _itemCustomTags.Length-2);
        }

        _itemChangeNotes = "";
        _itemPreviewImage = null;
        _itemContentPath = "";

        if(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).IsPrivate){
            _itemVisibilityIndex = 0;
        }
        else if(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).IsFriendsOnly){
            _itemVisibilityIndex = 1;
        }
        else if(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).IsPublic){
            _itemVisibilityIndex = 2;
        }
    }

    private static async Task GetWorkshopItems(){
        Debug.Log("Begin <i>GetWorkshopItems()</i>");
        _isBusy = true;

        _publishedItemsDictionary.Clear();

        var query = Steamworks.Ugc.Query.Items.WhereUserPublished().WithLongDescription(true);
        var page = await query.GetPageAsync(1);

        foreach(Steamworks.Ugc.Item item in page.Value.Entries){
            _publishedItemsDictionary.Add(item, item.Title + " (" + item.Id + ")");
            Debug.Log("<b>Found Workshop Item:</b> " + item.Title + " (" + item.Id + ")");
        }

        if(_publishedItemsDictionary.Count == 0){
            Debug.Log("No Existing Workshop Items Found.");
        }

        _isBusy = false;
    }

    private static async Task GetDummyTagItem(){
        Debug.Log("Begin <i>GetDummyTagItem()</i>");
        _isBusy = true;

        var query = Steamworks.Ugc.Query.Items.WithFileId(_dummyTagItemPublishedFileID);
        var page = await query.GetPageAsync(1);

        foreach(Steamworks.Ugc.Item item in page.Value.Entries){
            _appIdDefaultTags = item.Tags;
            Debug.Log("<b>Found Dummy Tag Item:</b> " + item.Title + " (" + item.Id + ")");
        }
        if(_publishedItemsDictionary.Count == 0){
            Debug.Log("No Dummy Tag Item Found.");
        }

        _isBusy = false;

        foreach(string tag in _appIdDefaultTags){
            _itemTagNameList.Add(tag);
        }
        foreach(string tag in _itemTagNameList){
            _itemTagActiveList.Add(false);
        }
    }

    private async Task CreateNewWorkshopItem(){
        Debug.Log("Begin <i>CreateNewWorkshopItem()</i>");
        _isBusy = true;

        var createResult = await Steamworks.Ugc.Editor.NewCommunityFile
                                .WithTitle("New Mod")
                                .WithPrivateVisibility()
                                .SubmitAsync();
       
        if (_showResultLogs) Debug.Log("<b><i>CreateNewWorkshopItem()</i> Result:</b> " + createResult.ToString());
        if(createResult.Success){
            Debug.Log("<b>SUCCESS</b> -- <i>CreateNewWorkshopItem()</i> " + createResult.FileId);
        }

        _isBusy = false;

        await GetWorkshopItems();
    }
    private async Task UpdateCurrentWorkshopItem(){
        Debug.Log("Begin <i>UpdateCurrentWorkshopItem()</i>: " + _publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Title +  " (" + _publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Id + ")");
        _isBusy = true;

        var editor = new Steamworks.Ugc.Editor(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Id);
        editor = editor.WithTitle(_itemTitle);
        editor = editor.WithDescription(_itemDescription);

        var hasTag = false;
        for(int i = 0; i < _itemTagActiveList.Count; i++){
            if(_itemTagActiveList[i]){
                editor = editor.WithTag(_itemTagNameList[i]);
                hasTag = true;
            }
        }
        if(_itemCustomTags != ""){
            hasTag = true;
            string[] customTags = _itemCustomTags.Split(',').Select(p => p.Trim()).ToArray();
            foreach(string customTag in customTags){
                Debug.Log("split " + customTag);
                editor = editor.WithTag(customTag);
            }
        }

        if(!hasTag){
            editor = editor.WithTag("");
        }

        if(_itemPreviewImage != null){
            var fullImagePreviewPath = Path.GetFullPath(AssetDatabase.GetAssetPath(_itemPreviewImage));
            editor = editor.WithPreviewFile(fullImagePreviewPath);
        }
        
        if(_itemContentPath != ""){
            var fullContentPath = Path.GetFullPath(_itemContentPath);
            Debug.Log("Content: " + fullContentPath);
            if(Directory.Exists(fullContentPath)){
                editor = editor.WithContent(fullContentPath);
            }
        }

        if(_enableVisibilityDropdown){
            if(_itemVisibilityIndex == 0){
                editor = editor.WithPrivateVisibility();
            }
            else if(_itemVisibilityIndex == 1){
                editor = editor.WithFriendsOnlyVisibility();
            }
            else if(_itemVisibilityIndex == 3){
                editor = editor.WithPublicVisibility();
            }
        }
        else{
            if(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).IsPrivate){
                editor = editor.WithPrivateVisibility();
            }
            else if(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).IsFriendsOnly){
                editor = editor.WithFriendsOnlyVisibility();
            }
            else if(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).IsPublic){
                editor = editor.WithPublicVisibility();
            }
        }

        var editResult = await editor.SubmitAsync();
        if (_showResultLogs) Debug.Log("<b><i>UpdateCurrentWorkshopItem()</i> Result:</b> " + editResult.Result.ToString());
        if(editResult.Result == Result.OK){
            Debug.Log("<b>SUCCESS</b> -- <i>UpdateCurrentWorkshopItem()</i>: " + _publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Title +  " (" + _publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Id + ")");
            
            if(_subscribeDownloadOnUpdate){
                if(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).IsSubscribed){
                    BeginDownloadWorkshopItem(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex), true);
                }
                else{
                    _ = SubscribeToWorkshopItem(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex));                
                }
            }

            Application.OpenURL("steam://openurl/https://steamcommunity.com/sharedfiles/filedetails/?id=" + _publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Id);
        }
        CloseEditSection();

        _isBusy = false;

        await GetWorkshopItems();
    }

    private async Task SubscribeToWorkshopItem(Steamworks.Ugc.Item item){
        Debug.Log("Begin <i>SubscribeToWorkshopItem()</i>: " + item.Title +  " (" + item.Id + ")");

        var subscribed = await item.Subscribe();

        if (_showResultLogs) Debug.Log("<b><i>SubscribeToWorkshopItem()</i></b> Result: " + subscribed);

        if(subscribed){
            Debug.Log("<b>SUCCESS</b> -- <i>SubscribeToWorkshopItem()</i>: " + item.Title +  " (" + item.Id + ")");
            BeginDownloadWorkshopItem(item, true);
        }
    }

    private void BeginDownloadWorkshopItem(Steamworks.Ugc.Item item, bool highPriority){
        var status = item.Download(highPriority);

        Debug.Log("<b><i>BeginDownloadWorkshopItem()</i></b> Is Downloading: " + status);
    }

    private async Task DeleteCurrentWorkshopItem(){
        Debug.Log("Begin <i>DeleteCurrentWorkshopItem()</i>: " + _publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Title +  " (" + _publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Id + ")");
        _isBusy = true;
        
        var deleted = await SteamUGC.DeleteFileAsync(_publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Id);

        if (_showResultLogs) Debug.Log("<b><i>DeleteCurrentWorkshopItem()</i></b> Result: " + deleted);

        if(deleted){
             Debug.Log("<b>SUCCESS</b> -- <i>DeleteCurrentWorkshopItem()</i>: " + _publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Title +  " (" + _publishedItemsDictionary.Keys.ElementAt(_currentItemIndex).Id + ")");
             CloseEditSection();
             _currentItemIndex = 0;
        }

        _isBusy = false;

        await GetWorkshopItems();
    }

    private string GetActiveTagsAsLabelString(){
        string tags = "";

        for(int i = 0; i < _itemTagActiveList.Count; i++){
            if(_itemTagActiveList[i]){
                tags += _itemTagNameList[i] + ", ";
            }
        }

        if(tags != ""){
            tags = tags.Substring(0, tags.Length-2);
        }
        
        return tags;
    }

    private static void CloseEditSection(){
        _canEditItem = false;
        _deleteItemToggle = false;
        GUI.FocusControl(null);
    }
}
