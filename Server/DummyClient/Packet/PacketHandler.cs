using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;
using DummyClient;

class PacketHandler
{
    public static void S_ChatHandler(PacketSession sesion, IPacket packet)
    {
        S_Chat chatpacket = packet as S_Chat;
        ServerSession serverSession = sesion as ServerSession;

        //if (chatpacket.playerid == 1)
            //Console.WriteLine(chatpacket.chat);
    }
}