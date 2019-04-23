using Common.StreamHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace UnrealReplayParser.UnrealObject
{
    public class SparseArrayParser<TParsedValue, TParser> : IParser<TParsedValue[]> where TParser : IParser<TParsedValue>
    {
        readonly CustomBinaryReaderAsync _binaryReader;
        readonly TParser _parser;

        public SparseArrayParser( CustomBinaryReaderAsync binaryReader, TParser parser )
        {
            _binaryReader = binaryReader;
            _parser = parser;
        }
        public async ValueTask<TParsedValue[]> Parse() 
        {
            //_ = await _binaryReader.ReadInt64Async();
            //_ = await _binaryReader.ReadInt32Async();
            //_ = await _binaryReader.ReadInt32Async();
            int newNumElement = await _binaryReader.ReadInt32Async();
            Debug.Assert( newNumElement >= 0 );
            TParsedValue[] output = new TParsedValue[newNumElement];
            for( int i = 0; i < newNumElement; i++ )
            {
                output[i] = await _parser.Parse();
            }
            return output;
        }
    }
}
