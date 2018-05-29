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

	    public Entry(TextReader textToParse)
	    {
		    _textToParse = textToParse;

	    }
	    public static async Task<Entry> GetEntry(TextReader textToParse)
	    {
		    var output = new Entry(textToParse);
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

		    output.UnknowData = await output.ReadAmount(56);
		    var ah = Encoding.ASCII.GetBytes(output.UnknowData);
		    var userNameLength = BitConverter.ToInt32(Encoding.ASCII.GetBytes(await output.ReadAmount(4)), 0);

			output.UserName = await output.ReadAmount(userNameLength);
		    var secondUserNameLength = BitConverter.ToInt32(Encoding.ASCII.GetBytes(await output.ReadAmount(4)), 0);
			output.SecondUsername = await output.ReadAmount(secondUserNameLength);
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
		public string UserName { get; set; }
		public string SecondUsername { get; set; }
	    public string Data { get; set; }
	}
}
