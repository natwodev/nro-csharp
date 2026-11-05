namespace Nro.Game.Services.Maps;

// Mã map nội bộ của MapService; không thuộc ConstMap vì Java không khai báo các trường này.
internal static class MapIds
{
    // Làng Trái Đất. (Java: MapService.isMapLang, dòng 475–477)
    public const int VILLAGE_EARTH = 0;
    // Làng Namếc. (Java: MapService.isMapLang, dòng 475–477)
    public const int VILLAGE_NAMEK = 7;
    // Làng Xayda. (Java: MapService.isMapLang, dòng 475–477)
    public const int VILLAGE_SAIYAN = 14;
    // Hành tinh Trái Đất. (Java: MapService.getPlanetName, dòng 348–359)
    public const sbyte PLANET_EARTH = 0;
    // Hành tinh Namếc. (Java: MapService.getPlanetName, dòng 348–359)
    public const sbyte PLANET_NAMEK = 1;
    // Hành tinh Xayda. (Java: MapService.getPlanetName, dòng 348–359)
    public const sbyte PLANET_SAIYAN = 2;
    // Map nhà đầu dải. (Java: MapService.isHome, dòng 471–473)
    public const int HOME_MAP_FIRST = 21;
    // Map nhà cuối dải. (Java: MapService.isHome, dòng 471–473)
    public const int HOME_MAP_LAST = 23;
    // Map nhà theo giới tính. (Java: MapService.getMapCapsule, dòng 373)
    public const int HOME_CAPSULE_FIRST = 21;
    // Map luyện tập Capsule. (Java: MapService.getMapCapsule, dòng 374)
    public const int TRAINING_CAPSULE_MAP = 47;
    // Map Capsule Trái Đất. (Java: MapService.getMapCapsule, dòng 378)
    public const int EARTH_CAPSULE_MAP = 0;
    // Map Capsule Namếc. (Java: MapService.getMapCapsule, dòng 379)
    public const int NAMEK_CAPSULE_MAP = 7;
    // Map Capsule Xayda. (Java: MapService.getMapCapsule, dòng 380)
    public const int SAIYAN_CAPSULE_MAP = 14;
    // Map Capsule kho báu. (Java: MapService.getMapCapsule, dòng 381)
    public const int TREASURE_CAPSULE_MAP = 5;
    // Map Capsule 20. (Java: MapService.getMapCapsule, dòng 382)
    public const int CAPSULE_MAP_20 = 20;
    // Map Capsule 13. (Java: MapService.getMapCapsule, dòng 383)
    public const int CAPSULE_MAP_13 = 13;
    // Map Capsule theo giới tính. (Java: MapService.getMapCapsule, dòng 384)
    public const int GENDER_CAPSULE_FIRST = 24;
    // Map Capsule 27. (Java: MapService.getMapCapsule, dòng 385)
    public const int CAPSULE_MAP_27 = 27;
    // Map Capsule 19. (Java: MapService.getMapCapsule, dòng 386)
    public const int CAPSULE_MAP_19 = 19;
    // Map Capsule 79. (Java: MapService.getMapCapsule, dòng 387)
    public const int CAPSULE_MAP_79 = 79;
    // Map Capsule yêu cầu sức mạnh cao. (Java: MapService.getMapCapsule, dòng 388–390)
    public const int HIGH_POWER_CAPSULE_MAP = 84;
    // Map Capsule yêu cầu cấp luyện tập cao. (Java: MapService.getMapCapsule, dòng 391–393)
    public const int HIGH_TRAINING_CAPSULE_MAP = 154;
    // Map Ngọc đen đầu dải. (Java: MapService.getMapBlackBall, dòng 397–403)
    public const int BLACK_BALL_WAR_FIRST = 85;
    // Map Ngọc đen cuối dải. (Java: MapService.getMapBlackBall, dòng 397–403)
    public const int BLACK_BALL_WAR_LAST = 91;
    // Map Mabư 12 giờ đầu dải. (Java: MapService.getMapMaBu, dòng 405–411)
    public const int MAJIN_BUU_TWELVE_HOUR_FIRST = 114;
    // Map Mabư 12 giờ cuối dải. (Java: MapService.getMapMaBu, dòng 405–411)
    public const int MAJIN_BUU_TWELVE_HOUR_LAST = 120;
    // Doanh trại đầu dải. (Java: MapService.isMapDoanhTrai, dòng 568–570)
    public const int BARRACKS_FIRST = 53;
    // Doanh trại cuối dải. (Java: MapService.isMapDoanhTrai, dòng 568–570)
    public const int BARRACKS_LAST = 62;
    // Kho báu đầu dải. (Java: MapService.isMapBanDoKhoBau, dòng 572–574)
    public const int TREASURE_UNDER_SEA_FIRST = 135;
    // Kho báu cuối dải. (Java: MapService.isMapBanDoKhoBau, dòng 572–574)
    public const int TREASURE_UNDER_SEA_LAST = 138;
    // Đường rắn đầu dải. (Java: MapService.isMapConDuongRanDoc, dòng 579–581)
    public const int SNAKE_WAY_FIRST = 141;
    // Đường rắn cuối dải. (Java: MapService.isMapConDuongRanDoc, dòng 579–581)
    public const int SNAKE_WAY_LAST = 144;
    // Khí gas đầu dải. (Java: MapService.isMapKhiGasHuyDiet, dòng 583–585)
    public const int DESTRON_GAS_FIRST = 147;
    // Map khí gas bị loại trừ. (Java: MapService.isMapKhiGasHuyDiet, dòng 583–585)
    public const int DESTRON_GAS_EXCLUDED = 150;
    // Khí gas cuối dải. (Java: MapService.isMapKhiGasHuyDiet, dòng 583–585)
    public const int DESTRON_GAS_LAST = 152;
    // Map khí gas không chuyển trực tiếp. (Java: MapService.shouldChangeMap, dòng 461–465)
    public const int DESTRON_GAS_TRANSITION_MAP = 148;
    // Map fallback đường rắn. (Java: MapService.getMapCanJoin, dòng 77–82)
    public const int SNAKE_WAY_FALLBACK_MAP = 48;
    // Map fallback boss bang hội. (Java: MapService.getMapCanJoin, dòng 84–89)
    public const int CLAN_BOSS_FALLBACK_MAP = 153;
    // Map boss bang hội. (Java: MapService.isMapBossBangHoi, dòng 587–589)
    public const int CLAN_BOSS_MAP = 165;
    // Tương lai đầu dải thứ nhất. (Java: MapService.isMapTuongLai, dòng 658–660)
    public const int FUTURE_FIRST = 92;
    // Tương lai cuối dải thứ nhất. (Java: MapService.isMapTuongLai, dòng 658–660)
    public const int FUTURE_EARLY_LAST = 94;
    // Tương lai đầu dải thứ hai. (Java: MapService.isMapTuongLai, dòng 658–660)
    public const int FUTURE_LATE_FIRST = 96;
    // Tương lai cuối dải thứ hai. (Java: MapService.isMapTuongLai, dòng 658–660)
    public const int FUTURE_LATE_LAST = 100;
    // Tương lai đầu cặp rời. (Java: MapService.isMapTuongLai, dòng 658–660)
    public const int FUTURE_SINGLE_FIRST = 102;
    // Tương lai cuối cặp rời. (Java: MapService.isMapTuongLai, dòng 658–660)
    public const int FUTURE_SINGLE_LAST = 103;
    // Map lạnh đầu dải. (Java: MapService.isMapCold, dòng 607–614)
    public const int COLD_FIRST = 105;
    // Map lạnh cuối dải. (Java: MapService.isMapCold, dòng 607–614)
    public const int COLD_LAST = 110;
    // Map khí gas cũng lạnh. (Java: MapService.isMapCold, dòng 607–614)
    public const int COLD_DESTRON_GAS_MAP = 152;
    // Thánh địa lạnh đầu dải. (Java: MapService.isMapCold, dòng 607–614)
    public const int COLD_SANCTUARY_FIRST = 158;
    // Thánh địa lạnh cuối dải. (Java: MapService.isMapCold, dòng 607–614)
    public const int COLD_SANCTUARY_LAST = 159;
    // Map 5.000 năm trước đầu dải. (Java: MapService.isMap5000NamTruoc, dòng 662–664)
    public const int FIVE_THOUSAND_YEARS_AGO_FIRST = 210;
    // Map 5.000 năm trước cuối dải. (Java: MapService.isMap5000NamTruoc, dòng 662–664)
    public const int FIVE_THOUSAND_YEARS_AGO_LAST = 248;
    // Map quá khứ bắt buộc của dump. (Java: MapService.getMapById, dòng 310–317)
    public const int REQUIRED_PAST_MAP = 205;
    // Map Hirudegarn. (Java: MapService.isMapHirudegarn, dòng 503–505)
    public const int HIRUDEGARN_MAP = 126;
    // Potara đầu dải. (Java: MapService.isMapPotara, dòng 28–30)
    public const int POTARA_FIRST = 189;
    // Potara cuối dải. (Java: MapService.isMapPotara, dòng 28–30)
    public const int POTARA_LAST = 192;
    // Map bình hút năng lượng. (Java: MapService.isMapBinhHutNangLuong, dòng 32–34)
    public const int ENERGY_ABSORPTION_BOTTLE_MAP = 180;
    // Map băng đầu dải. (Java: MapService.islanhdiabanghoi, dòng 650–652)
    public const int ICE_MAP_FIRST = 156;
    // Map băng cuối dải. (Java: MapService.islanhdiabanghoi, dòng 650–652)
    public const int ICE_MAP_LAST = 159;
    // Map thử sát thương đầu dải. (Java: MapService.isMapTestDame, dòng 479–481)
    public const int DAMAGE_TEST_FIRST = 169;
    // Map thử sát thương cuối dải. (Java: MapService.isMapTestDame, dòng 479–481)
    public const int DAMAGE_TEST_LAST = 171;
    // Hành tinh thực vật đầu dải. (Java: MapService.isMapHanhTinhThucVat, dòng 483–485)
    public const int PLANT_PLANET_FIRST = 160;
    // Hành tinh thực vật cuối dải. (Java: MapService.isMapHanhTinhThucVat, dòng 483–485)
    public const int PLANT_PLANET_LAST = 163;
    // Sự kiện Hùng Vương đầu dải. (Java: MapService.isMapHungVuongEvent, dòng 487–489)
    public const int HUNG_KING_EVENT_FIRST = 181;
    // Sự kiện Hùng Vương cuối dải. (Java: MapService.isMapHungVuongEvent, dòng 487–489)
    public const int HUNG_KING_EVENT_LAST = 184;
    // Map riêng đầu dải. (Java: MapService.isMapRiengTu, dòng 491–493)
    public const int PRIVATE_MAP_FIRST = 185;
    // Map riêng cuối dải. (Java: MapService.isMapRiengTu, dòng 491–493)
    public const int PRIVATE_MAP_LAST = 187;
    // Map tranh ngọc Namếc. (Java: MapService.isMapTranhNgocNamec, dòng 495–497)
    public const int NAMEK_JEWEL_WAR_MAP = 164;
    // Luyện tập đầu dải. (Java: MapService.isMapLuyenTap, dòng 499–501)
    public const int TRAINING_FIRST = 45;
    // Map luyện tập bị loại trừ. (Java: MapService.isMapLuyenTap, dòng 499–501)
    public const int TRAINING_EXCLUDED = 47;
    // Luyện tập cuối dải. (Java: MapService.isMapLuyenTap, dòng 499–501)
    public const int TRAINING_LAST = 50;
    // Map Granola. (Java: MapService.isMapgranola, dòng 507–509)
    public const int GRANOLA_MAP = 194;
    // Map Sơn Tinh. (Java: MapService.isMapsontinh, dòng 511–513)
    public const int SON_TINH_MAP = 207;
    // Map Thủy Tinh. (Java: MapService.isMapthuytinh, dòng 514–516)
    public const int THUY_TINH_MAP = 208;
    // Mabư 14 giờ đầu dải. (Java: MapService.isMapMabu14H, dòng 518–520)
    public const int MAJIN_BUU_FOURTEEN_HOUR_FIRST = 127;
    // Mabư 14 giờ cuối dải. (Java: MapService.isMapMabu14H, dòng 518–520)
    public const int MAJIN_BUU_FOURTEEN_HOUR_LAST = 128;
    // Map Đông Nam Karin. (Java: MapService.isMapDongNamKarin, dòng 522–524)
    public const int EAST_KARIN_MAP = 111;
    // Map Tây Karin. (Java: MapService.isMapTayKarin, dòng 525–527)
    public const int WEST_KARIN_MAP = 146;
    // Địa ngục thứ nhất. (Java: MapService.isMapDiaNguc, dòng 529–531)
    public const int HELL_FIRST = 167;
    // Địa ngục thứ hai. (Java: MapService.isMapDiaNguc, dòng 529–531)
    public const int HELL_SECOND = 168;
    // Địa ngục thứ ba. (Java: MapService.isMapDiaNguc, dòng 529–531)
    public const int HELL_THIRD = 172;
    // Địa ngục cuối. (Java: MapService.isMapDiaNguc, dòng 529–531)
    public const int HELL_LAST = 173;
    // Map Kong đấu Godzilla. (Java: MapService.isMapKongvsGodzila, dòng 541–543)
    public const int KONG_VS_GODZILLA_MAP = 181;
    // Map nâng sức khỏe thứ nhất. (Java: MapService.isMapUpSKH, dòng 533–535)
    public const int UP_HEALTH_FIRST = 1;
    // Map nâng sức khỏe thứ hai. (Java: MapService.isMapUpSKH, dòng 533–535)
    public const int UP_HEALTH_SECOND = 2;
    // Map nâng sức khỏe thứ ba. (Java: MapService.isMapUpSKH, dòng 533–535)
    public const int UP_HEALTH_THIRD = 3;
    // Map nâng sức khỏe thứ tư. (Java: MapService.isMapUpSKH, dòng 533–535)
    public const int UP_HEALTH_FOURTH = 8;
    // Map nâng sức khỏe thứ năm. (Java: MapService.isMapUpSKH, dòng 533–535)
    public const int UP_HEALTH_FIFTH = 9;
    // Map nâng sức khỏe thứ sáu. (Java: MapService.isMapUpSKH, dòng 533–535)
    public const int UP_HEALTH_SIXTH = 11;
    // Map nâng sức khỏe thứ bảy. (Java: MapService.isMapUpSKH, dòng 533–535)
    public const int UP_HEALTH_SEVENTH = 15;
    // Map nâng sức khỏe thứ tám. (Java: MapService.isMapUpSKH, dòng 533–535)
    public const int UP_HEALTH_EIGHTH = 16;
    // Map nâng sức khỏe cuối. (Java: MapService.isMapUpSKH, dòng 533–535)
    public const int UP_HEALTH_LAST = 17;
    // Quái bang hội thứ nhất. (Java: MapService.isMapMobBangHoi_Part1, dòng 537–539)
    public const int CLAN_MOB_FIRST = 27;
    // Quái bang hội thứ hai. (Java: MapService.isMapMobBangHoi_Part1, dòng 537–539)
    public const int CLAN_MOB_SECOND = 28;
    // Quái bang hội thứ ba. (Java: MapService.isMapMobBangHoi_Part1, dòng 537–539)
    public const int CLAN_MOB_THIRD = 31;
    // Quái bang hội thứ tư. (Java: MapService.isMapMobBangHoi_Part1, dòng 537–539)
    public const int CLAN_MOB_FOURTH = 32;
    // Quái bang hội thứ năm. (Java: MapService.isMapMobBangHoi_Part1, dòng 537–539)
    public const int CLAN_MOB_FIFTH = 35;
    // Quái bang hội cuối. (Java: MapService.isMapMobBangHoi_Part1, dòng 537–539)
    public const int CLAN_MOB_LAST = 36;
    // Sau làng Trái Đất. (Java: MapService.isMapSauLang, dòng 545–547)
    public const int BEHIND_VILLAGE_EARTH = 1;
    // Sau làng Namếc. (Java: MapService.isMapSauLang, dòng 545–547)
    public const int BEHIND_VILLAGE_NAMEK = 8;
    // Sau làng Xayda. (Java: MapService.isMapSauLang, dòng 545–547)
    public const int BEHIND_VILLAGE_SAIYAN = 15;
    // Yardrat đầu dải. (Java: MapService.isMapYardart, dòng 595–597)
    public const int YARDRAT_FIRST = 131;
    // Yardrat cuối dải. (Java: MapService.isMapYardart, dòng 595–597)
    public const int YARDRAT_LAST = 133;
    // Hành tinh ngục tù thứ nhất. (Java: MapService.isMapHanhTinhNgucTu, dòng 599–601)
    public const int PRISON_PLANET_FIRST = 155;
    // Hành tinh ngục tù thứ hai. (Java: MapService.isMapHanhTinhNgucTu, dòng 599–601)
    public const int PRISON_PLANET_SECOND = 206;
    // Map PvP. (Java: MapService.isMapPVP, dòng 603–605)
    public const int PVP_MAP = 112;
    // Giáng sinh đầu dải. (Java: MapService.isMapChristMasEvent, dòng 616–618)
    public const int CHRISTMAS_FIRST = 174;
    // Giáng sinh cuối dải. (Java: MapService.isMapChristMasEvent, dòng 616–618)
    public const int CHRISTMAS_LAST = 179;
    // Cereal đầu dải. (Java: MapService.isMapCereal, dòng 628–631)
    public const int CEREAL_FIRST = 194;
    // Cereal cuối dải. (Java: MapService.isMapCereal, dòng 628–631)
    public const int CEREAL_LAST = 201;
    // Map Cereal vô hình. (Java: MapService.isMapVohinh, dòng 633–636)
    public const int INVISIBLE_CEREAL_MAP = 201;
    // Potaufeu đầu dải. (Java: MapService.isMapPotaufeu, dòng 638–640)
    public const int POTAUFEU_FIRST = 139;
    // Potaufeu cuối dải. (Java: MapService.isMapPotaufeu, dòng 638–640)
    public const int POTAUFEU_LAST = 140;
    // Tân thủ đầu dải. (Java: MapService.isMapTanThu, dòng 642–644)
    public const int BEGINNER_FIRST = 0;
    // Tân thủ cuối dải. (Java: MapService.isMapTanThu, dòng 642–644)
    public const int BEGINNER_LAST = 38;
    // Ngũ hành đầu dải. (Java: MapService.isMapNguHanhSon, dòng 646–648)
    public const int FIVE_ELEMENTS_FIRST = 122;
    // Ngũ hành cuối dải. (Java: MapService.isMapNguHanhSon, dòng 646–648)
    public const int FIVE_ELEMENTS_LAST = 124;
    // Cứu Mị Nương đầu dải thứ nhất. (Java: MapService.isMapGiaiCuuMiNuong, dòng 654–656)
    public const int RESCUE_MI_NUONG_FIRST = 185;
    // Cứu Mị Nương cuối dải thứ nhất. (Java: MapService.isMapGiaiCuuMiNuong, dòng 654–656)
    public const int RESCUE_MI_NUONG_FIRST_LAST = 188;
    // Cứu Mị Nương đầu dải thứ hai. (Java: MapService.isMapGiaiCuuMiNuong, dòng 654–656)
    public const int RESCUE_MI_NUONG_SECOND = 207;
    // Cứu Mị Nương cuối dải thứ hai. (Java: MapService.isMapGiaiCuuMiNuong, dòng 654–656)
    public const int RESCUE_MI_NUONG_LAST = 209;
    // Map chiến tranh thứ nhất. (Java: MapService.isMapWar, dòng 666–668)
    public const int WAR_MAP_FIRST = 51;
    // Map chiến tranh thứ hai. (Java: MapService.isMapWar, dòng 666–668)
    public const int WAR_MAP_SECOND = 112;
    // Map chiến tranh thứ ba. (Java: MapService.isMapWar, dòng 666–668)
    public const int WAR_MAP_THIRD = 113;
    // Map chiến tranh cuối. (Java: MapService.isMapWar, dòng 666–668)
    public const int WAR_MAP_LAST = 129;
    // Gọi rồng Trái Đất đầu dải. (Java: MapService.isMapCallDragon, dòng 686–688)
    public const int CALL_DRAGON_EARTH_FIRST = 1;
    // Gọi rồng Trái Đất cuối dải. (Java: MapService.isMapCallDragon, dòng 686–688)
    public const int CALL_DRAGON_EARTH_LAST = 6;
    // Gọi rồng Namếc đầu dải. (Java: MapService.isMapCallDragon, dòng 686–688)
    public const int CALL_DRAGON_NAMEK_FIRST = 8;
    // Gọi rồng Namếc cuối dải. (Java: MapService.isMapCallDragon, dòng 686–688)
    public const int CALL_DRAGON_NAMEK_LAST = 13;
    // Gọi rồng Xayda đầu dải. (Java: MapService.isMapCallDragon, dòng 686–688)
    public const int CALL_DRAGON_SAIYAN_FIRST = 15;
    // Gọi rồng Xayda cuối dải. (Java: MapService.isMapCallDragon, dòng 686–688)
    public const int CALL_DRAGON_SAIYAN_LAST = 20;
    // Gọi rồng bang hội đầu dải. (Java: MapService.isMapCallDragon, dòng 686–688)
    public const int CALL_DRAGON_CLAN_FIRST = 27;
    // Gọi rồng bang hội cuối dải. (Java: MapService.isMapCallDragon, dòng 686–688)
    public const int CALL_DRAGON_CLAN_LAST = 38;
    // Số zone Ngọc đen. (Java: Map.initZone, dòng 114–145)
    public const int BLACK_BALL_WAR_ZONE_COUNT = 5;
    // Số zone Mabư 14 giờ. (Java: Map.initZone, dòng 114–145)
    public const int MAJIN_BUU_FOURTEEN_HOUR_ZONE_COUNT = 9;
    // Số zone Mabư 12 giờ. (Java: Map.initZone, dòng 114–145)
    public const int MAJIN_BUU_TWELVE_HOUR_ZONE_COUNT = 7;
    // Số zone phó bản bang hội. (Java: Map.initZone, dòng 114–145)
    public const int CLAN_DUNGEON_ZONE_COUNT = 50;
    // Số zone boss bang hội. (Java: Map.initZone, dòng 114–145)
    public const int CLAN_BOSS_ZONE_COUNT = 120;
}
