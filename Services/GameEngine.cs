using W8_assignment_template.Data;
using W8_assignment_template.Helpers;
using W8_assignment_template.Interfaces;
using W8_assignment_template.Models.Characters;

namespace W8_assignment_template.Services;

public class GameEngine
{
    private readonly IContext _context;
    private readonly MapManager _mapManager;
    private readonly MenuManager _menuManager;
    private readonly OutputManager _outputManager;
    
    private readonly IRoomFactory _roomFactory;
    private ICharacter _player;
    private ICharacter _goblin;
    private ICharacter _troll;
    private ICharacter _ogre;


    private List<IRoom> _rooms;

    public GameEngine(IContext context, IRoomFactory roomFactory, MenuManager menuManager, MapManager mapManager, OutputManager outputManager)
    {
        _roomFactory = roomFactory;
        _menuManager = menuManager;
        _mapManager = mapManager;
        _outputManager = outputManager;
        _context = context;
    }

    public void Run()
    {
        if (_menuManager.ShowMainMenu())
        {
            SetupGame();
        }
    }

    private void AttackCharacter()
    {
        // TODO Update this method to allow for attacking a selected monster in the room.
        // TODO e.g. "Which monster would you like to attack?"
        // TODO Right now it just attacks the first monster in the room.
        // TODO It is ok to leave this functionality if there is only one monster in the room.


        //Console.WriteLine("What monster would you like to attack");
        //var attackChoice = Console.ReadLine();
        //Console.WriteLine(attackChoice);
        int monsterCount = _player.CurrentRoom.Characters.Count();

        while (monsterCount != 0)
        {
            
            Console.WriteLine("Which monster would you like to attack?");

            foreach (var character in _player.CurrentRoom.Characters)
            {
                Console.WriteLine($"{character.Name}");
            }

            var attackChoice = Console.ReadLine();
            attackChoice = attackChoice.ToLower();
            
            var target = _player.CurrentRoom.Characters.FirstOrDefault(c => c.Name.ToLower() == attackChoice);

            if ( target != null)
            {
                _player.Attack(target);
                _player.CurrentRoom.RemoveCharacter(target);
            }
            else
            {
                Console.WriteLine("No characters in the room match match that name", ConsoleColor.Red);
            }
            monsterCount = _player.CurrentRoom.Characters.Count();
        }
    }

    private void GameLoop()
    {
        while (true)
        {
            _mapManager.DisplayMap();
            _outputManager.WriteLine("Choose an action:", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Move North");
            _outputManager.WriteLine("2. Move South");
            _outputManager.WriteLine("3. Move East");
            _outputManager.WriteLine("4. Move West");

            // Check if there are characters in the current room to attack
            if (_player.CurrentRoom.Characters.Any(c => c != _player))
            {
                _outputManager.WriteLine("5. Attack");
            }

            _outputManager.WriteLine("6. Exit Game");

            _outputManager.Display();

            var input = Console.ReadLine();

            string? direction = null;
            switch (input)
            {
                case "1":
                    direction = "north";
                    break;
                case "2":
                    direction = "south";
                    break;
                case "3":
                    direction = "east";
                    break;
                case "4":
                    direction = "west";
                    break;
                case "5":
                    if (_player.CurrentRoom.Characters.Any(c => c != _player))
                    {
                        AttackCharacter();
                    }
                    else
                    {
                        _outputManager.WriteLine("No characters to attack.", ConsoleColor.Red);
                    }

                    break;
                case "6":
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    Environment.Exit(0);
                    break;
                default:
                    _outputManager.WriteLine("Invalid selection. Please choose a valid option.", ConsoleColor.Red);
                    break;
            }

            // Update map manager with the current room after movement
            if (!string.IsNullOrEmpty(direction))
            {
                _outputManager.Clear();
                _player.Move(direction);
                _mapManager.UpdateCurrentRoom(_player.CurrentRoom);
            }
        }
    }

    private void LoadMonsters()
    {
        _goblin = _context.Characters.OfType<Goblin>().FirstOrDefault();
        _troll = _context.Characters.OfType<Troll>().FirstOrDefault();
        _ogre = _context.Characters.OfType<Ogre>().FirstOrDefault();


        var random = new Random();
        var randomRoom = _rooms[random.Next(_rooms.Count)];
        randomRoom.AddCharacter(_goblin); // Use helper method

        // Load your two new monsters here into the same treasure room
        var treasureRoom = _rooms.FirstOrDefault(r => r.Name == "Treasure Room");
        treasureRoom.AddCharacter(_troll);
        treasureRoom.AddCharacter(_ogre);

    }

    private void SetupGame()
    {
        var startingRoom = SetupRooms();
        _mapManager.UpdateCurrentRoom(startingRoom);

        _player = _context.Characters.OfType<Player>().FirstOrDefault();
        _player.Move(startingRoom);
        _outputManager.WriteLine($"{_player.Name} has entered the game.", ConsoleColor.Green);

        // Load monsters into random rooms 
        LoadMonsters();

        // Pause for a second before starting the game loop
        Thread.Sleep(1000);
        GameLoop();
    }

    private IRoom SetupRooms()
    {
        //  Updated this method to create more rooms and connect them together

        var entrance = _roomFactory.CreateRoom("entrance", _outputManager);
        var treasureRoom = _roomFactory.CreateRoom("treasure", _outputManager);
        var dungeonRoom = _roomFactory.CreateRoom("dungeon", _outputManager);
        var library = _roomFactory.CreateRoom("library", _outputManager);
        var armory = _roomFactory.CreateRoom("armory", _outputManager);
        var garden = _roomFactory.CreateRoom("garden", _outputManager);
        var kitchen = _roomFactory.CreateRoom("kitchen", _outputManager);
        var diningHall = _roomFactory.CreateRoom("dining", _outputManager);


        entrance.North = diningHall;
        entrance.West = library;
        entrance.East = garden;

        //treasureRoom.South = diningHall;
        treasureRoom.West = dungeonRoom;

        diningHall.South = entrance;
        diningHall.West = kitchen;
        //diningHall.North = treasureRoom;

        dungeonRoom.East = treasureRoom;
        dungeonRoom.South = kitchen;

        kitchen.North = dungeonRoom;
        kitchen.South = library;
        kitchen.East = diningHall;

        library.East = entrance;
        library.South = armory;
        library.North = kitchen;

        armory.North = library;

        garden.West = entrance;

        // Store rooms in a list for later use
        _rooms = new List<IRoom> { entrance, treasureRoom, dungeonRoom, library, armory, garden, kitchen, diningHall };

        return entrance;
    }
}
