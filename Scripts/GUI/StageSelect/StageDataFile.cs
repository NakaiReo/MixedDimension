using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Mochineko.SimpleReorderableList.Samples.Editor;
using UnityEditor;
using UnityEditorInternal;
using Mochineko.SimpleReorderableList;
#endif

/// <summary>
/// ステージデータのリスト
/// </summary>
[CreateAssetMenu(fileName = "StageDataFile", menuName = "ScriptableObject/StageDataFile")]
public class StageDataFile : ScriptableObject
{
	public List<StageDataObject> data;
}

#if UNITY_EDITOR
namespace Mochineko.SimpleReorderableList.StageDataFileEditor.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(StageDataFile))]
	public class StageDataFileEditor : UnityEditor.Editor
	{
		private ReorderableList reorderableList;

		private void OnEnable()
		{
			reorderableList = new ReorderableList(
				serializedObject.FindProperty("data")
				);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginChangeCheck();
			{
				EditorFieldUtility.ReadOnlyScriptableObjectField(target as ScriptableObject, this);

				if(reorderableList != null)
				{
					reorderableList.Layout();
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
#endif