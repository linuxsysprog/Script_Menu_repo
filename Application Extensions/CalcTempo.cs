// Copyright (C) 2011 Andrey Chislenko
// File: CalcTempo.cs - Monitor current selection and calculate tempo
//                    - Added "Mute/Solo Audio Tracks" feature

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
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
			
			calcTempoView.DefaultFloatingSize = new Size(315, 215);
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
	const string PLAYING_TRACK = "Playing Track: ";
	const string UNDO_STRING = "Mute/Solo Audio Tracks";
	const string NO_TRACKS = "No audio tracks found";

	private GroupBox gbCalcTempo = new GroupBox();
	private Label lblTempo = new Label();
	public TextBox txtTempo = new TextBox();
	public CheckBox chkDoubleTime = new CheckBox();
	public CheckBox chkMonitorRegion = new CheckBox();
	private Button btnCalcTempo = new Button();

	private GroupBox gbMuteTracks= new GroupBox();
	public CheckBox chkActivate= new CheckBox();
	public CheckBox chkMuteAll= new CheckBox();
	public CheckBox chkSoloAll= new CheckBox();
	private Label lblPlayingTrack = new Label();
	private Button btnPlayNext= new Button();

	public CalcTempoControl() {
		gbCalcTempo.Size = new Size(135, 170);
		gbCalcTempo.Location = new Point(10, 10);
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
		txtTempo.Text = "000.0000";
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
		
		gbMuteTracks.Size = new Size(135, 170);
		gbMuteTracks.Location = new Point(160, 10);
		gbMuteTracks.Text = "Mute/Solo Au Tracks";
		gbMuteTracks.Controls.AddRange(new Control[] {
			chkActivate,
			chkMuteAll,
			chkSoloAll,
			lblPlayingTrack,
			btnPlayNext});
			
		chkActivate.Size = new Size(100, 20);
		chkActivate.Location = new Point(10, 20);
		chkActivate.Text = "&Activate";
		chkActivate.Click += new EventHandler(chkActivate_Click);
		
		chkMuteAll.Size = new Size(100, 20);
		chkMuteAll.Location = new Point(10, 40);
		chkMuteAll.Text = "&Mute All";
		chkMuteAll.Click += new EventHandler(chkMuteAll_Click);
		
		chkSoloAll.Size = new Size(100, 20);
		chkSoloAll.Location = new Point(10, 60);
		chkSoloAll.Text = "&Solo All";
		chkSoloAll.Click += new EventHandler(chkSoloAll_Click);
		
		lblPlayingTrack.Size = new Size(100, 20);
		lblPlayingTrack.Location = new Point(10, 90);
		lblPlayingTrack.Text = PLAYING_TRACK;
		
		btnPlayNext.Location = new Point(30, 130);
		btnPlayNext.Text = "Play &Next";
		btnPlayNext.Click += new EventHandler(btnPlayNext_Click);
		
		Size = new Size(1000, 1000);
		Controls.AddRange(new Control[] {
			gbCalcTempo,
			gbMuteTracks});
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
	
	void chkActivate_Click(object sender, EventArgs e) {
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
		}
	}
	
	void btnPlayNext_Click(object sender, EventArgs e) {
		List<Track> tracks = Common.AudioTracksToTracks(Audio.FindAudioTracks(Common.vegas.Project));
		
		if (tracks.Count < 1) {
			MessageBox.Show(NO_TRACKS, Common.MUTE_TRACKS, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		using (UndoBlock undo = new UndoBlock(UNDO_STRING)) {
			// unsolo all tracks
			soloAllTracks(tracks, false);
			
			List<Track> unmutedTracks = findUnmutedTracks(tracks);
			
			// zero or more than one unmuted tracks
			if (unmutedTracks.Count == 0 || unmutedTracks.Count > 1) {
				muteAllTracks(tracks, true);
				tracks[0].Mute = false;
				return;
			}
			
			// exactly one unmuted track
			for (int i = 0; i < tracks.Count; i++) {
				if (!tracks[i].Mute) {
					muteAllTracks(tracks, true);
					if (i < tracks.Count - 1) {
						tracks[i + 1].Mute = false;
					} else {
						tracks[0].Mute = false;
					}
					return;
				}
			}
		}
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

