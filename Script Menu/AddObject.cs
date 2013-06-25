// Copyright (C) 2011-2013 Andrey Chislenko
// $Id$
// Insert a frame with a combination of the following objects (text strings):
// Filename, Notes, Tempo, Rate, Measure

using System;
using System.IO;
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
	private Regex filenameRegex = new Regex("^Object(\\d{4}).png$", RegexOptions.IgnoreCase);
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
	}
	
	//
	// Event handlers BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	void btnAdd_Click(object sender, EventArgs e) {
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
		
		try {
			Common.vegas.DebugClear();
			Common.vegas.DebugOut(getNextFilename());
		} catch (Exception ex) {
			MessageBox.Show("Failed to get next filename: " + ex.Message,
				Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
	
	private string getNextFilename() {
		// list full paths
		string[] paths = null;
		try {
			paths = Directory.GetFiles(txtProjectPath.Text, "Object????.png");
		} catch (Exception ex) {
			throw new Exception("Failed to get files in " + txtProjectPath.Text + ": " + ex.Message);
		}
		
		// trim to basenames
		List<string> files = new List<string>();
		foreach (string path in paths) {
			files.Add(Common.Basename(path));
		}
		
		// filter by regex
		string[] filesFiltered = Common.filterByRegex(files.ToArray(), filenameRegex);
		Array.Sort(filesFiltered);
		
		// get current filename
		string currentFilename = filesFiltered.Length > 0 ? filesFiltered[filesFiltered.Length - 1] : "Object0000.png";
		
		int n = Convert.ToInt32(filenameRegex.Match(currentFilename).Groups[1].Value);
		if (n > 9998) {
			throw new Exception(txtProjectPath + " is full");
		}
		
		return "Object" + (++n).ToString("D4") + ".png";
	}
	
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
		
		// show dialog
		ShowDialog();
	}
	
}

// Courtesy of http://stackoverflow.com/questions/97459/automatically-select-all-text-on-focus-in-winforms-textbox
public class MyTextBox : System.Windows.Forms.TextBox
{
    private bool _focused;

    protected override void OnEnter(EventArgs e)
    {
        base.OnEnter(e);
        if (MouseButtons == MouseButtons.None)
        {
            SelectAll();
            _focused = true;
        }
    }

    protected override void OnLeave(EventArgs e)
    {
        base.OnLeave(e);
        _focused = false;
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        if (!_focused)
        {
            if (SelectionLength == 0)
                SelectAll();
            _focused = true;
        }
    }
}

