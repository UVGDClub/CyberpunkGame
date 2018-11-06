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
    [CreateAssetMenu(fileName = "New Secret Tile", menuName = "Tiles/Secret Tile")]
    public class SecretTile : Tile
    {
        public Sprite m_sprite;
        public Color tint = Color.white;
        public Color cover = Color.white;
        public Color reveal = Color.clear;

            //Start by requiring m_sprites to be static
        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            return false;
        }

        public void ToggleVisibility(Vector3Int position, Tilemap tilemap, bool clear, List<Vector3Int> visited)
        {
            bool nextRevealed = Mathf.Approximately(tilemap.GetColor(position).a, reveal.a);
            if ((nextRevealed && clear) || (!nextRevealed && !clear))
                return;

            visited.Add(position);

            Color c = clear? Color.clear : cover;
            tilemap.SetColor(position, c);

            

            Vector3Int newPosition = Vector3Int.one;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    newPosition.Set(position.x + x, position.y + y, position.z);

                    if (position.x == x && position.y == y || !tilemap.GetTile<SecretTile>(newPosition) || visited.Contains(newPosition))
                        continue;
                    
                    nextRevealed = Mathf.Approximately(tilemap.GetColor(newPosition).a, reveal.a);
                    //Debug.Log("next revealed " + nextRevealed + "[" + next.tint.a + " , " + reveal.a + "] " + "; clear " + clear);
                    if ( (nextRevealed && clear) || (!nextRevealed && !clear) )
                        continue;

                    ToggleVisibility(newPosition, tilemap, clear, ref visited);
                    tilemap.RefreshTile(newPosition);
                }
            }

            tilemap.RefreshTile(position);
        }

        public void ToggleVisibility(Vector3Int position, Tilemap tilemap, bool clear, ref List<Vector3Int> visited)
        {
            //Debug.Log("Occlude running at " + position);

            visited.Add(position);

            Color c = clear ? Color.clear : cover;
            //Debug.Log("Colour before: " + tilemap.GetColor(position));
            tilemap.SetColor(position, c);
            //Debug.Log("Colour after: " + tilemap.GetColor(position));
            
            bool nextRevealed;

            Vector3Int newPosition = Vector3Int.one;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    newPosition.Set(position.x + x, position.y + y, position.z);

                    if (position.x == x && position.y == y || !tilemap.GetTile<SecretTile>(newPosition) || visited.Contains(newPosition))
                        continue;



                    nextRevealed = Mathf.Approximately(tilemap.GetColor(newPosition).a, reveal.a);
                    //Debug.Log("next revealed " + nextRevealed + "[" +next.tint.a + " , " + reveal.a + "] " + "; clear " + clear);
                    if ((nextRevealed && clear) || (!nextRevealed && !clear))
                        continue;

                    ToggleVisibility(newPosition, tilemap, clear, ref visited);
                    tilemap.RefreshTile(newPosition);
                }
            }

            tilemap.RefreshTile(position);
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = this.m_sprite;
            tileData.color = this.tint;
            tileData.transform = Matrix4x4.identity;
            tileData.gameObject = null;
            tileData.flags = this.flags;
            tileData.colliderType = this.colliderType;
        }

        /*public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            base.RefreshTile(position, tilemap);
        }*/

        /*public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
        {
            return base.StartUp(position, tilemap, go);
        }*/
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SecretTile))]
    public class OccludableTileEditorGUI : Editor
    {
        public SecretTile Target
        {
            get { return this.target as SecretTile; }
        }

        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();
            Target.m_sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", Target.m_sprite, typeof(Sprite), false);
            Target.tint = EditorGUILayout.ColorField("Tint", Target.tint);
            Target.cover = EditorGUILayout.ColorField("Cover", Target.cover);
            Target.reveal = EditorGUILayout.ColorField("Reveal", Target.reveal);
            Target.colliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup("Collider Type", Target.colliderType);
            Target.flags = (TileFlags)EditorGUILayout.EnumPopup("Tile Flags", Target.flags);

            EditorUtility.SetDirty(Target);

            Repaint();
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            if (Target.m_sprite == null)
                return null;

            Texture2D tex = new Texture2D(width, height);

            EditorUtility.CopySerialized(Target.m_sprite.texture, tex);

            return tex;
        }
    }
#endif
}
