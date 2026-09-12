# MatchMaking

A microservices-based matchmaking system for a multiplayer game, built with **.NET 8**. Players join a rank-based matchmaking queue, get paired using an expanding-search algorithm, have a game session created for them automatically, and are notified in real time over **SignalR** when their match is ready.

Services communicate asynchronously through **RabbitMQ**, with **Redis** backing the live matchmaking queue and **PostgreSQL** persisting game sessions.

## Architecture

```
                 POST /api/matchmaking/join
                            │
                            ▼
                 ┌─────────────────────┐
                 │  MatchmakingService  │──── Redis (queue: sorted by JoinedAt)
                 └─────────┬────────────┘
                            │ publishes "match_found"
                            ▼
                       RabbitMQ
                            │
                            ▼
                 ┌─────────────────────┐
                 │  GameSessionService   │──── PostgreSQL (GameSessions table)
                 └─────────┬────────────┘
                            │ publishes "game_created"
                            ▼
                       RabbitMQ
                            │
                            ▼
                 ┌─────────────────────┐
                 │  NotificationService  │──── SignalR Hub (/hubs/notifications)
                 └─────────┬────────────┘
                            │ pushes "GameCreated" event
                            ▼
                     Connected clients
```

## Services

### MatchmakingService (port 5000)
Handles queue join/leave requests and periodically runs a background matchmaking worker.

- `POST /api/matchmaking/join` — adds a player (`PlayerId`, `Rank`) to the queue
- `POST /api/matchmaking/leave/{playerId}` — removes a player from the queue
- **`MatchmakingWorker`** (background service): every 10 seconds, scans the queue and pairs players whose rank difference is within an allowed threshold. The threshold starts at 100 and expands by 100 for every 30 seconds a player has waited, so players are matched more loosely the longer they wait.
- Queue state lives in **Redis** (a list for ordering + a set for fast membership checks).
- On a successful pairing, publishes a `MatchFoundEvent` to the `match_found` RabbitMQ queue and removes both players from the matchmaking queue.

### GameSessionService (port 5001)
Owns the lifecycle of game sessions.

- `GET /api/games` — list all game sessions
- `GET /api/games/{id}` — get a single game session by id
- **`MatchFoundConsumer`** (background service): listens on the `match_found` queue, and for each `MatchFoundEvent` creates a new `GameSession` row.
- Persists sessions to **PostgreSQL** via EF Core (`GameSessionDbContext`), with migrations applied automatically on startup.
- On creation, publishes a `GameCreatedEvent` to the `game_created` RabbitMQ queue.

### NotificationService (port 5002)
Delivers real-time notifications to players.

- **SignalR hub** at `/hubs/notifications` — clients connect with a `playerId` query string parameter, which is used as their SignalR user id (`PlayerIdUserIdProvider`).
- **`GameCreatedConsumer`** (background service): listens on the `game_created` queue and pushes a `GameCreated` event to the two matched players via the hub.

### Shared.Contracts
Common event contracts shared across services:
- `MatchFoundEvent` (`Player1Id`, `Player2Id`, `CreatedAt`)
- `GameCreatedEvent` (`GameId`, `Player1Id`, `Player2Id`, `CreatedAt`, `Status`)

### Shared.Infrastructure
Shared RabbitMQ messaging infrastructure: connection management (`RabbitMqConnection`), publishing (`RabbitMqPublisher` / `IRabbitMqPublisher`), and configuration options (`RabbitMqOptions`).

### SignalRTestClient
A small console app for manually testing the notification flow. Prompts for a player id, connects to the NotificationService hub, and prints incoming `GameCreated` events to the console.

## Infrastructure dependencies

| Component | Purpose | Default port |
|---|---|---|
| Redis | Matchmaking queue storage | 6379 |
| RabbitMQ | Event bus between services (`match_found`, `game_created` queues) | 5672 (AMQP), 15672 (management UI) |
| PostgreSQL | Game session persistence | 5432 |

## Running the project

The whole stack is defined in `docker-compose.yml` at the repo root:

```bash
docker compose up --build
```

This starts:

| Service | URL |
|---|---|
| Matchmaking API | http://localhost:5000 |
| Game Session API | http://localhost:5001 |
| Notification hub | http://localhost:5002/hubs/notifications |
| RabbitMQ management UI | http://localhost:15672 (guest/guest) |
| PostgreSQL | localhost:5432 (postgres/postgres, db: `matchmaking`) |
| Redis | localhost:6379 |

Swagger UI is available on the Matchmaking and Game Session services (e.g. `http://localhost:5000/swagger`).

### Trying it out end-to-end

1. Start the stack with `docker compose up --build`.
2. Run `SignalRTestClient` (e.g. `dotnet run --project SignalRTestClient`) and enter a player id — this opens a live connection to the notification hub.
3. Send a couple of `join` requests to the Matchmaking API for different player ids with ranks close enough to match:
   ```bash
   curl -X POST http://localhost:5000/api/matchmaking/join \
     -H "Content-Type: application/json" \
     -d '{"playerId": "player1", "rank": 1000}'

   curl -X POST http://localhost:5000/api/matchmaking/join \
     -H "Content-Type: application/json" \
     -d '{"playerId": "player2", "rank": 1050}'
   ```
4. Within ~10 seconds the `MatchmakingWorker` pairs them, `GameSessionService` creates a session, and the `SignalRTestClient` instance connected as `player1` (or `player2`) prints the `GameCreated` event.
5. Confirm the session was persisted: `curl http://localhost:5001/api/games`.

## Tech stack

- .NET 8 / ASP.NET Core Web API
- SignalR (real-time notifications)
- RabbitMQ (async messaging between services)
- Redis (StackExchange.Redis — matchmaking queue)
- PostgreSQL + EF Core (game session persistence)
- Docker / Docker Compose

## Notes / known limitations

- RabbitMQ queues are declared as non-durable and messages are auto-acked, so events can be lost if a consumer is down when they're published.
- The matchmaking worker scans the entire queue every 10 seconds rather than matching incrementally, which is simple but won't scale well to very large queues.
- No authentication/authorization is implemented; `playerId` is trusted as given by the caller.
