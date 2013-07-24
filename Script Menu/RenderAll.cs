// Copyright (C) 2013 Andrey Chislenko
// $Id$
// Finds and renders splubs into final mp4s or into tracks for master project.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;
		vegas.DebugClear();
		
		Regex regex = new Regex("^(Split|Take)");
		
		List<Track> tracks = Common.TracksToTracks(vegas.Project.Tracks);
		foreach (Track track in tracks) {
			if (!track.IsVideo()) {
				continue;
			}
		
			if (track.DisplayIndex > 33) {
				continue;
			}
		
			List<TrackEvent> trackEvents = Common.FindEventsByRegex(Common.TrackEventsToTrackEvents(track.Events), regex);
			if (trackEvents.Count < 1) {
				continue;
			}
			
			List<Cluster> clusters = new List<Cluster>();
			
			// Timecode clusterStart = trackEvents[0].Start;
			// Timecode clusterEnd = null;
			// for (int i = 1; i < trackEvents.Count; i++) {
				// TrackEvent prevTrackEvent = trackEvents[i - 1];
				// TrackEvent nextTrackEvent = trackEvents[i];
				// string prevTrackEventName = Common.getFullName(Common.getTakeNamesNonNative(prevTrackEvent));
				// string nextTrackEventName = Common.getFullName(Common.getTakeNamesNonNative(nextTrackEvent));
				
				// if (prevTrackEventName != nextTrackEventName || i == trackEvents.Count - 1) {
					// if (i == trackEvents.Count - 1) {
						// clusterEnd = nextTrackEvent.End;
					// } else {
						// clusterEnd = prevTrackEvent.End;
					// }
					
					// vegas.DebugOut("cluster ready (" + clusterStart + "," + clusterEnd + ")");
					// clusterStart = nextTrackEvent.Start;
				// }
			// }
			
			int bottomRulerTrackIndex = getTrackIndex(TrackType.BottomRuler, track.Index);
			
			Timecode clusterStart = trackEvents[0].Start;
			Timecode clusterEnd = null;
			for (int i = 1; i < trackEvents.Count; i++) {
				TrackEvent prevTrackEvent = trackEvents[i - 1];
				TrackEvent nextTrackEvent = trackEvents[i];
				string prevTrackEventName = Common.getFullName(Common.getTakeNamesNonNative(prevTrackEvent));
				string nextTrackEventName = Common.getFullName(Common.getTakeNamesNonNative(nextTrackEvent));
				
				if (prevTrackEventName != nextTrackEventName) {
					clusterEnd = prevTrackEvent.End;
					
					clusters.Add(new Cluster(bottomRulerTrackIndex - 2, bottomRulerTrackIndex - 1,
						bottomRulerTrackIndex, track.Index - 1,
						track.Index, track.Index + 1,
						getTrackIndex(TrackType.Beep, track.Index),
						clusterStart, clusterEnd,
						prevTrackEventName));
					
					clusterStart = nextTrackEvent.Start;
				}
			}
			clusters.Add(new Cluster(bottomRulerTrackIndex - 2, bottomRulerTrackIndex - 1,
				bottomRulerTrackIndex, track.Index - 1,
				track.Index, track.Index + 1,
				getTrackIndex(TrackType.Beep, track.Index),
				clusterStart, trackEvents[trackEvents.Count - 1].End,
				Common.getFullName(Common.getTakeNamesNonNative(trackEvents[trackEvents.Count - 1]))));
			
			foreach (Cluster cluster in clusters) {
				cluster.Solo();
				vegas.DebugOut("" + cluster + "\n");
			}
		}
	}
	
	private int getTrackIndex(TrackType trackType, int index) {
		Regex regex = new Regex("BPM$");
	
		List<Track> tracks = Common.TracksToTracks(Common.vegas.Project.Tracks);
		if (index > tracks.Count - 1) {
			throw new ArgumentException("index out of range");
		}
		
		if (trackType == TrackType.Beep) {
			for (int i = index; i < tracks.Count; i++) {
				List<TrackEvent> trackEvents = Common.TrackEventsToTrackEvents(tracks[i].Events);
				if (!tracks[i].IsAudio() || trackEvents.Count < 1) {
					continue;
				}
			
				string currentTrackEventName = Common.getFullName(Common.getTakeNames(trackEvents[0]));
				if (regex.Match(currentTrackEventName).Success) {
					return tracks[i].Index;
				}
			}
		} else {
			for (int i = index; i >= 0 ; i--) {
				List<TrackEvent> trackEvents = Common.TrackEventsToTrackEvents(tracks[i].Events);
				if (!tracks[i].IsVideo() || trackEvents.Count < 1) {
					continue;
				}
			
				string currentTrackEventName = Common.getFullName(Common.getTakeNames(trackEvents[0]));
				if (regex.Match(currentTrackEventName).Success) {
					return tracks[i].Index;
				}
			}
		}
		
		throw new Exception("beep track not found");
	}
	
	private enum TrackType {
		Beep,
		BottomRuler
	}
	
}

public class Cluster {
	private int textTrackIndex;
	private int measureTrackIndex;
	private int topRulerTrackIndex;
	private int bottomRulerTrackIndex;
	private int videoTrackIndex;
	private int audioTrackIndex;
	private int beepTrackIndex;
	
	private QuantizedEvent start;
	private QuantizedEvent end;
	
	private string name;
	
	public Cluster(int textTrackIndex,
		int measureTrackIndex,
		int topRulerTrackIndex,
		int bottomRulerTrackIndex,
		int videoTrackIndex,
		int audioTrackIndex,
		int beepTrackIndex,
		Timecode start,
		Timecode end,
		string name) {
			int count = Common.TracksToTracks(Common.vegas.Project.Tracks).Count;
		
			if (textTrackIndex >= count ||
				measureTrackIndex >= count ||
				topRulerTrackIndex >= count ||
				bottomRulerTrackIndex >= count ||
				videoTrackIndex >= count ||
				audioTrackIndex >= count ||
				beepTrackIndex >= count) {
				throw new ArgumentException("index out of range");
			}
		
			this.textTrackIndex = textTrackIndex;
			this.measureTrackIndex = measureTrackIndex;
			this.topRulerTrackIndex = topRulerTrackIndex;
			this.bottomRulerTrackIndex = bottomRulerTrackIndex;
			this.videoTrackIndex = videoTrackIndex;
			this.audioTrackIndex = audioTrackIndex;
			this.beepTrackIndex = beepTrackIndex;
			
			this.start = new QuantizedEvent(start);
			this.end = new QuantizedEvent(end);
			
			this.name = name;
	}
	
	public Timecode Start {
		get {
			return start.QuantizedStart;
		}
	}
	
	public Timecode End {
		get {
			return end.QuantizedStart;
		}
	}
	
	public Timecode Length {
		get {
			return end.QuantizedStart - start.QuantizedStart;
		}
	}
	
	public string Name {
		get {
			return name;
		}
	}
	
	public void Solo() {
		List<Track> projectTracks = Common.TracksToTracks(Common.vegas.Project.Tracks);
	
		Common.SoloAllTracks(projectTracks, false);
		Common.MuteAllTracks(projectTracks, true);
		
		List<Track> tracks = new List<Track>();

		tracks.Add(projectTracks[textTrackIndex]);
		tracks.Add(projectTracks[measureTrackIndex]);
		tracks.Add(projectTracks[topRulerTrackIndex]);
		tracks.Add(projectTracks[bottomRulerTrackIndex]);
		tracks.Add(projectTracks[videoTrackIndex]);
		tracks.Add(projectTracks[audioTrackIndex]);
		tracks.Add(projectTracks[beepTrackIndex]);
		
		Common.MuteAllTracks(tracks, false);
	}
	
	public void Render() {
		return;
	}
	
	public override string ToString() {
		return "{textTrackIndex=" + textTrackIndex + ", measureTrackIndex=" + measureTrackIndex + "}\n" +
			"{topRulerTrackIndex=" + topRulerTrackIndex + ", bottomRulerTrackIndex=" + bottomRulerTrackIndex + "}\n" +
			"{videoTrackIndex=" + videoTrackIndex + ", audioTrackIndex=" + audioTrackIndex + "}\n" +
			"{beepTrackIndex=" + beepTrackIndex + "}\n" +
			"{start=" + start + ", end=" + end + ", length=" + Length + "}\n" +
			"{name=" + name + "}\n";
	}
	
}

