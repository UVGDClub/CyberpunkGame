using UnityEngine;

/// <summary>
/// Extensions class. Don't call this class (Extensions.), these methods will be located on the types provided as the first parameter. 
/// For example: 
///		Vector2 newV2 = new Vector2();
///		Vector2 normal = newV2.GetNormal()
/// </summary>
public static class Extensions
{
	//Vectors
	public static Vector2 ToV2(this Vector3 v3, bool zIsUp = false)
	{
		return new Vector2(v3.x, !zIsUp ? v3.y : v3.z);
	}

	public static Vector3 ToV3(this Vector2 v2, float setNewDimensionTo = 0f, bool zIsUp = false)
	{
		if (!zIsUp) return new Vector3(v2.x, v2.y, setNewDimensionTo);
		else return new Vector3(v2.x, setNewDimensionTo, v2.y);
	}

	public static Vector2 GetNormal(this Vector2 dir)
	{
		var normTmp = Vector3.Cross(dir, Vector3.forward);
		return new Vector2(normTmp.x, normTmp.y);
	}
}