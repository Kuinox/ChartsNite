using ReplayAnalyzer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common.StreamHelpers;
using UnrealReplayAnalyzer;

namespace FortniteReplayAnalyzer
{
    public class FortniteReplayReader : ReplayReader
    {

        public string Release { get; set; }
        public string SubGame { get; set; }
        public string MapPath { get; set; }
        public uint Version { get; set; }
        static List<string> awful = new List<string>();
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
            if (chunk == null) return null;
            if (chunk.Type == (uint) ChunkType.Header)
            {
                //following is an attempt and shouldnt be read as fact but an attempt to known what is the data behind.
                uint fortniteMagicNumber = await chunk.Stream.ReadUInt32();
                if (fortniteMagicNumber != 754295101)
                {
                    throw new InvalidDataException("Not the right magic number");
                }
                uint headerVersion = await chunk.Stream.ReadUInt32();
                uint notVersion = await chunk.Stream.ReadUInt32(); //Change value between versions, but not always
                uint notSeasonNumber = await chunk.Stream.ReadUInt32();//but it increased shortly after, https://fortnite.gamepedia.com/Season_6#Map_v6.02_.28October_11.29
                uint alwaysZero = await chunk.Stream.ReadUInt32();
                Debug.Assert(alwaysZero == 0);
                byte[] guid;
                if (headerVersion > 11)
                {
                    guid = await chunk.Stream.ReadBytes(16);
                }

                short alwaysFour = await chunk.Stream.ReadInt16();
                Debug.Assert(alwaysFour == 4);
                uint anotherUnknownNumber = await chunk.Stream.ReadUInt32();//want from 20 to 21 after a version upgrade
                Version = await chunk.Stream.ReadUInt32();
                Release = await chunk.Stream.ReadString();
                uint alwaysOne = await chunk.Stream.ReadUInt32(); // we get a always one b4 a string
                Debug.Assert(alwaysOne == 1);
                MapPath = await chunk.Stream.ReadString();
                uint alwaysZero2 = await chunk.Stream.ReadUInt32();
                Debug.Assert(alwaysZero2 == 0);
                uint alwaysThree = await chunk.Stream.ReadUInt32();
                Debug.Assert(alwaysThree == 3);
                uint alwaysOne2 = await chunk.Stream.ReadUInt32();// we get a always one b4 a string    
                if (alwaysOne2 == 1)
                {
                    SubGame = await chunk.Stream.ReadString();
                }

                string output = Release + " " + notSeasonNumber+ " "+headerVersion+" "+ anotherUnknownNumber + " "+MapPath+ " "+SubGame;
                if (!awful.Contains(output))
                {
                    awful.Add(output);
                    awful.Sort();
                }
                Console.WriteLine("________________");
                foreach (string s in awful)
                {
                    Console.WriteLine(s);
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
