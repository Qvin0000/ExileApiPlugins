using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ExileCore;
using ExileCore.Shared.Enums;
using SharpDX;
using Color = SharpDX.Color;
using Rectangle = System.Drawing.Rectangle;
using RectangleF = SharpDX.RectangleF;

namespace AdvancedUberLabLayout
{
    //http://www.poelab.com/wp-content/uploads/2017/09/2017-09-25_normal.jpg
    public class AdvancedUberLabLayoutCore : BaseSettingsPlugin<Settings>
    {
        public override void AreaChange(AreaInstance area)
        {
            if (Settings.AutoLabDetection.Value)
            {
                switch (area.RealLevel)
                {
                    case 33:
                        Settings.LabType.Value = LabTypes[0];
                        break;
                    case 55:
                        Settings.LabType.Value = LabTypes[1];
                        break;
                    case 68:
                        Settings.LabType.Value = LabTypes[2];
                        break;
                    case 75:
                        Settings.LabType.Value = LabTypes[3];
                        break;
                }
            }
        }

        public override bool Initialise()
        {
            UpdateTime();

#pragma warning disable 4014
            LoadImage();
#pragma warning restore 4014

            Settings.LabType.SetListValues(LabTypes.ToList());

#pragma warning disable 4014
            Settings.LabType.OnValueSelected += delegate { LoadImage(); };
#pragma warning restore 4014

            Input.RegisterKey(Settings.Reload);
            Input.RegisterKey(Settings.ToggleDraw);

            return true;
        }

        public override void Render()
        {
            if (Settings.ToggleDraw.PressedOnce())
                Settings.ShowImage = !Settings.ShowImage;

            if (!Settings.ShowImage) return;

            //if (GameController.Game.IngameState.Data.LabyrinthData == null) return;

            if (Settings.Reload.PressedOnce())
#pragma warning disable 4014
                LoadImage();
#pragma warning restore 4014

            var drawRect = new RectangleF(Settings.X, Settings.Y, ImageWidth * Settings.Size / 100f, ImageHeight * Settings.Size / 100f);

            UpdateTime();

            if (ImageState == ImageCheckState.ReadyToDraw &&
                !GameController.Game.IngameState.IngameUi.OpenLeftPanel.IsVisible &&
                !GameController.Game.IngameState.IngameUi.OpenRightPanel.IsVisible)
            {
                var color = Color.White;
                color.A = (byte) Settings.Transparency;
                Graphics.DrawImage(Path.GetFileName(ImagePathToDraw), drawRect, color);

                var toNextReset = LabResetTime - UTCTime;
                Graphics.DrawText("Reset in: " + toNextReset.ToString(@"hh\:mm\:ss"), drawRect.TopLeft + new Vector2(10, 5), Color.White, 15);

                var name = Path.GetFileNameWithoutExtension(ImagePathToDraw);
                Graphics.DrawText(name, drawRect.TopRight + new Vector2(-10, 5), Color.White, 15, FontAlign.Right);
            }
        }

        private void UpdateTime()
        {
            UTCTime = DateTime.Now.ToUniversalTime();
            LabResetTime = UTCTime.Date.AddDays(1);

            if (Settings.CurrentImageDateDay != UTCTime.Day)
            {
                ImageState = ImageCheckState.Checking;
                DeleteOldImages();
#pragma warning disable 4014
                LoadImage();
#pragma warning restore 4014
            }
        }

        private async Task LoadImage()
        {
            Settings.CurrentImageDateDay = UTCTime.Day;

            ImageState = ImageCheckState.Checking;
            await LoadData(UTCTime);

            if (ImageState == ImageCheckState.NotFound404)
            {
                LogMessage("New lab layout image is not created yet. Loading old image: " + DateTime.Now.AddDays(-1).ToString("dd MMMM"), 10);

                await LoadData(DateTime.Now.AddDays(-1));

                if (ImageState == ImageCheckState.NotFound404)
                {
                    LogMessage("Failed loading OLD lab layout image: " + DateTime.Now.AddDays(-1).ToString("dd MMMM"), 10);
                    ImageState = ImageCheckState.FailedToLoad;
                }
            }
        }

        private async Task LoadData(DateTime data)
        {
            var time = data;
            var month = time.Month.ToString("00");
            var day = time.Day.ToString("00");

            var tempFileName = $"{time.Year}-{month}-{day}_{Settings.LabType.Value}.jpg";

            Directory.CreateDirectory(ImagesDirectory);

            var FilePath = Path.Combine(ImagesDirectory, tempFileName);

            if (File.Exists(FilePath))
            {
                LogMessage("Loading lab layout image from cache", 3);
                ImagePathToDraw = FilePath;
                Graphics.InitImage(FilePath, false);
                ImageState = ImageCheckState.ReadyToDraw;
                return;
            }

            LogMessage("Loading new lab layout image from site", 3);

            var ImageUrl = $"https://www.poelab.com/wp-content/labfiles/" + tempFileName;

            ImageState = ImageCheckState.Downloading;

            var webClient = new WebClient();

            var imageBytes = new byte[0];

            try
            {
                imageBytes = await webClient.DownloadDataTaskAsync(ImageUrl);
            }
            catch // (Exception ex)
            {
                ImageState = ImageCheckState.NotFound404;

                //Not found
                return;
            }

            Bitmap downloadedImage;

            try
            {
                using (var ms = new MemoryStream(imageBytes))
                {
                    downloadedImage = new Bitmap(ms);
                }

                if (!Directory.Exists(ImagesDirectory))
                    Directory.CreateDirectory(ImagesDirectory);

                downloadedImage =
                    new Bitmap(downloadedImage); //Fix for exception (bitmap save problem https://stackoverflow.com/questions/5813633/a-generic-error-occurs-at-gdi-at-bitmap-save-after-using-savefiledialog )

                downloadedImage = downloadedImage.Clone(new Rectangle(302, 111, ImageWidth, ImageHeight), downloadedImage.PixelFormat);

                downloadedImage.Save(FilePath, ImageFormat.Jpeg);
                Graphics.InitImage(FilePath, false);
                ImageState = ImageCheckState.ReadyToDraw;
                ImagePathToDraw = FilePath;
              
                downloadedImage.Dispose();
            }
            catch (Exception ex)
            {
                LogError("AdvancedUberLabLayout Plugin: Error while cropping or saving image: " + ex.Message, 10);
                ImageState = ImageCheckState.FailedToLoad;
            }
        }

        private void DeleteOldImages()
        {
            if (!Directory.Exists(ImagesDirectory))
                return;

            var imgFiles = Directory.GetFiles(ImagesDirectory);

            foreach (var dir in imgFiles)
            {
                File.Delete(dir);
            }
        }

        private enum ImageCheckState
        {
            Undefined,
            Checking,
            NotFound404,
            Downloading,
            ReadyToDraw,
            FailedToLoad
        }

        #region ClassVariables

        private const string CachedImages = "CachedImages";
        private const string ImageLink = "http://www.poelab.com/wp-content/uploads/";
        private const int ImageWidth = 841;
        private const int ImageHeight = 270;
        private string ImagesDirectory => Path.Combine(DirectoryFullName, CachedImages);
        private DateTime UTCTime;
        private DateTime LabResetTime;
        private string ImagePathToDraw;
        private ImageCheckState ImageState = ImageCheckState.Undefined;

        private readonly string[] LabTypes =
        {
            "normal",
            "cruel",
            "merciless",
            "uber"
        };

        #endregion
    }
}
