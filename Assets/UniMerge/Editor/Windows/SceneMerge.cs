//Matt Schoen
//1-3-2018
//
// This software is the copyrighted material of its author, Matt Schoen, and his company Defective Studios.
// It is available for sale on the Unity Asset store and is subject to their restrictions and limitations, as well as
// the following: You shall not reproduce or re-distribute this software without the express written (e-mail is fine)
// permission of the author. If permission is granted, the code (this file and related files) must bear this license
// in its entirety. Anyone who purchases the script is welcome to modify and re-use the code at their personal risk
// and under the condition that it not be included in any distribution builds. The software is provided as-is without
// warranty and the author bears no responsibility for damages or losses caused by the software.
// This Agreement becomes effective from the day you have installed, copied, accessed, downloaded and/or otherwise used
// the software.

//UniMerge 1.9.1
//SceneMerge Window

#define DEV //Comment this out to not auto-populate scene merge

#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define Unity3
#endif

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
#define Unity4_0To4_2
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UniMerge.Editor.Helpers;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

#if !(UNITY_5_3 || UNITY_5_3_OR_NEWER)
using EditorSceneManager = UnityEditor.EditorApplication;
#endif

#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4
using PrefabUtility = UnityEditor.EditorUtility;
#endif

namespace UniMerge.Editor.Windows {
	[InitializeOnLoad]
	public class SceneMerge : UniMergeWindow {
#if DEV
		//If these names end up conflicting with names within your project, change them here
		public const string MySceneName = "Mine", TheirSceneName = "Theirs";
#endif
		const string MessagePath = "Assets/merges.txt";

		public GameObject myContainer { private get; set; }
		public GameObject theirContainer { private get; set; }
		public bool compareLightingData { get; private set; }

		//If these names end up conflicting with names within your scene, change them here
		string myContainerName = "mine", theirContainerName = "theirs";

		UnityObject mine, theirs;
		string myName = MySceneName, theirName = TheirSceneName;

		internal readonly List<PropertyHelper> properties = new List<PropertyHelper>();
		internal readonly List<string> cachedPropertyPaths = new List<string>();

		bool loading;
		bool merged;

		SceneData mySceneData, theirSceneData;
		SerializedObject mySO, theirSO;

		Action draw;

		static SceneMerge() {
			EditorApplication.update -= CheckForMessageFile;
			EditorApplication.update += CheckForMessageFile;
		}

		// ReSharper disable once UnusedMember.Local
		[MenuItem("Window/UniMerge/Scene Merge %&m")]
		static void Init() {
			GetWindow<SceneMerge>(false, "SceneMerge");
		}

		protected override void OnEnable() {
			draw = Draw;
			base.OnEnable();
			if (!merged)
				SceneData.Cleanup();

			//Get path
#if Unity3 //TODO: Unity 3 path stuff?
#else
			var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
			if (!string.IsNullOrEmpty(scriptPath))
				UniMergeConfig.DefaultPath = scriptPath.Substring(0, scriptPath.IndexOf("Editor") - 1);
#endif

			if (Directory.Exists(UniMergeConfig.DefaultPath + "/Demo/Scene Merge")) {
				var assets = Directory.GetFiles(UniMergeConfig.DefaultPath + "/Demo/Scene Merge");
				foreach (var asset in assets)
					if (asset.EndsWith(".unity")) {
						if (asset.Contains(MySceneName))
							mine = AssetDatabase.LoadAssetAtPath(asset.Replace('\\', '/'), typeof(UnityObject));
						if (asset.Contains(TheirSceneName))
							theirs = AssetDatabase.LoadAssetAtPath(asset.Replace('\\', '/'), typeof(UnityObject));
					}
			}

			if (EditorPrefs.HasKey(RowHeightKey))
				selectedRowHeight = EditorPrefs.GetInt(RowHeightKey);

			loading = false;
		}

		protected override void OnDisable() {
			base.OnDisable();
			EditorPrefs.SetInt("RowHeight", selectedRowHeight);
		}

		// ReSharper disable once UnusedMember.Local
		void OnGUI() {
			if (loading) {
				GUILayout.Label("Loading...");
				return;
			}

#if Unity3 || Unity4_0To4_2 //Layout fix for older versions?
#else
			EditorGUIUtility.labelWidth = 150;
#endif

			if (InitGUI())
				return;

#if UNITY_5 || UNITY_5_3_OR_NEWER
			if (mine == null || theirs == null) //TODO: check if valid scene object
#else
			if (mine == null || theirs == null
				|| mine.GetType() != typeof(UnityObject) || mine.GetType() != typeof(UnityObject)
				) //|| !AssetDatabase.GetAssetPath(mine).Contains(".unity") || !AssetDatabase.GetAssetPath(theirs).Contains(".unity"))
#endif
				merged = GUI.enabled = false;

			if (GUILayout.Button("Merge")) {
				loading = true;
				Merge(mine, theirs);
				GUIUtility.ExitGUI();
			}

			GUI.enabled = merged;
			GUILayout.BeginHorizontal();
			{
				GUI.enabled = myContainer;
				if (!GUI.enabled)
					merged = false;

				DrawSaveGUI(true);

				GUI.enabled = theirContainer;
				if (!GUI.enabled)
					merged = false;

				DrawSaveGUI(false);
			}
			GUILayout.EndHorizontal();

			if (GUILayout.Button("Refresh")) {
				updateType = RefreshType.Updating;
				update = Refresh();
			}

			GUI.enabled = true;
#if !Unity3
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
#endif

#if !(UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
			EditorGUI.BeginChangeCheck();
			compareLightingData = EditorGUILayout.Toggle("Compare Lighting Data", compareLightingData);
			if (EditorGUI.EndChangeCheck())
				update = Refresh();
#endif

			DrawRowHeight();

#if !Unity3
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
#endif

			GUILayout.BeginHorizontal();
			{
				DrawObjectFieldGUI(ref myName, ref mine);
				GUILayout.Space(UniMergeConfig.DoubleMidWidth);
				DrawObjectFieldGUI(ref theirName, ref theirs);
			}
			GUILayout.EndHorizontal();

			if (mine == null || theirs == null)
				merged = false;

			if (!merged)
				return;

			CustomScroll(draw);

			ProgressBar();
		}

		void Draw() {
			GUILayout.Space(0); // To provide a rect for ObjectDrawCheck

			var colWidthOption = GUILayout.Width(colWidth + columnPadding);
			var indentOption = GUILayout.Width(1);
			foreach (var property in properties) {
				if (property.property.name == "m_Script")
					continue;

				property.Draw(1, colWidthOption, indentOption);
			}
		}

		void DrawSaveGUI(bool isMine) {
			var name = isMine ? "Mine" : "Theirs";
			var saveAs = GUILayout.Button(string.Format("Save {0} as...", name));
			if (GUILayout.Button(string.Format("Save To {0}", name)) || saveAs) {
				var scene = isMine ? mine : theirs;
#if !UNITY_3_4 && !UNITY_3_3 && !UNITY_3_2 && !UNITY_3_1 && !UNITY_3_0_0 && !UNITY_3_0
				var path = AssetDatabase.GetAssetOrScenePath(scene);
#else
				var path = AssetDatabase.GetAssetPath(scene);
#endif

				if (saveAs) {
					var fileName = Path.GetFileNameWithoutExtension(path);
					path = EditorUtility.SaveFilePanelInProject("Save Mine", fileName, "unity", string.Empty);
				}

				if (!string.IsNullOrEmpty(path)) {
					var myContainer = this.myContainer;
					var theirContainer = this.theirContainer;
					DestroyImmediate(isMine ? theirContainer : myContainer);

					var tmp = new List<Transform>();
					var container = isMine ? myContainer : theirContainer;
					foreach (Transform t in container.transform)
						tmp.Add(t);

					foreach (var t in tmp)
						t.parent = null;

					DestroyImmediate(container);

					(isMine ? mySceneData : theirSceneData).ApplySettings();

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
					EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
#else
					EditorApplication.SaveScene(path);
#endif
				}
			}
		}

		void DrawObjectFieldGUI(ref string name, ref UnityObject scene) {
			GUILayout.BeginVertical(GUILayout.Width(colWidth));
			{
#if !(Unity3 || Unity4_0To4_2)
				GUILayout.BeginHorizontal();
				name = GUILayout.TextField(name, GUILayout.Width(EditorGUIUtility.labelWidth));
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
				scene = EditorGUILayoutExt.ObjectField(scene, typeof(SceneAsset), true);
#else
				scene = EditorGUILayoutExt.ObjectField(scene, typeof(UnityObject), true);
#endif
				GUILayout.EndHorizontal();
#else
				scene = EditorGUILayoutExt.ObjectField(name, scene, typeof(UnityObject), true);
#endif
			}
			GUILayout.EndVertical();
		}

		public static void CliIn() {
			var args = Environment.GetCommandLineArgs();
			foreach (var arg in args)
				Debug.Log(arg);
			Merge(args[args.Length - 2].Substring(args[args.Length - 2].IndexOf("Assets")).Replace("\\", "/").Trim(),
				args[args.Length - 1].Substring(args[args.Length - 1].IndexOf("Assets")).Replace("\\", "/").Trim());
		}

		static void CheckForMessageFile() {
			var mergeFile = (TextAsset) AssetDatabase.LoadAssetAtPath(MessagePath, typeof(TextAsset));
			if (mergeFile) {
				var files = mergeFile.text.Split('\n');
				AssetDatabase.DeleteAsset(MessagePath);
				for (var i = 0; i < files.Length; i++)
					if (!files[i].StartsWith("Assets"))
						if (files[i].IndexOf("Assets") > -1)
							files[i] = files[i].Substring(files[i].IndexOf("Assets")).Replace("\\", "/").Trim();
				DoMerge(files);
			}
		}

		public static void PrefabMerge(string myPath, string theirPath) {
			var window = (ObjectMerge) GetWindow(typeof(ObjectMerge));
			var root = window.root;
			root.SetGameObjects(
				(GameObject) AssetDatabase.LoadAssetAtPath(myPath, typeof(GameObject)),
				(GameObject) AssetDatabase.LoadAssetAtPath(theirPath, typeof(GameObject)));
		}

		public static void DoMerge(string[] paths) {
			if (paths.Length > 2)
				Merge(paths[0], paths[1]);
			else
				Debug.LogError("need at least 2 paths, " + paths.Length + " given");
		}

		internal void Merge() {
			Merge(mine, theirs);
		}

		public void Merge(UnityObject myScene, UnityObject theirScene) {
			if (myScene == null || theirScene == null)
				return;

			Merge(AssetDatabase.GetAssetPath(myScene), AssetDatabase.GetAssetPath(theirScene));
		}

		public static void Merge(string myPath, string theirPath) {
			var window = GetWindow<SceneMerge>(false, "SceneMerge");
			window.updateType = RefreshType.Comparing;
			window.update = window.MergeAsync(myPath, theirPath);
		}

		public IEnumerator MergeAsync(string myPath, string theirPath) {
			if (string.IsNullOrEmpty(myPath) || string.IsNullOrEmpty(theirPath))
				yield break;

			if (myPath.EndsWith("prefab") || theirPath.EndsWith("prefab")) {
				PrefabMerge(myPath, theirPath);
				yield break;
			}

			if (AssetDatabase.LoadAssetAtPath(myPath, typeof(UnityObject))
				&& AssetDatabase.LoadAssetAtPath(theirPath, typeof(UnityObject))) {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
#else
				if(EditorApplication.SaveCurrentSceneIfUserWantsTo()) {
#endif

					var enumerator = CollectSceneSettings(myPath, theirPath);
					while (enumerator.MoveNext())
						yield return null;

					MergeScenes(myPath, theirPath);

					RecaptureSettings();

					enumerator = Refresh();
					while (enumerator.MoveNext())
						yield return null;

					yield return null;
					var objectMerge = (ObjectMerge) GetWindow(typeof(ObjectMerge));
					var root = objectMerge.root;
					root.SetGameObjects(myContainer, theirContainer);
					objectMerge.update = root.Refresh();
					objectMerge.sceneMerge = this;
					objectMerge.Repaint();
					yield return null;

					merged = true;
				}
			}
			loading = false;
		}

		static IEnumerator CollectSceneSettings(string myPath, string theirPath) {
			yield return null;
			EditorSceneManager.OpenScene(myPath);
			SceneData.Capture(true);

			yield return null;
			EditorSceneManager.OpenScene(theirPath);
			SceneData.Capture(false);
		}

		void MergeScenes(string myPath, string theirPath) {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
			var myScene = EditorSceneManager.OpenScene(myPath, OpenSceneMode.Additive);
#else
#if UNITY_5
			EditorApplication.NewEmptyScene();
#else
			EditorApplication.NewScene();
			var _allObjects = (GameObject[]) Resources.FindObjectsOfTypeAll(typeof(GameObject));
			foreach (var obj in _allObjects)
				if (obj.transform.parent == null && PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab
					&& PrefabUtility.GetPrefabType(obj) != PrefabType.ModelPrefab
					&& obj.hideFlags == 0) //Want a better way to filter out "internal" objects
					DestroyImmediate(obj);
#endif
			EditorApplication.OpenSceneAdditive(myPath);
#endif

			var split = myPath.Split('/');
			myContainerName = split[split.Length - 1].Replace(".unity", "");
			this.myContainer = new GameObject { name = myContainerName };
			var myContainer = this.myContainer;
			Undo.RegisterCreatedObjectUndo(myContainer, "UniMerge");

			var myTransform = myContainer.transform;
			var allObjects = (GameObject[]) Resources.FindObjectsOfTypeAll(typeof(GameObject));

			foreach (var obj in allObjects)
				if (obj.transform.parent == null && PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab
					&& PrefabUtility.GetPrefabType(obj) != PrefabType.ModelPrefab
					&& obj.hideFlags == 0) //Want a better way to filter out "internal" objects
					obj.transform.parent = myTransform;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			SceneManager.MergeScenes(myScene, newScene);
#endif

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			var theirScene = EditorSceneManager.OpenScene(theirPath, OpenSceneMode.Additive);
			SceneManager.MergeScenes(theirScene, newScene);
#else
			EditorSceneManager.OpenSceneAdditive(theirPath);
#endif

			split = theirPath.Split('/');
			theirContainerName = split[split.Length - 1].Replace(".unity", "");

			this.theirContainer = new GameObject { name = theirContainerName };
			var theirContainer = this.theirContainer;
			Undo.RegisterCreatedObjectUndo(theirContainer, "UniMerge");

			allObjects = (GameObject[]) Resources.FindObjectsOfTypeAll(typeof(GameObject));

			foreach (var obj in allObjects)
				if (obj.transform.parent == null && obj.name != myContainerName
					&& PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab
					&& PrefabUtility.GetPrefabType(obj) != PrefabType.ModelPrefab
					&& obj.hideFlags == 0) //Want a better way to filter out "internal" objects
					obj.transform.parent = theirContainer.transform;
		}

		void RecaptureSettings() {
			mySceneData = SceneData.RecaptureSettings(true);
			mySO = new SerializedObject(mySceneData);

			theirSceneData = SceneData.RecaptureSettings(false);
			theirSO = new SerializedObject(theirSceneData);

			SceneData.Cleanup();
		}

		public override IEnumerator Refresh() {
			var enumerator = PropertyHelper.UpdatePropertyList(properties, mySO, theirSO, null, null, null, null, this, null, this);
			while (enumerator.MoveNext())
				yield return null;

			foreach (var helper in properties) {
				var children = helper.children;
				var tmp = new List<PropertyHelper>(children);
				foreach (var h in tmp)
					if (h.property.propertyPath.Contains("sunPath"))
						children.Remove(h);
			}
		}

		protected override int GetDrawCount() {
			var count = 0;
			foreach (var property in properties) { count += property.GetDrawCount(); }
			return count;
		}
	}
}
