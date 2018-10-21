using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Tilemaps
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Breakable Tile", menuName = "Tiles/Breakable Tile")]
    public class BreakableTile : Tile
    {
        [SerializeField] public GameObject m_Prefab;

        [Tooltip("sprites[0] is the initial, unbroken sprite, sprites[1]...[n] are animated, ending on sprite[n] when the tile is 'broken'.")]
        public Sprite[] sprites;

        [Tooltip("Toggles tint applied to sprites while tile is being broken.")]
        public bool lerpColour = true;
        [Tooltip("While the tile is being broken, lerp the sprite's tint between colour[0] and colour[1] based on (current sprite index) / (sprite length), with colour[2] being the resting tint of the final frame.")]
        public Color[] colourTransition = new Color[3] { Color.white, Color.magenta, Color.white };

        [Tooltip("The number of frames each cell of the animation will be visible.")]
        [Range(1, char.MaxValue)]
        public int frameDelta = 1;

        [Tooltip("When this tile is broken, set the tile to null in the tileset (do this if you want no sprite in place as the end result).")]
        public bool nullWhenBroken = true;

        [HideInInspector] public WaitForSeconds framewait;

        /*private void OnEnable()
        {
            sprite = sprites[0];
        }*/

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
            framewait = new WaitForSeconds(1 / frameDelta);

            if (go == null)
                go = Instantiate(m_Prefab);

            Tilemap map = tilemap.GetComponent<Tilemap>();

            go.transform.position = map.CellToWorld(position) + Vector3.up * 0.16f + Vector3.right * 0.16f;
            return base.StartUp(position, tilemap, go);
        }

        public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
        {
            tileData.sprite = null;
            tileData.color = this.color;
            tileData.transform = this.transform;
            tileData.gameObject = m_Prefab;
            tileData.flags = this.flags;

            tileData.colliderType = this.colliderType;
        }

        public override bool GetTileAnimationData(Vector3Int location, ITilemap tileMap, ref TileAnimationData tileAnimationData)
        {
            return false;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BreakableTile))]
    public class LevelGridEditorGUI : Editor
    {
        public BreakableTile Target
        {
            get { return this.target as BreakableTile; }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            //RenderStaticPreview(AssetDatabase.GetAssetPath(Target.sprite[0].GetInstanceID()), null, Target.sprite[0].texture.width, Target.sprite[0].texture.height);
            EditorUtility.SetDirty(Target);
            Repaint();
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        { 
            if (Target.sprites[0] == null)
                return null;

            Texture2D tex = new Texture2D(width, height);

            EditorUtility.CopySerialized( Target.sprites[0].texture, tex);

            return tex;
        }
    }
#endif
}
