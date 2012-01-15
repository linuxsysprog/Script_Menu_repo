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
		txtMeasure.Validated += new EventHandler(txtMeasure_Validated);
		
		lblBeat.Size = new Size(40, 20);
		lblBeat.Location = new Point(110, 20);
		lblBeat.Text = "&Beat:";
		
		txtBeat.Size = new Size(30, 50);
		txtBeat.Location = new Point(150, 20);
		txtBeat.Text = "1";
		txtBeat.Validated += new EventHandler(txtBeat_Validated);
		
		lblBPM.Size = new Size(120, 20);
		lblBPM.Location = new Point(10, 80);
		lblBPM.Text = "Beats &per Measure:";
		
		txtBPM.Size = new Size(30, 50);
		txtBPM.Location = new Point(170, 80);
		txtBPM.Text = "4";
		txtBPM.Validated += new EventHandler(txtBPM_Validated);
		
		lblTempo.Size = new Size(50, 20);
		lblTempo.Location = new Point(10, 120);
		lblTempo.Text = "&Tempo:";
		
		txtTempo.Size = new Size(60, 20);
		txtTempo.Location = new Point(140, 120);
		txtTempo.Text = "000.0000";
		txtTempo.Validated += new EventHandler(txtTempo_Validated);
		
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
		
		rbSelection.Size = new Size(100, 20);
		rbSelection.Location = new Point(10, 50);
		rbSelection.Text = "&Selection";
		
		txtNumber.Size = new Size(30, 50);
		txtNumber.Location = new Point(150, 20);
		txtNumber.Text = "16";
		txtNumber.Validated += new EventHandler(txtNumber_Validated);

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
	
	void txtMeasure_Validated(object sender, EventArgs e) {
		int measure;
		
		try {
			measure = Convert.ToInt32(txtMeasure.Text);
			if (measure < 1) {
				throw new Exception("measure is less than one");
			}
		} catch (Exception ex) {
			MessageBox.Show("Invalid Measure");
			txtMeasure.Focus();
		}
	}
	
	void txtBeat_Validated(object sender, EventArgs e) {
		int beat;
		
		try {
			beat = Convert.ToInt32(txtBeat.Text);
			if (beat < 1) {
				throw new Exception("beat is less than one");
			}
		} catch (Exception ex) {
			MessageBox.Show("Invalid Beat");
			txtBeat.Focus();
		}
	}
	
	void txtBPM_Validated(object sender, EventArgs e) {
		int bpm;
		
		try {
			bpm = Convert.ToInt32(txtBPM.Text);
			if (bpm < 1) {
				throw new Exception("Beats per Measure is less than one");
			}
		} catch (Exception ex) {
			MessageBox.Show("Invalid Beats per Measure");
			txtBPM.Focus();
		}
	}
	
	void txtNumber_Validated(object sender, EventArgs e) {
		int number;
		
		try {
			number = Convert.ToInt32(txtNumber.Text);
			if (number < 1) {
				throw new Exception("Number of beeps is less than one");
			}
		} catch (Exception ex) {
			MessageBox.Show("Invalid number of beeps");
			txtNumber.Focus();
		}
	}
	
	void txtTempo_Validated(object sender, EventArgs e) {
		double tempo;
		
		try {
			tempo = Convert.ToDouble(txtTempo.Text);
			if (tempo <= 0) {
				throw new Exception("Tempo is less or equal zero");
			}
		} catch (Exception ex) {
			MessageBox.Show("Invalid Tempo");
			txtTempo.Focus();
		}
	}
	
	void btnGenerate_Click(object sender, EventArgs e) {
		MessageBox.Show("btnGenerate_Click() Entry.");
	}
	
	void btnCancel_Click(object sender, EventArgs e) {
		Close();
	}
	
	//
	// Event handlers END
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;

		Text = Common.GEN_BEEPS;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		AcceptButton = btnGenerate;
		CancelButton = btnCancel;
		StartPosition = FormStartPosition.CenterParent;
		Size = new Size(220, 290);

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

