using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoCube : CubeData
{
	public bool isBack;

	private void Start()
	{
		base.Start();

		if(GameDirector.ins != null) GameDirector.ins.TutorialText = gameObject;
	}

	private void Update()
	{
		isBack = isBackGround;
	}

	public override Color GetCubeColor()
	{
		return Color.white;
	}

	public override CubeType GetCubeType()
	{
		return CubeType.None;
	}

	public override bool DimensionCollider(SecondDimentionData data)
	{
		return false;
	}
}