using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マップの外側の景観
/// </summary>
public class MapBooder : MonoBehaviour
{
	[SerializeField] float width;	//外壁の幅
	[SerializeField] float height;  //外壁の高さ
	[SerializeField] float posY;    //始める高さ
	
	//外壁の側面の設定
	[Space]
	[SerializeField] Transform booder_bottom;
	[SerializeField] Transform booder_left;
	[SerializeField] Transform booder_right;
	[SerializeField] Transform booder_top;

	//外壁の角の設定
	[Space]
	[SerializeField] Transform corner_bottomLeft;
	[SerializeField] Transform corner_bottomRight;
	[SerializeField] Transform corner_topLeft;
	[SerializeField] Transform corner_topRight;


	/// <summary>
	/// 外壁を更新する
	/// </summary>
	/// <param name="mapSizeV3">サイズ</param>
	public void ExtendBooder(Vector3Int mapSizeV3)
	{
		//外壁のサイズを更新
		Vector2Int mapSize = new Vector2Int(mapSizeV3.x, mapSizeV3.z);
		float half = width * 0.5f;

		booder_bottom.localPosition = new Vector3(0, posY, -half );
		booder_bottom.localScale = new Vector3(mapSize.x, height, width);

		booder_left.localPosition = new Vector3(-half , posY, mapSize.y);
		booder_left.localScale = new Vector3(mapSize.x, height, width);

		booder_right.localPosition = new Vector3(mapSize.x + half , posY, 0);
		booder_right.localScale = new Vector3(mapSize.x, height, width);

		booder_top.localPosition = new Vector3(mapSize.x, posY, mapSize.y + half );
		booder_top.localScale = new Vector3(mapSize.x, height, width);



		corner_bottomLeft.localPosition = new Vector3(-half , posY, 0);
		corner_bottomLeft.localScale = new Vector3(width, height, width);

		corner_bottomRight.localPosition = new Vector3(mapSize.x, posY, -half );
		corner_bottomRight.localScale = new Vector3(width, height, width);

		corner_topLeft.localPosition = new Vector3(0, posY, mapSize.y + half );
		corner_topLeft.localScale = new Vector3(width, height, width);

		corner_topRight.localPosition = new Vector3(mapSize.x + half , posY, mapSize.y);
		corner_topRight.localScale = new Vector3(width, height, width);
	}
}
