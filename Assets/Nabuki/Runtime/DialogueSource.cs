using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if ADDRESSABLE_EXISTS
using UnityEngine.AddressableAssets;
#endif
using NaughtyAttributes;

namespace Nabuki
{
    [Serializable]
    public class DialogueSource
    {
        public enum SourceType
        {
            Resources,
#if ADDRESSABLE_EXISTS
            Addressable = 2,
#endif
            Custom = 255
        }

        [SerializeField] SourceType sourceType;
        [SerializeField, HideIf("sourceType", SourceType.Custom)] string dialoguePath, imagePath, soundPath, objectPath;
        [SerializeField, ShowIf("sourceType", SourceType.Custom)] DialogueSourceProvider provider;
#if ADDRESSABLE_EXISTS
        [SerializeField, ShowIf("sourceType", SourceType.Addressable)] bool useFormatPath;
#endif

        Dictionary<string, Sprite> spriteDic;
        Dictionary<string, AudioClip> audioDic;

        public DialogueSource()
        {
            spriteDic = new Dictionary<string, Sprite>();
            audioDic = new Dictionary<string, AudioClip>();
        }

        public IEnumerator GetDialogueAsync(string key, Action<string> callback)
        {
            switch(sourceType)
            {
                case SourceType.Resources:
                    var progress = Resources.LoadAsync<TextAsset>(Path.Combine(dialoguePath, key).Replace("\\", "/"));
                    yield return new WaitUntil(() => progress.isDone);
                    var ta = progress.asset as TextAsset;
                    callback(ta.text);
                    break;
#if ADDRESSABLE_EXISTS
                case SourceType.Addressable:
                    var finalPath = useFormatPath
                        ? string.Format(dialoguePath, key)
                        : Path.Combine(dialoguePath, key).Replace("\\", "/");
                    var progress3 = Addressables.LoadAssetAsync<TextAsset>(finalPath);
                    yield return new WaitUntil(() => progress3.IsDone);
                    callback(progress3.Result.text);
                    Addressables.Release(progress3);
                    break;
#endif
                case SourceType.Custom:
                    yield return provider.GetDialogue(key, result => { callback(result); });
                    break;
            }
        }

        public IEnumerator GetSpriteAsync(string key, Action<Sprite> callback)
        {
            if (!spriteDic.ContainsKey(key))
            {
                switch(sourceType)
                {
                    case SourceType.Resources:
                        var progress = Resources.LoadAsync<Sprite>(Path.Combine(imagePath, key).Replace("\\", "/"));
                        yield return new WaitUntil(() => progress.isDone);
                        spriteDic.Add(key, progress.asset as Sprite);
                        break;
#if ADDRESSABLE_EXISTS
                    case SourceType.Addressable:
                        var finalPath = useFormatPath
                            ? string.Format(imagePath, key)
                            : Path.Combine(imagePath, key).Replace("\\", "/");

                        var pathProgress = Addressables.LoadResourceLocationsAsync(finalPath);
                        yield return pathProgress;

                        if (pathProgress.Result.Count > 0)
                        {
                            var progress3 = Addressables.LoadAssetAsync<Sprite>(finalPath);
                            yield return progress3;
                            spriteDic.Add(key, progress3.Result);
                        }
                        break;
#endif
                    case SourceType.Custom:
                        yield return provider.GetSprite(key, result => { spriteDic.Add(key, result); });
                        break;
                }
            }

            callback(spriteDic.ContainsKey(key) ? spriteDic[key] : null);
            yield break;
        }

        public IEnumerator GetSoundAsync(string key, Action<AudioClip> callback)
        {
            if(!audioDic.ContainsKey(key))
            {
                switch (sourceType)
                {
                    case SourceType.Resources:
                        var progress = Resources.LoadAsync<AudioClip>(Path.Combine(soundPath, key).Replace("\\", "/"));
                        yield return new WaitUntil(() => progress.isDone);
                        audioDic.Add(key, progress.asset as AudioClip);
                        break;
#if ADDRESSABLE_EXISTS
                    case SourceType.Addressable:
                        var finalPath = useFormatPath
                            ? string.Format(soundPath, key)
                            : Path.Combine(soundPath, key).Replace("\\", "/");
                        var progress3 = Addressables.LoadAssetAsync<AudioClip>(finalPath);
                        yield return new WaitUntil(() => progress3.IsDone);
                        audioDic.Add(key, progress3.Result);
                        break;
#endif
                    case SourceType.Custom:
                        yield return provider.GetSound(key, result => { audioDic.Add(key, result); });
                        break;
                }
            }

            callback(audioDic[key]);
            yield break;
        }

        public IEnumerator GetObjectAsync(string key, Action<GameObject> callback)
        {
            switch (sourceType)
            {
                case SourceType.Resources:
                    var progress = Resources.LoadAsync<GameObject>(Path.Combine(objectPath, key).Replace("\\", "/"));
                    yield return new WaitUntil(() => progress.isDone);
                    callback(GameObject.Instantiate(progress.asset as GameObject));
                    break;
#if ADDRESSABLE_EXISTS
                case SourceType.Addressable:
                    var finalPath = useFormatPath
                        ? string.Format(objectPath, key)
                        : Path.Combine(objectPath, key).Replace("\\", "/");
                    var progress3 = Addressables.InstantiateAsync(finalPath);
                    yield return new WaitUntil(() => progress3.IsDone);
                    callback(progress3.Result);
                    break;
#endif
                case SourceType.Custom:
                    yield return provider.GetObject(key, result => { callback(result); });
                    break;
            }
        }

        public void Dispose()
        {
#if ADDRESSABLE_EXISTS
            if(sourceType == SourceType.Addressable)
            {
                foreach (var sprite in spriteDic)
                    Addressables.Release(sprite.Value);
                foreach (var audio in audioDic)
                    Addressables.Release(audio.Value);
            }
#endif
            if (sourceType == SourceType.Custom)
                provider.Dispose();

            spriteDic.Clear();
            audioDic.Clear();
        }
    }
}