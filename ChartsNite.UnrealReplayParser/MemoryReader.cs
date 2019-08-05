using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnrealReplayParser.UnrealObject;
using static System.Buffers.Binary.BinaryPrimitives;
using static UnrealReplayParser.DemoHeader;

public enum Endianness
{
    Native,
    Little,
    Big
}

class MemoryReader
{
    readonly Memory<byte> _baseMemory;

    public Endianness Endianness { get; set; }

    public int Length => _baseMemory.Length;
    public int Offset { get; set; }

    public bool EndOfSpan => Offset >= _baseMemory.Length;

    public Memory<byte> Slice => _baseMemory.Slice( Offset );

    public Memory<byte> ReadBytes( int length )
    {
        Memory<byte> output = Slice[0..length];
        Offset += length;
        return output;
    }


    public MemoryReader( Memory<byte> memory, Endianness endianness )
    {
        Endianness = endianness;
        _baseMemory = memory;
        Offset = 0;
    }

    Endianness CurrentEndianness
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        get
        {
            if( Endianness == Endianness.Native )
                return BitConverter.IsLittleEndian ? Endianness.Little : Endianness.Big;
            return Endianness;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryReadInt16( out short result )
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadInt16LittleEndian( Slice.Span, out result )
            : TryReadInt16BigEndian( Slice.Span, out result );

        if( success )
            Offset += sizeof( short );

        return success;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryReadInt32( out int result )
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadInt32LittleEndian( Slice.Span, out result )
            : TryReadInt32BigEndian( Slice.Span, out result );

        if( success )
            Offset += sizeof( int );

        return success;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryReadInt64( out long result )
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadInt64LittleEndian( Slice.Span, out result )
            : TryReadInt64BigEndian( Slice.Span, out result );

        if( success )
            Offset += sizeof( long );

        return success;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryReadUInt16( out ushort result )
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadUInt16LittleEndian( Slice.Span, out result )
            : TryReadUInt16BigEndian( Slice.Span, out result );

        if( success )
            Offset += sizeof( ushort );

        return success;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryReadUInt32( out uint result )
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadUInt32LittleEndian( Slice.Span, out result )
            : TryReadUInt32BigEndian( Slice.Span, out result );

        if( success )
            Offset += sizeof( uint );

        return success;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryReadUInt64( out ulong result )
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadUInt64LittleEndian( Slice.Span, out result )
            : TryReadUInt64BigEndian( Slice.Span, out result );

        if( success )
            Offset += sizeof( ulong );

        return success;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryReadSingle( out float result )
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadUInt32LittleEndian( Slice.Span, out uint value )
            : TryReadUInt32BigEndian( Slice.Span, out value );

        if( success )
            Offset += sizeof( uint );

        result = Unsafe.As<uint, float>( ref value );
        return success;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool TryReadDouble( out double result )
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadUInt64LittleEndian( Slice.Span, out ulong value )
            : TryReadUInt64BigEndian( Slice.Span, out value );

        if( success )
            Offset += sizeof( ulong );

        result = Unsafe.As<ulong, double>( ref value );
        return success;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public short PeekInt16()
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadInt16LittleEndian( Slice.Span, out short result )
            : TryReadInt16BigEndian( Slice.Span, out result );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int PeekInt32()
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadInt32LittleEndian( Slice.Span, out int result )
            : TryReadInt32BigEndian( Slice.Span, out result );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public long PeekInt64()
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadInt64LittleEndian( Slice.Span, out long result )
            : TryReadInt64BigEndian( Slice.Span, out result );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ushort PeekUInt16()
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadUInt16LittleEndian( Slice.Span, out ushort result )
            : TryReadUInt16BigEndian( Slice.Span, out result );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public uint PeekUInt32()
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadUInt32LittleEndian( Slice.Span, out uint result )
            : TryReadUInt32BigEndian( Slice.Span, out result );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ulong PeekUInt64()
    {
        bool success = CurrentEndianness == Endianness.Little
            ? TryReadUInt64LittleEndian( Slice.Span, out ulong result )
            : TryReadUInt64BigEndian( Slice.Span, out result );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public float PeekSingle()
    {
        uint value = PeekUInt32();
        return Unsafe.As<uint, float>( ref value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public double PeekDouble()
    {
        ulong value = PeekUInt64();
        return Unsafe.As<ulong, double>( ref value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public short ReadInt16()
    {
        short result = CurrentEndianness == Endianness.Little
            ? ReadInt16LittleEndian( Slice.Span )
            : ReadInt16BigEndian( Slice.Span );
        Offset += sizeof( short );
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int ReadInt32()
    {
        int result = CurrentEndianness == Endianness.Little
            ? ReadInt32LittleEndian( Slice.Span )
            : ReadInt32BigEndian( Slice.Span );
        Offset += sizeof( int );
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public long ReadInt64()
    {
        long result = CurrentEndianness == Endianness.Little
            ? ReadInt64LittleEndian( Slice.Span )
            : ReadInt64BigEndian( Slice.Span );
        Offset += sizeof( long );
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ushort ReadUInt16()
    {
        ushort result = CurrentEndianness == Endianness.Little
            ? ReadUInt16LittleEndian( Slice.Span )
            : ReadUInt16BigEndian( Slice.Span );
        Offset += sizeof( ushort );
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public uint ReadUInt32()
    {
        uint result = CurrentEndianness == Endianness.Little
            ? ReadUInt32LittleEndian( Slice.Span )
            : ReadUInt32BigEndian( Slice.Span );
        Offset += sizeof( uint );
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ulong ReadUInt64()
    {
        ulong result = CurrentEndianness == Endianness.Little
            ? ReadUInt64LittleEndian( Slice.Span )
            : ReadUInt64BigEndian( Slice.Span );

        Offset += sizeof( ulong );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public float ReadSingle()
    {
        uint value = ReadUInt32();
        return Unsafe.As<uint, float>( ref value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public double ReadDouble()
    {
        ulong value = ReadUInt64();
        return Unsafe.As<ulong, double>( ref value );
    }


    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public byte ReadOneByte()
    {
        byte output = _baseMemory.Span[Offset];
        Offset++;
        return output;
    }
}
