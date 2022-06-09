using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneCube : CubeData
{
	bool isEW = false;

	private void Start()
	{
		base.Start();

		RePosition();
	}

	public enum PlaneDireciton
	{
		NS,
		EW,
	}

	public PlaneDireciton planeDireciton;

	public override Color GetCubeColor()
	{
		return new Color(255, 255, 0, 150);
	}

	public override CubeType GetCubeType()
	{
		return CubeType.Plane;
	}

	public override void DefaultCollider()
	{
		GetComponent<Renderer>().enabled = true;
	}

	public override Vector3 Resize(Vector3 scale)
	{
		if(isEW == true)
		{
			scale = Vector3.Scale(scale, new Vector3(0.01f, 1.0f, 100.0f));
		}

		return scale;
	}

	public override bool DimensionCollider(SecondDimentionData data)
	{
		bool result = false;

		isEW = !data.isZ;

		if(data.isZ == true)
		{
			result = planeDireciton == PlaneDireciton.NS;
		}

		if(data.isZ == false)
		{
			result = planeDireciton == PlaneDireciton.EW;
		}

		GetComponent<Renderer>().enabled = result;
		return result;
	}

	private void OnValidate()
	{
		RePosition();
	}

	void RePosition()
	{
		if(planeDireciton == PlaneDireciton.NS)
		{
			transform.localRotation = Quaternion.Euler(0, 0, 0);
		}
		else
		{
			transform.localRotation = Quaternion.Euler(0, 270, 0);
		}
	}
}
