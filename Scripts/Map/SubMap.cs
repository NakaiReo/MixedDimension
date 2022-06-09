using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マップとしてではなく見るために使うときのクラス
/// </summary>
public class SubMap : MonoBehaviour
{
	public static GameObject currentCamera = null;
	public StageDataObject stageData;

	Vector3Int mapSize;

	[SerializeField] Transform mapPivot;
	[SerializeField] Transform roomObjects;
	[SerializeField] Transform stageObjectFile;

	[Space]

	public GameObject viewCamera;

    void Start()
    {
		mapSize = stageData.mapData.GetComponent<MapData>().mapSize;

		mapPivot.localPosition = Vector3.Scale(mapSize, new Vector3(0.5f, 0.0f, 0.5f)) * -1;
		roomObjects.Find("Floor").localScale = new Vector3(mapSize.x, -10.0f, mapSize.z);
		roomObjects.Find("Wall").localScale = mapSize + Vector3.up * 3;
		GetComponent<MapBooder>().ExtendBooder(mapSize);
	}
}
