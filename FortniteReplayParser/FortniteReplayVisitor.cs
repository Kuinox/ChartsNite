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

        public override ValueTask<bool> ChooseEventChunkType( CustomBinaryReaderAsync chunkReader, EventOrCheckpointInfo eventInfo ) => eventInfo.Group switch
        {
            "playerElim" => VisitPlayerElimChunk( chunkReader, eventInfo ),
            "AthenaMatchStats" => VisitAthenaMatchStats( eventInfo ),
            "AthenaMatchTeamStats" => VisitAthenaMatchTeamStats( eventInfo ),
            "AthenaReplayBrowserEvents" => VisitAthenaReplayBrowserEvents( chunkReader, eventInfo ),
            "PlayerStateEncryptionKey" => VisitPlayerStateEncryptionKey( chunkReader, eventInfo ),
            "fortBenchEvent" => VisitFortBenchEvent( chunkReader, eventInfo ),
            _ => VisitUnknowEventChunkType( chunkReader, eventInfo )
        };

        public virtual ValueTask<bool> VisitFortBenchEvent( CustomBinaryReaderAsync chunkReader, EventOrCheckpointInfo eventInfo )
        {
            return new ValueTask<bool>( true );
        }
        public virtual ValueTask<bool> VisitPlayerStateEncryptionKey( CustomBinaryReaderAsync chunkReader, EventOrCheckpointInfo eventInfo )
        {
            return new ValueTask<bool>( true );
        }
        public virtual ValueTask<bool> VisitAthenaReplayBrowserEvents( CustomBinaryReaderAsync chunkReader, EventOrCheckpointInfo eventInfo )
        {
            return new ValueTask<bool>( true );
        }
        public virtual ValueTask<bool> VisitUnknowEventChunkType( CustomBinaryReaderAsync chunkReader, EventOrCheckpointInfo eventInfo )
        {
            throw new InvalidDataException( "I throw exceptions when i see a type of chunks i never saw." );
        }

        public virtual ValueTask<bool> VisitAthenaMatchStats( EventOrCheckpointInfo eventInfo )
        {
            return new ValueTask<bool>( true );
        }
        public virtual ValueTask<bool> VisitAthenaMatchTeamStats( EventOrCheckpointInfo eventInfo )
        {
            return new ValueTask<bool>( true );
        }
        public virtual async ValueTask<bool> VisitPlayerElimChunk( CustomBinaryReaderAsync binaryReader, EventOrCheckpointInfo eventInfo )
        {
            int amountToSkip;
            switch( DemoHeader!.Branch )
            {
                case "++Fortnite+Release-4.0":
                    amountToSkip = 12;
                    break;
                case "++Fortnite+Release-4.2":
                    amountToSkip = 40;
                    break;
                default:
                    amountToSkip = 45;
                    break;
            }
            Memory<byte> unknownData = await binaryReader.ReadBytesAsync( amountToSkip );
            
            string killed = await binaryReader.ReadStringAsync();
            string killer = await binaryReader.ReadStringAsync();
            PlayerElimChunk.WeaponType weapon = (PlayerElimChunk.WeaponType)await binaryReader.ReadByteAsync();
            PlayerElimChunk.State victimState = (PlayerElimChunk.State)await binaryReader.ReadInt32Async();
            return await VisitPlayerElimResult( new PlayerElimChunk( eventInfo, unknownData, killed, killer, weapon, victimState ) );
        }

        public virtual ValueTask<bool> VisitPlayerElimResult( PlayerElimChunk playerElim )
        {
            return new ValueTask<bool>( true );
        }

        public virtual ValueTask<bool> VisitHeaderChunkWhereWeDidntReadAllData()
        {
            return new ValueTask<bool>( false );
        }
    }
}
