// File: Beeps2Rulers.cs - Promote beeps to bottom rulers

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
		VideoTrack targetTrack;
		List<AudioEvent> sourceEvents;
		List<VideoEvent> targetEvents;
		
		Selection selection = new Selection(vegas.Transport.SelectionStart, vegas.Transport.SelectionLength);
		selection.Normalize();
		
		// check for the user has selected exactly two tracks, one audio (source track)
		// and the other video (target track)
		List<Track> tracks = Common.FindSelectedTracks(vegas.Project.Tracks);
		try {
			if (tracks.Count != 2) {
				throw new Exception("track count not equals two");
			}
			
			if (!((tracks[0].IsAudio() && tracks[1].IsVideo()) ||
					(tracks[1].IsAudio() && tracks[0].IsVideo()))) {
				throw new Exception("tracks are of same type");
			}
		} catch (Exception ex) {
			MessageBox.Show("Please make sure you have exactly two tracks selected. " +
				"One audio (source track) and the other video (target track)",
				Common.BEEPS_RULERS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// sort out tracks
		if (tracks[0].IsAudio()) {
			sourceTrack = (AudioTrack)tracks[0];
			targetTrack = (VideoTrack)tracks[1];
		} else {
			sourceTrack = (AudioTrack)tracks[1];
			targetTrack = (VideoTrack)tracks[0];
		}
		
		// deal with selections
		if (selection.SelectionLength == new Timecode()) {
			sourceEvents = Common.EventsToAudioEvents(Common.TrackEventsToTrackEvents(sourceTrack.Events));
			targetEvents = Common.EventsToVideoEvents(Common.TrackEventsToTrackEvents(targetTrack.Events));
		} else {
			sourceEvents = Common.EventsToAudioEvents(Common.FindEventsBySelection(sourceTrack, selection));
			targetEvents = Common.EventsToVideoEvents(Common.FindEventsBySelection(targetTrack, selection));
		}
		
		// dump lists
		// Common.vegas.DebugClear();
		// foreach (AudioEvent audioEvent in sourceEvents) {
			// Common.vegas.DebugOut("Audio: " + audioEvent.Start);
		// }
		// foreach (VideoEvent videoEvent in targetEvents) {
			// Common.vegas.DebugOut("Video: " + videoEvent.Start);
		// }
		
		// to continue, the target track (selection) should be empty and the source
		// track (selection) should have at least one event
		if (!(targetEvents.Count == 0 && sourceEvents.Count > 0)) {
			MessageBox.Show("Please make sure the target track (selection) is empty " +
				"and the source track (selection) has at least one event",
				Common.BEEPS_RULERS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// find and insert events
		int insertedEvents = 0;
		foreach (AudioEvent @event in sourceEvents) {
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
			AddVideoEvent(targetTrack, @event);
			insertedEvents++;
		}
		
		// report
		MessageBox.Show("Inserted " + insertedEvents + " events", Common.BEEPS_RULERS);
	}
	
	// add a video event with a bottom ruler onto specified target video track.
	// Copy position and take names from the source audio event.
	private VideoEvent AddVideoEvent(VideoTrack tagretTrack, AudioEvent sourceEvent) {
		VideoEvent videoEvent = tagretTrack.AddVideoEvent(sourceEvent.Start, Timecode.FromFrames(1));
		
		foreach (Take take in sourceEvent.Takes) {
			string path;
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
			} else {
				path = Common.vegas.InstallationDirectory + "\\Script Menu\\AddRuler.png\\empty.png";
			}

			Media media = new Media(path);
			(videoEvent.AddTake(media.GetVideoStreamByIndex(0), activeTake)).Name = take.Name;
		}
		
		return videoEvent;
	}
	
}

