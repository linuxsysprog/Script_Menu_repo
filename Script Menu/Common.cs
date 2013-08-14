// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Helper functions common to tracks and events

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using Sony.Vegas;

namespace AddRulerNamespace
{
public class Common {
	public const string CP_RIGHT = " - Copyright (C) 2011-2013 Andrey Chislenko";
	
	public const string ADD_RULER = "Add Ruler" + CP_RIGHT;
	public const string DUMP_TRACK = "Dump Track" + CP_RIGHT;
	public const string ADD_BEEP = "Add Beep" + CP_RIGHT;
	public const string ADD_OBJECT = "Add Object" + CP_RIGHT;
	public const string CALC_TEMPO = "Calculate Tempo" + CP_RIGHT;
	public const string NAV = "Navigate" + CP_RIGHT;
	public const string GEN_BEEPS = "Generate Beeps" + CP_RIGHT;
	public const string TRACK_OUT = "Track Outline" + CP_RIGHT;
	public const string BEEPS_RULERS = "Beeps to Rulers" + CP_RIGHT;
	public const string RULERS_RULERS = "Rulers to Rulers" + CP_RIGHT;
	public const string FILL_RULERS = "Fill Rulers" + CP_RIGHT;
	public const string RULERS_OBJECTS = "Rulers to Objects" + CP_RIGHT;
	public const string SLICE_TRACK = "Slice Track" + CP_RIGHT;
	public const string MATCH_TRACK = "Match Track" + CP_RIGHT;
	public const string SPREAD_BEEPS = "Spread Beeps" + CP_RIGHT;
	public const string LAYOUT_TRACK = "Layout Track" + CP_RIGHT;
	public const string RENDER_ALL = "Render All" + CP_RIGHT;
	public const string UNFX_AUDIO = "UnFX Audio" + CP_RIGHT;
	
	public const string SPACER = "     " + "     " + "     " + "     " + "     " + "     "  + "XXXXX";
	public const string AUDIO_RE = "^\\d+\\.\\d+";
	public const string VIDEO_RE = "^\\d+ (T|B)";

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
	
	// filters out tracks given a regex
	public static List<Track> FindTracksByRegex(List<Track> sourceTracks, Regex regex) {
		List<Track> tracks = new List<Track>();
		
		foreach (Track sourceTrack in sourceTracks) {
			if (regex.Match(sourceTrack.Name == null ? "" : sourceTrack.Name).Success) {
				tracks.Add(sourceTrack);
			}
		}
		
		return tracks;
	}
	
	public static List<Track> FindUnmutedTracks(List<Track> tracks) {
		List<Track> unmutedTracks = new List<Track>();
		
		foreach (Track track in tracks) {
			if (!track.Mute) {
				unmutedTracks.Add(track);
			}
		}
		
		return unmutedTracks;
	}
	
	// finds events that have at least one take whose name matches regex
	public static List<TrackEvent> FindEventsByRegex(List<TrackEvent> sourceEvents, Regex regex) {
		List<TrackEvent> events = new List<TrackEvent>();
		
		foreach (TrackEvent sourceEvent in sourceEvents) {
			string sourceEventName = getFullName(getTakeNamesNonNative(sourceEvent));
			
			if (regex.Match(sourceEventName).Success) {
				events.Add(sourceEvent);
			}
		}
		
		return events;
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
	
	// the 5th horseman
	// converts Tracks to List<Track>
	public static List<Track> TracksToTracks(Tracks sourceTracks) {
		List<Track> tracks = new List<Track>();
		
		foreach (Track sourceTrack in sourceTracks) {
			tracks.Add(sourceTrack);
		}
		
		return tracks;
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
	
	public static string TrackEventsToString(List<TrackEvent> trackEvents) {
		if (trackEvents.Count < 1) {
			return "No events found.";
		}
		
		string str = trackEvents.Count + " " + (trackEvents.Count > 1 ? "events" : "event") + " found.\r\n";

		string spacer = "    " + "    ";
		foreach (TrackEvent trackEvent in trackEvents) {
			str += "Event " + trackEvent.Index + spacer + trackEvent.Start + " " +
				trackEvent.End + " " + trackEvent.Length + " \r\n";
		
			foreach (Take take in trackEvent.Takes) {
				if (take.Media.Generator == null) {
					string takeName = "n/a";
					if (take.Name.IndexOf(Common.SPACER) != -1) {
						takeName = take.Name.Substring(0, take.Name.IndexOf(Common.SPACER));
					}
				
					str += "    Take " + take.Index + spacer + takeName + spacer +
						Common.Basename(take.Media.FilePath) + (take.IsActive == true ? " *" : "") + "\r\n";
				} else {
					str += "    Take " + take.Index + spacer + take.Name +
						(take.IsActive == true ? " *" : "") + "\r\n";
				}
			}
		}
		
		str += trackEvents.Count + " " + (trackEvents.Count > 1 ? "events" : "event") + " found.";
		
		return str;
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
	
	public static TrackEvent FindEventRight(Track track, Timecode position, Regex regex) {
		List<TrackEvent> events = TrackEventsToTrackEvents(track.Events);
		
		foreach (TrackEvent @event in events) {
			if (@event.Start >= position) {
				if (null == regex) {
					return @event;
				}
				
				string eventFullName = getFullName(getTakeNames(@event));
				if (regex.Match(eventFullName).Success) {
					return @event;
				}
			}
		}
			
		return null;
	}
	
	public static TrackEvent FindEventLeft(Track track, Timecode position, Regex regex) {
		List<TrackEvent> events = TrackEventsToTrackEvents(track.Events);
		
		for (int i = events.Count - 1; i >= 0; i--) {
			if (events[i].Start < position) {
				if (null == regex) {
					return events[i];
				}
				
				string eventFullName = getFullName(getTakeNames(events[i]));
				if (regex.Match(eventFullName).Success) {
					return events[i];
				}
			}
		}
			
		return null;
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
	
	// finds events by another event
	public static List<TrackEvent> FindEventsByEvent(Track track, TrackEvent sourceEvent) {
		List<TrackEvent> events = new List<TrackEvent>();
		
		foreach (TrackEvent @event in track.Events) {
			if (sourceEvent.Start > @event.Start &&
					sourceEvent.Start < @event.End) {
				events.Add(@event);
			}
		}
		
		return events;
	}
	
	// finds events by a quantized event
	public static List<TrackEvent> FindEventsByQuantizedEvent(Track track, QuantizedEvent quantizedEvent) {
		List<TrackEvent> events = new List<TrackEvent>();
		
		foreach (TrackEvent @event in track.Events) {
			if (quantizedEvent.QuantizedStart > @event.Start &&
					quantizedEvent.QuantizedStart < @event.End) {
				events.Add(@event);
			}
		}
		
		return events;
	}
	
	// finds measure start events
	public static List<TrackEvent> FindMeasureStartEvents(List<TrackEvent> sourceEvents) {
		List<TrackEvent> events = new List<TrackEvent>();
		
		foreach (TrackEvent @event in sourceEvents) {
			bool eventOK = false;
			
			// for event to qualify it should have at least one take that matches our pattern
			foreach (Take take in @event.Takes) {
				if (take.MediaStream == null) {
					continue;
				}
				
				if (take.MediaStream.MediaType != MediaType.Audio &&
						take.MediaStream.MediaType != MediaType.Video) {
					continue;
				}
				
				if (getMeasureStartRegex(take).Matches(take.Name).Count > 0) {
					eventOK = true;
					break;
				}
			}
			
			if (eventOK) {
				events.Add(@event);
			}
		}
		
		return events;
	}
	
	// finds events that have at least one take whose name matches "^\\d+\\.\\d+" for audio
	// and "^\\d+ (T|B)" for video
	public static List<TrackEvent> FindNativeEvents(List<TrackEvent> sourceEvents) {
		List<TrackEvent> events = new List<TrackEvent>();
		Regex audioRegex = new Regex(AUDIO_RE);
		Regex videoRegex = new Regex(VIDEO_RE);
		
		foreach (TrackEvent sourceEvent in sourceEvents) {
			string sourceEventName = getFullName(getTakeNames(sourceEvent));
			
			Regex regex;
			if (sourceEvent.IsAudio()) {
				regex = audioRegex;
			} else if (sourceEvent.IsVideo()) {
				regex = videoRegex;
			} else {
				throw new Exception("source event neither audio nor video");
			}
			
			if (regex.Match(sourceEventName).Success) {
				events.Add(sourceEvent);
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
	
	// return a list of strings in which the first string is the event's
	// main take. Ignore take names which
	// are not native to our application (the ones that do not have SPACER
	// at the end).
	public static List<string> getTakeNames(TrackEvent @event) {
		List<string> strings = new List<string>();
		string leadingString = null;
		
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
		
		if (leadingString != null) {
			strings.Insert(0, leadingString);
		}
		return strings;
	}
	
	// same as previous, but not restricted to native events
	public static List<string> getTakeNamesNonNative(TrackEvent @event) {
		List<string> strings = new List<string>();
		
		foreach (Take take in @event.Takes) {
			strings.Add(take.Name);
		}
		
		return strings;
	}
	
	// returns a regex appropriate for event's media
	private static Regex getRegex(Take take) {
		if (take.MediaStream.MediaType == MediaType.Audio) {
			return new Regex(AUDIO_RE);
		} else {
			return new Regex(VIDEO_RE);
		}
	}
	
	// returns a regex to match a measure start event.
	public static Regex getMeasureStartRegex(Take take) {
		if (take.MediaStream.MediaType == MediaType.Audio) {
			return new Regex("^\\d+\\.1");
		} else {
			return new Regex("^1 (T|B)");
		}
	}
	
	// returns a full name as a concatinated string of individual take names
	public static string getFullName(List<string> takeNames) {
		string fullName = "";
		
		for (int i = 0; i < takeNames.Count; i++) {
			fullName += takeNames[i];
			
			if (i < takeNames.Count - 1) {
				fullName += " ";
			}
		}
		
		return fullName;
	}
	
	// filters string array by a regex
	public static string[] filterByRegex(string[] ar, Regex regex) {
		List<string> filtered = new List<string>();
		
		foreach (string s in ar) {
			if (regex.Match(s).Success) {
				filtered.Add(s);
			}
		}
		
		return filtered.ToArray();
	}
	
	// returns next filename in project path
	public static string getNextFilename(string projectPath) {
		Regex filenameRegex = new Regex("^Object(\\d{4}).png$", RegexOptions.IgnoreCase);
	
		// list full paths
		string[] paths = null;
		try {
			paths = Directory.GetFiles(projectPath, "Object????.png");
		} catch (Exception ex) {
			throw new Exception("Failed to get files in " + projectPath + ": " + ex.Message);
		}
		
		// trim to basenames
		List<string> files = new List<string>();
		foreach (string path in paths) {
			files.Add(Basename(path));
		}
		
		// filter by regex
		string[] filesFiltered = filterByRegex(files.ToArray(), filenameRegex);
		Array.Sort(filesFiltered);
		
		// get current filename
		string currentFilename = filesFiltered.Length > 0 ? filesFiltered[filesFiltered.Length - 1] : "Object0000.png";
		
		int n = Convert.ToInt32(filenameRegex.Match(currentFilename).Groups[1].Value);
		if (n > 9998) {
			throw new Exception(projectPath + " is full");
		}
		
		return "Object" + (++n).ToString("D4") + ".png";
	}
	
	public static void MuteAllTracks(List<Track> tracks, bool mute) {
		using (UndoBlock undo = new UndoBlock("MuteAllTracks")) {
			foreach (Track track in tracks) {
				if (mute && !track.Mute) {
					track.Mute = true;
				} else if (!mute && track.Mute) {
					track.Mute = false;
				}
			}
		}
	}
	
	public static void SoloAllTracks(List<Track> tracks, bool solo) {
		using (UndoBlock undo = new UndoBlock("SoloAllTracks")) {
			foreach (Track track in tracks) {
				if (solo && !track.Solo) {
					track.Solo = true;
				} else if (!solo && track.Solo) {
					track.Solo = false;
				}
			}
		}
	}
	
	public static int getTrackIndex(TrackType trackType, int index) {
		List<Track> tracks = TracksToTracks(vegas.Project.Tracks);
		if (index > tracks.Count - 1) {
			throw new ArgumentException("index out of range");
		}
		
		if (trackType == TrackType.Beep) {
			for (int i = index; i < tracks.Count; i++) {
				List<TrackEvent> trackEvents = TrackEventsToTrackEvents(tracks[i].Events);
				if (!tracks[i].IsAudio() || trackEvents.Count < 1) {
					continue;
				}
				
				if (FindMeasureStartEvents(trackEvents).Count > 0) {
					return tracks[i].Index;
				}
			}
		} else {
			for (int i = index; i >= 0 ; i--) {
				List<TrackEvent> trackEvents = TrackEventsToTrackEvents(tracks[i].Events);
				if (!tracks[i].IsVideo() || trackEvents.Count < 1) {
					continue;
				}
			
				if (FindMeasureStartEvents(trackEvents).Count > 0) {
					return tracks[i].Index;
				}
			}
		}
		
		throw new Exception("track not found");
	}
	
	public static string[] getRange(int min, int max) {
		List<string> list = new List<string>();
		for (int i = min; i < max + 1; i++) {
			list.Add("" + i);
		}
		return list.ToArray();
	}
	
	public static void ToggleCheckBox(CheckBox cb) {
		if (cb.Checked) {
			cb.Checked = false;
		} else {
			cb.Checked = true;
		}
	}
	
}

public enum TrackType {
	Beep,
	BottomRuler
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

public class QuantizedEvent {
	private Timecode start;
	private Timecode quantizedStart;
	private string status;
	private Timecode offset;
	
	public QuantizedEvent(Timecode start) {
		this.start = start;
		Quantize();
	}
	
	public Timecode Start {
		get {
			return start;
		}
		set {
			start = value;
			Quantize();
		}
	}
	
	public Timecode QuantizedStart {
		get {
			return quantizedStart;
		}
	}
	
	public string Status {
		get {
			return status;
		}
	}
	
	public Timecode Offset {
		get {
			return offset;
		}
	}
	
	private void Quantize() {
		double frames = Convert.ToDouble(start.ToString(RulerFormat.AbsoluteFrames));
		quantizedStart = Timecode.FromFrames((int)Math.Round(frames));
		
		// a frame could be fast, slow or perfect
		if (quantizedStart == start) {
			offset = new Timecode();
			status = "P";
		} else if (quantizedStart > start) {
			offset = quantizedStart - start;
			status = "S";
		} else { // quantizedStart < start
			offset = start - quantizedStart;
			status = "F";
		}
	}
	
	public override string ToString() {
		return start + " " + quantizedStart + " " + offset + " " + status;
	}
	
	public static QuantizedEvent FromTimecode(Timecode start) {
		return new QuantizedEvent(start);
	}
	
}

public class Preset : IComparable {
	private string frameSize;
	private string @object;
	private string value;
	private Regex regex = new Regex("^\\d+");

	public Preset(string strPreset) {
		string[] result = strPreset.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		if (regex.Matches(strPreset).Count < 1 ||
				result.Length != 3) {
			throw new Exception("can't parse Preset string: " + strPreset);
		}
		
		this.frameSize = result[0];
		this.@object = result[1];
		this.value = result[2];
	}
	
	public string FrameSize {
		get {
			return frameSize;
		}
		set {
			frameSize = value;
		}
	}
	
	public string Object {
		get {
			return @object;
		}
		set {
			@object = value;
		}
	}
	
	public string Value {
		get {
			return value;
		}
		set {
			this.value = value;
		}
	}
	
	public override string ToString() {
		return frameSize + " " + @object + " " + value;
	}
	
    public int CompareTo(object obj) {
        if (obj == null) return 1;

        Preset otherPreset = obj as Preset;
        if (otherPreset != null) 
            return ToString().CompareTo(otherPreset.ToString());
        else
           throw new ArgumentException("Object is not a Preset");
    }

}

// Courtesy of http://stackoverflow.com/questions/97459/automatically-select-all-text-on-focus-in-winforms-textbox
public class MyTextBox : System.Windows.Forms.TextBox
{
    private bool _focused;

    protected override void OnEnter(EventArgs e)
    {
        base.OnEnter(e);
        if (MouseButtons == MouseButtons.None)
        {
            SelectAll();
            _focused = true;
        }
    }

    protected override void OnLeave(EventArgs e)
    {
        base.OnLeave(e);
        _focused = false;
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        if (!_focused)
        {
            if (SelectionLength == 0)
                SelectAll();
            _focused = true;
        }
    }
}

}

