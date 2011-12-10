// File: Common.cs - Helper functions common to tracks and events

using System;
using System.Collections;
using System.Collections.Generic;
using Sony.Vegas;

namespace AddRuler
{
public class Common {

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

}
}

