using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyser
{
    class Entry : DeserializerHelper
    {
	    string _bytes;
	    public Entry(TextReader textToParse, byte[] bytes): base(textToParse)
	    {
		    _bytes = BitConverter.ToString(bytes);
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
			var unknowInt = BitConverter.ToInt32(Encoding.ASCII.GetBytes(await output.ReadAmount(4)), 0);
			output.UnknowData = string.Join(';', new int[]
		    {
			    BitConverter.ToInt16(Encoding.ASCII.GetBytes(await output.ReadAmount(4)), 0),
			    BitConverter.ToInt16(Encoding.ASCII.GetBytes(await output.ReadAmount(2)), 0),
			    BitConverter.ToInt16(Encoding.ASCII.GetBytes(await output.ReadAmount(2)), 0),
			    BitConverter.ToInt16(Encoding.ASCII.GetBytes(await output.ReadAmount(2)), 0),
			    BitConverter.ToInt16(Encoding.ASCII.GetBytes(await output.ReadAmount(2)), 0),
		    }) ;
			
		    var entrySize = BitConverter.ToInt32(Encoding.ASCII.GetBytes(await output.ReadAmount(4)), 0);
		    if (entrySize > byte.MaxValue)
		    {
				//throw new InvalidDataException();
			    return null;
		    }
		    output.KillEntry = new KillEntry(await output.ReadAmount(entrySize));

		    /*var userNameLength = BitConverter.ToInt32(Encoding.ASCII.GetBytes(await output.ReadAmount(4)), 0);
			output.VictimUserName = await output.ReadAmount(userNameLength);
		    var secondUserNameLength = BitConverter.ToInt32(Encoding.ASCII.GetBytes(await output.ReadAmount(4)), 0);
			output.KillerUsername = await output.ReadAmount(secondUserNameLength);*/
		    return output;
	    }

	    

	    public string Id { get; set; }
	    public string EntryType { get; set; }
		public string UnknowData { get; set; }

		//public string UnknowData { get; set; }
		public KillEntry KillEntry { get; set; }


	}
}
