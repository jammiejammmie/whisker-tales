using System.Collections.Generic;
using UnityEngine;

#if WHISKER_ADDRESSABLES || UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

using WhiskerTales.Core;
namespace WhiskerTales.Assets
{
    public interface IAssetProvider
    {
        System.Threading.Tasks.Task<T> LoadAsync<T>(string key) where T : Object;
        void Release(string key);
        System.Threading.Tasks.Task PreloadBatch(string[] keys);
    }

    public sealed class ResourcesAssetProvider : IAssetProvider
    {
        private readonly Dictionary<string, Object> cache = new Dictionary<string, Object>();

        public async System.Threading.Tasks.Task<T> LoadAsync<T>(string key) where T : Object
        {
            if (string.IsNullOrEmpty(key) == true)
            {
                DebugLogger.Warning(LogCategory.UI, "ResourcesAssetProvider.LoadAsync empty key.");
                return null;
            }

            if (cache.TryGetValue(key, out Object cached) == true)
            {
                return cached as T;
            }

            ResourceRequest request = Resources.LoadAsync<T>(key);

            while (request.isDone == false)
            {
                await System.Threading.Tasks.Task.Yield();
            }

            T asset = request.asset as T;

            if (asset != null)
            {
                cache[key] = asset;
            }

            return asset;
        }

        public void Release(string key)
        {
            if (string.IsNullOrEmpty(key) == true)
            {
                return;
            }

            if (cache.ContainsKey(key) == true)
            {
                cache.Remove(key);
            }
        }

        public async System.Threading.Tasks.Task PreloadBatch(string[] keys)
        {
            if (keys == null)
            {
                return;
            }

            for (int i = 0; i < keys.Length; i++)
            {
                await LoadAsync<Object>(keys[i]);
            }
        }
    }

    public sealed class AddressableAssetProvider : IAssetProvider
    {
#if WHISKER_ADDRESSABLES || UNITY_ADDRESSABLES
        private readonly Dictionary<string, AsyncOperationHandle> handles = new Dictionary<string, AsyncOperationHandle>();
#endif

        public async System.Threading.Tasks.Task<T> LoadAsync<T>(string key) where T : Object
        {
#if WHISKER_ADDRESSABLES || UNITY_ADDRESSABLES
            if (handles.TryGetValue(key, out AsyncOperationHandle existing) == true && existing.IsValid() == true)
            {
                return existing.Result as T;
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            handles[key] = handle;
            await handle.Task;
            return handle.Result;
#else
            ResourcesAssetProvider fallback = new ResourcesAssetProvider();
            return await fallback.LoadAsync<T>(key);
#endif
        }

        public void Release(string key)
        {
#if WHISKER_ADDRESSABLES || UNITY_ADDRESSABLES
            if (handles.TryGetValue(key, out AsyncOperationHandle handle) == true)
            {
                if (handle.IsValid() == true)
                {
                    Addressables.Release(handle);
                }

                handles.Remove(key);
            }
#endif
        }

        public async System.Threading.Tasks.Task PreloadBatch(string[] keys)
        {
            if (keys == null)
            {
                return;
            }

            for (int i = 0; i < keys.Length; i++)
            {
                await LoadAsync<Object>(keys[i]);
            }
        }
    }

    public static class BackgroundAssetGroups
    {
        public static readonly string[] Zone1 =
        {
            "bg_zone1_stage1", "bg_zone1_stage2", "bg_zone1_stage3", "bg_zone1_stage4", "bg_zone1_stage5"
        };

        public static readonly string[] Zone2 =
        {
            "bg_zone2_stage1", "bg_zone2_stage2", "bg_zone2_stage3", "bg_zone2_stage4", "bg_zone2_stage5"
        };

        public static readonly string[] Zone3 =
        {
            "bg_zone3_stage1", "bg_zone3_stage2", "bg_zone3_stage3", "bg_zone3_stage4", "bg_zone3_stage5"
        };
    }
}
