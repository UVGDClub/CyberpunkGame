using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Tilemaps
{

    [System.Serializable]
    [CreateAssetMenu(fileName = "New Door Tile", menuName = "Tiles/Door Tile")]
    public class DoorTile : Tile
    {
        public Sprite[] sprites;
        public GameObject prefab;

        public AudioClip openSFX;
        public AudioClip closeSFX;

        [Tooltip("The number of frames each cell of the animation will be visible.")]
        [Range(1, char.MaxValue)]
        public int frameDelta = 1;
        [HideInInspector] public WaitForSeconds framewait;

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
            framewait = new WaitForSeconds(1 / (float)frameDelta);

            if (Application.isPlaying)
                go = Instantiate(prefab);

            Tilemap map = tilemap.GetComponent<Tilemap>();
            go.transform.position = map.CellToWorld(position) + Vector3.up * 0.5f * map.cellSize.y + Vector3.right * 0.5f * map.cellSize.x;

            if (Application.isPlaying)
                go.transform.parent = map.transform;
            else
                return base.StartUp(position, tilemap, null);

            map.RefreshTile(position);  //Refresh the tile so that the sprite is null
            
            return true; // base.StartUp(position, tilemap, null);
        }

        public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
        {
            if (Application.isPlaying)
                tileData.sprite = null;
            else
                tileData.sprite = sprites[0];

            tileData.color = this.color;
            tileData.transform = this.transform;

            tileData.gameObject = this.prefab;

            tileData.flags = this.flags;

            tileData.colliderType = this.colliderType;

        }

        public override bool GetTileAnimationData(Vector3Int location, ITilemap tileMap, ref TileAnimationData tileAnimationData)
        {
            return false;
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DoorTile))]
    public class DoorTileEditorGUI : Editor
    {
        //bool showSprites = false;
        //bool showColours = false;

        public DoorTile Target
        {
            get { return this.target as DoorTile; }
        }

        public override void OnInspectorGUI()
        {
            //Target.sprite = (Sprite)EditorGUILayout.ObjectField("Sprite (usually leave null)", Target.sprite, typeof(Sprite), false);
            DrawDefaultInspector();
            
            EditorUtility.SetDirty(Target);

            Repaint();
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            if (Target.sprites[0] == null)
                return null;

            Texture2D tex = new Texture2D(width, height);

            EditorUtility.CopySerialized(Target.sprites[0].texture, tex);

            return tex;
        }
    }
#endif

}
