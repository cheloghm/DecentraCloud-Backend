using HeyRed.Mime;

namespace DecentraCloud.API.Helpers
{
    public static class MimeTypeHelper
    {
        public static string GetMimeType(string filename)
        {
            return MimeTypesMap.GetMimeType(filename);
        }
    }
}
