using UnityEngine;
using System.Collections;

namespace Utils
{
	public struct CastInfo
	{
		public Ray2D ray;
		public RaycastHit2D hitInfo;
		public bool didHit;
		public float maxCastDistance;
	}

	public static class RaycastUtils
	{
		public static Ray2D RayZero
		{
			get
			{
				return new Ray2D(Vector2.zero, Vector2.zero);
			}
		}

		public static bool IsValid(Ray2D ray)
		{
			return ray.direction != Vector2.zero;
		}

		public static CastInfo Raycast(Vector2 origin, Vector2 direction, float maxDistance, int layerMask)
		{
			return Raycast(new Ray2D(origin, direction), maxDistance, layerMask);
		}

		public static CastInfo Raycast(Ray2D ray, float maxDistance, int layerMask)
		{
			var hitInfo = Physics2D.Raycast(ray.origin, ray.direction, maxDistance, layerMask);
			bool didHit = hitInfo.collider != null;
			return new CastInfo { ray = ray, hitInfo = hitInfo, didHit = didHit, maxCastDistance = maxDistance };
		}

		public static CastInfo LineCast(Vector2 start, Vector2 end, int layerMask)
		{
			var dir = end - start;
			var ray = new Ray2D(start, dir.normalized);
			return Raycast(ray, dir.magnitude, layerMask);
		}
		
	}
}