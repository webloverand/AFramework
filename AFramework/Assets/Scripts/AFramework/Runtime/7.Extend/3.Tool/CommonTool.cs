namespace AFramework
{
    using UnityEngine;

    public class CommonTool : MonoBehaviour
    {
        /// <summary>
        /// 跳转链接
        /// </summary>
        public static void UpdateAPP(string URL)
        {
            Application.OpenURL(URL);
        }
        public static Sprite TextureToSprite(Texture2D texture2D)
        {
            //创建一个Sprite,以Texture2D对象为基础
            Sprite sp = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
            return sp;
        }
    }

}
