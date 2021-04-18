using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using ServerCore;

public enum PacketID
{
    C_Chat = 1,
	S_Chat = 2,
	
}

interface IPacket
{
    ushort Protocol { get; }
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}




class C_Chat : IPacket
{
    public string chat;

    public ushort Protocol { get { return (ushort)PacketID.C_Chat; } }

    public C_Chat()
    {
        
    }


    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += 2;
        count += 2;

        ushort chatLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(s.Slice(count, chatLen));
		count += chatLen;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_Chat);
        count += sizeof(ushort);

        ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, openSegment.Array, openSegment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;

        // 패킷 크기를 넣어줍니다.
        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }


}


class S_Chat : IPacket
{
    public int playerid;
	public string chat;

    public ushort Protocol { get { return (ushort)PacketID.S_Chat; } }

    public S_Chat()
    {
        
    }


    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += 2;
        count += 2;

        this.playerid = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		ushort chatLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(s.Slice(count, chatLen));
		count += chatLen;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_Chat);
        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerid);
		count += sizeof(int);
		ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, openSegment.Array, openSegment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;

        // 패킷 크기를 넣어줍니다.
        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }


}


