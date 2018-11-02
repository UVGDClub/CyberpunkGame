using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoundsUtils {

	public static void GetTopLeft(this Bounds bounds, ref Vector2 point) {
        point.x = -bounds.extents.x;
        point.y = bounds.extents.y;
        point += (Vector2) bounds.center;
    }

    public static void GetTopRight( this Bounds bounds, ref Vector2 point ) {
        point.x = bounds.extents.x;
        point.y = bounds.extents.y;
        point += (Vector2)bounds.center;
    }

    public static void GetBottomLeft( this Bounds bounds, ref Vector2 point ) {
        point.x = -bounds.extents.x;
        point.y = -bounds.extents.y;
        point += (Vector2)bounds.center;
    }

    public static void GetBottomRight( this Bounds bounds, ref Vector2 point ) {
        point.x = bounds.extents.x;
        point.y = -bounds.extents.y;
        point += (Vector2)bounds.center;
    }
}
