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
//ComponentHelper class

#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define Unity3
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UniMerge.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace UniMerge.Editor.Helpers {
	public class ComponentHelper : Helper {
		ComponentContainer componentContainer;
		SerializedProperty mineProp, theirsProp;

		public Component mine { get { return componentContainer.mine; } }
		public Component theirs { get { return componentContainer.theirs; } }

		public readonly Type type;
		public readonly List<PropertyHelper> properties = new List<PropertyHelper>(4);

		SerializedObject mySO, theirSO;
		new readonly GameObjectHelper parent; // ComponentHelpers can only be children of GameObjectHelpers
		new readonly ObjectMerge window; // ComponentHelpers can only be in ObjectMerge windows

		//Used as an arguments to delegated static methods
		static Component componentArg;
		static ComponentHelper thisArg;
		static float indentArg;

		public ComponentHelper(Component mine, Component theirs, GameObjectHelper parent = null,
			ObjectMerge window = null) : base(parent, window, window, null) {
			this.parent = parent;
			this.window = window;
			SetComponents(mine, theirs);

			type = mine ? mine.GetType() : theirs.GetType();
		}

		public void SetComponents(Component mine, Component theirs) {
			if (componentContainer == null)
				componentContainer = ComponentContainer.Create(out mineProp, out theirsProp);

			mineProp.objectReferenceValue = mine;
			theirsProp.objectReferenceValue = theirs;

			theirsProp.serializedObject.ApplyModifiedProperties();
		}

		public Component GetComponent(bool isMine) { return isMine ? mine : theirs; }

		public override IEnumerator Refresh() {
			var mine = this.mine;
			if (mine)
				mySO = new SerializedObject(mine);

			var theirs = this.theirs;
			if (theirs)
				theirSO = new SerializedObject(theirs);

			var enumerator = PropertyHelper.UpdatePropertyList(properties, mySO, theirSO, null, this, null, objectMerge, sceneMerge, this, window);
			while (enumerator.MoveNext())
				yield return null;

			Same = enumerator.Current;
		}

		public void Draw(float indent, GUILayoutOption colWidth, GUILayoutOption indentOption) {
			if (window.drawAbort)
				return;

			if (window.ScrollCheck()) {
				window.StartRow(Same);
				//Store foldout state before doing GUI to check if it changed
				var foldoutState = showChildren;
				indentArg = indent;
				thisArg = this;
				DrawComponent(true, indentOption, colWidth);
				//Swap buttons
				var parentMine = parent.mine;
				var parentTheirs = parent.theirs;
				if (parentMine && parentTheirs)
					DrawMidButtons(mine, theirs, parentMine, parentTheirs, LeftButton, RightButton, LeftDeleteButton, RightDeleteButton);
				else
					GUILayout.Space(UniMergeConfig.DoubleMidWidth);
				//Display theirs
				DrawComponent(false, indentOption, colWidth);

				if (showChildren != foldoutState) {
					InvalidateDrawCount();
					//If foldout state changed and user was holding alt, set all child foldout states to this state
					if (Event.current.alt) {
						foreach (var property in properties)
							property.SetFoldoutRecursively(showChildren);
					}
				}
				window.EndRow(Same);
			}

			if (showChildren) {
				var tmp = new List<PropertyHelper>(properties);

				var newWidth = indent + Util.TabSize;
				var newIndent = GUILayout.Width(newWidth);
				foreach (var property in tmp)
					property.Draw(newWidth, colWidth, newIndent);
			}

			if (mySO != null && mySO.targetObject != null)
				if (mySO.ApplyModifiedProperties())
					window.update = BubbleRefresh();

			if (theirSO != null && theirSO.targetObject != null)
				if (theirSO.ApplyModifiedProperties())
					window.update = BubbleRefresh();
		}

		IEnumerator Delete(bool isMine) {
			var component = GetComponent(isMine);
			if (component is Camera)
			{
				var enumerator = parent.DestroyAndClearRefs(component.GetComponent<AudioListener>(), true);
				while (enumerator.MoveNext())
					yield return null;

#pragma warning disable 618
				enumerator = parent.DestroyAndClearRefs(component.GetComponent<GUILayer>(), true);
				while (enumerator.MoveNext())
					yield return null;
#pragma warning restore 618

				enumerator = parent.DestroyAndClearRefs(component.GetComponent("FlareLayer"), true);
				while (enumerator.MoveNext())
					yield return null;
			}
			var e = parent.DestroyAndClearRefs(component, true);
			while (e.MoveNext())
				yield return null;

			window.update = BubbleRefresh();
		}

		void DrawComponent(bool isMine, GUILayoutOption indent, GUILayoutOption colWidth) {
			GUILayout.BeginVertical(colWidth);

#if Unity3
			GUILayout.Space(3);
#endif

			componentArg = GetComponent(isMine);
			thisArg = this;
			Util.Indent(indent, DrawComponentRow);

#if Unity3
			GUILayout.Space(-4);
#endif

			GUILayout.EndVertical();
		}

		static void LeftButton() {
			var mine = thisArg.mine;
			var theirs = thisArg.theirs;
			// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
			if (mine)
				EditorUtility.CopySerialized(theirs, mine);
			else
				EditorUtility.CopySerialized(theirs, thisArg.parent.mine.AddComponent(thisArg.type));

			thisArg.window.update = thisArg.BubbleRefresh();
		}

		static void RightButton() {
			var mine = thisArg.mine;
			var theirs = thisArg.theirs;
			// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
			if (theirs)
				EditorUtility.CopySerialized(mine, theirs);
			else
				EditorUtility.CopySerialized(mine, thisArg.parent.theirs.AddComponent(thisArg.type));

			thisArg.window.update = thisArg.BubbleRefresh();
		}

		static void LeftDeleteButton() {
			var window = thisArg.window;
			window.updateType = RefreshType.Deleting;
			window.update = thisArg.Delete(true);
		}

		static void RightDeleteButton() {
			var window = thisArg.window;
			window.updateType = RefreshType.Deleting;
			window.update = thisArg.Delete(false);
		}

		static void DrawComponentRow() {
			if (componentArg) {
				var showChildren = thisArg.showChildren;
				var lastState = showChildren;
				GUILayout.BeginHorizontal();
				var colWidth = thisArg.window.columnWidth - indentArg;
				GUILayout.BeginHorizontal(GUILayout.Width(colWidth));
#if UNITY_5_5_OR_NEWER
				showChildren = EditorGUILayout.Foldout(showChildren, string.Empty, true);
#else
				showChildren = EditorGUILayout.Foldout(showChildren, string.Empty);
#endif
				GUILayout.EndHorizontal();
				// For some reason, the texture doens't show up in a foldout
				GUILayout.Space(-colWidth + 8);
#if Unity3
				var guiContent = thisArg.type.Name;
#else
				var guiContent = new GUIContent(thisArg.type.Name, AssetPreview.GetMiniThumbnail(componentArg));
#endif
				GUILayout.Label(guiContent, Util.LabelHeight);
				GUILayout.EndHorizontal();
				if (lastState != showChildren)
					thisArg.InvalidateDrawCount();

				thisArg.showChildren = showChildren;
			} else {
				GUILayout.Label("");
				GUILayout.Space(EmptyRowSpace);
			}
		}

		public void GetFullPropertyList(List<PropertyHelper> list) {
			for (var i = 0; i < properties.Count; i++)
				properties[i].ToList(list);
		}

		public override int GetDrawCount() {
			var count = 1;
			if (showChildren)
				foreach (var property in properties) { count += property.GetDrawCount(); }

			return count;
		}

		public void SetFoldoutRecursively(bool state) {
			showChildren = state;
			foreach (var property in properties)
				property.SetFoldoutRecursively(state);
		}
	}
}
