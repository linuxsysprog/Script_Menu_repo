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
				Common.RULERS_RULERS, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
				Common.RULERS_RULERS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// clear log
		Common.vegas.DebugClear();
		
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
			VideoEvent result = AddVideoEvent(targetTrack, @event);
			if (result != null) {
				insertedEvents++;
			}
		}
		
		// report
		MessageBox.Show("Inserted " + insertedEvents + " events", Common.RULERS_RULERS);
	}
	
	// add a video event with a bottom ruler onto specified target video track.
	// Quantize original position to the nearest frame. Copy take names from the source audio event.
	private VideoEvent AddVideoEvent(VideoTrack tagretTrack, AudioEvent sourceEvent) {
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

