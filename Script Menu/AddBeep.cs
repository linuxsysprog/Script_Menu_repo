// File: AddBeep.cs - Insert a beep with high and low pitch as takes

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint : Form {
	private object[] TOP_RANGE = new object[] { "1", "2", "3", "4", "5",
		"6", "7", "8", "9", "10",
		"11", "12" };

	private object[] BOTTOM_RANGE = new object[] { "1", "2", "3", "4", "5",
		"6", "7", "8", "9", "10",
		"11", "12", "13", "14", "15",
		"16" };

	private GroupBox gbLocation = new GroupBox();
	private RadioButton rbTop = new RadioButton();
	private RadioButton rbBottom = new RadioButton();
	private Label lblNumber = new Label();
	private ComboBox cbNumber = new ComboBox();
	private Label lblNotes = new Label();
	private TextBox txtNotes = new TextBox();
	private Button btnAdd = new Button();
	private Button btnFillGaps = new Button();
	private Button btnCancel = new Button();		

	public EntryPoint() {
		gbLocation.Size = new Size(170, 50);
		gbLocation.Location = new Point(10, 10);
		gbLocation.Text = "Location";
		gbLocation.Controls.AddRange(new Control[] {
			rbTop,
			rbBottom});
		
		rbTop.Size = new Size(50, 20);
		rbTop.Location = new Point(20, 20);
		rbTop.Text = "&Top";
		rbTop.Checked = true;
		rbTop.CheckedChanged += new EventHandler(rbTop_CheckedChanged);
		
		rbBottom.Size = new Size(60, 20);
		rbBottom.Location = new Point(100, 20);
		rbBottom.Text = "&Bottom";
		
		lblNumber.Location = new Point(10, 80);
		lblNumber.Text = "&Number:";
		
		cbNumber.Size = new Size(40, 50);
		cbNumber.Location = new Point(140, 80);
		cbNumber.Items.AddRange(TOP_RANGE);
		cbNumber.SelectedIndex = 0;
		cbNumber.Validated += new EventHandler(cbNumber_Validated);
		
		lblNotes.Size = new Size(40, 60);
		lblNotes.Location = new Point(10, 120);
		lblNotes.Text = "N&otes:";
		
		txtNotes.Multiline = true;
		txtNotes.ScrollBars = ScrollBars.Vertical;
		txtNotes.Size = new Size(120, 60);
		txtNotes.Location = new Point(60, 120);
		txtNotes.Text = "[Section]";
		
		btnAdd.Location = new Point(10, 200);
		btnAdd.Text = "&Add";
		btnAdd.Click += new EventHandler(btnAdd_Click);

		btnFillGaps.Location = new Point(105, 200);
		btnFillGaps.Text = "&Fill Gaps";
		btnFillGaps.Click += new EventHandler(btnFillGaps_Click);

		btnCancel.Click += new EventHandler(btnCancel_Click);
		btnCancel.Size = new Size(0, 0);

		Size = new Size(190, 240);
		Controls.AddRange(new Control[] {
			gbLocation,
			lblNumber,
			cbNumber,
			lblNotes,
			txtNotes,
			btnAdd,
			btnFillGaps,
			btnCancel});
	}
	
	//
	// Event handlers BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	void rbTop_CheckedChanged(object sender, EventArgs e) {
		if (rbTop.Checked) {
			cbNumber.Items.Clear();
			cbNumber.Items.AddRange(TOP_RANGE);
		} else {
			cbNumber.Items.Clear();
			cbNumber.Items.AddRange(BOTTOM_RANGE);
		}
		
		Validate_cbNumber();
	}
	
	void cbNumber_Validated(object sender, EventArgs e) {
		Validate_cbNumber();
	}
	
	void btnAdd_Click(object sender, EventArgs e) {
		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(Common.vegas.Project)))
		);
		
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
		
			DialogResult result = MessageBox.Show(msg, Common.ADD_BEEP, MessageBoxButtons.OKCancel,
				MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
			if (result != DialogResult.OK) {
				return;
			}
		}
		
		Video.AddRuler(selectedVideoTracks[0], Common.vegas.Transport.CursorPosition,
			rbTop.Checked, Convert.ToInt32(cbNumber.Text),
			txtNotes.Text == "[Section]" ? "." : txtNotes.Text);
			
		Close();
	}
	
	void btnFillGaps_Click(object sender, EventArgs e) {
		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(Common.vegas.Project)))
		);
		
		List<TrackEvent> events = Common.TrackEventsToTrackEvents(selectedVideoTracks[0].Events);
		
		// there must be at least two events on the track to continue
		if (events.Count < 2) {
			MessageBox.Show("Please make sure there are at least two events on the track to continue",
				Common.ADD_BEEP, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		int eventsTrimmed = 0;
		for (int i = 0; i < events.ToArray().Length - 1; i++) {
			TrackEvent @event = events.ToArray()[i];
			
			if (@event.End < events.ToArray()[i + 1].Start) {
				@event.End = events.ToArray()[i + 1].Start;
				eventsTrimmed++;
			}
		}
		
		MessageBox.Show(eventsTrimmed + " out of " + events.Count +
			" total events were trimmed", Common.ADD_BEEP);
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
	
	private void Validate_cbNumber() {
		int rulerNumber;
		
		try {
			rulerNumber = Convert.ToInt32(cbNumber.Text);
		} catch (Exception e) {
			MessageBox.Show("Invalid Ruler Number");
			cbNumber.Focus();
			return;
		}
		
		if ((rbTop.Checked && (rulerNumber < 1 || rulerNumber > 12))
			|| (rbBottom.Checked && (rulerNumber < 1 || rulerNumber > 16))) {
			MessageBox.Show("Ruler Number out of range");
			cbNumber.Focus();
		}
	}
	
    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;

		Text = Common.ADD_BEEP;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		AcceptButton = btnAdd;
		CancelButton = btnCancel;
		StartPosition = FormStartPosition.CenterParent;
		Size = new Size(200, 260);

		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(vegas.Project)))
		);
		if (selectedVideoTracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one video track selected",
				Common.ADD_BEEP, MessageBoxButtons.OK, MessageBoxIcon.Error);
			Close();
			return;
		}
		
		ShowDialog();
	}

}

