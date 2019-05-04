using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnrealReplayParser.UnrealObject;
using UnrealReplayParser.UnrealObject.Types;
using static UnrealReplayParser.DemoHeader;

namespace Common.StreamHelpers
{
    public class CustomBinaryReader : BinaryReader, IAsyncDisposable, IDisposable
    {
        readonly bool _leaveOpen;
        public CustomBinaryReader( Stream stream, bool leaveOpen = false ) : base( stream, new UTF8Encoding(), leaveOpen )
        {
            _leaveOpen = leaveOpen;
        }


        public byte[] DumpRemainingBytes()
        {
            return ReadBytes( (int)(BaseStream.Length - BaseStream.Position) );
        }
        public byte ReadOneByte() => ReadBytes( 1 )[0];

        public override string ReadString()
        {
            int length = ReadInt32();
            if( length == -2147483648 )//if we reverse this, it has an
            {
                throw new InvalidDataException( "The size of the string has an invalid value" );
            }
            if( length == 0 )
            {
                return "";
            }

            bool isUnicode = length < 0;
            byte[] data;
            string value;
            if( isUnicode )
            {
                length = -length;
                data = ReadBytes( length * 2 );
                value = Encoding.Unicode.GetString( data );
            }
            else
            {
                data = ReadBytes( length );
                value = Encoding.Default.GetString( data );
            }
            return value.Trim( ' ', '\0' );
        }

        /// <summary>
        /// In UnrealEngine source code: void FArchive::SerializeIntPacked( uint32& Value )
        /// </summary>
        /// <returns></returns>
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


        public StaticName ReadStaticName( EngineNetworkVersionHistory versionHistory )
        {
            byte b = ReadByte();
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

        public NetFieldExport ReadNetFieldExport( EngineNetworkVersionHistory versionHistory )
        {
            byte flags = ReadByte();
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


        public async ValueTask DisposeAsync()
        {
            if( !_leaveOpen )
            {
                await BaseStream.DisposeAsync();
            }
        }
    }
}
