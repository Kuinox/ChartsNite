using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.StreamHelpers
{
    public class CustomBinaryReaderAsync : IAsyncDisposable
    {
        public readonly Stream BaseStream;
        readonly bool _leaveOpen;
        private readonly Func<Task>? _errorFunc;
        string? _errorDescription;
        bool _fatal;
        bool _errorReported = true;
        public CustomBinaryReaderAsync( Stream stream, bool leaveOpen = false, Func<Task>? errorAction = null )
        {
            BaseStream = stream;
            _leaveOpen = leaveOpen;
            _errorFunc = errorAction;
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
        public async Task<bool> AddError( object? errorMessage = null, bool beforeExisting = false, bool fatal = false, [CallerMemberName]string? callerName = null )
        {
            if( !EndOfStream )
            {

            }
            _errorReported = false;
            if( _errorFunc != null )
            {
                await _errorFunc();
            }
            if( fatal )
            {
                SetFatal();
            }
            if( _errorDescription != null )
            {
                if( beforeExisting )
                {
                    _errorDescription = FormatMessage( errorMessage, callerName ) + Environment.NewLine + "<-- " + _errorDescription;
                }
                else
                {
                    _errorDescription = _errorDescription + Environment.NewLine + "<-- " + FormatMessage( errorMessage, callerName );
                }
            }
            else
            {
                _errorDescription = FormatMessage( errorMessage, callerName );
            }
            return false;
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
        public async Task<byte[]> ReadBytes( int count )
        {
            byte[] buffer = new byte[count];
            int toRead = count;
            while( toRead > 0 )
            {
                int read = await BaseStream.ReadAsync( buffer, count - toRead, count );
                if( read == 0 )
                {
                    if( EndOfStream )
                    {
                        await AddError( "No more bytes to read", true, true );
                        break;
                    }
                    EndOfStream = true;
                    break;
                }
                toRead -= read;
            }
            return buffer;
        }

        public Task<byte[]> DumpRemainingBytes()
        {
            return ReadBytes( (int)(BaseStream.Length - BaseStream.Position) );
        }
        #region ReadNumbers
        public async ValueTask<uint> ReadUInt32() => BitConverter.ToUInt32( await ReadBytes( 4 ), 0 );
        public async Task<int> ReadInt32() => BitConverter.ToInt32( await ReadBytes( 4 ), 0 );
        public async Task<byte> ReadOneByte() => (await ReadBytes( 1 ))[0];

        public async Task<float> ReadSingle() => BitConverter.ToSingle( await ReadBytes( 4 ), 0 );
        public async Task<short> ReadInt16() => BitConverter.ToInt16( await ReadBytes( 2 ), 0 );
        public async Task<ushort> ReadUInt16() => BitConverter.ToUInt16( await ReadBytes( 2 ), 0 );
        public async Task<long> ReadInt64() => BitConverter.ToInt64( await ReadBytes( 8 ), 0 );



        #endregion ReadNumbers

        #region ReadStructs
        public async ValueTask<string> ReadString()
        {
            int length = await ReadInt32();
            if( length == -2147483648 )//if we reverse this, it has an
            {
                await AddError( "The size of the string has an invalid value" );
                return "";
            }
            if( length > BaseStream.Length + BaseStream.Position || length < 0 && -length > BaseStream.Length + BaseStream.Position )
            {
                await AddError( "The size of the string was bigger than the stream. Probably not a string." );
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
                data = await ReadBytes( length * 2 );
                value = Encoding.Unicode.GetString( data );
            }
            else
            {
                data = await ReadBytes( length );
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
        public async Task<uint> ReadIntPacked()
        {
            uint value = 0;
            byte count = 0;
            bool more = true;

            while( more )
            {
                byte nextByte = await ReadOneByte();
                more = (nextByte & 1) == 1;         // Check 1 bit to see if theres more after this
                nextByte >>= 1;           // Shift to get actual 7 bit value
                value += (uint)nextByte << (7 * count++); // Add to total value
            }
            return value;
        }

        public async Task<NetFieldExport> ReadNetFieldExport()
        {
            bool exported = 1 == await ReadOneByte();
            if( !exported )
            {
                return NetFieldExport.InitializeNotExported();
            }
            uint handle = await ReadIntPacked();
            uint compatibleChecksum = await ReadUInt32();
            string name = await ReadString();
            string type = await ReadString();
            return NetFieldExport.InitializeExported( handle, compatibleChecksum, name, type );
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

        #endregion ReadStructs
    }
}
