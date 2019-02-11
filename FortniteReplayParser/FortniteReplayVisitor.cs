using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnrealReplayParser;
using Common.StreamHelpers;
using System.Diagnostics;
using FortniteReplayParser.Chunk;

namespace FortniteReplayParser
{
    class FortniteReplayVisitor : UnrealReplayVisitor
    {
        protected FortniteReplayVisitor(ReplayInfo info, BinaryReaderAsync binaryReader) : base(info, binaryReader)
        {
        }

        public override Task<bool> VisitEventChunkContent(EventInfo eventInfo)
        {
            return eventInfo.Group switch
            {
                "playerElim" => VisitPlayerElimChunk(eventInfo),
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
        public virtual async Task<bool> VisitPlayerElimChunk(EventInfo eventInfo)
        {
            if (eventInfo.EventSizeInBytes < 45)
            {
                byte[] bytes = await BinaryReader.ReadBytes(eventInfo.EventSizeInBytes);
                return await VisitPlayerElimResult( new PlayerElimChunk(eventInfo, new byte[0], "", "", PlayerElimChunk.WeaponType.Unknown, PlayerElimChunk.State.Unknow, false));
            }
            byte[] unknownData = await BinaryReader.ReadBytes(45);
            string killed = await BinaryReader.ReadString();
            string killer = await BinaryReader.ReadString();
            PlayerElimChunk.WeaponType weapon = (PlayerElimChunk.WeaponType)await BinaryReader.ReadByteOnce();
            PlayerElimChunk.State victimState = (PlayerElimChunk.State)await BinaryReader.ReadInt32();
            return await VisitPlayerElimResult(new PlayerElimChunk(eventInfo, unknownData, killed, killer, weapon, victimState, true));
        }

        public virtual Task<bool> VisitPlayerElimResult(PlayerElimChunk playerElim)
        {
            return Task.FromResult(true);
        }

        public override async Task<bool> VisitHeaderChunk(ChunkInfo chunk)
        {
            uint fortniteMagicNumber = await BinaryReader.ReadUInt32();//this is an attempt and shouldnt be read as fact but an attempt to known what is the data behind.
            if (fortniteMagicNumber != 754295101)
            {
                return false;
            }
            uint headerVersion = await BinaryReader.ReadUInt32();//TODO: Like JSONVisitor, TryRead return default value and we check later if it failed
            uint notVersion = await BinaryReader.ReadUInt32(); //Change value between versions, but not always
            uint notSeasonNumber = await BinaryReader.ReadUInt32();//if u change this value, the replay crash, so its information to deserialize the data.
            uint alwaysZero = await BinaryReader.ReadUInt32();
            Debug.Assert(alwaysZero == 0);
            byte[] guid;
            if (headerVersion > 11)
            {
                guid = await BinaryReader.ReadBytes(16);
            }

            short alwaysFour = await BinaryReader.ReadInt16(); //Maybe 4 is for struct "build+version
            Debug.Assert(alwaysFour == 4);
            uint a20Or21 = await BinaryReader.ReadUInt32();//want from 20 to 21 after a version upgrade, version and release, maybe in the same struct: "build+version
            uint version = await BinaryReader.ReadUInt32();
            string release = await BinaryReader.ReadString();
            uint alwaysOne = await BinaryReader.ReadUInt32(); // we get a always one b4 a string
            Debug.Assert(alwaysOne == 1);
            string mapPath = await BinaryReader.ReadString(); //string
            uint alwaysZero2 = await BinaryReader.ReadUInt32();
            Debug.Assert(alwaysZero2 == 0);
            uint alwaysThree = await BinaryReader.ReadUInt32();
            Debug.Assert(alwaysThree == 3);
            uint alwaysOne2 = await BinaryReader.ReadUInt32();// we get a always one b4 a string
            string subGame = "";
            if (alwaysOne2 == 1)
            {
                subGame = await BinaryReader.ReadString(); //string
            }
            if (chunk.Stream.Position != chunk.SizeInBytes) throw new InvalidDataException("Didnt expected more data");
            return await VisitFortniteHeaderChunk(new FortniteHeaderChunk(version, release, subGame, mapPath));
        }

        public virtual Task<bool> VisitFortniteHeaderChunk(FortniteHeaderChunk headerChunk)
        {
            return Task.FromResult(true);
        }

        public override Task<bool> VisitReplayDataChunkContent(ReplayDataInfo replayDataInfo)
        {
            return base.VisitReplayDataChunkContent(replayDataInfo);
        }
    }
}
