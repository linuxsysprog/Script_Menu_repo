// File: MyScript.cs

using System;
using System.Windows.Forms;
using Sony.Vegas;
using UtilityMethods;
using System.Collections.Generic;
using AddRulerNamespace;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(vegas.Project)))
		);
		
		if (selectedVideoTracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one video track selected",
				"Add Ruler", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		List<TrackEvent> events = Common.FindEventsByPosition(selectedVideoTracks[0], Timecode.FromFrames(25));
		MessageBox.Show("" + events.Count);
    }
	
}
