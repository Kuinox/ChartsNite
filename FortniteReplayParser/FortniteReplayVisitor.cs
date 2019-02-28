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
        public FortniteReplayVisitor( Stream stream ) : base( stream )
        {

        }


        public override Task<bool> ChooseEventChunkType( ChunkReader chunkReader, EventOrCheckpointInfo eventInfo ) => eventInfo.Group switch
        {
            "playerElim" => VisitPlayerElimChunk( chunkReader, eventInfo ),
            "AthenaMatchStats" => VisitAthenaMatchStats( eventInfo ),
            "AthenaMatchTeamStats" => VisitAthenaMatchTeamStats( eventInfo ),
            "AthenaReplayBrowserEvents" => VisitAthenaReplayBrowserEvents( chunkReader, eventInfo ),
            "checkpoint" => VisitCheckPoint( eventInfo ),
            "PlayerStateEncryptionKey" => VisitPlayerStateEncryptionKey( chunkReader, eventInfo ),
            "fortBenchEvent" => VisitFortBenchEvent( chunkReader, eventInfo ),
            _ => VisitUnknowEventChunkType( chunkReader, eventInfo )
        };
        public virtual Task<bool> VisitFortBenchEvent( ChunkReader chunkReader, EventOrCheckpointInfo eventInfo )
        {
            return Task.FromResult( true );
        }
        public virtual Task<bool> VisitPlayerStateEncryptionKey( ChunkReader chunkReader, EventOrCheckpointInfo eventInfo )
        {
            return Task.FromResult( true );
        }
        public virtual Task<bool> VisitAthenaReplayBrowserEvents( ChunkReader chunkReader, EventOrCheckpointInfo eventInfo )
        {
            return Task.FromResult( true );
        }
        public virtual Task<bool> VisitUnknowEventChunkType( ChunkReader chunkReader, EventOrCheckpointInfo eventInfo )
        {
            return Task.FromResult( false );
        }

        public virtual Task<bool> VisitCheckPoint( EventOrCheckpointInfo eventInfo )
        {
            return Task.FromResult( true );
        }
        public virtual Task<bool> VisitAthenaMatchStats( EventOrCheckpointInfo eventInfo )
        {
            return Task.FromResult( true );
        }
        public virtual Task<bool> VisitAthenaMatchTeamStats( EventOrCheckpointInfo eventInfo )
        {
            return Task.FromResult( true );
        }
        public virtual async Task<bool> VisitPlayerElimChunk( CustomBinaryReaderAsync binaryReader, EventOrCheckpointInfo eventInfo )
        {
            byte[] unknownData = await binaryReader.ReadBytes( 45 );
            string killed = await binaryReader.ReadString();
            string killer = await binaryReader.ReadString();
            PlayerElimChunk.WeaponType weapon = (PlayerElimChunk.WeaponType)await binaryReader.ReadOneByte();
            PlayerElimChunk.State victimState = (PlayerElimChunk.State)await binaryReader.ReadInt32();
            return await VisitPlayerElimResult( new PlayerElimChunk( eventInfo, unknownData, killed, killer, weapon, victimState ) );
        }

        public virtual Task<bool> VisitPlayerElimResult( PlayerElimChunk playerElim )
        {
            return Task.FromResult( true );
        }

        //public override async ValueTask<bool> ParseGameSpecificHeaderChunk( ChunkReader chunkReader)
        //{
        //    //uint fortniteMagicNumber = await binaryReader.ReadUInt32();//this is an attempt and shouldnt be read as fact but an attempt to known what is the data behind.
        //    //if( fortniteMagicNumber != 754295101 )
        //    //{
        //    //    return false;
        //    //}
        //    //uint headerVersion = await binaryReader.ReadUInt32();
        //    //uint notVersion = await binaryReader.ReadUInt32(); //Change value between versions, but not always
        //    //uint notSeasonNumber = await binaryReader.ReadUInt32();//if u change this value, the replay crash, so its information to deserialize the data.
        //    //uint alwaysZero = await binaryReader.ReadUInt32();
        //    //Debug.Assert( alwaysZero == 0 );
        //    //byte[] guid = new byte[0];
        //    //if( headerVersion > 11 )
        //    //{
        //    //    guid = await binaryReader.ReadBytes( 16 );
        //    //}
        //    //short alwaysFour = await binaryReader.ReadInt16(); //Maybe 4 is for struct "build+version
        //    //Debug.Assert( alwaysFour == 4 );
        //    //uint a20Or21 = await binaryReader.ReadUInt32();//want from 20 to 21 to 22 after a version upgrade, version and release, maybe in the same struct: "build+version
        //    //Debug.Assert( a20Or21 == 20 || a20Or21 == 21 || a20Or21 == 22 );
        //    //uint buildNumber = await binaryReader.ReadUInt32();
        //    //string release = await binaryReader.ReadString();
        //    //uint alwaysOne = await binaryReader.ReadUInt32(); // we get a always one b4 a string
        //    //Debug.Assert( alwaysOne == 1 );
        //    //string mapPath = await binaryReader.ReadString(); //string
        //    //uint alwaysZero2 = await binaryReader.ReadUInt32();
        //    //Debug.Assert( alwaysZero2 == 0 );
        //    //uint alwaysThree = await binaryReader.ReadUInt32();
        //    //Debug.Assert( alwaysThree == 3 );
        //    //uint alwaysOne2 = await binaryReader.ReadUInt32();// we get a always one b4 a string
        //    //string subGame = "";
        //    //if( alwaysOne2 == 1 )
        //    //{
        //    //    subGame = await binaryReader.ReadString(); //string
        //    //}
        //    //if( !binaryReader.AssertRemainingCountOfBytes( 0 ) && (!await VisitHeaderChunkWhereWeDidntReadAllData()))
        //    //{
        //    //    return false;
        //    //}
        //    return await VisitFortniteHeaderChunk( new FortniteHeaderChunk( buildNumber, release, subGame, mapPath, guid, headerVersion, notVersion, notSeasonNumber, a20Or21 ) );
        //}

        public virtual Task<bool> VisitHeaderChunkWhereWeDidntReadAllData()
        {
            return Task.FromResult( false );
        }

        public virtual Task<bool> VisitFortniteHeaderChunk( FortniteHeaderChunk headerChunk )
        {
            return Task.FromResult( true );
        }
    }
}
