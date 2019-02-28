using Common.StreamHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UnrealReplayParser.UnrealObject
{
    public class StringParser : IParser<string>
    {
        readonly CustomBinaryReaderAsync _binaryReader;
        public StringParser( CustomBinaryReaderAsync binaryReader )
        {
            _binaryReader = binaryReader;
        }
        public ValueTask<string> Parse()
        {
            return _binaryReader.ReadString();
        }
    }

    public class TupleParser<TParser1, TParser2, TParsedValue1, TParsedValue2>
        : IParser<(TParsedValue1, TParsedValue2)>
        where TParser1 : IParser<TParsedValue1>
        where TParser2 : IParser<TParsedValue2>
    {
        readonly TParser1 _parser1;
        readonly TParser2 _parser2;

        public TupleParser( TParser1 parser1, TParser2 parser2 )
        {
            _parser1 = parser1;
            _parser2 = parser2;
        }
        public async ValueTask<(TParsedValue1, TParsedValue2)> Parse()
        {
            return (await _parser1.Parse(), await _parser2.Parse());
        }
    }

    public class UInt32Parser : IParser<uint>
    {
        readonly CustomBinaryReaderAsync _binaryReader;
        public UInt32Parser( CustomBinaryReaderAsync binaryReader )
        {
            _binaryReader = binaryReader;
        }
        public ValueTask<uint> Parse() => _binaryReader.ReadUInt32();
    }
}
