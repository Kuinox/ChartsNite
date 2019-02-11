using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Common.StreamHelpers
{
    public class SubStreamFactory
    {
        readonly Stream _baseStream;
        SubStream? _previousSubStream;
        static readonly List<Stream> _factoryBaseStreams = new List<Stream>();
        /// <summary>
        /// There should be one, and only one <see cref="SubStreamFactory"/> per <see cref="Stream"/>.
        /// </summary>
        /// <param name="baseStream"></param>
        public SubStreamFactory(Stream baseStream)
        {
            if(_factoryBaseStreams.Contains(baseStream))
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
        public async Task<SubStream> Create(long length, bool leaveOpen = false)
        {
            SubStream newSubStream = new SubStream(_baseStream, length, leaveOpen);
            SubStream? previousSubStream = Interlocked.Exchange(ref _previousSubStream, newSubStream);//There should be no null after this.
            if (previousSubStream == null) return newSubStream;//We can't return null.
            if (!previousSubStream.Disposed)
            {
                throw new InvalidOperationException("Dispose the precedent SubStream first. I won't do it for you.");
            }
            await previousSubStream.DisposeAsync();
            return newSubStream;
        }
    }
}
