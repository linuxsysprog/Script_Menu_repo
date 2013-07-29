// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Monitor current selection and calculate tempo
// Added "Mute/Solo Audio/Video Tracks" feature

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Sony.Vegas;
using AddRulerNamespace;

public class CalcTempo : ICustomCommandModule {
	private CustomCommand calcTempoCmd = new CustomCommand(CommandCategory.View, "CalcTempoCmd");
	private CalcTempoControl calcTempoControl;
	
	public void InitializeModule(Vegas vegas) {
		Common.vegas = vegas;
	}

	public ICollection GetCustomCommands() {
		calcTempoCmd.DisplayName = Common.CALC_TEMPO + " View";
		calcTempoCmd.IconFile = Common.vegas.InstallationDirectory +
			"\\Application Extensions\\CalcTempo.cs.png";
		
		// subscribe to events
		calcTempoCmd.Invoked += HandleInvoked;
		calcTempoCmd.MenuPopup += HandleMenuPopup;
		Common.vegas.MarkersChanged += HandleMarkersChanged;
		
		return new CustomCommand[] { calcTempoCmd };
	}

	void HandleInvoked(Object sender, EventArgs args) {
		if (!Common.vegas.ActivateDockView(Common.CALC_TEMPO)) {
			DockableControl calcTempoView = new DockableControl(Common.CALC_TEMPO);
			
			calcTempoControl = new CalcTempoControl();
			calcTempoView.Controls.Add(calcTempoControl);
			
			calcTempoView.DefaultFloatingSize = new Size(465, 370);
			Common.vegas.LoadDockView(calcTempoView);
		}
	}

	void HandleMenuPopup(Object sender, EventArgs args) {
		calcTempoCmd.Checked = Common.vegas.FindDockView(Common.CALC_TEMPO);
	}
	
	void HandleMarkersChanged(Object sender, EventArgs args) {
		calcTempoControl.updateTempo();
	}
	
}

public class CalcTempoControl : UserControl {
	const string PLAYING_TRACK = "Playing Track:";
	const string PLAYING_TRACKS = "Playing Tracks:";
	const string UNDO_STRING = "Mute/Solo Audio Tracks";
	const string UNDO_VSTRING = "Mute/Solo Video Tracks";
	const string NO_TRACKS = "No audio tracks found";
	const string NO_VTRACKS = "No video tracks found";

	private GroupBox gbCalcTempo = new GroupBox();
	private Label lblTempo = new Label();
	public TextBox txtTempo = new TextBox();
	public CheckBox chkDoubleTime = new CheckBox();
	public CheckBox chkMonitorRegion = new CheckBox();
	private Button btnCalcTempo = new Button();

	private GroupBox gbMuteTracks= new GroupBox();
	public CheckBox chkMuteAll= new CheckBox();
	public CheckBox chkSoloAll= new CheckBox();
	private Label lblPlayingTrack = new Label();
	private Label lblPlayingTrackIndex = new Label();
	private Button btnPlayPrev = new Button();
	private Button btnPlayNext = new Button();
	private CheckBox chkLockWithVideo = new CheckBox();
	private CheckBox chkSplitAudio = new CheckBox();

	private GroupBox gbVMuteTracks= new GroupBox();
	public CheckBox chkVMuteAll= new CheckBox();
	public CheckBox chkVSoloAll= new CheckBox();
	private Label lblVPlayingTrack = new Label();
	private Label lblVLPlayingTrack = new Label();
	private Label lblVRPlayingTrack = new Label();
	private Button btnLPlayPrev = new Button();
	private Button btnLPlayNext = new Button();
	private Button btnRPlayPrev = new Button();
	private Button btnRPlayNext = new Button();
	public CheckBox chkLockLeftRight = new CheckBox();

	private GroupBox gbTC= new GroupBox();
	private Label lblChunk = new Label();
	private ComboBox cbChunk = new ComboBox();
	private Button btnLTrim = new Button();
	private Button btnRTrim = new Button();
	private Button btnPrevBeat = new Button();
	private Button btnNextBeat = new Button();
	private Button btnPlay = new Button();
	private Button btnPause = new Button();
	private Button btnStop = new Button();
	private Button btnRate = new Button();
	
	public CalcTempoControl() {
		gbCalcTempo.Size = new Size(135, 220);
		gbCalcTempo.Location = new Point(310, 10);
		gbCalcTempo.Text = "Calculate Tempo";
		gbCalcTempo.Controls.AddRange(new Control[] {
			lblTempo,
			txtTempo,
			chkDoubleTime,
			chkMonitorRegion,
			btnCalcTempo});
			
		lblTempo.Size = new Size(50, 20);
		lblTempo.Location = new Point(10, 20);
		lblTempo.Text = "&Tempo:";
		
		txtTempo.Size = new Size(60, 20);
		txtTempo.Location = new Point(60, 20);
		txtTempo.ReadOnly = true;
		
		chkDoubleTime.Size = new Size(100, 20);
		chkDoubleTime.Location = new Point(10, 60);
		chkDoubleTime.Text = "&Double Time";
		chkDoubleTime.Click += new EventHandler(chkDoubleTime_Click);
		
		chkMonitorRegion.Size = new Size(120, 50);
		chkMonitorRegion.Location = new Point(10, 70);
		chkMonitorRegion.Text = "&Monitor Regions";
		chkMonitorRegion.Click += new EventHandler(chkMonitorRegion_Click);
		
		btnCalcTempo.Location = new Point(30, 130);
		btnCalcTempo.Text = "&Calculate";
		btnCalcTempo.Click += new EventHandler(btnCalcTempo_Click);
		
		gbMuteTracks.Size = new Size(135, 220);
		gbMuteTracks.Location = new Point(10, 10);
		gbMuteTracks.Text = "Mute/Solo Au Tracks";
		gbMuteTracks.Controls.AddRange(new Control[] {
			chkMuteAll,
			chkSoloAll,
			lblPlayingTrack,
			lblPlayingTrackIndex,
			btnPlayPrev,
			btnPlayNext,
			chkLockWithVideo,
			chkSplitAudio});
			
		chkMuteAll.Size = new Size(100, 20);
		chkMuteAll.Location = new Point(10, 20);
		chkMuteAll.Text = "&Mute All";
		chkMuteAll.Click += new EventHandler(chkMuteAll_Click);
		
		chkSoloAll.Size = new Size(100, 20);
		chkSoloAll.Location = new Point(10, 40);
		chkSoloAll.Text = "&Solo All";
		chkSoloAll.Click += new EventHandler(chkSoloAll_Click);
		
		lblPlayingTrack.Size = new Size(100, 15);
		lblPlayingTrack.Location = new Point(10, 70);
		
		lblPlayingTrackIndex.Size = new Size(20, 15);
		lblPlayingTrackIndex.Location = new Point(59, 87);
		
		btnPlayPrev.Location = new Point(30, 105);
		btnPlayPrev.Text = "Play &Prev";
		btnPlayPrev.Click += new EventHandler(btnPlayNext_Click);
		
		btnPlayNext.Location = new Point(30, 135);
		btnPlayNext.Text = "Play &Next";
		btnPlayNext.Click += new EventHandler(btnPlayNext_Click);
		
		chkLockWithVideo.Size = new Size(100, 20);
		chkLockWithVideo.Location = new Point(10, 165);
		chkLockWithVideo.Text = "Lock w/ &Video";
		chkLockWithVideo.Click += new EventHandler(chkLockWithVideo_Click);
		
		chkSplitAudio.Size = new Size(100, 20);
		chkSplitAudio.Location = new Point(10, 185);
		chkSplitAudio.Text = "Split &Audio";
		chkSplitAudio.Click += new EventHandler(chkSplitAudio_Click);
		
		gbVMuteTracks.Size = new Size(135, 220);
		gbVMuteTracks.Location = new Point(160, 10);
		gbVMuteTracks.Text = "Mute/Solo Vid Tracks";
		gbVMuteTracks.Controls.AddRange(new Control[] {
			chkVMuteAll,
			chkVSoloAll,
			lblVPlayingTrack,
			lblVLPlayingTrack,
			lblVRPlayingTrack,
			btnLPlayPrev,
			btnLPlayNext,
			btnRPlayPrev,
			btnRPlayNext,
			chkLockLeftRight});
			
		chkVMuteAll.Size = new Size(100, 20);
		chkVMuteAll.Location = new Point(10, 20);
		chkVMuteAll.Text = "&Mute All";
		chkVMuteAll.Click += new EventHandler(chkVMuteAll_Click);
		
		chkVSoloAll.Size = new Size(100, 20);
		chkVSoloAll.Location = new Point(10, 40);
		chkVSoloAll.Text = "&Solo All";
		chkVSoloAll.Click += new EventHandler(chkVSoloAll_Click);
		
		lblVPlayingTrack.Size = new Size(100, 15);
		lblVPlayingTrack.Location = new Point(10, 70);
		
		lblVLPlayingTrack.Size = new Size(20, 15);
		lblVLPlayingTrack.Location = new Point(39, 87);
		
		lblVRPlayingTrack.Size = new Size(20, 15);
		lblVRPlayingTrack.Location = new Point(79, 87);
		
		// btnLPlayPrev.Size = new Size(75, 23);
		btnLPlayPrev.Size = new Size(35, 23);
		btnLPlayPrev.Location = new Point(30, 105);
		btnLPlayPrev.Text = "Prev";
		btnLPlayPrev.Click += new EventHandler(btnVPlayNext_Click);
		btnLPlayPrev.Enabled = false;
		
		btnLPlayNext.Size = new Size(35, 23);
		btnLPlayNext.Location = new Point(30, 135);
		btnLPlayNext.Text = "Next";
		btnLPlayNext.Click += new EventHandler(btnVPlayNext_Click);
		btnLPlayNext.Enabled = false;
		
		btnRPlayPrev.Size = new Size(35, 23);
		btnRPlayPrev.Location = new Point(70, 105);
		btnRPlayPrev.Text = "Prev";
		btnRPlayPrev.Click += new EventHandler(btnVPlayNext_Click);
		btnRPlayPrev.Enabled = false;
		
		btnRPlayNext.Size = new Size(35, 23);
		btnRPlayNext.Location = new Point(70, 135);
		btnRPlayNext.Text = "Next";
		btnRPlayNext.Click += new EventHandler(btnVPlayNext_Click);
		btnRPlayNext.Enabled = false;
		
		chkLockLeftRight.Size = new Size(100, 20);
		chkLockLeftRight.Location = new Point(10, 165);
		chkLockLeftRight.Text = "&Lock L and R";
		chkLockLeftRight.Click += new EventHandler(chkLockLeftRight_Click);
		
		gbTC.Size = new Size(435, 95);
		gbTC.Location = new Point(10, 240);
		gbTC.Text = "Transport Controls";
		gbTC.Controls.AddRange(new Control[] {
			lblChunk,
			cbChunk,
			btnLTrim,
			btnRTrim,
			btnPrevBeat,
			btnNextBeat,
			btnPlay,
			btnPause,
			btnStop,
			btnRate});
			

		lblChunk.Size = new Size(75, 20);
		lblChunk.Location = new Point(10, 20);
		lblChunk.Text = "Chunk:";
		
		cbChunk.Size = new Size(40, 20);
		cbChunk.Location = new Point(90, 20);
		cbChunk.DropDownStyle = ComboBoxStyle.DropDownList;
		cbChunk.Items.AddRange(Common.getRange(1, 16));
		cbChunk.SelectedIndex = 0;
		
		btnLTrim.Size = new Size(35, 23);
		btnLTrim.Location = new Point(180, 20);
		btnLTrim.Text = "<||";
		btnLTrim.Click += new EventHandler(btnLTrim_Click);
		
		btnRTrim.Size = new Size(35, 23);
		btnRTrim.Location = new Point(220, 20);
		btnRTrim.Text = "||>";
		btnRTrim.Click += new EventHandler(btnRTrim_Click);
		
		btnPrevBeat.Size = new Size(35, 23);
		btnPrevBeat.Location = new Point(330, 20);
		btnPrevBeat.Text = "Prev";
		btnPrevBeat.Click += new EventHandler(btnNextBeat_Click);
		
		btnNextBeat.Size = new Size(35, 23);
		btnNextBeat.Location = new Point(370, 20);
		btnNextBeat.Text = "Next";
		btnNextBeat.Click += new EventHandler(btnNextBeat_Click);
		
		btnPlay.Location = new Point(10, 55);
		btnPlay.Text = "Play";
		btnPlay.Click += new EventHandler(btnPlay_Click);
		
		btnPause.Size = new Size(35, 23);
		btnPause.Location = new Point(90, 55);
		btnPause.Text = "||";
		btnPause.Click += new EventHandler(btnPause_Click);
		
		btnStop.Size = new Size(35, 23);
		btnStop.Location = new Point(130, 55);
		btnStop.Text = "Stop";
		btnStop.Click += new EventHandler(btnStop_Click);
		
		btnRate.Location = new Point(330, 55);
		btnRate.Text = "Rate";
		btnRate.Click += new EventHandler(btnRate_Click);
		
		Size = new Size(1000, 1000);
		Controls.AddRange(new Control[] {
			gbCalcTempo,
			gbMuteTracks,
			gbVMuteTracks,
			gbTC});
			
		Common.vegas.ProjectClosed += HandleProjectClosed;
		Common.vegas.TrackCountChanged += HandleTrackCountChanged;
		InitializeCalcTempoForm();
		InitializeMuteTracksForm();
		InitializeVMuteTracksForm();
	}
	
	private void InitializeCalcTempoForm() {
		txtTempo.Text = "000.0000";
		chkDoubleTime.Checked = false;
		chkMonitorRegion.Checked = false;
	}
	
	private void InitializeMuteTracksForm() {
		chkMuteAll.Checked = false;
		chkSoloAll.Checked = false;
		lblPlayingTrack.Text = PLAYING_TRACK;
		lblPlayingTrackIndex.Text = "";
		chkLockWithVideo.Checked = false;
		chkLockWithVideo.Enabled = false;
		chkSplitAudio.Checked = false;
		chkSplitAudio.Enabled = false;
	}
	
	private void InitializeVMuteTracksForm() {
		chkVMuteAll.Checked = false;
		chkVSoloAll.Checked = false;
		lblVPlayingTrack.Text = PLAYING_TRACKS;
		lblVLPlayingTrack.Text = "";
		lblVRPlayingTrack.Text = "";
		chkLockLeftRight.Checked = false;
		chkLockLeftRight.Enabled = false;
	}
	
	//
	// Event handlers BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	void btnCalcTempo_Click(object sender, EventArgs e) {
		Selection selection = new Selection(Common.vegas.Transport.SelectionStart,
			Common.vegas.Transport.SelectionLength);
		selection.Normalize();
		
		if (selection.SelectionLength == new Timecode()) {
			MessageBox.Show("Selection is zero", Common.CALC_TEMPO, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		txtTempo.Text = "" +
			calcTempo(selection.SelectionLength).ToString("F4");
	}
	
	void chkDoubleTime_Click(object sender, EventArgs e) {
		double n;
		
		try {
			n = Convert.ToDouble(txtTempo.Text);
			if (n == 0) {
				throw new Exception("tempo is zero");
			}
		} catch (Exception ex) {
			return;
		}

		if (chkDoubleTime.Checked) {
			txtTempo.Text = (n * 2).ToString("F4");
		} else {
			txtTempo.Text = (n / 2).ToString("F4");
		}
	}
	
	void chkMonitorRegion_Click(object sender, EventArgs e) {
			updateTempo();
	}
	
	void chkMuteAll_Click(object sender, EventArgs e) {
		List<Track> tracks = Common.AudioTracksToTracks(Audio.FindAudioTracks(Common.vegas.Project));
		
		if (tracks.Count < 1) {
			MessageBox.Show(NO_TRACKS, Common.MUTE_TRACKS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			chkMuteAll.Checked = chkMuteAll.Checked ? false : true;
			return;
		}
		
		using (UndoBlock undo = new UndoBlock(UNDO_STRING)) {
			Common.MuteAllTracks(tracks, chkMuteAll.Checked);
			lblPlayingTrackIndex.Text = "";
		}
	}
	
	void chkVMuteAll_Click(object sender, EventArgs e) {
		List<Track> tracks = Common.VideoTracksToTracks(Video.FindVideoTracks(Common.vegas.Project));
		
		if (tracks.Count < 1) {
			MessageBox.Show(NO_VTRACKS, Common.MUTE_TRACKS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			chkVMuteAll.Checked = chkVMuteAll.Checked ? false : true;
			return;
		}
		
		using (UndoBlock undo = new UndoBlock(UNDO_STRING)) {
			Common.MuteAllTracks(tracks, chkVMuteAll.Checked);
			lblVLPlayingTrack.Text = "";
			lblVRPlayingTrack.Text = "";
		}
	}
	
	void chkSoloAll_Click(object sender, EventArgs e) {
		List<Track> tracks = Common.AudioTracksToTracks(Audio.FindAudioTracks(Common.vegas.Project));
		
		if (tracks.Count < 1) {
			MessageBox.Show(NO_TRACKS, Common.MUTE_TRACKS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			chkSoloAll.Checked = chkSoloAll.Checked ? false : true;
			return;
		}
		
		using (UndoBlock undo = new UndoBlock(UNDO_STRING)) {
			Common.SoloAllTracks(tracks, chkSoloAll.Checked);
			lblPlayingTrackIndex.Text = "";
		}
	}
	
	void chkVSoloAll_Click(object sender, EventArgs e) {
		List<Track> tracks = Common.VideoTracksToTracks(Video.FindVideoTracks(Common.vegas.Project));
		
		if (tracks.Count < 1) {
			MessageBox.Show(NO_VTRACKS, Common.MUTE_TRACKS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			chkVSoloAll.Checked = chkVSoloAll.Checked ? false : true;
			return;
		}
		
		using (UndoBlock undo = new UndoBlock(UNDO_STRING)) {
			Common.SoloAllTracks(tracks, chkVSoloAll.Checked);
			lblVLPlayingTrack.Text = "";
			lblVRPlayingTrack.Text = "";
		}
	}
	
	void btnPlayNext_Click(object sender, EventArgs e) {
		List<Track> projectTracks = Common.TracksToTracks(Common.vegas.Project.Tracks);
		List<Track> unfilteredTracks = Common.AudioTracksToTracks(Audio.FindAudioTracks(Common.vegas.Project));
		
		// only keep tracks matching ^Audio
		// List<Track> tracks = Common.FindTracksByRegex(unfilteredTracks, new Regex("^Audio"));
		List<Track> tracks = new List<Track>();
		foreach (Track unfilteredTrack in unfilteredTracks) {
			List<TrackEvent> trackEvents = Common.TrackEventsToTrackEvents(unfilteredTrack.Events);
			if (trackEvents.Count < 1 || Common.isBPMEvent(trackEvents[0])) {
				continue;
			}
			
			tracks.Add(unfilteredTrack);
		}
		if (tracks.Count < 1) {
			MessageBox.Show("No audio tracks starting with \"Audio\" found",
				Common.MUTE_TRACKS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		using (UndoBlock undo = new UndoBlock(UNDO_STRING)) {
			// unsplit audio
			if (chkSplitAudio.Checked) {
				chkSplitAudio.Checked = false;
				chkSplitAudio_Click(sender, e);
			}
		
			// unsolo soloed tracks if there are any
			Common.SoloAllTracks(tracks, false);
			if (chkSoloAll.Checked) {
				chkSoloAll.Checked = false;
			}
			
			List<Track> unmutedTracks = findUnmutedTracks(tracks);
			
			// zero or more than one unmuted tracks
			if (unmutedTracks.Count == 0 || unmutedTracks.Count > 1) {
				Common.MuteAllTracks(projectTracks, true);
				tracks[0].Mute = false;
				projectTracks[Common.getTrackIndex(TrackType.Beep, tracks[0].Index)].Mute = false;
				projectTracks[tracks[0].Index - 1].Mute = false;
				if (chkMuteAll.Checked) {
					chkMuteAll.Checked = false;
				}
				lblPlayingTrackIndex.Text = "" + tracks[0].DisplayIndex;
				return;
			}
			
			// exactly one unmuted track
			
			// if the unmuted track happens to be the only audio track in the project
			// nothing needs to be done
			if (tracks.Count == 1) {
				lblPlayingTrackIndex.Text = "" + tracks[0].DisplayIndex;
				Common.MuteAllTracks(projectTracks, true);
				tracks[0].Mute = false;
				projectTracks[Common.getTrackIndex(TrackType.Beep, tracks[0].Index)].Mute = false;
				projectTracks[tracks[0].Index - 1].Mute = false;
				return;
			}
			
			for (int i = 0; i < tracks.Count; i++) {
				if (!tracks[i].Mute) {
					Common.MuteAllTracks(projectTracks, true);
					
					if (sender == btnPlayPrev) {
						if (i > 0) {
							tracks[i - 1].Mute = false;
							projectTracks[Common.getTrackIndex(TrackType.Beep, tracks[i - 1].Index)].Mute = false;
							projectTracks[tracks[i - 1].Index - 1].Mute = false;
							lblPlayingTrackIndex.Text = "" + tracks[i - 1].DisplayIndex;
						} else {
							tracks[tracks.Count - 1].Mute = false;
							projectTracks[Common.getTrackIndex(TrackType.Beep, tracks[tracks.Count - 1].Index)].Mute = false;
							projectTracks[tracks[tracks.Count - 1].Index - 1].Mute = false;
							lblPlayingTrackIndex.Text = "" + tracks[tracks.Count - 1].DisplayIndex;
						}
					} else { // btnPlayNext
						if (i < tracks.Count - 1) {
							tracks[i + 1].Mute = false;
							projectTracks[Common.getTrackIndex(TrackType.Beep, tracks[i + 1].Index)].Mute = false;
							projectTracks[tracks[i + 1].Index - 1].Mute = false;
							lblPlayingTrackIndex.Text = "" + tracks[i + 1].DisplayIndex;
						} else {
							tracks[0].Mute = false;
							projectTracks[Common.getTrackIndex(TrackType.Beep, tracks[0].Index)].Mute = false;
							projectTracks[tracks[0].Index - 1].Mute = false;
							lblPlayingTrackIndex.Text = "" + tracks[0].DisplayIndex;
						}
					}
					
					if (chkMuteAll.Checked) {
						chkMuteAll.Checked = false;
					}
					return;
				}
			}
		}
	}
	
	void btnVPlayNext_Click(object sender, EventArgs e) {
		return;
		string label;
		string peerTracklabel;
		Label lbl;
		
		if (sender == btnLPlayPrev || sender == btnLPlayNext) {
			label = "LVideo";
			peerTracklabel = "RVideo";
			lbl = lblVLPlayingTrack;
		} else {
			label = "RVideo";
			peerTracklabel = "LVideo";
			lbl = lblVRPlayingTrack;
		}

		Regex regex = new Regex("^" + label);
		Regex peerTrackRegex = new Regex("^" + peerTracklabel);
		
		List<Track> unfilteredTracks = Common.VideoTracksToTracks(Video.FindVideoTracks(Common.vegas.Project));
		List<Track> unfilteredAudioTracks = Common.AudioTracksToTracks(Audio.FindAudioTracks(Common.vegas.Project));
		
		// only keep tracks matching ^LVideo or ^RVideo
		List<Track> tracks = Common.FindTracksByRegex(unfilteredTracks, regex);
		List<Track> peerTracks = Common.FindTracksByRegex(unfilteredTracks, peerTrackRegex);
		List<Track> peerAudioTracks = Common.FindTracksByRegex(unfilteredAudioTracks, new Regex("^Audio"));

		if (tracks.Count < 1) {
			MessageBox.Show("No video tracks starting with \"" + label + "\" found",
				Common.MUTE_VTRACKS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		using (UndoBlock undo = new UndoBlock(UNDO_STRING)) {
			// unsplit audio
			if (chkSplitAudio.Checked) {
				chkSplitAudio.Checked = false;
				chkSplitAudio_Click(sender, e);
			}
		
			// unsolo soloed tracks if there are any
			Common.SoloAllTracks(tracks, false);
			if (chkLockLeftRight.Checked) {
				Common.SoloAllTracks(peerTracks, false);
			}
			if (chkLockWithVideo.Checked) {
				Common.SoloAllTracks(peerAudioTracks, false);
				if (chkSoloAll.Checked) {
					chkSoloAll.Checked = false;
				}
			}
			if (chkVSoloAll.Checked) {
				chkVSoloAll.Checked = false;
			}
			
			List<Track> unmutedTracks = findUnmutedTracks(tracks);
			
			// zero or more than one unmuted tracks
			if (unmutedTracks.Count == 0 || unmutedTracks.Count > 1) {
				Common.MuteAllTracks(tracks, true);
				if (chkLockLeftRight.Checked) {
					Common.MuteAllTracks(peerTracks, true);
				}
				if (chkLockWithVideo.Checked) {
					Common.MuteAllTracks(peerAudioTracks, true);
					if (chkMuteAll.Checked) {
						chkMuteAll.Checked = false;
					}
				}
				if (chkVMuteAll.Checked) {
					chkVMuteAll.Checked = false;
				}
				tracks[0].Mute = false;
				lbl.Text = "" + tracks[0].DisplayIndex;
				unmutePeerTrack(tracks[0], sender);
				
				if (chkLockWithVideo.Checked) {
					Track peerAudioTrack = findPeerAudioTrack(tracks[0], sender);
					if (peerAudioTrack != null) {
						peerAudioTrack.Mute = false;
						lblPlayingTrackIndex.Text = "" + peerAudioTrack.DisplayIndex;
					}
				}
				
				return;
			}
			
			// exactly one unmuted track
			
			// if the unmuted track happens to be the only (labeled) video track in the project
			// nothing needs to be done
			if (tracks.Count == 1) {
				lbl.Text = "" + tracks[0].DisplayIndex;
				unmutePeerTrack(tracks[0], sender);
				
				if (chkLockWithVideo.Checked) {
					Track peerAudioTrack = findPeerAudioTrack(tracks[0], sender);
					if (peerAudioTrack != null) {
						peerAudioTrack.Mute = false;
						lblPlayingTrackIndex.Text = "" + peerAudioTrack.DisplayIndex;
					}
				}
				
				return;
			}
			
			for (int i = 0; i < tracks.Count; i++) {
				if (!tracks[i].Mute) {
					Common.MuteAllTracks(tracks, true);
					if (chkLockLeftRight.Checked) {
						Common.MuteAllTracks(peerTracks, true);
					}
					if (chkLockWithVideo.Checked) {
						Common.MuteAllTracks(peerAudioTracks, true);
					}
					
					if (sender == btnLPlayPrev || sender == btnRPlayPrev) {
						if (i > 0) {
							tracks[i - 1].Mute = false;
							lbl.Text = "" + tracks[i - 1].DisplayIndex;
							unmutePeerTrack(tracks[i - 1], sender);
							
							if (chkLockWithVideo.Checked) {
								Track peerAudioTrack = findPeerAudioTrack(tracks[i - 1], sender);
								if (peerAudioTrack != null) {
									peerAudioTrack.Mute = false;
									lblPlayingTrackIndex.Text = "" + peerAudioTrack.DisplayIndex;
								}
							}
						} else {
							tracks[tracks.Count - 1].Mute = false;
							lbl.Text = "" + tracks[tracks.Count - 1].DisplayIndex;
							unmutePeerTrack(tracks[tracks.Count - 1], sender);
							
							if (chkLockWithVideo.Checked) {
								Track peerAudioTrack = findPeerAudioTrack(tracks[tracks.Count - 1], sender);
								if (peerAudioTrack != null) {
									peerAudioTrack.Mute = false;
									lblPlayingTrackIndex.Text = "" + peerAudioTrack.DisplayIndex;
								}
							}
						}
					} else { // btnLPlayNext || btnRPlayNext
						if (i < tracks.Count - 1) {
							tracks[i + 1].Mute = false;
							lbl.Text = "" + tracks[i + 1].DisplayIndex;
							unmutePeerTrack(tracks[i + 1], sender);
							
							if (chkLockWithVideo.Checked) {
								Track peerAudioTrack = findPeerAudioTrack(tracks[i + 1], sender);
								if (peerAudioTrack != null) {
									peerAudioTrack.Mute = false;
									lblPlayingTrackIndex.Text = "" + peerAudioTrack.DisplayIndex;
								}
							}
						} else {
							tracks[0].Mute = false;
							lbl.Text = "" + tracks[0].DisplayIndex;
							unmutePeerTrack(tracks[0], sender);
							
							if (chkLockWithVideo.Checked) {
								Track peerAudioTrack = findPeerAudioTrack(tracks[0], sender);
								if (peerAudioTrack != null) {
									peerAudioTrack.Mute = false;
									lblPlayingTrackIndex.Text = "" + peerAudioTrack.DisplayIndex;
								}
							}
						}
					}
					
					if (chkVMuteAll.Checked) {
						chkVMuteAll.Checked = false;
					}
					if (chkLockWithVideo.Checked) {
						if (chkMuteAll.Checked) {
							chkMuteAll.Checked = false;
						}
					}
					return;
				}
			}
		}
	}
	
	void btnLTrim_Click(object sender, EventArgs e) {
	}
	
	void btnRTrim_Click(object sender, EventArgs e) {
	}
	
	void btnNextBeat_Click(object sender, EventArgs e) {
		if ("" == lblPlayingTrackIndex.Text) {
			btnPlayNext.PerformClick();
		}
		
		int playingTrackDisplayIndex = Convert.ToInt32(lblPlayingTrackIndex.Text);

		List<Track> projectTracks = Common.TracksToTracks(Common.vegas.Project.Tracks);
		Track beepTrack = projectTracks[Common.getTrackIndex(TrackType.Beep, playingTrackDisplayIndex - 1)];
		
		TrackEvent nextEvent = FindNextEvent(beepTrack, Common.vegas.Transport.CursorPosition, sender == btnNextBeat);
		if (null == nextEvent) {
			MessageBox.Show("could not find next event", Common.TC, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		SetCursorPosition(nextEvent.Start);
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
	
	void btnRate_Click(object sender, EventArgs e) {
	}
	
	private static TrackEvent FindNextEvent(Track track, Timecode position, bool forward) {
		List<TrackEvent> events = Common.TrackEventsToTrackEvents(track.Events);
		if (events.Count < 1) {
			return null;
		}
		
		List<TrackEvent> currentEvents = Common.FindEventsByPosition(track, position);
		if (currentEvents.Count > 0 && forward) {
			position += Timecode.FromFrames(1);
		}
		
		TrackEvent eventRight = Common.FindEventRight(track, position);
		TrackEvent BPMEventRight = FindBPMEventRight(track, position);
		TrackEvent eventLeft = Common.FindEventLeft(track, position);
		TrackEvent BPMEventLeft = FindBPMEventLeft(track, position);
		
		if (forward) {
			if (eventRight == BPMEventRight && null != BPMEventLeft) {
				return BPMEventLeft;
			}
			
			return eventRight;
		} else {
			if (null == eventRight) {
				return events[events.Count - 1];
			}
			
			if (eventRight == BPMEventRight) {
				TrackEvent @event = FindBPMEventRight(track, eventRight.Start + Timecode.FromFrames(1));
				return null == @event ? events[events.Count - 1] : Common.FindEventLeft(track, @event.Start);
			}
			
			return eventLeft;
		}
	}
	
	private static TrackEvent FindBPMEventRight(Track track, Timecode position) {
		List<TrackEvent> events = Common.TrackEventsToTrackEvents(track.Events);
		
		foreach (TrackEvent @event in events) {
			if (@event.Start >= position && Common.isBPMEvent(@event)) {
				return @event;
			}
		}
			
		return null;
	}
	
	private static TrackEvent FindBPMEventLeft(Track track, Timecode position) {
		List<TrackEvent> events = Common.TrackEventsToTrackEvents(track.Events);
		
		for (int i = events.Count - 1; i >= 0; i--) {
			if (events[i].Start < position && Common.isBPMEvent(events[i])) {
				return events[i];
			}
		}
			
		return null;
	}
	
	private void SetCursorPosition(Timecode pos) {
		TransportControl tc = Common.vegas.Transport;
		tc.CursorPosition = pos;
		tc.ViewCursor(true);
	}
	
	private void unmutePeerTrack(Track track, object sender) {
		if (!chkLockLeftRight.Checked) {
			return;
		}
	
		Regex peerTrackRegex;
		int peerTrackIndex;
		Label lbl;
		if (sender == btnLPlayPrev || sender == btnLPlayNext) {
			peerTrackRegex = new Regex("^RVideo");
			peerTrackIndex = track.Index + 1;
			lbl = lblVRPlayingTrack;
		} else { // btnRPlayPrev || btnRPlayNext
			peerTrackRegex = new Regex("^LVideo");
			peerTrackIndex = track.Index - 1;
			lbl = lblVLPlayingTrack;
		}
		
		if (peerTrackIndex < Common.vegas.Project.Tracks.Count &&
				peerTrackIndex > -1) {
			Track peerTrack = Common.vegas.Project.Tracks[peerTrackIndex];
			if (peerTrack.IsVideo() &&
					peerTrackRegex.Match(peerTrack.Name == null ? "" : peerTrack.Name).Success) {
				peerTrack.Mute = false;
				lbl.Text = "" + peerTrack.DisplayIndex;
			}
		}
	}
	
	private  Track findPeerAudioTrack(Track track, object sender) {
		int peerAudioTrackIndex;
		if (sender == btnLPlayPrev || sender == btnLPlayNext) {
			peerAudioTrackIndex = track.Index + 2;
		} else { // btnRPlayPrev || btnRPlayNext
			peerAudioTrackIndex = track.Index + 1;
		}
		
		if (peerAudioTrackIndex < Common.vegas.Project.Tracks.Count &&
				peerAudioTrackIndex > -1) {
			Track peerAudioTrack = Common.vegas.Project.Tracks[peerAudioTrackIndex];
			if (peerAudioTrack.IsAudio() &&
					new Regex("^Audio").Match(peerAudioTrack.Name == null ? "" : peerAudioTrack.Name).Success) {
				return peerAudioTrack;
			}
		}
		
		return null;
	}
	
	void chkLockLeftRight_Click(object sender, EventArgs e) {
	}
	
	void chkLockWithVideo_Click(object sender, EventArgs e) {
	}
	
	void chkSplitAudio_Click(object sender, EventArgs e) {
		return;
		List<Track> unfilteredAudioTracks = Common.AudioTracksToTracks(Audio.FindAudioTracks(Common.vegas.Project));
		List<Track> audioTracks = Common.FindTracksByRegex(unfilteredAudioTracks, new Regex("^Audio"));

		// unsplit
		if (!chkSplitAudio.Checked) {
			lblPlayingTrackIndex.Text = "";
			
			using (UndoBlock undo = new UndoBlock(UNDO_STRING)) {
				Common.MuteAllTracks(audioTracks, true);
				foreach (Track audioTrack in audioTracks) {
					audioTrack.Mute = true;
					((AudioTrack)audioTrack).PanX = 0;
				}
			}
			
			return;
		}
		
		// split
		
		List<Track> videoTracks = Common.VideoTracksToTracks(Video.FindVideoTracks(Common.vegas.Project));
		List<Track> unmutedLVideoTracks =
			findUnmutedTracks(Common.FindTracksByRegex(videoTracks, new Regex("^LVideo")));
		List<Track> unmutedRVideoTracks =
			findUnmutedTracks(Common.FindTracksByRegex(videoTracks, new Regex("^RVideo")));
		
		if (!(unmutedLVideoTracks.Count == 1 && unmutedRVideoTracks.Count == 1)) {
			MessageBox.Show("Please make sure that exactly one \"LVideo\" track and one \"RVideo\" track are unmuted",
				Common.MUTE_TRACKS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			chkSplitAudio.Checked = false;
			return;
		}
		
		if (unmutedLVideoTracks[0].Index + 1 == unmutedRVideoTracks[0].Index) {
			MessageBox.Show("Please make sure \"LVideo\" and \"RVideo\" tracks are not adjacent",
				Common.MUTE_TRACKS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			chkSplitAudio.Checked = false;
			return;
		}
		
		using (UndoBlock undo = new UndoBlock(UNDO_STRING)) {
			Common.MuteAllTracks(audioTracks, true);
			
			Track lPeerAudioTrack = findPeerAudioTrack(unmutedLVideoTracks[0], btnLPlayPrev);
			if (lPeerAudioTrack != null) {
				lPeerAudioTrack.Mute = false;
				lblPlayingTrackIndex.Text = "";
				((AudioTrack)lPeerAudioTrack).PanX = -1;
			}
			
			Track rPeerAudioTrack = findPeerAudioTrack(unmutedRVideoTracks[0], btnRPlayPrev);
			if (rPeerAudioTrack != null) {
				rPeerAudioTrack.Mute = false;
				lblPlayingTrackIndex.Text = "";
				((AudioTrack)rPeerAudioTrack).PanX = 1;
			}
			
		}
	}
	
	void HandleProjectClosed(Object sender, EventArgs args) {
		InitializeCalcTempoForm();
		InitializeMuteTracksForm();
		InitializeVMuteTracksForm();
	}
	
	void HandleTrackCountChanged(Object sender, EventArgs args) {
		InitializeMuteTracksForm();
		InitializeVMuteTracksForm();
	}
	
	//
	// Event handlers END
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	private double calcTempo(Timecode length) {
		double tempo = 60.0 / (length.ToMilliseconds() / 1000.0);
		
		if (chkDoubleTime.Checked) {
			tempo *= 2;
		}
		
		return tempo;
	}
	
	public void updateTempo() {
		if (!chkMonitorRegion.Checked) {
			return;
		}
	
		// find tempo region
		Sony.Vegas.Region tempoRegion = null;
		foreach (Sony.Vegas.Region region in Common.vegas.Project.Regions) {
			if (region.Label == "Tempo" ||
					region.Label == "tempo") {
				tempoRegion = region;
			}
		}
		if (tempoRegion == null) {
			MessageBox.Show("Tempo region not found", Common.CALC_TEMPO, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		if (tempoRegion.Length.ToMilliseconds() == 0) {
			MessageBox.Show("Tempo region is zero", Common.CALC_TEMPO, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		txtTempo.Text = "" +
			calcTempo(tempoRegion.Length).ToString("F4");
	}

	private List<Track> findUnmutedTracks(List<Track> tracks) {
		List<Track> unmutedTracks = new List<Track>();
		foreach (Track track in tracks) {
			if (!track.Mute) {
				unmutedTracks.Add(track);
			}
		}
		return unmutedTracks;
	}
	
}

public class CalcTempoControlTest : Form {
	private CalcTempoControl calcTempoControl = new CalcTempoControl();
	
	public CalcTempoControlTest() {
		Controls.Add(calcTempoControl);
		Size = new Size(315, 215);
	}
	
	public static void Main() {
		Application.Run(new CalcTempoControlTest());
	}
	
}

