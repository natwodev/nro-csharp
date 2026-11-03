# nro-csharp

C# (.NET 10) server ported from `SRC_6` (AWN_Version, Java). The documentation, the plan and the checker live in the `port-csharp` folder next to this repo.

| Project | Role |
|---|---|
| `Nro.Core` | constants, templates, `Util`, `Message`, Java-style big-endian read/write |
| `Nro.Data` | PostgreSQL (Npgsql), DAOs, player JSON |
| `Nro.Network` | session, packet framing, XOR encryption, handshake |
| `Nro.Game` | player, map, mob, skill, inventory, services |
| `Nro.Content` | boss, npc, combine, shop, minigame, dungeon instances, events, bots |
| `Nro.Host` | entry point, configuration, tick loop |
| `Nro.Tests` | unit, golden, architecture |

Dependency rule: Core ← Data/Network ← Game ← Content ← Host (enforced by a test).

Build and test: use the checker, because it limits itself to 2 workers and does not freeze the machine:

```
python ../port-csharp/tools/check_port.py build
python ../port-csharp/tools/check_port.py test
```

## Folder structure

Same conventions as the reference solution `backend_manage` (folders per kind of code, interfaces in an `Interfaces` folder), kept on top of the 6 layered projects.

```
nro-csharp/
├── Nro.sln, global.json, Directory.Build.props, nuget.config   solution-wide settings (one place, every project shares them)
├── docs/                      technical docs (PROTOCOL.md: client-server protocol specification)
├── src/
│   ├── Nro.Core/              depends on no other project
│   │   ├── Consts/            constants (Cmd_message, ConstNpc, ...)
│   │   ├── Templates/         template classes (ItemTemplate, MobTemplate, ...)
│   │   ├── IO/                Java-style binary reading/writing, Message
│   │   ├── Interfaces/        IMessage
│   │   ├── Exceptions/        UtfDataFormatException
│   │   └── Helpers/           JavaIoChecks
│   ├── Nro.Data/              PostgreSQL access
│   │   ├── Configurations/    NroDatabaseOptions
│   │   ├── Data/              NroDatabase, NroTransaction, DbArguments, ResultSnapshot (replace ConnectDB / CrisResultSet)
│   │   ├── Interfaces/        INroSqlExecutor, IResultSnapshot
│   │   ├── Exceptions/        SqlPlaceholderMismatchException
│   │   ├── Helpers/           SqlPlaceholders (JDBC `?` translation), JavaStringFormat, TableNameResolver
│   │   └── Entities/          GENERATED EF Core style entities for all 66 tables (do not edit by hand)
│   │       ├── Runtime/       state written while the server runs (Player, Account, Clan, ...)
│   │       ├── StaticData/    tables loaded once at start-up; class names end with Row (ItemTemplateRow, ...)
│   │       ├── Web/           tables only the PHP site uses
│   │       ├── Logs/          append-only logs and mini-game history
│   │       └── Unused/        tables nothing reads or writes (kept so the whole schema is mapped)
│   ├── Nro.Network/           packets on the wire
│   │   ├── Consts/            Cmd_message (288 command numbers, generated from the Java class)
│   │   ├── Framing/           WireCipher, FrameWriter, FrameReader, WireCommands
│   │   ├── Handshake/         KeyPacket (the -27 key packet)
│   │   ├── Configurations/    GameServerOptions (port, per-IP limit, socket buffers), CommandDispatcherOptions
│   │   ├── Interfaces/        ISession, IMessageHandler, ISessionAcceptHandler, ISessionManager, IIpConnectionLimiter, ICommandHandler, ICharacterGate
│   │   ├── Services/          GameServer (TCP listener), SessionManager, IpConnectionLimiter, CommandDispatcher, SubCommandHandler, CommandRules
│   │   └── Sessions/          Session (one client connection: read, write and dispatch loops), SessionOptions
│   ├── Nro.Game/  Nro.Content/  Nro.Host/   (empty so far, filled by the waves of the plan)
└── tests/
    ├── Nro.Tests/             one test project, folders per area: Consts, IO, Templates, Db, Entities, Network
    └── golden/                reference data generated from the original Java code (do not edit by hand)
```

Where new code goes (same names as the reference solution): `Configurations/` setup and options, `Data/` database access, `Entities/` table classes,
`Exceptions/` custom exceptions, `Extensions/` extension methods, `Helpers/` small utilities, `Messages/` queue/game messages, `Repositories/` data access per
aggregate with its `Repositories/Interfaces/`, `Services/` business logic with its `Services/Interfaces/` (feature subfolders inside `Services/`, as in the reference).

Notes:
- Subfolders only organize files; the namespace stays at project level (`Nro.Data`, `Nro.Core.IO`), except `Interfaces` (`Nro.Data.Interfaces`, `Nro.Core.Interfaces`) and `Entities` (`Nro.Data.Entities`).
- Entities are generated with `python ../port-csharp/tools/entities/gen_entities.py` (add `--check` to verify they were not edited by hand). They follow the house format: one class per file, data annotations (`[Key]`, `[Required]`, `[MaxLength]`, `[ForeignKey]`; `[Table]` and `[Column("name")]` only where the snake_case naming convention would give a different name, `[Column(TypeName = ...)]` where the SQL type matters), a short VIETNAMESE comment above every property (the house format of the reference entities; the only Vietnamese prose left in the repo besides game data), navigation properties for the 15 high-confidence relationships. Property comments come from `port-csharp/tools/entities/comments/vi_short/*.json`.
- JSON/array text columns are plain `string` marked `[JsonText]` (every character is preserved).
- The data layer maps entities by a snake_case naming convention (`AccountId` to `account_id`); renamed properties such as the primary key `AccountId` to column `id` carry an explicit `[Column]`.
