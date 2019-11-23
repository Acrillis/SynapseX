using System;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace Synapse_Bot
{
    /*
    *
    *	SYNAPSE BOT
    *	File.:	Init.cs
    *	Desc.:	Setup and connect the bot to Discord.
    *
    */


    class Init
    {
        // global declarations //

        public CommandService commands;
        public DiscordSocketClient client;
        public IServiceProvider services;
        public char prefix = '!';

        static void Main(string[] args) => new Init().MainAsync().GetAwaiter().GetResult();


        // main entry point, just running asynchronous ;3

        public async Task MainAsync()
        {
            client = new DiscordSocketClient();
            commands = new CommandService();

            client.Log += Log;

            services = new ServiceCollection().BuildServiceProvider();

            await InstallCommands();

            string token = "NDk5MzY1OTUwNjM0ODUyMzYx.Dp7OZw.-2M-y3t3kgRxkvozjKhzvYvN5gE"; //token for now
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1); // keep her up and runnin'!
        }


        // links all command modules to the command handler

        public async Task InstallCommands()
        {
            client.MessageReceived += HandleCommand;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }


        // command handler

        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;
            if (!(message.HasCharPrefix(prefix, ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;
            var context = new CommandContext(client, message);
            if (context.Channel.Id != 499366306147991566) return; // if not dev-bot, end it!
            var result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess) Console.WriteLine(result.ErrorReason); // create a better logging method later
        }


        // basic logging method, should be improved for more advanced formatting later

        public Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
