using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Boxophobic;

/// <summary>
/// 二次元化した際の情報まとめ
/// </summary>
public class SecondDimentionData
{
	public SideViewCameraSetting.Direction direction; //現在のカメラの方向
	public bool isZ; //z方向を見ているか
	public int sx; //xの大きさ
	public int sy; //yの大きさ
	public int sz; //zの大きさ
	public int keyX; //向いているX方向がプラス方向か
	public int keyZ; //向いているZ方向がプラス方向か
	public bool flipXZ;
}

/// <summary>
/// マップの生成と変換を使うクラス
/// </summary>
public class MapGenerator : MonoBehaviour
{
	public static MapGenerator ins = null;

	[SerializeField] Transform player; //プレイヤーオブジェクト
	[SerializeField] CameraController cameraController; //プレイヤーコントローラ

	KeyBind input;
	MapData mapData;

	public bool isSecondDimension; //二次元化されているかどうか

	SideViewCameraSetting.Direction currentDirection; //現在の向き

	[Space]
	[SerializeField] Transform mapPivot; //マップの基準位置
 	[SerializeField] Transform roomObjects; //部屋のオブジェクト
	[SerializeField] Transform mapObjects;  //マップのオブジェクト

	[Space]
	[SerializeField] GameObject behindPanel; //二次元化したときのパネル
	[SerializeField] Animator backgroundAnimator; //背景のアニメーター

	[Space]
	GameObject stageDataFile;
	[SerializeField] int stageIndex; //現在のステージインデックス
	[SerializeField] Transform stageObjects; //ステージのオブジェクト
 
	[Space]
	public Vector3Int mapSize; //マップのサイズ

	private void Awake()
	{
		if (ins == null)
		{
			ins = this;

			input = new KeyBind();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void Start()
	{
		//二次元化のパネルを切り替える
		behindPanel.SetActive(isSecondDimension);

		//ステージインデックスを設定する
		if(stageIndex >= 0) PlayerPrefs.SetInt("StageIndex", stageIndex);

		//設定されていないければ初期化する
		if (!PlayerPrefs.HasKey("StageIndex")) PlayerPrefs.SetInt("StageIndex", 0);

		//マップクリエイトツールがついていないかどうか
		if (GetComponent<MapCreateTool>() == null)
		{
			//ステージファイルからステージを読み込む
			StageDataFile stageData = Resources.Load("StageDataFile") as StageDataFile;
			stageDataFile = stageData.data[PlayerPrefs.GetInt("StageIndex")].mapData;

			mapObjects = MapLoad(mapObjects, stageDataFile);
			MapResize();

			//プレイヤーの位置を設定する
			player.transform.localPosition = mapData.startCube.localPosition + Vector3.one * 0.5f;
		}
#if UNITY_EDITOR
		else
		{
			//マップクリエイトツールがあるなら現在の状態を移す
			mapObjects = GetComponent<MapCreateTool>().mapObjects;
		}
#endif

	}

	/// <summary>
	/// 二次元化の際の情報抽出
	/// </summary>
	public SecondDimentionData GetSecondDimentionData()
	{
		SecondDimentionData data = new SecondDimentionData();

		data.direction = currentDirection = cameraController.currentDireciton; //現在の向きを渡す
		data.isZ = (data.direction == SideViewCameraSetting.Direction.N || data.direction == SideViewCameraSetting.Direction.S); //コライダーがZ方向に伸びるか
		data.sx = data.isZ ? mapSize.z : mapSize.x; //X方向のサイズ
		data.sy = mapSize.y;						//Y方向のサイズ
		data.sz = data.isZ ? mapSize.x : mapSize.z; //Z方向のサイズ
		data.keyX =   (data.direction == SideViewCameraSetting.Direction.N || data.direction == SideViewCameraSetting.Direction.W) ? 1 : -1; //Xの向き
		data.keyZ =   (data.direction == SideViewCameraSetting.Direction.E || data.direction == SideViewCameraSetting.Direction.N) ? 1 : -1; //Zの向き
		data.flipXZ = (data.direction == SideViewCameraSetting.Direction.E || data.direction == SideViewCameraSetting.Direction.W);			 //XとZを反転させるか

		return data;
	}

	/// <summary>
	/// 次元の変更
	/// </summary>
	public bool ChangeDimention()
	{
		//二次元化の情報を抽出
		SecondDimentionData data = GetSecondDimentionData();

		//次元の切り替え
		isSecondDimension = !isSecondDimension;

		//二次元の状態だったら
		if(isSecondDimension == true)
		{
			////二次元化
			Vector3 size = data.isZ ? new Vector3(1, 1, 100) : new Vector3(100, 1, 1);

			//プレイヤーが埋まらないかどうか
			for (int i = 0; i < mapObjects.childCount; i++)
			{
				GameObject obj = mapObjects.GetChild(i).gameObject;
				CubeData cubeData = obj.GetComponent<CubeData>();

				//プレイヤーがカメラから見て手前にブロックが重なるとき二次元化しない
				if (cubeData.CheckPosColliderFront(player.transform.position, data))
				{
					Debug.Log("埋まるため次元を変えられません。 >> " + obj.name);
					InfoText.ins.SetText("埋まるため次元を変えられません");

					isSecondDimension = false;
					return isSecondDimension;
				}
			}

			//コライダーをすべて伸ばす
			for (int i = 0; i < mapObjects.childCount; i++)
			{
				GameObject obj = mapObjects.GetChild(i).gameObject;
				CubeData cubeData = obj.GetComponent<CubeData>();
				bool isBackGround = cubeData.CheckPosCollider(player.transform.position + new Vector3(0, 0.25f, 0), data.isZ);

				if (isBackGround && cubeData.CheckPosColliderFront(player.transform.position, data)) Debug.Log("埋まってます。 >> " + obj.name);

				cubeData.DimensionColliderSize(data, size, isBackGround);
			}

			DimensionChangeMessage.ins.SetText("2D");
		}
		else
		{
			////三次元化
			RaycastHit[] hit = player.GetComponent<PlayerController>().GetRayObject();

			if (hit != null)
			{
				//一番奥のオブジェクトを取得
				float[] allDepth = new float[hit.Length];

				//奥行きを取得する
				for (int i = 0; i < hit.Length; i++)
				{
					allDepth[i] = GetDepth(hit[i].collider.transform, data);
				}

				//XとZ軸を反転させるか
				bool flip = (data.keyX == 1 && data.keyZ == -1) || (data.keyX == -1 && data.keyZ == -1);
				int index = flip ? Extend.MaxIndex(allDepth) : Extend.MinIndex(allDepth);

				Transform obj = hit[index].collider.transform;

				//プレイヤーの位置を一番手前のオブジェクトに移動
				if (!obj.CompareTag("Ground"))
				{
					Vector3 pos = player.transform.position;

					if (data.isZ == true) pos.z = obj.transform.position.z + 0.5f;
					if (data.isZ == false) pos.x = obj.transform.position.x + 0.5f;

					if (data.isZ == false && data.keyX == 1) pos.x += obj.localScale.x - 1.0f;
					if (data.isZ == true  && data.keyZ == -1) pos.z += obj.localScale.z - 1.0f;

					player.transform.position = pos;
				}
			}

			Vector3 size = Vector3.one;

			//コライダーをすべて通常サイズに戻す
			for (int i = 0; i < mapObjects.childCount; i++)
			{
				GameObject obj = mapObjects.GetChild(i).gameObject;
				CubeData cubeData = obj.GetComponent<CubeData>();
				cubeData.isBackGround = false;
				cubeData.DefaultColliderSize();
			}

			DimensionChangeMessage.ins.SetText("3D");
		}

		SoundDirector.ins.SE.Play("DimensionChange");
		//behindPanel.SetActive(isSecondDimension);
		backgroundAnimator.SetBool("Second Dimensions", isSecondDimension);
		return isSecondDimension;
	}

	//オブジェクトの奥行きを取得
	float GetDepth(Transform tf, SecondDimentionData data)
	{
		float depth;
		if (data.isZ)
		{
			depth = tf.position.z + 0.5f;
			if (data.keyZ == -1) depth += tf.localScale.z;
		}
		else
		{
			depth = tf.position.x + 0.5f;
			if (data.keyX == 1) depth += tf.localScale.x;
		}

		return depth;
	}

	/// <summary>
	/// マップを
	/// </summary>
	void MapResize()
	{
		if (roomObjects == null) return;

		mapPivot.localPosition = Vector3.Scale(mapSize, new Vector3(0.5f, 0.0f, 0.5f)) * -1;
		roomObjects.Find("Floor").localScale = new Vector3(mapSize.x, -10.0f, mapSize.z);
		roomObjects.Find("Wall").localScale = mapSize + Vector3.up * 3;
		GetComponent<MapBooder>().ExtendBooder(mapSize);
	}

	/// <summary>
	/// マップの読み込み
	/// </summary>
	/// <param name="mapObjects"></param>
	/// <param name="mapDataObject"></param>
	/// <returns></returns>
	public Transform MapLoad(Transform mapObjects, GameObject mapDataObject)
	{
		if (mapObjects != null) DestroyImmediate(mapObjects.gameObject);
#if UNITY_EDITOR
		GameObject ins = PrefabUtility.InstantiatePrefab(mapDataObject) as GameObject;
#else
		GameObject ins = Instantiate(mapDataObject) as GameObject;
#endif
		ins.transform.SetParent(mapPivot);
		ins.transform.localPosition = Vector3.zero;

		mapData = ins.GetComponent<MapData>();
		mapSize = mapData.mapSize;

		return ins.transform;
	}

	private void OnEnable() => input.Enable();
	private void OnDisable() => input.Disable();
	private void OnDestroy() => input.Dispose();
}