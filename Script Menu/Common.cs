// File: Common.cs - Helper functions common to tracks and events

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sony.Vegas;

namespace AddRulerNamespace
{
public class Common {
	public const string ADD_RULER = "Add Ruler";
	public const string ADD_BEEP = "Add Beep";
	public const string ADD_OBJECT = "Add Object";
	public const string CALC_TEMPO = "Calculate Tempo";
	public const string GEN_BEEPS = "Generate Beeps";
	public const string TRACK_OUT = "Track Outline";
	public const string BEEPS_RULERS = "Beeps to Rulers";
	public const string SPACER = "     " + "     " + "     " + "     " + "     " + "     "  + "XXXXX";
	public const string AUDIO_RE = "^\\d+\\.1";
	public const string VIDEO_RE = "^1 (T|B)";

	public static Vegas vegas;

	//
	// 2.1: How do I find the currently selected track or event?
	//
	// Note that multiple tracks and events selected at the same time. 
	// Events can be selected in tracks that are not selected.
	//
	
	// returns only the first selected track
	public static Track FindSelectedTrack(Project project) {
		foreach (Track track in project.Tracks) {
			if (track.Selected) {
				return track;
			}
		}
		return null;
	}

	// returns all of the selected events
	public static TrackEvent[] FindSelectedEvents(Project project) {
		List<TrackEvent> selectedEvents = new List<TrackEvent>();
		foreach (Track track in project.Tracks) {
			foreach (TrackEvent trackEvent in track.Events) {
				if (trackEvent.Selected) {
					selectedEvents.Add(trackEvent);
				}
			}
		}
		return selectedEvents.ToArray();
	}
	
	//
	// 2.4: How do I add media to the timeline?
	//
	// 
	//
	
	// insert a media file at a specific position on a specific track
	public static void InsertFileAt(Vegas vegas, String fileName, int trackIndex, Timecode cursorPosition) {
		// first clear all track selections
		foreach (Track track in vegas.Project.Tracks) {
			track.Selected = false;
		}
		// select the desired track
		vegas.Project.Tracks[trackIndex].Selected = true;
		// set the cursor position
		vegas.Transport.CursorPosition = cursorPosition;
		vegas.OpenFile(fileName);
	}

	// build events on the timeline by constructing media, tracks, events, and take objects individually
	public static VideoEvent AddVideoEvent(Vegas vegas, String mediaFile, Timecode start, Timecode length)
	{
		Media media = new Media(mediaFile);
		VideoTrack track = vegas.Project.AddVideoTrack();
		VideoEvent videoEvent = track.AddVideoEvent(start, length);
		Take take = videoEvent.AddTake(media.GetVideoStreamByIndex(0));
		return videoEvent;
	}
	
	//
	// 2.5: How do I add a text event to the timeline?
	//
	// 
	//
	
	// add a text event to the timeline
	public static VideoEvent AddTextEvent(Vegas vegas, VideoTrack track, Timecode start, Timecode length)
	{
		// find the text generator plug-in
		PlugInNode plugIn = vegas.Generators.GetChildByName("Sony Text");
		// create a media object with the generator plug-in
		Media media = new Media(plugIn);
		// set the generator preset
		media.Generator.Preset = "Banner";
		// add the video event
		VideoEvent videoEvent = track.AddVideoEvent(start, length);
		// add the take using the generated video stream
		Take take = videoEvent.AddTake(media.GetVideoStreamByIndex(0));
		return videoEvent;
	}

	// AddTextEvent II
	public static VideoEvent AddTextEvent(Vegas vegas, PlugInNode plugIn, VideoTrack videoTrack,
		string preset, Timecode start, Timecode length)
	{
		Media media = new Media(plugIn);
		media.Generator.Preset = preset;
		VideoEvent videoEvent = videoTrack.AddVideoEvent(start, length);
		(videoEvent.AddTake(media.GetVideoStreamByIndex(0))).Name = preset;
		return videoEvent;
	}

	// returns all selected tracks
	public static List<Track> FindSelectedTracks(List<Track> tracks) {
		List<Track> selectedTracks = new List<Track>();
		
		foreach (Track track in tracks) {
			if (track.Selected) {
				selectedTracks.Add(track);
			}
		}
		
		return selectedTracks;
	}
		
	// returns all selected tracks II
	public static List<Track> FindSelectedTracks(Tracks projectTracks) {
		List<Track> tracks = new List<Track>();
		
		foreach (Track track in projectTracks) {
			if (track.Selected) {
				tracks.Add(track);
			}
		}
		
		return tracks;
	}
	
	//
	// The "Four Horsemen" functions for tracks
	//
	// Functions to convert a list of Video/Audio tracks to and from a list of Track
	//
	
	// converts List<AudioTrack> to List<Track>
	public static List<Track> AudioTracksToTracks(List<AudioTrack> audioTracks) {
		return audioTracks.ConvertAll(new Converter<AudioTrack, Track>(AudioTrackToTrack));
	}
	
	private static Track AudioTrackToTrack(AudioTrack audioTrack) {
		return audioTrack;
	}
	
	// converts List<Track> to List<AudioTrack>
	public static List<AudioTrack> TracksToAudioTracks(List<Track> tracks) {
		return tracks.ConvertAll(new Converter<Track, AudioTrack>(TrackToAudioTrack));
	}
	
	private static AudioTrack TrackToAudioTrack(Track track) {
		return (AudioTrack)track;
	}
	
	// converts List<VideoTrack> to List<Track>
	public static List<Track> VideoTracksToTracks(List<VideoTrack> videoTracks) {
		return videoTracks.ConvertAll(new Converter<VideoTrack, Track>(VideoTrackToTrack));
	}
	
	private static Track VideoTrackToTrack(VideoTrack videoTrack) {
		return videoTrack;
	}
	
	// converts List<Track> to List<VideoTrack>
	public static List<VideoTrack> TracksToVideoTracks(List<Track> tracks) {
		return tracks.ConvertAll(new Converter<Track, VideoTrack>(TrackToVideoTrack));
	}
	
	private static VideoTrack TrackToVideoTrack(Track track) {
		return (VideoTrack)track;
	}
	
	//
	// The "Four Horsemen" functions for events
	//
	// Functions to convert a list of Video/Audio events to and from a list of TrackEvent
	//
	
	// converts List<AudioEvent> to List<TrackEvent>
	public static List<TrackEvent> AudioEventsToEvents(List<AudioEvent> audioEvents) {
		return audioEvents.ConvertAll(new Converter<AudioEvent, TrackEvent>(AudioEventToEvent));
	}
	
	private static TrackEvent AudioEventToEvent(AudioEvent audioEvent) {
		return audioEvent;
	}
	
	// converts List<TrackEvent> to List<AudioEvent>
	public static List<AudioEvent> EventsToAudioEvents(List<TrackEvent> events) {
		return events.ConvertAll(new Converter<TrackEvent, AudioEvent>(EventToAudioEvent));
	}
	
	private static AudioEvent EventToAudioEvent(TrackEvent @event) {
		return (AudioEvent)@event;
	}
	
	// converts List<VideoEvent> to List<TrackEvent>
	public static List<TrackEvent> VideoEventsToEvents(List<VideoEvent> videoEvents) {
		return videoEvents.ConvertAll(new Converter<VideoEvent, TrackEvent>(VideoEventToEvent));
	}
	
	private static TrackEvent VideoEventToEvent(VideoEvent videoEvent) {
		return videoEvent;
	}
	
	// converts List<TrackEvent> to List<VideoEvent>
	public static List<VideoEvent> EventsToVideoEvents(List<TrackEvent> events) {
		return events.ConvertAll(new Converter<TrackEvent, VideoEvent>(EventToVideoEvent));
	}
	
	private static VideoEvent EventToVideoEvent(TrackEvent @event) {
		return (VideoEvent)@event;
	}
	
	// the 5th horseman
	// converts TrackEvents to List<TrackEvent>
	public static List<TrackEvent> TrackEventsToTrackEvents(TrackEvents trackEvents) {
		List<TrackEvent> events = new List<TrackEvent>();
		
		foreach (TrackEvent @event in trackEvents) {
			events.Add(@event);
		}
		
		return events;
	}
	
	// finds events by track and position
	public static List<TrackEvent> FindEventsByPosition(Track track, Timecode position) {
		List<TrackEvent> events = new List<TrackEvent>();
		
		foreach (TrackEvent @event in track.Events) {
			if (@event.Start == position) {
				events.Add(@event);
			}
		}
		
		return events;
	}
	
	// finds events by track and selection
	public static List<TrackEvent> FindEventsBySelection(Track track, Selection selection) {
		List<TrackEvent> events = new List<TrackEvent>();
		
		foreach (TrackEvent @event in track.Events) {
			if (@event.Start >= selection.SelectionStart
					&& @event.Start < selection.SelectionEnd) {
				events.Add(@event);
			}
		}
		
		return events;
	}
	
	// convert Location&Number to basename
	public static string LocationNumber2Basename(bool top, int number) {
		return "ruler_" + (top ? "top" : "bot") + "_" +  number.ToString("D2") + ".png";
	}

	// get basename of a fully qualified file name
	public static string Basename(string path) {
	
		return path.Substring(path.LastIndexOf("\\") + 1);
	}
	
	// return a regex appropriate for the take's media
	public static Regex getRegex(Take take) {
		if (take.MediaStream.MediaType == MediaType.Audio) {
			return new Regex(AUDIO_RE);
		} else {
			return new Regex(VIDEO_RE);
		}
	}
	
	// return a list of strings in which the first string is the event's
	// main take (^N.N for audio/^N X for video). Ignore take names which
	// are not native to our application (the ones that do not have SPACER
	// at the end).
	public static List<string> getTakeNames(TrackEvent @event) {
		List<string> strings = new List<string>();
		string leadingString = "";
		
		foreach (Take take in @event.Takes) {
			// drop take names which do not have the spacer in them
			if (take.Name.IndexOf(SPACER) == -1) {
				continue;
			}
			
			if (getRegex(take).Matches(take.Name).Count > 0) {
				leadingString = take.Name.Substring(0, take.Name.IndexOf(SPACER));
			} else {
				strings.Add(take.Name.Substring(0, take.Name.IndexOf(SPACER)));
			}
		}
		
		strings.Insert(0, leadingString);
		return strings;
	}
		
}

public class Selection {
	private Timecode selectionStart;
	private Timecode selectionLength;

	private Timecode selectionEnd;

	public Selection(Timecode selectionStart, Timecode selectionLength) {
		this.selectionStart = this.selectionEnd = selectionStart;
		this.selectionLength = selectionLength;
	}
	
	public Timecode SelectionStart {
		get {
			return selectionStart;
		}
		set {
			selectionStart = value;
		}
	}
	
	public Timecode SelectionLength {
		get {
			return selectionLength;
		}
		set {
			selectionLength = value;
		}
	}
	
	public Timecode SelectionEnd {
		get {
			return selectionEnd;
		}
		set {
			selectionEnd = value;
		}
	}
	
	public Selection Normalize() {
		if (selectionLength > new Timecode()) {
			selectionEnd = selectionStart + selectionLength;
		} else {
			selectionStart = selectionStart + selectionLength;
			selectionLength = new Timecode() - selectionLength;
		}
	
		return this;
	}
	
	public override string ToString() {
		return selectionStart + " " + selectionLength + " " + selectionEnd;
	}
	
}

}

