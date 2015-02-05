using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageResizer;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.MediaCover
{
    public interface IImageResizer
    {
        void Resize(string source, string destination, int height);
    }

    public class ImageResizer : IImageResizer
    {
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        private bool _imageMagickAvailable = true;

        public ImageResizer(IDiskProvider diskProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public void Resize(string source, string destination, int height)
        {
            try
            {
                using (var sourceStream = _diskProvider.OpenReadStream(source))
                {
                    using (var outputStream = _diskProvider.OpenWriteStream(destination))
                    {
                        if (OsInfo.IsNotWindows && _imageMagickAvailable)
                        {
                            _logger.Trace("Trying to resize image with ImageMagick [{0}].", source);

                            var success = ResizeImageMagick(sourceStream, outputStream, height);
                            if (success)
                            {
                                _logger.Trace("Resized image with ImageMagick.");
                                return;
                            }
                        }

                        {
                            _logger.Trace("Trying to resize image with GDIPlus [{0}].", source);

                            sourceStream.Position = 0;
                            var success = ResizeGDIPlus(sourceStream, outputStream, height);
                            if (success)
                            {
                                _logger.Trace("Resized image with GDIPlus.");
                                return;
                            }
                        }
                    }
                }
            }
            catch
            {
                if (_diskProvider.FileExists(destination))
                {
                    _diskProvider.DeleteFile(destination);
                }
                throw;
            }
        }

        private bool ResizeImageMagick(Stream sourceStream, Stream destinationStream, int height)
        {
            try
            {
                var data = new byte[sourceStream.Length];
                sourceStream.Read(data, 0, data.Length);

                ImageMagick.WandGenesis();
                var wand = ImageMagick.NewWand();
                try
                {

                    if (!ImageMagick.ReadImageBlob(wand, data))
                    {
                        return false;
                    }

                    var curWidth = (int)ImageMagick.GetWidth(wand);
                    var curHeight = (int)ImageMagick.GetHeight(wand);

                    var newWidth = curWidth * height / curHeight;
                    var newHeight = height;

                    ImageMagick.ResizeImage(wand, (IntPtr)newWidth, (IntPtr)newHeight, ImageMagick.Filter.Cubic, 1.0);

                    var newdata = ImageMagick.GetImageBlob(wand);

                    destinationStream.Write(newdata, 0, newdata.Length);

                    return true;
                }
                finally
                {
                    ImageMagick.DestroyWand(wand);
                    ImageMagick.WandTerminus();
                }
            }
            catch (DllNotFoundException)
            {
                _imageMagickAvailable = false;
                return false;
            }
        }

        private bool ResizeGDIPlus(Stream sourceStream, Stream destinationStream, int height)
        {
            var settings = new Instructions();
            settings.Height = height;

            var job = new ImageJob(sourceStream, destinationStream, settings);

            ImageBuilder.Current.Build(job);

            return true;
        }
    }
}