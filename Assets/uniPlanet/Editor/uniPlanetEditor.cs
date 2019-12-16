using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class uniPlanetEditor : EditorWindow {
	private string blocksJson = "blocks.json";
	private string texturesJson = "textures.json";
	private string dataJson = "data.jspn";
	private string log;
	private Vector2 logScroll;

	private static readonly string KEY_BLOCKSJSON = "uniPlanet.blocks";
	private static readonly string KEY_TEXTURESJSON = "uniPlanet.textures";
	private static readonly string KEY_DATAJSON = "uniPlanet.data";

	[MenuItem("Assets/uniPlanet/Open")]
	static void Open() {
		uniPlanetEditor window = (uniPlanetEditor)EditorWindow.GetWindow(typeof(uniPlanetEditor));
		window.Init();
		window.Show();
	}


	private void Init() {
		this.blocksJson = PlayerPrefs.GetString(KEY_BLOCKSJSON);
		this.texturesJson = PlayerPrefs.GetString(KEY_TEXTURESJSON);
		this.dataJson = PlayerPrefs.GetString(KEY_DATAJSON);
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
		ChooseFolder("Data", KEY_DATAJSON, ref this.dataJson);
		ShowLog();
		EditorGUILayout.EndVertical();
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

		if (!File.Exists(this.dataJson)) {
			log += $"{dataJson} is missing.{nl}";
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
