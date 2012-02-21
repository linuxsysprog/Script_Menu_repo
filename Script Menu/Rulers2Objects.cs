// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Promote top/bottom rulers to Measure objects

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint {
	private Regex regex = new Regex("^1 (T|B)");

    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;
		VideoTrack sourceTrack;
		VideoTrack targetTrack;
		List<VideoEvent> sourceEvents;
		List<VideoEvent> targetEvents;
		
		const string frameSize = "320x240";
		const string @object = "Measure";
		
		// get text media generator
		PlugInNode plugIn = vegas.Generators.GetChildByName("Sony Text");
		if (plugIn == null) {
			MessageBox.Show("Couldn't find Sony Text media generator",
				Common.RULERS_OBJECTS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// get all presets
		List<Preset> presets = new List<Preset>();
		foreach (EffectPreset preset in plugIn.Presets) {
			try {
				presets.Add(new Preset(preset.Name));
			} catch (Exception e) {
				continue;
			}
		}

		// figure out measure range
		List<int> measures = new List<int>();
		foreach (Preset preset in presets) {
			if (preset.FrameSize == frameSize && preset.Object == @object) {
				try {
					int n = Convert.ToInt32(preset.Value);
					if (n < 1) {
						throw new Exception("value is less than one");
					}
					measures.Add(n);
				} catch (Exception ex) {
					MessageBox.Show("Invalid Preset (" + preset + "): " + ex.Message,
						Common.RULERS_OBJECTS, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}
		}
		measures.Sort();
		
		// dump measures
		// Common.vegas.DebugClear();
		// foreach (int measure in measures) {
			// Common.vegas.DebugOut("" + measure);
		// }
		
		Selection selection = new Selection(vegas.Transport.SelectionStart, vegas.Transport.SelectionLength);
		selection.Normalize();
		
		// check for the user has selected exactly two video tracks
		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(Common.vegas.Project)))
		);
		if (selectedVideoTracks.Count != 2) {
			MessageBox.Show("Please make sure you have exactly two video tracks selected",
				Common.RULERS_OBJECTS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// find events
		List<TrackEvent>[] events = new List<TrackEvent>[2];
		for (int i = 0; i < 2; i++) {
			if (selection.SelectionLength == new Timecode()) {
				events[i] = Common.TrackEventsToTrackEvents(selectedVideoTracks[i].Events);
			} else {
				events[i] = Common.FindEventsBySelection(selectedVideoTracks[i], selection);
			}
		}

		// to continue, one track (selection) should be empty and the other should not
		if ((events[0].Count == 0 && events[1].Count == 0) ||
				(events[0].Count != 0 && events[1].Count != 0)) {
			MessageBox.Show("Please make sure one track (selection) is empty and the other has at least one event",
				Common.RULERS_OBJECTS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// sort out which track and event collection is the source
		// and which is the target
		if (events[1].Count == 0) {
			sourceTrack = selectedVideoTracks[0];
			targetTrack = selectedVideoTracks[1];
			
			sourceEvents = Common.EventsToVideoEvents(events[0]);
			targetEvents = Common.EventsToVideoEvents(events[1]);
		} else {
			sourceTrack = selectedVideoTracks[1];
			targetTrack = selectedVideoTracks[0];
			
			sourceEvents = Common.EventsToVideoEvents(events[1]);
			targetEvents = Common.EventsToVideoEvents(events[0]);
		}
		
		// narrow down source events to a list of 1 (T|B)'s
		List<VideoEvent> filteredSourceEvents = new List<VideoEvent>();
		foreach (VideoEvent sourceEvent in sourceEvents) {
			string eventName = Common.getFullName(Common.getTakeNames(sourceEvent));
			if (regex.Match(eventName).Success) {
				filteredSourceEvents.Add(sourceEvent);
			}
		}
		
		if (filteredSourceEvents.Count < 1) {
			MessageBox.Show("No measure start events found",
				Common.RULERS_OBJECTS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// check if there's enough measure presets
		if (measures.Count < filteredSourceEvents.Count) {
			MessageBox.Show("Not enough Measure Presets (" + measures.Count + ")",
				Common.RULERS_OBJECTS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// insert measure objects
		// Common.vegas.DebugClear();
		{
			int i = 0;
			foreach (VideoEvent sourceEvent in filteredSourceEvents) {
				// create event
				// Common.vegas.DebugOut(frameSize + " " + @object + " " + measures.ToArray()[i++]);
				Common.AddTextEvent(Common.vegas, plugIn, targetTrack,
					frameSize + " " + @object + " " + measures.ToArray()[i++],
					sourceEvent.Start, Timecode.FromFrames(1));
			}
		}
		
		// report
		MessageBox.Show("Inserted " + filteredSourceEvents.Count + " events", Common.RULERS_OBJECTS);
	}
	
}

