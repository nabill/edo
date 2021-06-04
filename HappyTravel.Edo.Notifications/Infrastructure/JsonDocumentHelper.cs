using System.IO;
using System.Text;
using System.Text.Json;

namespace HappyTravel.Edo.Notifications.Infrastructure
{
    public static class JsonDocumentHelper
    {
        public static string ToJsonString(this JsonDocument jsonDocument)
        {
            using var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });
            jsonDocument.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
