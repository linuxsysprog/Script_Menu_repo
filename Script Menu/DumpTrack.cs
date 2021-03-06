// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Print a dump of a selection of a track's
// event's take's Name fields

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
				Common.DUMP_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
		
		vegas.DebugOut(Common.TrackEventsToString(events));
	}
	
}

