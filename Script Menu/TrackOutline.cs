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
		Regex regex = new Regex("^\\d+\\.\\d+");
		Track sourceTrack;
		Track targetTrack;
		
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
		
		// sort out which track is which
		if (events[1].Count == 0) {
			sourceTrack = tracks[0];
			targetTrack = tracks[1];
		} else {
			sourceTrack = tracks[1];
			targetTrack = tracks[0];
		}
		
		if (targetTrack.IsAudio()) {
			addAudioEvent((AudioTrack)targetTrack, vegas.Transport.CursorPosition, "hello");
		}
		
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
		
		// foreach (TrackEvent @event in sourceTrack.Events) {
			// find event with a take that matches the pattern
			// string label = null;
			// foreach (Take take in @event.Takes) {
				// if (regex.Matches(take.Name).Count > 0) {
					// label = take.Name;
					// break;
				// }
			// }
			// if (label == null) {
				// continue;
			// }
			
			// create event
			// vegas.DebugOut(label);
		// }
	}
	
	// add an audio event 1s long to the track specified at the specified position
	private AudioEvent addAudioEvent(AudioTrack audioTrack, Timecode position, string label) {
		Common.vegas.DebugOut("is audioTrack null = " + (audioTrack == null) + "\n" +
				"is position null = " + (position == null) + "\n" +
				"label = " + label);
		
		string path = Common.vegas.InstallationDirectory + "\\Script Menu\\AddBeep.wav";

		Media media = new Media(path + "\\high.wav");
		
		AudioEvent audioEvent = audioTrack.AddAudioEvent(position, Timecode.FromMilliseconds(1000.0));
		
		(audioEvent.AddTake(media.GetAudioStreamByIndex(0))).Name = label + Common.SPACER;
		// media = new Media(path + "\\beep.1.wav");
		// (audioEvent.AddTake(media.GetAudioStreamByIndex(0))).Name = measure +
			// "." + beat + Common.SPACER;
		
		return audioEvent;
	}
	
}

