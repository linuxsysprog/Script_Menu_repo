// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Spread out (or bring closer together) the beeps by a certain percentage.
// This script operates on a single audio track

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint : Form {
	private Label lblLabel = new Label();
	private TextBox txtTextBox = new TextBox();
	private Label lblLabel2 = new Label();
	private Button btnOK = new Button();
	private Button btnCancel = new Button();
	
	public EntryPoint() {
		lblLabel.Size = new Size(245, 20);
		lblLabel.Location = new Point(30, 20);
		lblLabel.Text = "Spread out (bring closer together) the beeps by ";
		
		txtTextBox.Size = new Size(30, 20);
		txtTextBox.Location = new Point(275, 20);
		
		lblLabel2.Size = new Size(20, 20);
		lblLabel2.Location = new Point(310, 20);
		lblLabel2.Text = "%";
		
		btnOK.Location = new Point(145, 60);
		btnOK.Text = "&OK";
		btnOK.Click += new EventHandler(btnOK_Click);

		btnCancel.Click += new EventHandler(btnCancel_Click);
		btnCancel.Size = new Size(0, 0);

		Controls.AddRange(new Control[] {
			lblLabel,
			txtTextBox,
			lblLabel2,
			btnOK,
			btnCancel});

		Text = Common.SPREAD_BEEPS;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		AcceptButton = btnOK;
		CancelButton = btnCancel;
		StartPosition = FormStartPosition.CenterParent;
		Size = new Size(365, 120);
	}

	void btnOK_Click(object sender, EventArgs e) {
		Close();
	}
	
	void btnCancel_Click(object sender, EventArgs e) {
		Close();
	}
	
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;
		Common.vegas.DebugClear();
		
		Selection selection = new Selection(vegas.Transport.SelectionStart, vegas.Transport.SelectionLength);
		selection.Normalize();
		
		// check for the user has selected exactly one audio track
		List<Track> tracks =
			Common.FindSelectedTracks(Common.AudioTracksToTracks(Audio.FindAudioTracks(Common.vegas.Project)));
		if (tracks.Count != 1) {
			MessageBox.Show("Please make sure you have selected exactly one audio track",
				Common.SPREAD_BEEPS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// at least two native events should exist to continue
		List<TrackEvent> events;
		if (selection.SelectionLength == new Timecode()) {
			// events = Common.TrackEventsToTrackEvents(tracks.Events);
			events = Common.FindNativeEvents(Common.TrackEventsToTrackEvents(tracks[0].Events));
		} else {
			events = Common.FindNativeEvents(Common.FindEventsBySelection(tracks[0], selection));
		}
		
		if (events.Count < 2) {
			MessageBox.Show("Please make sure you have at least two (native) events selected to continue",
				Common.SPREAD_BEEPS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		ShowDialog();
		return;

		/*// sort out which track and event collection is the source
		// and which is the target
		sourceTrack = tracks[0];
		targetTrack = tracks[1];
		
		sourceEvents = Common.FindMeasureStartEvents(events[0]);
		
		// source track (selection) should have at least one (measure start) event to continue
		if (sourceEvents.Count < 1) {
			MessageBox.Show("Please make sure source track (selection) has at least one (measure start) event",
				Common.SPREAD_BEEPS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// make sure each source event has at least one target event that
		// could be split
		foreach (TrackEvent sourceEvent in sourceEvents) {
			string eventName = Common.getFullName(Common.getTakeNames(sourceEvent));
			if (Common.FindEventsByEvent(targetTrack, sourceEvent).Count < 1) {
				MessageBox.Show("Source event " + sourceEvent.Index +
				(eventName == "" ? "" : " (" + eventName + ")") + " does not have a matching target event",
					Common.SPREAD_BEEPS, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}
		
		// find and split events. Copy take names
		int splitEvents = 0;
		foreach (TrackEvent sourceEvent in sourceEvents) {
			string sourceEventName = Common.getFullName(Common.getTakeNames(sourceEvent));
			
			List<TrackEvent> targetEvents = Common.FindEventsByEvent(targetTrack, sourceEvent);
			foreach (TrackEvent targetEvent in targetEvents) {
				// split event
				TrackEvent secondHalfEvent = targetEvent.Split(sourceEvent.Start - targetEvent.Start);
				
				// copy take names
				// RemoveEmptyTakes(secondHalfEvent);
				// AddEmptyTake(secondHalfEvent, sourceEventName);
				
				splitEvents++;
			}
		}
		
		// report
		MessageBox.Show("Split " + splitEvents + " events", Common.SPREAD_BEEPS);*/
	}
	
}
