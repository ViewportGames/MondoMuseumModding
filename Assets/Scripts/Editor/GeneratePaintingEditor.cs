using UnityEngine;
using UnityEditor;
using System.IO;

public class GeneratePaintingEditorWindow : EditorWindow{

    private Texture _texture = null;
    private float _meshScaleMultiplier = 1f;
    private Object _generatedMesh;

    private float _originalItemWidth;
    private float _originalItemHeight;

    private float _generatedItemWidth;
    private float _generatedItemHeight;

    private bool _meshFieldChanged = false;
    private GameObject _instantiantedObject;

    [MenuItem("Mondo Museum/Generate Painting from Image", false, 5)]
    static void Init(){
        GeneratePaintingEditorWindow window = EditorWindow.GetWindow<GeneratePaintingEditorWindow>("Generate Painting", (typeof(SceneView)));
        window.minSize = new Vector2(500f, 500f);
        window.Show();
    }

    protected virtual void OnEnable(){
        
    }

    protected virtual void OnDisable(){
        
    }

    void OnInspectorUpdate(){
        Repaint();
    }

    protected virtual void OnGUI(){
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Texture: ", GUILayout.Width(80));
        _texture = (Texture)EditorGUILayout.ObjectField(_texture, typeof(Texture), false, GUILayout.Width(61), GUILayout.Height(61));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        if(_texture == null){
            EditorGUI.BeginDisabledGroup(true);
        }
        if(GUILayout.Button("Generate Painting Assets", GUILayout.ExpandWidth(false))){
            GeneratePaintingAssets();
        }
        if(_texture == null){
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Mesh", GUILayout.Width(80));
        EditorGUI.BeginChangeCheck();
        _generatedMesh = EditorGUILayout.ObjectField(_generatedMesh, typeof(Mesh), false);
        if(EditorGUI.EndChangeCheck() || _meshFieldChanged){
            _meshFieldChanged = false;
            _meshScaleMultiplier = 1f;
            if(_generatedMesh != null){
                _originalItemWidth = ((Mesh)_generatedMesh).vertices[1].x * 2;
                _originalItemHeight = ((Mesh)_generatedMesh).vertices[2].y * 2;
            }
        }
        GUILayout.Label("Width: " + _originalItemWidth + ", Height: " + _originalItemHeight, GUILayout.ExpandWidth(false));
        EditorGUILayout.EndHorizontal();

        if(_generatedMesh == null){
            EditorGUI.BeginDisabledGroup(true);
        }
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Scale", GUILayout.Width(80));
        _meshScaleMultiplier = EditorGUILayout.FloatField(_meshScaleMultiplier, GUILayout.Width(60));
        if(GUILayout.Button("Set Scale", GUILayout.ExpandWidth(false))){
            SetMeshScale();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Width (X):\nHeight (Y):", GUILayout.Width(80), GUILayout.Height(36));
        EditorGUILayout.SelectableLabel(_generatedItemWidth + "\n" + _generatedItemHeight);
        EditorGUILayout.EndHorizontal();
        if(_generatedMesh == null){
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.HelpBox("Use these X and Y values for the Wall Frame Item Dimensions on your exhibit item Scriptable Object.", MessageType.Info);
    }

    private void GeneratePaintingAssets(){
        string folderPath = AssetDatabase.GetAssetPath(_texture);
        folderPath = folderPath.Replace(Path.GetFileName(folderPath), "");

        /* TEXTURE IMPORT SETTINGS */
        string texturePath = AssetDatabase.GetAssetPath(_texture);
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(texturePath);

        importer.textureType = TextureImporterType.Default;
        importer.textureShape = TextureImporterShape.Texture2D;
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.None;

        importer.npotScale = TextureImporterNPOTScale.None;
        importer.isReadable = true;
        importer.streamingMipmaps = false;
        importer.mipmapEnabled = false;

        importer.wrapMode = TextureWrapMode.Repeat;
        importer.filterMode = FilterMode.Bilinear;
        importer.anisoLevel = 1;

        var platformSettings = importer.GetDefaultPlatformTextureSettings();
        platformSettings.maxTextureSize = 512;
        platformSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        platformSettings.crunchedCompression = false;
        platformSettings.format = TextureImporterFormat.Automatic;
        platformSettings.textureCompression = TextureImporterCompression.Compressed;
        importer.SetPlatformTextureSettings(platformSettings);

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        /* CREATE MATERIAL ASSET */
        Material material = new Material(Shader.Find("Standard"));
        material.mainTexture = _texture;

        AssetDatabase.CreateAsset(material, folderPath + _texture.name + "_Material.mat");

        /* CREATE MESH ASSET */

        float maxLength = Mathf.Max(_texture.width, _texture.height);
        float normalizedWidth = _texture.width / maxLength;
        float normalizedHeight = _texture.height / maxLength;

        _generatedItemWidth = normalizedWidth;
        _generatedItemHeight = normalizedHeight;

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4]{
            new Vector3(-normalizedWidth/2, -normalizedHeight/2, 0),
            new Vector3(normalizedWidth/2, -normalizedHeight/2, 0),
            new Vector3(-normalizedWidth/2, normalizedHeight/2, 0),
            new Vector3(normalizedWidth/2, normalizedHeight/2, 0)
        };
        mesh.vertices = vertices;

        int[] tris = new int[6]{
            0, 2, 1,
            2, 3, 1
        };
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[4]{
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[4]{
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        mesh.RecalculateBounds();

        AssetDatabase.CreateAsset(mesh, folderPath + _texture.name + "_Mesh.asset");
        _generatedMesh = mesh;
        _meshFieldChanged = true;

        /* CREATE PREFAB */
        var obj = new GameObject(_texture.name + "_Prefab");
        var meshRenderer = obj.AddComponent<MeshRenderer>();
        var meshFilter = obj.AddComponent<MeshFilter>();

        meshRenderer.sharedMaterial = material;
        meshFilter.mesh = mesh;

        var prefabPath = folderPath + _texture.name + "_Prefab.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
        DestroyImmediate(obj);

        if(_instantiantedObject != null && _instantiantedObject.name == prefab.name){
            DestroyImmediate(_instantiantedObject);
        }
        _instantiantedObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        _instantiantedObject.name = prefab.name;
    }

    private void SetMeshScale(){
        if(_generatedMesh == null){
            return;
        }

        float scaledWidth = _originalItemWidth * _meshScaleMultiplier;
        float scaledHeight = _originalItemHeight * _meshScaleMultiplier;
        _generatedItemWidth = scaledWidth;
        _generatedItemHeight = scaledHeight;

        Vector3[] vertices = new Vector3[4]{
            new Vector3(-scaledWidth/2, -scaledHeight/2, 0),
            new Vector3(scaledWidth/2, -scaledHeight/2, 0),
            new Vector3(-scaledWidth/2, scaledHeight/2, 0),
            new Vector3(scaledWidth/2, scaledHeight/2, 0)
        };
        ((Mesh)_generatedMesh).vertices = vertices;

        ((Mesh)_generatedMesh).RecalculateBounds();
    }
   
}
