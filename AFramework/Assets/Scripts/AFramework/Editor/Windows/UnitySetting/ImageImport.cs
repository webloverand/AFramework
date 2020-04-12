/*******************************************************************
* Copyright(c)
* 文件名称: ImageImport.cs
* 简要描述:
* 作者: 千喜
* 邮箱: 2470460089@qq.com
******************************************************************/
namespace AFramework
{
    using Sirenix.OdinInspector;
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(fileName = "AFImageImport", menuName = "AFramework/CreateAFImageImport", order = 100)]
    public class ImageImport : ScriptableObject
    {
        public bool overridden = true;
        //特定平台设置
        [ShowIf("@this.overridden")]
        public int maxTextureSize = 1024;
        [ShowIf("@this.overridden")]
        public TextureImporterFormat AlphaTextureImporterFormat = TextureImporterFormat.ASTC_RGBA_8x8;
        [ShowIf("@this.overridden")]
        public TextureImporterFormat NoAlphaTextureImporterFormat = TextureImporterFormat.ASTC_RGB_8x8;
    }
}
