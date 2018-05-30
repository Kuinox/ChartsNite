using System;
using System.Collections.Generic;
using System.Text;

namespace Analyser
{
    class KillEntry
    {
	    public KillEntry(string killEntryString)
	    {
		    var test = Encoding.ASCII.GetBytes(killEntryString);
		    Data = killEntryString.Substring(0, 40);
		    var victimNameLength = BitConverter.ToInt32(Encoding.ASCII.GetBytes(killEntryString.Substring(40, 4)),0);
		    VictimUserName = killEntryString.Substring(44, victimNameLength);
		    var pointer = 44 + victimNameLength;
			var killerNameLength = BitConverter.ToInt32(Encoding.ASCII.GetBytes(killEntryString.Substring(pointer, 4)), 0);
		    pointer += 4;
			KillerUsername = killEntryString.Substring(pointer, killerNameLength);
		    pointer += killerNameLength;
		    EndData = killEntryString.Substring(pointer, killEntryString.Length-pointer);
	    }
	    public string VictimUserName { get; set; }
	    public string KillerUsername { get; set; }
	    public string Data { get; set; }
		public string EndData { get; set; }
	}


}
