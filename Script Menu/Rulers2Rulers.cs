// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Promote bottom rulers to top rulers

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint {
	private Regex regex = new Regex("^(1|5|9|13) B");

    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;
		VideoTrack sourceTrack;
		VideoTrack targetTrack;
		List<VideoEvent> sourceEvents;
		List<VideoEvent> targetEvents;
		
		Selection selection = new Selection(vegas.Transport.SelectionStart, vegas.Transport.SelectionLength);
		selection.Normalize();
		
		// check for the user has selected exactly two video tracks
		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(Common.vegas.Project)))
		);
		if (selectedVideoTracks.Count != 2) {
			MessageBox.Show("Please make sure you have exactly two video tracks selected",
				Common.RULERS_RULERS, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
				Common.RULERS_RULERS, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
		
		// find and insert events
		int insertedEvents = 0;
		foreach (VideoEvent @event in sourceEvents) {
			// for event to qualify it should have at least one take that matches 
			// ^(1|5|9|13) B pattern
			bool eventOK = false;
			foreach (Take take in @event.Takes) {
				if (take.MediaStream == null ||
						take.MediaStream.MediaType != MediaType.Video) {
					continue;
				}
				
				if (regex.Matches(take.Name).Count > 0) {
					eventOK = true;
					break;
				}
			}
			if (!eventOK) {
				continue;
			}
			
			// create event
			VideoEvent result = AddVideoEvent(targetTrack, @event);
			if (result != null) {
				insertedEvents++;
			}
		}
		
		// report
		MessageBox.Show("Inserted " + insertedEvents + " events", Common.RULERS_RULERS);
	}
	
	// add a video event with a top ruler onto specified target video track.
	// Copy position and take names from the source video event
	private VideoEvent AddVideoEvent(VideoTrack targetTrack, VideoEvent sourceEvent) {
		// do not insert into the same slot more than once
		List<TrackEvent> events = Common.FindEventsByPosition(targetTrack, sourceEvent.Start);
		if (events.Count > 0) {
			return null;
		}
		
		// insert event
		VideoEvent videoEvent = targetTrack.AddVideoEvent(sourceEvent.Start, Timecode.FromFrames(1));
		videoEvent.MaintainAspectRatio = false;
		
		// copy take names across
		foreach (Take take in sourceEvent.Takes) {
			string path;
			Media media;
			
			if (regex.Matches(take.Name).Count > 0) {
				int rulerNumber = Convert.ToInt32(regex.Match(take.Name).Groups[1].Value);

				// tweak ruler number
				if (rulerNumber == 5) {
					rulerNumber = 4;
				} else if (rulerNumber == 9) {
					rulerNumber = 7;
				} else if (rulerNumber == 13) {
					rulerNumber = 10;
				}
				
				path = Video.GetPNGDirectory() + "\\" +
					Common.LocationNumber2Basename(true, rulerNumber);
				media = new Media(path);
				(videoEvent.AddTake(media.GetVideoStreamByIndex(0), true)).Name =
					rulerNumber + " T" + Common.SPACER;
			} else {
				path = Common.vegas.InstallationDirectory + "\\Script Menu\\AddRuler.png\\empty.png";
				media = new Media(path);
				(videoEvent.AddTake(media.GetVideoStreamByIndex(0), false)).Name = take.Name;
			}
		}
		
		return videoEvent;
	}
	
}

