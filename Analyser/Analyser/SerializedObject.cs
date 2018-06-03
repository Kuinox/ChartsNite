using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Analyser
{
    class SerializedObject
    {
        public List<SerializedProperty> Properties { get; }

        public static async Task<SerializedObject> FromStream(TextReader stream)
        {
            var length = await DeserializerHelper.ReadAmount(stream, 4);
		    SerializedProperty property;
	        var output = new SerializedObject();
			do
		    {
			    property = await SerializedProperty.FromStream(stream);
			    output.Properties.Add(property);
			} while (property != null);
	        return output;
	    }
    }
}
