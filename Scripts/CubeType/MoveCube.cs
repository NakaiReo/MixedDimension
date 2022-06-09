using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveCube : CubeData
{
	Rigidbody rb;

	Vector3 startPos;
	Vector3 movePos { get { return startPos + checkPoints[index].transform.localPosition; } }

	int index = 0;
	int key = 1;

	Tweener tweener = null;

	[SerializeField] public float moveSpeed; //キューブの移動速度
	[SerializeField] public bool loopMovement; //始点と終点を繋げるか
	[SerializeField] MoveCubeCheckpoint[] checkPoints; //パスのポイント

	private void Start()
	{
		base.Start();

		rb = GetComponent<Rigidbody>();

		startPos = transform.position;
		transform.position = movePos;
		ChangeMovePos();
		LineRender();
	}

	public void Movement()
	{
		float distance = Vector3.Distance(transform.position, movePos);

		tweener = transform.DOMove(movePos, distance / moveSpeed)
			.SetEase(Ease.Linear);

		Invoke(nameof(ChangeMovePos), distance / moveSpeed);
	}

	public void ChangeMovePos()
	{
		int backIndex = index;

		index += key;

		if (index >= checkPoints.Length)
		{
			if (loopMovement)
			{
				index = 0;
			}
			else
			{
				key *= -1;
				index = checkPoints.Length - 1;
			}
		}

		if(index < 0)
		{
			key = 1;
			index = 0;
		}

		Invoke(nameof(Movement), checkPoints[backIndex].waitStopTime);
	}

	public override Color GetCubeColor()
	{
		return Color.blue;
	}

	public override CubeType GetCubeType()
	{
		return CubeType.MoveCube;
	}

	public override bool DimensionCollider(SecondDimentionData data)
	{
		return true;
	}

	private void OnValidate()
	{
		startPos = transform.position;
		checkPoints = new MoveCubeCheckpoint[transform.childCount];

		for(int i = 0; i < transform.childCount; i++)
		{
			checkPoints[i] = transform.GetChild(i).GetComponent<MoveCubeCheckpoint>();
		}
	}

	private void OnCollisionStay(Collision other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			if (GameDirector.ins.playerController.isPless)
			{
				tweener.Kill();
			}
		}
	}

	/// <summary>
	/// 進路を描写する
	/// </summary>
	private void LineRender()
	{
		LineRenderer line = gameObject.AddComponent<LineRenderer>();
		Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);

		Vector3[] pos = new Vector3[checkPoints.Length];

		for (int i = 0; i < checkPoints.Length; i++)
		{
			pos[i] = (Vector3.Scale(checkPoints[i].transform.position, checkPoints[i].transform.localScale) + offset);
		}

		line.positionCount = checkPoints.Length;
		line.material = new Material(Shader.Find("Sprites/Default"));
		line.loop = loopMovement;
		line.SetPositions(pos);
		line.startWidth = 0.1f;
		line.endWidth = 0.1f;
		line.startColor = Color.yellow;
		line.endColor = Color.yellow;
	}

	/// <summary>
	/// 経路とポイントをSceneビューで表示させる
	/// </summary>
	private void OnDrawGizmos()
	{
		Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f) + startPos;

		if (checkPoints.Length < 1) return;

		Gizmos.color = Color.green;

		for(int i = 0; i < checkPoints.Length; i++)
		{
			Gizmos.DrawCube(checkPoints[i].transform.localPosition + offset, Vector3.one / 2.0f);
		}

		if (checkPoints.Length < 2) return;

		Gizmos.color = Color.red;

		for (int i = 0; i < checkPoints.Length - 1; i++)
		{
			Gizmos.DrawLine(checkPoints[i].transform.localPosition + offset, checkPoints[i + 1].transform.localPosition + offset);
		}

		if (loopMovement)
		{
			Gizmos.DrawLine(checkPoints[0].transform.localPosition + offset, checkPoints[checkPoints.Length - 1].transform.localPosition + offset);
		}
	}

	public override Material GetMaterial()
	{
		return Resources.Load("MoveMaterial") as Material;
	}
}
