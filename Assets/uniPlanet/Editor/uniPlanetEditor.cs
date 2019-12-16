using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class uniPlanetEditor : EditorWindow {
	private string blocksJson = "blocks.json";
	private string texturesJson = "textures.json";
	private string worldJson = "data.jspn";
	private string log;
	private Vector2 logScroll;

	private static readonly string KEY_BLOCKSJSON = "uniPlanet.blocks";
	private static readonly string KEY_TEXTURESJSON = "uniPlanet.textures";
	private static readonly string KEY_WORLDJSON = "uniPlanet.data";

	[MenuItem("Assets/uniPlanet/Open")]
	static void Open() {
		uniPlanetEditor window = (uniPlanetEditor)EditorWindow.GetWindow(typeof(uniPlanetEditor));
		window.Init();
		window.Show();
	}


	private void Init() {
		this.blocksJson = PlayerPrefs.GetString(KEY_BLOCKSJSON);
		this.texturesJson = PlayerPrefs.GetString(KEY_TEXTURESJSON);
		this.worldJson = PlayerPrefs.GetString(KEY_WORLDJSON);
		this.log = "";
	}

	void OnGUI() {
		if (EditorApplication.isCompiling || Application.isPlaying) {
			return;
		}

		this.log = "";

		EditorGUILayout.BeginVertical();
		ChooseFolder("Block", KEY_BLOCKSJSON, ref this.blocksJson);
		ChooseFolder("Texture", KEY_TEXTURESJSON, ref this.texturesJson);
		ChooseFolder("Data", KEY_WORLDJSON, ref this.worldJson);

		if (GUILayout.Button("Generate")) {
			Generate();
		}

		ShowLog();
		EditorGUILayout.EndVertical();
	}

	private void Generate() {
		var blocksData = (uniPlanet.Blocks)JsonUtility.FromJson(File.ReadAllText(blocksJson), typeof(uniPlanet.Blocks));
		var texturesData = (uniPlanet.Textures)JsonUtility.FromJson(File.ReadAllText(texturesJson), typeof(uniPlanet.Textures));
		var worldData = (uniPlanet.World)JsonUtility.FromJson(File.ReadAllText(worldJson), typeof(uniPlanet.World));

		var texturesDir = Path.Combine(Path.GetDirectoryName(texturesJson), texturesData.baseDirectory);
		var textures = Directory.GetFiles(texturesDir);
        // create sprite directory
		(string absOutputDir, string relOutputDir, int _) = GenUniqueDirectory("planetData");
		var absSpriteDir = Path.Combine(absOutputDir, "sprite");
        var relSpriteDir = ToRelativePath(absSpriteDir);
		Directory.CreateDirectory(absSpriteDir);
        AssetDatabase.ImportAsset(relSpriteDir);
        // create material directory
        var absMaterialDir = Path.Combine(absOutputDir, "material");
        var relMaterialDir = ToRelativePath(absMaterialDir);
        Directory.CreateDirectory(absMaterialDir);
        AssetDatabase.ImportAsset(relMaterialDir);
        // create prefab directory
        var absPrefabDir = Path.Combine(absOutputDir, "prefab");
        var relPrefabDir = ToRelativePath(absPrefabDir);
        Directory.CreateDirectory(absPrefabDir);
        AssetDatabase.ImportAsset(relPrefabDir);
        // copy textures
        foreach(var textureData in texturesData.textures)
        {
            CreatePlanes(absSpriteDir, relMaterialDir, relPrefabDir, texturesDir, textureData);
        }
        AssetDatabase.Refresh();
	}

    private static void CreatePlanes(string absSpriteDir, string relMaterialDir, string relPrefabDir, string texturesDir, uniPlanet.Texture textureData)
    {
        var mappingRule = textureData.mappingRule;
        if (!string.IsNullOrEmpty(mappingRule.all))
        {
            CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.all + ".png"));
        }
        if (!string.IsNullOrEmpty(mappingRule.top))
        {
            CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.top + ".png"));
        }
        if (!string.IsNullOrEmpty(mappingRule.bottom))
        {
            CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.bottom + ".png"));
        }
        if (!string.IsNullOrEmpty(mappingRule.left))
        {
            CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.left + ".png"));
        }
        if (!string.IsNullOrEmpty(mappingRule.right))
        {
            CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.right + ".png"));
        }
        if (!string.IsNullOrEmpty(mappingRule.front))
        {
            CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.front + ".png"));
        }
        if (!string.IsNullOrEmpty(mappingRule.back))
        {
            CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.back + ".png"));
        }
    }

    private static void CreatePlane(string absSpriteDir, string relMaterialDir, string relPrefabDir, string absTextuePath)
    {
        var filename = Path.GetFileName(absTextuePath);
        var absSprite = Path.Combine(absSpriteDir, filename);
        var relSprite = ToRelativePath(absSprite);
        File.Copy(absTextuePath, absSprite);
        AssetDatabase.ImportAsset(relSprite);
        // create material
        var newMat = new Material(Shader.Find("Standard"));
        newMat.SetTexture("_MainTex", (Texture)AssetDatabase.LoadAssetAtPath(relSprite, typeof(Texture)));
        var relMaterial = Path.Combine(relMaterialDir, $"mat_{Path.GetFileNameWithoutExtension(absTextuePath)}.mat");
        AssetDatabase.CreateAsset(newMat, relMaterial);
        // create prefab
        var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.GetComponent<MeshRenderer>().material = newMat;
        var relPrefab = Path.Combine(relPrefabDir, $"pref_{Path.GetFileNameWithoutExtension(absTextuePath)}.prefab");
        PrefabUtility.SaveAsPrefabAssetAndConnect(plane, relPrefab, InteractionMode.AutomatedAction);
        GameObject.DestroyImmediate(plane);
    }

	private static (string, string, int) GenUniqueDirectory(string name) {
		(string absOutputDir, int count) = GetUniqueDirectory(name);
		var relOutputDir = ToRelativePath(absOutputDir);
		Debug.Log($"outputDir = {absOutputDir}");
		Debug.Log($"relOutputDir = {relOutputDir}");
		Directory.CreateDirectory(absOutputDir);
		AssetDatabase.ImportAsset(relOutputDir);
		return (absOutputDir, relOutputDir, count);
	}

	private static (string, int) GetUniqueDirectory(string name) {
		var app = Application.dataPath;
		var uniData = Path.Combine(app, $"{name}");
		var count = 1;

		while (Directory.Exists(uniData)) {
			uniData = Path.Combine(app, $"{name}{count}");
			count++;
		}

		return (uniData, count);
	}

	private static string ToRelativePath(string absPath) {
		return "Assets/" + absPath.Substring(Application.dataPath.Length);
	}

	private void ChooseFolder(string label, string key, ref string dir) {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(label);
		var newVal = EditorGUILayout.TextField(dir);

		if (GUILayout.Button("...")) {
			newVal = EditorUtility.OpenFilePanel("Choose Folder", Application.dataPath, ".json");
		}

		if (dir != newVal) {
			dir = newVal;
			PlayerPrefs.SetString(key, newVal);
			PlayerPrefs.Save();
		}

		EditorGUILayout.EndHorizontal();
	}

	private void ShowLog() {
		Separator(false);
		EditorGUILayout.LabelField("Console");
		this.logScroll = EditorGUILayout.BeginScrollView(this.logScroll);
		var nl = System.Environment.NewLine;

		if (!File.Exists(this.blocksJson)) {
			log += $"{blocksJson} is missing.{nl}";
		}

		if (!File.Exists(this.texturesJson)) {
			log += $"{texturesJson} is missing.{nl}";
		}

		if (!File.Exists(this.worldJson)) {
			log += $"{worldJson} is missing.{nl}";
		}

		EditorGUILayout.TextArea(log);
		EditorGUILayout.EndScrollView();
	}

	private static void Separator(bool useIndentLevel = false) {
		EditorGUILayout.BeginHorizontal();

		if (useIndentLevel) {
			GUILayout.Space(EditorGUI.indentLevel * 15);
		}

		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
		EditorGUILayout.EndHorizontal();
	}

	/// <summary>
	/// インデントレベルを設定する仕切り線.
	/// </summary>
	/// <param name="indentLevel">インデントレベル</param>
	private static void Separator(int indentLevel) {
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(indentLevel * 15);
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
		EditorGUILayout.EndHorizontal();
	}
}
