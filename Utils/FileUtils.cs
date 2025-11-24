using System.Drawing;

namespace WebBanMayTinh.Utils
{
    public class FileUtils
    {
        /// <summary>
        /// Lưu 1 file và trả về đường dẫn của file
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Đường dẫn file</returns>
        public async static Task<string> Upload (IFormFile file)
        {
            string wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string folder = Path.Combine(wwwRoot, "assets", "uploads");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string fileExt = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid()}{fileExt}";
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string urlPath = $"/assets/uploads/{fileName}";

            return urlPath;
        }
    }
}
