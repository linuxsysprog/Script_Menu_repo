// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Spread out (or bring closer together) the beeps by a certain percentage.
// This script operates on a single audio track

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
		Track sourceTrack;
		Track targetTrack;
		List<TrackEvent> sourceEvents;
		
		Selection selection = new Selection(vegas.Transport.SelectionStart, vegas.Transport.SelectionLength);
		selection.Normalize();
		
		// check for the user has selected exactly two tracks. Either track could be
		// audio or video
		List<Track> tracks = Common.FindSelectedTracks(vegas.Project.Tracks);
		if (tracks.Count != 2) {
			MessageBox.Show("Please make sure you have exactly two tracks selected",
				Common.SPREAD_BEEPS, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

		// sort out which track and event collection is the source
		// and which is the target
		sourceTrack = tracks[0];
		targetTrack = tracks[1];
		
		sourceEvents = Common.FindMeasureStartEvents(events[0]);
		
		// source track (selection) should have at least one (measure start) event to continue
		if (sourceEvents.Count < 1) {
			MessageBox.Show("Please make sure source track (selection) has at least one (measure start) event",
				Common.SPREAD_BEEPS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// make sure each source event has at least one target event that
		// could be split
		foreach (TrackEvent sourceEvent in sourceEvents) {
			string eventName = Common.getFullName(Common.getTakeNames(sourceEvent));
			if (Common.FindEventsByEvent(targetTrack, sourceEvent).Count < 1) {
				MessageBox.Show("Source event " + sourceEvent.Index +
				(eventName == "" ? "" : " (" + eventName + ")") + " does not have a matching target event",
					Common.SPREAD_BEEPS, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}
		
		// find and split events. Copy take names
		int splitEvents = 0;
		foreach (TrackEvent sourceEvent in sourceEvents) {
			string sourceEventName = Common.getFullName(Common.getTakeNames(sourceEvent));
			
			List<TrackEvent> targetEvents = Common.FindEventsByEvent(targetTrack, sourceEvent);
			foreach (TrackEvent targetEvent in targetEvents) {
				// split event
				TrackEvent secondHalfEvent = targetEvent.Split(sourceEvent.Start - targetEvent.Start);
				
				// copy take names
				// RemoveEmptyTakes(secondHalfEvent);
				// AddEmptyTake(secondHalfEvent, sourceEventName);
				
				splitEvents++;
			}
		}
		
		// report
		MessageBox.Show("Split " + splitEvents + " events", Common.SPREAD_BEEPS);
	}
	
}

