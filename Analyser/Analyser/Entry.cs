using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Analyser
{
    class Entry
    {
	    readonly TextReader _textToParse;

	    public Entry(TextReader textToParse)
	    {
		    _textToParse = textToParse;

	    }
	    public static async Task<Entry> GetEntry(TextReader textToParse)
	    {
		    var output = new Entry(textToParse);
		    output.Id = await output.ReadAmount(34);
		    var typeLength = (byte)await output.ReadSingle();
		    await output.ReadAmount(3);//Useless data ? nope an int
		    output.EntryType = await output.ReadAmount(typeLength);
		    if (output.EntryType != "playerElim\0")
		    {
			    Console.WriteLine("Unsupported entry");
			    return output;
		    }

		    output.UnknowData = await output.ReadAmount(56);

		    output.UserNameLength = (byte) await output.ReadSingle();
		    await output.ReadAmount(3); //TODO ???
		    output.UserName = await output.ReadAmount(output.UserNameLength);
		    output.SecondUserNameLength = (byte)await output.ReadSingle();
		    await output.ReadAmount(3); //TODO ???
			output.SecondUsername = await output.ReadAmount(output.SecondUserNameLength);
		    return output;
	    }

	    public async Task<string> ReadAmount(int amountToRead)
	    {
		    var buffer = new char[amountToRead];
		    await _textToParse.ReadAsync(buffer, 0, amountToRead);
		    return new string(buffer);
	    }

	    public async Task<char> ReadSingle()
	    {
		    return (await ReadAmount(1))[0];
	    }
		
	    

	    public string Id { get; set; }
	    public string EntryType { get; set; } //19 bytes
		public string UnknowData { get; set; }//51 bytes
		public byte UserNameLength { get; set; }
		public string UserName { get; set; }
		public byte SecondUserNameLength { get; set; }
		public string SecondUsername { get; set; }
	    public string Data { get; set; }
	}
}
