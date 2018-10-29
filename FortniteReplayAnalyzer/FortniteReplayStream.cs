using ReplayAnalyzer;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
namespace FortniteReplayAnalyzer
{
    public class FortniteReplayStream : ReplayStream
    {
        FortniteReplayStream(ReplayStream stream) : base(stream)
        {
        }

        public static async Task<FortniteReplayStream> FortniteReplayFromStream(Stream stream)
        {
            return new FortniteReplayStream(await FromStream(stream));
        }

        public override async Task<ChunkInfo> ReadChunk()
        {
            ChunkInfo chunkInfo = await base.ReadChunk();
            if (chunkInfo is EventInfo eventInfo)
            {
                switch (eventInfo.Group)
                {
                    case "playerElim":
                        uint size = await ReadUInt32();
                        byte[] unknownData = await ReadBytes(45);
                        string killed = await ReadString();
                        string killer = await ReadString();
                        KillEventChunk.WeaponType weapon = (KillEventChunk.WeaponType) await ReadByteOnce();
                        KillEventChunk.State victimState = (KillEventChunk.State)await ReadInt32();
                        
                        return new KillEventChunk(eventInfo, size, unknownData, killed, killer, weapon, victimState);
                    case "AthenaMatchStats":
                        break;
                    case "AthenaMatchTeamStats":
                        break;
                }
            }

            return chunkInfo;
        }
    }
}
