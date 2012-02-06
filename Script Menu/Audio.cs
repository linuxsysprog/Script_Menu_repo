// Copyright (C) 2011 Andrey Chislenko
// File: Audio.cs - Helper functions common to audio tracks

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;

namespace AddRulerNamespace
{
public class Audio {
	// returns all audio tracks
	public static List<AudioTrack> FindAudioTracks(Project project) {
		List<AudioTrack> audioTracks = new List<AudioTrack>();
		
		foreach (Track track in project.Tracks) {
			if (track is AudioTrack) {
				audioTracks.Add((AudioTrack)track);
			}
		}
		
		return audioTracks;
	}
	
	// add a beep 200ms long onto the track specified at the position specified
	public static AudioEvent AddBeep(AudioTrack audioTrack, Timecode position,
		int measure, int beat, string notes) {
		// Common.vegas.DebugOut("is audioTrack null = " + (audioTrack == null) + "\n" +
				// "is position null = " + (position == null) + "\n" +
				// "measure = " + measure + "\n" +
				// "beat = " + beat + "\n" +
				// "notes = " + notes);
		
		string path = Common.vegas.InstallationDirectory + "\\Script Menu\\AddBeep.wav";

		Media media = new Media(path + "\\high.wav");
		
		AudioEvent audioEvent = audioTrack.AddAudioEvent(position, Timecode.FromMilliseconds(200.0));
		
		(audioEvent.AddTake(media.GetAudioStreamByIndex(0))).Name = notes + Common.SPACER;
		media = new Media(path + "\\beep.1.wav");
		(audioEvent.AddTake(media.GetAudioStreamByIndex(0))).Name = measure +
			"." + beat + Common.SPACER;
		
		return audioEvent;
	}
	
}
}

