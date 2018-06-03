using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Analyser
{
    class SerializedProperty
    {
	    public static async Task<SerializedProperty> FromStream(TextReader stream)
	    {
		    var buffer = new char[4];
		    var numberOfCharRead = await stream.ReadBlockAsync(buffer, 0, buffer.Length);
		    if (numberOfCharRead == 0) return null;
		    var objectLength = BitConverter.ToInt32(Encoding.ASCII.GetBytes(buffer), 0);
			return new SerializedProperty(await DeserializerHelper.ReadAmount(stream, objectLength));
	    }
	    SerializedProperty(string data)
	    {
		    Data = data;
	    }
	    public string Data { get; }
    }
}
