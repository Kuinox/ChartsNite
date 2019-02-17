using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.StreamHelpers
{
    public class BinaryReaderAsync : IDisposable
    {
        public readonly Stream Stream;
        readonly bool _leaveOpen;
        private readonly Func<Task>? _errorFunc;
        string? _errorDescription;
        bool _fatal;
        public BinaryReaderAsync(Stream stream, bool leaveOpen = false, Func<Task>? errorAction = null)
        {
            Stream = stream;
            _leaveOpen = leaveOpen;
            _errorFunc = errorAction;
        }

        public bool IsError => !string.IsNullOrWhiteSpace(_errorDescription) || _fatal;

        public bool Fatal => _fatal;
        public string? ErrorMessage => _errorDescription;
        public bool EndOfStream { get; private set; }

        public bool AssertRemainingCountOfBytes(int length)
        {
            return Stream.Length - Stream.Position == length;
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
        public async Task<bool> AddError(object? errorMessage = null, bool beforeExisting = false, bool fatal = false, [CallerMemberName]string? callerName = null)
        {
            if(_errorFunc != null)
            {
                await _errorFunc();
            }
            if (fatal) {
                SetFatal();
            }
            if (_errorDescription != null)
            {
                if (beforeExisting)
                {
                    _errorDescription = FormatMessage(errorMessage, callerName) + Environment.NewLine + "<-- " + _errorDescription;
                }
                else
                {
                    _errorDescription = _errorDescription + Environment.NewLine + "<-- " + FormatMessage(errorMessage, callerName);
                }
            }
            else
            {
                _errorDescription = FormatMessage(errorMessage, callerName);
            }
            return false;
        }
        static string? FormatMessage(object? expectedMessage, string? callerName)
        {
            string? d = callerName;
            string? tail = expectedMessage?.ToString();
            if (!string.IsNullOrEmpty(tail))
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
        public async Task<byte[]> ReadBytes(int count)
        {
            byte[] buffer = new byte[count];
            int toRead = count;
            while (toRead > 0)
            {
                int read = await Stream.ReadAsync(buffer, count - toRead, count);
                if (read == 0)
                {
                    EndOfStream = true;
                    await AddError("No more bytes to read", true, true);
                    break;
                }
                toRead -= read;
            }
            return buffer;
        }
        #region ReadNumbers
        public async Task<uint> ReadUInt32() => BitConverter.ToUInt32(await ReadBytes(4), 0);
        public async Task<int> ReadInt32() => BitConverter.ToInt32(await ReadBytes(4), 0);
        public async Task<byte> ReadByteOnce() => (await ReadBytes(1))[0];

        public async Task<float> ReadSingle() => BitConverter.ToSingle(await ReadBytes(4), 0);
        public async Task<short> ReadInt16() => BitConverter.ToInt16(await ReadBytes(2), 0);
        public async Task<long> ReadInt64() => BitConverter.ToInt64(await ReadBytes(8), 0);
        #endregion ReadNumbers
        #region ReadString
        public async Task<string> ReadString()
        {
            int length = await ReadInt32();
            if (length > Stream.Length + Stream.Position || length<0 && -length > Stream.Length + Stream.Position)
            {
                await AddError("The size of the string was bigger than the stream. Probably not a string.");
                return "";
            }
            if (length == 0)
            {
                return "";
            }

            bool isUnicode = length < 0;
            byte[] data;
            string value;
            if (isUnicode)
            {
                length = -length;
                data = await ReadBytes(length * 2);
                value = Encoding.Unicode.GetString(data);
            }
            else
            {
                data = await ReadBytes(length);
                value = Encoding.Default.GetString(data);
            }
            return value.Trim(' ', '\0');
        }

        public void Dispose()
        {
            if(!_leaveOpen)
            {
                Stream.Dispose();
            }
        }
        #endregion ReadString
    }
}
