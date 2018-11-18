using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartsNite.Data;
using CK.Core;
using CK.SqlServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Common.StreamHelpers;
using FortniteReplayAnalyzer;
using Microsoft.AspNetCore.Http;
using ReplayAnalyzer;

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
            List<KillEventChunk> kills = new List<KillEventChunk>();
            ReplayInfo info;
            using (Stream replayStream = file.OpenReadStream())//TODO can timeout, BIGINT, check SQL types.
            using (Stream saveStream = System.IO.File.OpenWrite("save"))
            using (Stream stream = new CopyAsYouReadStream(replayStream, saveStream))
            using (FortniteReplayStream replay = await FortniteReplayStream.FortniteReplayFromStream(stream))
            {
                info = replay.Info;
                while (replay.Position < replay.Length)
                {
                    using (var chunk = await replay.ReadChunk())
                    {
                        if (chunk is KillEventChunk killEvent)  //TODO: throw if not disposed
                        {
                            kills.Add(killEvent);
                        }
                    }
                    
                }
            }

            Kill[] killsCasted = kills.Select(chunk => new Kill(TimeSpan.FromMilliseconds(chunk.Time1), chunk.PlayerKilling,
                chunk.PlayerKilled, (byte) chunk.Weapon, chunk.VictimState == KillEventChunk.State.KnockedDown)).ToArray();
            ReplayTable u = _stObjMap.StObjs.Obtain<ReplayTable>();
            using (var ctx = new SqlStandardCallContext())
            {
                await u.CreateAsync(ctx, 1, 1, DateTime.UtcNow, TimeSpan.MinValue, "random", 0, killsCasted);
            }

            return Ok();
        }
    }
}
