using System;
using System.Collections.Generic;
using System.Text;

namespace Analyser
{
    class Entry
    {
	    public Entry(string textToParse)
	    {
		    Id = textToParse.Substring(0, 33);
		    ConstantInitialData = textToParse.Substring(34, 19);
		    if (ConstantInitialData != "\v\0\0\0playerElim\0\0\0\0\0")
		    {
				Console.WriteLine("Unsupported entry");
			    return;
		    }
		    UnknowData = textToParse.Substring(54, 51);
		    UserNameLength = (byte) textToParse[105];
		    try
		    {
			    UserName = textToParse.Substring(106+3, UserNameLength);
			    Data = textToParse.Substring(106 + UserNameLength+3);
		    }
		    catch
		    {
				Console.WriteLine("ah");
		    }
		    
		    

	    }

		
	    

	    public string Id { get; set; }
	    public string ConstantInitialData { get; set; } //19 bytes
		public string UnknowData { get; set; }//51 bytes
		public byte UserNameLength { get; set; }
		public string UserName { get; set; }
	    public string Data { get; set; }
	}
}
