using Server;
using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

class PacketHandler
{
    public static void C_LeaveGameHandler(PacketSession sesion, IPacket packet)
    {
        ClientSession clientsession = sesion as ClientSession;

        if (clientsession.Room == null)
            return;

        GameRoom room = clientsession.Room;
        room.Push(() =>
        {
            //room.BroadCast(clientsession, p.chat);
            room.Leave(clientsession);
        });
    }
    public static void C_MoveHandler(PacketSession sesion, IPacket packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientsession = sesion as ClientSession;

        if (clientsession.Room == null)
            return;

        //Console.WriteLine($"{movePacket.posX}, {movePacket.posY}, {movePacket.posZ}");

        GameRoom room = clientsession.Room;
        room.Push(() =>
        {
            //room.BroadCast(clientsession, p.chat);
            room.Move(clientsession, movePacket);
        });
    }
    
}
