using System.IO;
using System.Threading.Tasks;
using UnrealReplayParser;
using Common.StreamHelpers;
using System.Diagnostics;
using UnrealReplayParser.Chunk;
using FortniteReplayParser.Chunk;
using System;

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
        public virtual async Task<bool> VisitPlayerElimChunk( ChunkReader binaryReader, EventOrCheckpointInfo eventInfo )
        {
            using( Stream stream = File.OpenRead( Guid.NewGuid().ToString() ) )
            {
                var buffer = await binaryReader.DumpRemainingBytes();
                await stream.WriteAsync( buffer, 0, buffer.Length );
            }
            return true;

            //byte[] unknownData = await binaryReader.ReadBytes( 45 );
            //string killed = await binaryReader.ReadString();
            //string killer = await binaryReader.ReadString();
            //PlayerElimChunk.WeaponType weapon = (PlayerElimChunk.WeaponType)await binaryReader.ReadOneByte();
            //PlayerElimChunk.State victimState = (PlayerElimChunk.State)await binaryReader.ReadInt32();
            //if(binaryReader.IsError)
            //{

            //}
            //return await VisitPlayerElimResult( new PlayerElimChunk( eventInfo, unknownData, killed, killer, weapon, victimState ) );
        }

        public virtual Task<bool> VisitPlayerElimResult( PlayerElimChunk playerElim )
        {
            return Task.FromResult( true );
        }

        public virtual Task<bool> VisitHeaderChunkWhereWeDidntReadAllData()
        {
            return Task.FromResult( false );
        }
    }
}
