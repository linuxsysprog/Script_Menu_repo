// Copyright (C) 2011-2013 Andrey Chislenko
// $Id$
// Navigation and Transport Controls

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
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
			
			navView.DefaultFloatingSize = new Size(165, 560);
			Common.vegas.LoadDockView(navView);
		}
	}

	void HandleMenuPopup(Object sender, EventArgs args) {
		navCmd.Checked = Common.vegas.FindDockView(Common.NAV);
	}
	
	void HandleProjectClosed(Object sender, EventArgs args) {
	}
	
}

public class NavigateControl : UserControl {
	private Color color = Color.Red;
	
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
	
	private ComboBox cbBeats = new ComboBox();
	private Label lblBeats = new Label();
	
	private GroupBox gbTrimSel = new GroupBox();
	
	private Label lblSelStart = new Label();
	private NumericUpDown spinSelStart = new NumericUpDown();
	
	private Label lblSelEnd = new Label();
	private NumericUpDown spinSelEnd = new NumericUpDown();
	
	private CheckBox chkCountIn = new CheckBox();
	
	// nav group box
	private GroupBox gbNav = new GroupBox();
	
	private Button btnUp = new Button();
	private Button btnLeft = new Button();
	private Button btnHome = new Button();
	private Button btnRight = new Button();
	private Button btnDown = new Button();
	
	// TC group box
	private GroupBox gbTC = new GroupBox();
	
	private Button btnPlay = new Button();
	private Button btnPause = new Button();
	private Button btnStop = new Button();
	private Button btnSlower = new Button();
	private Button btnFaster = new Button();
	
	public NavigateControl() {
		Size = new Size(165, 560);
	
		Controls.AddRange(new Control[] {
			CreateGroupBoxAudio(),
			CreateGroupBoxSel(),
			CreateGroupBoxNav(),
			CreateGroupBoxTC()});
			
		InitGroupBoxAudio();
		InitGroupBoxSel();
			
		// ToggleColor(gbTC);
	}
	
	private GroupBox CreateGroupBoxAudio() {
		gbAudio.Size = new Size(135, 130);
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
		rbChanLeft.CheckedChanged += new EventHandler(rbChanLeft_CheckedChanged);
		new ToolTip().SetToolTip(rbChanLeft, "Play left channel only");
		
		rbChanBoth.Size = new Size(50, 20);
		rbChanBoth.Location = new Point(60, 40);
		rbChanBoth.Text = "──";
		rbChanBoth.CheckedChanged += new EventHandler(rbChanBoth_CheckedChanged);
		new ToolTip().SetToolTip(rbChanBoth, "Play both channels");
		
		rbChanRight.Size = new Size(20, 20);
		rbChanRight.Location = new Point(110, 40);
		rbChanRight.CheckedChanged += new EventHandler(rbChanRight_CheckedChanged);
		new ToolTip().SetToolTip(rbChanRight, "Play right channel only");
		
		chkMuteAudio.Size = new Size(100, 20);
		chkMuteAudio.Location = new Point(10, 80);
		chkMuteAudio.Text = "Mute audio";
		chkMuteAudio.Click += new EventHandler(chkMuteAudio_Click);
		new ToolTip().SetToolTip(chkMuteAudio, "Mute audio track");
		
		chkMuteClick.Size = new Size(100, 20);
		chkMuteClick.Location = new Point(10, 100);
		chkMuteClick.Text = "Mute click";
		chkMuteClick.Click += new EventHandler(chkMuteClick_Click);
		new ToolTip().SetToolTip(chkMuteClick, "Mute beep track");
		
		return gbAudio;
	}
	
	private void InitGroupBoxAudio() {
		rbChanBoth.Checked = true;
		chkMuteAudio.Checked = false;
		chkMuteClick.Checked = false;
	}
	
	private GroupBox CreateGroupBoxSel() {
		gbSel.Size = new Size(135, 160);
		gbSel.Location = new Point(10, 150);
		gbSel.Text = "Selection";
		gbSel.Controls.AddRange(new Control[] {
			cbBeats,
			lblBeats,
			gbTrimSel,
			chkCountIn});
			
		cbBeats.Size = new Size(40, 20);
		cbBeats.Location = new Point(10, 20);
		cbBeats.DropDownStyle = ComboBoxStyle.DropDownList;
		cbBeats.SelectedValueChanged += new EventHandler(cbBeats_SelectedValueChanged);
		cbBeats.Items.AddRange(Common.getRange(0, 16));
		new ToolTip().SetToolTip(cbBeats, "Selection length in beats");
		
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
		
		chkCountIn.Size = new Size(100, 20);
		chkCountIn.Location = new Point(10, 130);
		chkCountIn.Text = "Count-in";
		chkCountIn.Click += new EventHandler(chkCountIn_Click);
		new ToolTip().SetToolTip(chkCountIn, "Enable two count-in clicks before playback");
		
		return gbSel;
	}
	
	private void InitGroupBoxSel() {
		cbBeats.SelectedIndex = 0;
		lblBeats.Text = "bts (1b=14f)";
		spinSelStart.Value = 0;
		spinSelEnd.Value = 0;
		chkCountIn.Checked = false;
	}
	
	private GroupBox CreateGroupBoxNav() {
		gbNav.Size = new Size(135, 105);
		gbNav.Location = new Point(10, 320);
		gbNav.Text = "Navigation";
		gbNav.Controls.AddRange(new Control[] {
			btnUp,
			btnLeft,
			btnHome,
			btnRight,
			btnDown});
			
		btnUp.Size = new Size(20, 20);
		btnUp.Location = new Point(55, 20);
		btnUp.Text = "↑";
		btnUp.Click += new EventHandler(btnUp_Click);
		new ToolTip().SetToolTip(btnUp, "Go one track up");
		
		btnLeft.Size = new Size(20, 20);
		btnLeft.Location = new Point(30, 45);
		btnLeft.Text = "←";
		btnLeft.Click += new EventHandler(btnLeft_Click);
		new ToolTip().SetToolTip(btnLeft, "Go one beat left");
		
		btnHome.Size = new Size(20, 20);
		btnHome.Location = new Point(55, 45);
		btnHome.Text = "H";
		btnHome.Click += new EventHandler(btnHome_Click);
		new ToolTip().SetToolTip(btnHome, "Go to home position");
		
		btnRight.Size = new Size(20, 20);
		btnRight.Location = new Point(80, 45);
		btnRight.Text = "→";
		btnRight.Click += new EventHandler(btnRight_Click);
		new ToolTip().SetToolTip(btnRight, "Go one beat right");
		
		btnDown.Size = new Size(20, 20);
		btnDown.Location = new Point(55, 70);
		btnDown.Text = "↓";
		btnDown.Click += new EventHandler(btnUp_Click);
		new ToolTip().SetToolTip(btnDown, "Go one track down");
		
		return gbNav;
	}
	
	private GroupBox CreateGroupBoxTC() {
		gbTC.Size = new Size(135, 85);
		gbTC.Location = new Point(10, 435);
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
		btnFaster.Click += new EventHandler(btnFaster_Click);
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
	
	void rbChanLeft_CheckedChanged(object sender, EventArgs e) {
	}
	
	void rbChanBoth_CheckedChanged(object sender, EventArgs e) {
	}
	
	void rbChanRight_CheckedChanged(object sender, EventArgs e) {
	}
	
	void chkMuteAudio_Click(object sender, EventArgs e) {
	}
	
	void chkMuteClick_Click(object sender, EventArgs e) {
	}
	
	void cbBeats_SelectedValueChanged(object sender, EventArgs e) {
	}
	
	void spinSelStart_ValueChanged(object sender, EventArgs e) {
	}
	
	void chkCountIn_Click(object sender, EventArgs e) {
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
			MessageBox.Show("Track not found",
				Common.NAV, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		int audioTrackIndex = filteredAudioTracks[0].Index;
		
		if (filteredAudioTracks.Count > 1 && Common.FindUnmutedTracks(filteredAudioTracks).Count > 0) {
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
	}
	
	void btnLeft_Click(object sender, EventArgs e) {
	}
	
	void btnHome_Click(object sender, EventArgs e) {
	}
	
	void btnRight_Click(object sender, EventArgs e) {
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
	}
	
	void btnFaster_Click(object sender, EventArgs e) {
	}
	
	//
	// Event handlers END
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
}

public class NavigateControlTest : Form {
	private NavigateControl navControl = new NavigateControl();
	
	public NavigateControlTest() {
		Controls.Add(navControl);
		Size = new Size(165, 560);
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

