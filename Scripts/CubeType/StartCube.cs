using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCube : CubeData
{
	public override Color GetCubeColor()
	{
		return Color.white;
	}

	public override CubeType GetCubeType()
	{
		return CubeType.Start;
	}

	public override bool DimensionCollider(SecondDimentionData data)
	{
		return false;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;

		Gizmos.DrawSphere(transform.position + Vector3.one * 0.5f, 0.5f);
	}
}
