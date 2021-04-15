using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using ServerCore;

public enum PacketID
{
    C_PlayerInfoReq = 1,
	S_Test = 2,
	
}

interface IPacket
{
    ushort Protocol { get; }
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}




class C_PlayerInfoReq : IPacket
{
    public byte testByte;
	public long playerId;
	public string name;
	
	public struct Skill
	{
	    public int id;
		public short level;
		public float duration;
	
	    public void Read(ReadOnlySpan<byte> s, ref ushort count)
	    {
	        this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
			count += sizeof(short);
			this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
	    }
	
	    public bool Write(Span<byte> s, ref ushort count)
	    {
	        bool success = true;
	
	        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
			count += sizeof(short);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
			count += sizeof(float);
	
	        return success;
	    }
	}
	
	public List<Skill> skills = new List<Skill>();
	

    public ushort Protocol { get { return (ushort)PacketID.C_PlayerInfoReq; } }

    public C_PlayerInfoReq()
    {
        
    }


    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += 2;
        count += 2;

        this.testByte = (byte)segment.Array[segment.Offset + count];
		count += sizeof(byte);
		
		this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
		count += sizeof(long);
		ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
		count += nameLen;
		this.skills.Clear();
		ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for(int i = 0; i< skillLen; ++i)
		{
		    Skill skill = new Skill();
		    skill.Read(s, ref count);
		    skills.Add(skill);
		}
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.C_PlayerInfoReq);
        count += sizeof(ushort);

        openSegment.Array[openSegment.Offset + count] = (byte)this.testByte;
		count += sizeof(byte);
		
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(long);
		ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, openSegment.Array, openSegment.Offset + count + sizeof(ushort));
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
		count += sizeof(ushort);
		count += nameLen;
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
		count += sizeof(ushort);
		foreach(Skill skill in skills)
		{
		    success &= skill.Write(s, ref count);
		}

        // 패킷 크기를 넣어줍니다.
        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }


}


class S_Test : IPacket
{
    public int TestInt;

    public ushort Protocol { get { return (ushort)PacketID.S_Test; } }

    public S_Test()
    {
        
    }


    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += 2;
        count += 2;

        this.TestInt = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(openSegment.Array, openSegment.Offset, openSegment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.S_Test);
        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.TestInt);
		count += sizeof(int);

        // 패킷 크기를 넣어줍니다.
        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }


}


