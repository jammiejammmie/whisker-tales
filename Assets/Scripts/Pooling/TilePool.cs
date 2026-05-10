using System.Collections.Generic;
using UnityEngine;

using WhiskerTales.Core;
namespace WhiskerTales.Pooling
{
    public sealed class TilePool : MonoBehaviour
    {
        [SerializeField] private WhiskerTales.Puzzle.TileView tilePrefab;
        [SerializeField] private Transform tileRoot;
        [SerializeField] private int prewarmCount = 80;

        private readonly Dictionary<int, Queue<WhiskerTales.Puzzle.TileView>> typedPools = new Dictionary<int, Queue<WhiskerTales.Puzzle.TileView>>();
        private readonly Queue<GameObject> effectPool = new Queue<GameObject>();

        private void Awake()
        {
            if (tileRoot == null)
            {
                tileRoot = transform;
            }

            Prewarm();
        }

        public void Prewarm()
        {
            if (tilePrefab == null)
            {
                DebugLogger.Warning(LogCategory.Puzzle, "TilePool missing tilePrefab.");
                return;
            }

            for (int i = 0; i < prewarmCount; i++)
            {
                WhiskerTales.Puzzle.TileView tile = CreateTile();
                Release(tile, 0);
            }
        }

        public WhiskerTales.Puzzle.TileView Get(int tileType)
        {
            Queue<WhiskerTales.Puzzle.TileView> pool = GetPool(tileType);
            WhiskerTales.Puzzle.TileView tile = null;

            while (pool.Count > 0 && tile == null)
            {
                tile = pool.Dequeue();
            }

            if (tile == null)
            {
                tile = CreateTile();
            }

            tile.gameObject.SetActive(true);
            return tile;
        }

        public void Release(WhiskerTales.Puzzle.TileView tile, int tileType)
        {
            if (tile == null)
            {
                return;
            }

            ResetTile(tile);
            tile.gameObject.SetActive(false);
            tile.transform.SetParent(tileRoot, false);
            GetPool(tileType).Enqueue(tile);
        }

        public GameObject GetEffect(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            GameObject instance = null;

            while (effectPool.Count > 0 && instance == null)
            {
                instance = effectPool.Dequeue();
            }

            if (instance == null)
            {
                instance = Instantiate(prefab, transform);
            }

            instance.SetActive(true);
            return instance;
        }

        public void ReleaseEffect(GameObject effect)
        {
            if (effect == null)
            {
                return;
            }

            effect.transform.SetParent(transform, false);
            effect.SetActive(false);
            effectPool.Enqueue(effect);
        }

        private WhiskerTales.Puzzle.TileView CreateTile()
        {
            WhiskerTales.Puzzle.TileView tile = Instantiate(tilePrefab, tileRoot);
            tile.name = "Tile_Pooled";
            tile.gameObject.SetActive(false);
            return tile;
        }

        private Queue<WhiskerTales.Puzzle.TileView> GetPool(int tileType)
        {
            if (typedPools.TryGetValue(tileType, out Queue<WhiskerTales.Puzzle.TileView> pool) == false)
            {
                pool = new Queue<WhiskerTales.Puzzle.TileView>();
                typedPools[tileType] = pool;
            }

            return pool;
        }

        private void ResetTile(WhiskerTales.Puzzle.TileView tile)
        {
            tile.transform.localScale = Vector3.one;
            tile.transform.localRotation = Quaternion.identity;
            tile.transform.localPosition = Vector3.zero;
            tile.SetSelected(false);
            tile.SetCoordinates(-1, -1);
        }
    }
}
