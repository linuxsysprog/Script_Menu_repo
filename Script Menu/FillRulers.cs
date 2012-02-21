// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Fill top/bottom ruler tracks with subdivision

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
		
		// all events should have correct labels
		foreach (VideoEvent @event in events) {
			string eventName = Common.getFullName(Common.getTakeNames(@event));
			if (new Regex(Common.VIDEO_RE).Matches(eventName).Count <= 0) {
				MessageBox.Show("Event " + @event.Index +
				(eventName == "" ? "" : " (" + eventName + ")") + " has an incorrect label",
					Common.FILL_RULERS, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}
		
		// clear log
		Common.vegas.DebugClear();
		
		// insert events
		int insertedEvents = 0;
		for (int i = 0; i < events.ToArray().Length - 1; i++) {
			string name = Common.getTakeNames(events.ToArray()[i])[0];
			bool topRuler = new Regex("^\\d+ T").Matches(name).Count > 0 ? true : false;
			int keyRulerNumber = Convert.ToInt32(new Regex("^(\\d+) (T|B)").Match(name).Groups[1].Value);
			
			// number of rulers to insert
			int nRulers = topRuler ? 2 : 3;
			
			double step = (events.ToArray()[i + 1].Start - events.ToArray()[i].Start).ToMilliseconds() /
				(double)(nRulers + 1);
			Timecode offset = events.ToArray()[i].Start + Timecode.FromMilliseconds(step);
			
			for (int j = 0; j < nRulers; j++) {
				// for debugging
				// Common.vegas.DebugOut("i = " + i + " j = " + j + " name = " + name + " nRulers = " +
					// nRulers + " step = " + Timecode.FromMilliseconds(step) + " offset = " + offset +
					// " topRuler = " + topRuler + " rulerNumber = " + (keyRulerNumber + j + 1) );
					
				// insert event
				VideoEvent result = AddVideoEvent(selectedVideoTracks[0],
					Common.getFullName(Common.getTakeNames(events.ToArray()[i])), offset,
					topRuler, keyRulerNumber + j + 1);
				if (result != null) {
					insertedEvents++;
				}
				
				offset = offset + Timecode.FromMilliseconds(step);
			}
		}
		
		// report
		MessageBox.Show("Inserted " + insertedEvents + " events", Common.FILL_RULERS);
	}
	
	// add a video event with a top/bottom ruler onto tagretTrack.
	// Quantize position to the nearest frame
	private VideoEvent AddVideoEvent(VideoTrack tagretTrack, string keyEventName, Timecode position,
							bool topRuler, int rulerNumber) {
		QuantizedEvent quantizedEvent = QuantizedEvent.FromTimecode(position);

		// write to log.
		List<TrackEvent> events = Common.FindEventsByPosition(tagretTrack, quantizedEvent.QuantizedStart);

		string spacer = "    " + "    ";
		Common.vegas.DebugOut(quantizedEvent + " " + (events.Count > 0 ? " skipped " : "         ") +
				rulerNumber + " " + (topRuler == true ? "T": "B") + spacer + keyEventName);

		// do not insert into the same slot more than once
		if (events.Count > 0) {
			return null;
		}
		
		return Video.AddRuler(tagretTrack, quantizedEvent.QuantizedStart, topRuler, rulerNumber, null);
	}
	
}

