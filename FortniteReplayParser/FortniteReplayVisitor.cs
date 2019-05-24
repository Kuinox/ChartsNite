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

        public override ValueTask<bool> ChooseEventChunkType( CustomBinaryReaderAsync chunkReader, EventOrCheckpointInfo eventInfo ) => eventInfo.Group.Trim( '\0' ) switch
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
        public virtual async ValueTask<bool> VisitUnknowEventChunkType( CustomBinaryReaderAsync chunkReader, EventOrCheckpointInfo eventInfo )
        {
            // throw new InvalidDataException( "I throw exceptions when i see a type of chunks i never saw." );
            return true;
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
            if( (int) DemoHeader!.EngineNetworkProtocolVersion >= 11 )
            {
                if( DemoHeader!.Branch == "++Fortnite+Release-4.0" || DemoHeader!.Branch == "++Fortnite+Release-4.2" )
                {
                    throw new InvalidOperationException();
                }

                Memory<byte> a = await binaryReader.ReadBytesAsync( 87 );
                var killedId = PlayerId.FromEpicId((await binaryReader.ReadBytesAsync( 16 )).ToArray());
                Debug.Assert(await binaryReader.ReadInt16Async()==4113);//wtf is this
                var killerId = PlayerId.FromEpicId( (await binaryReader.ReadBytesAsync( 16 )).ToArray() );
                PlayerElimChunk.WeaponType newWeapon = (PlayerElimChunk.WeaponType)await binaryReader.ReadByteAsync();
                PlayerElimChunk.State newVictimState = (PlayerElimChunk.State)await binaryReader.ReadInt32Async();
                return await VisitPlayerElimResult( new PlayerElimChunk( eventInfo, killedId, killerId, newWeapon, newVictimState ) );

            }
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
            PlayerId killed = PlayerId.FromPlayerName( await binaryReader.ReadStringAsync() );
            PlayerId killer = PlayerId.FromPlayerName( await binaryReader.ReadStringAsync() );
            PlayerElimChunk.WeaponType weapon = (PlayerElimChunk.WeaponType)await binaryReader.ReadByteAsync();
            PlayerElimChunk.State victimState = (PlayerElimChunk.State)await binaryReader.ReadInt32Async();
            return await VisitPlayerElimResult( new PlayerElimChunk( eventInfo, killed, killer, weapon, victimState ) );
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
