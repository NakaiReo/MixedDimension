using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マップの情報
/// </summary>
public class MapData : MonoBehaviour
{
	public string mapName; //マップの名前
	public Vector3Int mapSize; //マップのサイズ

	public Transform startCube; //スタート位置

	/// <summary>
	/// マップのデータの保存
	/// </summary>
	/// <param name="mapName">マップの名前</param>
	/// <param name="mapSize">マップのサイズ</param>
	/// <param name="startCube">スタート位置</param>
	public void SaveData(string mapName, Vector3Int mapSize, Transform startCube)
	{
		this.mapName = mapName;
		this.mapSize = mapSize;
		this.startCube = startCube;
	}

	/// <summary>
	/// キューブのID
	/// </summary>
	enum ID
	{
		Error = -99,
		Start = -1,
		Goal = -2,
		None = 0,
		Block = 1,
		MoveBlock = 2,
	}

	/// <summary>
	/// キューブのタイプを取得
	/// </summary>
	/// <param name="tagName"></param>
	/// <returns></returns>
	public static int TagNameToID(string tagName)
	{
		switch (tagName)
		{
			case "Start":
				return -1;
			case "Goal":
				return -2;
			case "Block":
				return 1;
		}

		return 0;
	}
}
