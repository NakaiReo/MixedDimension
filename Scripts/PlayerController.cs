using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
	KeyBind input;

	Rigidbody rb;
	Animator aniamtor;
	CapsuleCollider playerCollider;
	CameraController cameraController; //カメラのコントローラー

	Vector3 toMovementVector; //現在進んでいる方向

	int layerMask = 1 << 8; //BlockのLayer

	bool isDash { get { return input.Player.Dash.ReadValue<float>() > 0 || autoDash; } } // ダッシュキー
	bool canControl { get { return cameraController.canControl; } } // 動けるかどうか
	bool autoDash { get{ return SettingMenu.ins.data.system.isAutoDash; } } //オートダッシュ

	[HideInInspector] public bool canMove = true; //移動可能か

	/// <summary>
	/// 地面に足がついているかどうか
	/// </summary>
	public bool onGround
	{
		get
		{
			RaycastHit hit;
			Vector3 rayPosition = transform.position + Vector3.up * 0.1f;
			float rayDistace = playerCollider.height * 0.2f;
			Ray ray = new Ray(rayPosition, Vector3.down * rayDistace);

			//Debug.DrawRay(rayPosition, Vector3.down * rayDistace, Color.red);

			if (Physics.Raycast(ray, out hit, rayDistace, layerMask))
			{
				return true;
			}

			return false;
		}
	}
	
	/// <summary>
	/// プレイヤーが潰されているかどうか
	/// </summary>
	public bool isPless
	{
		get
		{
			RaycastHit hit;
			Vector3 rayPosition = transform.position + Vector3.up * 0.7f;
			float rayDistace = playerCollider.height * 0.3f;
			Ray ray = new Ray(rayPosition, Vector3.up * rayDistace);

			Debug.DrawRay(rayPosition, Vector3.up * rayDistace, Color.red);

			if (Physics.Raycast(ray, out hit, rayDistace, layerMask))
			{
				if (!onGround) return false;

				return true;
			}

			return false;
		}
	}

	/// <summary>
	/// 地面のオブジェクトを取得
	/// </summary>
	/// <returns>地面のオブジェクト</returns>
	GameObject GetGroundObject()
	{
		RaycastHit hit;
		Vector3 rayPosition = transform.position + Vector3.up * 0.1f;
		float rayDistace = playerCollider.height * 0.2f;
		Ray ray = new Ray(rayPosition, Vector3.down * rayDistace);

		Debug.DrawRay(rayPosition, Vector3.down * rayDistace, Color.red);

		if (Physics.Raycast(ray, out hit, rayDistace, layerMask))
		{
			return hit.collider.gameObject;
		}

		return null;
	}

	/// <summary>
	/// 正面のオブジェクト取得
	/// </summary>
	/// <returns></returns>
	public RaycastHit[] GetRayObject()
	{
		float offset = 0.25f;

		Vector3 rayPosition = transform.position + Vector3.up * offset;
		float rayDistace = offset + 0.5f;
		Ray ray = new Ray(rayPosition, Vector3.down * rayDistace);

		return Physics.RaycastAll(ray, rayDistace, layerMask);
	}

	[SerializeField] Transform mapPivot;

	[Space]
	[SerializeField] float walkForce; //移動の強さ
	[SerializeField] float dashForce; //ダッシュの強さ
	[SerializeField] float maxMoveSpeed; //最大速度
	[SerializeField] float jumpForce; //ジャンプの強さ
	[SerializeField] float checkPointTime; //チェックポイントの時間

	[Space]
	[SerializeField] KeyInfoGUI gui;
	[SerializeField] ParticleSystem dashParticle;
	[SerializeField] ParticleSystem jumpParticle;
	[SerializeField] GameObject teleportParticle;

	float currentCheckPointTime;

	private void Awake()
	{
		input = new KeyBind();
	}

	void Start()
    {
		//コンポーネント取得
		rb = GetComponent<Rigidbody>();
		playerCollider = GetComponent<CapsuleCollider>();
		aniamtor = GetComponent<Animator>();
		cameraController = GetComponent<CameraController>();
    }

    void Update()
    {
		if (Time.timeScale <= 0) return;

		CheckPoint();
		OnMoveCube();
		Movement();
		Jump();
    }

	GameObject backCube;    //前の地点のブロック
	GameObject currentCube; //現在の地点のブロック
	Vector3 backPosition = new Vector3(-99, -99, -99);   //前の座標
	Vector3 currentPostion = new Vector3(-99, -99, -99); //現在の座標

	bool startPlace;
	bool startTeleport;
	float placeKeyTime;
	float teleportKeyTime; //テレポートのチャージ

	/// <summary>
    /// 前の地点に戻る
    /// </summary>
	void CheckPoint()
	{
		//現在の真下のブロックを取得
		GameObject groundObject = GetGroundObject();

		//真下にブロックがあり、カメラが2次元状態でないとき
		if (groundObject != null && cameraController.currentCameraType != CameraController.CameraType.SecondDimention)
		{
			//前の地点が登録されていないとき、現在のブロックを登録
			if (backCube == null)
			{
				currentCube = groundObject;
				currentPostion = transform.position;
				backCube = groundObject;
				backPosition = transform.position;
			}

			//足場が登録されている足場と変わったら更新する
			if (currentCube != groundObject)
			{
				backCube = currentCube;
				backPosition = currentPostion;
				currentCube = groundObject;
				currentPostion = transform.position;
			}
		}

		//テレポートボタンが押されたら
		if (input.Player.TeleportCheckPoint.triggered)
		{
			//前の地点が登録されているか
			if (backPosition != new Vector3(-99,-99,-99))
			{
				//カメラが2次元状態ではないか
				if (cameraController.currentCameraType != CameraController.CameraType.SecondDimention)
				{
					//テレポート開始
					startTeleport = true;
				}
				else
				{
					//エラーメッセージの表示
					InfoText.ins.SetText("2次元では使用できません!");
				}
			}

			//エラーメッセージの表示
			else { InfoText.ins.SetText("ポータルが設置されていません!"); }
		}

		//長押ししている間、チャージをためる
		teleportKeyTime = input.Player.TeleportCheckPoint.ReadValue<float>() > 0 ? teleportKeyTime + Time.deltaTime : 0;

		//テレポートを開始しており、スタート地点ではないとき
		if (startTeleport && !startPlace)
		{
			//チャージが0ならテレポートを終了する
			if (teleportKeyTime <= 0) startTeleport = false;

			//UIの表示
			gui.portalPanel.SetActive(true);
			gui.portalText.text = "前の足場へ移動中...";
			gui.portalSlider.value = teleportKeyTime / checkPointTime;

			//チャージがたまったら前の地点へワープ
			if (teleportKeyTime >= checkPointTime)
			{
				GameObject ins = Instantiate(teleportParticle);
				ins.transform.position = transform.position;

				startTeleport = false;
				transform.position = backPosition;

				GameObject ins2 = Instantiate(teleportParticle);
				ins2.transform.position = transform.position;

				currentCube = backCube;
				currentPostion = backPosition;
			}
		}

		//ワープが終わったらUI非表示に
		if(!startTeleport && !startPlace)
		{
			gui.portalPanel.SetActive(false);
		}
	}

	//移動の処理
	void Movement()
	{
		//操作可能か
		if (!canControl || !canMove)
		{
			aniamtor.SetFloat("MoveSpeed", 0);
			rb.velocity = new Vector3(0, rb.velocity.y, 0);
			return;
		}

		//カメラの状態が二次元かどうか
		if (cameraController.currentCameraType == CameraController.CameraType.SideView || cameraController.currentCameraType == CameraController.CameraType.SecondDimention)
		{
			//二次元の移動
			SecondDimentionMovement();
		}
		else
		{
			//三次元の移動
			ThirdDimentionMovement();
		}
	}

	/// <summary>
	/// 2次元空間上の移動処理
	/// </summary>
	void SecondDimentionMovement()
	{
		//現在のカメラ
		Camera camera = cameraController.currentCamera;

		Vector2 inputAxis = input.Player.Move.ReadValue<Vector2>();
		Vector3 inputMoveVector = new Vector3(inputAxis.x, 0, 0);

		//移動処理
		float force = isDash && onGround ? dashForce : walkForce;
		Vector3 cameraForward = Vector3.Scale(camera.transform.forward, new Vector3(1, 0, 1)).normalized;
		Vector3 moveForward = cameraForward * inputMoveVector.z + camera.transform.right * inputMoveVector.x;
		rb.velocity = moveForward * force + new Vector3(0, rb.velocity.y, 0);
		DashParticle();

		toMovementVector = moveForward.normalized;

		//回転とアニメーション
		float currentSpeedPercent;
		if (inputMoveVector != Vector3.zero)
		{
			//体の向きを移動方向に合わせる
			float angle = Mathf.Atan2(moveForward.z, moveForward.x) * Mathf.Rad2Deg;
			angle = Extend.Clamp90MultipleAngle(angle + 90);

			if (angle == 0 || angle == 180) angle = Extend.Clamp90MultipleAngle(angle + 180);

			transform.localRotation = Quaternion.Euler(0, angle, 0);

			aniamtor.SetFloat("Move_DirectionX", 0);
			aniamtor.SetFloat("Move_DirectionZ", 1);
			currentSpeedPercent = isDash ? 1.0f : 0.5f;
		}
		else
		{
			currentSpeedPercent = 0.0f;
		}

		aniamtor.SetFloat("MoveSpeed", currentSpeedPercent);
	}

	/// <summary>
	/// 3次元空間上の移動処理
	/// </summary>
	void ThirdDimentionMovement()
	{
		//現在のカメラ
		Camera camera = cameraController.currentCamera;

		Vector2 inputAxis = input.Player.Move.ReadValue<Vector2>();
		Vector3 inputMoveVector = new Vector3(inputAxis.x, 0, inputAxis.y);

		//移動処理
		float force = isDash && onGround ? dashForce : walkForce;
		Vector3 cameraForward = Vector3.Scale(camera.transform.forward, new Vector3(1, 0, 1)).normalized;
		Vector3 moveForward = cameraForward * inputMoveVector.z + camera.transform.right * inputMoveVector.x;
		rb.velocity = moveForward * force + new Vector3(0, rb.velocity.y, 0);
		DashParticle();


		toMovementVector = cameraForward.normalized;

		//回転とアニメーション
		float currentSpeedPercent;
		if (inputMoveVector != Vector3.zero)
		{
			//進む方向に合わせてアニメーターに値を渡す
			aniamtor.SetFloat("Move_DirectionX", inputAxis.x);
			aniamtor.SetFloat("Move_DirectionZ", inputAxis.y);
			currentSpeedPercent = isDash ? 1.0f : 0.5f;
		}
		else
		{
			currentSpeedPercent = 0.0f;
		}

		aniamtor.SetFloat("MoveSpeed", currentSpeedPercent);
	}

	/// <summary>
	/// ジャンプの処理
	/// </summary>
	void Jump()
	{
		if (!canControl || !canMove) return;

		if (input.Player.Jump.triggered)
		{
			//地面についてなければ実行しない
			if (!onGround) return;

			SoundDirector.ins.SE.Play("Jump");
			aniamtor.SetTrigger("Jump");
			rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

			jumpParticle.Play();
		}
	}

	/// <summary>
	/// ダッシュ時の砂埃
	/// </summary>
	public void DashParticle()
	{
		if (isDash == true && rb.velocity != Vector3.zero && Mathf.Abs(rb.velocity.y) < 0.5f)
		{
			if (!dashParticle.isPlaying) dashParticle.Play();
		}
		else
		{ 
			if (dashParticle.isPlaying) dashParticle.Stop();
		}
	}

	/// <summary>
	/// 移動ブロックに乗っているときの処理
	/// </summary>
	void OnMoveCube()
	{
		RaycastHit hit;
		Vector3 rayPosition = transform.position + Vector3.up * 0.1f;
		float rayDistace = playerCollider.height * 0.2f;
		Ray ray = new Ray(rayPosition, Vector3.down * rayDistace);

		Debug.DrawRay(rayPosition, Vector3.down * rayDistace, Color.red);

		if (Physics.Raycast(ray, out hit, rayDistace, layerMask))
		{
			if (hit.collider.CompareTag("MoveCube"))
			{
				transform.SetParent(hit.collider.transform);
				return;
			}
		}

		transform.SetParent(mapPivot);
	}

	private void OnEnable() => input.Enable();
	private void OnDisable() => input.Disable();
	private void OnDestroy() => input.Dispose();
}