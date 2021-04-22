using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    MyPlayer _Myplayer;
    Dictionary<int, Player> _dic_Players = new Dictionary<int, Player>();

    public static PlayerManager Instance { get; } = new PlayerManager();

    public void Add(S_PlayerList packet)
    {
        Object obj = Resources.Load("Player");

       
        foreach(var p in packet.players)
        {
            GameObject go = Object.Instantiate(obj) as GameObject;

            if(p.isSelf)
            {
                MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                myPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                myPlayer.PlayerID = p.playerId;
                _Myplayer = myPlayer;
            }
            else
            {
                Player player = go.AddComponent<Player>();
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                player.PlayerID = p.playerId;

                _dic_Players.Add(p.playerId, player);
            }
        }
    }

    public void Move(S_BroadCastMove packet)
    {
        if (_Myplayer.PlayerID == packet.playerId)
        {
            _Myplayer.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
        }
        else
        {
            Player player = null;
            if (_dic_Players.TryGetValue(packet.playerId, out player))
            {
                player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
            }

        }
    }

    public void EnterGame(S_BroadcastEnterGame packet)
    {
        if (packet.playerId == _Myplayer.PlayerID)
            return;

        Object obj = Resources.Load("Player");
        GameObject go = Object.Instantiate(obj) as GameObject;

        Player player = go.AddComponent<Player>();
        player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);

        _dic_Players.Add(packet.playerId, player);
    }

    public void LeaveGame(S_BroadCastLeaveGame packet)
    {
        if(_Myplayer.PlayerID == packet.playerId)
        {
            GameObject.Destroy(_Myplayer.gameObject);
            _Myplayer = null;
        }
        else
        {
            Player player = null;
            if(_dic_Players.TryGetValue(packet.playerId, out player))
            {
                GameObject.Destroy(player.gameObject);
                _dic_Players.Remove(packet.playerId);
            }

        }
    }
}
