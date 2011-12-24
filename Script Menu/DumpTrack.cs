// File: DumpTrack.cs - Print a dump of a selection of a track's
//           event's take's Name fields

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;
		
		// check for the user has selected exactly one track
		// either audio or video
		List<Track> tracks = Common.FindSelectedTracks(vegas.Project.Tracks);
		if (tracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one track selected",
				Common.ADD_RULER, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		vegas.DebugClear();
		vegas.DebugOut("yo");
	}
}

