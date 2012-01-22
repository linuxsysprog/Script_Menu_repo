// File: AddBeep.cs - Insert a beep with high and low pitch as takes

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint : Form {
	private Label lblMeasure = new Label();
	private TextBox txtMeasure = new TextBox();
	private Label lblBeat = new Label();
	private TextBox txtBeat = new TextBox();
	private Label lblNotes = new Label();
	private TextBox txtNotes = new TextBox();
	private Button btnAdd = new Button();
	private Button btnCancel = new Button();		

	public EntryPoint() {
		lblMeasure.Size = new Size(60, 20);
		lblMeasure.Location = new Point(10, 10);
		lblMeasure.Text = "&Measure:";
		
		txtMeasure.Size = new Size(30, 50);
		txtMeasure.Location = new Point(70, 10);
		txtMeasure.Text = "1";
		
		lblBeat.Size = new Size(40, 20);
		lblBeat.Location = new Point(110, 10);
		lblBeat.Text = "&Beat:";
		
		txtBeat.Size = new Size(30, 50);
		txtBeat.Location = new Point(150, 10);
		txtBeat.Text = "1";
		
		lblNotes.Size = new Size(40, 60);
		lblNotes.Location = new Point(10, 50);
		lblNotes.Text = "N&otes:";
		
		txtNotes.Multiline = true;
		txtNotes.ScrollBars = ScrollBars.Vertical;
		txtNotes.Size = new Size(120, 60);
		txtNotes.Location = new Point(60, 50);
		txtNotes.Text = "[Section]";
		
		btnAdd.Location = new Point(60, 130);
		btnAdd.Text = "&Add";
		btnAdd.Click += new EventHandler(btnAdd_Click);

		btnCancel.Click += new EventHandler(btnCancel_Click);
		btnCancel.Size = new Size(0, 0);

		Controls.AddRange(new Control[] {
			lblMeasure,
			txtMeasure,
			lblBeat,
			txtBeat,
			lblNotes,
			txtNotes,
			btnAdd,
			btnCancel});
	}
	
	//
	// Event handlers BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	void btnAdd_Click(object sender, EventArgs e) {
		try {
			validateForm();
		} catch (Exception ex) {
			MessageBox.Show(ex.Message, Common.ADD_BEEP, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		List<AudioTrack> selectedAudioTracks = Common.TracksToAudioTracks(
			Common.FindSelectedTracks(Common.AudioTracksToTracks(Audio.FindAudioTracks(Common.vegas.Project)))
		);
		
		List<TrackEvent> events = Common.FindEventsByPosition(selectedAudioTracks[0],
			Common.vegas.Transport.CursorPosition);
		
		if (events.Count > 0) {
			string msg = "";
			if (events.Count == 1) {
				msg += "There is already " + events.Count + " event at position ";
			} else {
				msg += "There are already " + events.Count + " events at position ";
			}
			msg += Common.vegas.Transport.CursorPosition + ". Would you like to continue?";
		
			DialogResult result = MessageBox.Show(msg, Common.ADD_BEEP, MessageBoxButtons.OKCancel,
				MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
			if (result != DialogResult.OK) {
				return;
			}
		}
		
		Audio.AddBeep(selectedAudioTracks[0], Common.vegas.Transport.CursorPosition,
			Convert.ToInt32(txtMeasure.Text), Convert.ToInt32(txtBeat.Text),
			txtNotes.Text == "[Section]" ? "." : txtNotes.Text);
			
		Close();
	}
	
	void btnCancel_Click(object sender, EventArgs e) {
		Close();
	}
	
	//
	// Event handlers END
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	private void validateForm() {
		string[] labels = { "Measure", "Beat" };
		TextBox[] controls = { txtMeasure, txtBeat };
		
		for (int i = 0; i < labels.Length; i++) {
			try {
				int n = Convert.ToInt32(controls[i].Text);
				if (n < 1) {
					throw new Exception(labels[i] + " is less than one");
				}
			} catch (Exception ex) {
				controls[i].Focus();
				controls[i].SelectAll();
				throw new Exception("Invalid " + labels[i]);
			}
		}
	}
	
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;

		Text = Common.ADD_BEEP;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		AcceptButton = btnAdd;
		CancelButton = btnCancel;
		StartPosition = FormStartPosition.CenterParent;
		Size = new Size(200, 190);

		List<AudioTrack> selectedAudioTracks = Common.TracksToAudioTracks(
			Common.FindSelectedTracks(Common.AudioTracksToTracks(Audio.FindAudioTracks(vegas.Project)))
		);
		if (selectedAudioTracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one audio track selected",
				Common.ADD_BEEP, MessageBoxButtons.OK, MessageBoxIcon.Error);
			Close();
			return;
		}
		
		ShowDialog();
	}

}

