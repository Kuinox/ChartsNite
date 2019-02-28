using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UnrealReplayParser.UnrealObject
{
    public interface IParser<T>
    {
        ValueTask<T> Parse();
    }
}
