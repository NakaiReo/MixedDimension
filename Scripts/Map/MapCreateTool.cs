using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

/// <summary>
/// マップ制作ツール
/// </summary>
public class MapCreateTool : MonoBehaviour
{
#if UNITY_EDITOR

	public Vector3Int mapSize; //マップサイズ

	[Space]
	[SerializeField] Transform player; //プレイヤーのオブジェクト

	[Space]
	[SerializeField] public Transform mapPivot; //マップの基準位置
	[SerializeField] public Transform floor; //床オブジェクト
	[SerializeField] public Transform mapObjects; //マップのオブジェクト

	[HideInInspector] public string stageName; //ステージの名前
	[HideInInspector] public GameObject stageDataFile; //ステージのデータファイル

    void Start()
    {
		MapResize();

		GameObject startPos = mapObjects.Find("Start").gameObject;

		if(startPos != null) player.transform.position = startPos.transform.position;
	}

	/// <summary>
	/// マップのリサイズ
	/// </summary>
	public void MapResize()
	{
		mapPivot.localPosition = Vector3.Scale(mapSize, new Vector3(0.5f, 0.0f, 0.5f)) * -1;
		floor.localScale = new Vector3(mapSize.x, -1.0f, mapSize.z);
	}

	/// <summary>
	/// マップの保存
	/// </summary>
	/// <param name="stageName"></param>
	public void SaveMap(string stageName)
	{
		mapObjects.name = stageName;

		//スタート位置を入手
		Transform startCube = null;
		for(int i = 0; i < mapObjects.childCount; i++)
		{
			if (mapObjects.GetChild(i).GetComponent<CubeData>().GetCubeType() != CubeData.CubeType.Start) continue;

			startCube = mapObjects.GetChild(i);
			break;
		}

		//スタート位置がなければ保存しない
		if (startCube == null)
		{
			Debug.LogError("Startが存在しません!");
			return;
		}

		//マップデータに名前とスタート位置を渡す
		mapObjects.GetComponent<MapData>().SaveData(stageName, mapSize,startCube);

		//プレハブ化して保存する
		string localPath = "Assets/MapData/" + stageName + ".prefab";
		PrefabUtility.SaveAsPrefabAssetAndConnect(mapObjects.gameObject, localPath, InteractionMode.AutomatedAction);

		Debug.Log("保存が完了しました! >> " + stageName);
	}

	private void OnValidate()
	{
		MapResize();
	}
#endif
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(MapCreateTool))]
public class MapCreateToolEditor : Editor
{

	public override void OnInspectorGUI()
	{
		MapCreateTool script = target as MapCreateTool;

		base.OnInspectorGUI();

		EditorGUILayout.Space(30);

		//新規マップを作成する
		if (GUILayout.Button("新規マップ"))
		{
			if (script.mapObjects != null) DestroyImmediate(script.mapObjects.gameObject);

			GameObject ins = new GameObject("New Map Objects");
			ins.transform.SetParent(script.mapPivot, false);
			ins.transform.localPosition = Vector3.zero;
			ins.AddComponent<MapData>();

			script.mapObjects = ins.transform;
		}
		EditorGUILayout.Space(10);

		//ステージの名前を入力
		GUILayout.Label("ステージの名前");
		script.stageName = GUILayout.TextField(script.stageName);

		//ステージを保存する
		if (GUILayout.Button("保存"))
		{
			script.SaveMap(script.stageName);
		}

		EditorGUILayout.Space();

		//保存するデータの位置
		GUILayout.Label("ステージのファイル");
		script.stageDataFile = (GameObject)EditorGUILayout.ObjectField(script.stageDataFile, typeof(GameObject), true);

		//ステージの読み込み
		if (GUILayout.Button("読み込み"))
		{
			if (script.stageDataFile != null)
			{
				MapGenerator mapGenerator = script.GetComponent<MapGenerator>();
				script.mapObjects = mapGenerator.MapLoad(script.mapObjects, script.stageDataFile);
				script.mapSize = mapGenerator.mapSize;
				script.MapResize();
			}
		}

		EditorGUILayout.Space(20);

		//スクリーンショットを撮る
		if (GUILayout.Button("スクリーンショット"))
		{
			string path = "Assets/ScreenShot/" + script.mapObjects.name + ".png";
			ScreenCapture.CaptureScreenshot(string.Format(path));

			Debug.Log("撮影が完了しました >> " + path);
		}
	}
}
#endif