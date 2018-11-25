using ReplayAnalyzer;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common.StreamHelpers;
using UnrealReplayAnalyzer;

namespace FortniteReplayAnalyzer
{
    public class FortniteReplayReader : ReplayReader
    {

        public string FortniteRelease { get; set; }
        public string SubGame { get; set; }
        public string MapPath { get; set; }
        FortniteReplayReader(ChunkReader reader) : base(reader)
        {
        }


        public static async Task<FortniteReplayReader> FortniteReplayFromStream(Stream stream)
        {
            return new FortniteReplayReader(await FromStream(stream));
        }

        public override async Task<ChunkInfo> ReadChunk()
        {
            ChunkInfo chunk = await base.ReadChunk();
            if (chunk.Type == (uint) ChunkType.Header)
            {
                //following is an attempt and shouldnt be read as fact but an attempt to known what is the data behind.
                uint fortniteMagicNumber = await chunk.Stream.ReadUInt32();
                uint headerVersion = await chunk.Stream.ReadUInt32();
                uint fortniteVersionUUID = await chunk.Stream.ReadUInt32();
                uint seasonNumber = await chunk.Stream.ReadUInt32();
                uint alwaysZero = await chunk.Stream.ReadUInt32();

                byte[] guid;
                if (headerVersion > 11)
                {
                    guid = await chunk.Stream.ReadBytes(16);
                }

                short alwaysFour = await chunk.Stream.ReadInt16();
                uint anotherUnknownNumber = await chunk.Stream.ReadUInt32();//want from 20 to 21 after a version upgrade
                uint numberThatKeepValueAcrossReplays = await chunk.Stream.ReadUInt32();
                FortniteRelease = await chunk.Stream.ReadString();
                uint alwaysOne = await chunk.Stream.ReadUInt32();
                MapPath = await chunk.Stream.ReadString();
                uint alwaysZero2 = await chunk.Stream.ReadUInt32();
                uint alwaysThree = await chunk.Stream.ReadUInt32();
                uint alwaysOne2 = await chunk.Stream.ReadUInt32();
                if (alwaysOne2 == 1)
                {
                    SubGame = await chunk.Stream.ReadString();
                } 
                if(chunk.Stream.Position != chunk.SizeInBytes) throw new InvalidDataException("Didnt expected more data");
            }
            switch (chunk)
            {
                case EventInfo eventInfo:
                    switch (eventInfo.Group)
                    {
                        case "playerElim":
                            //using (StreamWriter writer = File.AppendText("dump"))
                            //{
                            //    await writer.WriteLineAsync(BitConverter.ToString(await eventInfo.Stream.ReadBytes(eventInfo.EventSizeInBytes)));
                            //}
                            return chunk;
                            //if (eventInfo.EventSizeInBytes < 45)
                            //{
                            //    byte[] bytes = await eventInfo.Stream.ReadBytes(eventInfo.EventSizeInBytes);
                            //    Console.WriteLine("WEIRD UNKNOWN DATA:" +BitConverter.ToString(bytes) +"  " + Encoding.ASCII.GetString(bytes));
                            //    return chunk;
                            //}
                            //byte[] unknownData = await chunk.Stream.ReadBytes(45);
                            //string killed = await chunk.Stream.ReadString();
                            //if (!UserNameChecker.CheckUserName(killed)) throw new InvalidDataException("Invalid user name.");
                            //string killer = await chunk.Stream.ReadString();
                            //if (!UserNameChecker.CheckUserName(killer)) throw new InvalidDataException("Invalid user name.");
                            //KillEventChunk.WeaponType weapon = (KillEventChunk.WeaponType)await chunk.Stream.ReadByteOnce();
                            //KillEventChunk.State victimState = (KillEventChunk.State)await chunk.Stream.ReadInt32();
                            //return new KillEventChunk(eventInfo, unknownData, killed, killer, weapon, victimState);
                        case "AthenaMatchStats":
                            return chunk;
                        case "AthenaMatchTeamStats":
                            return chunk;
                        case "checkpoint":
                            return chunk;
                        default:
                            //Console.WriteLine("UNKNOWN CASE" + eventInfo.Group); //TODO
                            return chunk;
                    }
                default:
                    return chunk;
            }
        }
    }
}
