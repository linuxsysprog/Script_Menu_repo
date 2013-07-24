// Copyright (C) 2011-2013 Andrey Chislenko
// $Id$
// Insert a frame with a combination of the following objects (text strings):
// Filename, Notes, Tempo, Rate, Measure

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint : Form {
	private Label lblProjectPath = new Label();
	private MyTextBox txtProjectPath = new MyTextBox();
	private Label lblFilename = new Label();
	private MyTextBox txtFilename = new MyTextBox();
	private Label lblNotes = new Label();
	private MyTextBox txtNotes = new MyTextBox();
	private Label lblTempo = new Label();
	private MyTextBox txtTempo = new MyTextBox();
	private Label lblRate = new Label();
	private MyTextBox txtRate = new MyTextBox();
	private Label lblMeasure = new Label();
	private MyTextBox txtMeasure = new MyTextBox();
	private Button btnAdd = new Button();
	private Button btnCancel = new Button();
	private Button btnClearAll = new Button();
	
	private List<VideoTrack> selectedVideoTracks;
	
	private string configFilename = Common.vegas.InstallationDirectory + "\\Script Menu\\AddObject.cs.config";
	private XmlDocument configXML = new XmlDocument();
	private bool history = true;
	
	public EntryPoint() {
		lblProjectPath.Size = new Size(70, 20);
		lblProjectPath.Location = new Point(10, 10);
		lblProjectPath.Text = "&Project path:";
		
		txtProjectPath.Size = new Size(200, 50);
		txtProjectPath.Location = new Point(80, 10);
		
		lblFilename.Size = new Size(70, 20);
		lblFilename.Location = new Point(10, 50);
		lblFilename.Text = "&Filename:";
		
		txtFilename.Size = new Size(200, 50);
		txtFilename.Location = new Point(80, 50);
		
		lblNotes.Size = new Size(70, 20);
		lblNotes.Location = new Point(10, 90);
		lblNotes.Text = "&Notes:";
		
		txtNotes.Size = new Size(200, 50);
		txtNotes.Location = new Point(80, 90);
		
		lblTempo.Size = new Size(70, 20);
		lblTempo.Location = new Point(10, 130);
		lblTempo.Text = "&Tempo:";
		
		txtTempo.Size = new Size(200, 50);
		txtTempo.Location = new Point(80, 130);
		
		lblRate.Size = new Size(70, 20);
		lblRate.Location = new Point(10, 170);
		lblRate.Text = "&Rate:";
		
		txtRate.Size = new Size(200, 50);
		txtRate.Location = new Point(80, 170);
		
		lblMeasure.Size = new Size(70, 20);
		lblMeasure.Location = new Point(10, 210);
		lblMeasure.Text = "&Measure:";
		
		txtMeasure.Size = new Size(200, 50);
		txtMeasure.Location = new Point(80, 210);
		
		btnAdd.Location = new Point(60, 250);
		btnAdd.Text = "&Add";
		btnAdd.Click += new EventHandler(btnAdd_Click);

		btnCancel.Click += new EventHandler(btnCancel_Click);
		btnCancel.Size = new Size(0, 0);

		btnClearAll.Location = new Point(155, 250);
		btnClearAll.Text = "&Clear All";
		btnClearAll.Click += new EventHandler(btnClearAll_Click);

		Controls.AddRange(new Control[] {
			lblProjectPath,
			txtProjectPath,
			lblFilename,
			txtFilename,
			lblNotes,
			txtNotes,
			lblTempo,
			txtTempo,
			lblRate,
			txtRate,
			lblMeasure,
			txtMeasure,
			btnAdd,
			btnCancel,
			btnClearAll});
	}
	
	//
	// Event handlers BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	void btnAdd_Click(object sender, EventArgs e) {
		// check fields
		if ("" == txtProjectPath.Text) {
			MessageBox.Show("Project path is empty", Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		if ("" == txtFilename.Text &&
				"" == txtNotes.Text &&
				"" == txtTempo.Text &&
				"" == txtRate.Text &&
				"" == txtMeasure.Text) {
			MessageBox.Show("Nothing to add", Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
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
				Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// truncate fields
		if (txtFilename.Text.Length > textGenerator.FilenameLengthMax) {
			txtFilename.Text = txtFilename.Text.Substring(0, textGenerator.FilenameLengthMax);
			MessageBox.Show("Filename truncated",
				Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			txtFilename.Focus();
			return;
		}
		
		if (txtNotes.Text.Length > textGenerator.NotesLengthMax) {
			txtNotes.Text = txtNotes.Text.Substring(0, textGenerator.NotesLengthMax);
			MessageBox.Show("Notes truncated",
				Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			txtNotes.Focus();
			return;
		}
		
		if (txtTempo.Text.Length > textGenerator.TempoLengthMax) {
			txtTempo.Text = txtTempo.Text.Substring(0, textGenerator.TempoLengthMax);
			MessageBox.Show("Tempo truncated",
				Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			txtTempo.Focus();
			return;
		}
		
		if (txtRate.Text.Length > textGenerator.RateLengthMax) {
			txtRate.Text = txtRate.Text.Substring(0, textGenerator.RateLengthMax);
			MessageBox.Show("Rate truncated",
				Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			txtRate.Focus();
			return;
		}
		
		if (txtMeasure.Text.Length > textGenerator.MeasureLengthMax) {
			txtMeasure.Text = txtMeasure.Text.Substring(0, textGenerator.MeasureLengthMax);
			MessageBox.Show("Measure truncated",
				Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			txtMeasure.Focus();
			return;
		}
		
		// add text
		if ("" != txtFilename.Text) {
			textGenerator.AddFilename(txtFilename.Text);
		}
		
		if ("" != txtNotes.Text) {
			textGenerator.AddNotes(txtNotes.Text);
		}
		
		if ("" != txtTempo.Text) {
			textGenerator.AddTempo(txtTempo.Text);
		}
		
		if ("" != txtRate.Text) {
			textGenerator.AddRate(txtRate.Text);
		}
		
		if ("" != txtMeasure.Text) {
			textGenerator.AddMeasure(txtMeasure.Text);
		}
		
		// save frame
		string mediaPath = null;
		try {
			mediaPath = txtProjectPath.Text + "\\" + Common.getNextFilename(txtProjectPath.Text);
			frame.Save(mediaPath);
		} catch (Exception ex) {
			MessageBox.Show("Failed to save frame: " + ex.Message,
				Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// check for existing events
		List<TrackEvent> events = Common.FindEventsByPosition(selectedVideoTracks[0],
			Common.vegas.Transport.CursorPosition);
		
		if (events.Count > 0) {
			string msg = "";
			if (events.Count == 1) {
				msg += "There is already " + events.Count + " event at position ";
			} else {
				msg += "There are already " + events.Count + " events at position ";
			}
			msg += Common.vegas.Transport.CursorPosition + ". Would you like to continue?";
		
			DialogResult result = MessageBox.Show(msg, Common.ADD_OBJECT, MessageBoxButtons.OKCancel,
				MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
			if (result != DialogResult.OK) {
				return;
			}
		}
		
		// create event
		Video.AddObject(selectedVideoTracks[0], Common.vegas.Transport.CursorPosition, mediaPath);
		
		// save to history file
		if (history) {
			try {
				configXML.SelectSingleNode("/ScriptSettings/ProjectPath").InnerText = txtProjectPath.Text;
				configXML.SelectSingleNode("/ScriptSettings/Filename").InnerText = txtFilename.Text;
				configXML.SelectSingleNode("/ScriptSettings/Notes").InnerText = txtNotes.Text;
				configXML.SelectSingleNode("/ScriptSettings/Tempo").InnerText = txtTempo.Text;
				configXML.SelectSingleNode("/ScriptSettings/Rate").InnerText = txtRate.Text;
				configXML.SelectSingleNode("/ScriptSettings/Measure").InnerText = txtMeasure.Text;

				configXML.Save(configFilename);
			} catch (Exception ex) {
				MessageBox.Show("Failed to save config file: " + ex.Message,
					Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		
		Close();
	}
	
	void btnCancel_Click(object sender, EventArgs e) {
		Close();
	}
	
	void btnClearAll_Click(object sender, EventArgs e) {
		txtProjectPath.Text = "";
		txtFilename.Text = "";
		txtNotes.Text = "";
		txtTempo.Text = "";
		txtRate.Text = "";
		txtMeasure.Text = "";
		
		txtProjectPath.Focus();
	}
	
	//
	// Event handlers END
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;

		// setup form
		Text = Common.ADD_OBJECT;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		AcceptButton = btnAdd;
		CancelButton = btnCancel;
		StartPosition = FormStartPosition.CenterParent;
		Size = new Size(300, 310);

		// get the video track to work on
		selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(vegas.Project)))
		);
		if (selectedVideoTracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one video track selected",
				Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Error);
			Close();
			return;
		}
		
		// load config
		try {
			configXML.Load(configFilename);
			
			XmlNode projectPath = configXML.SelectSingleNode("/ScriptSettings/ProjectPath");
			if (null == projectPath) {
				throw new Exception("ProjectPath element not found");
			}
			txtProjectPath.Text = projectPath.InnerText;
			
			XmlNode filename = configXML.SelectSingleNode("/ScriptSettings/Filename");
			if (null == filename) {
				throw new Exception("Filename element not found");
			}
			txtFilename.Text = filename.InnerText;
			
			XmlNode notes = configXML.SelectSingleNode("/ScriptSettings/Notes");
			if (null == notes) {
				throw new Exception("Notes element not found");
			}
			txtNotes.Text = notes.InnerText;
			
			XmlNode tempo = configXML.SelectSingleNode("/ScriptSettings/Tempo");
			if (null == tempo) {
				throw new Exception("Tempo element not found");
			}
			txtTempo.Text = tempo.InnerText;
			
			XmlNode rate = configXML.SelectSingleNode("/ScriptSettings/Rate");
			if (null == rate) {
				throw new Exception("Rate element not found");
			}
			txtRate.Text = rate.InnerText;
			
			XmlNode measure = configXML.SelectSingleNode("/ScriptSettings/Measure");
			if (null == measure) {
				throw new Exception("Measure element not found");
			}
			txtMeasure.Text = measure.InnerText;
		} catch (Exception ex) {
			MessageBox.Show("Failed to load config file. History will be disabled: " + ex.Message,
				Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Error);
			history = false;
		}
		
		// show dialog
		ShowDialog();
	}
	
}

