// File: TrackOutline.cs - Create a track which outlines an existing track

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
		selection.Normalize();
		
		// check for the user has selected exactly two tracks
		// either audio or video
		List<Track> tracks = Common.FindSelectedTracks(vegas.Project.Tracks);
		if (tracks.Count != 2) {
			MessageBox.Show("Please make sure you have exactly two tracks selected",
				Common.TRACK_OUT, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// find events
		List<TrackEvent>[] events = new List<TrackEvent>[2];
		for (int i = 0; i < 2; i++) {
			if (selection.SelectionLength == new Timecode()) {
				events[i] = Common.TrackEventsToTrackEvents(tracks[i].Events);
			} else {
				events[i] = Common.FindEventsBySelection(tracks[i], selection);
			}
		}

		// clear output window
		vegas.DebugClear();
		
		if ((events[0].Count == 0 && events[1].Count == 0) ||
				(events[0].Count != 0 && events[1].Count != 0)) {
			MessageBox.Show("Please make sure one track (selection) is empty and the other has at least one event",
				Common.TRACK_OUT, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		MessageBox.Show("Invaders!");
		
		// if (events.Count < 1) {
			// vegas.DebugOut("No events found.");
			// return;
		// }
		
		// string spacer = "    " + "    ";
		// foreach (TrackEvent @event in events) {
			// vegas.DebugOut("Event " + @event.Index + spacer + @event.Start + " " +
				// @event.Length + " " + @event.End + " ");
		
			// foreach (Take take in @event.Takes) {
				// if (take.Media.Generator == null) {
					// string takeName = "n/a";
					// if (take.Name.IndexOf(Common.SPACER) != -1) {
						// takeName = take.Name.Substring(0, take.Name.IndexOf(Common.SPACER));
					// }
				
					// vegas.DebugOut("    Take " + take.Index + spacer + takeName + spacer +
						// Common.Basename(take.Media.FilePath));
				// } else {
					// vegas.DebugOut("    Take " + take.Index + spacer + take.Name);
				// }
			// }
		// }
	}
	
}

