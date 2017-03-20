﻿using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public partial class CommandType
    {
        public const int Invalid = 0;
        public const int SyncTurnDone = 1;
        public const int RandomTest = 2;
    }

    public class Command
    {
        protected long m_player_pstid = -1;
        protected int m_type = CommandType.Invalid;
        protected int m_syncturn = -1;
        public long PlayerPstid
        {
            get { return m_player_pstid; }
            set { m_player_pstid = value; }
        }
        public int Type
        {
            get { return m_type; }
        }
        public int SyncTurn
        {
            get { return m_syncturn; }
            set { m_syncturn = value; }
        }
        //具体数据是什么，可以固定为几个int；或者提供序列化接口，以下只是随便写写
        public int Serialize(char[] buff, int index)
        {
            return 0;
        }
        public int Unserialize(char[] buff, int index)
        {
            return 0;
        }
    }

    public class SyncTurnDoneCommand : Command
    {
        public SyncTurnDoneCommand()
        {
            m_type = CommandType.SyncTurnDone;
        }
    }

    public class RandomTestCommand : Command
    {
        public int m_random = 0;
        public RandomTestCommand()
        {
            m_type = CommandType.RandomTest;
        }
    }
}