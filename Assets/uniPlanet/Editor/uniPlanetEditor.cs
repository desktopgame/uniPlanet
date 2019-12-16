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
		// parse data
		(uniPlanet.Blocks blocksData, Dictionary<string, uniPlanet.Block> blocksDict) = ParseBlocksJson(this.blocksJson);
		(uniPlanet.Textures texturesData, Dictionary<string, uniPlanet.Texture> texturesDict) = ParseTexturesJson(this.texturesJson);
		// create directories
		(string absOutputDir, string relOutputDir, int _) = GenUniqueDirectory("planetData");
		(string absSpriteDir, string relSpriteDir) = CreateDirectory(absOutputDir, "sprite");
		(string absMaterialDir, string relMaterialDir) = CreateDirectory(absOutputDir, "material");
		(string absPrefabDir, string relPrefabDir) = CreateDirectory(absOutputDir, "prefab");
		var texturesDir = Path.Combine(Path.GetDirectoryName(texturesJson), texturesData.baseDirectory);

		// copy textures
		for (int i = 0; i < blocksData.blocks.Length; i++) {
			var blockData = blocksData.blocks[i];
			var textureData = texturesDict[blockData.texture];
			CreatePlanes(absSpriteDir, relMaterialDir, relPrefabDir, texturesDir, textureData, blockData);
			EditorUtility.DisplayProgressBar("Copy Textures...", $"{i}/{ blocksData.blocks.Length}", (float)i / (float)blocksData.blocks.Length);
		}

		EditorUtility.ClearProgressBar();
		// create prefab
		var sideDict = new Dictionary<string, GameObject>();

		for (int i = 0; i < blocksData.blocks.Length; i++) {
			var blockData = blocksData.blocks[i];
			sideDict.Clear();
			var textureData = FindTextureFromReference(texturesData, blockData.texture);
			var mappingRule = textureData.mappingRule;
			var baseName = blockData.reference;

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
		// parse data
		(uniPlanet.Blocks blocksData, Dictionary<string, uniPlanet.Block> blocksDict) = ParseBlocksJson(this.blocksJson);
		(uniPlanet.Textures texturesData, Dictionary<string, uniPlanet.Texture> texturesDict) = ParseTexturesJson(this.texturesJson);
		(string absOutputDir, int count) = GetLatestDirectory("planetData");
		var worldData = (uniPlanet.World)JsonUtility.FromJson(File.ReadAllText(worldJson), typeof(uniPlanet.World));

		if (!Directory.Exists(absOutputDir)) {
			Debug.LogError($"{absOutputDir} is missing.");
			return;
		}

		var absPrefabDir = Path.Combine(absOutputDir, "prefab");
		var relPrefabDir = ToRelativePath(absPrefabDir);
		var worldObj = new GameObject($"world{count}");
		var map = TableToMap(worldData);
		var mapSize = worldData.worldSize.x * worldData.worldSize.y * worldData.worldSize.z;

		for (int x = 0; x < worldData.worldSize.x; x++) {
			for (int y = 0; y < worldData.worldSize.y; y++) {
				for (int z = 0; z < worldData.worldSize.z; z++) {
					int index = x + y + z;
					//EditorUtility.DisplayProgressBar("Generating World...", $"{index}/{mapSize}", (float)index / (float)mapSize);
					var key = map[x, y, z];

					if (key == null) {
						continue;
					}

					var blockData = blocksDict[key];
					var textureData = texturesDict[blockData.texture];
					var mappingRule = textureData.mappingRule;
					var visibleXP = x + 1 < worldData.worldSize.x;
					var visibleXN = x - 1 >= 0;
					var visibleYP = y + 1 < worldData.worldSize.y;
					var visibleYN = y - 1 >= 0;
					var visibleZP = z + 1 < worldData.worldSize.z;
					var visibleZN = z - 1 >= 0;
					var blockXP = visibleXP ? map[x + 1, y, z] : null;
					var blockXN = visibleXN ? map[x - 1, y, z] : null;
					var blockYP = visibleYP ? map[x, y + 1, z] : null;
					var blockYN = visibleYN ? map[x, y - 1, z] : null;
					var blockZP = visibleZP ? map[x, y, z + 1] : null;
					var blockZN = visibleZN ? map[x, y, z - 1] : null;
					var basePos = new Vector3(x * 5, y * 5, z * 5);

					// x+, x-
					if (blockXP == null) {
						var right = string.IsNullOrEmpty(mappingRule.right) ? mappingRule.all : mappingRule.right;
						var path = Path.Combine(relPrefabDir, $"pref_{map[x,y,z]}{right}.prefab");
						var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
						UnityEngine.Assertions.Assert.IsTrue(prefab != null, path);
						GameObject.Instantiate(prefab, basePos + (Vector3.right * 5), Quaternion.Euler(90, 90, 0), worldObj.transform);
					}

					if (blockXN == null) {
						var left = string.IsNullOrEmpty(mappingRule.left) ? mappingRule.all : mappingRule.left;
						var path = Path.Combine(relPrefabDir, $"pref_{map[x, y, z]}{left}.prefab");
						var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
						UnityEngine.Assertions.Assert.IsTrue(prefab != null, path);
						GameObject.Instantiate(prefab, basePos + (Vector3.left * 5), Quaternion.Euler(90, 0, 90), worldObj.transform);
					}

					// y+, y-
					if (blockYP == null) {
						var top = string.IsNullOrEmpty(mappingRule.top) ? mappingRule.all : mappingRule.top;
						var path = Path.Combine(relPrefabDir, $"pref_{map[x, y, z]}{top}.prefab");
						var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
						UnityEngine.Assertions.Assert.IsTrue(prefab != null, path);
						GameObject.Instantiate(prefab, basePos + (Vector3.up * 5), Quaternion.Euler(0, 0, 0), worldObj.transform);
					}

					if (blockYN == null) {
						var bottom = string.IsNullOrEmpty(mappingRule.bottom) ? mappingRule.all : mappingRule.bottom;
						var path = Path.Combine(relPrefabDir, $"pref_{map[x, y, z]}{bottom}.prefab");
						var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
						UnityEngine.Assertions.Assert.IsTrue(prefab != null, path);
						GameObject.Instantiate(prefab, basePos + (Vector3.down * 5), Quaternion.Euler(0, 0, 180), worldObj.transform);
					}

					// z+, z-
					if (blockZP == null) {
						var front = string.IsNullOrEmpty(mappingRule.front) ? mappingRule.all : mappingRule.front;
						var path = Path.Combine(relPrefabDir, $"pref_{map[x, y, z]}{front}.prefab");
						var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
						UnityEngine.Assertions.Assert.IsTrue(prefab != null, path);
						GameObject.Instantiate(prefab, basePos + (Vector3.forward * 5), Quaternion.Euler(90, 0, 0), worldObj.transform);

					}

					if (blockZN == null) {
						var back = string.IsNullOrEmpty(mappingRule.back) ? mappingRule.all : mappingRule.back;
						var path = Path.Combine(relPrefabDir, $"pref_{map[x, y, z]}{back}.prefab");
						var prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
						UnityEngine.Assertions.Assert.IsTrue(prefab != null, path);
						GameObject.Instantiate(prefab, basePos + (Vector3.back * 5), Quaternion.Euler(90, 180, 0), worldObj.transform);
					}
				}
			}
		}

		worldObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

		//EditorUtility.ClearProgressBar();
		//PrefabUtility.SaveAsPrefabAssetAndConnect(worldObj, Path.Combine(relPrefabDir, "world.prefab"), InteractionMode.AutomatedAction);
	}

	private static (uniPlanet.Blocks, Dictionary<string, uniPlanet.Block>) ParseBlocksJson(string json) {

		var blocksData = (uniPlanet.Blocks)JsonUtility.FromJson(File.ReadAllText(json), typeof(uniPlanet.Blocks));
		var blocksDict = new Dictionary<string, uniPlanet.Block>();

		foreach (var bd in blocksData.blocks) {
			blocksDict[bd.reference] = bd;
		}

		return (blocksData, blocksDict);
	}

	private static (uniPlanet.Textures, Dictionary<string, uniPlanet.Texture>) ParseTexturesJson(string json) {

		var texturesData = (uniPlanet.Textures)JsonUtility.FromJson(File.ReadAllText(json), typeof(uniPlanet.Textures));
		var texturesDict = new Dictionary<string, uniPlanet.Texture>();

		foreach (var td in texturesData.textures) {
			texturesDict[td.reference] = td;
		}

		return (texturesData, texturesDict);
	}

	private static string[,,] TableToMap(uniPlanet.World worldData) {
		var map = new string[worldData.worldSize.x, worldData.worldSize.y, worldData.worldSize.z];

		for (int i = 0; i < worldData.worldSize.x; i++) {
			for (int j = 0; j < worldData.worldSize.y; j++) {
				for (int k = 0; k < worldData.worldSize.z; k++) {
					map[i, j, k] = null;
				}
			}
		}

		foreach (var cell in worldData.cell) {
			map[cell.x, cell.y, cell.z] = cell.block;
		}

		return map;
	}

	private static uniPlanet.Texture FindTextureFromReference(uniPlanet.Textures textures, string reference) {
		foreach (var texture in textures.textures) {
			if (texture.reference == reference) {
				return texture;
			}
		}

		return null;
	}

	private static void CreatePlanes(string absSpriteDir, string relMaterialDir, string relPrefabDir, string texturesDir, uniPlanet.Texture textureData, uniPlanet.Block block) {
		var mappingRule = textureData.mappingRule;

		if (!string.IsNullOrEmpty(mappingRule.all)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, mappingRule.all, texturesDir, textureData, block);
		}

		if (!string.IsNullOrEmpty(mappingRule.top)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, mappingRule.top, texturesDir, textureData, block);
		}

		if (!string.IsNullOrEmpty(mappingRule.bottom)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, mappingRule.bottom, texturesDir, textureData, block);
		}

		if (!string.IsNullOrEmpty(mappingRule.left)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, mappingRule.left, texturesDir, textureData, block);
		}

		if (!string.IsNullOrEmpty(mappingRule.right)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, mappingRule.right, texturesDir, textureData, block);
		}

		if (!string.IsNullOrEmpty(mappingRule.front)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, mappingRule.front, texturesDir, textureData, block);
		}

		if (!string.IsNullOrEmpty(mappingRule.back)) {
			CreatePlane(absSpriteDir, relMaterialDir, relPrefabDir, mappingRule.back, texturesDir, textureData, block);
		}
	}

	private static void CreatePlane(string absSpriteDir, string relMaterialDir, string relPrefabDir, string side, string texturesDir, uniPlanet.Texture texture, uniPlanet.Block block) {
		var absTextuePath = Path.Combine(texturesDir, texture.baseFileName + side + ".png");
		var filename = Path.GetFileName(absTextuePath);
		var absSprite = Path.Combine(absSpriteDir, filename);
		var relSprite = ToRelativePath(absSprite);
		File.Copy(absTextuePath, absSprite);
		AssetDatabase.ImportAsset(relSprite);
		// create material
		var newMat = new Material(Shader.Find("Standard"));
		newMat.SetTexture("_MainTex", (Texture)AssetDatabase.LoadAssetAtPath(relSprite, typeof(Texture)));
		var relMaterial = Path.Combine(relMaterialDir, $"mat_{block.reference}{side}.mat");
		AssetDatabase.CreateAsset(newMat, relMaterial);
		// create prefab
		var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		plane.GetComponent<MeshRenderer>().material = newMat;
		var relPrefab = Path.Combine(relPrefabDir, $"pref_{block.reference}{side}.prefab");
		PrefabUtility.SaveAsPrefabAssetAndConnect(plane, relPrefab, InteractionMode.AutomatedAction);
		GameObject.DestroyImmediate(plane);
	}

	//
	// File Utility
	//

	private static (string, string) CreateDirectory(string parent, string name) {
		var abs = Path.Combine(parent, name);
		var rel = ToRelativePath(parent);
		Directory.CreateDirectory(parent);
		AssetDatabase.ImportAsset(rel);
		return (abs, rel);
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

	private static (string, int) GetLatestDirectory(string name) {
		var app = Application.dataPath;
		var uniData = Path.Combine(app, $"{name}");
		var count = 0;

		while (Directory.Exists(uniData)) {
			uniData = Path.Combine(app, $"{name}{count}");
			count++;
		}

		count--;

		if (count == 0) {
			return (Path.Combine(app, $"{name}"), count);
		}

		return (Path.Combine(app, $"{name}{count}"), count);
	}

	private static string ToRelativePath(string absPath) {
		var sub = absPath.Substring(Application.dataPath.Length);

		if (sub.StartsWith("/") || sub.StartsWith("\\")) {
			return "Assets" + sub;
		}

		return "Assets/" + sub;
	}

	//
	// GUI Utility
	//

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
