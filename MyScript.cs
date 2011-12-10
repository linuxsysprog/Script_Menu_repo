// File: MyScript.cs

using System.Windows.Forms;
using Sony.Vegas;
using UtilityMethods;
using System.Collections.Generic;
using AddRuler;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		Common.AddTextEvent(vegas,
			(VideoTrack)Common.FindSelectedTrack(vegas.Project),
			Timecode.FromString("13.000"), Timecode.FromString("20.000"));
    }

}
