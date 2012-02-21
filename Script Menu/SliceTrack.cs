// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Split event(s) on the target track at the start of
// each event on the source track

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
		List<TrackEvent> targetEvents;
		
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
		
		sourceEvents = events[0];
		targetEvents = events[1];
		
		List<TrackEvent> foundEvents = Common.FindEventsByEvent(targetTrack, events[0][0]);
		Common.vegas.DebugOut("" + foundEvents.Count);
		
		// to continue: a) source track (selection) should have at least one event
		// b) at every source event start position there should exist a target event
		if (sourceEvents.Count > 1 &&
				) {
			MessageBox.Show("Please make sure source track (selection) has at least one event" +
				" and at every source event start position there exist a target event",
				Common.SLICE_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// find and insert events
		/*int insertedEvents = 0;
		foreach (TrackEvent @event in sourceEvents) {
			// for event to qualify it should have at least one take that matches our pattern
			bool eventOK = false;
			foreach (Take take in @event.Takes) {
				if (take.MediaStream == null) {
					continue;
				}
				
				if (take.MediaStream.MediaType != MediaType.Audio &&
						take.MediaStream.MediaType != MediaType.Video) {
					continue;
				}
				
				if (getRegex(take).Matches(take.Name).Count > 0) {
					eventOK = true;
					break;
				}
			}
			if (!eventOK) {
				continue;
			}
			
			// create event
			AddEmptyEvent(targetTrack, @event.Start, Common.getFullName(Common.getTakeNames(@event)));
			insertedEvents++;
		}
		
		// report
		MessageBox.Show("Inserted " + insertedEvents + " events", Common.SLICE_TRACK);*/
	}
	
	// returns a regex to match a measure start event.
	private static Regex getRegex(Take take) {
		if (take.MediaStream.MediaType == MediaType.Audio) {
			return new Regex("^\\d+\\.1");
		} else {
			return new Regex("^1 (T|B)");
		}
	}
	
	// add an empty event to the track specified at the position specified.
	// The track could be either audio or video
	private TrackEvent AddEmptyEvent(Track track, Timecode position, string label) {
		Media media;
		TrackEvent @event;
		Timecode length;
		Regex regex = new Regex("(^\\d+\\.1 .$)|(^1 (T|B) .$)");

		if (regex.Matches(label).Count > 0) {
			length = Timecode.FromMilliseconds(1000.0);
			label = new Regex("^\\d+").Match(label).Groups[0].Value;
		} else {
			length = Timecode.FromMilliseconds(4000.0);
		}
		
		if (track.IsAudio()) {
			media = new Media(Common.vegas.InstallationDirectory + "\\Script Menu\\AddBeep.wav\\empty.wav");
			@event = (TrackEvent)((AudioTrack)track).AddAudioEvent(position, length);
			(@event.AddTake(media.GetAudioStreamByIndex(0))).Name = label + Common.SPACER;
		} else if (track.IsVideo()) {
			media = new Media(Common.vegas.InstallationDirectory + "\\Script Menu\\AddRuler.png\\empty.png");
			@event = (TrackEvent)((VideoTrack)track).AddVideoEvent(position, length);
			(@event.AddTake(media.GetVideoStreamByIndex(0))).Name = label + Common.SPACER;
		} else {
			throw new Exception("track type is neither audio nor video");
		}

		return @event;
	}
	
}

