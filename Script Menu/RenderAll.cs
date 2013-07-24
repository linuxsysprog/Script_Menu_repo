// Copyright (C) 2013 Andrey Chislenko
// $Id$
// Finds and renders splubs into final mp4s or into tracks for master project.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Xml;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint : Form {
	private Label lblRenderer = new Label();
	private MyTextBox txtRenderer = new MyTextBox();
	private Label lblRenderTemplate = new Label();
	private MyTextBox txtRenderTemplate = new MyTextBox();
	private Label lblOutputPath = new Label();
	private MyTextBox txtOutputPath = new MyTextBox();
	private Label lblRenderPlan = new Label();
	private MyTextBox txtRenderPlan = new MyTextBox();
	private RadioButton rbRegions = new RadioButton();
	private RadioButton rbTracks = new RadioButton();
	private Button btnContinue = new Button();
	private Button btnCancel = new Button();
	
	private string configFilename = Common.vegas.InstallationDirectory + "\\Script Menu\\RenderAll.cs.config";
	private XmlDocument configXML = new XmlDocument();
	private bool history = true;	
	
	private List<Cluster> clusters = new List<Cluster>();
	private Regex regex = new Regex("^(Split|Take)");
		
	public EntryPoint() {
		lblRenderer.Size = new Size(70, 35);
		lblRenderer.Location = new Point(10, 10);
		lblRenderer.Text = "R&enderer:";
		
		txtRenderer.Size = new Size(400, 50);
		txtRenderer.Location = new Point(80, 10);
		
		lblRenderTemplate.Size = new Size(70, 35);
		lblRenderTemplate.Location = new Point(10, 50);
		lblRenderTemplate.Text = "Render te&mplate:";
		
		txtRenderTemplate.Size = new Size(400, 50);
		txtRenderTemplate.Location = new Point(80, 50);
		
		lblOutputPath.Size = new Size(70, 20);
		lblOutputPath.Location = new Point(10, 90);
		lblOutputPath.Text = "&Output path:";
		
		txtOutputPath.Size = new Size(400, 50);
		txtOutputPath.Location = new Point(80, 90);
		
		lblRenderPlan.Size = new Size(70, 20);
		lblRenderPlan.Location = new Point(10, 130);
		lblRenderPlan.Text = "Render &plan:";
		
		txtRenderPlan.Size = new Size(400, 260);
		txtRenderPlan.Location = new Point(80, 130);
		txtRenderPlan.Multiline = true;
		txtRenderPlan.ScrollBars = ScrollBars.Vertical;
		txtRenderPlan.ReadOnly = true;
		
		rbRegions.Size = new Size(70, 20);
		rbRegions.Location = new Point(80, 400);
		rbRegions.Text = "&Regions";
		rbRegions.Checked = true;
		rbRegions.CheckedChanged += new EventHandler(rbRegions_CheckedChanged);
		
		rbTracks.Size = new Size(200, 20);
		rbTracks.Location = new Point(155, 400);
		rbTracks.Text = "Continuous &tracks";
		
		btnContinue.Location = new Point(170, 440);
		btnContinue.Text = "&Continue";
		btnContinue.Click += new EventHandler(btnContinue_Click);

		btnCancel.Location = new Point(260, 440);
		btnCancel.Text = "Cancel";
		btnCancel.Click += new EventHandler(btnCancel_Click);

		Controls.AddRange(new Control[] {
			lblRenderer,
			txtRenderer,
			lblRenderTemplate,
			txtRenderTemplate,
			lblOutputPath,
			txtOutputPath,
			lblRenderPlan,
			txtRenderPlan,
			rbRegions,
			rbTracks,
			btnContinue,
			btnCancel});
	}
	
	//
	// Event handlers BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	void rbRegions_CheckedChanged(object sender, EventArgs e) {
		txtRenderPlan.Text = "";
		rescanProject();
	}
	
	void btnContinue_Click(object sender, EventArgs e) {
		// check fields
		if ("" == txtRenderer.Text) {
			MessageBox.Show("Renderer is empty", Common.RENDER_ALL, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		if ("" == txtRenderTemplate.Text) {
			MessageBox.Show("Render template is empty", Common.RENDER_ALL, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		if ("" == txtOutputPath.Text) {
			MessageBox.Show("Output path is empty", Common.RENDER_ALL, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		Common.vegas.DebugClear();
		foreach (Cluster cluster in clusters) {
			try {
				cluster.Render(txtRenderer.Text, txtRenderTemplate.Text, txtOutputPath.Text);
			} catch (Exception ex) {
				MessageBox.Show("Failed to render: " + ex.Message,
					Common.RENDER_ALL, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}
		
		// save to history file
		if (history) {
			try {
				configXML.SelectSingleNode("/ScriptSettings/Renderer").InnerText = txtRenderer.Text;
				configXML.SelectSingleNode("/ScriptSettings/RenderTemplate").InnerText = txtRenderTemplate.Text;
				configXML.SelectSingleNode("/ScriptSettings/OutputPath").InnerText = txtOutputPath.Text;
				configXML.Save(configFilename);
			} catch (Exception ex) {
				MessageBox.Show("Failed to save config file: " + ex.Message,
					Common.RENDER_ALL, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		
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
	
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;
		
		// setup form
		Text = Common.RENDER_ALL;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		AcceptButton = btnContinue;
		CancelButton = btnCancel;
		StartPosition = FormStartPosition.CenterParent;
		Size = new Size(500, 500);

		// load config
		try {
			configXML.Load(configFilename);
			
			XmlNode renderer = configXML.SelectSingleNode("/ScriptSettings/Renderer");
			if (null == renderer) {
				throw new Exception("Renderer element not found");
			}
			txtRenderer.Text = renderer.InnerText;
			
			XmlNode renderTemplate = configXML.SelectSingleNode("/ScriptSettings/RenderTemplate");
			if (null == renderTemplate) {
				throw new Exception("RenderTemplate element not found");
			}
			txtRenderTemplate.Text = renderTemplate.InnerText;
			
			XmlNode outputPath = configXML.SelectSingleNode("/ScriptSettings/OutputPath");
			if (null == outputPath) {
				throw new Exception("OutputPath element not found");
			}
			txtOutputPath.Text = outputPath.InnerText;
		} catch (Exception ex) {
			MessageBox.Show("Failed to load config file. History will be disabled: " + ex.Message,
				Common.RENDER_ALL, MessageBoxButtons.OK, MessageBoxIcon.Error);
			history = false;
		}
		
		rescanProject();
		
		// show dialog
		ShowDialog();
	}
	
	private void rescanProject() {
		clusters.Clear();
	
		List<Track> tracks = Common.TracksToTracks(Common.vegas.Project.Tracks);
		
		Timecode rightmostBoundary = new Timecode();
		if (rbTracks.Checked) {
			foreach (Track track in tracks) {
				if (!track.IsVideo()) {
					continue;
				}
			
				List<TrackEvent> trackEvents = Common.FindEventsByRegex(Common.TrackEventsToTrackEvents(track.Events), regex);
				if (trackEvents.Count < 1) {
					continue;
				}
				
				if (trackEvents[trackEvents.Count - 1].End > rightmostBoundary) {
					rightmostBoundary = trackEvents[trackEvents.Count - 1].End;
				}
			}
		}

		foreach (Track track in tracks) {
			if (!track.IsVideo()) {
				continue;
			}
		
			List<TrackEvent> trackEvents = Common.FindEventsByRegex(Common.TrackEventsToTrackEvents(track.Events), regex);
			if (trackEvents.Count < 1) {
				continue;
			}
			
			int bottomRulerTrackIndex = getTrackIndex(TrackType.BottomRuler, track.Index);
			
			if (rbRegions.Checked) {
				Timecode clusterStart = trackEvents[0].Start;
				Timecode clusterEnd = null;
				for (int i = 1; i < trackEvents.Count; i++) {
					TrackEvent prevTrackEvent = trackEvents[i - 1];
					TrackEvent nextTrackEvent = trackEvents[i];
					string prevTrackEventName = Common.getFullName(Common.getTakeNamesNonNative(prevTrackEvent));
					string nextTrackEventName = Common.getFullName(Common.getTakeNamesNonNative(nextTrackEvent));
					
					if (prevTrackEventName != nextTrackEventName) {
						clusterEnd = prevTrackEvent.End;
						
						clusters.Add(new Cluster(bottomRulerTrackIndex - 2, bottomRulerTrackIndex - 1,
							bottomRulerTrackIndex, track.Index - 1,
							track.Index, track.Index + 1,
							getTrackIndex(TrackType.Beep, track.Index),
							clusterStart, clusterEnd,
							prevTrackEventName));
						
						clusterStart = nextTrackEvent.Start;
					}
				}
				clusters.Add(new Cluster(bottomRulerTrackIndex - 2, bottomRulerTrackIndex - 1,
					bottomRulerTrackIndex, track.Index - 1,
					track.Index, track.Index + 1,
					getTrackIndex(TrackType.Beep, track.Index),
					clusterStart, trackEvents[trackEvents.Count - 1].End,
					Common.getFullName(Common.getTakeNamesNonNative(trackEvents[trackEvents.Count - 1]))));
			} else {
				clusters.Add(new Cluster(bottomRulerTrackIndex - 2, bottomRulerTrackIndex - 1,
					bottomRulerTrackIndex, track.Index - 1,
					track.Index, track.Index + 1,
					getTrackIndex(TrackType.Beep, track.Index),
					new Timecode(), rightmostBoundary,
					Common.getFullName(Common.getTakeNamesNonNative(trackEvents[0]))));
			}
		}
		
		Timecode count = new Timecode();
		foreach (Cluster cluster in clusters) {
			txtRenderPlan.Text += (cluster.GetLabel() + "\r\n");
			count += cluster.Length;
		}
		txtRenderPlan.Text += ("Total files: " + clusters.Count + " Total length: " + count + " (" + count.ToString(RulerFormat.Time) + ")\r\n");
	}

	private int getTrackIndex(TrackType trackType, int index) {
		Regex regex = new Regex("BPM$");
	
		List<Track> tracks = Common.TracksToTracks(Common.vegas.Project.Tracks);
		if (index > tracks.Count - 1) {
			throw new ArgumentException("index out of range");
		}
		
		if (trackType == TrackType.Beep) {
			for (int i = index; i < tracks.Count; i++) {
				List<TrackEvent> trackEvents = Common.TrackEventsToTrackEvents(tracks[i].Events);
				if (!tracks[i].IsAudio() || trackEvents.Count < 1) {
					continue;
				}
			
				string currentTrackEventName = Common.getFullName(Common.getTakeNames(trackEvents[0]));
				if (regex.Match(currentTrackEventName).Success) {
					return tracks[i].Index;
				}
			}
		} else {
			for (int i = index; i >= 0 ; i--) {
				List<TrackEvent> trackEvents = Common.TrackEventsToTrackEvents(tracks[i].Events);
				if (!tracks[i].IsVideo() || trackEvents.Count < 1) {
					continue;
				}
			
				string currentTrackEventName = Common.getFullName(Common.getTakeNames(trackEvents[0]));
				if (regex.Match(currentTrackEventName).Success) {
					return tracks[i].Index;
				}
			}
		}
		
		throw new Exception("beep track not found");
	}
	
	private enum TrackType {
		Beep,
		BottomRuler
	}
	
}

public class Cluster {
	private int textTrackIndex;
	private int measureTrackIndex;
	private int topRulerTrackIndex;
	private int bottomRulerTrackIndex;
	private int videoTrackIndex;
	private int audioTrackIndex;
	private int beepTrackIndex;
	
	private QuantizedEvent start;
	private QuantizedEvent end;
	
	private string name;
	
	public Cluster(int textTrackIndex,
		int measureTrackIndex,
		int topRulerTrackIndex,
		int bottomRulerTrackIndex,
		int videoTrackIndex,
		int audioTrackIndex,
		int beepTrackIndex,
		Timecode start,
		Timecode end,
		string name) {
			int count = Common.TracksToTracks(Common.vegas.Project.Tracks).Count;
		
			if (textTrackIndex >= count ||
				measureTrackIndex >= count ||
				topRulerTrackIndex >= count ||
				bottomRulerTrackIndex >= count ||
				videoTrackIndex >= count ||
				audioTrackIndex >= count ||
				beepTrackIndex >= count) {
				throw new ArgumentException("index out of range");
			}
		
			this.textTrackIndex = textTrackIndex;
			this.measureTrackIndex = measureTrackIndex;
			this.topRulerTrackIndex = topRulerTrackIndex;
			this.bottomRulerTrackIndex = bottomRulerTrackIndex;
			this.videoTrackIndex = videoTrackIndex;
			this.audioTrackIndex = audioTrackIndex;
			this.beepTrackIndex = beepTrackIndex;
			
			this.start = new QuantizedEvent(start);
			this.end = new QuantizedEvent(end);
			
			this.name = name;
	}
	
	public Timecode Start {
		get {
			return start.QuantizedStart;
		}
	}
	
	public Timecode End {
		get {
			return end.QuantizedStart;
		}
	}
	
	public Timecode Length {
		get {
			return end.QuantizedStart - start.QuantizedStart;
		}
	}
	
	public string Name {
		get {
			return name;
		}
	}
	
	public void Render(string rendererStr, string templateStr, string outputPath) {
		Renderer renderer = Common.vegas.Renderers.FindByName(rendererStr);
		if (null == renderer) {
			throw new Exception("renderer not found");
		}
		
		renderer.Templates.Refresh();
		
		RenderTemplate template = renderer.Templates.FindByName(templateStr);
		if (null == template) {
			throw new Exception("template not found");
		}
		
		string ext = ".avi";
		if (template.FileExtensions.Length > 0) {
			ext = template.FileExtensions[0].Substring(1);
		}
		
		UnFXAudio();
		Solo();
		
		RenderArgs args = new RenderArgs();
		
		args.RenderTemplate = template;
		args.OutputFile = outputPath + "\\" + Name + ext;
		args.Start = Start;
		args.Length = Length;
		
		RenderStatus ret = Common.vegas.Render(args);
		if (RenderStatus.Complete != ret) {
			throw new Exception("RenderStatus is " + ret);
		}
		
		Common.vegas.DebugOut(GetLabel() + " -> " + args.OutputFile);
	}
	
	public override string ToString() {
		return "{textTrackIndex=" + textTrackIndex + ", measureTrackIndex=" + measureTrackIndex + "}\n" +
			"{topRulerTrackIndex=" + topRulerTrackIndex + ", bottomRulerTrackIndex=" + bottomRulerTrackIndex + "}\n" +
			"{videoTrackIndex=" + videoTrackIndex + ", audioTrackIndex=" + audioTrackIndex + "}\n" +
			"{beepTrackIndex=" + beepTrackIndex + "}\n" +
			"{start=" + start + ", end=" + end + ", length=" + Length + "}\n" +
			"{name=" + name + "}\n";
	}
	
	public string GetLabel() {
		string spacer = "    " + "    ";
		
		return (textTrackIndex + 1) + "," +
			(measureTrackIndex + 1) + "," +
			(topRulerTrackIndex + 1) + "," +
			(bottomRulerTrackIndex + 1) + "," +
			(videoTrackIndex + 1) + "," +
			(audioTrackIndex + 1) + "," +
			(beepTrackIndex + 1) + spacer + Name + spacer + Start + "-" + End + " (" + Length + ")";
	}
	
	private void UnFXAudio() {
		List<Track> tracks = Common.TracksToTracks(Common.vegas.Project.Tracks);
		
		tracks[audioTrackIndex].Effects.Clear();
		tracks[beepTrackIndex].Effects.Clear();
	}
	
	private void Solo() {
		List<Track> projectTracks = Common.TracksToTracks(Common.vegas.Project.Tracks);
	
		Common.SoloAllTracks(projectTracks, false);
		Common.MuteAllTracks(projectTracks, true);
		
		List<Track> tracks = new List<Track>();

		tracks.Add(projectTracks[textTrackIndex]);
		tracks.Add(projectTracks[measureTrackIndex]);
		tracks.Add(projectTracks[topRulerTrackIndex]);
		tracks.Add(projectTracks[bottomRulerTrackIndex]);
		tracks.Add(projectTracks[videoTrackIndex]);
		tracks.Add(projectTracks[audioTrackIndex]);
		tracks.Add(projectTracks[beepTrackIndex]);
		
		Common.MuteAllTracks(tracks, false);
	}
	
}

