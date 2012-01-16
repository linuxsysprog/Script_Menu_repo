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
	private GroupBox gbHowMany = new GroupBox();
	private RadioButton rbNumber = new RadioButton();
	private RadioButton rbSelection = new RadioButton();
	private TextBox txtNumber = new TextBox();
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
		
		gbHowMany.Size = new Size(190, 80);
		gbHowMany.Location = new Point(10, 150);
		gbHowMany.Text = "How many beeps";
		gbHowMany.Controls.AddRange(new Control[] {
			rbNumber,
			rbSelection,
			txtNumber});
		
		rbNumber.Size = new Size(100, 20);
		rbNumber.Location = new Point(10, 20);
		rbNumber.Text = "&Number";
		rbNumber.Checked = true;
		rbNumber.Click += new EventHandler(rbNumber_Click);
		
		rbSelection.Size = new Size(100, 20);
		rbSelection.Location = new Point(10, 50);
		rbSelection.Text = "&Selection";
		rbSelection.Click += new EventHandler(rbSelection_Click);
		
		txtNumber.Size = new Size(30, 50);
		txtNumber.Location = new Point(150, 20);
		txtNumber.Text = "16";

		btnGenerate.Location = new Point(70, 250);
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
			gbHowMany,
			btnGenerate,
			btnCancel});

		Size = new Size(220, 310);
	}
	
	//
	// Event handlers BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	void rbNumber_Click(object sender, EventArgs e) {
		txtNumber.Enabled = true;
	}
	
	void rbSelection_Click(object sender, EventArgs e) {
		txtNumber.Enabled = false;
	}
	
	void btnGenerate_Click(object sender, EventArgs e) {
		try {
			validateForm();
		} catch (Exception ex) {
			MessageBox.Show(ex.Message);
			return;
		}
		
		List<AudioTrack> selectedAudioTracks = Common.TracksToAudioTracks(
			Common.FindSelectedTracks(Common.AudioTracksToTracks(Audio.FindAudioTracks(Common.vegas.Project)))
		);
		
		// generate a series of beep positions
		List<Timecode> beepPositions = new List<Timecode>();
		Timecode offset = Common.vegas.Transport.CursorPosition;
		for (int i = 0; i < Convert.ToInt32(txtNumber.Text); i++) {
			double length = (60.0 / Convert.ToDouble(txtTempo.Text)) * 1000;
			beepPositions.Add(offset);
			offset = offset + Timecode.FromMilliseconds(length);
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
		foreach (Timecode beepPosition in beepPositions) {
			Audio.AddBeep(selectedAudioTracks[0], beepPosition,
				Convert.ToInt32(txtMeasure.Text), Convert.ToInt32(txtBeat.Text),
				"" + Convert.ToDouble(txtTempo.Text));
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
		string[] labels = { "Measure", "Beat", "Beats per Measure", "Tempo", "Number" };
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
				if (labels[i] == "Number" && rbSelection.Checked == true) {
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
		Size = new Size(220, 310);

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

