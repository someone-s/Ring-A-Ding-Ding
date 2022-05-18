using DSharpPlus.Entities;

namespace RingADingDing
{

    internal static class PathHandle
    {
        private static string botName = "Ring-A-Ding-Ding";

        public static string GetRootPath()
        {
            var root = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}{Path.DirectorySeparatorChar}{botName}";
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            return root;
        }

        public static string GetAudioPath()
        {
            var path = $"{GetRootPath()}{Path.DirectorySeparatorChar}audio.wav";
            if (!File.Exists(path))
                File.Create(path);
            return path;
        }

        public static string GetServersPath()
        {
            var path = $"{GetRootPath()}{Path.DirectorySeparatorChar}servers";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        public static string GetServerPath(DiscordGuild guild)
        {
            var path = $"{GetServersPath()}{Path.DirectorySeparatorChar}{guild.Id}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        public static string GetServerPath(ulong id)
        {
            var path = $"{GetServersPath()}{Path.DirectorySeparatorChar}{id}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        public static string GetConfigPath()
        {
            var config = $"{GetRootPath()}{Path.DirectorySeparatorChar}config.json";
            if (!File.Exists(config))
                File.CreateText(config);
            return config;
        }
    }
}
