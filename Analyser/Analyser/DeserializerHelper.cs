using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Analyser
{
    class DeserializerHelper
    {
	    readonly TextReader _textToParse;

	    public DeserializerHelper(TextReader textToParse)
	    {
		    _textToParse = textToParse;
	    }

	    protected async Task<string> ReadAmount(int amountToRead)
	    {
		    var buffer = new char[amountToRead];
		    await _textToParse.ReadBlockAsync(buffer, 0, amountToRead);
		    return new string(buffer);
	    }
	    public static async Task<string> ReadAmount(TextReader textToParse, int amountToRead)
	    {
		    var buffer = new char[amountToRead];
		    await textToParse.ReadBlockAsync(buffer, 0, amountToRead);
		    return new string(buffer);
	    }
	}
}
