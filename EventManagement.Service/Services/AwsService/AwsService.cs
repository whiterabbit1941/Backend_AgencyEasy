using System;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;

namespace EventManagement.Service
{
    public class AwsService : IAwsService
    {

        #region PRIVATE MEMBERS

        private readonly IConfiguration _configuration;

        #endregion


        #region CONSTRUCTOR

        public AwsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Convert Bitmap To Base64
        /// </summary>
        /// <param name="bitMap">bitMap</param>
        /// <returns>string</returns>
        private string ConvertBitmapToBase64(Bitmap bitMap)
        {
            Bitmap bImage = bitMap;  // Your Bitmap Image
            System.IO.MemoryStream ms = new MemoryStream();
            bImage.Save(ms, ImageFormat.Jpeg);
            byte[] byteImage = ms.ToArray();
            var SigBase64 = Convert.ToBase64String(byteImage);

            return SigBase64;
        }

        /// <summary>
        /// Convert Base64 To Image
        /// </summary>
        /// <param name="baseImageUrl">baseImageUrl</param>
        /// <returns>Image</returns>
        private Image ConvertBase64ToImage(string baseImageUrl)
        {
            //data:image/gif;base64,
            //this image is a single pixel (black)
            byte[] bytes = Convert.FromBase64String(baseImageUrl);

            Image image = null;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }

            var imageFile = image;
            image.Dispose();

            return imageFile;
        }

        #endregion 

        #region PUBLIC METHODS

        /// <summary>
        /// ResizeImage
        /// </summary>
        /// <param name="base64">base64</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <returns>string</returns>
        public string ResizeImage(string base64, int width, int height)
        {
            var image = ConvertBase64ToImage(base64);

            var bmp = ResizeImage(image, width, height);

            var resizedBase64 = ConvertBitmapToBase64(bmp);

            return resizedBase64;
        }

        /// <summary>
        /// UploadImageToAws
        /// </summary>
        /// <param name="directory">directory</param>
        /// <param name="base64">base64</param>
        /// <param name="fileName">fileName</param>
        /// <returns>string</returns>
        public async Task<string> UploadImageToAws(string directory, string base64, string fileName)
        {
            var preparedUrl = string.Empty;
            string configaccess = _configuration.GetSection("AWS:AccessKeyID").Value;
            string configsecret = _configuration.GetSection("AWS:SecretAccessKeyID").Value;

            var s3Client = new AmazonS3Client(
                configaccess,
                configsecret,
                RegionEndpoint.USEast1
            );


            try
            {
                byte[] bytes = Convert.FromBase64String(base64);

                using (s3Client)
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = "whitelabelboardimages",
                        CannedACL = S3CannedACL.PublicRead,
                        Key = string.Format("{0}/{1}", directory, fileName)
                    };
                    using (var ms = new MemoryStream(bytes))
                    {
                        request.InputStream = ms;
                        await s3Client.PutObjectAsync(request);
                        preparedUrl = string.Format("https://whitelabelboardimages.s3.amazonaws.com/{0}/{1}", directory, fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("AWS Fail");
            }

            return preparedUrl;

        }

        #endregion

    }
}
