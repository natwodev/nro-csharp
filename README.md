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

Feature code is grouped by capability. Interfaces implemented in the same project sit beside their implementation; contracts implemented by another project live in that feature's `Ports/` folder. Foundation projects retain their own layouts where shown below.

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

Where new code goes: `Db/` database access, contracts, options, and database exceptions; `Entities/` table classes;
`Repositories/<feature>/` data access plus its local models and implementation-owned interfaces; `Extensions/` extension methods,
`Messages/` queue/game messages, and `Services/` business logic with implementation-owned interfaces beside the implementation and external contracts in the feature's `Ports/` folder.

Notes:
- Namespaces mirror project folders (for example `Nro.Data.Db`, `Nro.Data.Repositories.Players`, and `Nro.Data.Entities.Runtime`).
- Entities are generated with `python ../port-csharp/tools/entities/gen_entities.py` (add `--check` to verify they were not edited by hand). They follow the house format: one class per file, data annotations (`[Key]`, `[Required]`, `[MaxLength]`, `[ForeignKey]`, `[Column("name")]` only for non-key renamed properties, `[Column(TypeName = ...)]` where the SQL type matters), a short VIETNAMESE comment above every property (the house format of the reference entities; the only Vietnamese prose left in the repo besides game data), navigation properties for the 15 high-confidence relationships. The PostgreSQL table name is snake_case of the class name after removing a trailing `Row`, so generated entities never use `[Table]`. Property comments come from `port-csharp/tools/entities/comments/vi_short/*.json`.
- JSON/array text columns are plain `string` marked `[JsonText]` (every character is preserved).
- The data layer maps entities by a snake_case naming convention (`AccountId` to `account_id`); a single primary key named `<Entity>Id` conventionally maps to `id` without `[Column("id")]`. Other renamed properties, such as `GiftCodeRedemption.AccountId`, retain an explicit `[Column]`.
