using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class CubeData : MonoBehaviour
{
	//背景ブロック化した時の色
	static Color BACK_GROUND_COLOR = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	Renderer renderer;
	Material normalMaterial;
	static Material alphaMaterial;
	static PhysicMaterial noFriction;
	BoxCollider boxCollider;
	EPOOutline.Outlinable outlinable;

	virtual public Vector3 Resize(Vector3 scale) { return scale; } //サイズを変更する
	virtual public void DefaultCollider() { } //元のコライダーに戻す
	abstract public bool DimensionCollider(SecondDimentionData data); //次元ごとのコライダー変形処理
	abstract public CubeType GetCubeType(); //キューブの種類を取得
	abstract public Color GetCubeColor(); //キューブの色を取得
	virtual public Material GetMaterial() //キューブのマテリアルを取得
	{
		return renderer != null ? renderer.material : null;
	}

	/// <summary>
	/// キューブの種類
	/// </summary>
	public enum CubeType
	{
		Error = -99,
		Start = -1,
		Goal = -2,
		None = 0,
		Normal = 1,
		Plane = 2,
		MoveCube = 3,
	}

	public bool isBackGround
	{
		get { return _isBackGround; }
		set
		{
			_isBackGround = value;

			SetColor();
		}
	}
	bool _isBackGround;



	public void Start()
	{
		renderer = GetComponent<Renderer>();

		if (noFriction == null)
		{
			noFriction = Resources.Load("NoPhysic") as PhysicMaterial;
		}

		normalMaterial = GetMaterial();

		if (alphaMaterial == null)
		{
			alphaMaterial = Resources.Load("AlphaMaterial") as Material;
		}

		outlinable = GetComponent<EPOOutline.Outlinable>();

		AddBoxCollider();
		SetColor();
	}

	/// <summary>
	/// コライダーやマテリアルをもとに戻す
	/// </summary>
	public void DefaultColliderSize()
	{
		//コライダーを戻す
		DefaultCollider();

		//マテリアルと色を戻す
		if (renderer != null) renderer.material = normalMaterial;
		SetColor();

		if (boxCollider == null) return;

		//アウトラインをつける
		if (outlinable != null) outlinable.enabled = true;
		boxCollider.enabled = false;
	}

	public void DimensionColliderSize(SecondDimentionData data, Vector3 scale, bool isBackGround)
	{
		//背景化されているか
		this.isBackGround = isBackGround;

		//状態に応じたマテリアルと色に設定
		if(renderer != null) renderer.material = isBackGround ? alphaMaterial : normalMaterial;
		SetColor();

		//背景化されているならアウトラインとコライダーを無効化する
		if (isBackGround)
		{
			outlinable.enabled = false;
			boxCollider.enabled = false;
			return;
		}

		if (boxCollider == null) return;

		//コライダーを次元にあわせて変更する
		if(outlinable != null) outlinable.enabled = DimensionCollider(data);
		boxCollider.enabled = DimensionCollider(data);
		boxCollider.size = Resize(scale);

	}

	/// <summary>
	/// コライダーを追加する
	/// </summary>
	private void AddBoxCollider()
	{
		//コライダーがついていないかどうか
		if (boxCollider == null)
		{
			boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.material = noFriction;
			boxCollider.center = new Vector3(0.5f, 0.5f, 0.5f);
			boxCollider.size = Vector3.one;
			boxCollider.enabled = !isBackGround;

			DefaultColliderSize();
		}
	}
	
	/// <summary>
	/// 次元変更でこのブロックにプレイヤーが埋まるかどうか
	/// </summary>
	/// <param name="playerPos">プレイヤーの座標</param>
	/// <param name="isZ">Z軸方向に伸びているか</param>
	/// <returns></returns>
	public bool CheckPosCollider(Vector3 playerPos, bool isZ)
	{
		//キューブタイプが存在しているか
		if ((int)GetCubeType() <= 0) return false;

		//埋まっているかどうか判断する
		Vector3 pos = transform.position + new Vector3(0, -0.05f, 0);
		Vector3 size = transform.localScale;

		if ((playerPos.x < pos.x || playerPos.x > pos.x + size.x) && isZ != false) return false;
		if ((playerPos.y < pos.y || playerPos.y > pos.y + size.y)) return false;
		if ((playerPos.z < pos.z || playerPos.z > pos.z + size.z) && isZ != true) return false;

		return true;
	}

	/// <summary>
	/// コライダーがカメラから見てプレイヤーより手前に重なるかどうか
	/// </summary>
	/// <param name="playerPos">プレイヤーの位置</param>
	/// <param name="data"></param>
	/// <returns></returns>
	public bool CheckPosColliderFront(Vector3 playerPos, SecondDimentionData data)
	{
		//キューブタイプが存在しているか
		if ((int)GetCubeType() <= 0) return false;

		Vector3 pos = transform.position + new Vector3(0, -0.05f, 0);
		Vector3 size = transform.localScale;

		if (CheckPosCollider(playerPos, data.isZ) == false) return false;
		if ((playerPos.y < pos.y || playerPos.y > pos.y + size.y)) return false;

		//二次元化するときの軸はどちらか
		if (data.isZ)
		{
			if (data.keyZ == 1)
			{
				if (playerPos.z > pos.z + size.z) return true;
			}
			else
			{
				if (playerPos.z < pos.z) return true;
			}
		}
		else
		{
			if (data.keyX != 1)
			{
				if (playerPos.x > pos.x + size.x) return true;
			}
			else
			{
				if (playerPos.x < pos.x) return true;
			}
		}

		return false;
	}

	/// <summary>
	/// マテリアルの色を変える
	/// </summary>
	void SetColor()
	{
		if (renderer == null) return;
		renderer.material.color = isBackGround ? BACK_GROUND_COLOR : GetCubeColor();
	}
}
