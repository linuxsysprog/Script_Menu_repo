// File: MyScript.cs

using System.Windows.Forms;
using Sony.Vegas;
using UtilityMethods;
using System.Collections.Generic;
using AddRuler;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		List<VideoTrack> videoTracks = Video.FindVideoTracks(vegas.Project);
		MessageBox.Show("" + videoTracks.Count);
    }

}
