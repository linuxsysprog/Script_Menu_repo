// File: Video.cs - Helper functions common to video tracks

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;

namespace AddRulerNamespace
{
public class Video {
	// returns all video tracks
	public static List<VideoTrack> FindVideoTracks(Project project) {
		List<VideoTrack> videoTracks = new List<VideoTrack>();
		
		foreach (Track track in project.Tracks) {
			if (track is VideoTrack) {
				videoTracks.Add((VideoTrack)track);
			}
		}
		
		return videoTracks;
	}
	
	// add a ruler 1 frame long onto the track specified at the position specified
	public static VideoEvent AddRuler(VideoTrack videoTrack, Timecode position,
		bool top, int number, string notes) {
		Media media = new Media(Common.vegas.InstallationDirectory + "\\Application Extensions\\AddRuler.png\\" +
			Common.LocationNumber2Basename(top, number));
		VideoEvent videoEvent = videoTrack.AddVideoEvent(position, Timecode.FromFrames(1));
		
		(videoEvent.AddTake(media.GetVideoStreamByIndex(0))).Name = number + " " + (top ? "T" : "B");
		(videoEvent.AddTake(media.GetVideoStreamByIndex(0))).Name = notes;
		
		return videoEvent;
	}
	
}
}

