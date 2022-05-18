using System.Diagnostics;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus.SlashCommands;

namespace RingADingDing
{
    public class TimedStorage<T> : List<T>
    {
        private Dictionary<Wrapper, Task> timers = new Dictionary<Wrapper, Task>();

        public new void Add(T item)
        {
            base.Add(item);

            Wrapper key = new Wrapper { item = item };
            if (timers.ContainsKey(key))
                timers[key] = Timer(item);
            else
                timers.Add(key, Timer(item));
        }

        public new void Remove(T item)
        {
            if (Contains(item))
                base.Remove(item);

            Wrapper key = new Wrapper { item = item };
            if (timers.ContainsKey(key))
            {
                timers[key].Dispose();
                timers.Remove(key);
            }
        }

        public async Task Timer(T item)
        {
            await Task.Delay(30000);

            if (Contains(item))
                base.Remove(item);

            Wrapper key = new Wrapper { item = item };
            if (timers.ContainsKey(key))
                timers.Remove(key);
        }

        private struct Wrapper
        {
            public T item;
        }
    }

    public class SlashCommands : ApplicationCommandModule
    {
        private static TimedStorage<DiscordGuild> activeGuilds = new TimedStorage<DiscordGuild>();
        

        [SlashCommand("ring", "ring all channels")]
        public async Task Ring(InteractionContext ctx)
        {
            if (activeGuilds.Contains(ctx.Guild))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                   new DiscordInteractionResponseBuilder().WithContent("At Work"));

                await Task.Delay(1000);

                await ctx.DeleteResponseAsync();
            }
            else
            {
                activeGuilds.Add(ctx.Guild);

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

                    activeGuilds.Remove(ctx.Guild);

                    return;
                }

                DiscordChannel[] voiceChannels = ctx.Guild.Channels.Where(c => c.Value.Type == ChannelType.Voice).Select(c => c.Value).ToArray();

                Stream pcm = new MemoryStream();
                await ffmpeg.StandardOutput.BaseStream.CopyToAsync(pcm);

                for (int i = 0; i < voiceChannels.Length; i++)
                {
                    DiscordChannel voiceChannel = voiceChannels[i];

                    if (!ctx.Guild.CurrentMember.PermissionsIn(voiceChannel).HasPermission(Permissions.AccessChannels)) continue;
                    if (!ctx.Guild.CurrentMember.PermissionsIn(voiceChannel).HasPermission(Permissions.UseVoice)) continue;
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

                activeGuilds.Remove(ctx.Guild);
            }
        }
    }
}
