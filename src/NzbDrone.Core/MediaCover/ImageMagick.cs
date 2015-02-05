using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace NzbDrone.Core.MediaCover
{
    // Source: http://www.toptensoftware.com/Articles/17/High-Quality-Image-Resampling-in-Mono-Linux
    public static class ImageMagick
    {
        public enum Filter
        {
            Undefined,
            Point,
            Box,
            Triangle,
            Hermite,
            Hanning,
            Hamming,
            Blackman,
            Gaussian,
            Quadratic,
            Cubic,
            Catrom,
            Mitchell,
            Lanczos,
            Bessel,
            Sinc,
            Kaiser,
            Welsh,
            Parzen,
            Lagrange,
            Bohman,
            Bartlett,
            SincFast
        };

        public enum InterpolatePixel
        {
            Undefined,
            Average,
            Bicubic,
            Bilinear,
            Filter,
            Integer,
            Mesh,
            NearestNeighbor,
            Spline
        };


        [DllImport("MagickWand", EntryPoint = "MagickResizeImage")]
        public static extern bool ResizeImage(IntPtr mgck_wand, IntPtr columns, IntPtr rows, Filter filter_type, double blur);

        [DllImport("MagickWand", EntryPoint = "MagickWandGenesis")]
        public static extern void WandGenesis();

        [DllImport("MagickWand", EntryPoint = "MagickWandTerminus")]
        public static extern void WandTerminus();

        [DllImport("MagickWand", EntryPoint = "NewMagickWand")]
        public static extern IntPtr NewWand();

        [DllImport("MagickWand", EntryPoint = "DestroyMagickWand")]
        public static extern IntPtr DestroyWand(IntPtr wand);

        [DllImport("MagickWand", EntryPoint = "MagickGetImageBlob")]
        public static extern IntPtr GetImageBlob(IntPtr wand, [Out] out IntPtr length);

        [DllImport("MagickWand", EntryPoint = "MagickReadImageBlob")]
        public static extern bool ReadImageBlob(IntPtr wand, IntPtr blob, IntPtr length);

        [DllImport("MagickWand", EntryPoint = "MagickRelinquishMemory")]
        public static extern IntPtr RelinquishMemory(IntPtr resource);

        [DllImport("MagickWand", EntryPoint = "MagickGetImageWidth")]
        public static extern IntPtr GetWidth(IntPtr wand);

        [DllImport("MagickWand", EntryPoint = "MagickGetImageHeight")]
        public static extern IntPtr GetHeight(IntPtr wand);

        public static bool ReadImageBlob(IntPtr wand, byte[] blob)
        {
            GCHandle pinnedArray = GCHandle.Alloc(blob, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();

            bool bRetv = ReadImageBlob(wand, pointer, (IntPtr)blob.Length);

            pinnedArray.Free();

            return bRetv;
        }

        public static byte[] GetImageBlob(IntPtr wand)
        {
            IntPtr len;
            IntPtr buf = GetImageBlob(wand, out len);

            var dest = new byte[len.ToInt32()];
            Marshal.Copy(buf, dest, 0, len.ToInt32());

            RelinquishMemory(buf);

            return dest;
        }
    }
}
