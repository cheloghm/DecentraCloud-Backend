using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace DecentraCloud.API.Helpers
{
    public static class FileHelper
    {
        public static async Task<byte[]> ConvertToByteArrayAsync(IFormFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }

}
