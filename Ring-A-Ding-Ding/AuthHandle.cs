using System.Text.Json;

namespace RingADingDing
{
    internal static class AuthHandle
    {
        public static bool TryGetToken(out string token)
        {
            var path = PathHandle.GetConfigPath();

            var jr = new Utf8JsonReader(File.ReadAllBytes(path), new JsonReaderOptions { AllowTrailingCommas = true });

            if (JsonDocument.TryParseValue(ref jr, out var json) &&
                json.RootElement.TryGetProperty("token", out var element))
            {
                string? s = element.GetString();
                token = s is null ? string.Empty : s;
                return true;
            }
            else
            {
                token = string.Empty;
                return false;
            }
        }
    }
}
