using System.IO;
using System.Threading.Tasks;
using UnrealReplayParser;
using Common.StreamHelpers;
using System.Diagnostics;
using FortniteReplayParser.Chunk;
using UnrealReplayParser.Chunk;

namespace FortniteReplayParser
{
    public class FortniteReplayVisitor : UnrealReplayVisitor
    {
        protected FortniteReplayVisitor(ReplayInfo info, SubStreamFactory subStreamFactory) : base(info, subStreamFactory)
        {
        }

        public static async Task<FortniteReplayVisitor> FortniteVisitorFromStream(SubStreamFactory subStreamFactory)
        {
            return new FortniteReplayVisitor((await FromStream(subStreamFactory)).Info, subStreamFactory);
        }

        public override Task<bool> VisitEventChunkContent(BinaryReaderAsync binaryReader, EventInfo eventInfo)
        {
            return eventInfo.Group switch
            {
                "playerElim" => VisitPlayerElimChunk(binaryReader, eventInfo),
                "AthenaMatchStats" => VisitAthenaMatchStats(eventInfo),
                "AthenaMatchTeamStats" => VisitAthenaMatchTeamStats(eventInfo),
                "checkpoint" => VisitCheckPoint(eventInfo),
                _ => Task.FromResult(false)
            };
        }
        public virtual Task<bool> VisitCheckPoint(EventInfo eventInfo)
        {
            return Task.FromResult(true);
        }
        public virtual Task<bool> VisitAthenaMatchStats(EventInfo eventInfo)
        {
            return Task.FromResult(true);
        }
        public virtual Task<bool> VisitAthenaMatchTeamStats(EventInfo eventInfo)
        {
            return Task.FromResult(true);
        }
        public virtual async Task<bool> VisitPlayerElimChunk(BinaryReaderAsync binaryReader, EventInfo eventInfo)
        {
            if (eventInfo.EventSizeInBytes < 45)
            {
                byte[] bytes = await binaryReader.ReadBytes(eventInfo.EventSizeInBytes);
                return await VisitPlayerElimResult( new PlayerElimChunk(eventInfo, new byte[0], "", "", PlayerElimChunk.WeaponType.Unknown, PlayerElimChunk.State.Unknow, false));
            }
            byte[] unknownData = await binaryReader.ReadBytes(45);
            string killed = await binaryReader.ReadString();
            string killer = await binaryReader.ReadString();
            PlayerElimChunk.WeaponType weapon = (PlayerElimChunk.WeaponType)await binaryReader.ReadByteOnce();
            PlayerElimChunk.State victimState = (PlayerElimChunk.State)await binaryReader.ReadInt32();
            return await VisitPlayerElimResult(new PlayerElimChunk(eventInfo, unknownData, killed, killer, weapon, victimState, true));
        }

        public virtual Task<bool> VisitPlayerElimResult(PlayerElimChunk playerElim)
        {
            return Task.FromResult(true);
        }

        public override async Task<bool> VisitHeaderChunk(BinaryReaderAsync binaryReader, ChunkInfo chunk)
        {
            uint fortniteMagicNumber = await binaryReader.ReadUInt32();//this is an attempt and shouldnt be read as fact but an attempt to known what is the data behind.
            if (fortniteMagicNumber != 754295101)
            {
                return false;
            }
            uint headerVersion = await binaryReader.ReadUInt32();//TODO: Like JSONVisitor, TryRead return default value and we check later if it failed
            uint notVersion = await binaryReader.ReadUInt32(); //Change value between versions, but not always
            uint notSeasonNumber = await binaryReader.ReadUInt32();//if u change this value, the replay crash, so its information to deserialize the data.
            uint alwaysZero = await binaryReader.ReadUInt32();
            Debug.Assert(alwaysZero == 0);
            byte[] guid;
            if (headerVersion > 11)
            {
                guid = await binaryReader.ReadBytes(16);
            }

            short alwaysFour = await binaryReader.ReadInt16(); //Maybe 4 is for struct "build+version
            Debug.Assert(alwaysFour == 4);
            uint a20Or21 = await binaryReader.ReadUInt32();//want from 20 to 21 after a version upgrade, version and release, maybe in the same struct: "build+version
            uint version = await binaryReader.ReadUInt32();
            string release = await binaryReader.ReadString();
            uint alwaysOne = await binaryReader.ReadUInt32(); // we get a always one b4 a string
            Debug.Assert(alwaysOne == 1);
            string mapPath = await binaryReader.ReadString(); //string
            uint alwaysZero2 = await binaryReader.ReadUInt32();
            Debug.Assert(alwaysZero2 == 0);
            uint alwaysThree = await binaryReader.ReadUInt32();
            Debug.Assert(alwaysThree == 3);
            uint alwaysOne2 = await binaryReader.ReadUInt32();// we get a always one b4 a string
            string subGame = "";
            if (alwaysOne2 == 1)
            {
                subGame = await binaryReader.ReadString(); //string
            }
            if (binaryReader.Stream.Position != chunk.SizeInBytes) throw new InvalidDataException("Didnt expected more data");
            return await VisitFortniteHeaderChunk(new FortniteHeaderChunk(version, release, subGame, mapPath));
        }

        public virtual Task<bool> VisitFortniteHeaderChunk(FortniteHeaderChunk headerChunk)
        {
            return Task.FromResult(true);
        }

        public override Task<bool> VisitReplayDataChunkContent(BinaryReaderAsync binaryReader, ReplayDataInfo replayDataInfo)
        {
            return base.VisitReplayDataChunkContent(binaryReader, replayDataInfo);
        }
    }
}
