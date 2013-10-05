// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Spread out (or bring closer together) the beeps proportionally by a certain percentage.
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
		
		btnOK.Location = new Point(100, 60);
		btnOK.Text = "&OK";
		btnOK.Click += new EventHandler(btnOK_Click);

		btnCancel.Location = new Point(180, 60);
		btnCancel.Text = "&Cancel";
		btnCancel.Click += new EventHandler(btnCancel_Click);

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
		try {
			validateForm();
		} catch (Exception ex) {
			MessageBox.Show(ex.Message, Common.SPREAD_BEEPS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			txtTextBox.Focus();
			txtTextBox.SelectAll();
			return;
		}
		
		Selection selection =
			new Selection(Common.vegas.Transport.SelectionStart, Common.vegas.Transport.SelectionLength);
		selection.Normalize();
		
		List<Track> tracks =
			Common.FindSelectedTracks(Common.AudioTracksToTracks(Audio.FindAudioTracks(Common.vegas.Project)));
		
		List<TrackEvent> events;
		if (selection.SelectionLength == new Timecode()) {
			events = Common.FindNativeEvents(Common.TrackEventsToTrackEvents(tracks[0].Events));
		} else {
			events = Common.FindNativeEvents(Common.FindEventsBySelection(tracks[0], selection));
		}
		
		// spread out events proportionally
		int adjustedEvents = 0;
		for (int i = 0; i < events.ToArray().Length - 1; i++) {
			Timecode offset = (events[i + 1].Start - events[i].Start) -
				Timecode.FromNanos((int)Math.Round(
					(events[i + 1].Start - events[i].Start).Nanos * (Convert.ToDouble(txtTextBox.Text) / 100.0))
				);
				
			int count = 0;
			for (int j = i + 1; j < events.ToArray().Length; j++) {
				if (events[j].Start != events[j].Start - offset) {
					events[j].Start = events[j].Start - offset;
					count++;
				}
			}
			
			if (count != 0) {
				adjustedEvents++;
			}
		}
		
		// report
		MessageBox.Show("Adjusted the start of " + adjustedEvents + " events", Common.SPREAD_BEEPS);
		Close();
	}
	
	void btnCancel_Click(object sender, EventArgs e) {
		Close();
	}
	
	private void validateForm() {
		double f;
		try {
			f = Convert.ToDouble(txtTextBox.Text);
		} catch (Exception ex) {
			throw new Exception("Invalid percentage");
		}
		if (f < 25.0 || f > 1600.0) {
			throw new Exception("Percentage should stay within 25.0% - 1600.0% range");
		}
	}
	
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;
		
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
	}
	
}

