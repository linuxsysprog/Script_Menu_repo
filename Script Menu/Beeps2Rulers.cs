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
		
		// sort out tracks
		if (tracks[0].IsAudio()) {
			sourceTrack = (AudioTrack)tracks[0];
			targetTrack = (VideoTrack)tracks[1];
		} else {
			sourceTrack = (AudioTrack)tracks[1];
			targetTrack = (VideoTrack)tracks[0];
		}
		
		// deal with selections
		if (selection.SelectionLength == new Timecode()) {
			sourceEvents = Common.EventsToAudioEvents(Common.TrackEventsToTrackEvents(sourceTrack.Events));
			targetEvents = Common.EventsToVideoEvents(Common.TrackEventsToTrackEvents(targetTrack.Events));
		} else {
			sourceEvents = Common.EventsToAudioEvents(Common.FindEventsBySelection(sourceTrack, selection));
			targetEvents = Common.EventsToVideoEvents(Common.FindEventsBySelection(targetTrack, selection));
		}
		
		// dump lists
		Common.vegas.DebugClear();
		foreach (AudioEvent audioEvent in sourceEvents) {
			Common.vegas.DebugOut("Audio: " + audioEvent.Start);
		}
		foreach (VideoEvent videoEvent in targetEvents) {
			Common.vegas.DebugOut("Video: " + videoEvent.Start);
		}
		
		// to continue, one track (selection) should be empty and the other should not
		// if ((events[0].Count == 0 && events[1].Count == 0) ||
				// (events[0].Count != 0 && events[1].Count != 0)) {
			// MessageBox.Show("Please make sure one track (selection) is empty and the other has at least one event",
				// Common.BEEPS_RULERS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			// return;
		// }
		
	}
	
}

