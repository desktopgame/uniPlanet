using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

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

		if (GUILayout.Button("Generate Prefab")) {
			GeneratePrefab();
		}

		if (GUILayout.Button("Generate World")) {
            GenerateWorld();
		}

		ShowLog();
		EditorGUILayout.EndVertical();
	}

	private void GeneratePrefab() {
		var blocksData = (uniPlanet.Blocks)JsonUtility.FromJson(File.ReadAllText(blocksJson), typeof(uniPlanet.Blocks));
		var texturesData = (uniPlanet.Textures)JsonUtility.FromJson(File.ReadAllText(texturesJson), typeof(uniPlanet.Textures));
		var texturesDir = Path.Combine(Path.GetDirectoryName(texturesJson), texturesData.baseDirectory);
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
        for(int i=0; i<texturesData.textures.Length; i++) {
            var textureData = texturesData.textures[i];
			CreatePlanes(absSpriteDir, relMaterialDir, relPrefabDir, texturesDir, textureData);
            EditorUtility.DisplayProgressBar("Copy Textures...", $"{i}/{texturesData.textures.Length}", (float)i / (float)texturesData.textures.Length);
		}
        EditorUtility.ClearProgressBar();
        // create prefab
		var sideDict = new Dictionary<string, GameObject>();
        for(int i=0; i<blocksData.blocks.Length; i++) {
            var blockData = blocksData.blocks[i];
			sideDict.Clear();
			//var block = new GameObject();
			var textureData = FindTextureFromReference(texturesData, blockData.texture);
			var mappingRule = textureData.mappingRule;
			var baseName = textureData.baseFileName;

			if (!string.IsNullOrEmpty(mappingRule.all)) {
				var name = Path.Combine(relPrefabDir, $"pref_{baseName}{mappingRule.all}") + ".prefab";
				var pref = AssetDatabase.LoadAssetAtPath(name, typeof(GameObject)) as GameObject;
				UnityEngine.Assertions.Assert.IsTrue(pref != null, name);
				sideDict["top"] = pref;
				sideDict["bottom"] = pref;
				sideDict["left"] = pref;
				sideDict["right"] = pref;
				sideDict["front"] = pref;
				sideDict["back"] = pref;
			}

			if (!string.IsNullOrEmpty(mappingRule.top)) {
				var name = Path.Combine(relPrefabDir, $"pref_{baseName}{mappingRule.top}") + ".prefab";
				var pref = AssetDatabase.LoadAssetAtPath(name, typeof(GameObject)) as GameObject;
				UnityEngine.Assertions.Assert.IsTrue(pref != null, name);
				sideDict["top"] = pref;
			}

			if (!string.IsNullOrEmpty(mappingRule.bottom)) {
				var name = Path.Combine(relPrefabDir, $"pref_{baseName}{mappingRule.bottom}") + ".prefab";
				var pref = AssetDatabase.LoadAssetAtPath(name, typeof(GameObject)) as GameObject;
				UnityEngine.Assertions.Assert.IsTrue(pref != null, name);
				sideDict["bottom"] = pref;
			}

			if (!string.IsNullOrEmpty(mappingRule.left)) {
				var name = Path.Combine(relPrefabDir, $"pref_{baseName}{mappingRule.left}") + ".prefab";
				var pref = AssetDatabase.LoadAssetAtPath(name, typeof(GameObject)) as GameObject;
				UnityEngine.Assertions.Assert.IsTrue(pref != null, name);
				sideDict["left"] = pref;
			}

			if (!string.IsNullOrEmpty(mappingRule.right)) {
				var name = Path.Combine(relPrefabDir, $"pref_{baseName}{mappingRule.right}") + ".prefab";
				var pref = AssetDatabase.LoadAssetAtPath(name, typeof(GameObject)) as GameObject;
				UnityEngine.Assertions.Assert.IsTrue(pref != null, name);
				sideDict["right"] = pref;
			}

			if (!string.IsNullOrEmpty(mappingRule.front)) {
				var name = Path.Combine(relPrefabDir, $"pref_{baseName}{mappingRule.front}") + ".prefab";
				var pref = AssetDatabase.LoadAssetAtPath(name, typeof(GameObject)) as GameObject;
				UnityEngine.Assertions.Assert.IsTrue(pref != null, name);
				sideDict["front"] = pref;
			}

			if (!string.IsNullOrEmpty(mappingRule.back)) {
				var name = Path.Combine(relPrefabDir, $"pref_{baseName}{mappingRule.back}") + ".prefab";
				var pref = AssetDatabase.LoadAssetAtPath(name, typeof(GameObject)) as GameObject;
				UnityEngine.Assertions.Assert.IsTrue(pref != null, name);
				sideDict["back"] = pref;
			}

			var block = new GameObject();
			var prefabFile = Path.Combine(relPrefabDir, $"block_{blockData.reference}.prefab");

			try {
				var top = GameObject.Instantiate(sideDict["top"], Vector3.up * 5, Quaternion.Euler(0, 0, 0), block.transform);
				top.name = "top";
				var bottom = GameObject.Instantiate(sideDict["bottom"], Vector3.down * 5, Quaternion.Euler(0, 0, 180), block.transform);
				bottom.name = "bottom";
				var left = GameObject.Instantiate(sideDict["left"], Vector3.left * 5, Quaternion.Euler(90, 0, 90), block.transform);
				left.name = "left";
				var right = GameObject.Instantiate(sideDict["right"], Vector3.right * 5, Quaternion.Euler(90, 90, 0), block.transform);
				right.name = "right";
				var front = GameObject.Instantiate(sideDict["front"], Vector3.forward * 5, Quaternion.Euler(90, 0, 0), block.transform);
				front.name = "front";
				var back = GameObject.Instantiate(sideDict["back"], Vector3.back * 5, Quaternion.Euler(90, 180, 0), block.transform);
				back.name = "back";
				PrefabUtility.SaveAsPrefabAssetAndConnect(block, prefabFile, InteractionMode.AutomatedAction);
				GameObject.DestroyImmediate(block);
			} catch (System.ArgumentException) {
				Debug.LogError(prefabFile);
			}

            EditorUtility.DisplayProgressBar("Create Prefabs...", $"{i}/{blocksData.blocks.Length}", (float)i / (float)blocksData.blocks.Length);
        }
        EditorUtility.ClearProgressBar();

        AssetDatabase.Refresh();
	}

	private void GenerateWorld() {
        var worldData = (uniPlanet.World)JsonUtility.FromJson(File.ReadAllText(worldJson), typeof(uniPlanet.World));
        var absOutputDir = GetLatestDirectory("planetData");
        if(!Directory.Exists(absOutputDir))
        {
            Debug.LogError($"{absOutputDir} is missing.");
            return;
        }
        var absPrefabDir = Path.Combine(absOutputDir, "prefab");
        var relPrefabDir = ToRelativePath(absPrefabDir);
        var worldObj = new GameObject($"world");
        for (int i= 0; i < worldData.cell.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Generating World...", $"{i}/{worldData.cell.Length}", (float)i / (float)worldData.cell.Length);
            var cell = worldData.cell[i];
            var blockPath = Path.Combine(relPrefabDir, $"block_{cell.block}.prefab");
            var blockPref = AssetDatabase.LoadAssetAtPath(blockPath, typeof(GameObject)) as GameObject;
            UnityEngine.Assertions.Assert.IsTrue(blockPref != null, blockPath);
            if (blockPref == null) {
                break;
            }
            var blockObj = GameObject.Instantiate(blockPref, worldObj.transform);
            blockObj.name = cell.block;
            blockObj.transform.position = new Vector3(cell.x * 5, cell.y * 5, cell.z * 5);
        }
        EditorUtility.ClearProgressBar();
        //PrefabUtility.SaveAsPrefabAssetAndConnect(worldObj, Path.Combine(relPrefabDir, "world.prefab"), InteractionMode.AutomatedAction);
    }

	private static uniPlanet.Texture FindTextureFromReference(uniPlanet.Textures textures, string reference) {
		foreach (var texture in textures.textures) {
			if (texture.reference == reference) {
				return texture;
			}
		}

		return null;
	}

	private static void CreatePlanes(string absSpriteDir, string relMaterialDir, string relPrefabDir, string texturesDir, uniPlanet.Texture textureData) {
		var mappingRule = textureData.mappingRule;

		if (!string.IsNullOrEmpty(mappingRule.all)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.all + ".png"));
		}

		if (!string.IsNullOrEmpty(mappingRule.top)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.top + ".png"));
		}

		if (!string.IsNullOrEmpty(mappingRule.bottom)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.bottom + ".png"));
		}

		if (!string.IsNullOrEmpty(mappingRule.left)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.left + ".png"));
		}

		if (!string.IsNullOrEmpty(mappingRule.right)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.right + ".png"));
		}

		if (!string.IsNullOrEmpty(mappingRule.front)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.front + ".png"));
		}

		if (!string.IsNullOrEmpty(mappingRule.back)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, Path.Combine(texturesDir, textureData.baseFileName + mappingRule.back + ".png"));
		}
	}

	private static void CreatePlane(string absSpriteDir, string relMaterialDir, string relPrefabDir, string absTextuePath) {
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

    private static string GetLatestDirectory(string name)
    {
        var app = Application.dataPath;
        var uniData = Path.Combine(app, $"{name}");
        var count = 1;

        while (Directory.Exists(uniData))
        {
            uniData = Path.Combine(app, $"{name}{count}");
            count++;
        }

        return Path.Combine(app, $"{name}{count-1}");
    }

	private static string ToRelativePath(string absPath) {
		var sub = absPath.Substring(Application.dataPath.Length);

		if (sub.StartsWith("/") || sub.StartsWith("\\")) {
			return "Assets" + sub;
		}

		return "Assets/" + sub;
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
