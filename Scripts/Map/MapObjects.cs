using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapObejcts", menuName = "ScriptableObject/MapObjects")]
public class MapObjects : ScriptableObject
{
	public GameObject start;
	public GameObject goal;

	[Space]

	public GameObject normalCube;
	public GameObject plane;
	public GameObject moveCube;

	[Space]

	public GameObject checkPoint;
}
