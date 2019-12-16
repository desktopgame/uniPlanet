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
        // create directory for assets
		(string absOutputDir, string relOutputDir, int _) = GenUniqueDirectory("planetData");
		var absSpriteDir = Path.Combine(absOutputDir, "sprite");
        var relSpriteDir = ToRelativePath(absSpriteDir);
		Directory.CreateDirectory(absSpriteDir);
        AssetDatabase.ImportAsset(relSpriteDir);
        // copy textures
        foreach(var texture in textures)
        {
            var absSprite = Path.Combine(absSpriteDir, Path.GetFileName(texture));
            var relSprite = ToRelativePath(absSprite);
            File.Copy(texture, absSprite);
            AssetDatabase.ImportAsset(relSprite);
        }
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
