using Microsoft.AspNetCore.SignalR.Client;

Console.Write("Enter Player ID: ");

var playerId = Console.ReadLine();

if (string.IsNullOrWhiteSpace(playerId))
{
    Console.WriteLine("Player ID is required.");
    return;
}

var connection = new HubConnectionBuilder()
    .WithUrl($"http://localhost:5002/hubs/notifications?playerId={playerId}")
    .WithAutomaticReconnect()
    .Build();

connection.On<GameCreatedEvent>(
    "GameCreated",
    gameCreatedEvent =>
    {
        Console.WriteLine();
        Console.WriteLine("=================================");
        Console.WriteLine("GAME CREATED!");
        Console.WriteLine($"Game ID: {gameCreatedEvent.GameId}");
        Console.WriteLine(
            $"Players: {gameCreatedEvent.Player1Id} vs {gameCreatedEvent.Player2Id}");
        Console.WriteLine($"Status: {gameCreatedEvent.Status}");
        Console.WriteLine("=================================");
        Console.WriteLine();
    });

await connection.StartAsync();

Console.WriteLine();
Console.WriteLine($"Connected as player: {playerId}");
Console.WriteLine("Waiting for GameCreated events...");
Console.WriteLine("Press ENTER to exit.");

Console.ReadLine();

await connection.StopAsync();

public class GameCreatedEvent
{
    public Guid GameId { get; set; }

    public string Player1Id { get; set; } = default!;

    public string Player2Id { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = default!;
}