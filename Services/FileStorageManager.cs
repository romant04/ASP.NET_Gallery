using Gallery.Data;
using Gallery.InputModels;
using Gallery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Stores;

namespace Gallery.Services
{
    public class FileStorageManager
    {
        private readonly ILogger<FileStorageManager> _logger;
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _environment;
        private readonly int _squareSize = 768;
        private readonly int _sameAspectRatioHeigth = 2160;
        public FileStorageManager(ILogger<FileStorageManager> logger, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _logger = logger;
            _context = context;
            _environment = environment;
        }

        public async Task StoreTus(ITusFile file, CancellationToken token)
        {
            _logger.Log(LogLevel.Debug, "Storing file", file.Id);
            Dictionary<string, tusdotnet.Models.Metadata> metadata = await file.GetMetadataAsync(token);
            string? filename = metadata.FirstOrDefault(m => m.Key == "filename").Value.GetString(System.Text.Encoding.UTF8);
            string? filetype = metadata.FirstOrDefault(m => m.Key == "filetype").Value.GetString(System.Text.Encoding.UTF8);
            string? title = metadata.FirstOrDefault(m => m.Key == "title").Value.GetString(System.Text.Encoding.UTF8);
            string? description = metadata.FirstOrDefault(m => m.Key == "description").Value.GetString(System.Text.Encoding.UTF8);
            bool isPublic = metadata.FirstOrDefault(m => m.Key == "isPublic").Value.GetString(System.Text.Encoding.UTF8) == "jo" ? true : false; 
            string userId = metadata.FirstOrDefault(m => m.Key == "userId").Value.GetString(System.Text.Encoding.UTF8);
            int albumId = int.Parse(metadata.FirstOrDefault(m => m.Key == "albumId").Value.GetString(System.Text.Encoding.UTF8));

            var album = _context.Albums.Include(f => f.Files).Where(c => c.Id == albumId).SingleOrDefault();

            var f = new StoredFile
            {
                Id = Guid.Parse(file.Id),
                OriginalName = filename,
                UploadedAt = DateTime.Now,
                ContentType = filetype,
                DateTaken = DateTime.Now,
                IsPublic = isPublic,
                Description = description,
                Title = title,
                UploaderId = userId,
                Album = new List<Album> { album },
                Thumbnails = new List<Thumbnail>() // initialize Thumbnails with an empty list
            };
            using Stream content = await file.GetContentAsync(token);
            content.Seek(0, SeekOrigin.Begin);

            MemoryStream ims = new MemoryStream();
            MemoryStream oms1 = new MemoryStream();
            MemoryStream oms2 = new MemoryStream();
            IImageFormat format;
            content.CopyTo(ims);
            Image img = Image.Load(ims.ToArray(), out IImageFormat form);
            var result = img.Metadata;
            DateTime imageDatetime;
            if (result.ExifProfile == null ||
                result.ExifProfile.GetValue(ExifTag.DateTimeOriginal) == null ||
                result.ExifProfile.GetValue(ExifTag.DateTimeOriginal).Value == null ||
                DateTime.TryParseExact(result.ExifProfile.GetValue(ExifTag.DateTimeOriginal).Value, "yyyy:MM:dd HH:mm:ss",
                                       CultureInfo.InvariantCulture, DateTimeStyles.None, out imageDatetime) == false)
            {
                //nemáme datum z exifu
                f.DateTaken = f.UploadedAt; //nebo DateTime.Now()
            }
            else
            {
                //bereme datum z Exifu fotky
                f.DateTaken = DateTime.ParseExact(result.ExifProfile.GetValue(ExifTag.DateTimeOriginal).Value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            using (Image image = Image.Load(ims.ToArray(), out format))
            {
                int largestSize = Math.Max(image.Height, image.Width);
                if (image.Width > image.Height)
                {
                    image.Mutate(x => x.Resize(0, _squareSize));
                }
                else
                {
                    image.Mutate(x => x.Resize(_squareSize, 0));
                }
                image.Mutate(x => x.Crop(new Rectangle((image.Width - _squareSize) / 2, (image.Height - _squareSize) / 2, _squareSize, _squareSize)));
                image.Save(oms1, format);
            }
            using (Image image = Image.Load(ims.ToArray(), out format))
            {
                image.Mutate(x => x.Resize(0, _sameAspectRatioHeigth));
                image.Save(oms2, format);
            }
            f.Thumbnails.Add(new Thumbnail { Type = ThumbnailType.Square, Blob = oms1.ToArray() });
            f.Thumbnails.Add(new Thumbnail { Type = ThumbnailType.SameAspectRatio, Blob = oms2.ToArray() });
            f.Position = new List<Position> { new Position { row = 0, column = 0, order = 0, UserId = Guid.Parse(userId) } };
            f.Order = new List<GalleryOrder> { new GalleryOrder { GalleryId = albumId, Order = album.Files.Count + 1 } };
            _context.Files.Add(f);
            await _context.SaveChangesAsync();
            var f1 = Path.Combine(_environment.ContentRootPath, "Uploads", f.Id.ToString());
            using (var fileStream = new FileStream(f1, FileMode.Create))
            {
                await content.CopyToAsync(fileStream);
                await _context.SaveChangesAsync();
            };

            if (album.CoverImageId == null)
            {
                album.CoverImageId = f.Id;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ICollection<StoredFile>> ListAsync()
        {
            return await _context.Files.ToListAsync();
        }

        public async Task<StoredFile> CreateAsync(StoredFile fileRecord)
        {
            _context.Files.Add(fileRecord);
            await _context.SaveChangesAsync();
            return fileRecord;
        }
    }
}
