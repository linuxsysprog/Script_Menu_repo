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
			
			AddRulerControl addRulerControl = new AddRulerControl(vegas);
			addRulerView.Controls.Add(addRulerControl);
			
			addRulerView.DefaultFloatingSize = new Size(200, 260);
			vegas.LoadDockView(addRulerView);
		}
	}

	void HandleMenuPopup(Object sender, EventArgs args) {
		addRulerCmd.Checked = vegas.FindDockView(Common.ADD_RULER);
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

	private GroupBox gbLocation = new GroupBox();
	private RadioButton rbTop = new RadioButton();
	private RadioButton rbBottom = new RadioButton();
	private Label lblNumber = new Label();
	private ComboBox cbNumber = new ComboBox();
	private Label lblNotes = new Label();
	private TextBox txtNotes = new TextBox();
	private Button btnAdd = new Button();
	private Button btnFillGaps = new Button();

	public AddRulerControl(Vegas vegas) {
		this.vegas = vegas;
	
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
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(vegas.Project)))
		);
		
		if (selectedVideoTracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one video track selected",
				Common.ADD_RULER, MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}
		
		MessageBox.Show("Not yet implemented.");
	}
	
	void btnFillGaps_Click(object sender, EventArgs e) {
		MessageBox.Show(cbNumber.Text);
	}
	
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
	
}

public class AddRulerControlTest : Form {
	private AddRulerControl addRulerControl = new AddRulerControl(null);
	
	public AddRulerControlTest() {
		ClientSize = new Size (190, 240);
		Controls.Add(addRulerControl);
	}
	
	public static void Main() {
		Application.Run(new AddRulerControlTest());
	}
	
}

