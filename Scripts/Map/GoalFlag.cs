using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゴール判定
/// </summary>
public class GoalFlag : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		//ゴールした状態では振れられないように
		if (GameDirector.ins.isGoal) return;

		if (other.gameObject.CompareTag("Player"))
		{
			CameraController cameraController = other.gameObject.GetComponent<CameraController>();

			//二次元状態では触れないように
			if(cameraController.currentCameraType != CameraController.CameraType.SecondDimention)
			{
				//ゴール処理
				GameObject obj = Instantiate(Resources.Load("Goal Effect") as GameObject);
				obj.transform.position = transform.position;
				GameDirector.ins.Goal();
				gameObject.SetActive(false);
			}
		}
	}
}
