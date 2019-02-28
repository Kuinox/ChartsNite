using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Common.StreamHelpers
{
    public class SubStreamFactory : IDisposable
    {
        private readonly Stream _baseStream;
        SubStream? _previousSubStream;
        static readonly List<Stream> _factoryBaseStreams = new List<Stream>();
        /// <summary>
        /// WARNING. If you change the position while a SubStream, everything will burn, and your computer will try to kill you.
        /// </summary>
        public Stream BaseStream => _baseStream;

        /// <summary>
        /// There should be one, and only one <see cref="SubStreamFactory"/> per <see cref="Stream"/>.
        /// </summary>
        /// <param name="baseStream"></param>
        public SubStreamFactory(Stream baseStream)
        {
            if (_factoryBaseStreams.Contains(baseStream))
            {
                throw new InvalidOperationException("Cannot create two Factory for the same base Stream. You should not do that.");
            }
            _factoryBaseStreams.Add(baseStream);
            _baseStream = baseStream;
        }

        /// <summary>
        /// Return a new <see cref="SubStream"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throw if you didn't dispose the previous <see cref="SubStream"/> of this <see cref="SubStreamFactory"/></exception>
        /// <param name="length"></param>
        /// <param name="leaveOpen"></param>
        /// <returns></returns>
        public SubStream Create(long length, bool leaveOpen = false)
        {
            if (_previousSubStream != null)
            {
                if (!_previousSubStream.Disposed)
                {
                    throw new InvalidOperationException("Dispose the precedent SubStream first. I won't do it for you.");
                }
            }
            _previousSubStream = new SubStream(BaseStream, length, leaveOpen);
            return _previousSubStream;
        }

        public void Dispose()
        {
            _factoryBaseStreams.Remove( _baseStream );
            BaseStream.Dispose();
        }
    }
}
