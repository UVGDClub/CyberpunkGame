using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
	Unspecified,
	Left,
	Right
}

public enum SpriteAnchorPoint
{
	Center,
	BottomCenter
}

namespace Utils
{
	public static class GameUtils
	{
		public static class Layers
		{
			public const int Ground = 8;
			public const int GroundMask = (1 << Ground);

			public const int Enemy = 9;
			public const int EnemyMask = (1 << Enemy);
		}



		//Cached yield instructions (so no GC every step in coroutines)
		public static YieldInstruction yieldFixedUpdate = new WaitForFixedUpdate();
		public static YieldInstruction yieldLateUpdate = new WaitForEndOfFrame();

		public static Transform GetRootObject(Transform current)
		{
			while (current.parent != null)
			{
				current = current.parent;
			}
			return current;
		}

		public static Transform GetChild(Transform parent, string name)
		{
			foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
			{
				if (child.name.Equals(name))
					return child;
			}
			Debug.LogError("GetChild failed! - '" + parent.name + "' does not have a child with name '" + name + "'");
			return null;
		}

		public static bool IsVisibleToCamera(Vector2 position)
		{
			var point = Camera.main.WorldToScreenPoint(new Vector3(position.x, position.y, 0f));
			return !((point.x < 0 || point.x > Screen.width) || (point.y < 0 || point.y > Screen.height));
		}
	}
}