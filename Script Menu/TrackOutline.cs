// File: TrackOutline.cs - Create a track which outlines an existing track

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
		
		const string AUDIO_RE = "^\\d+\\.1";
		const string VIDEO_RE = "^1 (T|B)";
		
		Selection selection = new Selection(vegas.Transport.SelectionStart, vegas.Transport.SelectionLength);
		selection.Normalize();
		
		// check for the user has selected exactly two tracks. Either track could be
		// audio or video
		List<Track> tracks = Common.FindSelectedTracks(vegas.Project.Tracks);
		if (tracks.Count != 2) {
			MessageBox.Show("Please make sure you have exactly two tracks selected",
				Common.TRACK_OUT, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

		// to continue, one track (selection) should be empty and the other should not
		if ((events[0].Count == 0 && events[1].Count == 0) ||
				(events[0].Count != 0 && events[1].Count != 0)) {
			MessageBox.Show("Please make sure one track (selection) is empty and the other has at least one event",
				Common.TRACK_OUT, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// sort out which track and event collection is the source
		// and which is the target
		if (events[1].Count == 0) {
			sourceTrack = tracks[0];
			targetTrack = tracks[1];
			
			sourceEvents = events[0];
			targetEvents = events[1];
		} else {
			sourceTrack = tracks[1];
			targetTrack = tracks[0];
			
			sourceEvents = events[1];
			targetEvents = events[0];
		}
		
		
		// find events
		vegas.DebugClear();
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
				
				Regex regex;
				if (take.MediaStream.MediaType == MediaType.Audio) {
					regex = new Regex(AUDIO_RE);
				} else {
					regex = new Regex(VIDEO_RE);
				}
			
				if (regex.Matches(take.Name).Count > 0) {
					eventOK = true;
					break;
				}
			}
			if (!eventOK) {
				continue;
			}
			
			
			// compose label
			List<string> strings = new List<string>();
			string leadingString = "";
			foreach (Take take in @event.Takes) {
				// drop take names which do not have the spacer in them
				if (take.Name.IndexOf(Common.SPACER) == -1) {
					continue;
				}
				
				Regex regex;
				if (take.MediaStream.MediaType == MediaType.Audio) {
					regex = new Regex(AUDIO_RE);
				} else {
					regex = new Regex(VIDEO_RE);
				}
			
				if (regex.Matches(take.Name).Count > 0) {
					leadingString = take.Name.Substring(0, take.Name.IndexOf(Common.SPACER));
				} else {
					strings.Add(take.Name.Substring(0, take.Name.IndexOf(Common.SPACER)));
				}
			}
			
			// concatinate all into one string
			foreach (string @string in strings) {
				leadingString += " ";
				leadingString += @string;
			}
			
			// create event
			vegas.DebugOut(leadingString + Common.SPACER);
		}
	}
	
	// add an empty event to the track specified at the position specified.
	// The track could be either audio or video
	private TrackEvent AddEmptyEvent(Track track, Timecode position, string label) {
		Media media;
		TrackEvent @event;

		if (track.IsAudio()) {
			media = new Media(Common.vegas.InstallationDirectory + "\\Script Menu\\AddBeep.wav\\empty.wav");
			@event = (TrackEvent)((AudioTrack)track).AddAudioEvent(position, Timecode.FromMilliseconds(2000.0));
			(@event.AddTake(media.GetAudioStreamByIndex(0))).Name = label + Common.SPACER;
		} else if (track.IsVideo()) {
			media = new Media(Common.vegas.InstallationDirectory + "\\Script Menu\\AddRuler.png\\empty.png");
			@event = (TrackEvent)((VideoTrack)track).AddVideoEvent(position, Timecode.FromMilliseconds(2000.0));
			(@event.AddTake(media.GetVideoStreamByIndex(0))).Name = label + Common.SPACER;
		} else {
			throw new Exception("track type is neither audio nor video");
		}

		return @event;
	}
	
}

