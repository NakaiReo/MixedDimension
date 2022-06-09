using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージごとのデータ
/// </summary>
[CreateAssetMenu(fileName = "StageDataObject", menuName = "ScriptableObject/StageDataObject")]
public class StageDataObject : ScriptableObject
{
	/// <summary>
	/// 難易度
	/// </summary>
	public enum Difficult
	{
		Tutorial,
		Easy,
		Normal,
		Hard
	}
	public Difficult difficult; //難易度
	public int stageNumber;     //難易度ごとのステージ番号

	[Space]
	public string mapName; //マップの名前
	[Multiline(4)] public string mapLore; //マップの説明
	public GameObject mapData; //マップのプレハブ
	public Sprite mapImage; //マップのイメージ
}