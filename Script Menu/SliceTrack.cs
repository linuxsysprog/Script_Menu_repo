// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Split event(s) on the target track at the start of
// each (measure start) event on the source track

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
				Common.SLICE_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
				Common.SLICE_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// make sure each source event has at least one target event that
		// could be split
		foreach (TrackEvent sourceEvent in sourceEvents) {
			string eventName = Common.getFullName(Common.getTakeNames(sourceEvent));
			if (Common.FindEventsByEvent(targetTrack, sourceEvent).Count < 1) {
				MessageBox.Show("Source event " + sourceEvent.Index +
				(eventName == "" ? "" : " (" + eventName + ")") + " does not have a matching target event",
					Common.SLICE_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
				RemoveEmptyTakes(secondHalfEvent);
				AddEmptyTake(secondHalfEvent, sourceEventName);
				
				splitEvents++;
			}
		}
		
		// report
		MessageBox.Show("Split " + splitEvents + " events", Common.SLICE_TRACK);
	}
	
	// add an empty take to the event specified.
	// The event could be either audio or video
	private TrackEvent AddEmptyTake(TrackEvent @event, string label) {
		Media media;
		
		if (@event.MediaType == MediaType.Audio) {
			media = new Media(Common.vegas.InstallationDirectory + "\\Script Menu\\AddBeep.wav\\empty.wav");
			(@event.AddTake(media.GetAudioStreamByIndex(0))).Name = label + Common.SPACER;
		} else if (@event.MediaType == MediaType.Video) {
			media = new Media(Common.vegas.InstallationDirectory + "\\Script Menu\\AddRuler.png\\empty.png");
			(@event.AddTake(media.GetVideoStreamByIndex(0))).Name = label + Common.SPACER;
		} else {
			throw new Exception("event media type is neither audio nor video");
		}

		return @event;
	}
	
	// remove empty takes from the event specified
	private void RemoveEmptyTakes(TrackEvent @event) {
		foreach (Take take in @event.Takes) {
			if (new Regex("empty\\.(wav|png)$").Match(take.MediaPath).Success) {
				@event.Takes.Remove(take);
			}
		}
	}
	
}

