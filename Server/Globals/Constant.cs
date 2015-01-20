using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MirageXNA.Globals
{
    class Constant
    {
        // Socket Buffer Size //
        public const int R_socketBufferSize = 50000;
        public const int S_socketBufferSize = 50000;

        // Direction //
        public const byte DIR_NORTH = 0;
        public const byte DIR_SOUTH = 1;
        public const byte DIR_WEST = 2;
        public const byte DIR_EAST = 3;

        // Player Movement //
        public const byte MOVING_WALKING = 1;
        public const byte MOVING_RUNNING = 2;

        // General Constants //
        public const string GAME_NAME = "Mirage XNA Client";
        public const string GAME_WEBSITE = "http://www.rpgcreation.com";
        public const byte MAX_MAPS = 10;
        public const byte MAX_PLAYERS = 10;
        public const byte MAX_ELEMENTS = 25;
        public const byte MAX_ITEMS = 255;
        public const byte MAX_NPCS = 255;
        public const byte MAX_ANIMATIONS = 255;
        public const byte MAX_INV = 35;
        public const byte MAX_SKILLS = 35;
        public const byte MAX_MAP_ITEMS = 255;
        public const byte MAX_MAP_NPCS = 30;
        public const byte MAX_SHOPS = 50;
        public const byte MAX_SPELLS = 255;
        public const byte MAX_TRADES = 35;
        public const byte MAX_RESOURCES = 100;
        public const byte MAX_LEVELS = 100;
        public const byte MAX_BANK = 42;
        public const byte MAX_PARTYS = 35;
        public const byte MAX_PARTY_MEMBERS = 4;
        public const byte MAX_CONVS = 255;
        public const byte MAX_NPC_DROPS = 10;
        public const byte MAX_QUESTS = 255;
        public const byte MAX_PLAYER_QUESTS = 10;
    }
}
