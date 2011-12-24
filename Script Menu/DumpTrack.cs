// File: DumpTrack.cs - Print a dump of a selection of a track's
//           event's take's Name fields

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;
		
		Selection selection = new Selection(vegas.Transport.SelectionStart, vegas.Transport.SelectionLength);
		MessageBox.Show("" + selection.Normalize());
		return;
		
		// check for the user has selected exactly one track
		// either audio or video
		List<Track> tracks = Common.FindSelectedTracks(vegas.Project.Tracks);
		if (tracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one track selected",
				Common.ADD_RULER, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		vegas.DebugClear();
		string spacer = "    " + "    ";

		foreach (TrackEvent @event in tracks[0].Events) {
			vegas.DebugOut("Event " + @event.Index + spacer + @event.Start + " " +
				@event.Length + " " + @event.End + " ");
		
			foreach (Take take in @event.Takes) {
				vegas.DebugOut("    Take " + take.Index + spacer +
					take.Name.Substring(0, take.Name.IndexOf(Common.SPACER)) + spacer +
					Common.Basename(take.Media.FilePath));
			}
		}
	}
	
}

