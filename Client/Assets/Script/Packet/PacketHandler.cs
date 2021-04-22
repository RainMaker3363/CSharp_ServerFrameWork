using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;
using DummyClient;
using UnityEngine;

class PacketHandler
{
    public static void S_BroadcastEnterGameHandler(PacketSession sesion, IPacket packet)
    {
        S_BroadcastEnterGame Broadpacket = packet as S_BroadcastEnterGame;
        ServerSession serverSession = sesion as ServerSession;

        PlayerManager.Instance.EnterGame(Broadpacket);
    }

    public static void S_BroadCastLeaveGameHandler(PacketSession sesion, IPacket packet)
    {
        S_BroadCastLeaveGame Leavepacket = packet as S_BroadCastLeaveGame;
        ServerSession serverSession = sesion as ServerSession;

        PlayerManager.Instance.LeaveGame(Leavepacket);
    }

    public static void S_PlayerListHandler(PacketSession sesion, IPacket packet)
    {
        S_PlayerList Listpacket = packet as S_PlayerList;
        ServerSession serverSession = sesion as ServerSession;

        PlayerManager.Instance.Add(Listpacket);
    }

    public static void S_BroadCastMoveHandler(PacketSession sesion, IPacket packet)
    {
        S_BroadCastMove BroadMovepacket = packet as S_BroadCastMove;
        ServerSession serverSession = sesion as ServerSession;

        PlayerManager.Instance.Move(BroadMovepacket);
    }
}