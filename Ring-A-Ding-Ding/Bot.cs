using Microsoft.Extensions.Logging;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;

using RingADingDing;

namespace RingADingDing
{
    public class Bot
    {
        public static DiscordClient? Client { get; private set; }
        public static CommandsNextExtension? Commands { get; private set; }

        public async Task RunAsync()
        {
            var config = new DiscordConfiguration
            {
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };

            if (AuthHandle.TryGetToken(out var token))
                config.Token = token;
            else
            {
                Console.WriteLine("error: Invalid Token File");
                await Task.Delay(10000);
                return;
            }

            Client = new DiscordClient(config);

            var slash = Client.UseSlashCommands();
            slash.RegisterCommands<SlashCommands>();
            Client.UseVoiceNext();

            Client.Ready += OnClientReady;

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient c, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
