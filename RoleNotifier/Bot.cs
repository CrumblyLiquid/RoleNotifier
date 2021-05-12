using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace RoleNotifier
{
    class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;

        public class Config {
            public String token { get; set; }
            public Dictionary<String, String> data { get; set; }
        }

        private static Config GetConfig() {
            using (StreamReader reader = new StreamReader("config.json"))
            {
                string json = reader.ReadToEnd();
                Config conf = JsonConvert.DeserializeObject<Config>(json);
                return conf;
            }
        }

        private Config _config = GetConfig();

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, _config.token);
            await _client.StartAsync();

            _client.GuildMemberUpdated += GuildMemberUpdated;

            await Task.Delay(-1);
        }

        private async Task GuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            // Get the new role
            // I don't have better solution yet
            List<SocketRole> broles = new List<SocketRole>();
            foreach (SocketRole role in before.Roles) {
                broles.Add(role);
            }
            List<SocketRole> aroles = new List<SocketRole>();
            foreach (SocketRole role in after.Roles) {
                aroles.Add(role);
            }

            foreach (SocketRole role in aroles) {
                if (!broles.Contains(role)) {
                    // Send the message
                    SocketTextChannel channel = after.Guild.GetTextChannel(Convert.ToUInt64(_config.data.GetValueOrDefault(role.Id.ToString())));
                    await channel.SendMessageAsync($"{after.Mention} has joined, please welcome him to the chat!");
                    break;
                }
            }
        }
    }
}
