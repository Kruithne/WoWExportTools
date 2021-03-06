﻿using System;
using OpenTK.Graphics.OpenGL;
using WoWFormatLib.SereniaBLPLib;
using WoWFormatLib.Utils;

namespace WoWExportTools.Loaders
{
    class BLPLoader
    {
        public static int LoadTexture(string fileName)
        {
            if(Listfile.TryGetFileDataID(fileName, out var fileDataID))
                return LoadTexture(fileDataID);
            else
                throw new Exception("Couldn't find filedataid for file " + fileName + " in listfile!");
        }

        public static int LoadTexture(uint fileDataID)
        {
            GL.ActiveTexture(TextureUnit.Texture0);

            int textureID = GL.GenTexture();
            using (var blp = new BlpFile(CASC.OpenFile(fileDataID)))
            {
                var bmp = blp.GetBitmap(0);
                if (bmp == null)
                    throw new Exception("BMP is null!");

                GL.BindTexture(TextureTarget.Texture2D, textureID);

                var bmp_data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                bmp.UnlockBits(bmp_data);
            }

            return textureID;
        }

        public static int GenerateAlphaTexture(byte[] values, bool saveToFile = false)
        {
            GL.ActiveTexture(TextureUnit.Texture1);

            var textureId = GL.GenTexture();

            using (var bmp = new System.Drawing.Bitmap(64, 64))
            {
                for (var x = 0; x < 64; x++)
                {
                    for (var y = 0; y < 64; y++)
                    {
                        var color = System.Drawing.Color.FromArgb(values[x * 64 + y], values[x * 64 + y], values[x * 64 + y], values[x * 64 + y]);
                        bmp.SetPixel(y, x, color);
                    }
                }

                GL.BindTexture(TextureTarget.Texture2D, textureId);
                var bmp_data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

                bmp.UnlockBits(bmp_data);

                if (saveToFile)
                {
                    bmp.Save("alphatest_" + textureId + ".bmp");
                }
            }

            GL.ActiveTexture(TextureUnit.Texture0);

            return textureId;
        }
    }
}
