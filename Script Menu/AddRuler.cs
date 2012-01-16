// File: AddRuler.cs - Insert a frame with a ruler at the top or bottom

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;
using AddRulerNamespace;

public class AddRuler : ICustomCommandModule {
	private Vegas vegas;
	private CustomCommand addRulerCmd = new CustomCommand(CommandCategory.View, "AddRulerCmd");
	
	public void InitializeModule(Vegas vegas) {
		this.vegas = vegas;
		Common.vegas = vegas;
	}

	public ICollection GetCustomCommands() {
		addRulerCmd.DisplayName = Common.ADD_RULER + " View";
		addRulerCmd.Invoked += this.HandleInvoked;
		addRulerCmd.MenuPopup += this.HandleMenuPopup;
		return new CustomCommand[] { addRulerCmd };
	}

	void HandleInvoked(Object sender, EventArgs args) {
		if (!vegas.ActivateDockView(Common.ADD_RULER)) {
			DockableControl addRulerView = new DockableControl(Common.ADD_RULER);
			
			AddRulerControl addRulerControl = new AddRulerControl(vegas, null);
			addRulerView.Controls.Add(addRulerControl);
			
			addRulerView.DefaultFloatingSize = new Size(200, 260);
			vegas.LoadDockView(addRulerView);
		}
	}

	void HandleMenuPopup(Object sender, EventArgs args) {
		addRulerCmd.Checked = vegas.FindDockView(Common.ADD_RULER);
	}
}

public class EntryPoint {
	private Form form = new Form();

    public void FromVegas(Vegas vegas) {
		Common.vegas = vegas;

		Button btnCancel = new Button();		
		btnCancel.Click += new EventHandler(btnCancel_Click);
		btnCancel.Size = new Size(0, 0);

		AddRulerControl addRulerControl = new AddRulerControl(vegas, form);
		form.Controls.Add(addRulerControl);
		form.Controls.Add(btnCancel);
		
	   form.Text = Common.ADD_RULER;
	   form.FormBorderStyle = FormBorderStyle.FixedDialog;
	   form.MaximizeBox = false;
	   form.MinimizeBox = false;
	   form.AcceptButton = addRulerControl.btnAdd;
	   form.CancelButton = btnCancel;
	   form.StartPosition = FormStartPosition.CenterParent;
	   form.Size = new Size(200, 260);
	   
	   // the icon is ugly
	   // Bitmap bitmap;
	   // try {
		   // bitmap = new Bitmap(vegas.InstallationDirectory +
				// "\\Script Menu\\AddRuler.cs.png");
		   // form.Icon = Icon.FromHandle(bitmap.GetHicon());
	   // } catch (Exception e) { vegas.ShowError(e); }
		
		// check for the user has selected exactly one track
		// and that track is of type video
		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(vegas.Project)))
		);
		if (selectedVideoTracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one video track selected",
				Common.ADD_RULER, MessageBoxButtons.OK, MessageBoxIcon.Error);
			form.Close();
			return;
		}
		
		// Application.Run(form);
		form.ShowDialog();
		
		// this causes the script to crash
		// form.Show();
	}

	void btnCancel_Click(object sender, EventArgs e) {
		form.Close();
	}
	
}

public class AddRulerControl : UserControl {
	private object[] TOP_RANGE = new object[] { "1", "2", "3", "4", "5",
		"6", "7", "8", "9", "10",
		"11", "12" };

	private object[] BOTTOM_RANGE = new object[] { "1", "2", "3", "4", "5",
		"6", "7", "8", "9", "10",
		"11", "12", "13", "14", "15",
		"16" };

	private Vegas vegas;
	private Form form;

	private GroupBox gbLocation = new GroupBox();
	private RadioButton rbTop = new RadioButton();
	private RadioButton rbBottom = new RadioButton();
	private Label lblNumber = new Label();
	private ComboBox cbNumber = new ComboBox();
	private Label lblNotes = new Label();
	private TextBox txtNotes = new TextBox();
	public Button btnAdd = new Button();
	private Button btnFillGaps = new Button();

	public AddRulerControl(Vegas vegas, Form form) {
		this.vegas = vegas;
		this.form = form;
	
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
		
		lblNotes.Size = new Size(40, 60);
		lblNotes.Location = new Point(10, 120);
		lblNotes.Text = "N&otes:";
		
		txtNotes.Multiline = true;
		txtNotes.ScrollBars = ScrollBars.Vertical;
		txtNotes.Size = new Size(120, 60);
		txtNotes.Location = new Point(60, 120);
		txtNotes.Text = "[Section]";
		
		// btnAdd.Size = new Size(75, 23);
		btnAdd.Location = new Point(10, 200);
		btnAdd.Text = "&Add";
		btnAdd.Click += new EventHandler(btnAdd_Click);

		btnFillGaps.Location = new Point(105, 200);
		btnFillGaps.Text = "&Fill Gaps";
		btnFillGaps.Click += new EventHandler(btnFillGaps_Click);

		Size = new Size(190, 240);
		Controls.AddRange(new Control[] {
			gbLocation,
			lblNumber,
			cbNumber,
			lblNotes,
			txtNotes,
			btnAdd,
			btnFillGaps});
	}
	
	//
	// Event handlers BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	void rbTop_CheckedChanged(object sender, EventArgs e) {
		cbNumber.Items.Clear();
		
		if (rbTop.Checked) {
			cbNumber.Items.AddRange(TOP_RANGE);
		} else {
			cbNumber.Items.AddRange(BOTTOM_RANGE);
		}
		
		cbNumber.SelectedIndex = 0;
	}
	
	void btnAdd_Click(object sender, EventArgs e) {
		try {
			validateForm();
		} catch (Exception ex) {
			MessageBox.Show(ex.Message);
			return;
		}
		
		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(vegas.Project)))
		);
		
		List<TrackEvent> events = Common.FindEventsByPosition(selectedVideoTracks[0],
			vegas.Transport.CursorPosition);
		
		if (events.Count > 0) {
			string msg = "";
			if (events.Count == 1) {
				msg += "There is already " + events.Count + " event at position ";
			} else {
				msg += "There are already " + events.Count + " events at position ";
			}
			msg += vegas.Transport.CursorPosition + ". Would you like to continue?";
		
			DialogResult result = MessageBox.Show(msg, Common.ADD_RULER, MessageBoxButtons.OKCancel,
				MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
			if (result != DialogResult.OK) {
				return;
			}
		}
		
		Video.AddRuler(selectedVideoTracks[0], vegas.Transport.CursorPosition,
			rbTop.Checked, Convert.ToInt32(cbNumber.Text),
			txtNotes.Text == "[Section]" ? "." : txtNotes.Text);
			
		form.Close();
	}
	
	void btnFillGaps_Click(object sender, EventArgs e) {
		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(vegas.Project)))
		);
		
		List<TrackEvent> events = Common.TrackEventsToTrackEvents(selectedVideoTracks[0].Events);
		
		// there must be at least two events on the track to continue
		if (events.Count < 2) {
			MessageBox.Show("Please make sure there are at least two events on the track to continue",
				Common.ADD_RULER, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
			" total events were trimmed", Common.ADD_RULER);
		form.Close();
	}
	
	//
	// Event handlers END
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	private void validateForm() {
		try {
			int n = Convert.ToInt32(cbNumber.Text);
			if ((rbTop.Checked && (n < 1 || n > 12))
				|| (rbBottom.Checked && (n < 1 || n > 16))) {
				throw new Exception("Ruler Number is out of range");
			}
		} catch (Exception ex) {
			cbNumber.Focus();
			cbNumber.SelectAll();
			throw new Exception("Invalid Ruler Number");
		}
	}
		
}

public class AddRulerControlTest : Form {
	private AddRulerControl addRulerControl = new AddRulerControl(null, null);
	
	public AddRulerControlTest() {
		ClientSize = new Size (190, 240);
		Controls.Add(addRulerControl);
	}
	
	public static void Main() {
		Application.Run(new AddRulerControlTest());
	}
	
}

