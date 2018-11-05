using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Tilemaps
{

    public class HazardTile : Tile
    {
        public Sprite sprite;
        public int damage = 1;
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(HazardTile))]
    public class HazardTileEditorGUI : Editor
    {
        HazardTile Target { get { return (HazardTile)target; } }

        public override void OnInspectorGUI()
        {

        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            if (Target.sprite == null)
                return null;

            Texture2D tex = new Texture2D(width, height);

            EditorUtility.CopySerialized(Target.sprite.texture, tex);

            return tex;
        }
    }

#endif

}