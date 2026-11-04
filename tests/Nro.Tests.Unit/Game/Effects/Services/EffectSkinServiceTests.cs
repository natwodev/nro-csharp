using Nro.Game;
using static Nro.Tests.Unit.Game.Effects.Pkt;

namespace Nro.Tests.Unit.Game.Effects.Services;

public class EffectSkinServiceTests
{
    [Fact]
    public void SetDraburaFrost_stores_the_state_and_sends_overlay_effect_and_points_in_order()
    {
        var rig = new EffectRig();
        Player p = rig.Add(4);
        rig.Env.Skins.SetDraburaFrost(p, 123456, 5000);

        Assert.True(p.EffectSkin.IsDraburaFrost);
        Assert.Equal(123456, p.EffectSkin.LastTimeDraburaFrost);
        Assert.Equal(5000, p.EffectSkin.TimeDraburaFrost);
        Assert.Equal(2, rig.World.Packets.Count);
        Assert.Equal(-128, rig.World.Packets[0].Command);
        Assert.Equal(-124, rig.World.Packets[1].Command);
        Assert.Equal(Of(B(1), B(0), B(42), 4, 4), rig.World.Packets[1].Data);
        Assert.Equal(["Point:4"], rig.World.Calls);
    }

    [Fact]
    public void RemoveDraburaFrost_clears_the_flag_and_refreshes_the_costume()
    {
        var rig = new EffectRig();
        Player p = rig.Add(4);
        p.EffectSkin.IsDraburaFrost = true;
        rig.Env.Skins.RemoveDraburaFrost(p);

        Assert.False(p.EffectSkin.IsDraburaFrost);
        Assert.Equal(["Point:4", "Caitrang:4"], rig.World.Calls);
        Assert.Equal(-128, rig.World.Packets[0].Command);
        Assert.Equal("Player:4", rig.World.Packets[0].Scope);
        Assert.Equal("All", rig.World.Packets[1].Scope);
        Assert.Equal(Of(B(0), B(0), B(42), 4, 4), rig.World.Packets[2].Data);
    }

    [Fact]
    public void Slow_and_socola_set_and_remove()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skins.SetSlow(p, 10, 3000);
        Assert.True(p.EffectSkin.IsSlow);
        Assert.Equal(10, p.EffectSkin.LastTimeSlow);
        Assert.Equal(3000, p.EffectSkin.TimeSlow);
        rig.Env.Skins.RemoveSlow(p);
        Assert.False(p.EffectSkin.IsSlow);

        rig.Env.Skins.SetChocolate(p, 20, 5000);
        Assert.True(p.EffectSkin.IsChocolate);
        Assert.Equal(20, p.EffectSkin.LastTimeChocolate);
        Assert.Equal(5000, p.EffectSkin.TimeChocolate);
        rig.Env.Skins.RemoveChocolate(p);
        Assert.False(p.EffectSkin.IsChocolate);

        Assert.Equal(["Point:1", "Point:1", "Point:1", "Point:1", "Caitrang:1"], rig.World.Calls);
    }

    [Fact]
    public void SetRabbitLeader_is_ignored_when_the_player_is_a_carrot_himself()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1, configure: s => s.NPoint = s.NPoint with { IsRabbitLeader = true });
        rig.Env.Skins.SetRabbitLeader(p, 10, 300_000);
        Assert.False(p.EffectSkin.IsRabbitLeader);
        Assert.Empty(rig.World.Calls);
    }

    [Fact]
    public void SetRabbitLeader_and_remove_for_a_normal_player()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skins.SetRabbitLeader(p, 10, 300_000);
        Assert.True(p.EffectSkin.IsRabbitLeader);
        Assert.Equal(10, p.EffectSkin.LastTimeRabbitLeader);
        Assert.Equal(300_000, p.EffectSkin.TimeRabbitLeader);
        rig.Env.Skins.RemoveRabbitLeader(p);
        Assert.False(p.EffectSkin.IsRabbitLeader);
        Assert.Equal(["Point:1", "Point:1", "Caitrang:1"], rig.World.Calls);
    }

    [Fact]
    public void Demon_slayer_effects_set_and_remove()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        EffectSkin skin = p.EffectSkin;

        rig.Env.Skins.SetRage(p, 11, 1000);
        Assert.True(skin.IsNezuko);
        Assert.Equal((11L, 1000), (skin.LastTimeNezuko, skin.TimeNezuko));
        rig.Env.Skins.RemoveRage(p);
        Assert.False(skin.IsNezuko);

        rig.Env.Skins.SetInosuke(p, 12, 2000);
        Assert.True(skin.IsInosuke);
        Assert.Equal((12L, 2000), (skin.LastTimeInosuke, skin.TimeInosuke));
        rig.Env.Skins.RemoveInosuke(p);
        Assert.False(skin.IsInosuke);

        rig.Env.Skins.SetTanjiro(p, 13, 3000);
        Assert.True(skin.IsTanjiro);
        Assert.Equal((13L, 3000), (skin.LastTimeTanjiro, skin.TimeTanjiro));
        rig.Env.Skins.RemoveTanjiro(p);
        Assert.False(skin.IsTanjiro);

        rig.Env.Skins.SetZenitsu(p, 14, 4000);
        Assert.True(skin.IsZenitsu);
        Assert.Equal((14L, 4000), (skin.LastTimeZenitsu, skin.TimeZenitsu));
        rig.Env.Skins.RemoveZenitsu(p);
        Assert.False(skin.IsZenitsu);

        rig.Env.Skins.SetInosukeHashibira(p, 15, 5000);
        Assert.True(skin.IsInoHashi);
        Assert.Equal((15L, 5000), (skin.LastTimeInoHashi, skin.TimeInoHashi));
        rig.Env.Skins.RemoveInosukeHashibira(p);
        Assert.False(skin.IsInoHashi);

        Assert.Equal(10, rig.World.Count("Point:1"));
    }

    [Fact]
    public void A_second_set_overwrites_the_first_instead_of_stacking()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skins.SetSlow(p, 100, 3000);
        rig.Env.Skins.SetSlow(p, 900, 1000);
        Assert.True(p.EffectSkin.IsSlow);
        Assert.Equal((900L, 1000), (p.EffectSkin.LastTimeSlow, p.EffectSkin.TimeSlow));
    }

    [Fact]
    public void SendEffectPlayer_writes_effect_target_and_user_for_on_and_off()
    {
        var rig = new EffectRig();
        Player user = rig.Add(10);
        Player target = rig.Add(20);
        rig.Env.Skins.SendEffectPlayer(user, target, EffectSkinService.TURN_ON_EFFECT, EffectSkinService.STONE_EFFECT);
        rig.Env.Skins.SendEffectPlayer(user, target, EffectSkinService.TURN_OFF_EFFECT, EffectSkinService.BLIND_EFFECT);
        Assert.Equal(Of(B(1), B(0), B(42), 20, 10), rig.World.Packets[0].Data);
        Assert.Equal(Of(B(0), B(0), B(40), 20, 10), rig.World.Packets[1].Data);
        Assert.All(rig.World.Packets, p => Assert.Equal("ZoneOf:10", p.Scope));
    }

    [Fact]
    public void SendEffectPlayer_turn_off_all_writes_only_the_target()
    {
        var rig = new EffectRig();
        Player user = rig.Add(10);
        Player target = rig.Add(20);
        rig.Env.Skins.SendEffectPlayer(user, target, EffectSkinService.TURN_OFF_ALL_EFFECT, EffectSkinService.SLEEP_EFFECT);
        Assert.Equal(Of(B(2), B(0), 20), Assert.Single(rig.World.Packets).Data);
    }

    [Fact]
    public void SendEffectPlayer_errors_are_logged_not_thrown()
    {
        var rig = new EffectRig();
        Player user = rig.Add(10);
        rig.Env.Skins.SendEffectPlayer(user, null!, EffectSkinService.TURN_ON_EFFECT, 1);
        Assert.Single(rig.Errors);
        Assert.Empty(rig.World.Packets);
    }

    [Fact]
    public void Constants_have_the_Java_values()
    {
        Assert.Equal(1, EffectSkinService.TURN_ON_EFFECT);
        Assert.Equal(0, EffectSkinService.TURN_OFF_EFFECT);
        Assert.Equal(2, EffectSkinService.TURN_OFF_ALL_EFFECT);
        Assert.Equal(40, EffectSkinService.BLIND_EFFECT);
        Assert.Equal(41, EffectSkinService.SLEEP_EFFECT);
        Assert.Equal(42, EffectSkinService.STONE_EFFECT);
    }
}
