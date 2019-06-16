using System.IO;
using System.Threading.Tasks;
using UnrealReplayParser;
using Common.StreamHelpers;
using System.Diagnostics;
using UnrealReplayParser.Chunk;
using FortniteReplayParser.Chunk;
using System;
using ChartsNite.UnrealReplayParser;
using ChartsNite.UnrealReplayParser.StreamArchive;

namespace FortniteReplayParser
{
    public class FortniteReplayVisitor : UnrealReplayVisitor
    {
        public FortniteReplayVisitor( Stream stream ) : base( stream )
        {
        }

        public override ValueTask<bool> ChooseEventChunkType( ReplayArchiveAsync ar, EventOrCheckpointInfo eventInfo ) => eventInfo.Group.Trim( '\0' ) switch
        {
            "playerElim" => VisitPlayerElimChunk( ar, eventInfo ),
            "AthenaMatchStats" => VisitAthenaMatchStats( eventInfo ),
            "AthenaMatchTeamStats" => VisitAthenaMatchTeamStats( eventInfo ),
            "AthenaReplayBrowserEvents" => VisitAthenaReplayBrowserEvents( ar, eventInfo ),
            "PlayerStateEncryptionKey" => VisitPlayerStateEncryptionKey( ar, eventInfo ),
            "fortBenchEvent" => VisitFortBenchEvent( ar, eventInfo ),
            _ => VisitUnknowEventChunkType( ar, eventInfo )
        };

        public virtual ValueTask<bool> VisitFortBenchEvent( ReplayArchiveAsync chunkReader, EventOrCheckpointInfo eventInfo )
        {
            return new ValueTask<bool>( true );
        }
        public virtual ValueTask<bool> VisitPlayerStateEncryptionKey( ReplayArchiveAsync chunkReader, EventOrCheckpointInfo eventInfo )
        {
            return new ValueTask<bool>( true );
        }
        public virtual ValueTask<bool> VisitAthenaReplayBrowserEvents( ReplayArchiveAsync chunkReader, EventOrCheckpointInfo eventInfo )
        {
            return new ValueTask<bool>( true );
        }
        public virtual async ValueTask<bool> VisitUnknowEventChunkType( ReplayArchiveAsync chunkReader, EventOrCheckpointInfo eventInfo )
        {
            //throw new InvalidDataException( "I throw exceptions when i see a type of chunks i never saw." );
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
        public virtual async ValueTask<bool> VisitPlayerElimChunk( ReplayArchiveAsync ar, EventOrCheckpointInfo eventInfo )
        {
            int amountToSkip;
            if( (int)DemoHeader!.EngineNetworkProtocolVersion >= 11
                && DemoHeader!.Branch != "++Fortnite+Release-8.20"
                && DemoHeader!.Branch != "++Fortnite+Release-8.30"
                && DemoHeader!.Branch != "++Fortnite+Release-8.40" )//a little awful lol.
            {
                if( DemoHeader!.Branch == "++Fortnite+Release-4.0" || DemoHeader!.Branch == "++Fortnite+Release-4.2" )
                {
                    throw new InvalidOperationException();
                }
                Memory<byte> a = await ar.ReadBytesAsync( 87 );
                Console.WriteLine( BitConverter.ToString( a.Span.ToArray() ) );
                try
                {
                    var killedId = PlayerId.FromEpicId( (await ar.ReadBytesAsync( 16 )).ToArray() );
                    Debug.Assert( await ar.ReadInt16Async() == 4113 );//wtf is this
                    var killerId = PlayerId.FromEpicId( (await ar.ReadBytesAsync( 16 )).ToArray() );
                    PlayerElimChunk.WeaponType newWeapon = (PlayerElimChunk.WeaponType)await ar.ReadByteAsync();
                    PlayerElimChunk.State newVictimState = (PlayerElimChunk.State)await ar.ReadInt32Async();
                    File.AppendAllText( "debugfile", $"{DemoHeader!.Branch} OK \n" );
                    return await VisitPlayerElimResult( new PlayerElimChunk( eventInfo, killedId, killerId, newWeapon, newVictimState ) );
                }
                catch
                {
                    File.AppendAllText( "debugfile", $"{DemoHeader!.Branch} FAIL \n" );
                    throw;
                }
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
            Memory<byte> unknownData = await ar.ReadBytesAsync( amountToSkip );
            PlayerId killed = PlayerId.FromPlayerName( await ar.ReadStringAsync() );
            PlayerId killer = PlayerId.FromPlayerName( await ar.ReadStringAsync() );
            PlayerElimChunk.WeaponType weapon = (PlayerElimChunk.WeaponType)await ar.ReadByteAsync();
            PlayerElimChunk.State victimState = (PlayerElimChunk.State)await ar.ReadInt32Async();
            return await VisitPlayerElimResult( new PlayerElimChunk( eventInfo, killed, killer, weapon, victimState ) );
        }

        public virtual ValueTask<bool> VisitPlayerElimResult( PlayerElimChunk playerElim )
        {
            return new ValueTask<bool>( true );
        }
    }
}
