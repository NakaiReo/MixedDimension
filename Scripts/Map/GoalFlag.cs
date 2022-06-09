using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalFlag : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (GameDirector.ins.isGoal) return;

		if (other.gameObject.CompareTag("Player"))
		{
			CameraController cameraController = other.gameObject.GetComponent<CameraController>();

			if(cameraController.currentCameraType != CameraController.CameraType.SecondDimention)
			{
				GameObject obj = Instantiate(Resources.Load("Goal Effect") as GameObject);
				obj.transform.position = transform.position;
				GameDirector.ins.Goal();
				gameObject.SetActive(false);
			}
		}
	}
}
