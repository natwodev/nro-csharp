namespace Nro.Network.Configurations;

// Các mã lệnh số của giao thức client-server.
// Nguồn Java: network/logic/Cmd_message.java (giữ nguyên tuyệt đối tên hằng số Java, giá trị sbyte có dấu và các giá trị trùng theo từng pha; mã được sinh để đối chiếu Java).

public static class Cmd_message
{

    // Mã lệnh giao thức LOGIN; giữ nguyên tên hằng số Java.
    public const sbyte LOGIN = 0;

    // Mã lệnh giao thức REGISTER; giữ nguyên tên hằng số Java.
    public const sbyte REGISTER = 1;

    // Mã lệnh giao thức CLIENT_INFO; giữ nguyên tên hằng số Java.
    public const sbyte CLIENT_INFO = 2;

    // Mã lệnh giao thức SEND_SMS; giữ nguyên tên hằng số Java.
    public const sbyte SEND_SMS = 3;

    // Mã lệnh giao thức REGISTER_IMAGE; giữ nguyên tên hằng số Java.
    public const sbyte REGISTER_IMAGE = 4;

    // Mã lệnh giao thức MESSAGE_TIME; giữ nguyên tên hằng số Java.
    public const sbyte MESSAGE_TIME = 65;

    // Mã lệnh giao thức LOGOUT; giữ nguyên tên hằng số Java.
    public const sbyte LOGOUT = 0;

    // Mã lệnh giao thức SELECT_PLAYER; giữ nguyên tên hằng số Java.
    public const sbyte SELECT_PLAYER = 1;

    // Mã lệnh giao thức CREATE_PLAYER; giữ nguyên tên hằng số Java.
    public const sbyte CREATE_PLAYER = 2;

    // Mã lệnh giao thức DELETE_PLAYER; giữ nguyên tên hằng số Java.
    public const sbyte DELETE_PLAYER = 3;

    // Mã lệnh giao thức UPDATE_VERSION; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_VERSION = 4;

    // Mã lệnh giao thức UPDATE_MAP; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_MAP = 6;

    // Mã lệnh giao thức UPDATE_SKILL; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_SKILL = 7;

    // Mã lệnh giao thức UPDATE_ITEM; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_ITEM = 8;

    // Mã lệnh giao thức REQUEST_SKILL; giữ nguyên tên hằng số Java.
    public const sbyte REQUEST_SKILL = 9;

    // Mã lệnh giao thức REQUEST_MAPTEMPLATE; giữ nguyên tên hằng số Java.
    public const sbyte REQUEST_MAPTEMPLATE = 10;

    // Mã lệnh giao thức REQUEST_NPCTEMPLATE; giữ nguyên tên hằng số Java.
    public const sbyte REQUEST_NPCTEMPLATE = 11;

    // Mã lệnh giao thức REQUEST_NPCPLAYER; giữ nguyên tên hằng số Java.
    public const sbyte REQUEST_NPCPLAYER = 12;

    // Mã lệnh giao thức UPDATE_TYPE_PK; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_TYPE_PK = 35;

    // Mã lệnh giao thức PLAYER_ATTACK_PLAYER; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_ATTACK_PLAYER = -60;

    // Mã lệnh giao thức PLAYER_VS_PLAYER; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_VS_PLAYER = -59;

    // Mã lệnh giao thức CLAN_PARTY; giữ nguyên tên hằng số Java.
    public const sbyte CLAN_PARTY = -58;

    // Mã lệnh giao thức CLAN_INVITE; giữ nguyên tên hằng số Java.
    public const sbyte CLAN_INVITE = -57;

    // Mã lệnh giao thức CLAN_REMOTE; giữ nguyên tên hằng số Java.
    public const sbyte CLAN_REMOTE = -56;

    // Mã lệnh giao thức CLAN_LEAVE; giữ nguyên tên hằng số Java.
    public const sbyte CLAN_LEAVE = -55;

    // Mã lệnh giao thức CLAN_DONATE; giữ nguyên tên hằng số Java.
    public const sbyte CLAN_DONATE = -54;

    // Mã lệnh giao thức CLAN_MESSAGE; giữ nguyên tên hằng số Java.
    public const sbyte CLAN_MESSAGE = -51;

    // Mã lệnh giao thức CLAN_UPDATE; giữ nguyên tên hằng số Java.
    public const sbyte CLAN_UPDATE = -52;

    // Mã lệnh giao thức CLAN_INFO; giữ nguyên tên hằng số Java.
    public const sbyte CLAN_INFO = -53;

    // Mã lệnh giao thức CLAN_JOIN; giữ nguyên tên hằng số Java.
    public const sbyte CLAN_JOIN = -49;

    // Mã lệnh giao thức CLAN_MEMBER; giữ nguyên tên hằng số Java.
    public const sbyte CLAN_MEMBER = -50;

    // Mã lệnh giao thức CLAN_SEARCH; giữ nguyên tên hằng số Java.
    public const sbyte CLAN_SEARCH = -47;

    // Mã lệnh giao thức CLAN_CREATE_INFO; giữ nguyên tên hằng số Java.
    public const sbyte CLAN_CREATE_INFO = -46;

    // Mã lệnh giao thức CLIENT_OK; giữ nguyên tên hằng số Java.
    public const sbyte CLIENT_OK = 13;

    // Mã lệnh giao thức CLIENT_OK_INMAP; giữ nguyên tên hằng số Java.
    public const sbyte CLIENT_OK_INMAP = 14;

    // Mã lệnh giao thức UPDATE_VERSION_OK; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_VERSION_OK = 15;

    // Mã lệnh giao thức INPUT_CARD; giữ nguyên tên hằng số Java.
    public const sbyte INPUT_CARD = 16;

    // Mã lệnh giao thức CLEAR_TASK; giữ nguyên tên hằng số Java.
    public const sbyte CLEAR_TASK = 17;

    // Mã lệnh giao thức CHANGE_NAME; giữ nguyên tên hằng số Java.
    public const sbyte CHANGE_NAME = 18;

    // Mã lệnh giao thức UPDATE_PK; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_PK = 20;

    // Mã lệnh giao thức CREATE_CLAN; giữ nguyên tên hằng số Java.
    public const sbyte CREATE_CLAN = 21;

    // Mã lệnh giao thức CONVERT_UPGRADE; giữ nguyên tên hằng số Java.
    public const sbyte CONVERT_UPGRADE = 33;

    // Mã lệnh giao thức INVITE_CLANDUN; giữ nguyên tên hằng số Java.
    public const sbyte INVITE_CLANDUN = 34;

    // Mã lệnh giao thức NOT_USEACC; giữ nguyên tên hằng số Java.
    public const sbyte NOT_USEACC = 35;

    // Mã lệnh giao thức ME_LOAD_ACTIVE; giữ nguyên tên hằng số Java.
    public const sbyte ME_LOAD_ACTIVE = 36;

    // Mã lệnh giao thức ME_ACTIVE; giữ nguyên tên hằng số Java.
    public const sbyte ME_ACTIVE = 37;

    // Mã lệnh giao thức ME_UPDATE_ACTIVE; giữ nguyên tên hằng số Java.
    public const sbyte ME_UPDATE_ACTIVE = 38;

    // Mã lệnh giao thức ME_OPEN_LOCK; giữ nguyên tên hằng số Java.
    public const sbyte ME_OPEN_LOCK = 39;

    // Mã lệnh giao thức ITEM_SPLIT; giữ nguyên tên hằng số Java.
    public const sbyte ITEM_SPLIT = 40;

    // Mã lệnh giao thức ME_CLEAR_LOCK; giữ nguyên tên hằng số Java.
    public const sbyte ME_CLEAR_LOCK = 41;

    // Mã lệnh giao thức GET_IMG_BY_NAME; giữ nguyên tên hằng số Java.
    public const sbyte GET_IMG_BY_NAME = 66;

    // Mã lệnh giao thức ME_LOAD_ALL; giữ nguyên tên hằng số Java.
    public const sbyte ME_LOAD_ALL = 0;

    // Mã lệnh giao thức ME_LOAD_CLASS; giữ nguyên tên hằng số Java.
    public const sbyte ME_LOAD_CLASS = 1;

    // Mã lệnh giao thức ME_LOAD_SKILL; giữ nguyên tên hằng số Java.
    public const sbyte ME_LOAD_SKILL = 2;

    // Mã lệnh giao thức ME_LOAD_INFO; giữ nguyên tên hằng số Java.
    public const sbyte ME_LOAD_INFO = 4;

    // Mã lệnh giao thức ME_LOAD_HP; giữ nguyên tên hằng số Java.
    public const sbyte ME_LOAD_HP = 5;

    // Mã lệnh giao thức ME_LOAD_MP; giữ nguyên tên hằng số Java.
    public const sbyte ME_LOAD_MP = 6;

    // Mã lệnh giao thức PLAYER_LOAD_ALL; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_LOAD_ALL = 7;

    // Mã lệnh giao thức PLAYER_SPEED; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_SPEED = 8;

    // Mã lệnh giao thức PLAYER_LOAD_LEVEL; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_LOAD_LEVEL = 9;

    // Mã lệnh giao thức PLAYER_LOAD_VUKHI; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_LOAD_VUKHI = 10;

    // Mã lệnh giao thức PLAYER_LOAD_AO; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_LOAD_AO = 11;

    // Mã lệnh giao thức PLAYER_LOAD_QUAN; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_LOAD_QUAN = 12;

    // Mã lệnh giao thức PLAYER_LOAD_BODY; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_LOAD_BODY = 13;

    // Mã lệnh giao thức PLAYER_LOAD_HP; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_LOAD_HP = 14;

    // Mã lệnh giao thức PLAYER_LOAD_LIVE; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_LOAD_LIVE = 15;

    // Mã lệnh giao thức GOTO_PLAYER; giữ nguyên tên hằng số Java.
    public const sbyte GOTO_PLAYER = 18;

    // Mã lệnh giao thức POTENTIAL_UP; giữ nguyên tên hằng số Java.
    public const sbyte POTENTIAL_UP = 16;

    // Mã lệnh giao thức SKILL_UP; giữ nguyên tên hằng số Java.
    public const sbyte SKILL_UP = 17;

    // Mã lệnh giao thức BAG_SORT; giữ nguyên tên hằng số Java.
    public const sbyte BAG_SORT = 18;

    // Mã lệnh giao thức BOX_SORT; giữ nguyên tên hằng số Java.
    public const sbyte BOX_SORT = 19;

    // Mã lệnh giao thức BOX_COIN_OUT; giữ nguyên tên hằng số Java.
    public const sbyte BOX_COIN_OUT = 21;

    // Mã lệnh giao thức REQUEST_ITEM; giữ nguyên tên hằng số Java.
    public const sbyte REQUEST_ITEM = 22;

    // Mã lệnh giao thức ME_ADD_SKILL; giữ nguyên tên hằng số Java.
    public const sbyte ME_ADD_SKILL = 23;

    // Mã lệnh giao thức ME_UPDATE_SKILL; giữ nguyên tên hằng số Java.
    public const sbyte ME_UPDATE_SKILL = 62;

    // Mã lệnh giao thức GET_PLAYER_MENU; giữ nguyên tên hằng số Java.
    public const sbyte GET_PLAYER_MENU = 63;

    // Mã lệnh giao thức PLAYER_MENU_ACTION; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_MENU_ACTION = 64;

    // Mã lệnh giao thức SAVE_RMS; giữ nguyên tên hằng số Java.
    public const sbyte SAVE_RMS = 60;

    // Mã lệnh giao thức LOAD_RMS; giữ nguyên tên hằng số Java.
    public const sbyte LOAD_RMS = 61;

    // Mã lệnh giao thức USE_BOOK_SKILL; giữ nguyên tên hằng số Java.
    public const sbyte USE_BOOK_SKILL = 43;

    // Mã lệnh giao thức LOCK_INVENTORY; giữ nguyên tên hằng số Java.
    public const sbyte LOCK_INVENTORY = -104;

    // Mã lệnh giao thức CHANGE_FLAG; giữ nguyên tên hằng số Java.
    public const sbyte CHANGE_FLAG = -103;

    // Mã lệnh giao thức LOGINFAIL; giữ nguyên tên hằng số Java.
    public const sbyte LOGINFAIL = -102;

    // Mã lệnh giao thức LOGIN2; giữ nguyên tên hằng số Java.
    public const sbyte LOGIN2 = -101;

    // Mã lệnh giao thức KIGUI; giữ nguyên tên hằng số Java.
    public const sbyte KIGUI = -100;

    // Mã lệnh giao thức ENEMY_LIST; giữ nguyên tên hằng số Java.
    public const sbyte ENEMY_LIST = -99;

    // Mã lệnh giao thức ANDROID_IAP; giữ nguyên tên hằng số Java.
    public const sbyte ANDROID_IAP = -98;

    // Mã lệnh giao thức UPDATE_ACTIVEPOINT; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_ACTIVEPOINT = -97;

    // Mã lệnh giao thức TOP; giữ nguyên tên hằng số Java.
    public const sbyte TOP = -96;

    // Mã lệnh giao thức MOB_ME_UPDATE; giữ nguyên tên hằng số Java.
    public const sbyte MOB_ME_UPDATE = -95;

    // Mã lệnh giao thức UPDATE_COOLDOWN; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_COOLDOWN = -94;

    // Mã lệnh giao thức BGITEM_VERSION; giữ nguyên tên hằng số Java.
    public const sbyte BGITEM_VERSION = -93;

    // Mã lệnh giao thức SET_CLIENTTYPE; giữ nguyên tên hằng số Java.
    public const sbyte SET_CLIENTTYPE = -92;

    // Mã lệnh giao thức MAP_TRASPORT; giữ nguyên tên hằng số Java.
    public const sbyte MAP_TRASPORT = -91;

    // Mã lệnh giao thức UPDATE_BODY; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_BODY = -90;

    // Mã lệnh giao thức SERVERSCREEN; giữ nguyên tên hằng số Java.
    public const sbyte SERVERSCREEN = -88;

    // Mã lệnh giao thức UPDATE_DATA; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_DATA = -87;

    // Mã lệnh giao thức GIAO_DICH; giữ nguyên tên hằng số Java.
    public const sbyte GIAO_DICH = -86;

    // Mã lệnh giao thức MOB_CAPCHA; giữ nguyên tên hằng số Java.
    public const sbyte MOB_CAPCHA = -85;

    // Mã lệnh giao thức MOB_MAX_HP; giữ nguyên tên hằng số Java.
    public const sbyte MOB_MAX_HP = -84;

    // Mã lệnh giao thức CALL_DRAGON; giữ nguyên tên hằng số Java.
    public const sbyte CALL_DRAGON = -83;

    // Mã lệnh giao thức TILE_SET; giữ nguyên tên hằng số Java.
    public const sbyte TILE_SET = -82;

    // Mã lệnh giao thức COMBINNE; giữ nguyên tên hằng số Java.
    public const sbyte COMBINNE = -81;

    // Mã lệnh giao thức FRIEND; giữ nguyên tên hằng số Java.
    public const sbyte FRIEND = -80;

    // Mã lệnh giao thức PLAYER_MENU; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_MENU = -79;

    // Mã lệnh giao thức CHECK_MOVE; giữ nguyên tên hằng số Java.
    public const sbyte CHECK_MOVE = -78;

    // Mã lệnh giao thức SMALLIMAGE_VERSION; giữ nguyên tên hằng số Java.
    public const sbyte SMALLIMAGE_VERSION = -77;

    // Mã lệnh giao thức ARCHIVEMENT; giữ nguyên tên hằng số Java.
    public const sbyte ARCHIVEMENT = -76;

    // Mã lệnh giao thức NPC_BOSS; giữ nguyên tên hằng số Java.
    public const sbyte NPC_BOSS = -75;

    // Mã lệnh giao thức GET_IMAGE_SOURCE; giữ nguyên tên hằng số Java.
    public const sbyte GET_IMAGE_SOURCE = -74;

    // Mã lệnh giao thức NPC_ADD_REMOVE; giữ nguyên tên hằng số Java.
    public const sbyte NPC_ADD_REMOVE = -73;

    // Mã lệnh giao thức CHAT_PLAYER; giữ nguyên tên hằng số Java.
    public const sbyte CHAT_PLAYER = -72;

    // Mã lệnh giao thức CHAT_THEGIOI_CLIENT; giữ nguyên tên hằng số Java.
    public const sbyte CHAT_THEGIOI_CLIENT = -71;

    // Mã lệnh giao thức BIG_MESSAGE; giữ nguyên tên hằng số Java.
    public const sbyte BIG_MESSAGE = -70;

    // Mã lệnh giao thức MAXSTAMINA; giữ nguyên tên hằng số Java.
    public const sbyte MAXSTAMINA = -69;

    // Mã lệnh giao thức STAMINA; giữ nguyên tên hằng số Java.
    public const sbyte STAMINA = -68;

    // Mã lệnh giao thức REQUEST_ICON; giữ nguyên tên hằng số Java.
    public const sbyte REQUEST_ICON = -67;

    // Mã lệnh giao thức GET_EFFDATA; giữ nguyên tên hằng số Java.
    public const sbyte GET_EFFDATA = -66;

    // Mã lệnh giao thức TELEPORT; giữ nguyên tên hằng số Java.
    public const sbyte TELEPORT = -65;

    // Mã lệnh giao thức UPDATE_BAG; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_BAG = -64;

    // Mã lệnh giao thức GET_BAG; giữ nguyên tên hằng số Java.
    public const sbyte GET_BAG = -63;

    // Mã lệnh giao thức CLAN_IMAGE; giữ nguyên tên hằng số Java.
    public const sbyte CLAN_IMAGE = -62;

    // Mã lệnh giao thức UPDATE_CLANID; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_CLANID = -61;

    // Mã lệnh giao thức SKILL_NOT_FOCUS; giữ nguyên tên hằng số Java.
    public const sbyte SKILL_NOT_FOCUS = -45;

    // Mã lệnh giao thức SHOP; giữ nguyên tên hằng số Java.
    public const sbyte SHOP = -44;

    // Mã lệnh giao thức USE_ITEM; giữ nguyên tên hằng số Java.
    public const sbyte USE_ITEM = -43;

    // Mã lệnh giao thức ME_LOAD_POINT; giữ nguyên tên hằng số Java.
    public const sbyte ME_LOAD_POINT = -42;

    // Mã lệnh giao thức UPDATE_CAPTION; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_CAPTION = -41;

    // Mã lệnh giao thức GET_ITEM; giữ nguyên tên hằng số Java.
    public const sbyte GET_ITEM = -40;

    // Mã lệnh giao thức FINISH_LOADMAP; giữ nguyên tên hằng số Java.
    public const sbyte FINISH_LOADMAP = -39;

    // Mã lệnh giao thức FINISH_UPDATE; giữ nguyên tên hằng số Java.
    public const sbyte FINISH_UPDATE = -38;

    // Mã lệnh giao thức BODY; giữ nguyên tên hằng số Java.
    public const sbyte BODY = -37;

    // Mã lệnh giao thức BAG; giữ nguyên tên hằng số Java.
    public const sbyte BAG = -36;

    // Mã lệnh giao thức BOX; giữ nguyên tên hằng số Java.
    public const sbyte BOX = -35;

    // Mã lệnh giao thức MAGIC_TREE; giữ nguyên tên hằng số Java.
    public const sbyte MAGIC_TREE = -34;

    // Mã lệnh giao thức MAP_OFFLINE; giữ nguyên tên hằng số Java.
    public const sbyte MAP_OFFLINE = -33;

    // Mã lệnh giao thức BACKGROUND_TEMPLATE; giữ nguyên tên hằng số Java.
    public const sbyte BACKGROUND_TEMPLATE = -32;

    // Mã lệnh giao thức ITEM_BACKGROUND; giữ nguyên tên hằng số Java.
    public const sbyte ITEM_BACKGROUND = -31;

    // Mã lệnh giao thức SUB_COMMAND; giữ nguyên tên hằng số Java.
    public const sbyte SUB_COMMAND = -30;

    // Mã lệnh giao thức NOT_LOGIN; giữ nguyên tên hằng số Java.
    public const sbyte NOT_LOGIN = -29;

    // Mã lệnh giao thức NOT_MAP; giữ nguyên tên hằng số Java.
    public const sbyte NOT_MAP = -28;

    // Mã lệnh giao thức GET_SESSION_ID; giữ nguyên tên hằng số Java.
    public const sbyte GET_SESSION_ID = -27;

    // Mã lệnh giao thức DIALOG_MESSAGE; giữ nguyên tên hằng số Java.
    public const sbyte DIALOG_MESSAGE = -26;

    // Mã lệnh giao thức SERVER_MESSAGE; giữ nguyên tên hằng số Java.
    public const sbyte SERVER_MESSAGE = -25;

    // Mã lệnh giao thức MAP_INFO; giữ nguyên tên hằng số Java.
    public const sbyte MAP_INFO = -24;

    // Mã lệnh giao thức MAP_CHANGE; giữ nguyên tên hằng số Java.
    public const sbyte MAP_CHANGE = -23;

    // Mã lệnh giao thức MAP_CLEAR; giữ nguyên tên hằng số Java.
    public const sbyte MAP_CLEAR = -22;

    // Mã lệnh giao thức ITEMMAP_REMOVE; giữ nguyên tên hằng số Java.
    public const sbyte ITEMMAP_REMOVE = -21;

    // Mã lệnh giao thức ITEMMAP_MYPICK; giữ nguyên tên hằng số Java.
    public const sbyte ITEMMAP_MYPICK = -20;

    // Mã lệnh giao thức ITEMMAP_PLAYERPICK; giữ nguyên tên hằng số Java.
    public const sbyte ITEMMAP_PLAYERPICK = -19;

    // Mã lệnh giao thức ME_THROW; giữ nguyên tên hằng số Java.
    public const sbyte ME_THROW = -18;

    // Mã lệnh giao thức ME_DIE; giữ nguyên tên hằng số Java.
    public const sbyte ME_DIE = -17;

    // Mã lệnh giao thức ME_LIVE; giữ nguyên tên hằng số Java.
    public const sbyte ME_LIVE = -16;

    // Mã lệnh giao thức ME_BACK; giữ nguyên tên hằng số Java.
    public const sbyte ME_BACK = -15;

    // Mã lệnh giao thức PLAYER_THROW; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_THROW = -14;

    // Mã lệnh giao thức NPC_LIVE; giữ nguyên tên hằng số Java.
    public const sbyte NPC_LIVE = -13;

    // Mã lệnh giao thức NPC_DIE; giữ nguyên tên hằng số Java.
    public const sbyte NPC_DIE = -12;

    // Mã lệnh giao thức NPC_ATTACK_ME; giữ nguyên tên hằng số Java.
    public const sbyte NPC_ATTACK_ME = -11;

    // Mã lệnh giao thức NPC_ATTACK_PLAYER; giữ nguyên tên hằng số Java.
    public const sbyte NPC_ATTACK_PLAYER = -10;

    // Mã lệnh giao thức MOB_HP; giữ nguyên tên hằng số Java.
    public const sbyte MOB_HP = -9;

    // Mã lệnh giao thức PLAYER_DIE; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_DIE = -8;

    // Mã lệnh giao thức PLAYER_MOVE; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_MOVE = -7;

    // Mã lệnh giao thức PLAYER_REMOVE; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_REMOVE = -6;

    // Mã lệnh giao thức PLAYER_ADD; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_ADD = -5;

    // Mã lệnh giao thức PLAYER_ATTACK_N_P; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_ATTACK_N_P = -4;

    // Mã lệnh giao thức PLAYER_UP_EXP; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_UP_EXP = -3;

    // Mã lệnh giao thức ME_UP_COIN_LOCK; giữ nguyên tên hằng số Java.
    public const sbyte ME_UP_COIN_LOCK = -2;

    // Mã lệnh giao thức ME_CHANGE_COIN; giữ nguyên tên hằng số Java.
    public const sbyte ME_CHANGE_COIN = -1;

    // Mã lệnh giao thức ITEM_BUY; giữ nguyên tên hằng số Java.
    public const sbyte ITEM_BUY = 6;

    // Mã lệnh giao thức ITEM_SALE; giữ nguyên tên hằng số Java.
    public const sbyte ITEM_SALE = 7;

    // Mã lệnh giao thức UPPEARL_LOCK; giữ nguyên tên hằng số Java.
    public const sbyte UPPEARL_LOCK = 13;

    // Mã lệnh giao thức UPGRADE; giữ nguyên tên hằng số Java.
    public const sbyte UPGRADE = 14;

    // Mã lệnh giao thức PLEASE_INPUT_PARTY; giữ nguyên tên hằng số Java.
    public const sbyte PLEASE_INPUT_PARTY = 16;

    // Mã lệnh giao thức ACCEPT_PLEASE_PARTY; giữ nguyên tên hằng số Java.
    public const sbyte ACCEPT_PLEASE_PARTY = 17;

    // Mã lệnh giao thức REQUEST_PLAYERS; giữ nguyên tên hằng số Java.
    public const sbyte REQUEST_PLAYERS = 18;

    // Mã lệnh giao thức UPDATE_ACHIEVEMENT; giữ nguyên tên hằng số Java.
    public const sbyte UPDATE_ACHIEVEMENT = 19;

    // Mã lệnh giao thức PHUBANG_INFO; giữ nguyên tên hằng số Java.
    public const sbyte PHUBANG_INFO = 20;

    // Mã lệnh giao thức ZONE_CHANGE; giữ nguyên tên hằng số Java.
    public const sbyte ZONE_CHANGE = 21;

    // Mã lệnh giao thức MENU; giữ nguyên tên hằng số Java.
    public const sbyte MENU = 22;

    // Mã lệnh giao thức OPEN_UI; giữ nguyên tên hằng số Java.
    public const sbyte OPEN_UI = 23;

    // Mã lệnh giao thức OPTION_HAT; giữ nguyên tên hằng số Java.
    public const sbyte OPTION_HAT = 24;

    // Mã lệnh giao thức OPEN_UI_PT; giữ nguyên tên hằng số Java.
    public const sbyte OPEN_UI_PT = 25;

    // Mã lệnh giao thức OPEN_UI_SHOP; giữ nguyên tên hằng số Java.
    public const sbyte OPEN_UI_SHOP = 26;

    // Mã lệnh giao thức OPEN_MENU_ID; giữ nguyên tên hằng số Java.
    public const sbyte OPEN_MENU_ID = 27;

    // Mã lệnh giao thức OPEN_UI_COLLECT; giữ nguyên tên hằng số Java.
    public const sbyte OPEN_UI_COLLECT = 28;

    // Mã lệnh giao thức OPEN_UI_ZONE; giữ nguyên tên hằng số Java.
    public const sbyte OPEN_UI_ZONE = 29;

    // Mã lệnh giao thức OPEN_UI_TRADE; giữ nguyên tên hằng số Java.
    public const sbyte OPEN_UI_TRADE = 30;

    // Mã lệnh giao thức OPEN_UI_SAY; giữ nguyên tên hằng số Java.
    public const sbyte OPEN_UI_SAY = 38;

    // Mã lệnh giao thức OPEN_UI_CONFIRM; giữ nguyên tên hằng số Java.
    public const sbyte OPEN_UI_CONFIRM = 32;

    // Mã lệnh giao thức OPEN_UI_MENU; giữ nguyên tên hằng số Java.
    public const sbyte OPEN_UI_MENU = 33;

    // Mã lệnh giao thức SKILL_SELECT; giữ nguyên tên hằng số Java.
    public const sbyte SKILL_SELECT = 34;

    // Mã lệnh giao thức REQUEST_ITEM_INFO; giữ nguyên tên hằng số Java.
    public const sbyte REQUEST_ITEM_INFO = 35;

    // Mã lệnh giao thức TRADE_INVITE; giữ nguyên tên hằng số Java.
    public const sbyte TRADE_INVITE = 36;

    // Mã lệnh giao thức TRADE_INVITE_ACCEPT; giữ nguyên tên hằng số Java.
    public const sbyte TRADE_INVITE_ACCEPT = 37;

    // Mã lệnh giao thức TRADE_LOCK_ITEM; giữ nguyên tên hằng số Java.
    public const sbyte TRADE_LOCK_ITEM = 38;

    // Mã lệnh giao thức TRADE_ACCEPT; giữ nguyên tên hằng số Java.
    public const sbyte TRADE_ACCEPT = 39;

    // Mã lệnh giao thức TASK_GET; giữ nguyên tên hằng số Java.
    public const sbyte TASK_GET = 40;

    // Mã lệnh giao thức TASK_NEXT; giữ nguyên tên hằng số Java.
    public const sbyte TASK_NEXT = 41;

    // Mã lệnh giao thức GAME_INFO; giữ nguyên tên hằng số Java.
    public const sbyte GAME_INFO = 50;

    // Mã lệnh giao thức TASK_UPDATE; giữ nguyên tên hằng số Java.
    public const sbyte TASK_UPDATE = 43;

    // Mã lệnh giao thức CHAT_MAP; giữ nguyên tên hằng số Java.
    public const sbyte CHAT_MAP = 44;

    // Mã lệnh giao thức NPC_MISS; giữ nguyên tên hằng số Java.
    public const sbyte NPC_MISS = 45;

    // Mã lệnh giao thức RESET_POINT; giữ nguyên tên hằng số Java.
    public const sbyte RESET_POINT = 46;

    // Mã lệnh giao thức ALERT_MESSAGE; giữ nguyên tên hằng số Java.
    public const sbyte ALERT_MESSAGE = 47;

    // Mã lệnh giao thức AUTO_SERVER; giữ nguyên tên hằng số Java.
    public const sbyte AUTO_SERVER = 48;

    // Mã lệnh giao thức ALERT_SEND_SMS; giữ nguyên tên hằng số Java.
    public const sbyte ALERT_SEND_SMS = 49;

    // Mã lệnh giao thức TRADE_INVITE_CANCEL; giữ nguyên tên hằng số Java.
    public const sbyte TRADE_INVITE_CANCEL = 50;

    // Mã lệnh giao thức BOSS_SKILL; giữ nguyên tên hằng số Java.
    public const sbyte BOSS_SKILL = 51;

    // Mã lệnh giao thức MABU_HOLD; giữ nguyên tên hằng số Java.
    public const sbyte MABU_HOLD = 52;

    // Mã lệnh giao thức FRIEND_INVITE; giữ nguyên tên hằng số Java.
    public const sbyte FRIEND_INVITE = 53;

    // Mã lệnh giao thức PLAYER_ATTACK_NPC; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_ATTACK_NPC = 54;

    // Mã lệnh giao thức HAVE_ATTACK_PLAYER; giữ nguyên tên hằng số Java.
    public const sbyte HAVE_ATTACK_PLAYER = 56;

    // Mã lệnh giao thức OPEN_UI_NEWMENU; giữ nguyên tên hằng số Java.
    public const sbyte OPEN_UI_NEWMENU = 57;

    // Mã lệnh giao thức MOVE_FAST; giữ nguyên tên hằng số Java.
    public const sbyte MOVE_FAST = 58;

    // Mã lệnh giao thức TEST_INVITE; giữ nguyên tên hằng số Java.
    public const sbyte TEST_INVITE = 59;

    // Mã lệnh giao thức ADD_CUU_SAT; giữ nguyên tên hằng số Java.
    public const sbyte ADD_CUU_SAT = 62;

    // Mã lệnh giao thức ME_CUU_SAT; giữ nguyên tên hằng số Java.
    public const sbyte ME_CUU_SAT = 63;

    // Mã lệnh giao thức CLEAR_CUU_SAT; giữ nguyên tên hằng số Java.
    public const sbyte CLEAR_CUU_SAT = 64;

    // Mã lệnh giao thức PLAYER_UP_EXPDOWN; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_UP_EXPDOWN = 65;

    // Mã lệnh giao thức ME_DIE_EXP_DOWN; giữ nguyên tên hằng số Java.
    public const sbyte ME_DIE_EXP_DOWN = 66;

    // Mã lệnh giao thức PLAYER_ATTACK_P_N; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_ATTACK_P_N = 67;

    // Mã lệnh giao thức ITEMMAP_ADD; giữ nguyên tên hằng số Java.
    public const sbyte ITEMMAP_ADD = 68;

    // Mã lệnh giao thức DEL_ACC; giữ nguyên tên hằng số Java.
    public const sbyte DEL_ACC = 69;

    // Mã lệnh giao thức USE_SKILL_MY_BUFF; giữ nguyên tên hằng số Java.
    public const sbyte USE_SKILL_MY_BUFF = 70;

    // Mã lệnh giao thức NPC_CHANGE; giữ nguyên tên hằng số Java.
    public const sbyte NPC_CHANGE = 74;

    // Mã lệnh giao thức PARTY_INVITE; giữ nguyên tên hằng số Java.
    public const sbyte PARTY_INVITE = 75;

    // Mã lệnh giao thức PARTY_ACCEPT; giữ nguyên tên hằng số Java.
    public const sbyte PARTY_ACCEPT = 76;

    // Mã lệnh giao thức PARTY_CANCEL; giữ nguyên tên hằng số Java.
    public const sbyte PARTY_CANCEL = 77;

    // Mã lệnh giao thức PLAYER_IN_PARTY; giữ nguyên tên hằng số Java.
    public const sbyte PLAYER_IN_PARTY = 78;

    // Mã lệnh giao thức PARTY_OUT; giữ nguyên tên hằng số Java.
    public const sbyte PARTY_OUT = 79;

    // Mã lệnh giao thức FRIEND_ADD; giữ nguyên tên hằng số Java.
    public const sbyte FRIEND_ADD = 80;

    // Mã lệnh giao thức NPC_IS_DISABLE; giữ nguyên tên hằng số Java.
    public const sbyte NPC_IS_DISABLE = 81;

    // Mã lệnh giao thức NPC_IS_MOVE; giữ nguyên tên hằng số Java.
    public const sbyte NPC_IS_MOVE = 82;

    // Mã lệnh giao thức SUMON_ATTACK; giữ nguyên tên hằng số Java.
    public const sbyte SUMON_ATTACK = 83;

    // Mã lệnh giao thức RETURN_POINT_MAP; giữ nguyên tên hằng số Java.
    public const sbyte RETURN_POINT_MAP = 84;

    // Mã lệnh giao thức NPC_IS_FIRE; giữ nguyên tên hằng số Java.
    public const sbyte NPC_IS_FIRE = 85;

    // Mã lệnh giao thức NPC_IS_ICE; giữ nguyên tên hằng số Java.
    public const sbyte NPC_IS_ICE = 86;

    // Mã lệnh giao thức NPC_IS_WIND; giữ nguyên tên hằng số Java.
    public const sbyte NPC_IS_WIND = 87;

    // Mã lệnh giao thức OPEN_TEXT_BOX_ID; giữ nguyên tên hằng số Java.
    public const sbyte OPEN_TEXT_BOX_ID = 88;

    // Mã lệnh giao thức REQUEST_ITEM_PLAYER; giữ nguyên tên hằng số Java.
    public const sbyte REQUEST_ITEM_PLAYER = 90;

    // Mã lệnh giao thức CHAT_PRIVATE; giữ nguyên tên hằng số Java.
    public const sbyte CHAT_PRIVATE = 91;

    // Mã lệnh giao thức CHAT_THEGIOI_SERVER; giữ nguyên tên hằng số Java.
    public const sbyte CHAT_THEGIOI_SERVER = 92;

    // Mã lệnh giao thức CHAT_VIP; giữ nguyên tên hằng số Java.
    public const sbyte CHAT_VIP = 93;

    // Mã lệnh giao thức SERVER_ALERT; giữ nguyên tên hằng số Java.
    public const sbyte SERVER_ALERT = 94;

    // Mã lệnh giao thức ME_UP_COIN_BAG; giữ nguyên tên hằng số Java.
    public const sbyte ME_UP_COIN_BAG = 95;

    // Mã lệnh giao thức GET_TASK_ORDER; giữ nguyên tên hằng số Java.
    public const sbyte GET_TASK_ORDER = 96;

    // Mã lệnh giao thức GET_TASK_UPDATE; giữ nguyên tên hằng số Java.
    public const sbyte GET_TASK_UPDATE = 97;

    // Mã lệnh giao thức CLEAR_TASK_ORDER; giữ nguyên tên hằng số Java.
    public const sbyte CLEAR_TASK_ORDER = 98;

    // Mã lệnh giao thức ADD_ITEM_MAP; giữ nguyên tên hằng số Java.
    public const sbyte ADD_ITEM_MAP = 99;

    // Mã lệnh giao thức TRANSPORT; giữ nguyên tên hằng số Java.
    public const sbyte TRANSPORT = -105;

    // Mã lệnh giao thức ITEM_TIME; giữ nguyên tên hằng số Java.
    public const sbyte ITEM_TIME = -106;

    // Mã lệnh giao thức PET_INFO; giữ nguyên tên hằng số Java.
    public const sbyte PET_INFO = -107;

    // Mã lệnh giao thức PET_STATUS; giữ nguyên tên hằng số Java.
    public const sbyte PET_STATUS = -108;

    // Mã lệnh giao thức SERVER_DATA; giữ nguyên tên hằng số Java.
    public const sbyte SERVER_DATA = -110;

    // Mã lệnh giao thức CLIENT_INPUT; giữ nguyên tên hằng số Java.
    public const sbyte CLIENT_INPUT = -125;

    // Mã lệnh giao thức HOLD; giữ nguyên tên hằng số Java.
    public const sbyte HOLD = -124;

    // Mã lệnh giao thức SHOW_ADS; giữ nguyên tên hằng số Java.
    public const sbyte SHOW_ADS = 121;

    // Mã lệnh giao thức LOGIN_DE; giữ nguyên tên hằng số Java.
    public const sbyte LOGIN_DE = 122;

    // Mã lệnh giao thức SET_POS; giữ nguyên tên hằng số Java.
    public const sbyte SET_POS = 123;

    // Mã lệnh giao thức NPC_CHAT; giữ nguyên tên hằng số Java.
    public const sbyte NPC_CHAT = 124;

    // Mã lệnh giao thức FUSION; giữ nguyên tên hằng số Java.
    public const sbyte FUSION = 125;

    // Mã lệnh giao thức ANDROID_PACK; giữ nguyên tên hằng số Java.
    public const sbyte ANDROID_PACK = 126;

    // Mã lệnh giao thức GET_IMAGE_SOURCE2; giữ nguyên tên hằng số Java.
    public const sbyte GET_IMAGE_SOURCE2 = -111;

    // Mã lệnh giao thức CHAGE_MOD_BODY; giữ nguyên tên hằng số Java.
    public const sbyte CHAGE_MOD_BODY = -112;

    // Mã lệnh giao thức CHANGE_ONSKILL; giữ nguyên tên hằng số Java.
    public const sbyte CHANGE_ONSKILL = -113;

    // Mã lệnh giao thức REQUEST_PEAN; giữ nguyên tên hằng số Java.
    public const sbyte REQUEST_PEAN = -114;

    // Mã lệnh giao thức POWER_INFO; giữ nguyên tên hằng số Java.
    public const sbyte POWER_INFO = -115;

    // Mã lệnh giao thức AUTOPLAY; giữ nguyên tên hằng số Java.
    public const sbyte AUTOPLAY = -116;

    // Mã lệnh giao thức MABU; giữ nguyên tên hằng số Java.
    public const sbyte MABU = -117;

    // Mã lệnh giao thức THACHDAU; giữ nguyên tên hằng số Java.
    public const sbyte THACHDAU = -118;

    // Mã lệnh giao thức THELUC; giữ nguyên tên hằng số Java.
    public const sbyte THELUC = -119;

    // Mã lệnh giao thức UPDATECHAR_MP; giữ nguyên tên hằng số Java.
    public const sbyte UPDATECHAR_MP = -123;

    // Mã lệnh giao thức REFRESH_ITEM; giữ nguyên tên hằng số Java.
    public const sbyte REFRESH_ITEM = 100;

    // Mã lệnh giao thức CHECK_CONTROLLER; giữ nguyên tên hằng số Java.
    public const sbyte CHECK_CONTROLLER = -120;

    // Mã lệnh giao thức CHECK_MAP; giữ nguyên tên hằng số Java.
    public const sbyte CHECK_MAP = -121;

    // Mã lệnh giao thức BIG_BOSS; giữ nguyên tên hằng số Java.
    public const sbyte BIG_BOSS = 101;

    // Mã lệnh giao thức BIG_BOSS_2; giữ nguyên tên hằng số Java.
    public const sbyte BIG_BOSS_2 = 102;

    // Mã lệnh giao thức DUAHAU; giữ nguyên tên hằng số Java.
    public const sbyte DUAHAU = -122;

    // Mã lệnh giao thức QUAYSO; giữ nguyên tên hằng số Java.
    public const sbyte QUAYSO = -126;

    // Mã lệnh giao thức USER_INFO; giữ nguyên tên hằng số Java.
    public const sbyte USER_INFO = 42;

    // Mã lệnh giao thức OPEN3HOUR; giữ nguyên tên hằng số Java.
    public const sbyte OPEN3HOUR = -89;

    // Mã lệnh giao thức STATUS_PET; giữ nguyên tên hằng số Java.
    public const sbyte STATUS_PET = 31;

    // Mã lệnh giao thức SPEACIAL_SKILL; giữ nguyên tên hằng số Java.
    public const sbyte SPEACIAL_SKILL = 112;

    // Mã lệnh giao thức SERVER_EFFECT; giữ nguyên tên hằng số Java.
    public const sbyte SERVER_EFFECT = 113;

    // Mã lệnh giao thức INAPP; giữ nguyên tên hằng số Java.
    public const sbyte INAPP = 114;

    // Mã lệnh giao thức LUCKY_ROUND; giữ nguyên tên hằng số Java.
    public const sbyte LUCKY_ROUND = -127;

    // Mã lệnh giao thức RADA_CARD; giữ nguyên tên hằng số Java.
    public const sbyte RADA_CARD = 127;

    // Mã lệnh giao thức CHAR_EFFECT; giữ nguyên tên hằng số Java.
    public const sbyte CHAR_EFFECT = -128;
}
