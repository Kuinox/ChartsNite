using System;
using System.Collections.Generic;
using System.Text;

namespace UnrealReplayParser.Archive
{
    interface IMemoryArchive
    {
        int Length { get; }
        int Offset { get; set; }

        Memory<byte> ReadBytes( int count );
        byte ReadOneByte();
        uint ReadIntPacked();
        double ReadDouble();
        float ReadSingle();
        ulong ReadUInt64();
        long ReadInt64();
        int ReadInt32();
        uint ReadUInt32();
        short ReadInt16();
        ushort ReadUInt16();
    }
}
