using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCube : CubeData
{
	public override Color GetCubeColor()
	{
		return Color.white;
	}

	public override CubeType GetCubeType()
	{
		return CubeType.Goal;
	}
	public override bool DimensionCollider(SecondDimentionData data)
	{
		return false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (GameDirector.ins.isGoal) return;

		if (other.gameObject.CompareTag("Player"))
		{

			CameraController cameraController = other.gameObject.GetComponent<CameraController>();

			//二次元状態ではゴールできないように
			if (cameraController.currentCameraType != CameraController.CameraType.SecondDimention)
			{
				GameObject obj = Instantiate(Resources.Load("Goal Effect") as GameObject);
				obj.transform.position = transform.position;
				GameDirector.ins.Goal();
				other.gameObject.GetComponent<PlayerController>().canMove = false;
				other.gameObject.GetComponent<PlayerController>().DashParticle();
				gameObject.SetActive(false);
			}
		}
	}
}
