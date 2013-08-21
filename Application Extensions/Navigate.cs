// Copyright (C) 2011-2013 Andrey Chislenko
// $Id$
// Navigation and Transport Controls

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Sony.Vegas;
using AddRulerNamespace;

public class Navigate : ICustomCommandModule {
	private CustomCommand navCmd = new CustomCommand(CommandCategory.View, "NavigateCmd");
	private NavigateControl navControl;
	
	public void InitializeModule(Vegas vegas) {
		Common.vegas = vegas;
	}

	public ICollection GetCustomCommands() {
		navCmd.DisplayName = Common.NAV + " View";
		navCmd.IconFile = Common.vegas.InstallationDirectory +
			"\\Application Extensions\\Navigate.cs.png";
		
		// subscribe to events
		navCmd.Invoked += HandleInvoked;
		navCmd.MenuPopup += HandleMenuPopup;
		
		return new CustomCommand[] { navCmd };
	}

	void HandleInvoked(Object sender, EventArgs args) {
		if (!Common.vegas.ActivateDockView(Common.NAV)) {
			DockableControl navView = new DockableControl(Common.NAV);
			
			navControl = new NavigateControl();
			Common.vegas.ProjectClosed += HandleProjectClosed;
			navView.Controls.Add(navControl);
			
			navView.DefaultFloatingSize = new Size(165, 600);
			Common.vegas.LoadDockView(navView);
		}
	}

	void HandleMenuPopup(Object sender, EventArgs args) {
		navCmd.Checked = Common.vegas.FindDockView(Common.NAV);
	}
	
	void HandleProjectClosed(Object sender, EventArgs args) {
		navControl.Init();
	}
	
}

public class NavigateControl : UserControl {
	private Color color = Color.Red;
	
	private Regex rateRegionStartEventRegex = new Regex("1\\.1");
	private Regex BPMRegex = new Regex(" ([0-9\\.]+) BPM$");
	
	private double prevTrackBPM = 0.0;
	private double trackBPM = 0.0;
	
	public Track audioTrack = null;
	public Track beepTrack = null;
	private double[] rates;
	
	private Timer timer = new Timer();
	private int clickCount = 0;
	
	private List<RateRegion> rateRegions = new List<RateRegion>();
	private List<RateRegion> prevRateRegions = new List<RateRegion>();
	
	// audio group box
	private GroupBox gbAudio = new GroupBox();
	
	private Label lblChanLeft = new Label();
	private Label lblChanBoth = new Label();
	private Label lblChanRight = new Label();
	
	private RadioButton rbChanLeft = new MyRadioButton();
	private RadioButton rbChanBoth = new MyRadioButton();
	private RadioButton rbChanRight = new RadioButton();

	private CheckBox chkMuteAudio = new CheckBox();
	private CheckBox chkMuteClick = new CheckBox();
	
	// sel group box
	private GroupBox gbSel = new GroupBox();
	
	private Label lblFrameCount = new Label();
	private long frameCount = 0;
	
	private NumericUpDown spinBeats = new MyNumericUpDown();
	private Label lblBeats = new Label();
	
	private NumericUpDown spinFrames = new MyNumericUpDown();
	private Label lblFrames = new Label();
	
	private GroupBox gbTrimSel = new GroupBox();
	
	private Label lblSelStart = new Label();
	private Label lblSelEnd = new Label();
	
	private Button btnSelStartMinus = new MyButton();
	private Button btnSelStartPlus = new MyButton();
	
	private Button btnSelEndMinus = new MyButton();
	private Button btnSelEndPlus = new MyButton();
	
	private Button btnZoom = new MyButton();
	private Label lblZoom = new Label();
	
	private Button btnReset = new MyButton();
	private Label lblReset = new Label();
	
	// nav group box
	private GroupBox gbNav = new GroupBox();
	
	private Label lblStep = new Label();
	private NumericUpDown spinStep = new MyNumericUpDown();
	private Label lblStepFrames = new Label();
	
	private Button btnUp = new Button();
	private Button btnStepLeft = new Button();
	private Button btnLeft = new Button();
	private Button btnHome = new Button();
	private Button btnRight = new Button();
	private Button btnStepRight = new Button();
	private Button btnDown = new Button();
	
	// TC group box
	private GroupBox gbTC = new GroupBox();
	
	private Button btnPlay = new Button();
	private Button btnPause = new Button();
	private Button btnStop = new Button();
	private Button btnSlower = new Button();
	private Button btnFaster = new Button();
	
	public NavigateControl() {
		rates = new double[] { 100.0, 50.0, 25.0 };
		
		timer.Interval = 500;
		timer.Tick += new EventHandler(timer_Tick);
		
		Size = new Size(165, 600);
	
		Controls.AddRange(new Control[] {
			CreateGroupBoxAudio(),
			CreateGroupBoxSel(),
			CreateGroupBoxNav(),
			CreateGroupBoxTC()});
			
		Init();
			
		// ToggleColor(gbSel);
	}
	
	public void Init() {
		// audio groupbox
		rbChanLeft.Checked = false;
		rbChanBoth.Checked = true;
		rbChanRight.Checked = false;
		
		chkMuteAudio.Checked = false;
		chkMuteClick.Checked = false;
		
		// selection groupbox
		spinBeats.Value = 1;
		spinBeats.Maximum = 255;
		spinBeats.Minimum = 0;
		
		lblFrameCount.Text = "";
		frameCount = 0;
		
		spinFrames.Value = 0;
		spinFrames.Maximum = 255;
		spinFrames.Minimum = 0;
		
		// navigation groupbox
		spinStep.Value = 1;
		spinStep.Maximum = 255;
		spinStep.Minimum = 0;
		
		// everything else
		prevTrackBPM = 0.0;
		trackBPM = 0.0;
		
		audioTrack = null;
		beepTrack = null;
		
		timer.Stop();
		clickCount = 0;
		
		rateRegions.Clear();
		prevRateRegions.Clear();
		
		Common.vegas.Transport.SelectionLength = new Timecode();
	}
	
	private GroupBox CreateGroupBoxAudio() {
		gbAudio.Size = new Size(135, 120);
		gbAudio.Location = new Point(10, 10);
		gbAudio.Text = "Audio";
		gbAudio.Controls.AddRange(new Control[] {
			lblChanLeft,
			lblChanBoth,
			lblChanRight,
			rbChanLeft,
			rbChanBoth,
			rbChanRight,
			chkMuteAudio,
			chkMuteClick});
			
		lblChanLeft.Size = new Size(20, 20);
		lblChanLeft.Location = new Point(10, 20);
		lblChanLeft.Text = "L";
		
		lblChanBoth.Size = new Size(40, 20);
		lblChanBoth.Location = new Point(55, 20);
		lblChanBoth.Text = "Both";
		
		lblChanRight.Size = new Size(20, 20);
		lblChanRight.Location = new Point(110, 20);
		lblChanRight.Text = "R";
		
		rbChanLeft.Size = new Size(50, 20);
		rbChanLeft.Location = new Point(10, 40);
		rbChanLeft.Text = "──";
		rbChanLeft.AutoCheck = false;
		rbChanLeft.Click += new EventHandler(rbChanLeft_Click);
		new ToolTip().SetToolTip(rbChanLeft, "Play left channel only");
		
		rbChanBoth.Size = new Size(50, 20);
		rbChanBoth.Location = new Point(60, 40);
		rbChanBoth.Text = "──";
		rbChanBoth.AutoCheck = false;
		rbChanBoth.Click += new EventHandler(rbChanLeft_Click);
		new ToolTip().SetToolTip(rbChanBoth, "Play both channels");
		
		rbChanRight.Size = new Size(20, 20);
		rbChanRight.Location = new Point(110, 40);
		rbChanRight.AutoCheck = false;
		rbChanRight.Click += new EventHandler(rbChanLeft_Click);
		new ToolTip().SetToolTip(rbChanRight, "Play right channel only");
		
		chkMuteAudio.Size = new Size(100, 20);
		chkMuteAudio.Location = new Point(10, 70);
		chkMuteAudio.Text = "Mute audio";
		chkMuteAudio.Click += new EventHandler(chkMuteAudio_Click);
		new ToolTip().SetToolTip(chkMuteAudio, "Mute audio track");
		
		chkMuteClick.Size = new Size(100, 20);
		chkMuteClick.Location = new Point(10, 90);
		chkMuteClick.Text = "Mute click";
		chkMuteClick.Click += new EventHandler(chkMuteClick_Click);
		new ToolTip().SetToolTip(chkMuteClick, "Mute beep track");
		
		return gbAudio;
	}
	
	private GroupBox CreateGroupBoxSel() {
		gbSel.Size = new Size(135, 180);
		gbSel.Location = new Point(10, 140);
		gbSel.Text = "Selection";
		gbSel.Controls.AddRange(new Control[] {
			lblFrameCount,
			spinBeats,
			lblBeats,
			spinFrames,
			lblFrames,
			gbTrimSel,
			btnZoom,
			lblZoom,
			btnReset,
			lblReset});
			
		lblFrameCount.Size = new Size(115, 20);
		lblFrameCount.Location = new Point(10, 20);
		
		spinBeats.Size = new Size(40, 20);
		spinBeats.Location = new Point(10, 45);
		new ToolTip().SetToolTip(spinBeats, "Add full beats to selection");
		
		lblBeats.Size = new Size(10, 20);
		lblBeats.Location = new Point(50, 45);
		lblBeats.Text = "b";
		
		spinFrames.Size = new Size(40, 20);
		spinFrames.Location = new Point(75, 45);
		new ToolTip().SetToolTip(spinFrames, "Add full frames to selection");
		
		lblFrames.Size = new Size(10, 20);
		lblFrames.Location = new Point(115, 45);
		lblFrames.Text = "f";
		
		gbTrimSel.Size = new Size(115, 70);
		gbTrimSel.Location = new Point(10, 70);
		gbTrimSel.Text = "Trim";
		gbTrimSel.Controls.AddRange(new Control[] {
			lblSelStart,
			lblSelEnd,
			btnSelStartMinus,
			btnSelStartPlus,
			btnSelEndMinus,
			btnSelEndPlus});
			
		lblSelStart.Size = new Size(40, 20);
		lblSelStart.Location = new Point(10, 20);
		lblSelStart.Text = "Start";
		
		lblSelEnd.Size = new Size(40, 20);
		lblSelEnd.Location = new Point(65, 20);
		lblSelEnd.Text = "End";
		
		btnSelStartMinus.Size = new Size(13, 13);
		btnSelStartMinus.Location = new Point(10, 40);
		btnSelStartMinus.Click += new EventHandler(btnSelStartMinus_Click);
		new ToolTip().SetToolTip(btnSelStartMinus, "Extend selection start");
		
		btnSelStartPlus.Size = new Size(13, 13);
		btnSelStartPlus.Location = new Point(23, 40);
		btnSelStartPlus.Click += new EventHandler(btnSelStartMinus_Click);
		new ToolTip().SetToolTip(btnSelStartPlus, "Reduce selection start");
		
		btnSelEndMinus.Size = new Size(13, 13);
		btnSelEndMinus.Location = new Point(65, 40);
		btnSelEndMinus.Click += new EventHandler(btnSelStartMinus_Click);
		new ToolTip().SetToolTip(btnSelEndMinus, "Reduce selection end");
		
		btnSelEndPlus.Size = new Size(13, 13);
		btnSelEndPlus.Location = new Point(78, 40);
		btnSelEndPlus.Click += new EventHandler(btnSelStartMinus_Click);
		new ToolTip().SetToolTip(btnSelEndPlus, "Extend selection end");
		
		btnReset.Size = new Size(13, 13);
		btnReset.Location = new Point(10, 150);
		btnReset.Click += new EventHandler(btnReset_Click);
		new ToolTip().SetToolTip(btnReset, "Reset selection");
		
		lblReset.Size = new Size(40, 15);
		lblReset.Location = new Point(27, 150);
		lblReset.Text = "Reset";
		
		btnZoom.Size = new Size(13, 13);
		btnZoom.Location = new Point(75, 150);
		btnZoom.Click += new EventHandler(btnZoom_Click);
		new ToolTip().SetToolTip(btnZoom, "Zoom to favorite scale");
		
		lblZoom.Size = new Size(40, 15);
		lblZoom.Location = new Point(92, 150);
		lblZoom.Text = "Zoom";
		
		return gbSel;
	}
	
	private GroupBox CreateGroupBoxNav() {
		gbNav.Size = new Size(135, 135);
		gbNav.Location = new Point(10, 330);
		gbNav.Text = "Navigation";
		gbNav.Controls.AddRange(new Control[] {
			lblStep,
			spinStep,
			lblStepFrames,
			btnUp,
			btnStepLeft,
			btnLeft,
			btnHome,
			btnRight,
			btnStepRight,
			btnDown});
			
		lblStep.Size = new Size(35, 20);
		lblStep.Location = new Point(10, 20);
		lblStep.Text = "Step:";
		
		spinStep.Size = new Size(45, 20);
		spinStep.Location = new Point(55, 20);
		new ToolTip().SetToolTip(spinStep, "Define step in frames for step right/left buttons");
		
		lblStepFrames.Size = new Size(20, 20);
		lblStepFrames.Location = new Point(105, 20);
		lblStepFrames.Text = "f";
		
		btnUp.Size = new Size(20, 20);
		btnUp.Location = new Point(55, 50);
		btnUp.Text = "↑";
		btnUp.Click += new EventHandler(btnUp_Click);
		new ToolTip().SetToolTip(btnUp, "Go one track up");
		
		btnStepLeft.Size = new Size(15, 20);
		btnStepLeft.Location = new Point(15, 75);
		btnStepLeft.Text = "<";
		btnStepLeft.Click += new EventHandler(btnStepLeft_Click);
		new ToolTip().SetToolTip(btnStepLeft, "Go one step left");
		
		btnLeft.Size = new Size(20, 20);
		btnLeft.Location = new Point(30, 75);
		btnLeft.Text = "──";
		btnLeft.Click += new EventHandler(btnLeft_Click);
		new ToolTip().SetToolTip(btnLeft, "Go one beat left");
		
		btnHome.Size = new Size(20, 20);
		btnHome.Location = new Point(55, 75);
		btnHome.Text = "H";
		btnHome.Click += new EventHandler(btnHome_Click);
		new ToolTip().SetToolTip(btnHome, "Go to home position");
		
		btnRight.Size = new Size(20, 20);
		btnRight.Location = new Point(80, 75);
		btnRight.Text = "──";
		btnRight.Click += new EventHandler(btnLeft_Click);
		new ToolTip().SetToolTip(btnRight, "Go one beat right");
		
		btnStepRight.Size = new Size(15, 20);
		btnStepRight.Location = new Point(100, 75);
		btnStepRight.Text = ">";
		btnStepRight.Click += new EventHandler(btnStepLeft_Click);
		new ToolTip().SetToolTip(btnStepRight, "Go one step right");
		
		btnDown.Size = new Size(20, 20);
		btnDown.Location = new Point(55, 100);
		btnDown.Text = "↓";
		btnDown.Click += new EventHandler(btnUp_Click);
		new ToolTip().SetToolTip(btnDown, "Go one track down");
		
		return gbNav;
	}
	
	private GroupBox CreateGroupBoxTC() {
		gbTC.Size = new Size(135, 85);
		gbTC.Location = new Point(10, 475);
		gbTC.Text = "Transport Controls";
		gbTC.Controls.AddRange(new Control[] {
			btnPlay,
			btnPause,
			btnStop,
			btnSlower,
			btnFaster});
			
		btnPlay.Size = new Size(60, 20);
		btnPlay.Location = new Point(10, 25);
		btnPlay.Text = "Play";
		btnPlay.Click += new EventHandler(btnPlay_Click);
		new ToolTip().SetToolTip(btnPlay, "Play");
		
		btnPause.Size = new Size(20, 20);
		btnPause.Location = new Point(70, 25);
		btnPause.Text = "||";
		btnPause.Click += new EventHandler(btnPause_Click);
		new ToolTip().SetToolTip(btnPause, "Pause");
		
		btnStop.Size = new Size(35, 20);
		btnStop.Location = new Point(90, 25);
		btnStop.Text = "Stop";
		btnStop.Click += new EventHandler(btnStop_Click);
		new ToolTip().SetToolTip(btnStop, "Stop");
		
		btnSlower.Size = new Size(60, 20);
		btnSlower.Location = new Point(10, 55);
		btnSlower.Text = "<<";
		btnSlower.Click += new EventHandler(btnSlower_Click);
		new ToolTip().SetToolTip(btnSlower, "Slower");
		
		btnFaster.Size = new Size(55, 20);
		btnFaster.Location = new Point(70, 55);
		btnFaster.Text = ">>";
		btnFaster.Click += new EventHandler(btnSlower_Click);
		new ToolTip().SetToolTip(btnFaster, "Faster");
		
		return gbTC;
	}
	
	private void ToggleColor(Control control) {
		foreach (Control childControl in control.Controls) {
			if (Color.Red == color) {
				color = Color.Blue;
			} else {
				color = Color.Red;
			}
		
			childControl.BackColor = color;
		}
	}
	
	//
	// Event handlers BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	void rbChanLeft_Click(object sender, EventArgs e) {
	try {
		if (null == audioTrack) {
			MessageBox.Show("Audio track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		List<AudioEvent> audioEvents = Common.EventsToAudioEvents(Common.TrackEventsToTrackEvents(audioTrack.Events));
		if (audioEvents.Count < 1) {
			MessageBox.Show("Audio event not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		using (UndoBlock undo = new UndoBlock("rbChanLeft_Click")) {
			if (rbChanLeft == sender) {
				audioEvents[0].Channels = ChannelRemapping.DisableRight;
			
				rbChanLeft.Checked = true;
				rbChanBoth.Checked = false;
				rbChanRight.Checked = false;
			} else if (rbChanBoth == sender) {
				audioEvents[0].Channels = ChannelRemapping.None;
				
				rbChanLeft.Checked = false;
				rbChanBoth.Checked = true;
				rbChanRight.Checked = false;
			} else { // rbChanRight == sender
				audioEvents[0].Channels = ChannelRemapping.DisableLeft;
				
				rbChanLeft.Checked = false;
				rbChanBoth.Checked = false;
				rbChanRight.Checked = true;
			}
		}
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void chkMuteAudio_Click(object sender, EventArgs e) {
	try {
		if (null == audioTrack) {
			MessageBox.Show("Audio track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			Common.ToggleCheckBox(chkMuteAudio);
			return;
		}
		
		List<Track> tracks = new List<Track>();
		tracks.Add(audioTrack);
		Common.MuteAllTracks(tracks, chkMuteAudio.Checked);
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void chkMuteClick_Click(object sender, EventArgs e) {
	try {
		if (null == beepTrack) {
			MessageBox.Show("Beep track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			Common.ToggleCheckBox(chkMuteClick);
			return;
		}
		
		List<Track> tracks = new List<Track>();
		tracks.Add(beepTrack);
		Common.MuteAllTracks(tracks, chkMuteClick.Checked);
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void btnSelStartMinus_Click(object sender, EventArgs e) {
	try {
		if (null == audioTrack) {
			MessageBox.Show("Audio track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		TransportControl tc = Common.vegas.Transport;
		
		long offset = (long)spinBeats.Value * frameCount + (long)spinFrames.Value;
		if (sender == btnSelStartMinus || sender == btnSelEndMinus) {
			offset = -offset;
		}
		
		if (sender == btnSelStartMinus || sender == btnSelStartPlus) {
			tc.SelectionStart = tc.SelectionStart + Timecode.FromFrames(offset);
			tc.SelectionLength = tc.SelectionLength - Timecode.FromFrames(offset);
		} else {
			tc.SelectionLength = tc.SelectionLength + Timecode.FromFrames(offset);
		}
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void btnZoom_Click(object sender, EventArgs e) {
	try {
		if (null == audioTrack) {
			MessageBox.Show("Audio track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		TransportControl tc = Common.vegas.Transport;
		
		// save selection
		Selection selection = new Selection(tc.SelectionStart, tc.SelectionLength);
		
		// zoom
		tc.SelectionStart = new Timecode();
		tc.SelectionLength = Timecode.FromFrames(110);
		tc.ZoomSelection();
		
		// restore selection
		tc.SelectionStart = selection.SelectionStart;
		tc.SelectionLength = selection.SelectionLength;
		
		SetCursorPosition(tc.SelectionStart);
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void btnReset_Click(object sender, EventArgs e) {
	try {
		if (null == audioTrack) {
			MessageBox.Show("Audio track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		TransportControl tc = Common.vegas.Transport;
	
		if (new Timecode() == tc.SelectionLength) {
			return;
		}
		
		tc.SelectionLength = new Timecode();
		SetCursorPosition(tc.SelectionStart);
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void btnUp_Click(object sender, EventArgs e) {
	try {
		List<Track> projectTracks = Common.TracksToTracks(Common.vegas.Project.Tracks);
		
		// skip non-audio, empty and beep tracks
		List<Track> filteredAudioTracks = new List<Track>();
		foreach (Track projectTrack in projectTracks) {
			if (!projectTrack.IsAudio()) {
				continue;
			}
			
			List<TrackEvent> trackEvents = Common.TrackEventsToTrackEvents(projectTrack.Events);
			if (trackEvents.Count < 1) {
				continue;
			}
			
			List<TrackEvent> measureStartEvents = Common.FindMeasureStartEvents(trackEvents);
			if (measureStartEvents.Count > 0) {
				continue;
			}
			
			filteredAudioTracks.Add(projectTrack);
		}
		
		if (filteredAudioTracks.Count < 1) {
			MessageBox.Show("Audio track not found",
				Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		int audioTrackIndex = filteredAudioTracks[0].Index;
		
		bool home = (null == e);
		if (!home && filteredAudioTracks.Count > 1 && Common.FindUnmutedTracks(filteredAudioTracks).Count > 0) {
			for (int i = 0; i < filteredAudioTracks.Count; i++) {
				// skip to first unmuted track
				if (filteredAudioTracks[i].Mute) {
					continue;
				}
				
				if (sender == btnUp) { // backwards
					if (i > 0) {
						audioTrackIndex = filteredAudioTracks[i - 1].Index;
					} else {
						audioTrackIndex = filteredAudioTracks[filteredAudioTracks.Count - 1].Index; // wrap around
					}
				} else { // forward
					if (i < filteredAudioTracks.Count - 1) {
						audioTrackIndex = filteredAudioTracks[i + 1].Index;
					} else {
						audioTrackIndex = filteredAudioTracks[0].Index; // wrap around
					}
				}
				
				break;
			}
		}
		
		// validate audio track
		List<AudioEvent> audioEvents = Common.EventsToAudioEvents(Common.TrackEventsToTrackEvents(projectTracks[audioTrackIndex].Events));
		if (audioEvents.Count < 1) {
			MessageBox.Show("Audio event not found",
				Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		int videoTrackIndex = audioTrackIndex - 1;
		int beepTrackIndex = Common.getTrackIndex(TrackType.Beep, audioTrackIndex);
		
		// build rate regions
		List<TrackEvent> beepTrackEvents = Common.TrackEventsToTrackEvents(projectTracks[beepTrackIndex].Events);
		List<TrackEvent> rateRegionStartEvents = Common.FindEventsByRegex(beepTrackEvents, rateRegionStartEventRegex);
		if (rateRegionStartEvents.Count != rates.Length) {
			MessageBox.Show("Inconsistent number (" + rateRegionStartEvents.Count +
				") of rate region start events. " + rates.Length + " expected.",
				Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		rateRegions.Clear();
		
		for (int i = 0; i < rateRegionStartEvents.Count; i++) {
			Timecode start = rateRegionStartEvents[i].Start;
			Timecode end;
			List<TrackEvent> regionEvents;
			double rate = rates[i];
			
			Selection selection;
			
			if (i < rateRegionStartEvents.Count - 1) {
				end = rateRegionStartEvents[i + 1].Start;
				selection = new Selection(rateRegionStartEvents[i].Start,
					rateRegionStartEvents[i + 1].Start - rateRegionStartEvents[i].Start);
			} else {
				end = beepTrackEvents[beepTrackEvents.Count - 1].Start + rateRegionStartEvents[0].Start;
				selection = new Selection(rateRegionStartEvents[i].Start,
					beepTrackEvents[beepTrackEvents.Count - 1].Start - rateRegionStartEvents[i].Start);
			}
			
			regionEvents = Common.FindEventsBySelection(projectTracks[beepTrackIndex], selection.Normalize());
			if (!(i < rateRegionStartEvents.Count - 1)) {
				regionEvents.Add(beepTrackEvents[beepTrackEvents.Count - 1]);
			}
		
			RateRegion rateRegion = new RateRegion(start, end, regionEvents, rate);
			rateRegions.Add(rateRegion);
		}
		
		// validate rate regions
		for (int i = 0; i < rateRegions.Count - 1; i++) {
			if (rateRegions[i].RegionEvents.Count != rateRegions[i + 1].RegionEvents.Count) {
				MessageBox.Show("Number of events differs across rate regions",
					Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}
		
		// read in track BPM
		string BPMEventFullName = Common.getFullName(Common.getTakeNames(beepTrackEvents[0]));
		try {
			trackBPM = Convert.ToDouble(BPMRegex.Match(BPMEventFullName).Groups[1].Value);
		} catch (Exception ex) {
			MessageBox.Show("Can not read in track BPM: " + ex.Message,
				Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		// set cursor position
		if (prevRateRegions.Count > 0) {
			TransportControl tc = Common.vegas.Transport;
			
			// figure target rate region
			RateRegion srcRateRegion = FindRateRegion(prevRateRegions, tc.CursorPosition);
			int srcRateRegionIndex = prevRateRegions.IndexOf(srcRateRegion);
			RateRegion tarRateRegion = rateRegions[srcRateRegionIndex];
			
			// figure target region event
			TrackEvent srcRegionEvent = FindRegionEvent(srcRateRegion, tc.CursorPosition, false, true);
			
			Timecode tarOffset = null;
			int srcRegionEventIndex = srcRateRegion.RegionEvents.IndexOf(srcRegionEvent);
			if (srcRegionEventIndex > tarRateRegion.RegionEvents.Count - 1) {
				srcRegionEventIndex = tarRateRegion.RegionEvents.Count - 1;
				tarOffset = new Timecode();
			}
			
			TrackEvent tarRegionEvent = tarRateRegion.RegionEvents[srcRegionEventIndex];
			
			// figure target offset
			Timecode srcOffset = tc.CursorPosition - srcRegionEvent.Start;
			double scaleFactor = prevTrackBPM / trackBPM;
			if (null == tarOffset) {
				tarOffset = Timecode.FromNanos((int)Math.Round(srcOffset.Nanos * scaleFactor));
			}
			
			// scale selection
			if (tc.SelectionLength != new Timecode()) {
				tc.SelectionLength = Timecode.FromNanos((int)Math.Round(tc.SelectionLength.Nanos * scaleFactor));
			}
			
			SetCursorPosition(tarRegionEvent.Start + tarOffset);
		}
		
		prevTrackBPM = trackBPM;
		prevRateRegions = new List<RateRegion>(rateRegions);
		
		// restore channel mapping
		using (UndoBlock undo = new UndoBlock("btnUp_Click")) {
			audioEvents[0].Channels = ChannelRemapping.None;
		}
		
		// mute all
		Common.MuteAllTracks(projectTracks, true);
		Common.SoloAllTracks(projectTracks, false);
		
		// unmute select tracks
		List<Track> tracksPendingUnmute = new List<Track>();
		tracksPendingUnmute.Add(projectTracks[audioTrackIndex]);
		tracksPendingUnmute.Add(projectTracks[videoTrackIndex]);
		tracksPendingUnmute.Add(projectTracks[beepTrackIndex]);
		Common.MuteAllTracks(tracksPendingUnmute, false);
		
		// save tracks for future reference
		audioTrack = projectTracks[audioTrackIndex];
		beepTrack = projectTracks[beepTrackIndex];
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void btnStepLeft_Click(object sender, EventArgs e) {
	try {
		if (null == beepTrack) {
			MessageBox.Show("Beep track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		Timecode step = Timecode.FromFrames((long)spinStep.Value);
		if (new Timecode() == step) {
			return;
		}
		
		bool forward = (sender == btnStepRight);
		SetCursorPosition(forward ? Common.vegas.Transport.CursorPosition + step : Common.vegas.Transport.CursorPosition - step);
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void btnLeft_Click(object sender, EventArgs e) {
	try {
		if (null == beepTrack) {
			MessageBox.Show("Beep track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		bool forward = (sender == btnRight);
		RateRegion srcRateRegion = FindRateRegion(rateRegions, Common.vegas.Transport.CursorPosition);
		TrackEvent regionEvent = FindRegionEvent(srcRateRegion, Common.vegas.Transport.CursorPosition, forward, false);
		SetCursorPosition(regionEvent.Start);
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void btnHome_Click(object sender, EventArgs e) {
	try {
		if (null == audioTrack) {
			MessageBox.Show("Audio track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		clickCount++;
		if (!timer.Enabled) {
			timer.Start();
		}
		
		if (1 == clickCount) {
			SetCursorPosition(FindRateRegion(rateRegions, Common.vegas.Transport.CursorPosition).Start);
		} else if (clickCount >= 2) {
			Init();
			btnUp_Click(null, null);
			SetCursorPosition(rateRegions[0].Start);
		}
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void btnPlay_Click(object sender, EventArgs e) {
	try {
		Common.vegas.Transport.Play();
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void btnPause_Click(object sender, EventArgs e) {
	try {
		Common.vegas.Transport.Pause();
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void btnStop_Click(object sender, EventArgs e) {
	try {
		TransportControl tc = Common.vegas.Transport;
		
		if (tc.IsPlaying) {
			tc.Stop();
		} else {
			// invert selection
			if (tc.SelectionLength != new Timecode()) {
				Selection selection = new Selection(tc.SelectionStart + tc.SelectionLength, new Timecode() - tc.SelectionLength);
				tc.SelectionStart = selection.SelectionStart;
				tc.SelectionLength = selection.SelectionLength;
				SetCursorPosition(tc.SelectionStart);
			}
		}
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void btnSlower_Click(object sender, EventArgs e) {
	try {
		if (null == beepTrack) {
			MessageBox.Show("Beep track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		TransportControl tc = Common.vegas.Transport;
		
		RateRegion srcRateRegion = FindRateRegion(rateRegions, tc.CursorPosition);
		int srcRateRegionIndex = rateRegions.IndexOf(srcRateRegion);
		RateRegion tarRateRegion = null;
		Timecode offset = null;
		
		bool faster = (sender == btnFaster);
		if (faster) {
			if (srcRateRegionIndex < rateRegions.Count - 1) {
				tarRateRegion = rateRegions[srcRateRegionIndex + 1];
				offset = srcRateRegion.Offset + srcRateRegion.Offset;
				
				if (tc.SelectionLength != new Timecode()) {
					tc.SelectionLength = tc.SelectionLength + tc.SelectionLength;
				}
			}
		} else {
			if (srcRateRegionIndex > 0) {
				tarRateRegion = rateRegions[srcRateRegionIndex - 1];
				offset = Timecode.FromNanos((int)Math.Round(srcRateRegion.Offset.Nanos / 2.0));
				
				if (tc.SelectionLength != new Timecode()) {
					tc.SelectionLength = Timecode.FromNanos((int)Math.Round(tc.SelectionLength.Nanos / 2.0));
				}
			}
		}
		
		if (null != tarRateRegion && null != offset) {
			SetCursorPosition(tarRateRegion.Start + offset);
		}
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	void timer_Tick(object sender, EventArgs e) {
	try {
		timer.Stop();
		clickCount = 0;
	} catch (Exception ex) {
		MessageBox.Show(ex.Message, Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	}
	
	//
	// Event handlers END
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	private void SetCursorPosition(Timecode position) {
		// figure beat size in frames
		RateRegion rateRegion = FindRateRegion(rateRegions, position);
		TrackEvent regionEvent = FindRegionEvent(rateRegion, position, true, false);
		int regionEventIndex = rateRegion.RegionEvents.IndexOf(regionEvent);
		
		TrackEvent prevRegionEvent;
		if (0 == regionEventIndex) {
			regionEvent = rateRegion.RegionEvents[rateRegion.RegionEvents.Count - 1];
			prevRegionEvent = rateRegion.RegionEvents[rateRegion.RegionEvents.Count - 2];
		} else {
			prevRegionEvent = rateRegion.RegionEvents[regionEventIndex - 1];
		}
		
		frameCount = (regionEvent.Start - prevRegionEvent.Start).FrameCount;
		spinFrames.Maximum = frameCount;
		
		lblFrameCount.Text = "1 beat = " + frameCount + " frames";
		
		// set cursor position preserving selection
		TransportControl tc = Common.vegas.Transport;
		
		if (new Timecode() == tc.SelectionLength) {
			tc.CursorPosition = position;
		} else {
			tc.SelectionStart = position;
		}
		
		tc.ViewCursor(false);
		
		// ensure continuous playback
		if (tc.IsPlaying) {
			tc.Play();
		}
	}
	
	private RateRegion FindRateRegion(List<RateRegion> rateRegions, Timecode position) {
		foreach (RateRegion rateRegion in rateRegions) {
			if (rateRegion.BelongsTo(position)) {
				rateRegion.Offset = position - rateRegion.Start;
				return rateRegion;
			}
		}
		
		if (position < rateRegions[0].Start) {
			rateRegions[0].Offset = new Timecode();
			return rateRegions[0];
		}
		
		rateRegions[rateRegions.Count - 1].Offset = rateRegions[rateRegions.Count - 1].End - rateRegions[rateRegions.Count - 1].Start;
		return rateRegions[rateRegions.Count - 1];
	}
	
	private TrackEvent FindRegionEvent(RateRegion rateRegion, Timecode position, bool forward, bool btnUp_Click_mode) {
		if (forward) {
			for (int i = 0; i < rateRegion.RegionEvents.Count - 1; i++) {
				if (position >= rateRegion.RegionEvents[i].Start && position < rateRegion.RegionEvents[i + 1].Start) {
					return rateRegion.RegionEvents[i + 1];
				}
			}
			
			return rateRegion.RegionEvents[0];
		} else {
			for (int i = rateRegion.RegionEvents.Count - 1; i > 0; i--) {
				if (btnUp_Click_mode) {
					if (position >= rateRegion.RegionEvents[i - 1].Start && position < rateRegion.RegionEvents[i].Start) {
						return rateRegion.RegionEvents[i - 1];
					}
				} else {
					if (position > rateRegion.RegionEvents[i - 1].Start && position <= rateRegion.RegionEvents[i].Start) {
						return rateRegion.RegionEvents[i - 1];
					}
				}
			}
			
			return rateRegion.RegionEvents[rateRegion.RegionEvents.Count - 1];
		}
	}
	
}

public class RateRegion {
	private Timecode start;
	private Timecode end;
	private List<TrackEvent> regionEvents;
	private double rate;
	
	private Timecode offset = null;

	public RateRegion(Timecode start, Timecode end, List<TrackEvent> regionEvents, double rate) {
		this.start = start;
		this.end = end;
		this.regionEvents = regionEvents;
		this.rate = rate;
	}
	
	public Timecode Start {
		get {
			return start;
		}
	}
	
	public Timecode End {
		get {
			return end;
		}
	}
	
	public List<TrackEvent> RegionEvents {
		get {
			return regionEvents;
		}
	}
	
	public double Rate {
		get {
			return rate;
		}
	}
	
	public Timecode Offset {
		get {
			return offset;
		}
		set {
			this.offset = value;
		}
	}
	
	public bool BelongsTo(Timecode position) {
		return (position >= start && position < end);
	}
	
	public override string ToString() {
		return "{start=" + start + ", end=" + end + "}\r\n" +
		"{rate=" + rate + ", offset=" + offset + "}\r\n" +
		Common.TrackEventsToString(regionEvents);
	}
	
}

public class NavigateControlTest : Form {
	private NavigateControl navControl = new NavigateControl();
	
	public NavigateControlTest() {
		Controls.Add(navControl);
		Size = new Size(165, 600);
	}
	
	public static void Main() {
		Application.Run(new NavigateControlTest());
	}
	
}

public class MyRadioButton : RadioButton {
	protected override bool ShowFocusCues {
		get {
			return false;
		}
	}
}

public class MyButton: Button {
	protected override bool ShowFocusCues {
		get {
			return false;
		}
	}
}

// http://connect.microsoft.com/VisualStudio/feedback/details/216189/numericupdown-use-of-mouse-wheel-may-result-in-different-increment
/// <summary>
/// A NumericUpDown class that handles mouse wheel scrolling correctly
/// </summary>
public class MyNumericUpDown : NumericUpDown
{
	protected override void OnMouseWheel(MouseEventArgs e)
	{
		// Change the value based on the number of wheel clicks.

		// NOTE: This overrides a bug in NumericUpDown where the value
		// change is based on the mouse wheel scrolling setting in Control
		// Panel.
		//
		decimal val = Value;

		val += ((e.Delta/120) * Increment);
	
		if (val < Minimum)
		{
			val = Minimum;
		}
		else if (val > Maximum)
		{
			val = Maximum;
		}

		Value = val;
	}
}
