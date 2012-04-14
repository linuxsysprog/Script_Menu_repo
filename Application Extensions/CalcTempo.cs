// Copyright (C) 2011 Andrey Chislenko
// $Id$
// Monitor current selection and calculate tempo
// Added "Mute/Solo Audio Tracks" feature

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
			
			calcTempoView.DefaultFloatingSize = new Size(465, 240);
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
	public CheckBox chkLockLeftRight= new CheckBox();

	public CalcTempoControl() {
		gbCalcTempo.Size = new Size(135, 170);
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
		
		gbMuteTracks.Size = new Size(135, 200);
		gbMuteTracks.Location = new Point(10, 10);
		gbMuteTracks.Text = "Mute/Solo Au Tracks";
		gbMuteTracks.Controls.AddRange(new Control[] {
			chkMuteAll,
			chkSoloAll,
			lblPlayingTrack,
			lblPlayingTrackIndex,
			btnPlayPrev,
			btnPlayNext});
			
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
		
		gbVMuteTracks.Size = new Size(135, 200);
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
		
		btnLPlayNext.Size = new Size(35, 23);
		btnLPlayNext.Location = new Point(30, 135);
		btnLPlayNext.Text = "Next";
		btnLPlayNext.Click += new EventHandler(btnVPlayNext_Click);
		
		btnRPlayPrev.Size = new Size(35, 23);
		btnRPlayPrev.Location = new Point(70, 105);
		btnRPlayPrev.Text = "Prev";
		btnRPlayPrev.Click += new EventHandler(btnVPlayNext_Click);
		
		btnRPlayNext.Size = new Size(35, 23);
		btnRPlayNext.Location = new Point(70, 135);
		btnRPlayNext.Text = "Next";
		btnRPlayNext.Click += new EventHandler(btnVPlayNext_Click);
		
		chkLockLeftRight.Size = new Size(100, 20);
		chkLockLeftRight.Location = new Point(10, 165);
		chkLockLeftRight.Text = "&Lock L and R";
		chkLockLeftRight.Click += new EventHandler(chkLockLeftRight_Click);
		
		Size = new Size(1000, 1000);
		Controls.AddRange(new Control[] {
			gbCalcTempo,
			gbMuteTracks,
			gbVMuteTracks});
			
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
	}
	
	private void InitializeVMuteTracksForm() {
		chkVMuteAll.Checked = false;
		chkVSoloAll.Checked = false;
		lblVPlayingTrack.Text = PLAYING_TRACKS;
		lblVLPlayingTrack.Text = "";
		lblVRPlayingTrack.Text = "";
		chkLockLeftRight.Checked = false;
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
			muteAllTracks(tracks, chkMuteAll.Checked);
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
			muteAllTracks(tracks, chkVMuteAll.Checked);
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
			soloAllTracks(tracks, chkSoloAll.Checked);
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
			soloAllTracks(tracks, chkVSoloAll.Checked);
			lblVLPlayingTrack.Text = "";
			lblVRPlayingTrack.Text = "";
		}
	}
	
	void btnPlayNext_Click(object sender, EventArgs e) {
		List<Track> unfilteredTracks = Common.AudioTracksToTracks(Audio.FindAudioTracks(Common.vegas.Project));
		
		// only keep tracks matching ^Audio
		List<Track> tracks = Common.FindTracksByRegex(unfilteredTracks, new Regex("^Audio"));
		if (tracks.Count < 1) {
			MessageBox.Show("No audio tracks starting with \"Audio\" found",
				Common.MUTE_TRACKS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		using (UndoBlock undo = new UndoBlock(UNDO_STRING)) {
			// unsolo soloed tracks if there are any
			soloAllTracks(tracks, false);
			if (chkSoloAll.Checked) {
				chkSoloAll.Checked = false;
			}
			
			List<Track> unmutedTracks = findUnmutedTracks(tracks);
			
			// zero or more than one unmuted tracks
			if (unmutedTracks.Count == 0 || unmutedTracks.Count > 1) {
				muteAllTracks(tracks, true);
				tracks[0].Mute = false;
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
				return;
			}
			
			for (int i = 0; i < tracks.Count; i++) {
				if (!tracks[i].Mute) {
					muteAllTracks(tracks, true);
					
					if (sender == btnPlayPrev) {
						if (i > 0) {
							tracks[i - 1].Mute = false;
							lblPlayingTrackIndex.Text = "" + tracks[i - 1].DisplayIndex;
						} else {
							tracks[tracks.Count - 1].Mute = false;
							lblPlayingTrackIndex.Text = "" + tracks[tracks.Count - 1].DisplayIndex;
						}
					} else { // btnPlayNext
						if (i < tracks.Count - 1) {
							tracks[i + 1].Mute = false;
							lblPlayingTrackIndex.Text = "" + tracks[i + 1].DisplayIndex;
						} else {
							tracks[0].Mute = false;
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
		
		// only keep tracks matching ^LVideo or ^RVideo
		List<Track> tracks = Common.FindTracksByRegex(unfilteredTracks, regex);
		List<Track> peerTracks = Common.FindTracksByRegex(unfilteredTracks, peerTrackRegex);

		if (tracks.Count < 1) {
			MessageBox.Show("No video tracks starting with \"" + label + "\" found",
				Common.MUTE_VTRACKS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		using (UndoBlock undo = new UndoBlock(UNDO_STRING)) {
			// unsolo soloed tracks if there are any
			soloAllTracks(tracks, false);
			if (chkLockLeftRight.Checked) {
				soloAllTracks(peerTracks, false);
			}
			if (chkVSoloAll.Checked) {
				chkVSoloAll.Checked = false;
			}
			
			List<Track> unmutedTracks = findUnmutedTracks(tracks);
			
			// zero or more than one unmuted tracks
			if (unmutedTracks.Count == 0 || unmutedTracks.Count > 1) {
				muteAllTracks(tracks, true);
				if (chkLockLeftRight.Checked) {
					muteAllTracks(peerTracks, true);
				}
				if (chkVMuteAll.Checked) {
					chkVMuteAll.Checked = false;
				}
				tracks[0].Mute = false;
				lbl.Text = "" + tracks[0].DisplayIndex;
				unmutePeerTrack(tracks[0], sender);
				return;
			}
			
			// exactly one unmuted track
			
			// if the unmuted track happens to be the only (labeled) video track in the project
			// nothing needs to be done
			if (tracks.Count == 1) {
				lbl.Text = "" + tracks[0].DisplayIndex;
				unmutePeerTrack(tracks[0], sender);
				return;
			}
			
			for (int i = 0; i < tracks.Count; i++) {
				if (!tracks[i].Mute) {
					muteAllTracks(tracks, true);
					if (chkLockLeftRight.Checked) {
						muteAllTracks(peerTracks, true);
					}
					
					if (sender == btnLPlayPrev || sender == btnRPlayPrev) {
						if (i > 0) {
							tracks[i - 1].Mute = false;
							lbl.Text = "" + tracks[i - 1].DisplayIndex;
							unmutePeerTrack(tracks[i - 1], sender);
						} else {
							tracks[tracks.Count - 1].Mute = false;
							lbl.Text = "" + tracks[tracks.Count - 1].DisplayIndex;
							unmutePeerTrack(tracks[tracks.Count - 1], sender);
						}
					} else { // btnLPlayNext || btnRPlayNext
						if (i < tracks.Count - 1) {
							tracks[i + 1].Mute = false;
							lbl.Text = "" + tracks[i + 1].DisplayIndex;
							unmutePeerTrack(tracks[i + 1], sender);
						} else {
							tracks[0].Mute = false;
							lbl.Text = "" + tracks[0].DisplayIndex;
							unmutePeerTrack(tracks[0], sender);
						}
					}
					
					if (chkVMuteAll.Checked) {
						chkVMuteAll.Checked = false;
					}
					return;
				}
			}
		}
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
	
	void chkLockLeftRight_Click(object sender, EventArgs e) {
	}
	
	void HandleProjectClosed(Object sender, EventArgs args) {
		InitializeCalcTempoForm();
		InitializeMuteTracksForm();
		InitializeVMuteTracksForm();
		Common.vegas.DebugClear();
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

	private void muteAllTracks(List<Track> tracks, bool mute) {
		foreach (Track track in tracks) {
			if (mute && !track.Mute) {
				track.Mute = true;
			} else if (!mute && track.Mute) {
				track.Mute = false;
			}
		}
	}
	
	private void soloAllTracks(List<Track> tracks, bool solo) {
		foreach (Track track in tracks) {
			if (solo && !track.Solo) {
				track.Solo = true;
			} else if (!solo && track.Solo) {
				track.Solo = false;
			}
		}
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

