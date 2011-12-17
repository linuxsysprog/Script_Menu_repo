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
	
}
}

