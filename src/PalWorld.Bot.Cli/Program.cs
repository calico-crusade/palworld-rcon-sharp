using CardboardBox.Discord;
using PalWorld.Bot.Core;
using PalWorld.Bot.Commands;
using PalWorld.Bot.Database;

var bot = DiscordBotBuilder.Start()
    .WithServices(c => c.AddPalWorldCore()
         .AddDatabase()
         .AddBotServices())
    .WithSlashCommands(c => c.AddBotCommands())
    .Build();

await bot.Login();

bot.Background<IBackgroundHandler>(t => t.StartAll(), out _);

await Task.Delay(-1);