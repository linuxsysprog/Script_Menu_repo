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
		
		Cluster cluster = new Cluster(0, 1,
			2, 3,
			4, 5,
			6,
			new Timecode(0.0), new Timecode(17));
		
		vegas.DebugOut("" + cluster);
		
		cluster.Solo();
	}
	
}

public class Cluster {
	private QuantizedEvent start;
	private QuantizedEvent end;
	
	private int textTrackIndex;
	private int measureTrackIndex;
	private int topRulerTrackIndex;
	private int bottomRulerTrackIndex;
	private int videoTrackIndex;
	private int audioTrackIndex;
	private int beepTrackIndex;
	
	public Cluster(int textTrackIndex,
		int measureTrackIndex,
		int topRulerTrackIndex,
		int bottomRulerTrackIndex,
		int videoTrackIndex,
		int audioTrackIndex,
		int beepTrackIndex,
		Timecode start,
		Timecode end) {
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
	
	public override string ToString() {
		return "{textTrackIndex=" + textTrackIndex + ", measureTrackIndex=" + measureTrackIndex + "}\n" +
			"{topRulerTrackIndex=" + topRulerTrackIndex + ", bottomRulerTrackIndex=" + bottomRulerTrackIndex + "}\n" +
			"{videoTrackIndex=" + videoTrackIndex + ", audioTrackIndex=" + audioTrackIndex + "}\n" +
			"{beepTrackIndex=" + beepTrackIndex + "}\n" +
			"{start=" + start + ", end=" + end + ", length=" + Length + "}\n";
	}
	
}

