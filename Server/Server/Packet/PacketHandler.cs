using Server;
using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

class PacketHandler
{
    public static void C_ChatHandler(PacketSession sesion, IPacket packet)
    {
        C_Chat p = packet as C_Chat;
        ClientSession clientsession = sesion as ClientSession;

        if (clientsession.Room == null)
            return;

        GameRoom room = clientsession.Room;
        room.Push(() =>
        {
            room.BroadCast(clientsession, p.chat);
        });
    }
}
