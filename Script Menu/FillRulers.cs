// File: FillRulers.cs - Fill top/bottom ruler tracks with subdivision

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
		List<VideoEvent> events;
		
		// check for the user has selected exactly one video track
		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(Common.vegas.Project)))
		);
		if (selectedVideoTracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one video track selected",
				Common.FILL_RULERS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// narrow down events by selection
		Selection selection = new Selection(vegas.Transport.SelectionStart, vegas.Transport.SelectionLength);
		selection.Normalize();
		
		if (selection.SelectionLength == new Timecode()) {
			events = Common.EventsToVideoEvents(
				Common.TrackEventsToTrackEvents(selectedVideoTracks[0].Events));
		} else {
			events = Common.EventsToVideoEvents(
				Common.FindEventsBySelection(selectedVideoTracks[0], selection));
		}
		
		// there must be at least two events on the track to continue
		if (events.Count < 2) {
			MessageBox.Show("Please make sure there are at least two events to continue",
				Common.FILL_RULERS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		MessageBox.Show("Invaders!");/*
		int eventsTrimmed = 0;
		for (int i = 0; i < events.ToArray().Length - 1; i++) {
			TrackEvent @event = events.ToArray()[i];
			
			if (@event.End < events.ToArray()[i + 1].Start) {
				@event.End = events.ToArray()[i + 1].Start;
				eventsTrimmed++;
			}
		}
		
		// report
		MessageBox.Show(eventsTrimmed + " out of " + events.Count +
			" total events were trimmed", Common.FILL_RULERS);*/
	}
	
}

