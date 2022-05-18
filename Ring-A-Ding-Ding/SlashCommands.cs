﻿using System.Diagnostics;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus.SlashCommands;

namespace RingADingDing
{
    [SlashCommandGroup("General", "Typical commands")]
    public class SlashCommands : ApplicationCommandModule
    {
        private static List<DiscordGuild> activeGuilds = new List<DiscordGuild>();

        [SlashCommand("Ring", "Ring the bong")]
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
                    try
                    {
                        DiscordChannel voiceChannel = voiceChannels[i];

                        if (!ctx.Guild.CurrentMember.PermissionsIn(voiceChannel).HasPermission(Permissions.UseVoice)) continue;
                        VoiceNextConnection connection = await voiceChannel.ConnectAsync();

                        if (connection == null) continue;
                        VoiceTransmitSink transmit = connection.GetTransmitSink();
                        pcm.Position = 0;
                        await pcm.CopyToAsync(transmit);
                        connection.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
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
