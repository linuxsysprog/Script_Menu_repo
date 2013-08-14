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
		navControl.InitGroupBoxAudio();
		navControl.InitGroupBoxSel();
		
		navControl.audioTrack = null;
		navControl.beepTrack = null;
	}
	
}

public class NavigateControl : UserControl {
	private Color color = Color.Red;
	private Regex specialBeepRegex = new Regex("1\\.1");
	
	public Track audioTrack = null;
	public Track beepTrack = null;
	
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
	
	private NumericUpDown spinBeats = new NumericUpDown();
	private Label lblBeats = new Label();
	
	private GroupBox gbTrimSel = new GroupBox();
	
	private Label lblSelStart = new Label();
	private NumericUpDown spinSelStart = new NumericUpDown();
	
	private Label lblSelEnd = new Label();
	private NumericUpDown spinSelEnd = new NumericUpDown();
	
	private CheckBox chkCountIn = new CheckBox();
	private CheckBox chkZoom = new CheckBox();
	
	// nav group box
	private GroupBox gbNav = new GroupBox();
	
	private Label lblStep = new Label();
	private NumericUpDown spinStep = new NumericUpDown();
	private Label lblFrames = new Label();
	
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
		Size = new Size(165, 600);
	
		Controls.AddRange(new Control[] {
			CreateGroupBoxAudio(),
			CreateGroupBoxSel(),
			CreateGroupBoxNav(),
			CreateGroupBoxTC()});
			
		InitGroupBoxAudio();
		InitGroupBoxSel();
			
		// ToggleColor(gbSel);
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
	
	public void InitGroupBoxAudio() {
		rbChanLeft.Checked = false;
		rbChanBoth.Checked = true;
		rbChanRight.Checked = false;
		
		chkMuteAudio.Checked = false;
		chkMuteClick.Checked = false;
	}
	
	private GroupBox CreateGroupBoxSel() {
		gbSel.Size = new Size(135, 180);
		gbSel.Location = new Point(10, 140);
		gbSel.Text = "Selection";
		gbSel.Controls.AddRange(new Control[] {
			spinBeats,
			lblBeats,
			gbTrimSel,
			chkCountIn,
			chkZoom});
			
		spinBeats.Size = new Size(40, 20);
		spinBeats.Location = new Point(10, 20);
		spinBeats.Maximum = 16;
		spinBeats.Minimum = 0;
		spinBeats.ValueChanged += new EventHandler(spinBeats_ValueChanged);
		new ToolTip().SetToolTip(spinBeats, "Selection length in beats");
		
		lblBeats.Size = new Size(70, 20);
		lblBeats.Location = new Point(55, 20);
		
		gbTrimSel.Size = new Size(115, 70);
		gbTrimSel.Location = new Point(10, 50);
		gbTrimSel.Text = "Trim";
		gbTrimSel.Controls.AddRange(new Control[] {
			lblSelStart,
			spinSelStart,
			lblSelEnd,
			spinSelEnd});
			
		lblSelStart.Size = new Size(40, 20);
		lblSelStart.Location = new Point(10, 20);
		lblSelStart.Text = "Start";
		
		spinSelStart.Size = new Size(40, 20);
		spinSelStart.Location = new Point(10, 40);
		spinSelStart.Maximum = 16;
		spinSelStart.Minimum = -16;
		spinSelStart.ValueChanged += new EventHandler(spinSelStart_ValueChanged);
		new ToolTip().SetToolTip(spinSelStart, "Trim selection start N frames left or right");
		
		lblSelEnd.Size = new Size(40, 20);
		lblSelEnd.Location = new Point(65, 20);
		lblSelEnd.Text = "End";
		
		spinSelEnd.Size = new Size(40, 20);
		spinSelEnd.Location = new Point(65, 40);
		spinSelEnd.Maximum = 16;
		spinSelEnd.Minimum = -16;
		spinSelEnd.ValueChanged += new EventHandler(spinSelStart_ValueChanged);
		new ToolTip().SetToolTip(spinSelEnd, "Trim selection end N frames left or right");
		
		chkCountIn.Size = new Size(70, 20);
		chkCountIn.Location = new Point(10, 130);
		chkCountIn.Text = "Count-in";
		chkCountIn.Click += new EventHandler(chkCountIn_Click);
		new ToolTip().SetToolTip(chkCountIn, "Enable two count-in clicks before playback");
		
		chkZoom.Size = new Size(70, 20);
		chkZoom.Location = new Point(10, 150);
		chkZoom.Text = "Zoom";
		new ToolTip().SetToolTip(chkZoom, "Zoom to fav scale");
		
		return gbSel;
	}
	
	public void InitGroupBoxSel() {
		spinBeats.Value = 0;
		lblBeats.Text = "bts (1b=14f)";
		
		spinSelStart.Value = 0;
		spinSelEnd.Value = 0;
		
		chkCountIn.Checked = false;
		chkZoom.Checked = false;
	}
	
	private GroupBox CreateGroupBoxNav() {
		gbNav.Size = new Size(135, 135);
		gbNav.Location = new Point(10, 330);
		gbNav.Text = "Navigation";
		gbNav.Controls.AddRange(new Control[] {
			lblStep,
			spinStep,
			lblFrames,
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
		spinStep.Maximum = 16;
		spinStep.Minimum = 0;
		spinStep.ValueChanged += new EventHandler(spinStep_ValueChanged);
		new ToolTip().SetToolTip(spinStep, "Define step in frames for step right/left buttons");
		
		lblFrames.Size = new Size(20, 20);
		lblFrames.Location = new Point(105, 20);
		lblFrames.Text = "f";
		
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
	}
	
	void chkMuteAudio_Click(object sender, EventArgs e) {
		if (null == audioTrack) {
			MessageBox.Show("Audio track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			Common.ToggleCheckBox(chkMuteAudio);
			return;
		}
		
		List<Track> tracks = new List<Track>();
		tracks.Add(audioTrack);
		Common.MuteAllTracks(tracks, chkMuteAudio.Checked);
	}
	
	void chkMuteClick_Click(object sender, EventArgs e) {
		if (null == beepTrack) {
			MessageBox.Show("Beep track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			Common.ToggleCheckBox(chkMuteClick);
			return;
		}
		
		List<Track> tracks = new List<Track>();
		tracks.Add(beepTrack);
		Common.MuteAllTracks(tracks, chkMuteClick.Checked);
	}
	
	void spinBeats_ValueChanged(object sender, EventArgs e) {
	}
	
	void spinSelStart_ValueChanged(object sender, EventArgs e) {
	}
	
	void chkCountIn_Click(object sender, EventArgs e) {
	}
	
	void spinStep_ValueChanged(object sender, EventArgs e) {
	}
	
	void btnUp_Click(object sender, EventArgs e) {
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
		
		int videoTrackIndex = audioTrackIndex - 1;
		int beepTrackIndex = Common.getTrackIndex(TrackType.Beep, audioTrackIndex);
		
		// mute all
		Common.MuteAllTracks(projectTracks, true);
		Common.SoloAllTracks(projectTracks, false);
		
		// unmute select tracks
		List<Track> tracksPendingUnmute = new List<Track>();
		tracksPendingUnmute.Add(projectTracks[audioTrackIndex]);
		tracksPendingUnmute.Add(projectTracks[videoTrackIndex]);
		tracksPendingUnmute.Add(projectTracks[beepTrackIndex]);
		Common.MuteAllTracks(tracksPendingUnmute, false);
		
		// force no channel mapping
		List<AudioEvent> audioEvents = Common.EventsToAudioEvents(Common.TrackEventsToTrackEvents(projectTracks[audioTrackIndex].Events));
		if (audioEvents.Count < 1) {
			MessageBox.Show("Audio event not found, not mapping channels", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
		} else {
			using (UndoBlock undo = new UndoBlock("btnUp_Click")) {
				audioEvents[0].Channels = ChannelRemapping.None;
			}
		}
		
		// save tracks for future reference
		audioTrack = projectTracks[audioTrackIndex];
		beepTrack = projectTracks[beepTrackIndex];
		
		InitGroupBoxAudio();
		InitGroupBoxSel();
	}
	
	void btnStepLeft_Click(object sender, EventArgs e) {
	}
	
	void btnLeft_Click(object sender, EventArgs e) {
		if (null == beepTrack) {
			MessageBox.Show("Beep track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		bool forward = (sender == btnRight);
		TrackEvent nextBeep = FindNextBeep(beepTrack, Common.vegas.Transport.CursorPosition, forward);
		if (null == nextBeep) {
			MessageBox.Show("Could not find next beep", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		SetCursorPosition(nextBeep.Start);
	}
	
	void btnHome_Click(object sender, EventArgs e) {
		btnUp_Click(null, null);
		if (null == beepTrack) {
			return;
		}
		
		Common.vegas.Transport.CursorPosition = new Timecode();
		
		TrackEvent nextBeep = FindNextBeep(beepTrack, Common.vegas.Transport.CursorPosition, true);
		if (null == nextBeep) {
			MessageBox.Show("Could not find next beep", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		SetCursorPosition(nextBeep.Start);
	}
	
	void btnPlay_Click(object sender, EventArgs e) {
		Common.vegas.Transport.Play();
	}
	
	void btnPause_Click(object sender, EventArgs e) {
		Common.vegas.Transport.Pause();
	}
	
	void btnStop_Click(object sender, EventArgs e) {
		Common.vegas.Transport.Stop();
	}
	
	void btnSlower_Click(object sender, EventArgs e) {
		if (null == beepTrack) {
			MessageBox.Show("Beep track not found", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		bool forward = (sender == btnFaster);
		Timecode nextRate = FindNextRate(beepTrack, Common.vegas.Transport.CursorPosition, forward);
		if (null == nextRate) {
			MessageBox.Show("Could not find next rate", Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		SetCursorPosition(nextRate);
	}
	
	//
	// Event handlers END
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	private void SetCursorPosition(Timecode position) {
		TransportControl tc = Common.vegas.Transport;
		
		if (chkZoom.Checked) {
			tc.SelectionStart = new Timecode();
			tc.SelectionLength = Timecode.FromFrames(110);
			tc.ZoomSelection();
			tc.SelectionLength = new Timecode();
		}

		tc.CursorPosition = position;
		tc.ViewCursor(true);
	}
	
	private TrackEvent FindRightmostSpecialBeep(Track beepTrack, List<TrackEvent> events) {
		return Common.FindEventLeft(beepTrack, events[events.Count - 1].Start + Timecode.FromFrames(1), specialBeepRegex);
	}
	
	private TrackEvent FindNextBeep(Track beepTrack, Timecode position, bool forward) {
		List<TrackEvent> events = Common.TrackEventsToTrackEvents(beepTrack.Events);
		if (events.Count < 1) {
			return null;
		}
		
		if (forward) {
			List<TrackEvent> currentEvents = Common.FindEventsByPosition(beepTrack, position);
			if (currentEvents.Count > 0) {
				position += Timecode.FromFrames(1);
			}
		}
		
		TrackEvent eventLeft = Common.FindEventLeft(beepTrack, position, null);
		TrackEvent eventRight = Common.FindEventRight(beepTrack, position, null);
		
		TrackEvent specialBeepLeft = Common.FindEventLeft(beepTrack, position, specialBeepRegex);
		TrackEvent specialBeepRight = Common.FindEventRight(beepTrack, position, specialBeepRegex);
		
		if (forward) {
			if (eventRight == specialBeepRight && null != specialBeepLeft) {
				return specialBeepLeft;
			}
			
			return eventRight;
		}
		
		// backwards
		if (null == eventRight) {
			return events[events.Count - 1];
		}
		
		if (eventRight == specialBeepRight) {
			TrackEvent @event = Common.FindEventRight(beepTrack, eventRight.Start + Timecode.FromFrames(1), specialBeepRegex);
			return (null == @event) ? events[events.Count - 1] : Common.FindEventLeft(beepTrack, @event.Start, null);
		}
		
		return eventLeft;
	}
	
	private Timecode FindNextRate(Track beepTrack, Timecode position, bool forward) {
		Common.vegas.DebugClear();
		List<TrackEvent> events = Common.TrackEventsToTrackEvents(beepTrack.Events);
		if (events.Count < 1) {
			return null;
		}
		
		Timecode unchangedPosition = position;
		
		if (forward) {
			List<TrackEvent> currentEvents = Common.FindEventsByPosition(beepTrack, position);
			if (currentEvents.Count > 0) {
				position += Timecode.FromFrames(1);
				Common.vegas.DebugOut("position = " + position);
			}
		}
		
		TrackEvent specialBeepLeft = Common.FindEventLeft(beepTrack, position, specialBeepRegex);
		TrackEvent specialBeepRight = Common.FindEventRight(beepTrack, position, specialBeepRegex);
		
		Timecode srcOrigin = GetSrcOrigin(beepTrack, specialBeepLeft, events);
		Common.vegas.DebugOut("srcOrigin = " + srcOrigin);
		
		Timecode srcOffset = GetSrcOffset(beepTrack, unchangedPosition, specialBeepLeft, events, srcOrigin);
		Common.vegas.DebugOut("srcOffset = " + srcOffset);
		
		Timecode tarOrigin = GetTarOrigin(beepTrack, forward, events, specialBeepLeft, specialBeepRight);
		Common.vegas.DebugOut("tarOrigin = " + tarOrigin);
		
		Timecode tarOffset = GetTarOffset(beepTrack, forward, specialBeepLeft, specialBeepRight, srcOffset);
		Common.vegas.DebugOut("tarOffset = " + tarOffset);
		
		return tarOrigin + tarOffset;
	}
	
	private Timecode GetSrcOrigin(Track beepTrack, TrackEvent specialBeepLeft, List<TrackEvent> events) {
		return (null == specialBeepLeft) ? FindRightmostSpecialBeep(beepTrack, events).Start : specialBeepLeft.Start;
	}
	
	private Timecode GetSrcOffset(Track beepTrack, Timecode unchangedPosition, TrackEvent specialBeepLeft,
		List<TrackEvent> events, Timecode srcOrigin) {
		
		if (null == specialBeepLeft) {
			return events[events.Count - 1].Start - FindRightmostSpecialBeep(beepTrack, events).Start + unchangedPosition;
		}
		
		return unchangedPosition - srcOrigin;
	}
	
	private Timecode GetTarOrigin(Track beepTrack, bool forward, List<TrackEvent> events,
		TrackEvent specialBeepLeft, TrackEvent specialBeepRight) {
		
		if (forward) {
			return (null == specialBeepRight) ? events[0].Start : specialBeepRight.Start;
		}
		
		TrackEvent rightmostSpecial = FindRightmostSpecialBeep(beepTrack, events);
		
		if (null == specialBeepLeft) {
			return Common.FindEventLeft(beepTrack, rightmostSpecial.Start, specialBeepRegex).Start;
		}
		
		TrackEvent @event = Common.FindEventLeft(beepTrack, specialBeepLeft.Start, specialBeepRegex);
		return (null == @event) ? rightmostSpecial.Start : @event.Start;
	}
	
	private Timecode GetTarOffset(Track beepTrack,
		bool forward,
		TrackEvent specialBeepLeft,
		TrackEvent specialBeepRight,
		Timecode srcOffset) {
		
		if (forward) {
			if (null == specialBeepRight || null == specialBeepLeft) {
				return Timecode.FromNanos((int)Math.Round(srcOffset.Nanos / 4.0));
			}
			
			return srcOffset + srcOffset;
		}
		
		if (null == specialBeepLeft) {
			return Timecode.FromNanos((int)Math.Round(srcOffset.Nanos / 2.0));
		}
		
		TrackEvent @event = Common.FindEventLeft(beepTrack, specialBeepLeft.Start, specialBeepRegex);
		if (null == @event) {
			return srcOffset + srcOffset + srcOffset + srcOffset;
		}
		
		return Timecode.FromNanos((int)Math.Round(srcOffset.Nanos / 2.0));
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

