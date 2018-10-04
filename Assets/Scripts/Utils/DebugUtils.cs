using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	/// <summary>
	/// More robust class for drawing debug stuff - generally more helpful than just drawing a line (thanks, Unity).
	/// </summary>
	public static class DebugUtils
	{
		/// <summary>
		/// Draws a thicker line than Debug.DrawLine().
		/// </summary>
		/// <param name="origin">Line start position.</param>
		/// <param name="destination">Line end position.</param>
		/// <param name="color">Color to draw the line.</param>
		public static void DrawThickLine(Vector2 origin, Vector2 destination, Color color)
		{
			const float THICKNESS = 0.004f;

			Vector2 dir = destination - origin;
			Vector2 norm = dir.GetNormal();

			Vector2 cl = origin + norm * THICKNESS;
			Vector2 cr = origin - norm * THICKNESS;

			Debug.DrawLine(origin, destination, color);
			Debug.DrawLine(cl, cl + dir, color);
			Debug.DrawLine(cr, cr + dir, color);
		}

		/// <summary>
		/// Draw a line connecting two points, with squares at either point.
		/// </summary>
		/// <param name="origin">Line start position.</param>
		/// <param name="destination">Line end position.</param>
		/// <param name="colorOrigin">The color to draw the square at origin.</param>
		/// <param name="colorDestination">The color to draw the square at destination.</param>
		/// <param name="size">How large the end point squares should be.</param>
		public static void DrawConnection(Vector2 origin, Vector2 destination, Color colorOrigin, Color colorDestination, float size = 0.1f)
		{
			Debug.DrawLine(origin, destination, Color.Lerp(colorOrigin, colorDestination, 0.5f));
			DrawSquare(origin, size, colorOrigin);
			DrawSquare(destination, size, colorDestination);
		}

		/// <summary>
		/// Draws a square centered at the given point.
		/// </summary>
		/// <param name="center">The center position of the square.</param>
		/// <param name="length">The side length of the square.</param>
		/// <param name="color">The color to draw the square.</param>
		public static void DrawSquare(Vector2 center, float length, Color color)
		{
			float halfL = length / 2f;

			Vector2 topLeft = center + Vector2.left * halfL + Vector2.up * halfL;
			Vector2 topRight = center + Vector2.right * halfL + Vector2.up * halfL;
			Vector2 bottomLeft = center + Vector2.left * halfL + Vector2.down * halfL;
			Vector2 bottomRight = center + Vector2.right * halfL + Vector2.down * halfL;

			Debug.DrawLine(topLeft, topRight, color);
			Debug.DrawLine(topRight, bottomRight, color);
			Debug.DrawLine(bottomRight, bottomLeft, color);
			Debug.DrawLine(bottomLeft, topLeft, color);
		}

		/// <summary>
		/// Draws a line from start to end, with an arrow at the tip indicating direction.
		/// </summary>
		/// <param name="start">Arrow start position.</param>
		/// <param name="end">Arrow end position.</param>
		/// <param name="color">Color to draw the arrow.</param>
		/// <param name="arrowHeight">How tall the arrowhead should be.</param>
		/// <param name="arrowAngle">How wide the arrowhead should be.</param>
		public static void DrawArrow(Vector2 start, Vector2 end, Color color, float arrowHeight = 0.15f, float arrowAngle = 15f)
		{
			//Draw a point on the line
			var dir = (end - start).normalized;
			if (dir == Vector2.zero) return;

			var norm = dir.GetNormal();
			var shift = norm * arrowHeight * Mathf.Tan(Mathf.Deg2Rad * arrowAngle);

			var p0 = end - dir * arrowHeight;

			var p1 = p0 + shift;
			var p2 = p0 - shift;

			Debug.DrawLine(start, p0, color);
			Debug.DrawLine(p1, p2, color);
			Debug.DrawLine(p1, end, color);
			Debug.DrawLine(p2, end, color);
		}
	}
}