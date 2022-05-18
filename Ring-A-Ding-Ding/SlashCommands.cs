using System.Diagnostics;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus.SlashCommands;

namespace RingADingDing
{
    [SlashCommandGroup("General", "Typical commands")]
    public class SlashCommands : ApplicationCommandModule
    {
        [SlashCommand("Ring", "Ring the bong")]
        public async Task Ring(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Ringing"));

            var filePath = PathHandle.GetAudioPath();
            Process? ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            if (ffmpeg is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error"));
                return;
            }

            DiscordChannel[] voiceChannels = ctx.Guild.Channels.Where(c => c.Value.Type == ChannelType.Voice).Select(c => c.Value).ToArray();

            Stream pcm = new MemoryStream();
            await ffmpeg.StandardOutput.BaseStream.CopyToAsync(pcm);

            for (int i = 0; i < voiceChannels.Length; i++)
            {
                DiscordChannel voiceChannel = voiceChannels[i];
                VoiceNextConnection connection = await voiceChannel.ConnectAsync();
                if (connection == null) continue;

                VoiceTransmitSink transmit = connection.GetTransmitSink();
                pcm.Position = 0;
                await pcm.CopyToAsync(transmit);
                connection.Disconnect();
            }

            await pcm.DisposeAsync();

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Ringed"));

            await Task.Delay(1000);

            await ctx.DeleteResponseAsync();
        }
    }
}
