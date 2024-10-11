using Microsoft.Extensions.DependencyInjection;
using W8_assignment_template.Data;
using W8_assignment_template.Helpers;
using W8_assignment_template.Interfaces;
using W8_assignment_template.Models.Characters;
using W8_assignment_template.Models.Rooms;
using W8_assignment_template.Services;

namespace W8_assignment_template;

internal class Program
{
    private static void ConfigureServices(IServiceCollection services)
    {
        // Register for DI
        services.AddTransient<GameEngine>();
        services.AddTransient<MenuManager>();
        services.AddTransient<MapManager>();
        services.AddSingleton<OutputManager>();

        services.AddTransient<IRoom, Room>();
        services.AddTransient<IRoomFactory, RoomFactory>();
        services.AddSingleton<IContext, DataContext>();
        
    }

    private static void Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var gameEngine = serviceProvider.GetService<GameEngine>();
        gameEngine?.Run();
    }
}


