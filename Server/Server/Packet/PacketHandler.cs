using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

class PacketHandler
{
    public static void C_PlayerInfoReqHandler(PacketSession sesion, IPacket packet)
    {
        C_PlayerInfoReq p = packet as C_PlayerInfoReq;

        Console.WriteLine($"playerId : {p.playerId}");

        foreach (C_PlayerInfoReq.Skill skill in p.skills)
        {
            Console.WriteLine($"Skill({skill.id}, {skill.level}, {skill.duration})");
        }
    }
}
