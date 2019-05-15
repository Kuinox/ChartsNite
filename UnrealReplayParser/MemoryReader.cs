using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnrealReplayParser.UnrealObject;
using UnrealReplayParser.UnrealObject.Types;
using static System.Buffers.Binary.BinaryPrimitives;
using static UnrealReplayParser.DemoHeader;

public enum Endianness
{
    Native,
    Little,
    Big
}

public class MemoryReader
{
    readonly Memory<byte> _baseMemory;

    public Endianness Endianness { get; set; }

    public int Length => _baseMemory.Length;
    public int Offset { get; set; }

    public Memory<byte> Slice => _baseMemory.Slice( Offset );

    public Memory<byte> ReadBytes( int length )
    {
        Memory<byte> output = Slice[..length];
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




    /// <summary>
    /// In UnrealEngine source code: void FArchive::SerializeIntPacked( uint32& Value )
    /// </summary>
    /// <returns></returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public uint ReadIntPacked()
    {
        uint value = 0;
        byte count = 0;
        bool more = true;

        while( more )
        {
            byte nextByte = ReadOneByte();
            more = (nextByte & 1) == 1;         // Check 1 bit to see if theres more after this
            nextByte >>= 1;           // Shift to get actual 7 bit value
            value += (uint)nextByte << (7 * count++); // Add to total value
        }
        return value;
    }

    public StaticName ReadStaticName( EngineNetworkVersionHistory versionHistory )
    {
        byte b = ReadOneByte();
        bool hardcoded = b != 0;
        if( hardcoded )
        {
            if( versionHistory < EngineNetworkVersionHistory.HISTORY_CHANNEL_NAMES )
            {
                return new StaticName( ReadUInt32(), "", true );
            }
            else
            {
                return new StaticName( ReadIntPacked(), "", true );
            }
            //hard coded names in "UnrealNames.inl"
        }
        else
        {
            string inString = ReadString();
            int inNumber = ReadInt32();
            return new StaticName( (uint)inNumber, inString, false );
        }
    }
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public byte ReadOneByte()
    {
        byte output = _baseMemory.Span[Offset];
        Offset++;
        return output;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public string ReadString()
    {
        int length = ReadInt32();
        if( length == -2147483648 )//if we reverse this, it overflow
        {
            throw new InvalidDataException( "The size of the string has an invalid value" );
        }
        if( length == 0 )
        {
            return "";
        }
        bool isUnicode = length < 0;
        string value;
        if( isUnicode )
        {
            length = -length;
            
            value = Encoding.Unicode.GetString( ReadBytes( length * 2 - 2 ).Span );
            Offset += 2;
        }
        else
        {
            value = Encoding.ASCII.GetString( ReadBytes( length - 1 ).Span );
            Offset += 1;
        }
        return value;
    }

    public T[] ReadSparseArray<T>( Func<T> baseTypeParser )
    {
        int newNumElement = ReadInt32();
        Debug.Assert( newNumElement >= 0 );
        T[] output = new T[newNumElement];
        for( int i = 0; i < newNumElement; i++ )
        {
            output[i] = baseTypeParser();
        }
        return output;
    }

    public T[] ReadArray<T>( Func<T> baseTypeParser )
    {
        int length = ReadInt32();
        Debug.Assert( length >= 0 );
        T[] output = new T[length];
        for( int i = 0; i < length; i++ )
        {
            output[i] = baseTypeParser();
        }
        return output;
    }
    public NetFieldExport ReadNetFieldExport( EngineNetworkVersionHistory versionHistory )
    {
        byte flags = ReadOneByte();
        bool exported = 1 == flags;
        if( !exported )
        {
            return NetFieldExport.InitializeNotExported();
        }
        uint handle = ReadIntPacked();
        uint compatibleChecksum = ReadUInt32();

        string name;
        string type;
        if( versionHistory < EngineNetworkVersionHistory.HISTORY_NETEXPORT_SERIALIZATION )
        {
            name = ReadString();
            type = ReadString();
        }
        else
        {
            if( versionHistory < EngineNetworkVersionHistory.HISTORY_NETEXPORT_SERIALIZE_FIX )
            {
                throw new NotImplementedException();
            }
            else
            {
                var staticName = ReadStaticName( versionHistory );
                name = staticName.Name;
                type = "";
            }
        }
        return NetFieldExport.InitializeExported( handle, compatibleChecksum, name, type );
    }
}
