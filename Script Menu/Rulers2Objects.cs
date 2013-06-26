// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Promote top/bottom rulers to Measure objects

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint {
	private Regex regex = new Regex("^1 (T|B)");

    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;
		VideoTrack sourceTrack;
		VideoTrack targetTrack;
		List<VideoEvent> sourceEvents;
		List<VideoEvent> targetEvents;
		
		Selection selection = new Selection(vegas.Transport.SelectionStart, vegas.Transport.SelectionLength);
		selection.Normalize();
		
		// check for the user has selected exactly two video tracks
		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(Common.vegas.Project)))
		);
		if (selectedVideoTracks.Count != 2) {
			MessageBox.Show("Please make sure you have exactly two video tracks selected",
				Common.RULERS_OBJECTS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// find events
		List<TrackEvent>[] events = new List<TrackEvent>[2];
		for (int i = 0; i < 2; i++) {
			if (selection.SelectionLength == new Timecode()) {
				events[i] = Common.TrackEventsToTrackEvents(selectedVideoTracks[i].Events);
			} else {
				events[i] = Common.FindEventsBySelection(selectedVideoTracks[i], selection);
			}
		}

		// to continue, one track (selection) should be empty and the other should not
		if ((events[0].Count == 0 && events[1].Count == 0) ||
				(events[0].Count != 0 && events[1].Count != 0)) {
			MessageBox.Show("Please make sure one track (selection) is empty and the other has at least one event",
				Common.RULERS_OBJECTS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// sort out which track and event collection is the source
		// and which is the target
		if (events[1].Count == 0) {
			sourceTrack = selectedVideoTracks[0];
			targetTrack = selectedVideoTracks[1];
			
			sourceEvents = Common.EventsToVideoEvents(events[0]);
			targetEvents = Common.EventsToVideoEvents(events[1]);
		} else {
			sourceTrack = selectedVideoTracks[1];
			targetTrack = selectedVideoTracks[0];
			
			sourceEvents = Common.EventsToVideoEvents(events[1]);
			targetEvents = Common.EventsToVideoEvents(events[0]);
		}
		
		// narrow down source events to a list of 1 (T|B)'s
		List<VideoEvent> filteredSourceEvents = new List<VideoEvent>();
		foreach (VideoEvent sourceEvent in sourceEvents) {
			string eventName = Common.getFullName(Common.getTakeNames(sourceEvent));
			if (regex.Match(eventName).Success) {
				filteredSourceEvents.Add(sourceEvent);
			}
		}
		
		if (filteredSourceEvents.Count < 1) {
			MessageBox.Show("No measure start events found",
				Common.RULERS_OBJECTS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// load config
		XmlNode projectPath = null;
		try {
			XmlDocument configXML = new XmlDocument();
			configXML.Load(Common.vegas.InstallationDirectory + "\\Script Menu\\AddObject.cs.config");
			
			projectPath = configXML.SelectSingleNode("/ScriptSettings/ProjectPath");
			if (null == projectPath) {
				throw new Exception("ProjectPath element not found");
			}
		} catch (Exception ex) {
			MessageBox.Show("Failed to load config file: " + ex.Message,
				Common.RULERS_OBJECTS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// insert measure objects
		int count = 0;
		foreach (VideoEvent sourceEvent in filteredSourceEvents) {
			// do not insert into the same slot more than once
			List<TrackEvent> existingEvents = Common.FindEventsByPosition(targetTrack, sourceEvent.Start);
			if (existingEvents.Count > 0) {
				continue;
			}
			
			// init text generator
			TextGenerator textGenerator = null;
			Bitmap frame = null;
			try {
				frame = new Bitmap(Common.vegas.Project.Video.Width, Common.vegas.Project.Video.Height,
					new Bitmap(Common.vegas.InstallationDirectory + "\\Script Menu\\AddRuler.png\\ascii_chart.8x12.png").PixelFormat);
				textGenerator = TextGenerator.FromTextGeneratorFactory(frame);;
			} catch (Exception ex) {
				MessageBox.Show("Failed to init text generator: " + ex.Message,
					Common.RULERS_OBJECTS, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			// add text
			textGenerator.AddMeasure("" + ++count);
			
			// save frame
			string mediaPath = null;
			try {
				mediaPath = projectPath.InnerText + "\\" + Common.getNextFilename(projectPath.InnerText);
				frame.Save(mediaPath);
			} catch (Exception ex) {
				MessageBox.Show("Failed to save frame: " + ex.Message,
					Common.RULERS_OBJECTS, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			// create event
			Video.AddObject(targetTrack, sourceEvent.Start, mediaPath);
		}
		
		// report
		MessageBox.Show("Inserted " + count + " events", Common.RULERS_OBJECTS);
	}
	
}

