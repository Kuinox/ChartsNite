using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Analyser
{
	class Program
	{
		static void Main(string[] args)
		{
			const string saveName = @"UnsavedReplay-2018.05.27-01.26.52";
			
			using (var saveFile = File.OpenRead(saveName + ".replay"))
			{
				var buffer = new byte[saveFile.Length];
				saveFile.ReadAsync(buffer, 0, buffer.Length);
				var saveText = Encoding.ASCII.GetString(buffer);
				var entriesString = saveText.Split(new[]{ saveName }, StringSplitOptions.None).ToList();
				var entries = new List<Entry>();
				var constantData = new List<string>();
				foreach (var entry in entriesString)
				{
					var candidate = new Entry(entry);
					entries.Add(candidate);
					constantData.Add(candidate.ConstantInitialData);
				}
			}
		}
	}
}