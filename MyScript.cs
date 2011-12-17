// File: MyScript.cs

using System.Windows.Forms;
using Sony.Vegas;
using UtilityMethods;
using System.Collections.Generic;
using AddRuler;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		List<AudioTrack> audioTracks = Audio.FindAudioTracks(vegas.Project);
		MessageBox.Show("" + audioTracks.Count);
    }

}
