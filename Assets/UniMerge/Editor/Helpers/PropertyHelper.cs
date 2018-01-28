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
//PropertyHelper class

#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define Unity3
#endif

using System.Collections;
using System.Collections.Generic;
using UniMerge.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace UniMerge.Editor.Helpers {
	public class PropertyHelper : Helper {
		struct StackHelper {
			public readonly PropertyHelper helper;
			public readonly int siblingIndex;

			public StackHelper(PropertyHelper helper, int siblingIndex) {
				this.helper = helper;
				this.siblingIndex = siblingIndex;
			}
		}

		public SerializedProperty mine { get; private set; }
		public SerializedProperty theirs { get; private set; }

		public List<PropertyHelper>children { get; private set; }

		public SerializedProperty property {
			get {
				// ReSharper disable once ConvertIfStatementToReturnStatement
				if (mine != null)
					return mine;

				return theirs;
			}
		}

		readonly GameObjectHelper gameObjectParent;
		readonly ComponentHelper componentParent;
		readonly PropertyHelper propertyParent;

		readonly string propertyPath;
		readonly SerializedPropertyType propertyType;

		GameObjectHelper root { get { return objectMerge ? objectMerge.root : null; } }

		//Local use only for GC
		readonly List<PropertyHelper> tmpList = new List<PropertyHelper>(); // Local because this is used in recursive draw call
		static readonly Stack<StackHelper> HelperStack = new Stack<StackHelper>();
		static readonly Stack<StackHelper> HelperCountStack = new Stack<StackHelper>();

		static readonly Stack<PropertyHelper> PropertyHelperStack = new Stack<PropertyHelper>();

		static SerializedProperty propertyArg; // Used for indent delegates
		static PropertyHelper thisArg;

		public PropertyHelper(SerializedProperty mine, SerializedProperty theirs, string propertyPath,
			SerializedPropertyType propertyType, GameObjectHelper gameObjectParent, ComponentHelper componentParent,
			PropertyHelper propertyParent, Helper parent, UniMergeWindow window,
			ObjectMerge objectMerge, SceneMerge sceneMerge) : base(parent, window, objectMerge, sceneMerge) {
			this.mine = mine;
			this.theirs = theirs;
			this.propertyPath = propertyPath;
			this.propertyType = propertyType;

			this.gameObjectParent = gameObjectParent;
			this.componentParent = componentParent;
			this.propertyParent = propertyParent;
		}

		public void CheckSame() {
			var mine = this.mine;
			var theirs = this.theirs;
			if (mine == null || theirs == null) {
				Same = false;
				return;
			}

			if (sceneMerge && !sceneMerge.compareLightingData) {
				if (propertyPath.Contains("lightingData") || propertyPath.Contains("lightmaps")) {
					Same = true;
					return;
				}
			}

#if !UNITY_3_4 && !UNITY_3_3 && !UNITY_3_2 && !UNITY_3_1 && !UNITY_3_0_0 && !UNITY_3_0
			if (propertyType == SerializedPropertyType.ArraySize) {
				Same = propertyParent.mine.arraySize == propertyParent.theirs.arraySize;
				return;
			}
#endif

			GameObject myRoot = null;
			GameObject theirRoot = null;
			if (objectMerge) {
				var root = objectMerge.root;
				myRoot = root.mine;
				theirRoot = root.theirs;
			}

			Same = Util.PropEqual(mine, theirs, myRoot, theirRoot);
		}

		public static IEnumerator DeepCheckSame(PropertyHelper root) {
			//Clear stacks in case of canceled refresh
			HelperStack.Clear();

			HelperStack.Push(new StackHelper(root, 0));
			while (HelperStack.Count > 0) {
				var pop = HelperStack.Pop();
				var helper = pop.helper;
				var i = pop.siblingIndex;
				if (i == 0)
					helper.CheckSame();

				var children = helper.children;
				if (children != null) {
					if (children.Count > i) {
						HelperStack.Push(new StackHelper(helper, i + 1));
						HelperStack.Push(new StackHelper(children[i], 0));
					} else {
						foreach (var child in children) {
							if (child.Same)
								continue;

							helper.Same = false;
							break;
						}
					}
				}
				yield return null;
			}

#if UNIMERGE_ASSERTS
			Debug.Assert(HelperStack.Count == 0);
#endif
		}

		public SerializedProperty GetProperty(bool isMine) { return isMine ? mine : theirs; }

		public void Draw(float indent, GUILayoutOption colWidth, GUILayoutOption indentOption) {
			if (window.drawAbort)
				return;

			if (!window || window.ScrollCheck()) {
				window.StartRow(Same);

				var foldoutState = showChildren;

				thisArg = this;
				DrawProperty(true, indent, colWidth, indentOption);

				//Swap buttons
				var mine = this.mine;
				var theirs = this.theirs;
				var hasBothParents = gameObjectParent != null && gameObjectParent.mine && gameObjectParent.theirs
					|| componentParent != null && componentParent.mine && componentParent.theirs
					|| propertyParent != null && propertyParent.mine != null && propertyParent.theirs != null
					|| mine != null && theirs != null && mine.depth == 0;
				if (hasBothParents)
					DrawMidButtons(mine != null, theirs != null, LeftButton, RightButton, LeftDeleteButton, RightDeleteButton);
				else
					GUILayout.Space(UniMergeConfig.DoubleMidWidth);

				DrawProperty(false, indent, colWidth, indentOption);

				if (foldoutState != showChildren) {
					thisArg.InvalidateDrawCount();
					if (Event.current.alt)
						thisArg.SetFoldoutRecursively(showChildren);
				}

				window.EndRow(Same);
			}

			var children = this.children;
			if (children != null) {
				if (showChildren) {
					tmpList.Clear();
					var count = children.Count;
					for (var i = 0; i < count; i++)
						tmpList.Add(children[i]);
					var newWidth = indent + Util.TabSize;
					var newIndent = GUILayout.Width(newWidth);
					for (var i = 0; i < count; i++)
						tmpList[i].Draw(newWidth, colWidth, newIndent);
				}
			}
		}

		static void LeftButton() {
			thisArg.Copy(true);
			thisArg.window.update = thisArg.BubbleRefresh();
		}

		static void RightButton() {
			thisArg.Copy(false);
			thisArg.window.update = thisArg.BubbleRefresh();
		}

		static void LeftDeleteButton() {
			var propertyParent = thisArg.propertyParent;
			var children = propertyParent.children;
			if (children != null) {
#if !UNITY_3_4 && !UNITY_3_3 && !UNITY_3_2 && !UNITY_3_1 && !UNITY_3_0_0 && !UNITY_3_0
				var index = children.IndexOf(thisArg) - 1; // Index - 1 because of array size
				propertyParent.mine.DeleteArrayElementAtIndex(index);
#endif
				children.Clear(); //Clear PropertyHelpers whenever we remove array elements
			}

			thisArg.window.update = thisArg.BubbleRefresh();
		}

		static void RightDeleteButton() {
			var propertyParent = thisArg.propertyParent;
			var children = propertyParent.children;
			if (children != null) {
#if !UNITY_3_4 && !UNITY_3_3 && !UNITY_3_2 && !UNITY_3_1 && !UNITY_3_0_0 && !UNITY_3_0
				var index = children.IndexOf(thisArg) - 1; // Index - 1 because of array size
				propertyParent.theirs.DeleteArrayElementAtIndex(index);
#endif
				children.Clear(); //Clear PropertyHelpers whenever we remove array elements
			}

			thisArg.window.update = thisArg.BubbleRefresh();
		}

		public void Copy(bool toMine) {
#if !UNITY_3_4 && !UNITY_3_3 && !UNITY_3_2 && !UNITY_3_1 && !UNITY_3_0_0 && !UNITY_3_0
			if (propertyType == SerializedPropertyType.ArraySize) {
				var leftArray = propertyParent.GetProperty(toMine);
				var rightArray = propertyParent.GetProperty(!toMine);
				leftArray.arraySize = rightArray.arraySize;

				var children = propertyParent.children;
				if (children != null)
					children.Clear(); //Clear PropertyHelpers whenever we remove array elements

				return;
			}
#endif

			var left = GetProperty(toMine);
			var right = GetProperty(!toMine);

#if !UNITY_3_4 && !UNITY_3_3 && !UNITY_3_2 && !UNITY_3_1 && !UNITY_3_0_0 && !UNITY_3_0
			if (left == null) {
				//Insert should only occur for arrays
				var array = propertyParent.GetProperty(toMine);
				var count = array.arraySize;
				array.InsertArrayElementAtIndex(count);
				Util.SetProperty(right, array.GetArrayElementAtIndex(count));
				return;
			}
#endif

			var leftSerializedObject = left.serializedObject;
			var leftTargetObject = leftSerializedObject.targetObject;
			if (left.propertyType == SerializedPropertyType.ObjectReference) {
				var rightObjectReferenceValue = right.objectReferenceValue;
				if (rightObjectReferenceValue != null) {
					var t = rightObjectReferenceValue.GetType();
					if (window)
						if (t == typeof(GameObject)) {
							var g = root.GetObjectSpouse((GameObject) rightObjectReferenceValue, true);
							// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
							if (g == null)
								left.objectReferenceValue = rightObjectReferenceValue;
							else
								left.objectReferenceValue = g;
						} else if (t.IsSubclassOf(typeof(Component))) {
							var g = root.GetObjectSpouse(((Component) rightObjectReferenceValue).gameObject, true);
							if (g) {
								var c = g.GetComponent(t);
								left.objectReferenceValue = c ? c : g.AddComponent(t);
							} else { left.objectReferenceValue = rightObjectReferenceValue; }
						} else { left.objectReferenceValue = rightObjectReferenceValue; }
					else
						left.objectReferenceValue = rightObjectReferenceValue;

					if (leftTargetObject != null)
						leftSerializedObject.ApplyModifiedProperties();

					return;
				}
			}

#if Unity3
			Util.SetProperty(right, left);
#else
			leftSerializedObject.CopyFromSerializedProperty(right);
#endif

			if (leftTargetObject != null)
				leftSerializedObject.ApplyModifiedProperties();
		}

		void DrawProperty(bool isMine, float indent, GUILayoutOption colWidth, GUILayoutOption indentOption) {
			if (window.drawAbort)
				return;

			GUILayout.BeginVertical(colWidth);
			propertyArg = GetProperty(isMine);
			if (propertyArg != null) {
#if !UNITY_3_4 && !UNITY_3_3 && !UNITY_3_2 && !UNITY_3_1 && !UNITY_3_0_0 && !UNITY_3_0
				EditorGUI.BeginChangeCheck();
#endif

				switch (propertyType) {
#if UNITY_4_5 || UNITY_4_5_0 || UNITY_4_6 || UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER
					case SerializedPropertyType.Quaternion:
					case SerializedPropertyType.Vector4:
#elif !Unity3
					case (SerializedPropertyType) 16:
#endif
					case SerializedPropertyType.Vector2:
					case SerializedPropertyType.Vector3:
#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3)
					case SerializedPropertyType.Bounds:
#endif
					case SerializedPropertyType.Generic:
					case SerializedPropertyType.Rect:
						Util.Indent(indentOption, DrawFoldoutProperty);
						break;
					default:
						var children = this.children;
						if (children != null && children.Count > 0) {

							Util.Indent(GUILayout.Width(indent + Util.TabSize), DrawChildren);
						} else { Util.Indent(indentOption, DrawSingleProperty); }
						break;
				}

#if !UNITY_3_4 && !UNITY_3_3 && !UNITY_3_2 && !UNITY_3_1 && !UNITY_3_0_0 && !UNITY_3_0
				if (EditorGUI.EndChangeCheck()) {
#endif
					if (propertyArg.serializedObject != null && propertyArg.serializedObject.targetObject != null) {
						if (propertyArg.serializedObject.ApplyModifiedProperties()) {
							if (propertyType == SerializedPropertyType.ArraySize) {
								var children = propertyParent.children;
								if (children != null)
									children.Clear();
							}

							window.update = BubbleRefresh();
						}
#if !UNITY_3_4 && !UNITY_3_3 && !UNITY_3_2 && !UNITY_3_1 && !UNITY_3_0_0 && !UNITY_3_0
					}
#endif
				}
			} else {
				GUILayout.Label("");
				GUILayout.Space(EmptyRowSpace);
			}
			GUILayout.EndVertical();
		}

		static void DrawFoldoutProperty() {
			var showChildren = thisArg.showChildren;
			var lastState = showChildren;
#if UNITY_5_5_OR_NEWER
			showChildren = EditorGUILayout.Foldout(showChildren, ObjectNames.NicifyVariableName(propertyArg.name), true);
#else
			showChildren = EditorGUILayout.Foldout(showChildren, ObjectNames.NicifyVariableName(propertyArg.name));
#endif

			if (lastState != showChildren)
				thisArg.InvalidateDrawCount();

			thisArg.showChildren = showChildren;
		}

		static void DrawChildren() {
			GUILayout.BeginHorizontal();
#if Unity3
			EditorGUILayout.PropertyField(propertyArg);
#else
			EditorGUILayout.PropertyField(propertyArg, false);
#endif
			var lastRect = GUILayoutUtility.GetLastRect();
			lastRect.xMin -= Util.TabSize;
			var showChildren = thisArg.showChildren;
#if !UNITY_3_4 && !UNITY_3_3 && !UNITY_3_2 && !UNITY_3_1 && !UNITY_3_0_0 && !UNITY_3_0
			showChildren = EditorGUI.Foldout(lastRect, showChildren, " ", true);
#else
			showChildren = EditorGUI.Foldout(lastRect, showChildren, " ");
#endif
			GUILayout.EndHorizontal();

			thisArg.showChildren = showChildren;
		}

		static void DrawSingleProperty() {
#if Unity3
			EditorGUILayout.PropertyField(propertyArg);
#else
			EditorGUILayout.PropertyField(propertyArg, false);
#endif
		}

		public static void GetProperties(List<SerializedProperty> properties, SerializedObject obj) {
			var iterator = obj.GetIterator();
			while (iterator.Next(true))
				properties.Add(iterator.Copy());
		}

		public static IEnumerator<bool> UpdatePropertyList(List<PropertyHelper> properties, SerializedObject myObject,
			SerializedObject theirObject, GameObjectHelper gameObjectParent, ComponentHelper componentParent,
			PropertyHelper propertyParent, ObjectMerge objectMerge, SceneMerge sceneMerge,
			Helper parent, UniMergeWindow window, bool showHidden = false) {
			var myObjectIsNull = myObject == null;
			var theirObjectIsNull = theirObject == null;
			if (myObjectIsNull && theirObjectIsNull)
				yield break;

			SerializedProperty myIterator = null;
			if (!myObjectIsNull)
				myIterator = myObject.GetIterator();

			SerializedProperty theirIterator = null;
			if (!theirObjectIsNull)
				theirIterator = theirObject.GetIterator();

			if (theirIterator != null)
				theirIterator.Reset();

			var isGameObject = (myObjectIsNull ? theirObject.targetObject : myObject.targetObject) is GameObject;
			var isTransform = (myObjectIsNull ? theirObject.targetObject : myObject.targetObject) is Transform;
			var tempShowHiddenDepth = -1;
			var tempShowHidden = false;
			var same = true;
			var mineHasNext = myIterator != null;
			var theirsHasNext = theirIterator != null;
			var lastDepth = 0;
			PropertyHelper lastHelper = null;
			var root = parent;
			var gameObjectRoot = gameObjectParent;
			var componentRoot = componentParent;
			var propertyRoot = propertyParent;
			var ignored = false;
			while (mineHasNext || theirsHasNext) {
				var _myIterator = myIterator;
				var _theirIterator = theirIterator;
				var iterator = _myIterator != null && mineHasNext ? _myIterator : _theirIterator;

#if UNITY_4 || UNITY_5 || UNITY_5_3_OR_NEWER
				if (iterator.propertyType == SerializedPropertyType.Gradient) {
					tempShowHiddenDepth = iterator.depth;
					tempShowHidden = true;
				} else
#endif
				if (iterator.depth == tempShowHiddenDepth) {
					tempShowHidden = false;
					tempShowHiddenDepth = -1;
				}

				if (mineHasNext && theirsHasNext) {
					if (myIterator.depth > theirIterator.depth) {
						//Catch up myIterator
						if (showHidden || tempShowHidden)
							mineHasNext &= myIterator.Next(!ignored);
						else
							mineHasNext &= myIterator.NextVisible(!ignored);
					} else if (theirIterator.depth > myIterator.depth && theirsHasNext) {
						// Catch up theirIterator
						if (showHidden || tempShowHidden)
							theirsHasNext &= theirIterator.Next(!ignored);
						else
							theirsHasNext &= theirIterator.NextVisible(!ignored);
					} else {
						if (showHidden || tempShowHidden) {
							mineHasNext &= myIterator.Next(!ignored);
							theirsHasNext &= theirIterator.Next(!ignored);
						} else {
							mineHasNext &= myIterator.NextVisible(!ignored);
							theirsHasNext &= theirIterator.NextVisible(!ignored);
						}
					}

					if (mineHasNext && theirsHasNext) {
						if (myIterator.depth > theirIterator.depth) // Missing elements in mine
							_theirIterator = null;

						if (theirIterator.depth > myIterator.depth) // Missing elements in theirs
							_myIterator = null;
					}
				} else {
					if (mineHasNext)
						if (showHidden || tempShowHidden)
							mineHasNext &= myIterator.Next(!ignored);
						else
							mineHasNext &= myIterator.NextVisible(!ignored);

					if (theirsHasNext)
						if (showHidden || tempShowHidden)
							theirsHasNext &= theirIterator.Next(!ignored);
						else
							theirsHasNext &= theirIterator.NextVisible(!ignored);
				}

				if (!mineHasNext && !theirsHasNext)
					break;

				if (!mineHasNext)
					_myIterator = null;

				if (!theirsHasNext)
					_theirIterator = null;

				// Get new iterator if one has become null
				// ReSharper disable once ConvertIfStatementToNullCoalescingExpression
				if (_myIterator == null)
					iterator = _theirIterator;
				else
					iterator = _myIterator;

				var path = iterator.propertyPath;
				var type = iterator.propertyType;
				ignored = path == "m_Script";

				if (isGameObject) {
					ignored = type == SerializedPropertyType.ObjectReference || type == SerializedPropertyType.Generic;
				} else if (isTransform) {
#if UNITY_4_5 || UNITY_4_5_0 || UNITY_4_6 || UNITY_4_7 || UNITY_5 || UNITY_5_3_OR_NEWER
					ignored = type != SerializedPropertyType.Vector3 && type != SerializedPropertyType.Quaternion && type != SerializedPropertyType.Float;
#elif !Unity3
					ignored = type != SerializedPropertyType.Vector3 && type != (SerializedPropertyType)16 && type != SerializedPropertyType.Float;
#else
					ignored = type != SerializedPropertyType.Vector3 && type != SerializedPropertyType.Float;
#endif
				}

				if (ignored)
					continue;

				PropertyHelper ph = null;
				var count = properties.Count;
				for (var i = 0; i < count; i++) {
					var property = properties[i];
					if (property.propertyPath == path) {
						ph = property;
						break;
					}
				}

				var depth = iterator.depth;
				if (depth > lastDepth) {
					parent = lastHelper;
					propertyParent = lastHelper;
				}

				if (depth < lastDepth && parent != null) {
					parent = parent.parent;
					propertyParent = propertyParent.propertyParent;
				}

				if (depth > 0) {
					var children = propertyParent.children;
					if (children != null) {
						count = children.Count;
						for (var i = 0; i < count; i++) {
							var child = children[i];
							if (child.propertyPath == path) {
								ph = child;
								break;
							}
						}
					}
				}

				SerializedProperty myIteratorCopy = null;
				if (_myIterator != null)
					myIteratorCopy = _myIterator.Copy();

				SerializedProperty theirIteratorCopy = null;
				if (_theirIterator != null)
					theirIteratorCopy = _theirIterator.Copy();

				if (ph == null) {
					if (depth == 0) {
						ph = new PropertyHelper(myIteratorCopy, theirIteratorCopy, path, type,
							gameObjectRoot, componentRoot, propertyRoot, root, window, objectMerge, sceneMerge);
						properties.Add(ph);
					} else {
						ph = new PropertyHelper(myIteratorCopy, theirIteratorCopy, path, type,
							gameObjectParent, componentParent, propertyParent, parent, window, objectMerge, sceneMerge);
						var children = propertyParent.children;
						if (children == null)
							propertyParent.children = children = new List<PropertyHelper>(1);

						children.Add(ph);
						
					}
				} else {
					ph.mine = myIteratorCopy;
					ph.theirs = theirIteratorCopy;
				}

				lastHelper = ph;
				lastDepth = depth;
			}

			for (var i = 0; i < properties.Count; i++) {
				var property = properties[i];
				if (property.children == null) {
					property.CheckSame();
				} else {
					var enumerator = DeepCheckSame(property);
					while (enumerator.MoveNext()) {
						yield return false;
					}
				}

				if (!property.Same)
					same = false;
			}

			yield return same;
		}

		public override IEnumerator Refresh() {
			if (children == null) {
				CheckSame();
			} else {
				var enumerator = DeepCheckSame(this);
				while (enumerator.MoveNext())
					yield return null;
			}
		}

		public override int GetDrawCount() {
			return GetCount(this);
		}

		static int GetCount(PropertyHelper root) {
#if UNIMERGE_ASSERTS
			Debug.Assert(HelperCountStack.Count == 0);
#endif

			var count = 0;
			HelperCountStack.Push(new StackHelper(root, 0));
			while (HelperCountStack.Count > 0) {
				var pop = HelperCountStack.Pop();
				var helper = pop.helper;
				var i = pop.siblingIndex;

				var children = helper.children;
				if (i == 0)
					count++;

				if (children != null && helper.showChildren) {
					if (children.Count > i) {
						HelperCountStack.Push(new StackHelper(helper, i + 1));
						HelperCountStack.Push(new StackHelper(children[i], 0));
					}
				}
			}

#if UNIMERGE_ASSERTS
			Debug.Assert(HelperCountStack.Count == 0);
#endif

			return count;
		}

		public void ToList(List<PropertyHelper> list) {
#if UNIMERGE_ASSERTS
			Debug.Assert(PropertyHelperStack.Count == 0);
#endif

			PropertyHelperStack.Push(this);

			while (PropertyHelperStack.Count > 0) {
				var helper = PropertyHelperStack.Pop();
				list.Add(helper);
				var children = helper.children;
				if (children != null) {
					var count = children.Count;
					for (var i = 0; i < count; i++)
						PropertyHelperStack.Push(children[i]);
				}
			}

#if UNIMERGE_ASSERTS
			Debug.Assert(PropertyHelperStack.Count == 0);
#endif
		}

		public void SetFoldoutRecursively(bool state) {
			showChildren = state;
			if (children != null)
				foreach (var child in children)
					child.SetFoldoutRecursively(state);
		}
	}
}
