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
        [SerializeField] public GameObject prefab;

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

        public AudioClip breakSFX;

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
    [CustomEditor(typeof(BreakableTile))]
    public class BreakableTileEditorGUI : Editor
    {
        bool showSprites = false;
        bool showColours = false;

        public BreakableTile Target
        {
            get { return this.target as BreakableTile; }
        }

        public override void OnInspectorGUI()
        {
            //Target.sprite = (Sprite)EditorGUILayout.ObjectField("Sprite (usually leave null)", Target.sprite, typeof(Sprite), false);

            Target.sprite = null;

            SerializedObject soTarget = new SerializedObject(Target);

            SerializedProperty spPrefab = soTarget.FindProperty("prefab");
            EditorGUILayout.ObjectField(spPrefab);

            SerializedProperty spSprites = soTarget.FindProperty("sprites");
            EditorGUILayout.BeginHorizontal();
            showSprites = EditorGUILayout.Foldout(showSprites, "Sprites");
            EditorGUILayout.IntField(spSprites.arraySize);
            EditorGUILayout.EndHorizontal();

            if(showSprites)
            {
                for (int i = 0; i < spSprites.arraySize; i++)
                {
                    GUIContent label = new GUIContent(((Sprite)spSprites.GetArrayElementAtIndex(i).objectReferenceValue).texture);
                    EditorGUILayout.ObjectField(spSprites.GetArrayElementAtIndex(i), typeof(Sprite), label);
                }
            }

            Target.lerpColour = EditorGUILayout.Toggle("Lerp Colours", Target.lerpColour);

            SerializedProperty spColours = soTarget.FindProperty("colourTransition");

            EditorGUILayout.BeginHorizontal();
            showColours = EditorGUILayout.Foldout(showColours, "Transition Colours");
            EditorGUILayout.IntField(spColours.arraySize);
            EditorGUILayout.EndHorizontal();

            if(showColours)
            {
                for(int i = 0; i < spColours.arraySize; i++)
                {
                    EditorGUILayout.ColorField(spColours.GetArrayElementAtIndex(i).colorValue);
                }
            }
            
            Target.colliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup("Collider Type", Target.colliderType);
            Target.frameDelta = EditorGUILayout.IntSlider("Frame Delta", Target.frameDelta, 0, 60);
            Target.nullWhenBroken = EditorGUILayout.Toggle("Null When Broken", Target.nullWhenBroken);

            SerializedProperty spSFX = soTarget.FindProperty("breakSFX");
            EditorGUILayout.ObjectField(spSFX);

            soTarget.ApplyModifiedProperties();
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
