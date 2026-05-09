using UnityEngine;
using WhiskerTales.Puzzle;

namespace WhiskerTales.UI
{
    /// <summary>
    /// 인계 패킷 §4-3 타일 6종 sprite를 TileView에 일괄 주입.
    /// TileType 순서대로 Inspector에서 할당:
    ///   0=Fish, 1=Milk, 2=Yarn, 3=Catnip, 4=Pawprint, 5=Fishbone
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class TileSpriteBinder : MonoBehaviour
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
            Debug.Log("[TileSpriteBinder] Tile sprites injected into TileView.");
        }
    }
}
