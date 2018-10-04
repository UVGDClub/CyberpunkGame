using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Debugging
{
	/// <summary>
	/// Just for debugging. Simulates moving stuff in the scene.
	/// </summary>
	public class DebugMover : MonoBehaviour
	{
		public Vector2 direction = Vector2.up;
		public float tripTime = 3f;
		public float moveDistance = 5;

		private float timer = 0f;
		private Vector3 targetPos;
		private Vector3 startPos;
		private void Start()
		{
			startPos = transform.position;
			targetPos = startPos + (direction.normalized).ToV3() * moveDistance;
		}

		void Update()
		{
			float t = timer / tripTime;
			transform.position = Vector3.Lerp(startPos, targetPos, t);

			if (t >= 1f)
			{
				var tmp = startPos;
				startPos = targetPos;
				targetPos = tmp;
				timer = 0f;
			}
			else
			{
				timer += Time.deltaTime;
			}
		}
	}
}