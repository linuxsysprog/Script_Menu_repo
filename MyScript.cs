// File: MyScript.cs

using System;
using System.Windows.Forms;
using Sony.Vegas;
using UtilityMethods;
using System.Collections.Generic;
using AddRuler;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		List<AudioTrack> selectedAudioTracks = Common.TracksToAudioTracks(Common.FindSelectedTracks(Common.AudioTracksToTracks(Audio.FindAudioTracks(vegas.Project))));
		MessageBox.Show("" + selectedAudioTracks.Count);
    }
	
}
