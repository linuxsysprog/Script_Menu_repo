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
		selection.Normalize();
		
		// check for the user has selected exactly one track
		// either audio or video
		List<Track> tracks = Common.FindSelectedTracks(vegas.Project.Tracks);
		if (tracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one track selected",
				Common.ADD_RULER, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// find events
		List<TrackEvent> events;
		if (selection.SelectionLength == new Timecode()) {
			events = Common.TrackEventsToTrackEvents(tracks[0].Events);
		} else {
			events = Common.FindEventsBySelection(tracks[0], selection);
		}

		// clear output window
		vegas.DebugClear();
		
		if (events.Count < 1) {
			vegas.DebugOut("No events found.");
			return;
		}
		
		string spacer = "    " + "    ";
		foreach (TrackEvent @event in events) {
			vegas.DebugOut("Event " + @event.Index + spacer + @event.Start + " " +
				@event.Length + " " + @event.End + " ");
		
			foreach (Take take in @event.Takes) {
				if (take.Media.Generator == null) {
					string takeName = "n/a";
					if (take.Name.IndexOf(Common.SPACER) != -1) {
						takeName = take.Name.Substring(0, take.Name.IndexOf(Common.SPACER));
					}
				
					vegas.DebugOut("    Take " + take.Index + spacer + takeName + spacer +
						Common.Basename(take.Media.FilePath));
				} else {
					vegas.DebugOut("    Take " + take.Index + spacer + take.Name);
				}
			}
		}
	}
	
}

