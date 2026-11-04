using Microsoft.Extensions.Logging;
using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.IO;
using Nro.Game;
using Nro.Tests.Unit.Helpers;
using Nro.Tests.Unit.Core.Logging;

namespace Nro.Tests.Unit.Game.Effects;

/// <summary>An option of a <see cref="FakeItem"/>.</summary>
internal sealed class FakeOption(int optionId, int param) : IEffectItemOption
{
    public int OptionId { get; } = optionId;

    public int Param { get; set; } = param;
}

/// <summary>An item for the inventory port.</summary>
internal sealed class FakeItem : IEffectItem
{
    public bool IsNotNullItem { get; set; } = true;

    public int TemplateId { get; set; }

    public List<FakeOption?> OptionList { get; set; } = [];

    /// <summary>True when the Java <c>itemOptions</c> field is null.</summary>
    public bool NoOptionList { get; set; }

    public bool TrainingArmor { get; set; }

    public IReadOnlyList<IEffectItemOption?>? Options => NoOptionList ? null : OptionList.Cast<IEffectItemOption?>().ToList();

    public static FakeItem Empty() => new() { IsNotNullItem = false, NoOptionList = true };
}

/// <summary>A mob for the mob port; records what the effects do to it.</summary>
internal sealed class FakeMob : IEffectMobTarget
{
    public int Id { get; set; } = 1;

    public long Hp { get; set; } = 1000;

    public long HpFull { get; set; } = 1000;

    public int X { get; set; }

    public int Y { get; set; }

    public List<(long Player, double Damage, bool DieWhenHpFull)> InjuredCalls { get; } = [];

    public int RemoveBindCalls { get; private set; }

    public List<(long Player, long LastTime, int Time)> SetBottleCalls { get; } = [];

    public List<(long LastTime, int Time)> SetChocolateCalls { get; } = [];

    public void Injured(Player? attacker, double damage, bool dieWhenHpFull)
    {
        InjuredCalls.Add((attacker?.Id ?? -1, damage, dieWhenHpFull));
        Hp = Math.Max(0, Hp - (long)damage);
    }

    public void RemoveBind() => RemoveBindCalls++;

    public void SetBottle(Player attacker, long bottleStartTime, int bottleDuration) => SetBottleCalls.Add((attacker.Id, bottleStartTime, bottleDuration));

    public void SetChocolate(long lastChocolateTime, int chocolateDuration) => SetChocolateCalls.Add((lastChocolateTime, chocolateDuration));
}

/// <summary>A packet captured by the fake broadcaster: where it went, its command and its bytes.</summary>
internal sealed record SentPacket(string Scope, sbyte Command, byte[] Data);

/// <summary>
/// One object that plays every port of the effect classes (player model, inventory, rules, client commands, packets, scheduler), keeps a state per player and records every call as
/// a text line in <see cref="Calls"/>, so a test can assert both the state change and the exact order of the side effects.
/// </summary>
internal sealed class FakeWorld : IEffectPlayerAccess, IEffectInventoryAccess, IEffectRules, IEffectClientNotifier, IEffectBroadcaster, IEffectScheduler
{
    /// <summary>The simulated model of one player.</summary>
    public sealed class Sim
    {
        public EffectNPointState NPoint { get; set; } = new() { Hp = 1000, HpMax = 1000, Mp = 500, MpMax = 500 };

        public EffectSetClothesState Clothes { get; set; } = new();

        public EffectNewSkillState NewSkill { get; set; } = new();

        public int X { get; set; }

        public int Y { get; set; }

        public int ZoneId { get; set; } = 1;

        public bool HasZone { get; set; } = true;

        public int MapId { get; set; } = 5;

        public bool IsPl { get; set; } = true;

        public bool FaceMaskItemTime { get; set; }

        public bool HasFrostSkin { get; set; }

        public bool HasBongTuyet { get; set; }

        public bool HasFaceMaskAvatar { get; set; }

        public Dictionary<int, EffectSkillRef> Skills { get; } = [];

        public EffectSkillRef? Selected { get; set; }

        public Dictionary<int, FakeItem> Body { get; } = [];

        public FakeItem? TrainArmorWorn { get; set; }

        public List<FakeItem?> Bag { get; } = [];

        public List<FakeItem?> Box { get; } = [];

        public bool DuoiKhi { get; set; }

        public long MonkeyTailStartTime { get; set; }
    }

    private readonly Dictionary<Player, Sim> sims = [];
    private readonly List<(TaskCompletionSource Source, TimeSpan Delay)> delays = [];

    /// <summary>Every command and query side effect, one text line each, in call order.</summary>
    public List<string> Calls { get; } = [];

    /// <summary>Every packet handed to the broadcaster.</summary>
    public List<SentPacket> Packets { get; } = [];

    /// <summary>The mobs of the (single) zone.</summary>
    public List<FakeMob> Mobs { get; } = [];

    /// <summary>The maps that <see cref="IsMapOffline"/> answers true for.</summary>
    public HashSet<int> OfflineMaps { get; } = [];

    /// <summary>Called at the start of every <see cref="Chat"/>; a test uses it to change the zone while an effect is iterating it.</summary>
    public Action<Player, string>? OnChat { get; set; }

    /// <summary>Players whose <see cref="IEffectPlayerAccess.SaveIsBienHinh"/> was called, with the value of <c>Isbienhinh</c> at that moment.</summary>
    public List<(long Player, int Value)> SavedBienHinh { get; } = [];

    /// <summary>Every <see cref="IEffectPlayerAccess.Injure"/> call: victim id and the damage exactly as passed.</summary>
    public List<(long Victim, double Damage)> InjureCalls { get; } = [];

    /// <summary>The amounts passed to <see cref="IEffectPlayerAccess.AddHp"/> and <see cref="IEffectPlayerAccess.AddMp"/>.</summary>
    public List<long> AddHpAmounts { get; } = [];

    public List<long> AddMpAmounts { get; } = [];

    /// <summary>The delays the code asked the scheduler for and has not got an answer to yet.</summary>
    public IReadOnlyList<TimeSpan> PendingDelays => delays.Select(d => d.Delay).ToList();

    public Sim SimOf(Player player) => sims[player];

    public Player Add(EffectEnvironment? env, long id, int x = 0, int y = 0, Action<Sim>? configure = null, Action<Player>? configurePlayer = null)
    {
        var player = new Player { Id = id, Name = "P" + id };
        var sim = new Sim { X = x, Y = y };
        configure?.Invoke(sim);
        configurePlayer?.Invoke(player);
        sims[player] = sim;
        if (env is not null)
        {
            Attach(env, player);
        }

        return player;
    }

    /// <summary>Registers a player created by the test (for example a <c>PhanThan</c> clone) with the world.</summary>
    public Player Adopt(EffectEnvironment env, Player player, Action<Sim>? configure = null)
    {
        var sim = new Sim();
        configure?.Invoke(sim);
        sims[player] = sim;
        Attach(env, player);
        return player;
    }

    public static void Attach(EffectEnvironment env, Player player)
    {
        player.EffectSkill.Environment = env;
        player.EffectSkin.Environment = env;
    }

    public void CompleteAllDelays()
    {
        foreach ((TaskCompletionSource source, _) in delays.ToList())
        {
            source.TrySetResult();
        }

        delays.Clear();
    }

    public int Count(string prefix) => Calls.Count(c => c.StartsWith(prefix, StringComparison.Ordinal));

    // ---- IEffectPlayerAccess -------------------------------------------------------------------------------------------------------------------------------------

    public bool IsDie(Player player) => sims[player].NPoint.Hp <= 0;

    public bool IsPl(Player player) => sims[player].IsPl;

    public bool HasZone(Player player) => sims[player].HasZone;

    public EffectLocation GetLocation(Player player) => new(sims[player].X, sims[player].Y);

    public int GetMapId(Player player)
        => sims[player].HasZone ? sims[player].MapId : throw new InvalidOperationException("no zone");

    public IReadOnlyList<Player> GetNotBosses(Player player)
    {
        Sim own = sims[player];
        if (!own.HasZone)
        {
            throw new InvalidOperationException("no zone");
        }

        return sims.Where(kv => kv.Value.HasZone && kv.Value.ZoneId == own.ZoneId && !kv.Key.IsBoss).Select(kv => kv.Key).ToList();
    }

    public IReadOnlyList<Player> GetPlayersInZone(Player player) => GetNotBosses(player);

    public IReadOnlyList<IEffectMobTarget> GetMobs(Player player) => Mobs.Cast<IEffectMobTarget>().ToList();

    public EffectNPointState GetNPoint(Player player) => sims[player].NPoint;

    public EffectSetClothesState GetSetClothes(Player player) => sims[player].Clothes;

    public EffectNewSkillState GetNewSkill(Player player) => sims[player].NewSkill;

    public EffectSkillRef? GetSkillById(Player player, int skillTemplateId)
        => sims[player].Skills.TryGetValue(skillTemplateId, out EffectSkillRef skill) ? skill : null;

    public EffectSkillRef? GetSelectedSkill(Player player) => sims[player].Selected;

    public bool HasFaceMaskItemTime(Player player) => sims[player].FaceMaskItemTime;

    public void CalPoint(Player player) => Calls.Add("CalPoint:" + player.Id);

    public void SetHp(Player player, long hp)
    {
        Calls.Add($"SetHp:{player.Id}:{hp}");
        sims[player].NPoint = sims[player].NPoint with { Hp = Math.Max(0, hp) };
    }

    public void HalveHp(Player player)
    {
        Calls.Add("HalveHp:" + player.Id);
        sims[player].NPoint = sims[player].NPoint with { Hp = sims[player].NPoint.Hp / 2 };
    }

    public void AddHp(Player player, long hp)
    {
        Calls.Add($"AddHp:{player.Id}:{hp}");
        AddHpAmounts.Add(hp);
        EffectNPointState n = sims[player].NPoint;
        sims[player].NPoint = n with { Hp = Math.Min(n.HpMax, n.Hp + hp) };
    }

    public void AddMp(Player player, long mp)
    {
        Calls.Add($"AddMp:{player.Id}:{mp}");
        AddMpAmounts.Add(mp);
        EffectNPointState n = sims[player].NPoint;
        sims[player].NPoint = n with { Mp = Math.Min(n.MpMax, n.Mp + mp) };
    }

    public void StartDuoiKhi(Player player, long timeDuoiKhi)
    {
        Calls.Add($"StartDuoiKhi:{player.Id}:{timeDuoiKhi}");
        sims[player].DuoiKhi = true;
        sims[player].MonkeyTailStartTime = timeDuoiKhi;
    }

    public void StopDuoiKhi(Player player)
    {
        Calls.Add("StopDuoiKhi:" + player.Id);
        sims[player].DuoiKhi = false;
    }

    public void SaveIsBienHinh(Player player) => SavedBienHinh.Add((player.Id, player.Isbienhinh));

    public void Injure(Player victim, Player? attacker, double damage, bool piercing, bool isMobAttack)
    {
        Calls.Add($"Injure:{victim.Id}:{attacker?.Id.ToString() ?? "-"}:{damage}:{piercing}:{isMobAttack}");
        InjureCalls.Add((victim.Id, damage));
        EffectNPointState n = sims[victim].NPoint;
        sims[victim].NPoint = n with { Hp = Math.Max(0, n.Hp - (long)damage) };
    }

    public (short X, short Y) GetMabuHoldPosition(MaBuHold hold) => (MabuHoldX, MabuHoldY);

    public short MabuHoldX { get; set; } = 111;

    public short MabuHoldY { get; set; } = 222;

    // ---- IEffectInventoryAccess ----------------------------------------------------------------------------------------------------------------------------------

    public bool HasDraburaFrostCostume(Player player) => sims[player].HasFrostSkin;

    public bool HasSnowflake(Player player) => sims[player].HasBongTuyet;

    public bool HasFaceMaskAvatar(Player player) => sims[player].HasFaceMaskAvatar;

    public IEffectItem GetBodyItem(Player player, int index)
        => sims[player].Body.TryGetValue(index, out FakeItem? item) ? item : FakeItem.Empty();

    public IEffectItem? GetTrainArmor(Player player) => sims[player].TrainArmorWorn;

    public IReadOnlyList<IEffectItem?> GetBagItems(Player player) => sims[player].Bag.Cast<IEffectItem?>().ToList();

    public IReadOnlyList<IEffectItem?> GetBoxItems(Player player) => sims[player].Box.Cast<IEffectItem?>().ToList();

    public bool IsTrainArmor(IEffectItem item) => item is FakeItem { TrainingArmor: true };

    // ---- IEffectRules (the formulas are written out again here, from SkillUtil.java and MapService.java, so a wrong rule in the code under test shows up) -----------------

    public bool IsMapOffline(int mapId) => OfflineMaps.Contains(mapId);

    public bool IsMapBlackBallWar(int mapId) => mapId >= 85 && mapId <= 91;

    public bool IsMapMaBu12H(int mapId) => mapId >= 114 && mapId <= 120;

    public int GetTimeMonkey(int level) => (level + 5) * 10000;

    public int GetTimeShield(int level) => (level + 2) * 5000;

    public int GetTimeTranformation() => 300000;

    public int GetTimett() => 300000;

    public int GetTimeSucManhBocPha(int level) => (level + 5) * 5000;

    // ---- IEffectClientNotifier -----------------------------------------------------------------------------------------------------------------------------------

    public void Point(Player player) => Calls.Add("Point:" + player.Id);

    public void SendCostume(Player player) => Calls.Add("Caitrang:" + player.Id);

    public void SendInfoNv(Player player) => Calls.Add("InfoNv:" + player.Id);

    public void SendInfoHp(Player player) => Calls.Add("InfoHp:" + player.Id);

    public void SendInfoHpMp(Player player) => Calls.Add("InfoHpMp:" + player.Id);

    public void SendInfoHpMpMoney(Player player) => Calls.Add("InfoHpMpMoney:" + player.Id);

    public void Chat(Player player, string text)
    {
        OnChat?.Invoke(player, text);
        Calls.Add($"Chat:{player.Id}:{text}");
    }

    public void SendNotification(Player player, string text) => Calls.Add($"Notice:{player.Id}:{text}");

    public void SendSpeedPlayer(Player player, int speed) => Calls.Add($"Speed:{player.Id}:{speed}");

    public void SetNotMonkey(Player player) => Calls.Add("NotMonkey:" + player.Id);

    public void SetNotTranformation(Player player) => Calls.Add("NotTranformation:" + player.Id);

    public void SetNotVolution(Player player) => Calls.Add("NotVolution:" + player.Id);

    public void SendInfoPlayerEatPea(Player player) => Calls.Add("EatPea:" + player.Id);

    public void SetPos(Player player, int x, int y) => Calls.Add($"SetPos:{player.Id}:{x}:{y}");

    public void SetPos2(Player player, int x, int y) => Calls.Add($"SetPos2:{player.Id}:{x}:{y}");

    public void SendChibi(Player player) => Calls.Add("Chibi:" + player.Id);

    public void SendHaveChibiFollowToAllMap(Player player) => Calls.Add("ChibiFollow:" + player.Id);

    public void SendMabuHold(Player player, int action, short x, short y) => Calls.Add($"MabuHold:{player.Id}:{action}:{x}:{y}");

    public void PlayerInfoUpdate(Player player, Player receiver, string name, int head, int body, int leg)
        => Calls.Add($"InfoUpdate:{player.Id}:{receiver.Id}:{name}:{head}:{body}:{leg}");

    public void ChangeAndSendTypePk(Player player, int type) => Calls.Add($"TypePk:{player.Id}:{type}");

    public void ReleaseCooldownSkill(Player player, EffectSkillRef? skill)
        => Calls.Add($"ReleaseCooldown:{player.Id}:{(skill is null ? "none" : skill.Value.SkillId.ToString())}");

    public void RecoverHpMp(Player player, long hp, long mp) => Calls.Add($"Recover:{player.Id}:{hp}:{mp}");

    public void SendItemTime(Player player, int itemId, int seconds) => Calls.Add($"ItemTime:{player.Id}:{itemId}:{seconds}");

    public void RemoveItemTime(Player player, int itemId) => Calls.Add($"RemoveItemTime:{player.Id}:{itemId}");

    public void SendAllItemTime(Player player) => Calls.Add("AllItemTime:" + player.Id);

    public void SendItemBody(Player player) => Calls.Add("ItemBody:" + player.Id);

    public void SendItemBag(Player player) => Calls.Add("ItemBag:" + player.Id);

    // ---- IEffectBroadcaster --------------------------------------------------------------------------------------------------------------------------------------

    public void SendToPlayer(Player player, Message message) => Packets.Add(new SentPacket("Player:" + player.Id, message.Command, message.GetData()));

    public void SendToZoneOf(Player player, Message message) => Packets.Add(new SentPacket("ZoneOf:" + player.Id, message.Command, message.GetData()));

    public void SendToZone(Zone zone, Message message) => Packets.Add(new SentPacket("Zone", message.Command, message.GetData()));

    public void SendToZoneOfMob(IEffectMobTarget mob, Message message) => Packets.Add(new SentPacket("ZoneOfMob:" + mob.Id, message.Command, message.GetData()));

    public void SendToZoneExcept(Player player, Message message) => Packets.Add(new SentPacket("Except:" + player.Id, message.Command, message.GetData()));

    public void SendToAll(Message message) => Packets.Add(new SentPacket("All", message.Command, message.GetData()));

    // ---- IEffectScheduler ----------------------------------------------------------------------------------------------------------------------------------------

    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        var source = new TaskCompletionSource();
        cancellationToken.Register(() => source.TrySetCanceled(cancellationToken));
        delays.Add((source, delay));
        return source.Task;
    }
}

/// <summary>Everything a test of the effect classes needs: the fake world, a fake clock, a scripted random source, a recording logger and the wired environment.</summary>
internal sealed class EffectRig
{
    public static readonly DateTimeOffset Start = new(2026, 11, 3, 10, 0, 0, TimeSpan.FromHours(7));

    public EffectRig(bool readInt = false)
    {
        Time = new ManualTimeProvider(Start);
        Clock = new VietnamClock(Time);
        Random = new ScriptedRandomProvider();
        Log = new RecordingLogger();
        World = new FakeWorld();
        Settings = new EffectSettings { ReadInt = readInt };
        Env = EffectEnvironment.Create(Clock, new RandomUtil(Random), new GameLogger(Log), Settings, World, World, World, World, World, World);
    }

    public ManualTimeProvider Time { get; }

    public VietnamClock Clock { get; }

    public ScriptedRandomProvider Random { get; }

    public RecordingLogger Log { get; }

    public FakeWorld World { get; }

    public EffectSettings Settings { get; }

    public EffectEnvironment Env { get; }

    public long Now => Clock.CurrentTimeMillis();

    public void Advance(long milliseconds) => Time.Advance(TimeSpan.FromMilliseconds(milliseconds));

    public Player Add(long id, int x = 0, int y = 0, Action<FakeWorld.Sim>? configure = null, Action<Player>? configurePlayer = null)
        => World.Add(Env, id, x, y, configure, configurePlayer);

    public IReadOnlyList<RecordedEntry> Errors => Log.Entries.Where(e => e.Level == LogLevel.Error).ToList();
}

/// <summary>Builds the expected bytes of a packet from typed parts: a <see cref="byte"/> is one byte, a <see cref="short"/> two, an <see cref="int"/> four, all big-endian (independent of the code under test).</summary>
internal static class Pkt
{
    public static byte B(int value) => unchecked((byte)value);

    public static short S(int value) => unchecked((short)value);

    public static byte[] Of(params object[] parts)
    {
        var bytes = new List<byte>();
        foreach (object part in parts)
        {
            switch (part)
            {
                case byte b:
                    bytes.Add(b);
                    break;
                case short s:
                    bytes.Add((byte)(s >> 8));
                    bytes.Add((byte)s);
                    break;
                case int i:
                    bytes.Add((byte)(i >> 24));
                    bytes.Add((byte)(i >> 16));
                    bytes.Add((byte)(i >> 8));
                    bytes.Add((byte)i);
                    break;
                default:
                    throw new ArgumentException("unsupported part " + part.GetType());
            }
        }

        return [.. bytes];
    }
}
