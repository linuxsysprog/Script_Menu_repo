// File: 2M.cs - Promote a beep track 2 Measure track

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
		AudioTrack targetTrack;
		Regex regex = new Regex("^\\d+\\.\\d+");

		List<AudioTrack> selectedAudioTracks = Common.TracksToAudioTracks(
			Common.FindSelectedTracks(Common.AudioTracksToTracks(Audio.FindAudioTracks(vegas.Project)))
		);
		if (selectedAudioTracks.Count != 2) {
			MessageBox.Show("Please make sure you have exactly two audio tracks selected",
				Common.ADD_BEEP, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		if ((selectedAudioTracks[0].Events.Count == 0 && selectedAudioTracks[1].Events.Count == 0) ||
				(selectedAudioTracks[0].Events.Count != 0 && selectedAudioTracks[1].Events.Count != 0)) {
			MessageBox.Show("Please make sure one audio track is empty and the other has at least one event",
				Common.ADD_BEEP, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		sourceTrack = selectedAudioTracks[0];
		targetTrack = selectedAudioTracks[1];
		
		// reverse tracks if the above guess was wrong
		if (targetTrack.Events.Count != 0) {
			sourceTrack = selectedAudioTracks[1];
			targetTrack = selectedAudioTracks[0];
		}
		
		// promote track
		vegas.DebugClear();
		foreach (TrackEvent @event in sourceTrack.Events) {
			// find event with a take that matches the pattern
			string label = null;
			foreach (Take take in @event.Takes) {
				if (regex.Matches(take.Name).Count > 0) {
					label = take.Name;
					break;
				}
			}
			if (label == null) {
				continue;
			}
			
			// create event
			vegas.DebugOut(label);
		}
		
		// report
		MessageBox.Show("Invaders!");
	}

}

