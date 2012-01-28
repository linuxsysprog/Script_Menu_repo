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
		// Common.vegas.DebugClear();
		// foreach (AudioEvent audioEvent in sourceEvents) {
			// Common.vegas.DebugOut("Audio: " + audioEvent.Start);
		// }
		// foreach (VideoEvent videoEvent in targetEvents) {
			// Common.vegas.DebugOut("Video: " + videoEvent.Start);
		// }
		
		// to continue, the target track (selection) should be empty and the source
		// track (selection) should have at least one event
		if (!(targetEvents.Count == 0 && sourceEvents.Count > 0)) {
			MessageBox.Show("Please make sure the target track (selection) is empty " +
				"and the source track (selection) has at least one event",
				Common.BEEPS_RULERS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// insert frames
		foreach (AudioEvent audioEvent in sourceEvents) {
			// work out takes (take names)
		}
			
		MessageBox.Show("Inserted " + sourceEvents.Count + " bottom ruler frames", Common.BEEPS_RULERS);
	}
	
	// add an empty event to the track specified at the position specified.
	// The track could be either audio or video
	// private TrackEvent AddEmptyEvent(Track track, Timecode position, string label) {
		// Media media;
		// TrackEvent @event;
		// Timecode length;
		// Regex regex = new Regex("(" + AUDIO_RE + " .$)|(" + VIDEO_RE + " .$)");

		// if (regex.Matches(label).Count > 0) {
			// length = Timecode.FromMilliseconds(1000.0);
			// label = new Regex("^\\d+").Match(label).Groups[0].Value;
		// } else {
			// length = Timecode.FromMilliseconds(4000.0);
		// }
		
		// if (track.IsAudio()) {
			// media = new Media(Common.vegas.InstallationDirectory + "\\Script Menu\\AddBeep.wav\\empty.wav");
			// @event = (TrackEvent)((AudioTrack)track).AddAudioEvent(position, length);
			// (@event.AddTake(media.GetAudioStreamByIndex(0))).Name = label + Common.SPACER;
		// } else if (track.IsVideo()) {
			// media = new Media(Common.vegas.InstallationDirectory + "\\Script Menu\\AddRuler.png\\empty.png");
			// @event = (TrackEvent)((VideoTrack)track).AddVideoEvent(position, length);
			// (@event.AddTake(media.GetVideoStreamByIndex(0))).Name = label + Common.SPACER;
		// } else {
			// throw new Exception("track type is neither audio nor video");
		// }

		// return @event;
	// }
	
}

