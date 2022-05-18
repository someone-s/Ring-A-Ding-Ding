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
            Process? ffprobe = Process.Start(new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $@"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 ""{filePath}""",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            if (ffmpeg is null || ffprobe is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error"));
                return;
            }

            DiscordChannel[] voiceChannels = ctx.Guild.Channels.Where(c => c.Value.Type == ChannelType.Voice).Select(c => c.Value).ToArray();

            Stream pcm = ffmpeg.StandardOutput.BaseStream;
            float durationS = float.Parse(ffprobe.StandardOutput.ReadToEnd());


            for (int i = 0; i < voiceChannels.Length; i++)
            {
                DiscordChannel voiceChannel = voiceChannels[i];
                VoiceNextConnection connection = await voiceChannel.ConnectAsync();
                if (connection == null) continue;

                VoiceTransmitSink transmit = connection.GetTransmitSink();
                await pcm.CopyToAsync(transmit);
                await Task.Delay((int)(durationS * 1000f));
                connection.Disconnect();
            }

            await pcm.DisposeAsync();

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Ringed"));
        }
    }
}
