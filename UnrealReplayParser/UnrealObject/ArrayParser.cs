using Common.StreamHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace UnrealReplayParser.UnrealObject
{
    public class ArrayParser<TParsedValue,TParser> : IParser<TParsedValue[]> where TParser : IParser<TParsedValue>
    {
        readonly CustomBinaryReaderAsync _binaryReader;
        readonly TParser _parser;

        public ArrayParser( CustomBinaryReaderAsync binaryReader, TParser parser)
        {
            _binaryReader = binaryReader;
            _parser = parser;
        }
        public async ValueTask<TParsedValue[]> Parse()
        {
            int length = await _binaryReader.ReadInt32Async();
            Debug.Assert( length >= 0 );
            TParsedValue[] output = new TParsedValue[length];
            for( int i = 0; i < length; i++ )
            {
                output[i] = await _parser.Parse();
            }
            return output;
        }
    }
}
