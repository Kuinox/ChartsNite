using System;
using System.Collections.Generic;
using System.IO;

namespace Common.StreamHelpers
{
    public class SubStreamFactory : IDisposable
    {
        private readonly Stream _baseStream;
        SubStream? _previousSubStream;
        static readonly List<Stream> _factoryBaseStreams = new List<Stream>();
        /// <summary>
        /// WARNING. If you change the position while a SubStream is instancied and not Disposed, everything will burn, and your computer will try to kill you.
        /// </summary>
        public Stream BaseStream => _baseStream;
        public bool CanReadLength => _canReadLength;
        public bool CanReadPosition => _canReadPosition;
        bool _canReadLength;
        bool _canReadPosition;
        void StreamCapabilitiesTest( Stream stream )
        {
            if( stream.CanSeek )
            {
                _ = stream.Length;
                _canReadLength = true;
                _canReadPosition = true;
            }
            ///Even if some stream have the property CanSeek set to false, they may have their Length or Position available.
            try
            {
                _canReadLength = true;
            }
            catch( NotSupportedException )
            {
                _canReadLength = false;
            }
            try
            {
                _ = stream.Position;
                _canReadPosition = true;
            }
            catch( NotSupportedException )
            {
                _canReadPosition = false;
            }
        }

        /// <summary>
        /// There should be one, and only one <see cref="SubStreamFactory"/> per <see cref="Stream"/>.
        /// </summary>
        /// <param name="baseStream"></param>
        public SubStreamFactory( Stream baseStream )
        {
            _baseStream = baseStream;
            if( _factoryBaseStreams.Contains( baseStream ) )
            {
                throw new InvalidOperationException( "Cannot create two Factory for the same base Stream. You should not do that." );
            }
            StreamCapabilitiesTest( BaseStream );
            _factoryBaseStreams.Add( baseStream );
        }

        /// <summary>
        /// Return a new <see cref="SubStream"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throw if you didn't dispose the previous <see cref="SubStream"/> of this <see cref="SubStreamFactory"/></exception>
        /// <param name="length"></param>
        /// <returns></returns>
        public SubStream CreateSubstream( long length )
        {
            if( !_previousSubStream?.Disposed ?? false )
            {
                throw new InvalidOperationException( "Dispose the precedent SubStream first. I won't do it for you." );
            }
            _previousSubStream = new SubStream( BaseStream, length, _canReadLength, _canReadPosition, true );
            return _previousSubStream;
        }

        public void Dispose()
        {
            _factoryBaseStreams.Remove( _baseStream );
            BaseStream.Dispose();
        }
    }
}
