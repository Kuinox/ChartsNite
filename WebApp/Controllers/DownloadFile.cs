﻿using ChartsNite.Data;
using CK.Core;
using CK.SqlServer;
using Common.StreamHelpers;
using FortniteReplayParser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnrealReplayParser;

namespace WebApp.Controllers
{
    [Produces("application/json")]
    [Route("upload")]
    public class DownloadFile : Controller
    {
        readonly IStObjMap _stObjMap;

        public DownloadFile(IStObjMap stObjMap)
        {
            _stObjMap = stObjMap;
        }
        [HttpPost("replay")]
        public async Task<IActionResult> ReplayUpload(IFormFile file)
        {
            List<PlayerElimChunk> kills = new List<PlayerElimChunk>();
            ReplayInfo info;
            string hash;
            using (Stream replayStream = file.OpenReadStream())//TODO can timeout, BIGINT, check SQL types.
            using (SHA1Stream hashStream = new SHA1Stream(replayStream, true, false))
            using (Stream saveStream = System.IO.File.OpenWrite("save"))
            using (Stream stream = new CopyAsYouReadStream(hashStream, saveStream))
            using (var chunkReader = await UnrealReplayParser.UnrealReplayParser.FromStream(stream))
            using (FortniteReplayParser.FortniteReplayParser replay = new FortniteReplayParser.FortniteReplayParser(chunkReader))
            {
                info = replay.Info;
                ChunkInfo? chunk;
                do
                {
                    using (chunk = await replay.ReadChunk())//TODO redo the loop, or abstract it.
                    {
                        if (chunk is PlayerElimChunk killEvent) //TODO: throw if not disposed
                        {
                            kills.Add(killEvent);
                        }
                    }
                } while (chunk != null);
                hash = hashStream.GetFinalResult().ToString();
            }

            Kill[] killsCasted = kills.Select(chunk => new Kill(TimeSpan.FromMilliseconds(chunk.Time1), chunk.PlayerKilling,
                chunk.PlayerKilled, (byte)chunk.Weapon, chunk.VictimState == PlayerElimChunk.State.KnockedDown)).ToArray();
            ReplayTable u = _stObjMap.StObjs.Obtain<ReplayTable>();
            using (var ctx = new SqlStandardCallContext())
            {
                await u.CreateAsync(ctx, 1, 1, DateTime.UtcNow, TimeSpan.FromMilliseconds(info.LengthInMs), hash, 0, killsCasted);
            }
            return Ok();
        }
    }
}
