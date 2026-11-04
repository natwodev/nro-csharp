namespace Nro.Game.Items.Rewards;

// Bảng chỉ số gốc của initBaseOptionClothes: chọn năm cặp option-id/giá trị theo template và loại trang bị trước khi RewardService quay biến thiên ±10%.
// Nguồn Java: models/Reward/RewardService.java (lớp phụ của RewardService; 174 nhánh switch được chép cơ học để không gõ lại số, ô -1 là chưa dùng; type ngoài 0..4 hoặc template lạ giữ -1).
public static class BaseClothesOptions
{
    // Số ô option_param của Java; ô 0 là chỉ số chính, ô 1 là hồi phục hoặc giá trị phụ, ô 2 là yêu cầu sức mạnh đồ thần.
    public const int SlotCount = 5;

    // Trả mảng mới gồm năm cặp [optionId, value] cho template; type 0 áo, 1 quần, 2 găng, 3 giày, 4 rada/nhẫn.
    public static int[][] GetOptionParams(int tempId, int type)
    {
        int[][] optionParam = [[-1, -1], [-1, -1], [-1, -1], [-1, -1], [-1, -1]];
        switch (type) {
        case 0:
            optionParam[0][0] = 47;
            switch (tempId) {
                case 0:
                    optionParam[0][1] = 2;
                    break;
                case 33:
                    optionParam[0][1] = 4;
                    break;
                case 3:
                    optionParam[0][1] = 8;
                    break;
                case 34:
                    optionParam[0][1] = 16;
                    break;
                case 136:
                    optionParam[0][1] = 24;
                    break;
                case 137:
                    optionParam[0][1] = 40;
                    break;
                case 138:
                    optionParam[0][1] = 60;
                    break;
                case 139:
                    optionParam[0][1] = 90;
                    break;
                case 230:
                    optionParam[0][1] = 200;
                    break;
                case 231:
                    optionParam[0][1] = 250;
                    break;
                case 232:
                    optionParam[0][1] = 300;
                    break;
                case 233:
                    optionParam[0][1] = 400;
                    break;
                case 1:
                    optionParam[0][1] = 2;
                    break;
                case 41:
                    optionParam[0][1] = 4;
                    break;
                case 4:
                    optionParam[0][1] = 8;
                    break;
                case 42:
                    optionParam[0][1] = 16;
                    break;
                case 152:
                    optionParam[0][1] = 24;
                    break;
                case 153:
                    optionParam[0][1] = 40;
                    break;
                case 154:
                    optionParam[0][1] = 60;
                    break;
                case 155:
                    optionParam[0][1] = 90;
                    break;
                case 234:
                    optionParam[0][1] = 200;
                    break;
                case 235:
                    optionParam[0][1] = 250;
                    break;
                case 236:
                    optionParam[0][1] = 300;
                    break;
                case 237:
                    optionParam[0][1] = 400;
                    break;
                case 2:
                    optionParam[0][1] = 3;
                    break;
                case 49:
                    optionParam[0][1] = 5;
                    break;
                case 5:
                    optionParam[0][1] = 10;
                    break;
                case 50:
                    optionParam[0][1] = 20;
                    break;
                case 168:
                    optionParam[0][1] = 30;
                    break;
                case 169:
                    optionParam[0][1] = 50;
                    break;
                case 170:
                    optionParam[0][1] = 70;
                    break;
                case 171:
                    optionParam[0][1] = 100;
                    break;
                case 238:
                    optionParam[0][1] = 230;
                    break;
                case 239:
                    optionParam[0][1] = 280;
                    break;
                case 240:
                    optionParam[0][1] = 330;
                    break;
                case 241:
                    optionParam[0][1] = 450;
                    break;
                case 555:
                    optionParam[2][0] = 21;
                    optionParam[0][1] = 800;
                    optionParam[2][1] = 15;
                    break;
                case 557:
                    optionParam[2][0] = 21;
                    optionParam[0][1] = 800;
                    optionParam[2][1] = 15;
                    break;
                case 559:
                    optionParam[2][0] = 21;
                    optionParam[0][1] = 800;
                    optionParam[2][1] = 15;
                    break;
            }
            break;
        case 1:
            optionParam[0][0] = 6;
            optionParam[1][0] = 27;
            switch (tempId) {
                case 6:
                    optionParam[0][1] = 30;
                    break;
                case 35:
                    optionParam[0][1] = 150;
                    optionParam[1][1] = 12;
                    break;
                case 9:
                    optionParam[0][1] = 300;
                    optionParam[1][1] = 40;
                    break;
                case 36:
                    optionParam[0][1] = 600;
                    optionParam[1][1] = 120;
                    break;
                case 140:
                    optionParam[0][1] = 1400;
                    optionParam[1][1] = 280;
                    break;
                case 141:
                    optionParam[0][1] = 3000;
                    optionParam[1][1] = 600;
                    break;
                case 142:
                    optionParam[0][1] = 6000;
                    optionParam[1][1] = 1200;
                    break;
                case 143:
                    optionParam[0][1] = 10000;
                    optionParam[1][1] = 2000;
                    break;
                case 242:
                    optionParam[0][1] = 14000;
                    optionParam[1][1] = 2500;
                    break;
                case 243:
                    optionParam[0][1] = 18000;
                    optionParam[1][1] = 3000;
                    break;
                case 244:
                    optionParam[0][1] = 22000;
                    optionParam[1][1] = 3500;
                    break;
                case 245:
                    optionParam[0][1] = 26000;
                    optionParam[1][1] = 4000;
                    break;
                case 7:
                    optionParam[0][1] = 20;
                    break;
                case 43:
                    optionParam[0][1] = 25;
                    optionParam[1][1] = 10;
                    break;
                case 10:
                    optionParam[0][1] = 120;
                    optionParam[1][1] = 28;
                    break;
                case 44:
                    optionParam[0][1] = 250;
                    optionParam[1][1] = 100;
                    break;
                case 156:
                    optionParam[0][1] = 600;
                    optionParam[1][1] = 240;
                    break;
                case 157:
                    optionParam[0][1] = 1200;
                    optionParam[1][1] = 480;
                    break;
                case 158:
                    optionParam[0][1] = 2400;
                    optionParam[1][1] = 960;
                    break;
                case 159:
                    optionParam[0][1] = 4800;
                    optionParam[1][1] = 1800;
                    break;
                case 246:
                    optionParam[0][1] = 13000;
                    optionParam[1][1] = 2200;
                    break;
                case 247:
                    optionParam[0][1] = 17000;
                    optionParam[1][1] = 2700;
                    break;
                case 248:
                    optionParam[0][1] = 21000;
                    optionParam[1][1] = 3200;
                    break;
                case 249:
                    optionParam[0][1] = 25000;
                    optionParam[1][1] = 3700;
                    break;
                case 8:
                    optionParam[0][1] = 20;
                    break;
                case 51:
                    optionParam[0][1] = 20;
                    optionParam[1][1] = 8;
                    break;
                case 11:
                    optionParam[0][1] = 100;
                    optionParam[1][1] = 20;
                    break;
                case 52:
                    optionParam[0][1] = 200;
                    optionParam[1][1] = 80;
                    break;
                case 172:
                    optionParam[0][1] = 500;
                    optionParam[1][1] = 200;
                    break;
                case 173:
                    optionParam[0][1] = 1000;
                    optionParam[1][1] = 400;
                    break;
                case 174:
                    optionParam[0][1] = 2000;
                    optionParam[1][1] = 800;
                    break;
                case 175:
                    optionParam[0][1] = 4000;
                    optionParam[1][1] = 1600;
                    break;
                case 250:
                    optionParam[0][1] = 12000;
                    optionParam[1][1] = 2100;
                    break;
                case 251:
                    optionParam[0][1] = 16000;
                    optionParam[1][1] = 2600;
                    break;
                case 252:
                    optionParam[0][1] = 20000;
                    optionParam[1][1] = 3100;
                    break;
                case 253:
                    optionParam[0][1] = 24000;
                    optionParam[1][1] = 3600;
                    break;
                case 556:
                    optionParam[0][0] = 22;
                    optionParam[2][0] = 21;
                    optionParam[0][1] = 52;
                    optionParam[1][1] = 10000;
                    optionParam[2][1] = 15;
                    break;
                case 558:
                    optionParam[0][0] = 22;
                    optionParam[2][0] = 21;
                    optionParam[0][1] = 50;
                    optionParam[1][1] = 10000;
                    optionParam[2][1] = 15;
                    break;
                case 560:
                    optionParam[0][0] = 22;
                    optionParam[2][0] = 21;
                    optionParam[0][1] = 48;
                    optionParam[1][1] = 10000;
                    optionParam[2][1] = 15;
                    break;
            }
            break;
        case 2:
            optionParam[0][0] = 0;
            switch (tempId) {
                case 21:
                    optionParam[0][1] = 4;
                    break;
                case 24:
                    optionParam[0][1] = 7;
                    break;
                case 37:
                    optionParam[0][1] = 14;
                    break;
                case 38:
                    optionParam[0][1] = 28;
                    break;
                case 144:
                    optionParam[0][1] = 55;
                    break;
                case 145:
                    optionParam[0][1] = 110;
                    break;
                case 146:
                    optionParam[0][1] = 220;
                    break;
                case 147:
                    optionParam[0][1] = 530;
                    break;
                case 254:
                    optionParam[0][1] = 680;
                    break;
                case 255:
                    optionParam[0][1] = 1000;
                    break;
                case 256:
                    optionParam[0][1] = 1500;
                    break;
                case 257:
                    optionParam[0][1] = 2200;
                    break;
                case 22:
                    optionParam[0][1] = 3;
                    break;
                case 46:
                    optionParam[0][1] = 6;
                    break;
                case 25:
                    optionParam[0][1] = 12;
                    break;
                case 45:
                    optionParam[0][1] = 24;
                    break;
                case 160:
                    optionParam[0][1] = 50;
                    break;
                case 161:
                    optionParam[0][1] = 100;
                    break;
                case 162:
                    optionParam[0][1] = 200;
                    break;
                case 163:
                    optionParam[0][1] = 500;
                    break;
                case 258:
                    optionParam[0][1] = 630;
                    break;
                case 259:
                    optionParam[0][1] = 950;
                    break;
                case 260:
                    optionParam[0][1] = 1450;
                    break;
                case 261:
                    optionParam[0][1] = 2150;
                    break;
                case 23:
                    optionParam[0][1] = 5;
                    break;
                case 53:
                    optionParam[0][1] = 8;
                    break;
                case 26:
                    optionParam[0][1] = 16;
                    break;
                case 54:
                    optionParam[0][1] = 32;
                    break;
                case 176:
                    optionParam[0][1] = 60;
                    break;
                case 177:
                    optionParam[0][1] = 120;
                    break;
                case 178:
                    optionParam[0][1] = 240;
                    break;
                case 179:
                    optionParam[0][1] = 560;
                    break;
                case 262:
                    optionParam[0][1] = 700;
                    break;
                case 263:
                    optionParam[0][1] = 1050;
                    break;
                case 264:
                    optionParam[0][1] = 1550;
                    break;
                case 265:
                    optionParam[0][1] = 2250;
                    break;
                case 562:
                    optionParam[2][0] = 21;
                    optionParam[0][1] = 3700;
                    optionParam[2][1] = 17;
                    break;
                case 564:
                    optionParam[2][0] = 21;
                    optionParam[0][1] = 3500;
                    optionParam[2][1] = 17;
                    break;
                case 566:
                    optionParam[2][0] = 21;
                    optionParam[0][1] = 3800;
                    optionParam[2][1] = 17;
                    break;
            }
            break;
        case 3:
            optionParam[0][0] = 7;
            optionParam[1][0] = 28;
            switch (tempId) {
                case 27:
                    optionParam[0][1] = 10;
                    break;
                case 30:
                    optionParam[0][1] = 25;
                    optionParam[1][1] = 5;
                    break;
                case 39:
                    optionParam[0][1] = 120;
                    optionParam[1][1] = 24;
                    break;
                case 40:
                    optionParam[0][1] = 250;
                    optionParam[1][1] = 50;
                    break;
                case 148:
                    optionParam[0][1] = 500;
                    optionParam[1][1] = 100;
                    break;
                case 149:
                    optionParam[0][1] = 1200;
                    optionParam[1][1] = 240;
                    break;
                case 150:
                    optionParam[0][1] = 2400;
                    optionParam[1][1] = 480;
                    break;
                case 151:
                    optionParam[0][1] = 5000;
                    optionParam[1][1] = 1000;
                    break;
                case 266:
                    optionParam[0][1] = 9000;
                    optionParam[1][1] = 1500;
                    break;
                case 267:
                    optionParam[0][1] = 14000;
                    optionParam[1][1] = 2000;
                    break;
                case 268:
                    optionParam[0][1] = 19000;
                    optionParam[1][1] = 2500;
                    break;
                case 269:
                    optionParam[0][1] = 24000;
                    optionParam[1][1] = 3000;
                    break;
                case 28:
                    optionParam[0][1] = 15;
                    break;
                case 47:
                    optionParam[0][1] = 30;
                    optionParam[1][1] = 6;
                    break;
                case 31:
                    optionParam[0][1] = 150;
                    optionParam[1][1] = 30;
                    break;
                case 48:
                    optionParam[0][1] = 300;
                    optionParam[1][1] = 60;
                    break;
                case 164:
                    optionParam[0][1] = 600;
                    optionParam[1][1] = 120;
                    break;
                case 165:
                    optionParam[0][1] = 1500;
                    optionParam[1][1] = 300;
                    break;
                case 166:
                    optionParam[0][1] = 3000;
                    optionParam[1][1] = 600;
                    break;
                case 167:
                    optionParam[0][1] = 6000;
                    optionParam[1][1] = 1200;
                    break;
                case 270:
                    optionParam[0][1] = 10000;
                    optionParam[1][1] = 1700;
                    break;
                case 271:
                    optionParam[0][1] = 15000;
                    optionParam[1][1] = 2200;
                    break;
                case 272:
                    optionParam[0][1] = 20000;
                    optionParam[1][1] = 2700;
                    break;
                case 273:
                    optionParam[0][1] = 25000;
                    optionParam[1][1] = 3200;
                    break;
                case 29:
                    optionParam[0][1] = 10;
                    break;
                case 55:
                    optionParam[0][1] = 20;
                    optionParam[1][1] = 4;
                    break;
                case 32:
                    optionParam[0][1] = 100;
                    optionParam[1][1] = 20;
                    break;
                case 56:
                    optionParam[0][1] = 200;
                    optionParam[1][1] = 40;
                    break;
                case 180:
                    optionParam[0][1] = 400;
                    optionParam[1][1] = 80;
                    break;
                case 181:
                    optionParam[0][1] = 1000;
                    optionParam[1][1] = 200;
                    break;
                case 182:
                    optionParam[0][1] = 2000;
                    optionParam[1][1] = 400;
                    break;
                case 183:
                    optionParam[0][1] = 4000;
                    optionParam[1][1] = 800;
                    break;
                case 274:
                    optionParam[0][1] = 8000;
                    optionParam[1][1] = 1300;
                    break;
                case 275:
                    optionParam[0][1] = 13000;
                    optionParam[1][1] = 1800;
                    break;
                case 276:
                    optionParam[0][1] = 18000;
                    optionParam[1][1] = 2300;
                    break;
                case 277:
                    optionParam[0][1] = 23000;
                    optionParam[1][1] = 2800;
                    break;
                case 563:
                    optionParam[0][0] = 23;
                    optionParam[2][0] = 21;
                    optionParam[0][1] = 48;
                    optionParam[1][1] = 10000;
                    optionParam[2][1] = 14;
                    break;
                case 565:
                    optionParam[0][0] = 23;
                    optionParam[2][0] = 21;
                    optionParam[0][1] = 48;
                    optionParam[1][1] = 10000;
                    optionParam[2][1] = 14;
                    break;
                case 567:
                    optionParam[0][0] = 23;
                    optionParam[2][0] = 21;
                    optionParam[0][1] = 46;
                    optionParam[1][1] = 10000;
                    optionParam[2][1] = 14;
                    break;
            }
            break;
        case 4:
            optionParam[0][0] = 14;
            switch (tempId) {
                case 12:
                    optionParam[0][1] = 1;
                    break;
                case 57:
                    optionParam[0][1] = 2;
                    break;
                case 58:
                    optionParam[0][1] = 3;
                    break;
                case 59:
                    optionParam[0][1] = 4;
                    break;
                case 184:
                    optionParam[0][1] = 5;
                    break;
                case 185:
                    optionParam[0][1] = 6;
                    break;
                case 186:
                    optionParam[0][1] = 7;
                    break;
                case 187:
                    optionParam[0][1] = 8;
                    break;
                case 278:
                    optionParam[0][1] = 9;
                    break;
                case 279:
                    optionParam[0][1] = 10;
                    break;
                case 280:
                    optionParam[0][1] = 11;
                    break;
                case 281:
                    optionParam[0][1] = 12;
                    break;
                case 561:
                    optionParam[2][0] = 21;
                    optionParam[0][1] = 15;
                    optionParam[2][1] = 18;
                    break;
            }
            break;
    }

        return optionParam;
    }
}
