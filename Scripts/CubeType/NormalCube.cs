using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalCube : CubeData
{
	public bool isBack;

	public Color cubeColor;

	private void Update()
	{
		isBack = isBackGround;
	}

	public override Color GetCubeColor()
	{
		return cubeColor;
	}

	public override CubeType GetCubeType()
	{
		return CubeType.Normal;
	}

	public override bool DimensionCollider(SecondDimentionData data)
	{
		return true;
	}

	public override Material GetMaterial()
	{
		return Resources.Load("NormalMaterial") as Material;
	}
}