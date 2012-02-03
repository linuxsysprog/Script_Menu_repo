// File: Rulers2Rulers.cs - Promote bottom rulers to top rulers

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
		
		MessageBox.Show("Invaders!");
		return;/*
		
		// clear log
		Common.vegas.DebugClear();
		
		// find and insert events
		int insertedEvents = 0;
		foreach (VideoEvent @event in sourceEvents) {
			// for event to qualify it should have at least one take that matches 
			// ^N.[1-4] pattern
			bool eventOK = false;
			foreach (Take take in @event.Takes) {
				if (take.MediaStream == null) {
					continue;
				}
				
				if (take.MediaStream.MediaType != MediaType.Audio &&
						take.MediaStream.MediaType != MediaType.Video) {
					continue;
				}
				
				if (new Regex("^\\d+\\.[1-4]").Matches(take.Name).Count > 0) {
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
		MessageBox.Show("Inserted " + insertedEvents + " events", Common.RULERS_RULERS);*/
	}
	
	// add a video event with a bottom ruler onto specified target video track.
	// Quantize original position to the nearest frame. Copy take names from the source audio event.
	private VideoEvent AddVideoEvent(VideoTrack tagretTrack, VideoEvent sourceEvent) {
		// quantize position to frames
		double frames = Convert.ToDouble(sourceEvent.Start.ToString(RulerFormat.AbsoluteFrames));
		int nearestFrame = (int)Math.Round(frames);
		
		// a frame could be fast, slow or perfect
		string frameStatus;
		Timecode offset = new Timecode();
		if (Timecode.FromFrames(nearestFrame) == sourceEvent.Start) {
			frameStatus = "P";
		} else if (Timecode.FromFrames(nearestFrame) > sourceEvent.Start) {
			frameStatus = "S";
			offset = Timecode.FromFrames(nearestFrame) - sourceEvent.Start;
		} else { // Timecode.FromFrames(nearestFrame) < sourceEvent.Start
			frameStatus = "F";
			offset = sourceEvent.Start - Timecode.FromFrames(nearestFrame);
		}
		
		// write to log.
		string spacer = "    " + "    ";
		List<TrackEvent> events = Common.FindEventsByPosition(tagretTrack, Timecode.FromFrames(nearestFrame));

		Common.vegas.DebugOut("Event " + sourceEvent.Index + spacer + sourceEvent.Start + " " +
				Timecode.FromFrames(nearestFrame) + spacer + frameStatus + " " + offset +
				(events.Count > 0 ? " skipped " : "         ") +
				Common.getFullName(Common.getTakeNames(sourceEvent)));

		// do not insert into the same slot more than once
		if (events.Count > 0) {
			return null;
		}
		
		// insert event at the nearest frame
		VideoEvent videoEvent = tagretTrack.AddVideoEvent(Timecode.FromFrames(nearestFrame),
			Timecode.FromFrames(1));
		
		foreach (Take take in sourceEvent.Takes) {
			string path;
			Media media;
			bool activeTake = new Regex("^\\d+\\.[1-4]").Matches(take.Name).Count > 0;
			
			if (activeTake) {
				int beat = Convert.ToInt32(new Regex("^\\d+\\.([1-4])").Match(take.Name).Groups[1].Value);
				
				// convert beat to ruler number
				if (beat == 2) {	
					beat = 5;
				} else if (beat == 3) {
					beat = 9;
				} else if (beat == 4) {
					beat = 13;
				}
				
				path = Common.vegas.InstallationDirectory + "\\Script Menu\\AddRuler.png\\" +
					Common.LocationNumber2Basename(false, beat);
				media = new Media(path);
				(videoEvent.AddTake(media.GetVideoStreamByIndex(0), true)).Name =
					beat + " B" + Common.SPACER;
			} else {
				path = Common.vegas.InstallationDirectory + "\\Script Menu\\AddRuler.png\\empty.png";
				media = new Media(path);
				(videoEvent.AddTake(media.GetVideoStreamByIndex(0), false)).Name = take.Name;
			}
		}
		
		return videoEvent;
	}
	
}

