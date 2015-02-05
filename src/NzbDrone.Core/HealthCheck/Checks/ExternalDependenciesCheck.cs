using System;
using System.IO;
using System.Runtime.CompilerServices;
using MediaInfoLib;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.MediaCover;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class ExternalDependenciesCheck : HealthCheckBase
    {
        private readonly Logger _logger = NzbDroneLogger.GetLogger(typeof(ExternalDependenciesCheck));

        public override HealthCheck Check()
        {

            if (!TryLoadMediaInfo())
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, "MediaInfo could not be loaded");
            }
            

            if (OsInfo.IsNotWindows)
            {
                if (!TryLoadImageMagick() && !TryLoadGDIPlus())
                {
                    return new HealthCheck(GetType(), HealthCheckResult.Warning, "Sonarr requires ImageMagick or libgdiplus to resize media covers");
                }
            }

            return new HealthCheck(GetType());
        }

        public override bool CheckOnConfigChange
        {
            get
            {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private bool TryLoadMediaInfo()
        {
            try
            {
                var mediaInfo = new MediaInfo();
                _logger.Debug("MediaInfo is available");
                return true;
            }
            catch (Exception ex)
            {
                _logger.DebugException("MediaInfo is not available", ex);
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private bool TryLoadImageMagick()
        {
            try
            {
                ImageMagick.WandGenesis();
                ImageMagick.WandTerminus();
                _logger.Debug("ImageMagick is available");
                return true;
            }
            catch (Exception ex)
            {
                _logger.DebugException("ImageMagick is not available", ex);
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private bool TryLoadGDIPlus()
        {
            try
            {
                var bitmap = new System.Drawing.Bitmap(42, 42);
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                _logger.Debug("libgdiplus is available");
                return true;
            }
            catch (Exception ex)
            {
                _logger.DebugException("libgdiplus is not available", ex);
                return false;
            }
        }
    }
}
