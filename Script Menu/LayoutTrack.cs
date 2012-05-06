// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Layout the track according to user's preferences.
// The script accepts two tracks, source and target. Source track is the audio beep track.
// Target track is either audio or video. The script supports selection.

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint : Form {
	private GroupBox gbSize= new GroupBox();
	private Label lblChunk = new Label();
	private ComboBox cbChunk = new ComboBox();
	private Label lblCount = new Label();
	private ComboBox cbCount = new ComboBox();
	private Label lblMargins = new Label();
	private ComboBox cbMargins = new ComboBox();
	private CheckBox chkMiddle = new CheckBox();
	private CheckBox chkHead = new CheckBox();
	private CheckBox chkTail = new CheckBox();
	private Button btnOK = new Button();
	private Button btnCancel = new Button();
	
	private List<Track> tracks;
	private Track sourceTrack;
	private Track targetTrack;
	private Selection selection;
	
	public EntryPoint() {
		gbSize.Size = new Size(140, 110);
		gbSize.Location = new Point(10, 10);
		gbSize.Text = "Size in beats";
		gbSize.Controls.AddRange(new Control[] {
			lblChunk,
			cbChunk,
			lblCount,
			cbCount,
			lblMargins,
			cbMargins});
		
		lblChunk.Size = new Size(75, 20);
		lblChunk.Location = new Point(10, 20);
		lblChunk.Text = "&Chunk:";
		
		cbChunk.Size = new Size(40, 20);
		cbChunk.Location = new Point(90, 20);
		cbChunk.DropDownStyle = ComboBoxStyle.DropDownList;
		cbChunk.SelectedValueChanged += new EventHandler(cbChunk_SelectedValueChanged);
		
		lblCount.Size = new Size(75, 20);
		lblCount.Location = new Point(10, 50);
		lblCount.Text = "&In/Out Count:";
		
		cbCount.Size = new Size(40, 20);
		cbCount.Location = new Point(90, 50);
		cbCount.DropDownStyle = ComboBoxStyle.DropDownList;
		cbCount.SelectedValueChanged += new EventHandler(cbCount_SelectedValueChanged);
		
		lblMargins.Size = new Size(75, 20);
		lblMargins.Location = new Point(10, 80);
		lblMargins.Text = "M&argins:";
		
		cbMargins.Size = new Size(40, 20);
		cbMargins.Location = new Point(90, 80);
		cbMargins.DropDownStyle = ComboBoxStyle.DropDownList;
		cbMargins.SelectedValueChanged += new EventHandler(cbMargins_SelectedValueChanged);
		
		chkMiddle.Size = new Size(150, 20);
		chkMiddle.Location = new Point(10, 130);
		chkMiddle.Text = "Cut at the &Middle";
		chkMiddle.Click += new EventHandler(chkMiddle_Click);
		
		chkHead.Size = new Size(150, 20);
		chkHead.Location = new Point(10, 150);
		chkHead.Text = "Include &Head";
		chkHead.Click += new EventHandler(chkHead_Click);
		
		chkTail.Size = new Size(150, 20);
		chkTail.Location = new Point(10, 170);
		chkTail.Text = "Include &Tail";
		chkTail.Click += new EventHandler(chkTail_Click);
		
		btnOK.Location = new Point(45, 205);
		btnOK.Text = "&OK";
		btnOK.Click += new EventHandler(btnOK_Click);

		btnCancel.Click += new EventHandler(btnCancel_Click);
		btnCancel.Size = new Size(0, 0);

		Controls.AddRange(new Control[] {
			gbSize,
			chkMiddle,
			chkHead,
			chkTail,
			btnOK,
			btnCancel});

		Text = Common.LAYOUT_TRACK;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		AcceptButton = btnOK;
		CancelButton = btnCancel;
		StartPosition = FormStartPosition.CenterParent;
		Size = new Size(170, 265);
		
		initializeForm();
	}

	//
	// Event handlers BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	void btnOK_Click(object sender, EventArgs e) {
		// find events on both tracks
		List<TrackEvent>[] events = new List<TrackEvent>[2];
		for (int i = 0; i < 2; i++) {
			if (selection.SelectionLength == new Timecode()) {
				events[i] = Common.TrackEventsToTrackEvents(tracks[i].Events);
			} else {
				events[i] = Common.FindEventsBySelection(tracks[i], selection);
			}
		}

		// sort out which event collection is the source and which is the target
		List<TrackEvent> sourceEvents = events[0];
		List<TrackEvent> targetEvents = events[1];
		
		int chunkSize = Convert.ToInt32(cbChunk.Text);
		
		// source track (selection) should have at least (chunkSize + 1) beats
		if (sourceEvents.Count < chunkSize + 1) {
			MessageBox.Show("Please make sure you have selected at least (chunk size + 1) beats",
				Common.LAYOUT_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// the number of beats should be in increments of chunk size plus one beat
		if (((sourceEvents.Count - 1) % chunkSize) != 0) {
			MessageBox.Show("Please make sure the number of beats is in increments of chunk size plus one beat",
				Common.LAYOUT_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// make sure the first and the last source events have at least one target event that
		// could be split
		for (int i = 0; i < sourceEvents.Count; i += sourceEvents.Count - 1) {
			int targetEventCount;
			if (targetTrack.IsAudio()) {
				targetEventCount = Common.FindEventsByEvent(targetTrack, sourceEvents[i]).Count;
			} else if (targetTrack.IsVideo()) {
				QuantizedEvent quantizedEvent = QuantizedEvent.FromTimecode(sourceEvents[i].Start);
				targetEventCount = Common.FindEventsByQuantizedEvent(targetTrack, quantizedEvent).Count;
			} else {
				throw new Exception("target track neither audio nor video");
			}
			
			if (targetEventCount < 1) {
				string eventName = Common.getFullName(Common.getTakeNames(sourceEvents[i]));
				MessageBox.Show("Source event " + sourceEvents[i].Index +
				(eventName == "" ? "" : " (" + eventName + ")") + " does not have a matching target event",
					Common.LAYOUT_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}
		
		MessageBox.Show("Invaders!");
		
	}
	
	void btnCancel_Click(object sender, EventArgs e) {
		Close();
	}
	
	void cbChunk_SelectedValueChanged(object sender, EventArgs e) {
		if (cbChunk.Text == "1") {
			chkMiddle.Enabled = false;
		} else {
			chkMiddle.Enabled = true;
		}
	}
	
	void cbCount_SelectedValueChanged(object sender, EventArgs e) {
	}
	
	void cbMargins_SelectedValueChanged(object sender, EventArgs e) {
	}
	
	void chkMiddle_Click(object sender, EventArgs e) {
	}
	
	void chkHead_Click(object sender, EventArgs e) {
	}
	
	void chkTail_Click(object sender, EventArgs e) {
	}
	
	/*void btnOK_Click(object sender, EventArgs e) {
		try {
			validateForm();
		} catch (Exception ex) {
			MessageBox.Show(ex.Message, Common.LAYOUT_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
		MessageBox.Show("Adjusted the start of " + adjustedEvents + " events", Common.LAYOUT_TRACK);
		Close();
	}*/
	
	//
	// Event handlers END
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	private void initializeForm() {
		cbChunk.Items.AddRange(getRange(1, 16));
		cbChunk.SelectedIndex = 3;
		
		cbCount.Items.AddRange(getRange(2, 4));
		cbCount.SelectedIndex = 0;
		
		cbMargins.Items.AddRange(getRange(1, 4));
		cbMargins.SelectedIndex = 0;
		
		chkMiddle.Checked = false;
		chkMiddle.Checked = false;
		chkMiddle.Checked = false;
	}
	
	private string[] getRange(int min, int max) {
		List<string> list = new List<string>();
		for (int i = min; i < max + 1; i++) {
			list.Add("" + i);
		}
		return list.ToArray();
	}
	
	/*private void validateForm() {
		double f;
		try {
			f = Convert.ToDouble(txtTextBox.Text);
		} catch (Exception ex) {
			throw new Exception("Invalid percentage");
		}
		if (f < 25.0 || f > 400.0) {
			throw new Exception("Percentage should stay within 25.0% - 400.0% range");
		}
	}*/
	
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;
		
		selection = new Selection(vegas.Transport.SelectionStart, vegas.Transport.SelectionLength);
		selection.Normalize();
		
		// check for the user has selected exactly two tracks
		tracks = Common.FindSelectedTracks(vegas.Project.Tracks);
		if (tracks.Count != 2) {
			MessageBox.Show("Please make sure you have exactly two tracks selected",
				Common.LAYOUT_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// sort out which track is the source and which is the target
		sourceTrack = tracks[0];
		targetTrack = tracks[1];
		
		// source track must be audio
		if (!sourceTrack.IsAudio()) {
			MessageBox.Show("Please make sure the source (upper) track is audio",
				Common.LAYOUT_TRACK, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		ShowDialog();
	}
	
}

