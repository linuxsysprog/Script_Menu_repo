// File: MyScript.cs

using System;
using System.Windows.Forms;
using Sony.Vegas;
using UtilityMethods;
using System.Collections.Generic;
using AddRuler;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		List<VideoTrack> videoTracks = Video.FindVideoTracks(vegas.Project);
		// List<Track> tracks = (List<Track>)audioTracks;
		List<Track> tracks = Common.VideoTracksToTracks(videoTracks);
		
		List<VideoTrack> videoTracks2 = Common.TracksToVideoTracks(tracks);
		MessageBox.Show("" + videoTracks2[0].CompositeLevel);
	
		// List<AudioTrack> audioTracks = Audio.FindAudioTracks(vegas.Project);
		// List<Track> tracks = audioTracks.ConvertAll(new Converter<AudioTrack, Track>(AudioTrackToTrack));
		// List<Track> selectedTracks = Common.FindSelectedTracks(tracks);
		// MessageBox.Show("" + selectedTracks.Count);
    }
	
	public static Track AudioTrackToTrack(AudioTrack audioTrack) {
		return (Track)audioTrack;
	}

}
