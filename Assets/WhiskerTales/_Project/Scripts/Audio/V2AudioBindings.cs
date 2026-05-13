using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WhiskerTales.Core;
using WhiskerTales.Feel;

namespace WhiskerTales.Audio
{
    // V2 audio installer. SystemsBootstrap creates AudioService programmatically with empty clip
    // bindings, so PlaySfx/PlayBgm warn "Missing clip" until something feeds the lookup tables.
    // We touch the V1 AudioService via reflection (per arch doc: "AudioService를 살린다") rather
    // than refactoring its API. Idempotent — re-runs are no-ops once clips/source are attached.
    public static class V2AudioBindings
    {
        private static bool installed;

        public static void EnsureInstalled()
        {
            if (installed == true)
            {
                return;
            }

            AudioService service = AudioService.Instance;

            if (service == null)
            {
                DebugLogger.Warning(LogCategory.Audio, "[V2AudioBindings] AudioService.Instance null; bindings skipped.");
                return;
            }

            try
            {
                EnsureBgmSource(service);
                BindBgm(service);
                BindSfx(service);
                installed = true;
                DebugLogger.Info(LogCategory.Audio, "[V2AudioBindings] BGM + SFX bindings attached.");
            }
            catch (Exception e)
            {
                DebugLogger.Warning(LogCategory.Audio, "[V2AudioBindings] Failed: " + e.GetType().Name + ": " + e.Message);
            }
        }

        private static void EnsureBgmSource(AudioService service)
        {
            FieldInfo field = typeof(AudioService).GetField("bgmSource", BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                return;
            }

            AudioSource existing = field.GetValue(service) as AudioSource;

            if (existing != null)
            {
                return;
            }

            GameObject go = new GameObject("BgmSource");
            go.transform.SetParent(service.transform, false);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.volume = 0.55f;
            field.SetValue(service, source);
        }

        private static void BindBgm(AudioService service)
        {
            Dictionary<BgmId, AudioClip> dict = GetDictionary<BgmId, AudioClip>(service, "bgmClips");

            if (dict == null)
            {
                return;
            }

            AudioClip ambient = Resources.Load<AudioClip>("Sounds/hanok_ambient_bgm");

            if (ambient != null)
            {
                dict[BgmId.HomeAmbience] = ambient;
                dict[BgmId.GameplayLoop] = ambient;
                dict[BgmId.MeditationAmbience] = ambient;
                dict[BgmId.SleepAmbience] = ambient;
            }
        }

        private static void BindSfx(AudioService service)
        {
            Dictionary<SfxId, AudioClip> dict = GetDictionary<SfxId, AudioClip>(service, "sfxClips");

            if (dict == null)
            {
                return;
            }

            AudioClip meowNabi = Resources.Load<AudioClip>("Audio/Cats/cat_meow_nabi");

            if (meowNabi != null)
            {
                dict[SfxId.CatPet] = meowNabi;
            }

            AudioClip purring = Resources.Load<AudioClip>("Audio/Cats/cat_purring");

            if (purring != null)
            {
                dict[SfxId.SleepEnter] = purring;
            }
        }

        private static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(object target, string fieldName)
        {
            FieldInfo field = typeof(AudioService).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                return null;
            }

            return field.GetValue(target) as Dictionary<TKey, TValue>;
        }
    }
}
