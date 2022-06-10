using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Extend
{
	/// <summary>
    ///	範囲を扱う構造体
    /// </summary>
	[System.Serializable]
	public struct RangeFloat
	{
		[SerializeField] private float min;
		[SerializeField] private float max;

		public RangeFloat(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		public float Min { get { return min; } set { min = Mathf.Min(value, max); } }
		public float Max { get { return max; } set { max = Mathf.Max(value, min); } }
		public float Range { get { return max - min; } }
	}

	/// <summary>
    /// 角度を0～360に直す
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
	public static float AngleNormalization(float value)
	{
		while(value < 0 || value >= 360)
		{
			if (value < 0) value += 360;
			if (value >= 360) value -= 360;
		}

		return value;
	}

	/// <summary>
    /// 軸
    /// </summary>
	public enum Axis
	{
		x,
		y,
		z,
		xy,
		xz,
		yz,
		xyz
	}

	/// <summary>
    /// 軸のベクトル
    /// </summary>
	static Vector3[] AxisVector = new Vector3[]
	{
		new Vector3(1,0,0),
		new Vector3(0,1,0),
		new Vector3(0,0,1),
		new Vector3(1,1,0),
		new Vector3(1,0,1),
		new Vector3(0,1,1),
		new Vector3(1,1,1),
	};

	public static Vector3 LeftPos(this Transform tf, Axis axis)
	{
		return tf.position - Vector3.Scale(tf.localScale, AxisVector[(int)axis]) / 2.0f;
	}

	public static Vector3 RightPos(this Transform tf, Axis axis)
	{
		return tf.position + Vector3.Scale(tf.localScale, AxisVector[(int)axis]) / 2.0f;
	}

	/// <summary>
    /// 特定の軸だけの値を取る
    /// </summary>
    /// <param name="tf">元となる値</param>
    /// <param name="axis">軸</param>
    /// <returns>絞り込んだ値</returns>
	public static Vector3 ScaleAxis(this Transform tf, Axis axis)
	{
		return Vector3.Scale(tf.lossyScale, AxisVector[(int)axis]);
	}

	/// <summary>
	/// 特定の軸だけの値を取る
	/// </summary>
	/// <param name="vector">元となる値</param>
	/// <param name="axis">軸</param>
	/// <returns>絞り込んだ値</returns>
	public static Vector3 ScaleAxis(this Vector3 vector, Axis axis)
	{
		return Vector3.Scale(vector, AxisVector[(int)axis]);
	}

	/// <summary>
	/// 特定の軸だけの値を取る
	/// </summary>
	/// <param name="vector">元となる値</param>
	/// <param name="axis">軸</param>
	/// <returns>絞り込んだ値</returns>
	public static Vector3 ScaleAxis(this Vector3Int vector, Axis axis)
	{
		return Vector3.Scale((Vector3)vector, AxisVector[(int)axis]);
	}

	/// <summary>
	/// RigidBodyのVelocityに制限をかける
	/// </summary>
	/// <param name="rb">Rigidbody2D</param>
	/// <param name="maxSpeed">最大速度</param>
	/// <returns>補正された速度</returns>
	public static Vector3 VelocityClamp(this Rigidbody rb, float maxSpeed)
	{
		Vector3 velocity = rb.velocity;

		if(Mathf.Abs(velocity.x) > maxSpeed)
		{
			velocity.x = Mathf.Sign(velocity.x) * maxSpeed;
		}
		if (Mathf.Abs(velocity.z) > maxSpeed)
		{
			velocity.z = Mathf.Sign(velocity.z) * maxSpeed;
		}

		return velocity;
	}

	/// <summary>
	/// Vector3をFloorしてVecotor3Intに変換する
	/// </summary>
	/// <param name="v"></param>
	/// <returns></returns>
	public static Vector3Int Floor(this Vector3 v)
	{
		Vector3Int vi = Vector3Int.zero;

		vi.x = (int)Mathf.Floor(v.x);
		vi.y = (int)Mathf.Floor(v.y);
		vi.z = (int)Mathf.Floor(v.z);

		return vi;
	}

	/// <summary>
	/// minからmaxまでループするように値を変換する
	/// </summary>
	/// <param name="value">値</param>
	/// <param name="min">最小値</param>
	/// <param name="max">最大値</param>
	/// <returns></returns>
	public static float ClampNormalization(float value,float min,float max)
	{
		float difference = max - min;

		while (value < min || value >= max)
		{
			if (value < min)  value += difference;
			if (value >= max)  value -= difference;
		}

		return value;
	}

	/// <summary>
	/// 2つの値を入れ替える
	/// </summary>
	public static void Flip(ref int a, ref int b)
	{
		int t;
		t = a;
		a = b;
		b = t;
	}

	/// <summary>
	/// 角度を90度単位で近いものに変換する
	/// </summary>
	/// <param name="value">角度</param>
	/// <returns>返還後の角度</returns>
	public static float Clamp90MultipleAngle(float value)
	{
		value = Extend.AngleNormalization(value);

		float[] data = new float[] { 0, 90, 180, 270 };
		int index = 0;

		for (int i = 1; i < data.Length; i++)
		{
			float a = Mathf.Abs(value - data[index]);
			float b = Mathf.Abs(value - data[i]);

			if (b < a)
			{
				index = i;
			}
		}

		return data[index];
	}

	/// <summary>
	/// ベクトルを90角度単位で近いものに直す
	/// </summary>
	/// <param name="vector">ベクトル</param>
	/// <returns>変換後のベクトル</returns>
	public static Vector3 Clamp90MultipleVector(Vector3 vector)
	{
		vector = vector.normalized;

		float angle = Mathf.Atan2(vector.z, vector.x) * Mathf.Rad2Deg;
		angle = AngleNormalization(angle);
		angle = Clamp90MultipleAngle(angle);

		Vector3 rv = Vector3.zero;

		rv.x = Mathf.Cos(angle * Mathf.Deg2Rad);
		rv.z = Mathf.Sin(angle * Mathf.Deg2Rad);

		return rv;
	}

	/// <summary>
	/// 角度をベクトルに変換する
	/// </summary>
	/// <param name="angle">角度</param>
	/// <returns>ベクトル</returns>
	public static Vector3 AngleToVector(float angle)
	{
		Vector3 vector = Vector3.zero;

		vector.x = Mathf.Cos(angle * Mathf.Deg2Rad);
		vector.z = Mathf.Sin(angle * Mathf.Deg2Rad);

		return vector;
	}

	/// <summary>
	/// 与えられた数値から一番大きい値の要素番号を返す
	/// </summary>
	/// <param name="list">float配列</param>
	/// <returns>要素数</returns>
	public static int MaxIndex(float[] list)
	{
		int index = 0;
		for(int i = 1; i < list.Length; i++)
		{
			if(list[index] < list[i])
			{
				index = i;
			}
		}

		return index;
	}

	/// <summary>
	/// 与えられた数値から一番小さい値の要素番号を返す
	/// </summary>
	/// <param name="list">float配列</param>
	/// <returns>要素数</returns>
	public static int MinIndex(float[] list)
	{
		int index = 0;
		for (int i = 1; i < list.Length; i++)
		{
			if (list[index] > list[i])
			{
				index = i;
			}
		}

		return index;
	}

	/// <summary>
    /// ImageのAlphaだけを変える
    /// </summary>
    /// <param name="image">イメージ</param>
    /// <param name="alpth">設定するアルファ値</param>
	public static void SetAlpth(this Image image, float alpth)
	{
		Color color = image.color;
		color.a = alpth;

		image.color = color;
	}
}