// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Trim Start and End of each target event according to
// its corresponding (measure start) source event.
// Accomodate (compress/stretch) media to the new event's length.
// The script is media-neutral

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
				Common.MATCH_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
				Common.MATCH_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// make sure that for each source event there's exatly one namesake target event
		foreach (TrackEvent sourceEvent in sourceEvents) {
			string sourceEventName = TrimEventName(Common.getFullName(Common.getTakeNames(sourceEvent)));
			
			List<TrackEvent> targetEvents =
				FindEventsByName(Common.TrackEventsToTrackEvents(targetTrack.Events), sourceEventName);

			if (targetEvents == null || targetEvents.Count != 1) {
				MessageBox.Show("Source event " + sourceEvent.Index +
				(sourceEventName == "" ? "" : " (" + sourceEventName + ")") +
					" has none or more than one namesakes",
					Common.MATCH_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}
		
		// adjust Start of all target events
		int adjustedEvents = 0;
		foreach (TrackEvent sourceEvent in sourceEvents) {
			string sourceEventName = TrimEventName(Common.getFullName(Common.getTakeNames(sourceEvent)));
			
			List<TrackEvent> targetEvents =
				FindEventsByName(Common.TrackEventsToTrackEvents(targetTrack.Events), sourceEventName);
				
			if (targetEvents[0].Start != sourceEvent.Start) {
				targetEvents[0].Start = sourceEvent.Start;
				adjustedEvents++;
			}
		}
		
		// report
		MessageBox.Show("Adjusted start position of " + adjustedEvents + " events", Common.MATCH_TRACK);
		
		// adjust Length of all (but last) target events. Accomodate media
		adjustedEvents = 0;
		for (int i = 0; i < sourceEvents.ToArray().Length - 1; i++) {
			string prevSourceEventName =
				TrimEventName(Common.getFullName(Common.getTakeNames(sourceEvents.ToArray()[i])));
			string nextSourceEventName =
				TrimEventName(Common.getFullName(Common.getTakeNames(sourceEvents.ToArray()[i + 1])));
			
			List<TrackEvent> prevTargetEvents =
				FindEventsByName(Common.TrackEventsToTrackEvents(targetTrack.Events), prevSourceEventName);
			List<TrackEvent> nextTargetEvents =
				FindEventsByName(Common.TrackEventsToTrackEvents(targetTrack.Events), nextSourceEventName);

			if (prevTargetEvents[0].End != nextTargetEvents[0].Start) {
				// adjust the length
				Timecode oldLength = prevTargetEvents[0].Length;
				prevTargetEvents[0].End = nextTargetEvents[0].Start;
				
				// accomodate media
				if (prevTargetEvents[0].Length.Nanos == 0) {
					throw new Exception("event " + prevTargetEvents[0].Index + " has zero length");
				}
				
				Double playbackRate = oldLength.Nanos / (double)prevTargetEvents[0].Length.Nanos;
				prevTargetEvents[0].AdjustPlaybackRate(playbackRate, true);
				
				adjustedEvents++;
			}
		}
		
		// report
		MessageBox.Show("Adjusted the length of " + adjustedEvents + " events", Common.MATCH_TRACK);
	}
	
	// for events matching the pattern ^N.N or ^N X
	// trim down their name to (^N.N) or (^N X) respectively
	private string TrimEventName(string eventName) {
		if (new Regex(Common.AUDIO_RE).Match(eventName).Success) {
			return new Regex(Common.AUDIO_RE).Match(eventName).Groups[0].Value;
		}
		
		if (new Regex(Common.VIDEO_RE).Match(eventName).Success) {
			return new Regex(Common.VIDEO_RE).Match(eventName).Groups[0].Value;
		}
		
		return eventName;
	}
	
	// filter events by their abbreviated name
	private List<TrackEvent> FindEventsByName(List<TrackEvent> events, string name) {
		List<TrackEvent> foundEvents = new List<TrackEvent>();
		
		foreach (TrackEvent @event in events) {
			if (TrimEventName(Common.getFullName(Common.getTakeNames(@event))) == name) {
				foundEvents.Add(@event);
			}
		}
		
		return foundEvents;
	}
	
}

