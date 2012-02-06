// Copyright (C) 2011 Andrey Chislenko
// File: GenBeeps.cs - Generate series of beeps starting from a particular Measure.Beat

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint : Form {
	private GroupBox gbStartWith = new GroupBox();
	private Label lblMeasure = new Label();
	private TextBox txtMeasure = new TextBox();
	private Label lblBeat = new Label();
	private TextBox txtBeat = new TextBox();
	private Label lblBPM = new Label();
	private TextBox txtBPM = new TextBox();
	private Label lblTempo = new Label();
	private TextBox txtTempo = new TextBox();
	private Label lblNumber = new Label();
	private TextBox txtNumber = new TextBox();
	private GroupBox gbMode = new GroupBox();
	private RadioButton rbTempoNumber = new RadioButton();
	private RadioButton rbTempoSelection = new RadioButton();
	private RadioButton rbSelectionNumber = new RadioButton();
	private Button btnGenerate = new Button();
	private Button btnCancel = new Button();		

	public EntryPoint() {
		gbStartWith.Size = new Size(190, 50);
		gbStartWith.Location = new Point(10, 10);
		gbStartWith.Text = "Start with";
		gbStartWith.Controls.AddRange(new Control[] {
			lblMeasure,
			txtMeasure,
			lblBeat,
			txtBeat});
		
		lblMeasure.Size = new Size(60, 20);
		lblMeasure.Location = new Point(10, 20);
		lblMeasure.Text = "&Measure:";
		
		txtMeasure.Size = new Size(30, 50);
		txtMeasure.Location = new Point(70, 20);
		txtMeasure.Text = "1";
		
		lblBeat.Size = new Size(40, 20);
		lblBeat.Location = new Point(110, 20);
		lblBeat.Text = "&Beat:";
		
		txtBeat.Size = new Size(30, 50);
		txtBeat.Location = new Point(150, 20);
		txtBeat.Text = "1";
		
		lblBPM.Size = new Size(120, 20);
		lblBPM.Location = new Point(10, 80);
		lblBPM.Text = "Beats &per Measure:";
		
		txtBPM.Size = new Size(30, 50);
		txtBPM.Location = new Point(170, 80);
		txtBPM.Text = "4";
		
		lblTempo.Size = new Size(50, 20);
		lblTempo.Location = new Point(10, 120);
		lblTempo.Text = "&Tempo:";
		
		txtTempo.Size = new Size(60, 20);
		txtTempo.Location = new Point(140, 120);
		txtTempo.Text = "120.0000";
		txtTempo.Enabled = false;
		
		lblNumber.Size = new Size(100, 20);
		lblNumber.Location = new Point(10, 160);
		lblNumber.Text = "&Number of Beeps:";
		
		txtNumber.Size = new Size(30, 50);
		txtNumber.Location = new Point(170, 160);
		txtNumber.Text = "4";

		gbMode.Size = new Size(190, 110);
		gbMode.Location = new Point(10, 200);
		gbMode.Text = "&Mode";
		gbMode.Controls.AddRange(new Control[] {
			rbTempoNumber,
			rbTempoSelection,
			rbSelectionNumber});
		
		rbTempoNumber.Size = new Size(170, 20);
		rbTempoNumber.Location = new Point(10, 20);
		rbTempoNumber.Text = "Tempo and Number";
		rbTempoNumber.Click += new EventHandler(rbTempoNumber_Click);
		
		rbTempoSelection.Size = new Size(170, 20);
		rbTempoSelection.Location = new Point(10, 50);
		rbTempoSelection.Text = "Tempo and Selection";
		rbTempoSelection.Click += new EventHandler(rbTempoSelection_Click);
		
		rbSelectionNumber.Size = new Size(170, 20);
		rbSelectionNumber.Location = new Point(10, 80);
		rbSelectionNumber.Text = "Selection and Number";
		rbSelectionNumber.Checked = true;
		rbSelectionNumber.Click += new EventHandler(rbSelectionNumber_Click);
		
		btnGenerate.Location = new Point(70, 330);
		btnGenerate.Text = "&Generate";
		btnGenerate.Click += new EventHandler(btnGenerate_Click);

		btnCancel.Click += new EventHandler(btnCancel_Click);
		btnCancel.Size = new Size(0, 0);

		Controls.AddRange(new Control[] {
			gbStartWith,
			lblBPM,
			txtBPM,
			lblTempo,
			txtTempo,
			lblNumber,
			txtNumber,
			gbMode,
			btnGenerate,
			btnCancel});

		Size = new Size(220, 390);
	}
	
	//
	// Event handlers BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	void rbTempoNumber_Click(object sender, EventArgs e) {
		txtTempo.Enabled = true;
		txtNumber.Enabled = true;
	}
	
	void rbTempoSelection_Click(object sender, EventArgs e) {
		txtTempo.Enabled = true;
		txtNumber.Enabled = false;
	}
	
	void rbSelectionNumber_Click(object sender, EventArgs e) {
		txtTempo.Enabled = false;
		txtNumber.Enabled = true;
	}
	
	void btnGenerate_Click(object sender, EventArgs e) {
		try {
			validateForm();
		} catch (Exception ex) {
			MessageBox.Show(ex.Message, Common.GEN_BEEPS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// check selection
		Selection selection = new Selection(Common.vegas.Transport.SelectionStart,
			Common.vegas.Transport.SelectionLength);
		selection.Normalize();
		if (!rbTempoNumber.Checked && selection.SelectionLength == new Timecode()) {
			MessageBox.Show("Selection is zero", Common.GEN_BEEPS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		List<AudioTrack> selectedAudioTracks = Common.TracksToAudioTracks(
			Common.FindSelectedTracks(Common.AudioTracksToTracks(Audio.FindAudioTracks(Common.vegas.Project)))
		);
		
		// generate a series of beep positions
		List<Timecode> beepPositions = new List<Timecode>();
		Timecode offset = selection.SelectionStart;
		double step;
		int nBeats;
		
		if (rbTempoNumber.Checked) {
			offset = Common.vegas.Transport.CursorPosition;
			
			step = (60.0 / Convert.ToDouble(txtTempo.Text)) * 1000;
			nBeats = Convert.ToInt32(txtNumber.Text);
		} else if (rbTempoSelection.Checked) {
			step = (60.0 / Convert.ToDouble(txtTempo.Text)) * 1000;
			nBeats = (int)(Convert.ToDouble(txtTempo.Text) *
				selection.SelectionLength.ToMilliseconds() / 1000.0 / 60.0);
			CustomMessageBox.Show(CustomMessageBoxMode.Number, "" + nBeats);
		} else {
			nBeats = Convert.ToInt32(txtNumber.Text);
			step = selection.SelectionLength.ToMilliseconds() / (double)nBeats;
			
			double tempo = 60.0 / (step / 1000.0);
			CustomMessageBox.Show(CustomMessageBoxMode.Tempo, tempo.ToString("F4"));
		}

		for (int i = 0; i < nBeats; i++) {
			beepPositions.Add(offset);
			offset = offset + Timecode.FromMilliseconds(step);
		}
		
		// dump the above list
		// Common.vegas.DebugClear();
		// foreach (Timecode beepPosition in beepPositions) {
			// Common.vegas.DebugOut("" + beepPosition.ToMilliseconds());
		// }
		
		// check for pre-existing events
		List<TrackEvent> existingEvents = new List<TrackEvent>();
		foreach (Timecode beepPosition in beepPositions) {
			existingEvents.AddRange(Common.FindEventsByPosition(selectedAudioTracks[0], beepPosition));
		}

		// dump the above list
		// Common.vegas.DebugClear();
		// foreach (TrackEvent existingEvent in existingEvents) {
			// Common.vegas.DebugOut("" + existingEvent.Start.ToMilliseconds());
		// }

		foreach (TrackEvent existingEvent in existingEvents) {
			string msg = "There is already an event at position " +
				existingEvent.Start + ". Would you like to continue?";
		
			DialogResult result = MessageBox.Show(msg, Common.ADD_BEEP, MessageBoxButtons.OKCancel,
				MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
			if (result != DialogResult.OK) {
				return;
			}
		}
		
		// insert beeps
		int measure = Convert.ToInt32(txtMeasure.Text);
		int beat = Convert.ToInt32(txtBeat.Text);
		int bpm = Convert.ToInt32(txtBPM.Text);
		foreach (Timecode beepPosition in beepPositions) {
			int rem = beat % bpm;
		
			Audio.AddBeep(selectedAudioTracks[0], beepPosition, measure, rem == 0 ? bpm : rem, ".");
				
			beat++;
			if (rem == 0) {
				measure++;
			}
		}
		
		MessageBox.Show("Inserted " + beepPositions.Count + " beeps", Common.GEN_BEEPS);
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
		string[] labels = { "Measure", "Beat", "Beats per Measure", "Tempo", "Number of Beeps" };
		TextBox[] controls = { txtMeasure, txtBeat, txtBPM, txtTempo, txtNumber };
		
		for (int i = 0; i < labels.Length; i++) {
			try {
				if (labels[i] == "Tempo") {
					double f = Convert.ToDouble(controls[i].Text);
					if (f <= 0) {
						throw new Exception(labels[i] + " is less or equal zero");
					}
				} else {
					int n = Convert.ToInt32(controls[i].Text);
					if (n < 1) {
						throw new Exception(labels[i] + " is less than one");
					}
				}
			} catch (Exception ex) {
				if ((labels[i] == "Number of Beeps" && rbTempoSelection.Checked == true) ||
						(labels[i] == "Tempo" && rbSelectionNumber.Checked == true)) {
					continue;
				}
				controls[i].Focus();
				controls[i].SelectAll();
				throw new Exception("Invalid " + labels[i]);
			}
		}
	}
	
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;

		Text = Common.GEN_BEEPS;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		AcceptButton = btnGenerate;
		CancelButton = btnCancel;
		StartPosition = FormStartPosition.CenterParent;
		Size = new Size(220, 390);

		List<AudioTrack> selectedAudioTracks = Common.TracksToAudioTracks(
			Common.FindSelectedTracks(Common.AudioTracksToTracks(Audio.FindAudioTracks(vegas.Project)))
		);
		if (selectedAudioTracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one audio track selected",
				Common.GEN_BEEPS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			Close();
			return;
		}
		
		ShowDialog();
	}

	public static void Main() {
		Application.Run(new EntryPoint());
	}
	
}

public enum CustomMessageBoxMode {
	Tempo,
	Number
}

public class CustomMessageBox : Form {
	private Label lblLabel = new Label();
	private TextBox txtTextBox = new TextBox();
	private Button btnOK = new Button();
	private Button btnCancel = new Button();
	
	static public void Show(CustomMessageBoxMode mode, string value) {
		CustomMessageBox customMessageBox = new CustomMessageBox(mode, value);
		customMessageBox.ShowDialog();
	}

	public CustomMessageBox(CustomMessageBoxMode mode, string value) {
		txtTextBox.ReadOnly = true;
		txtTextBox.Text = value;
		
		if (mode == CustomMessageBoxMode.Tempo) {
			lblLabel.Size = new Size(130, 20);
			lblLabel.Location = new Point(80, 20);
			lblLabel.Text = "The calculated Tempo is ";
			
			txtTextBox.Size = new Size(60, 20);
			txtTextBox.Location = new Point(210, 20);
		} else {
			lblLabel.Size = new Size(180, 20);
			lblLabel.Location = new Point(70, 20);
			lblLabel.Text = "The calculated Number of Beeps is ";
			
			txtTextBox.Size = new Size(30, 20);
			txtTextBox.Location = new Point(250, 20);
		}
		
		btnOK.Location = new Point(145, 60);
		btnOK.Text = "&OK";
		btnOK.Click += new EventHandler(btnOK_Click);

		btnCancel.Click += new EventHandler(btnCancel_Click);
		btnCancel.Size = new Size(0, 0);

		Controls.AddRange(new Control[] {
			lblLabel,
			txtTextBox,
			btnOK,
			btnCancel});

		Text = Common.GEN_BEEPS;
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
	
}

