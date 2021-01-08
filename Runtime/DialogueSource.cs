using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Nabuki
{
    public class DialogueSource
    {
        public readonly DialogueSourceType sourceType;
        public string imagePath, soundPath, objectPath;
        public AssetBundle imageBundle, soundBundle, objectBundle;

        public DialogueSource(DialogueSourceType type)
        {
            sourceType = type;
        }

        public DialogueSource(string ip, string sp, string op)
        {
            imagePath = ip;
            soundPath = sp;
            objectPath = op;
            sourceType = DialogueSourceType.Resources;
        }

        public DialogueSource(AssetBundle ib, AssetBundle sb, AssetBundle ob)
        {
            imageBundle = ib;
            soundBundle = sb;
            objectBundle = ob;
            sourceType = DialogueSourceType.AssetBundle;
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
            switch(sourceType)
            {
                case DialogueSourceType.Resources:
                    var progress = Resources.LoadAsync<Sprite>(Path.Combine(imagePath, key).Replace("\\", "/"));
                    yield return new WaitUntil(() => progress.isDone);
                    callback(progress.asset as Sprite);
                    break;
                case DialogueSourceType.AssetBundle:
                    var progress2 = imageBundle.LoadAssetAsync<Texture2D>(key);
                    yield return new WaitUntil(() => progress2.isDone);
                    var texture = progress2.asset as Texture2D;
                    callback(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
                    break;
            }
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
            switch(sourceType)
            {
                case DialogueSourceType.Resources:
                    var progress = Resources.LoadAsync<AudioClip>(Path.Combine(soundPath, key).Replace("\\", "/"));
                    yield return new WaitUntil(() => progress.isDone);
                    callback(progress.asset as AudioClip);
                    break;
                case DialogueSourceType.AssetBundle:
                    var progress2 = soundBundle.LoadAssetAsync<AudioClip>(key);
                    yield return new WaitUntil(() => progress2.isDone);
                    callback(progress2.asset as AudioClip);
                    break;
            }
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
            }
        }
    }

    public enum DialogueSourceType { Resources, AssetBundle }
}