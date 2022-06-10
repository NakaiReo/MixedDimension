using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapObejcts", menuName = "ScriptableObject/MapObjects")]
public class MapObjects : ScriptableObject
{
	public GameObject start; //開始地点
	public GameObject goal;  //ゴール

	[Space]

	public GameObject normalCube; //通常のブロック
	public GameObject plane;      //壁
	public GameObject moveCube;   //移動するブロック

	[Space]
	public GameObject checkPoint; //チェックポイント
}
