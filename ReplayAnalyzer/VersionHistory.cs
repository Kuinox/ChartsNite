using System;
using System.Collections.Generic;
using System.Text;

namespace ReplayAnalyzer
{
    public enum VersionHistory : uint
    {
        HISTORY_INITIAL = 0,
        HISTORY_FIXEDSIZE_FRIENDLY_NAME = 1,
        HISTORY_COMPRESSION = 2,
        HISTORY_RECORDED_TIMESTAMP = 3,
        HISTORY_STREAM_CHUNK_TIMES = 4,
        HISTORY_FRIENDLY_NAME_ENCODING = 5,

        // -----<new versions can be added before this line>-------------------------------------------------
        HISTORY_PLUS_ONE,
        HISTORY_LATEST = HISTORY_PLUS_ONE - 1
    }
}
