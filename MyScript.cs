// File: MyScript.cs

using System.Windows.Forms;
using Sony.Vegas;
using UtilityMethods;
using System.Collections.Generic;
using AddRuler;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		Track track = Common.FindSelectedTrack(vegas.Project);
		
		// if (track == null) {
			// MessageBox.Show("Track not found");
		// } else {
			// MessageBox.Show("Track found: Name: " + track.Name +
				// " Index: " + track.Index +
				// " Display Index: " + track.DisplayIndex);
		// }
		
		TrackEvent[] trackEvents = Common.FindSelectedEvents(vegas.Project);
		
		if (trackEvents.Length > 0) {
			MessageBox.Show(trackEvents.Length + " event(s) found");
		} else {
			MessageBox.Show("No events found");
		}
		
    }

}
