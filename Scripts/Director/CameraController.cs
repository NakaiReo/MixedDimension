using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

#region CameraClass

/// <summary>
/// カメラごとの設定
/// </summary>
[System.Serializable]
public abstract class CameraSetting
{
	public Camera camera; //カメラ
	public CameraProjectionType projectionType; //カメラの投写方法

	public float fov;
	public float yaw; //横角度
	public float pitch; //縦角度

	public Transform yawPivot; //横回転の中心
	public Transform pitchPivot; //縦回転の中心

	public bool yawCameraFlip; //横回転は反転させるか
	public bool pitchCameraFlip; //縦回転は反転させるか

	public float clipNeer = 0.3f; //カメラの描画範囲の手前

	/// <summary>
	/// 投影方法
	/// </summary>
	public enum CameraProjectionType
	{
		Perspective,
		Orthographic
	}

	/// <summary>
	/// カメラの向き
	/// </summary>
	public enum CameraDirection
	{
		North,
		East,
		South,
		West,
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public abstract void ResetValue();

	/// <summary>
	/// カメラの回転処理
	/// </summary>
	public abstract float CameraRotation(Vector2 inputAxis);

	/// <summary>
	/// ピッチの調整
	/// </summary>
	public abstract void CopyPitch(float value, bool use = true);
}

[System.Serializable]
public class NormalCameraSetting : CameraSetting
{
	public float distance; //カメラの距離(未使用)

	public float sensitive; //感度
	public float yawRotationSpeed; //縦回転の感度
	public float pitchRotationSpeed; //横回転の感度

	public float offsetPitch; //切り替え時の追加ピッチ

	public Extend.RangeFloat distanceRange; //距離の限界範囲
	public Extend.RangeFloat yawRange; //縦軸の回転限界範囲

	/// <summary>
	/// 初期化
	/// </summary>
	override public void ResetValue()
	{
		if (pitchPivot != null)
		{
			pitchPivot.localRotation = Quaternion.Euler(0, pitch, 0);
		}
		else
		{
			pitch = 0;
		}

		if (yawPivot != null)
		{
			yawPivot.localRotation = Quaternion.Euler(yaw, 0, 0);
		}
		else
		{
			yaw = 0;
		}
	}

	/// <summary>
	/// カメラの回転
	/// </summary>
	/// <param name="inputAxis">入力</param>
	/// <returns></returns>
	override public float CameraRotation(Vector2 inputAxis)
	{
		Vector2 rotationValue = inputAxis;
		rotationValue.x *= pitchRotationSpeed * sensitive * (pitchCameraFlip ? -1 : 1);
		rotationValue.y *= yawRotationSpeed * sensitive * (yawCameraFlip ? -1 : 1);

		if (pitchPivot != null)
		{
			pitch = Extend.AngleNormalization(pitch + rotationValue.x);
			pitchPivot.localRotation = Quaternion.Euler(0, pitch, 0);
		}
		else
		{
			pitch = 0;
		}

		if (yawPivot != null)
		{
			yaw = Mathf.Clamp(yaw + rotationValue.y, yawRange.Min, yawRange.Max);
			yawPivot.localRotation = Quaternion.Euler(yaw, 0, 0);
		}
		else
		{
			yaw = 0;
		}

		return 0;
	}

	/// <summary>
	/// ピッチの微調整
	/// </summary>
	/// <param name="value">値</param>
	public override void CopyPitch(float value, bool use = true)
	{
		pitch = value + offsetPitch;
	}
}

/// <summary>
/// サイドビューのカメラの設定
/// </summary>
[System.Serializable]
public class SideViewCameraSetting : CameraSetting
{
	public float rotationSpeed; //回転速度
	public float sensitive;		//感度
	public float offsetPitch;	//横回転の補正値

	/// <summary>
	/// カメラの方向
	/// </summary>
	public enum Direction
	{
		N,E,S,W,
	}

	/// <summary>
	/// カメラの方向を取得
	/// </summary>
	/// <returns></returns>
	public Direction GetDirection()
	{
		float angle = Extend.AngleNormalization(pitch);

		//315度以上か45度未満の時 南
		if(360 - 45 <= angle || 0 + 45 > angle)
		{
			return Direction.S;
		}
		//45度以上かつ135度未満の時 西
		if(90 - 45 <= angle && 90 + 45 > angle)
		{
			return Direction.W;
		}
		//135度以上かつ225度未満のとき 北
		if(180 - 45 <= angle && 180 + 45 > angle)
		{
			return Direction.N;
		}
		//225度以上かつ315℃未満のとき 東
		if(270 - 45 <= angle && 270 + 45 > angle)
		{
			return Direction.E;
		}

		return Direction.W;
	}

	/// <summary>
	/// 方角をベクトルで取得
	/// </summary>
	public Vector3Int DirectionVector()
	{
		return DirectionVector(GetDirection());
	}

	/// <summary>
	/// 方角をベクトルで取得
	/// </summary>
	public Vector3Int DirectionVector(Direction direction)
	{
		switch (direction)
		{
			case Direction.N:
				return new Vector3Int(0, 0, 1);
			case Direction.E:
				return new Vector3Int(1, 0, 0);
			case Direction.S:
				return new Vector3Int(0, 0, -1);
			case Direction.W:
				return new Vector3Int(-1, 0, 0);
		}

		return new Vector3Int(0, 0, 0);
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public override void ResetValue()
	{
		Pitch(pitch);
		if (pitchPivot != null)
		{
			pitchPivot.transform.localRotation = Quaternion.Euler(0, pitch, 0);
		}
	}

	/// <summary>
	/// カメラの回転
	/// </summary>
	/// <param name="inputAxis">入力</param>
	/// <returns></returns>
	override public float CameraRotation(Vector2 inputAxis)
	{
		//X軸の入力だけを取る
		float rotationValue = inputAxis.x;
		//一定の値未満は回転しない
		if (Mathf.Abs(rotationValue * sensitive) < 2.5f) rotationValue = 0.0f; 
		//回転方向に直す
		int key = rotationValue != 0 ? (int)Mathf.Sign(rotationValue * (pitchCameraFlip ? -1 : 1)) : 0;

		if (pitchPivot != null && key != 0)
		{
			SoundDirector.ins.SE.Play("UIMove");

			//回転方向に90度回転させる
			pitch += 90 * key;
			Pitch(pitch);
			pitchPivot.transform.DOLocalRotate(new Vector3(0, pitch, 0), rotationSpeed);

			return rotationSpeed;
		}

		return 0;
	}

	void Pitch(float value)
	{
		pitch = Extend.Clamp90MultipleAngle(value);
	}

	public override void CopyPitch(float value, bool use = true)
	{
		pitch = Extend.Clamp90MultipleAngle(value + offsetPitch);
	}
}

#endregion

/// <summary>
/// カメラの回転を処理するクラス
/// </summary>
public class CameraController : MonoBehaviour
{
	KeyBind input;
	PlayerController playerController;
	ProjectionTween projectionTween;

	bool takeMessage = false;

	/// <summary>
	/// カメラタイプ
	/// </summary>
	public enum CameraType
	{
		Player,			 //プレイヤーTPSビュー
		TopDown,		 //トップダウンビュー
		SideView,		 //サイドビュー
		SecondDimention, //二次元化ビュー
		PlayerView,		 //プレイヤー正面ビュー
	}

	[SerializeField] public Camera mainCamera; //メインカメラ
	[SerializeField] public CameraType currentCameraType; //現在のカメラタイプ

	[Space]
	[Space] public NormalCameraSetting topDownCamera;			//トップダウンビューの設定
	[Space] public SideViewCameraSetting sideViewCamera;		//サイドビューの設定
	[Space] public NormalCameraSetting playerCamera;			//プレイヤーTPSの設定
	[Space] public SideViewCameraSetting secondDimentionCamera; //二次元化ビューの設定
	[Space] public NormalCameraSetting playerViewCamera;		//プレイヤー正面ビューの設定

	[Space]
	[SerializeField] float cameraChangeTime; float currentCameraChangeTime; //カメラの切り替え速度
	[SerializeField] float cameraChangeCooldown; float CurrentCameraChangeCooldown;

	/// <summary>
	/// 操作可能か
	/// </summary>
	public bool canControl
	{
		get
		{
			return (currentCameraChangeTime <= 0);
		}
	}

	/// <summary>
	/// 現在のカメラを取得
	/// </summary>
	public Camera currentCamera
	{
		get
		{
			switch (currentCameraType)
			{
				case CameraType.TopDown:
					return topDownCamera.camera;
				case CameraType.SideView:
					return sideViewCamera.camera;
				case CameraType.Player:
					return playerCamera.camera;
				case CameraType.SecondDimention:
					return secondDimentionCamera.camera;
				case CameraType.PlayerView:
					return playerViewCamera.camera;
				default:
					return null;
			}
		}
	}

	/// <summary>
	/// 現在のカメラの設定を取得
	/// </summary>
	public CameraSetting currentCameraSetting
	{
		get
		{
			switch (currentCameraType)
			{
				case CameraType.TopDown:
					return topDownCamera;
				case CameraType.SideView:
					return sideViewCamera;
				case CameraType.Player:
					return playerCamera;
				case CameraType.SecondDimention:
					return secondDimentionCamera;
				case CameraType.PlayerView:
					return playerViewCamera;
				default:
					return null;
			}
		}
	}

	/// <summary>
	/// 現在の向き
	/// </summary>
	public SideViewCameraSetting.Direction currentDireciton
	{
		get
		{
			return sideViewCamera.GetDirection();
		}
	}

	/// <summary>
	/// 現在の向きのベクトル
	/// </summary>
	public Vector3Int currentDirecitonVector
	{
		get
		{
			return sideViewCamera.DirectionVector();
		}
	}

	private void Awake()
	{
		input = new KeyBind();
	}

	void Start()
	{
		playerController = GetComponent<PlayerController>();
		projectionTween = mainCamera.GetComponent<ProjectionTween>();

		//マップサイズに応じてトップダウンカメラのピボットを変える
		Vector3 mapSize = MapGenerator.ins.mapSize;
		topDownCamera.pitchPivot.localPosition = Vector3.up * Mathf.Max(mapSize.x, mapSize.y, mapSize.z) * 1.5f;
		topDownCamera.camera.transform.localPosition = mapSize.ScaleAxis(Extend.Axis.z) * -1.5f;

		//カメラの初期化
		topDownCamera.ResetValue();
		sideViewCamera.ResetValue();
		playerCamera.ResetValue();
		secondDimentionCamera.ResetValue();

		//キー情報に現在のカメラを渡す
		KeyInfoGUI.ins.SetActive(true);
		KeyInfoGUI.ins.SetGUI(currentCameraType);
		CameraViewChange(currentCameraType);

		currentCameraChangeTime = 0.0f;
		takeMessage = true;
	}

	private void Update()
	{
		if (Time.timeScale <= 0) return;
		if (GameDirector.ins.isGoal == true) return;

		//カメラの移動が終わるまで操作を受け付けない
		if (currentCameraChangeTime > 0)
		{
			currentCameraChangeTime = Mathf.Max(currentCameraChangeTime - Time.deltaTime, 0);

			mainCamera.transform.position = currentCamera.transform.position;
			mainCamera.transform.rotation = currentCamera.transform.rotation;

			return;
		}

		//カメラの切り替えクールダウン
		if (CurrentCameraChangeCooldown > 0)
		{
			CurrentCameraChangeCooldown -= Time.deltaTime;
		}

		//カメラの切り替え
		#region ViewTypeCahnge

		if (input.ViewType.OrderChange.triggered)
		{
			//カメラの順次切り替え処理
			switch (currentCameraType)
			{
				case CameraType.TopDown:
					CameraViewChange(CameraType.SideView);
					break;
				case CameraType.Player:
					CameraViewChange(CameraType.TopDown);
					break;
				case CameraType.SideView:
					CameraViewChange(CameraType.Player);
					break;
				case CameraType.SecondDimention:
					CameraViewChange(CameraType.SideView);
					break;
			}
		}

		//トップビュー切り替えが入力されたら
		if (input.ViewType.TopViewChange.triggered)
		{
			CameraViewChange(CameraType.TopDown);
			return;
		}
		//プレイヤービュー切り替えが入力されたら
		if (input.ViewType.PlayerViewChange.triggered)
		{
			CameraViewChange(CameraType.Player);
			return;
		}
		//サイドビュー切り替えが入力されたら
		if (input.ViewType.SideViewChange.triggered)
		{
			CameraViewChange(CameraType.SideView);
			return;
		}
		//次元切り替えが入力されたら
		if (input.Camera.DimensionChange.triggered)
		{
			if (currentCameraType == CameraType.SideView || currentCameraType == CameraType.SecondDimention)
			{
				if (CurrentCameraChangeCooldown > 0)
				{
					return;
				}

				if (!playerController.onGround)
				{
					isJampCameraChangeErrorMessage();
					return;
				}

				//次元を変更する
				bool isSecondDimension = MapGenerator.ins.ChangeDimention();

				//次元に合ったカメラに切り替える
				if (isSecondDimension)
				{
					CameraViewChange(CameraType.SecondDimention, true);
					cameraChangeTime = 0.25f;
					return;
				}
				else
				{
					CameraViewChange(CameraType.SideView, true);
					cameraChangeTime = 0.25f;
					return;
				}
			}
			else
			{
				canNotChangeDimensionErrorMessage();
				return;
			}
		}

		#endregion


		Vector2 inputAxis = input.Camera.Control.ReadValue<Vector2>();
		Vector2 scroolAxis = new Vector2(input.Camera.CameraScroll.ReadValue<float>(), 0);

		//topDownCamera.camera.gameObject.SetActive(currentCameraType == CameraType.TopDown);
		//playerCamera.camera.gameObject.SetActive(currentCameraType == CameraType.Player);

		playerCamera.camera.gameObject.SetActive(false);
		topDownCamera.camera.gameObject.SetActive(false);
		sideViewCamera.camera.gameObject.SetActive(false);
		secondDimentionCamera.camera.gameObject.SetActive(false);

		if (CurrentCameraChangeCooldown <= 0)
		{
			//カメラに合った回転量を与える
			switch (currentCameraType)
			{
				case CameraType.TopDown:
					currentCameraChangeTime = topDownCamera.CameraRotation(inputAxis);
					break;
				case CameraType.SideView:
					if (input.Camera.HoldCamera.ReadValue<float>() > 0) scroolAxis = Vector2.zero;
					currentCameraChangeTime = sideViewCamera.CameraRotation(scroolAxis);
					break;
				case CameraType.Player:
					currentCameraChangeTime = playerCamera.CameraRotation(inputAxis);
					break;
				case CameraType.SecondDimention:
					break;
			}

			//メインカメラを指定のカメラワークに動かす
			mainCamera.transform.position = currentCamera.transform.position;
			mainCamera.transform.rotation = currentCamera.transform.rotation;
		}
	}

	/// <summary>
	/// ジャンプエラーメッゼージ
	/// </summary>
	void isJampCameraChangeErrorMessage()
	{
		//Debug.Log("ジャンプ中にカメラの視点変更はできません。");
		InfoText.ins.SetText("ジャンプ中にカメラの視点変更はできません");
	}

	/// <summary>
	/// カメラエラーメッセージ
	/// </summary>
	void canNotChangeDimensionErrorMessage()
	{
		//Debug.Log("このカメラからは次元を変えられません。");
		InfoText.ins.SetText("このカメラからは次元を変えられません");
	}

	/// <summary>
	/// カメラの切り替え処理
	/// </summary>
	/// <param name="type">どのカメラタイプか</param>
	/// <param name="noBackThirdDimenstion">ディメンションを三次元に戻さないようにするか</param>
	public void CameraViewChange(CameraType type, bool noBackThirdDimenstion = false)
	{
		float animTime = 0.75f;

		//カメラの切り替えのクールダウンが終わってなければ実行しない
		if(CurrentCameraChangeCooldown > 0)
		{
			return;
		}

		//同じカメラタイプだったら実行しない
		if(currentCameraType == type)
		{
			return;
		}

		//空中にいたら警告を出して終了する
		if (!playerController.onGround && takeMessage)
		{
			isJampCameraChangeErrorMessage();
			return;
		}

		//カメラが二次元化していたら三次元に戻す
		bool isDimensionChange = currentCameraType == CameraType.SecondDimention && noBackThirdDimenstion == false;
		if (isDimensionChange)
		{
			MapGenerator.ins.ChangeDimention();
		}

		SoundDirector.ins.SE.Play("UISelect");

		//キー情報の更新
		KeyInfoGUI.ins.SetGUI(type);

		//カメラの情報と設定をメインカメラに渡す
		CameraSetting backCamera = currentCameraSetting;
		float backPitch = backCamera.pitch;

		CameraType backType = currentCameraType;
		currentCameraType = type;

		currentCameraChangeTime = cameraChangeTime;
		currentCameraSetting.CopyPitch(backPitch, backType != CameraType.SecondDimention);
		currentCameraSetting.ResetValue();

		//mainCamera.orthographic = currentCameraSetting.projectionType == CameraSetting.CameraProjectionType.Orthographic;
		//mainCamera.orthographicSize = currentCamera.orthographicSize;
		//mainCamera.transform.position = currentCamera.transform.position;
		//mainCamera.transform.rotation = currentCamera.transform.rotation;

		float aspect = (float)Screen.width / (float)Screen.height;
		Matrix4x4 ortho = Matrix4x4.Ortho(-currentCameraSetting.fov * aspect, currentCameraSetting.fov * aspect, -currentCameraSetting.fov, currentCameraSetting.fov, currentCameraSetting.clipNeer, 1000);
		Matrix4x4 perspective = Matrix4x4.Perspective(currentCameraSetting.fov, aspect, currentCameraSetting.clipNeer, 1000);

		//カメラのFOVを変更する
		//投影方法が違う場合アニメーションを行う
		if (currentCameraSetting.projectionType != backCamera.projectionType)
		{
			Debug.Log("AAAA");
			mainCamera.transform.position =  currentCamera.transform.position;
			mainCamera.transform.rotation =  currentCamera.transform.rotation;

			if (currentCameraSetting.projectionType == CameraSetting.CameraProjectionType.Perspective)
			{
				projectionTween.BlendToMatrix(perspective, animTime);
			}
			else
			{
				projectionTween.BlendToMatrix(ortho, animTime);
			}
		}
		else
		{
			mainCamera.transform.position = currentCamera.transform.position;
			mainCamera.transform.rotation = currentCamera.transform.rotation;
			//mainCamera.transform.DOMove(currentCamera.transform.position, animTime);
			//mainCamera.transform.DOLocalRotateQuaternion(currentCamera.transform.localRotation, animTime);
			mainCamera.fieldOfView = currentCameraSetting.fov;
			projectionTween.BlendToMatrix(perspective, 0);
		}

		CurrentCameraChangeCooldown = cameraChangeCooldown;

		return;
	}

	private void OnEnable() => input.Enable();
	private void OnDisable() => input.Disable();
	private void OnDestroy() => input.Dispose();

	private void OnValidate()
	{
		topDownCamera.ResetValue();
		playerCamera.ResetValue();
	}
}