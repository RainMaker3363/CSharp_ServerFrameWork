using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ServerCore;
using DummyClient;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();

    public void Send(ArraySegment<byte> segment)
    {
        _session.Send(segment);
    }

    // Start is called before the first frame update
    void Start()
    {
        // DNS 서버를 가지고 옵니다.
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endpoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();
        connector.Connect(endpoint,
            () => { return _session; },
            1);
    }

    // Update is called once per frame
    void Update()
    {
        List<IPacket> packetList = PacketQueue.Instance.PopAll();
        foreach(IPacket packet in packetList)
        {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }
}
