// File: Beeps2Rulers.cs - Promote beeps to bottom rulers

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
		AudioTrack sourceTrack;
		VideoTrack targetTrack;
		List<AudioEvent> sourceEvents;
		List<VideoEvent> targetEvents;
		
		Selection selection = new Selection(vegas.Transport.SelectionStart, vegas.Transport.SelectionLength);
		selection.Normalize();
		
		// check for the user has selected exactly two tracks, one audio (source track)
		// and the other video (target track)
		List<Track> tracks = Common.FindSelectedTracks(vegas.Project.Tracks);
		try {
			if (tracks.Count != 2) {
				throw new Exception("track count not equals two");
			}
			
			if (!((tracks[0].IsAudio() && tracks[1].IsVideo()) ||
					(tracks[1].IsAudio() && tracks[0].IsVideo()))) {
				throw new Exception("tracks are of same type");
			}
		} catch (Exception ex) {
			MessageBox.Show("Please make sure you have exactly two tracks selected. " +
				"One audio (source track) and the other video (target track)",
				Common.BEEPS_RULERS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		MessageBox.Show("Invaders!");
		/*
		// find events
		List<TrackEvent>[] events = new List<TrackEvent>[2];
		for (int i = 0; i < 2; i++) {
			if (selection.SelectionLength == new Timecode()) {
				events[i] = Common.TrackEventsToTrackEvents(tracks[i].Events);
			} else {
				events[i] = Common.FindEventsBySelection(tracks[i], selection);
			}
		}

		// to continue, one track (selection) should be empty and the other should not
		if ((events[0].Count == 0 && events[1].Count == 0) ||
				(events[0].Count != 0 && events[1].Count != 0)) {
			MessageBox.Show("Please make sure one track (selection) is empty and the other has at least one event",
				Common.BEEPS_RULERS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// sort out which track and event collection is the source
		// and which is the target
		if (events[1].Count == 0) {
			sourceTrack = tracks[0];
			targetTrack = tracks[1];
			
			sourceEvents = events[0];
			targetEvents = events[1];
		} else {
			sourceTrack = tracks[1];
			targetTrack = tracks[0];
			
			sourceEvents = events[1];
			targetEvents = events[0];
		}
		
		*/
	}
	
}

