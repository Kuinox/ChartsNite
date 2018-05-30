using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyser
{
    class Entry
    {
	    readonly TextReader _textToParse;
	    byte[] _bytes;
	    public Entry(TextReader textToParse, byte[] bytes)
	    {
		    _textToParse = textToParse;
		    _bytes = bytes;
	    }
	    public static async Task<Entry> GetEntry(TextReader textToParse, byte[] bytes)
	    {
		    var output = new Entry(textToParse, bytes);
		    output.Id = await output.ReadAmount(34);
		    var typeLength = BitConverter.ToInt32(Encoding.ASCII.GetBytes(await output.ReadAmount(4)),0);
		    if (typeLength > byte.MaxValue)
		    {
			    return output;
		    }
		    //await output.ReadAmount(3);//Useless data ? nope an int
		    output.EntryType = await output.ReadAmount(typeLength);
		    if (output.EntryType != "playerElim\0")
		    {
			    Console.WriteLine("Unsupported entry");
			    return output;
		    }
		    output.UnknowData = await output.ReadAmount(4*3);//3 unknow int
		    var entrySize = BitConverter.ToInt32(Encoding.ASCII.GetBytes(await output.ReadAmount(4)), 0);
		    output.KillEntry = new KillEntry(await output.ReadAmount(entrySize));

		    /*var userNameLength = BitConverter.ToInt32(Encoding.ASCII.GetBytes(await output.ReadAmount(4)), 0);
			output.VictimUserName = await output.ReadAmount(userNameLength);
		    var secondUserNameLength = BitConverter.ToInt32(Encoding.ASCII.GetBytes(await output.ReadAmount(4)), 0);
			output.KillerUsername = await output.ReadAmount(secondUserNameLength);*/
		    return output;
	    }

	    public async Task<string> ReadAmount(int amountToRead)
	    {
		    var buffer = new char[amountToRead];
		    await _textToParse.ReadAsync(buffer, 0, amountToRead);
		    return new string(buffer);
	    }

	    public string Id { get; set; }
	    public string EntryType { get; set; }
		public string UnknowData { get; set; }
		public KillEntry KillEntry { get; set; }


	}
}
