// File: AddObject.cs - Insert an object given a Sony Text media generator preset.
//                        This could be Notes, Rate, Tempo, Measure or Filename

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Sony.Vegas;
using AddRulerNamespace;

public class EntryPoint : Form {
	private Label lblFrameSize = new Label();
	private ComboBox cbFrameSize = new ComboBox();
	private Label lblObject = new Label();
	private ComboBox cbObject = new ComboBox();
	private Label lblPreset = new Label();
	private ComboBox cbPreset = new ComboBox();
	private Button btnAdd = new Button();
	private Button btnCancel = new Button();		

	public EntryPoint() {
		lblFrameSize.Size = new Size(70, 20);
		lblFrameSize.Location = new Point(10, 10);
		lblFrameSize.Text = "&Frame Size:";
		
		cbFrameSize.Size = new Size(200, 50);
		cbFrameSize.Location = new Point(80, 10);
		cbFrameSize.Items.AddRange(new object[] { "n/a" });
		cbFrameSize.SelectedIndex = 0;
		cbFrameSize.Validated += new EventHandler(cbFrameSize_Validated);
		
		lblObject.Size = new Size(70, 20);
		lblObject.Location = new Point(10, 50);
		lblObject.Text = "&Object:";
		
		cbObject.Size = new Size(200, 50);
		cbObject.Location = new Point(80, 50);
		cbObject.Items.AddRange(new object[] { "n/a" });
		cbObject.SelectedIndex = 0;
		cbObject.Validated += new EventHandler(cbFrameSize_Validated);
		
		lblPreset.Size = new Size(70, 20);
		lblPreset.Location = new Point(10, 90);
		lblPreset.Text = "&Preset:";
		
		cbPreset.Size = new Size(200, 50);
		cbPreset.Location = new Point(80, 90);
		cbPreset.Items.AddRange(new object[] { "n/a" });
		cbPreset.SelectedIndex = 0;
		cbPreset.Validated += new EventHandler(cbFrameSize_Validated);
		
		btnAdd.Location = new Point(110, 130);
		btnAdd.Text = "&Add";
		btnAdd.Click += new EventHandler(btnAdd_Click);

		btnCancel.Click += new EventHandler(btnCancel_Click);
		btnCancel.Size = new Size(0, 0);

		Controls.AddRange(new Control[] {
			lblFrameSize,
			cbFrameSize,
			lblObject,
			cbObject,
			lblPreset,
			cbPreset,
			btnAdd,
			btnCancel});
	}
	
	//
	// Event handlers BEGIN
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	void cbFrameSize_Validated(object sender, EventArgs e) {
		// int measure;
		
		// try {
			// measure = Convert.ToInt32(cbFrameSize.Text);
			// if (measure < 1) {
				// throw new Exception("measure is less than one");
			// }
		// } catch (Exception ex) {
			// MessageBox.Show("Invalid Measure");
			// cbFrameSize.Focus();
		// }
	}
	
	void btnAdd_Click(object sender, EventArgs e) {
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

		Text = Common.ADD_OBJECT;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		AcceptButton = btnAdd;
		CancelButton = btnCancel;
		StartPosition = FormStartPosition.CenterParent;
		Size = new Size(300, 190);

		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(vegas.Project)))
		);
		if (selectedVideoTracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one video track selected",
				Common.ADD_RULER, MessageBoxButtons.OK, MessageBoxIcon.Error);
			Close();
			return;
		}
		
		ShowDialog();
	}

}

