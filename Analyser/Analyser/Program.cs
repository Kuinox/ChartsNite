using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyser
{
	class Program
	{
		static async Task Main(string[] args)
		{
			const string saveName = @"UnsavedReplay-2018.06.02-15.14.56";

			using (var saveFile = File.OpenRead(saveName + ".replay"))
			{
				var debug = BitConverter.ToInt32(new byte[] {00, 00, 43, 43}, 0);
				var buffer = new byte[saveFile.Length];
				await saveFile.ReadAsync(buffer, 0, buffer.Length);
				var saveText = Encoding.ASCII.GetString(buffer);
				var entriesPreProcess = saveText.Split(new[]{ saveName }, StringSplitOptions.None).ToList();
                var entriesPosition = entriesPreProcess.Select(s => saveText.IndexOf(s, StringComparison.InvariantCulture)-4);
				var entries = new List<Entry>();
			    var testEntries = new List<SerializedObject>();
                foreach (var position in entriesPosition)
				{
                    testEntries.Add(await SerializedObject.FromStream(new StringReader(saveText.Substring(position, await DeserializerHelper.ReadAmount()))));
					//var candidate = await Entry.GetEntry(new StringReader(entry), Encoding.ASCII.GetBytes(entry));
					//entries.Add(candidate);
				}
			}
		}
	}
}