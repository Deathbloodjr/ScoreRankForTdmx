using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ScoreRankForTdmx.Patches
{
    internal class AssetUtility
    {
        static Dictionary<string, Sprite> LoadedSprites;

        static public Sprite LoadSprite(string spriteFilePath)
        {
            if (LoadedSprites == null)
            {
                LoadedSprites = new Dictionary<string, Sprite>();
            }
            if (LoadedSprites.ContainsKey(spriteFilePath))
            {
                return LoadedSprites[spriteFilePath];
            }
            else if (File.Exists(spriteFilePath))
            {
                LoadedSprites.Add(spriteFilePath, LoadSpriteFromFile(spriteFilePath));
                return LoadedSprites[spriteFilePath];
            }
            // otherwise, the file doesn't exist, log an error, and return null (or hopefully a small transparent sprite
            else
            {
                Plugin.LogError("Could not find file: " + spriteFilePath);
                // Instead of null, could I have this return just a 1x1 transparent sprite or something?

                // Creates a transparent 2x2 texture, and returns that as the sprite
#if TAIKO_IL2CPP
                Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, 1, false, (IntPtr)0);
#elif TAIKO_MONO
                Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, 1, false);
#endif
                Color fillColor = Color.clear;
                Color[] fillPixels = new Color[tex.width * tex.height];
                for (int i = 0; i < fillPixels.Length; i++)
                {
                    fillPixels[i] = fillColor;
                }
                tex.SetPixels(fillPixels);
                tex.Apply();

                Rect rect = new Rect(0, 0, tex.width, tex.height);
                LoadedSprites.Add(spriteFilePath, Sprite.Create(tex, rect, new Vector2(0, 0)));
                return LoadedSprites[spriteFilePath];
            }
        }

        static private Sprite LoadSpriteFromFile(string spriteFilePath)
        {
#if TAIKO_IL2CPP
            Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, 1, false, (IntPtr)0);
#elif TAIKO_MONO
            Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, 1, false);
#endif
            if (!File.Exists(spriteFilePath))
            {
                Plugin.Log.LogError("Could not find file: " + spriteFilePath);
            }
            else
            {
#if TAIKO_IL2CPP
                tex.LoadRawTextureDataImplArray(File.ReadAllBytes(spriteFilePath));
#elif TAIKO_MONO
                tex.LoadImage(File.ReadAllBytes(spriteFilePath));
#endif
            }


            Rect rect = new Rect(0, 0, tex.width, tex.height);
            return Sprite.Create(tex, rect, new Vector2(0, 0));
        }

        static public GameObject GetOrCreateEmptyChild(GameObject parent, string name, Vector2 position)
        {
            var child = GetChildByName(parent, name);
            if (child == null)
            {
                child = CreateEmptyObject(parent, name, position);
            }
            return child;
        }

        static public GameObject CreateEmptyObject(GameObject parent, string name, Vector2 position)
        {
            Rect rect = new Rect(position, Vector2.zero);
            return CreateEmptyObject(parent, name, rect);
        }

        public static GameObject GetChildByName(GameObject obj, string name)
        {
            Transform trans = obj.transform;
            Transform childTrans = trans.Find(name);
            if (childTrans != null)
            {
                return childTrans.gameObject;
            }
            else
            {
                return null;
            }
        }

        static public GameObject CreateEmptyObject(GameObject parent, string name, Rect rect)
        {
            GameObject newObject = new GameObject(name);
            if (parent != null)
            {
                newObject.transform.SetParent(parent.transform);
            }
            SetRect(newObject, rect);
            return newObject;
        }

#region Image

        static public GameObject GetOrCreateImageChild(GameObject parent, string name, Vector2 position, string spriteFilePath)
        {
            var imageChild = GetChildByName(parent, name);
            if (imageChild == null)
            {
                imageChild = CreateImageChild(parent, name, position, spriteFilePath);
            }
            else
            {
                imageChild.GetOrAddComponent<Image>().sprite = LoadSprite(spriteFilePath);
            }
            return imageChild;
        }

        static public GameObject CreateImageChild(GameObject parent, string name, Rect rect, Color32 color)
        {
            GameObject newObject = CreateEmptyObject(parent, name, rect);
            var image = newObject.GetOrAddComponent<Image>();
            image.color = color;

            return newObject;
        }

        static public GameObject CreateImageChild(GameObject parent, string name, Vector2 position, string spriteFilePath)
        {
            var sprite = LoadSprite(spriteFilePath);
            return CreateImageChild(parent, name, position, sprite);
        }

        static public GameObject CreateImageChild(GameObject parent, string name, Rect rect, string spriteFilePath)
        {
            var sprite = LoadSprite(spriteFilePath);
            return CreateImageChild(parent, name, rect, sprite);
        }

        static public GameObject CreateImageChild(GameObject parent, string name, Vector2 position, Sprite sprite)
        {
            Rect rect = new Rect(position, new Vector2(sprite.rect.width, sprite.rect.height));
            return CreateImageChild(parent, name, rect, sprite);
        }

        static public GameObject CreateImageChild(GameObject parent, string name, Rect rect, Sprite sprite)
        {
            GameObject newObject = CreateEmptyObject(parent, name, rect);
            var image = newObject.GetOrAddComponent<Image>();
            image.sprite = sprite;

            return newObject;
        }

        static public void ChangeImageColor(GameObject gameObject, Color32 color)
        {
            var image = gameObject.GetOrAddComponent<Image>();
            image.color = color;
        }

        static public Image ChangeImageSprite(GameObject gameObject, string spriteFilePath)
        {
            var image = gameObject.GetOrAddComponent<Image>();
            return ChangeImageSprite(image, spriteFilePath);
        }

        static public Image ChangeImageSprite(GameObject gameObject, Sprite sprite)
        {
            var image = gameObject.GetOrAddComponent<Image>();
            return ChangeImageSprite(image, sprite);
        }

        static public Image ChangeImageSprite(Image image, string spriteFilePath)
        {
            var sprite = LoadSprite(spriteFilePath);
            if (sprite == null)
            {
                return image;
            }
            return ChangeImageSprite(image, sprite);
        }

        static public Image ChangeImageSprite(Image image, Sprite sprite)
        {
            image.sprite = sprite;
            return image;
        }



#endregion

#region RectTransform

        // This feels kinda repetitive, but I think it's fine
        static public RectTransform SetRect(GameObject gameObject, Rect rect)
        {
            var rectTransform = gameObject.GetOrAddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
            rectTransform.anchoredPosition = new Vector2(rect.x, rect.y);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = Vector2.zero;
            return rectTransform;
        }
        static public RectTransform SetRect(GameObject gameObject, Rect rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rectTransform = SetRect(gameObject, rect);
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            return rectTransform;
        }
        static public void SetRect(GameObject gameObject, Rect rect, Vector2 pivot)
        {
            var rectTransform = SetRect(gameObject, rect);
            rectTransform.pivot = pivot;
        }
        static public void SetRect(GameObject gameObject, Rect rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            var rectTransform = SetRect(gameObject, rect, anchorMin, anchorMax);
            rectTransform.pivot = pivot;
        }

#endregion

        public static IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds, bool deleteAfter = false)
        {
            float elapsedTime = 0;
            Vector3 startingPos = objectToMove.transform.position;
            while (elapsedTime < seconds)
            {
                objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            objectToMove.transform.position = end;
            if (deleteAfter)
            {
                GameObject.Destroy(objectToMove);
            }
        }

        public static IEnumerator ChangeTransparencyOverSeconds(GameObject obj, float seconds, bool makeVisible)
        {
            float endValue = makeVisible ? 1f : 0f;
            var image = obj.GetComponent<Image>();
            float imageStartValue = 0f;
            if (image != null)
            {
                imageStartValue = image.color.a;
            }

            float elapsedTime = 0;
            while (elapsedTime < seconds)
            {
                if (image != null)
                {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(imageStartValue, endValue, elapsedTime / seconds));
                }
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            if (image != null)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, endValue);
            }
        }
    }


    public static class Extensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent<T>(out T t))
            {
                return t;
            }
            else
            {
                return gameObject.AddComponent<T>();
            }
        }
    }
}
