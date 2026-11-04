namespace Nro.Game.Effects.Services;

// Cung cấp thao tác bật, tắt hiệu ứng kỹ năng có thời hạn và gửi gói tương ứng, thay singleton gI() Java.
// Nguồn Java: nro/effect/EffectSkillService.java (tên phương thức C# viết hoa chữ đầu của Java, ví dụ SetCumber từ setiscumber).
public interface IEffectSkillService
{
    // ---- packets -------------------------------------------------------------------------------------------------------------------------------------------------

    // Gửi gói -45/8 ra zone trừ người chơi dùng kỹ năng (Java: sendEffectUseSkill).
    void SendEffectUseSkill(Player player, sbyte skillId);

    // Gửi gói -124 với hiệu ứng 35 lên người chơi (Java: effcha).
    void SendSelfEffect35(Player player);

    // Gửi gói -124 hiệu ứng lên mục tiêu cho zone của người dùng kỹ năng (Java: sendEffectPlayer).
    void SendEffectPlayer(Player plUseSkill, Player plTarget, sbyte toggle, int effect);

    // Bật hiệu ứng trên người chơi (Java: sendEffPlayer).
    void SendSelfEffect(Player player, int effect);

    // Gửi gói -124 hiệu ứng cho quái (Java: sendEffectMob).
    void SendEffectMob(Player plUseSkill, IEffectMobTarget mobTarget, sbyte toggle, sbyte effect);

    // Gửi gói -45/0 liệt kê quái và người chơi bị choáng (Java: sendEffectBlindThaiDuongHaSan).
    void SendSolarFlareBlindEffect(Player plUseSkill, IReadOnlyList<Player> targetPlayers, IReadOnlyList<IEffectMobTarget> mobs, int timeStun);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: sendEffectStartCharge).
    void SendEffectStartCharge(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: sendEffectCharge).
    void SendEffectCharge(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: sendEffectStopCharge).
    void SendEffectStopCharge(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: sendEffectEndCharge).
    void SendEffectEndCharge(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: sendEffectMonkey).
    void SendEffectMonkey(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: SendTransformationOrItemEffect).
    void SendTransformationOrItemEffect(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: sendEffectTranformation (sic)).
    void SendTransformationEffect(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: sendEffectVolution (sic)).
    void SendEvolutionEffect(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: sendMobToSocola).
    void TransformMobToChocolate(Player player, IEffectMobTarget mob, int timeSocola);

    // ---- simple timed flags --------------------------------------------------------------------------------------------------------------------------------------

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setBiNgo).
    void SetPumpkin(Player player, int time);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeBiNgo).
    void RemovePumpkin(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setBongTuyet).
    void SetSnowflake(Player player, int time);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeBongTuyet).
    void RemoveSnowflake(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setVirus).
    void SetVirus(Player player, int time);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeVirus).
    void RemoveVirus(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setMabuHold).
    void SetMabuHold(Player player, MaBuHold mabuHold);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeMabuHold).
    void RemoveMabuHold(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setPKSTT).
    void SetPvpSuperDivineWater(Player player, int time);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removePKSTT).
    void RemovePvpSuperDivineWater(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setPKCommeson).
    void SetPvpCommeson(Player player, int time);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removePKCommeson).
    void RemovePvpCommeson(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setDameBuff).
    void SetDamageBuff(Player player, int time, int tiLe);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeDameBuff).
    void RemoveDamageBuff(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setXinbato).
    void SetXinbato(Player player, int time);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeXinbato).
    void RemoveXinbato(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setFideDaiCa).
    void SetFideLeader(Player player, int time);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeFideDaiCa).
    void RemoveFideLeader(Player player);

    // Bật Halloween, nhưng bỏ qua người có chỉ số kháng bí ngô (Java: setIsHalloween).
    void SetIsHalloween(Player player, int outFit, int time);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeHalloween).
    void RemoveHalloween(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setChibi).
    void SetMiniature(Player player, int time);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeChibi).
    void RemoveMiniature(Player player);

    // ---- Mafuba, intrinsic, stone, body change, invisibility ----------------------------------------------------------------------------------------------------

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setIsBinh).
    void SetBottled(Player plAtt, Player player, int time);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: RemoveBottled).
    void RemoveBottled(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: startUseMafuba).
    void StartUseMafuba(Player player, int time);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: finishUseMafuba).
    void FinishUseMafuba(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setIntrinsic).
    void SetIntrinsic(Player player, int skillId, int cooldown, long lastTimeUseSkill);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: releaseCooldownSkillByIntrinsic).
    void ReleaseCooldownSkillByIntrinsic(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setIntrinsicVip).
    void SetIntrinsicVip(Player player, int skillId, int cooldown, long lastTimeUseSkill);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: releaseCooldownSkillByIntrinsicVip).
    void ReleaseCooldownSkillByIntrinsicVip(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setIsStone).
    void SetIsStone(Player player, int time);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeStone).
    void RemoveStone(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setIsBodyChangeTechnique).
    void SetIsBodyChangeTechnique(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeBodyChangeTechnique).
    void RemoveBodyChangeTechnique(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setIsTanHinh).
    void SetInvisible(Player player, int time);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeTanHinh).
    void RemoveInvisibility(Player player);

    // ---- monkey ----------------------------------------------------------------------------------------------------------------------------------------------------

    // Thực hiện thao tác hiệu ứng tương ứng (Java: startUseSkillMonkey).
    void StartUseSkillMonkey(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: finishUseMonkey).
    void FinishUseMonkey(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setIsMonkey).
    void SetIsMonkey(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: monkeyDown).
    void MonkeyDown(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setIsMonkeyTrungThu WITHOUT its 200 ms sleep (see )).
    void SetMoonFestivalMonkey(Player player);

    // Chờ 200 mili giây qua scheduler rồi bật khỉ Trung Thu (Java: setIsMonkeyTrungThu).
    Task SetMoonFestivalMonkeyAsync(Player player, CancellationToken cancellationToken = default);

    // ---- bind, hypnosis, stun, chocolate, teleport blind, whistle, charge, shield -------------------------------------------------------------------------------

    // Cho người trói buông trói, kể cả trói của phân thân (Java: removeUseTroi).
    void RemoveBinding(Player player);

    // Giải phóng người chơi đang bị trói (Java: removeAnTroi).
    void RemoveBound(Player player);

    // Đặt trạng thái bị trói; tham số thời gian không được Java dùng (Java: setAnTroi).
    void SetBound(Player player, Player plTroi, long lastTimeAnTroi, int timeAnTroi);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setUseTroi).
    void SetBinding(Player player, long lastTimeTroi, int timeTroi);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setThoiMien).
    void SetHypnosis(Player player, long lastTimeThoiMien, int timeThoiMien);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeThoiMien).
    void RemoveHypnosis(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: startStun).
    void StartStun(Player player, long lastTimeStartBlind, int timeBlind);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeStun).
    void RemoveStun(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setSocola).
    void SetChocolate(Player player, long lastTimeSocola, int timeSocola);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeSocola).
    void RemoveChocolate(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setBlindDCTT).
    void SetTeleportBlind(Player player, long lastTimeDCTT, int timeBlindDCTT);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeBlindDCTT).
    void RemoveTeleportBlind(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setStartHuytSao).
    void StartWhistle(Player player, int tiLeHP);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeHuytSao).
    void RemoveWhistle(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: startCharge).
    void StartCharge(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: stopCharge).
    void StopCharge(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setStartShield).
    void SetStartShield(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeShield).
    void RemoveShield(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: breakShield).
    void BreakShield(Player player);

    // ---- transformations (Cumber, Pain, Kefla, Duoi Khi, Tranformation, Evolution) -----------------------------------------------------------------------------

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setiscumber WITHOUT its 1 s sleep (see )).
    void SetCumber(Player player);

    // Chờ 1 giây qua scheduler rồi bật Cumber (Java: setiscumber).
    Task SetCumberAsync(Player player, CancellationToken cancellationToken = default);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: cumberdown).
    void RemoveCumber(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setiscumber2 WITHOUT its 1 s sleep).
    void SetCumber2(Player player);

    // Chờ 1 giây qua scheduler rồi bật Cumber cấp 2 (Java: setiscumber2).
    Task SetCumber2Async(Player player, CancellationToken cancellationToken = default);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: cumberdown2).
    void RemoveCumber2(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setispain WITHOUT its 1 s sleep).
    void SetPain(Player player);

    // Chờ 1 giây qua scheduler rồi bật Pain (Java: setispain).
    Task SetPainAsync(Player player, CancellationToken cancellationToken = default);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: paindown).
    void RemovePain(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setiskefla WITHOUT its 1 s sleep).
    void SetKefla(Player player);

    // Chờ 1 giây qua scheduler rồi bật Kefla (Java: setiskefla).
    Task SetKeflaAsync(Player player, CancellationToken cancellationToken = default);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: kefladown).
    void RemoveKefla(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setHasMonkeyTail WITHOUT its 2 s sleep).
    void SetMonkeyTail(Player player);

    // Chờ 2 giây qua scheduler rồi bật Đuôi Khỉ (Java: setHasMonkeyTail).
    Task SetMonkeyTailAsync(Player player, CancellationToken cancellationToken = default);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: RemoveMonkeyTail).
    void RemoveMonkeyTail(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setIsTranformation (sic)).
    void SetTransformation(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: RemoveTransformation (sic)).
    void RemoveTransformation(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setIsEvolution).
    void SetEvolution(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: RemoveEvolution).
    void RemoveEvolution(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: setStartSucManhBocPha).
    void StartPowerBurst(Player player);

    // Thực hiện thao tác hiệu ứng tương ứng (Java: removeSucManhBocPha).
    void RemovePowerBurst(Player player);
}
