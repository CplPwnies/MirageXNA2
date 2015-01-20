///////////////////////////////////////////////////
////// MIRAGE XNA - DEVELOPED BY MARK MORRIS //////
//////          WWW.RPGCREATION.COM          //////
///////////////////////////////////////////////////

using System;

using MirageXNA.Global;
using MirageXNA.Network;

namespace MirageXNA.Engine
{
    class Logic
    {
        //////////////////
        // Player Logic //
        //////////////////

        public static void processMovement(Int32 Index)
        {
            int MovementSpeed = 4;

            switch (Types.Players[Index].Dir)
            {
                case Constant.DIR_NORTH:
                    Types.Players[Index].OffsetY = Types.Players[Index].OffsetY - MovementSpeed;
                    if (Types.Players[Index].OffsetY < 0) Types.Players[Index].OffsetY = 0;
                    break;
                case Constant.DIR_SOUTH:
                    Types.Players[Index].OffsetY = Types.Players[Index].OffsetY + MovementSpeed;
                    if (Types.Players[Index].OffsetY > 0) Types.Players[Index].OffsetY = 0;
                    break;
                case Constant.DIR_WEST:
                    Types.Players[Index].OffsetX = Types.Players[Index].OffsetX - MovementSpeed;
                    if (Types.Players[Index].OffsetX < 0) Types.Players[Index].OffsetX = 0;
                    break;
                case Constant.DIR_EAST:
                    Types.Players[Index].OffsetX = Types.Players[Index].OffsetX + MovementSpeed;
                    if (Types.Players[Index].OffsetX > 0) Types.Players[Index].OffsetX = 0;
                    break;
            }

            // Check if completed walking over to the next tile
            if (Types.Players[Index].Moving > 0)
            {
                switch (Types.Players[Index].Dir)
                {

                    case Constant.DIR_SOUTH:
                        if (Types.Players[Index].OffsetX >= 0 & Types.Players[Index].OffsetY >= 0)
                        {
                            Types.Players[Index].Moving = 0;
                            if (Types.Players[Index].Step == 0)
                            {
                                Types.Players[Index].Step = 2;
                            }
                            else
                            {
                                Types.Players[Index].Step = 0;
                            }
                        }
                        break;

                    case Constant.DIR_EAST:
                        if (Types.Players[Index].OffsetX >= 0 & Types.Players[Index].OffsetY >= 0)
                        {
                            Types.Players[Index].Moving = 0;
                            if (Types.Players[Index].Step == 0)
                            {
                                Types.Players[Index].Step = 2;
                            }
                            else
                            {
                                Types.Players[Index].Step = 0;
                            }
                        }
                        break;

                    case Constant.DIR_WEST:
                        if (Types.Players[Index].OffsetX <= 0 & Types.Players[Index].OffsetY <= 0)
                        {
                            Types.Players[Index].Moving = 0;
                            if (Types.Players[Index].Step == 0)
                            {
                                Types.Players[Index].Step = 2;
                            }
                            else
                            {
                                Types.Players[Index].Step = 0;
                            }
                        }
                        break;

                    case Constant.DIR_NORTH:
                        if (Types.Players[Index].OffsetX <= 0 & Types.Players[Index].OffsetY <= 0)
                        {
                            Types.Players[Index].Moving = 0;
                            if (Types.Players[Index].Step == 0)
                            {
                                Types.Players[Index].Step = 2;
                            }
                            else
                            {
                                Types.Players[Index].Step = 0;
                            }
                        }
                        break;
                }
            }
        }

        public static bool checkDirection(byte direction)
        {
            int X = 0; int Y = 0;

            switch (direction)
            {
                case Constant.DIR_NORTH:
                    X = Types.Players[Static.MyIndex].X;
                    Y = Types.Players[Static.MyIndex].Y - 1;
                    break;
                case Constant.DIR_SOUTH:
                    X = Types.Players[Static.MyIndex].X;
                    Y = Types.Players[Static.MyIndex].Y + 1;
                    break;
                case Constant.DIR_WEST:
                    X = Types.Players[Static.MyIndex].X - 1;
                    Y = Types.Players[Static.MyIndex].Y;
                    break;
                case Constant.DIR_EAST:
                    X = Types.Players[Static.MyIndex].X + 1;
                    Y = Types.Players[Static.MyIndex].Y;
                    break;
            }

            // Check to see if a player is already on that tile
            for (int i = 1; i <= Constant.MAX_PLAYERS; i++)
            {
                if (ClientTCP.IsPlaying(i) && Types.Players[i].Map == Types.Players[Static.MyIndex].Map)
                {
                    if (Types.Players[i].X == X)
                    {
                        if (Types.Players[i].Y == Y)
                        {
                            // Cant Move - Movement Checked //
                            return true;
                        }
                    }
                }
            }

            // No Blocks - Can Move //
            return false;
        }

        public static bool canMove()
        {
            bool cMove = true;
            int dir = Types.Players[Static.MyIndex].Dir;

            if (Types.Players[Static.MyIndex].Moving != 0) { return false; }
            if (Static.DirUp) 
            {
                Types.Players[Static.MyIndex].Dir = Constant.DIR_NORTH; 
                if (Types.Players[Static.MyIndex].Y > 0)
                {
                    if (checkDirection(Constant.DIR_NORTH))
                    {
                        cMove = false;

                        // Change Direction //
                        if (dir != Constant.DIR_NORTH)
                        {
                            ClientTCP.SendPlayerDir();
                        }
                    }
                }
                else
                {
                    // Check if they can warp to a new map
                    if (Types.Map.Up > 0)
                    {
                        ClientTCP.SendPlayerRequestNewMap();
                        //CanMoveNow = False
                    }
                    return false;
                }
                return cMove; 
            }

            if (Static.DirDown)
            {
                Types.Players[Static.MyIndex].Dir = Constant.DIR_SOUTH; 
                if (Types.Players[Static.MyIndex].Y < (Static._MaxY - 1))
                {
                    if (checkDirection(Constant.DIR_SOUTH))
                    {
                        cMove = false;

                        // Change Direction //
                        if (dir != Constant.DIR_SOUTH)
                        {
                            ClientTCP.SendPlayerDir();
                        }
                    }
                }
                else
                {
                    // Check if they can warp to a new map
                    if (Types.Map.Down > 0)
                    {
                        ClientTCP.SendPlayerRequestNewMap();
                        //CanMoveNow = False
                    }
                    return false;
                }
                return cMove;
            }

            if (Static.DirLeft) 
            {
                Types.Players[Static.MyIndex].Dir = Constant.DIR_WEST; 
                if (Types.Players[Static.MyIndex].X > 0)
                {
                    if (checkDirection(Constant.DIR_WEST))
                    {
                        cMove = false;

                        // Change Direction //
                        if (dir != Constant.DIR_WEST)
                        {
                            ClientTCP.SendPlayerDir();
                        }
                    }
                }
                else
                {
                    // Check if they can warp to a new map
                    if (Types.Map.Left > 0)
                    {
                        ClientTCP.SendPlayerRequestNewMap();
                        //CanMoveNow = False
                    }
                    return false;
                }
                return cMove; 
            }
            if (Static.DirRight) 
            {
                Types.Players[Static.MyIndex].Dir = Constant.DIR_EAST;
                if (Types.Players[Static.MyIndex].X < (Static._MaxX - 1))
                {
                    if (checkDirection(Constant.DIR_EAST))
                    {
                        cMove = false;

                        // Change Direction //
                        if (dir != Constant.DIR_EAST)
                        {
                            ClientTCP.SendPlayerDir();
                        }
                    }
                }
                else
                {
                    // Check if they can warp to a new map
                    if (Types.Map.Right > 0)
                    {
                        ClientTCP.SendPlayerRequestNewMap();
                        //CanMoveNow = False
                    }
                    return false;
                }
                return cMove; 
            }

            // Return Final Value //
            return cMove;
        }

        public static void setMovement(byte Direction)
        {
            switch (Direction)
            {
                case Constant.DIR_NORTH: Static.DirUp = true; Static.DirDown = false; Static.DirLeft = false; Static.DirRight = false; break;
                case Constant.DIR_SOUTH: Static.DirUp = false; Static.DirDown = true; Static.DirLeft = false; Static.DirRight = false; break;
                case Constant.DIR_EAST: Static.DirUp = false; Static.DirDown = false; Static.DirLeft = false; Static.DirRight = true; break;
                case Constant.DIR_WEST: Static.DirUp = false; Static.DirDown = false; Static.DirLeft = true; Static.DirRight = false; break;
            }

        }

        public static void checkMovement()
        {
            if (Static.DirUp | Static.DirDown | Static.DirLeft | Static.DirRight)
            {
                if (canMove())
                {
                    Types.Players[Static.MyIndex].Moving = Constant.MOVING_RUNNING;

                    switch (Types.Players[Static.MyIndex].Dir)
                    {
                        case Constant.DIR_NORTH:
                            ClientTCP.SendPlayerMove();
                            Types.Players[Static.MyIndex].OffsetY = 32;
                            Types.Players[Static.MyIndex].Y = Types.Players[Static.MyIndex].Y - 1;
                            break;
                        case Constant.DIR_SOUTH:
                            ClientTCP.SendPlayerMove();
                            Types.Players[Static.MyIndex].OffsetY = 32 * -1;
                            Types.Players[Static.MyIndex].Y = Types.Players[Static.MyIndex].Y + 1;
                            break;
                        case Constant.DIR_WEST:
                            ClientTCP.SendPlayerMove();
                            Types.Players[Static.MyIndex].OffsetX = 32;
                            Types.Players[Static.MyIndex].X = Types.Players[Static.MyIndex].X - 1;
                            break;
                        case Constant.DIR_EAST:
                            ClientTCP.SendPlayerMove();
                            Types.Players[Static.MyIndex].OffsetX = 32 * -1;
                            Types.Players[Static.MyIndex].X = Types.Players[Static.MyIndex].X + 1;
                            break;
                    }
                }
            }
        }
    }
}
