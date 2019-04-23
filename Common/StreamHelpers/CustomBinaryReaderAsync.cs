using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.StreamHelpers
{
    public class CustomBinaryReaderAsync : IAsyncDisposable, IDisposable
    {
        public readonly Stream BaseStream;
        readonly bool _leaveOpen;
        string? _errorDescription;
        bool _fatal;
        bool _errorReported = true;
        public CustomBinaryReaderAsync( Stream stream, bool leaveOpen = false)
        {
            BaseStream = stream;
            _leaveOpen = leaveOpen;
        }

        public bool IsError => !string.IsNullOrWhiteSpace( _errorDescription ) || _fatal;
        public void SetErrorReported() => _errorReported = true;
        public bool Fatal => _fatal;
        public string? ErrorMessage => _errorDescription;
        bool _endOfStream;
        public bool EndOfStream
        {
            get => BaseStream.Position == BaseStream.Length || _endOfStream;
            private set => _endOfStream = value;
        }

        public bool AssertRemainingCountOfBytes( int length )
        {
            return BaseStream.Length - BaseStream.Position == length;
        }
        public void SetFatal()
        {
            _fatal = true;
        }

        /// <summary>
        /// Adds an error (the message starts with the caller's method name) to the existing ones (if any).
        /// </summary>
        /// <param name="expectedMessage">
        /// Optional object. Its <see cref="object.ToString()"/> will be used to generate an "expected '...'" message.
        /// </param>
        /// <param name="beforeExisting">
        /// True to add the error before the existing ones (as a consequence: [added] &lt;-- [previous]), 
        /// false to append it (as a cause: [previous] &lt;-- [added])</param>
        /// <param name="callerName">Name of the caller (automatically injected by the compiler).</param>
        /// <returns>Always false to use it as the return statement in a match method.</returns>
        public bool AddError( object? errorMessage = null, bool beforeExisting = false, bool fatal = false, [CallerMemberName]string? callerName = null )
        {
            throw new InvalidOperationException();
            //_errorReported = false;
            //if( fatal )
            //{
            //    SetFatal();
            //}
            //if( _errorDescription != null )
            //{
            //    if( beforeExisting )
            //    {
            //        _errorDescription = FormatMessage( errorMessage, callerName ) + Environment.NewLine + "<-- " + _errorDescription;
            //    }
            //    else
            //    {
            //        _errorDescription = _errorDescription + Environment.NewLine + "<-- " + FormatMessage( errorMessage, callerName );
            //    }
            //}
            //else
            //{
            //    _errorDescription = FormatMessage( errorMessage, callerName );
            //}
            //return false;
        }
        static string? FormatMessage( object? expectedMessage, string? callerName )
        {
            string? d = callerName;
            string? tail = expectedMessage?.ToString();
            if( !string.IsNullOrEmpty( tail ) )
            {
                d += ": expected '" + tail + "'.";
            }
            return d;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns>Asked bytes, then zeros if there was no enough, in this case, the <see cref="BinaryReader"/></returns>
        public async ValueTask<Memory<byte>> ReadBytesAsync( int count )
        {
            if(count<0)
            {
                throw new ArgumentException("Cannot read a negative amount.");
            }
            var buffer = new Memory<byte>(new byte[count]);
            int toRead = count;
            while( toRead > 0 )
            {
                int read = await BaseStream.ReadAsync( buffer[Range.StartAt( count - toRead )]);
                if( read == 0 )
                {
                    if( EndOfStream )
                    {
                        AddError( "No more bytes to read", true, true );
                        break;
                    }
                    EndOfStream = true;
                    break;
                }
                toRead -= read;
            }
            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns>Asked bytes, then zeros if there was no enough, in this case, the <see cref="BinaryReader"/></returns>
        public byte[] ReadBytes( int count )
        {
            if( count < 0 )
            {
                throw new ArgumentException( "Cannot read a negative amount." );
            }
            byte[] buffer = new byte[count];
            int toRead = count;
            while( toRead > 0 )
            {
                int read = BaseStream.Read( buffer, count - toRead, toRead );
                if( read == 0 )
                {
                    if( EndOfStream )
                    {
                        AddError( "No more bytes to read", true, true );
                        break;
                    }
                    EndOfStream = true;
                    break;
                }
                toRead -= read;
            }
            return buffer;
        }

        public async ValueTask<byte[]> DumpRemainingBytesAsync()
        {
            return (await ReadBytesAsync( (int)(BaseStream.Length - BaseStream.Position))).ToArray();
        }

        public byte[] DumpRemainingBytes()
        {
            return ReadBytes( (int)(BaseStream.Length - BaseStream.Position));
        }
        #region ReadNumbers
        public async ValueTask<uint> ReadUInt32Async() => BitConverter.ToUInt32( (await ReadBytesAsync( 4 )).Span );
        public uint ReadUInt32() => BitConverter.ToUInt32( ReadBytes( 4 ), 0 );
        public async ValueTask<int> ReadInt32Async() => BitConverter.ToInt32( (await ReadBytesAsync( 4 )).Span );
        public int ReadInt32() => BitConverter.ToInt32( ReadBytes( 4 ), 0 );
        public async ValueTask<byte> ReadOneByteAsync() => (await ReadBytesAsync( 1 )).Span[0];
        public byte ReadOneByte() => ReadBytes( 1 )[0];
        public async ValueTask<float> ReadSingleAsync() => BitConverter.ToSingle( (await ReadBytesAsync( 4 )).Span );
        public float ReadSingle() => BitConverter.ToSingle( ReadBytes( 4 ), 0 );

        public async ValueTask<short> ReadInt16() => BitConverter.ToInt16( (await ReadBytesAsync( 2 )).Span );
        public async ValueTask<ushort> ReadUInt16() => BitConverter.ToUInt16( (await ReadBytesAsync( 2 )).Span );
        public async ValueTask<long> ReadInt64Async() => BitConverter.ToInt64( (await ReadBytesAsync( 8 )).Span );
        public long ReadInt64() => BitConverter.ToInt64( ReadBytes( 8 ), 0 );


        #endregion ReadNumbers

        #region ReadStructs
        public async ValueTask<string> ReadStringAsync()
        {
            int length = await ReadInt32Async();
            if( length == -2147483648 )//if we reverse this, it overflow
            {
                AddError( "The size of the string has an invalid value" );
                return "";
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
                value = Encoding.Unicode.GetString( (await ReadBytesAsync( length * 2 )).Span );
            }
            else
            {
                value = Encoding.Default.GetString( (await ReadBytesAsync( length )).Span );
            }
            return value.Trim( ' ', '\0' );
        }

        public string ReadString()
        {
            int length = ReadInt32();
            if( length == -2147483648 )//if we reverse this, it has an
            {
                AddError( "The size of the string has an invalid value" );
                return "";
            }
            if( length > BaseStream.Length + BaseStream.Position || length < 0 && -length > BaseStream.Length + BaseStream.Position )
            {
                AddError( "The size of the string was bigger than the stream. Probably not a string." );
                return "";
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
        public class NetFieldExport
        {
            NetFieldExport( bool exported, uint handle, uint compatibleChecksum, string name, string type )
            {
                Exported = exported;
                Handle = handle;
                CompatibleChecksum = compatibleChecksum;
                Name = name;
                Type = type;
            }
            public static NetFieldExport InitializeNotExported() => new NetFieldExport( false, 0, 0, "", "" );
            public static NetFieldExport InitializeExported( uint handle, uint compatibleChecksum, string name, string type ) => new NetFieldExport( true, handle, compatibleChecksum, name, type );

            public bool Exported { get; set; }
            public uint Handle { get; set; }
            public uint CompatibleChecksum { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
        }


        /// <summary>
        /// In UnrealEngine source code: void FArchive::SerializeIntPacked( uint32& Value )
        /// </summary>
        /// <returns></returns>
        public async ValueTask<uint> ReadIntPackedAsync()
        {
            uint value = 0;
            byte count = 0;
            bool more = true;

            while( more )
            {
                byte nextByte = await ReadOneByteAsync();
                more = (nextByte & 1) == 1;         // Check 1 bit to see if theres more after this
                nextByte >>= 1;           // Shift to get actual 7 bit value
                value += (uint)nextByte << (7 * count++); // Add to total value
            }
            return value;
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

        

        public async ValueTask DisposeAsync()
        {
            if( !_errorReported )
            {
                throw new InvalidOperationException( "You must report errors before Diposing" );
            }
            if( !_leaveOpen )
            {
                await BaseStream.DisposeAsync();
            }
        }

        public void Dispose()
        {
            if( !_errorReported )
            {
                throw new InvalidOperationException( "You must report errors before Diposing" );
            }
            if( !_leaveOpen )
            {
                BaseStream.Dispose();
            }
        }

        #endregion ReadStructs
    }
}
