using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NaughtyAttributes;
#if ADDRESSABLE_EXISTS
using UnityEngine.AddressableAssets;
#endif

namespace Nabuki
{
    public enum DialogueSourceType 
    { 
        Resources, AssetBundle,
#if ADDRESSABLE_EXISTS
        Addressable
#endif
    }

    [Serializable]
    public class DialogueSource
    {
        [SerializeField] DialogueSourceType sourceType;
        [SerializeField, HideIf("sourceType", DialogueSourceType.AssetBundle)] string dialoguePath, imagePath, soundPath, objectPath;
        [SerializeField, ShowIf("sourceType", DialogueSourceType.AssetBundle)] AssetBundle dialogueBundle, imageBundle, soundBundle, objectBundle;

        Dictionary<string, Sprite> spriteDic;
        Dictionary<string, AudioClip> audioDic;

        public DialogueSource()
        {
            spriteDic = new Dictionary<string, Sprite>();
            audioDic = new Dictionary<string, AudioClip>();
        }

        public string GetDialogue(string key)
        {
            switch (sourceType)
            {
                case DialogueSourceType.Resources:
                    return Resources.Load<TextAsset>(Path.Combine(dialoguePath, key).Replace("\\", "/")).text;
                case DialogueSourceType.AssetBundle:
                    return dialogueBundle.LoadAsset<TextAsset>(key).text;
                default:
                    return null;
            }
        }

        public IEnumerator GetDialogueAsync(string key, Action<string> callback)
        {
            switch(sourceType)
            {
                case DialogueSourceType.Resources:
                    var progress = Resources.LoadAsync<TextAsset>(Path.Combine(dialoguePath, key).Replace("\\", "/"));
                    yield return new WaitUntil(() => progress.isDone);
                    var ta = progress.asset as TextAsset;
                    callback(ta.text);
                    break;
                case DialogueSourceType.AssetBundle:
                    var progress2 = dialogueBundle.LoadAssetAsync<TextAsset>(key);
                    yield return new WaitUntil(() => progress2.isDone);
                    var ta2 = progress2.asset as TextAsset;
                    callback(ta2.text);
                    break;
#if ADDRESSABLE_EXISTS
                case DialogueSourceType.Addressable:
                    var progress3 = Addressables.LoadAssetAsync<TextAsset>(Path.Combine(dialoguePath, key).Replace("\\", "/"));
                    yield return new WaitUntil(() => progress3.IsDone);
                    callback(progress3.Result.text);
                    Addressables.Release(progress3);
                    break;
#endif
            }
        }

        public Sprite GetSprite(string key)
        {
            switch(sourceType)
            {
                case DialogueSourceType.Resources:
                    return Resources.Load<Sprite>(Path.Combine(imagePath, key).Replace("\\", "/"));
                case DialogueSourceType.AssetBundle:
                    var texture = imageBundle.LoadAsset<Texture2D>(key);
                    return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                default:
                    return null;
            }
        }

        public IEnumerator GetSpriteAsync(string key, Action<Sprite> callback)
        {
            if (!spriteDic.ContainsKey(key))
            {
                switch(sourceType)
                {
                    case DialogueSourceType.Resources:
                        var progress = Resources.LoadAsync<Sprite>(Path.Combine(imagePath, key).Replace("\\", "/"));
                        yield return new WaitUntil(() => progress.isDone);
                        spriteDic.Add(key, progress.asset as Sprite);
                        break;
                    case DialogueSourceType.AssetBundle:
                        var progress2 = imageBundle.LoadAssetAsync<Texture2D>(key);
                        yield return new WaitUntil(() => progress2.isDone);
                        var texture = progress2.asset as Texture2D;
                        spriteDic.Add(key, Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
                        break;
#if ADDRESSABLE_EXISTS
                    case DialogueSourceType.Addressable:
                        var pathProgress = Addressables.LoadResourceLocationsAsync(Path.Combine(imagePath, key));
                        yield return pathProgress;

                        if (pathProgress.Result.Count > 0)
                        {
                            var progress3 = Addressables.LoadAssetAsync<Sprite>(pathProgress.Result[0]);
                            yield return progress3;
                            spriteDic.Add(key, progress3.Result);
                        }
                        break;
#endif
                }
            }

            callback(spriteDic.ContainsKey(key) ? spriteDic[key] : null);
            yield break;
        }

        public AudioClip GetSound(string key)
        {
            switch (sourceType)
            {
                case DialogueSourceType.Resources:
                    return Resources.Load<AudioClip>(Path.Combine(soundPath, key).Replace("\\", "/"));
                case DialogueSourceType.AssetBundle:
                    return soundBundle.LoadAsset<AudioClip>(key);
                default:
                    return null;
            }
        }

        public IEnumerator GetSoundAsync(string key, Action<AudioClip> callback)
        {
            if(!audioDic.ContainsKey(key))
            {
                switch (sourceType)
                {
                    case DialogueSourceType.Resources:
                        var progress = Resources.LoadAsync<AudioClip>(Path.Combine(soundPath, key).Replace("\\", "/"));
                        yield return new WaitUntil(() => progress.isDone);
                        audioDic.Add(key, progress.asset as AudioClip);
                        break;
                    case DialogueSourceType.AssetBundle:
                        var progress2 = soundBundle.LoadAssetAsync<AudioClip>(key);
                        yield return new WaitUntil(() => progress2.isDone);
                        audioDic.Add(key, progress2.asset as AudioClip);
                        break;
#if ADDRESSABLE_EXISTS
                    case DialogueSourceType.Addressable:
                        var progress3 = Addressables.LoadAssetAsync<AudioClip>(Path.Combine(soundPath, key));
                        yield return new WaitUntil(() => progress3.IsDone);
                        audioDic.Add(key, progress3.Result);
                        break;
#endif
                }
            }

            callback(audioDic[key]);
            yield break;
        }

        public GameObject GetObject(string key)
        {
            switch(sourceType)
            {
                case DialogueSourceType.Resources:
                    var prefab = Resources.Load<GameObject>(Path.Combine(objectPath, key).Replace("\\", "/"));
                    return GameObject.Instantiate(prefab);
                case DialogueSourceType.AssetBundle:
                    var prefab2 = objectBundle.LoadAsset<GameObject>(key);
                    return GameObject.Instantiate(prefab2);
                default:
                    return null;
            }
        }

        public IEnumerator GetObjectAsync(string key, Action<GameObject> callback)
        {
            switch (sourceType)
            {
                case DialogueSourceType.Resources:
                    var progress = Resources.LoadAsync<GameObject>(Path.Combine(objectPath, key).Replace("\\", "/"));
                    yield return new WaitUntil(() => progress.isDone);
                    callback(GameObject.Instantiate(progress.asset as GameObject));
                    break;
                case DialogueSourceType.AssetBundle:
                    var progress2 = objectBundle.LoadAssetAsync<GameObject>(key);
                    yield return new WaitUntil(() => progress2.isDone);
                    callback(GameObject.Instantiate(progress2.asset as GameObject));
                    break;
#if ADDRESSABLE_EXISTS
                case DialogueSourceType.Addressable:
                    var progress3 = Addressables.InstantiateAsync(Path.Combine(objectPath, key));
                    yield return new WaitUntil(() => progress3.IsDone);
                    callback(progress3.Result);
                    break;
#endif
            }
        }

        public void Dispose()
        {
#if ADDRESSABLE_EXISTS
            if(sourceType == DialogueSourceType.Addressable)
            {
                foreach (var sprite in spriteDic)
                    Addressables.Release(sprite.Value);
                foreach (var audio in audioDic)
                    Addressables.Release(audio.Value);
            }
#endif
            spriteDic.Clear();
            audioDic.Clear();
        }
    }
}