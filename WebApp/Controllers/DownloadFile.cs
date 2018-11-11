using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Common.StreamHelpers;
using FortniteReplayAnalyzer;
using ReplayAnalyzer;

namespace WebApp.Controllers
{
    [Produces("application/json")]
    [Route("/api/upload")]
    public class DownloadFile : Controller
    {
        [HttpPost("replay")]
        public async Task<IActionResult> ReplayUpload(IFileInfo fileInfo)
        {
            using (Stream replayStream = fileInfo.CreateReadStream())//TODO can timeout, BIGINT
            using (Stream saveStream = System.IO.File.OpenWrite("save"))
            using (Stream stream = new CopyAsYouReadStream(replayStream, saveStream))
            using (FortniteReplayStream replay = await FortniteReplayStream.FortniteReplayFromStream(stream))
            {
                List<KillEventChunk> kills = new List<KillEventChunk>();
                while (replay.Position < replay.Length)
                {
                    var chunk = await replay.ReadChunk();
                    if (chunk is KillEventChunk killEvent)
                    {
                        kills.Add(killEvent);
                    }
                }



            }

            return Ok();
        }
    }
}
