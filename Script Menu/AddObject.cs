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
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint : Form {
	private Label lblFilename = new Label();
	private TextBox txtFilename = new TextBox();
	private Label lblNotes = new Label();
	private TextBox txtNotes = new TextBox();
	private Label lblTempo = new Label();
	private TextBox txtTempo = new TextBox();
	private Label lblRate = new Label();
	private TextBox txtRate = new TextBox();
	private Label lblMeasure = new Label();
	private TextBox txtMeasure = new TextBox();
	private Button btnAdd = new Button();
	private Button btnCancel = new Button();
	private Button btnClearAll = new Button();
	
	private List<VideoTrack> selectedVideoTracks;

	public EntryPoint() {
		lblFilename.Size = new Size(70, 20);
		lblFilename.Location = new Point(10, 10);
		lblFilename.Text = "&Filename:";
		
		txtFilename.Size = new Size(200, 50);
		txtFilename.Location = new Point(80, 10);
		
		lblNotes.Size = new Size(70, 20);
		lblNotes.Location = new Point(10, 50);
		lblNotes.Text = "&Notes:";
		
		txtNotes.Size = new Size(200, 50);
		txtNotes.Location = new Point(80, 50);
		
		lblTempo.Size = new Size(70, 20);
		lblTempo.Location = new Point(10, 90);
		lblTempo.Text = "&Tempo:";
		
		txtTempo.Size = new Size(200, 50);
		txtTempo.Location = new Point(80, 90);
		
		lblRate.Size = new Size(70, 20);
		lblRate.Location = new Point(10, 130);
		lblRate.Text = "&Rate:";
		
		txtRate.Size = new Size(200, 50);
		txtRate.Location = new Point(80, 130);
		
		lblMeasure.Size = new Size(70, 20);
		lblMeasure.Location = new Point(10, 170);
		lblMeasure.Text = "&Measure:";
		
		txtMeasure.Size = new Size(200, 50);
		txtMeasure.Location = new Point(80, 170);
		
		btnAdd.Location = new Point(60, 210);
		btnAdd.Text = "&Add";
		btnAdd.Click += new EventHandler(btnAdd_Click);

		btnCancel.Click += new EventHandler(btnCancel_Click);
		btnCancel.Size = new Size(0, 0);

		btnClearAll.Location = new Point(155, 210);
		btnClearAll.Text = "&Clear All";
		btnClearAll.Click += new EventHandler(btnClearAll_Click);

		Controls.AddRange(new Control[] {
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
		Close();
	}
	
	void btnCancel_Click(object sender, EventArgs e) {
		Close();
	}
	
	void btnClearAll_Click(object sender, EventArgs e) {
		txtFilename.Text = "";
		txtNotes.Text = "";
		txtTempo.Text = "";
		txtRate.Text = "";
		txtMeasure.Text = "";
		
		txtFilename.Focus();
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
		Size = new Size(300, 270);

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
		
		// show dialog
		ShowDialog();
	}
	
}

