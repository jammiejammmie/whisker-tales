using UnityEngine;
using WhiskerTales.Core;
using WhiskerTales.Puzzle;

namespace WhiskerTales.Puzzle
{
    // Scene-resident binder that hands V2 tile sprites to TileView's static sprite table.
    // Order matches TileType enum: Fish=0, Milk=1, Yarn=2, Catnip=3, Pawprint=4, Fishbone=5.
    // MainAppSceneBuilder instantiates this in Main_App and binds the 6 sprites via SerializedObject.
    // Mirrors the legacy WhiskerTales.UI.TileSpriteBinder but lives in the V2 namespace and avoids
    // that script's V1 scene assumptions.
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public sealed class V2TileSpriteRegistry : MonoBehaviour
    {
        [SerializeField] private Sprite tileFish;
        [SerializeField] private Sprite tileMilk;
        [SerializeField] private Sprite tileYarn;
        [SerializeField] private Sprite tileCatnip;
        [SerializeField] private Sprite tilePawprint;
        [SerializeField] private Sprite tileFishbone;

        private void Awake()
        {
            Sprite[] sprites = new Sprite[6];
            sprites[(int)TileType.Fish]     = tileFish;
            sprites[(int)TileType.Milk]     = tileMilk;
            sprites[(int)TileType.Yarn]     = tileYarn;
            sprites[(int)TileType.Catnip]   = tileCatnip;
            sprites[(int)TileType.Pawprint] = tilePawprint;
            sprites[(int)TileType.Fishbone] = tileFishbone;

            TileView.SetTileSprites(sprites);

            int bound = 0;

            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null)
                {
                    bound++;
                }
            }

            DebugLogger.Info(LogCategory.Puzzle, "[V2TileSpriteRegistry] Bound " + bound + "/6 tile sprites.");
        }
    }
}
