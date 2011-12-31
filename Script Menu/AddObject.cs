// File: AddObject.cs - Insert an object given a Sony Text media generator preset.
//                        This could be Notes, Rate, Tempo, Measure or Filename

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
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
		
		cbFrameSize.Size = new Size(70, 50);
		cbFrameSize.Location = new Point(210, 10);
		cbFrameSize.Validated += new EventHandler(cbFrameSize_Validated);
		
		lblObject.Size = new Size(70, 20);
		lblObject.Location = new Point(10, 50);
		lblObject.Text = "&Object:";
		
		cbObject.Size = new Size(70, 50);
		cbObject.Location = new Point(210, 50);
		cbObject.Validated += new EventHandler(cbFrameSize_Validated);
		
		lblPreset.Size = new Size(70, 20);
		lblPreset.Location = new Point(10, 90);
		lblPreset.Text = "&Preset:";
		
		cbPreset.Size = new Size(200, 50);
		cbPreset.Location = new Point(80, 90);
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

		// setup form
		Text = Common.ADD_OBJECT;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		AcceptButton = btnAdd;
		CancelButton = btnCancel;
		StartPosition = FormStartPosition.CenterParent;
		Size = new Size(300, 190);

		// get the video track to work on
		List<VideoTrack> selectedVideoTracks = Common.TracksToVideoTracks(
			Common.FindSelectedTracks(Common.VideoTracksToTracks(Video.FindVideoTracks(vegas.Project)))
		);
		if (selectedVideoTracks.Count != 1) {
			MessageBox.Show("Please make sure you have exactly one video track selected",
				Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Error);
			Close();
			return;
		}
		
		// get text media generator
		PlugInNode plugIn = vegas.Generators.GetChildByName("Sony Text");
		if (plugIn == null) {
			MessageBox.Show("Couldn't find Sony Text media generator",
				Common.ADD_OBJECT, MessageBoxButtons.OK, MessageBoxIcon.Error);
			Close();
			return;
		}
		
		// get presets
		List<Preset> presets = new List<Preset>();
		foreach (EffectPreset preset in plugIn.Presets) {
			try {
				presets.Add(new Preset(preset.Name));
			} catch (Exception e) {
				continue;
			}
		}

		// dump presets
		// Common.vegas.DebugClear();
		// foreach (Preset preset in presets) {
			// Common.vegas.DebugOut("" + preset);
		// }

		// populate Frame Size drop-down
		List<string> frameSizes = new List<string>();
		foreach (Preset preset in presets) {
			frameSizes.Add(preset.FrameSize);
		}
		frameSizes = removeDuplicates(frameSizes);
		frameSizes.Sort();
		cbFrameSize.Items.AddRange(frameSizes.ToArray());
		cbFrameSize.SelectedIndex = 0;
		
		// populate Object drop-down
		List<string> objects = new List<string>();
		foreach (Preset preset in presets) {
			if (preset.FrameSize == cbFrameSize.Text) {
				objects.Add(preset.Object);
			}
		}
		objects = removeDuplicates(objects);
		objects.Sort();
		cbObject.Items.AddRange(objects.ToArray());
		cbObject.SelectedIndex = 0;
		
		// populate Preset drop-down
		List<string> values = new List<string>();
		foreach (Preset preset in presets) {
			if (preset.FrameSize == cbFrameSize.Text &&
					preset.Object == cbObject.Text) {
				values.Add(preset.Value);
			}
		}
		values = removeDuplicates(values);
		values.Sort();
		cbPreset.Items.AddRange(values.ToArray());
		cbPreset.SelectedIndex = 0;
		
		// show dialog
		ShowDialog();
	}

	// Courtesy of http://www.kirupa.com/forum/showthread.php?240523-C-Removing-Duplicates-from-List
	static List<string> removeDuplicates(List<string> inputList)
	{
		Dictionary<string, int> uniqueStore = new Dictionary<string, int>();
		List<string> finalList = new List<string>();
		foreach (string currValue in inputList)
		{
			if (!uniqueStore.ContainsKey(currValue))
			{
				uniqueStore.Add(currValue, 0);
				finalList.Add(currValue);
			}
		}
		return finalList;
	}

}

public class Preset : IComparable {
	private string frameSize;
	private string @object;
	private string value;
	private Regex regex = new Regex("^\\d+");

	public Preset(string strPreset) {
		string[] result = strPreset.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		if (regex.Matches(strPreset).Count < 1 ||
				result.Length != 3) {
			throw new Exception("can't parse Preset string: " + strPreset);
		}
		
		this.frameSize = result[0];
		this.@object = result[1];
		this.value = result[2];
	}
	
	public string FrameSize {
		get {
			return frameSize;
		}
		set {
			frameSize = value;
		}
	}
	
	public string Object {
		get {
			return @object;
		}
		set {
			@object = value;
		}
	}
	
	public string Value {
		get {
			return value;
		}
		set {
			this.value = value;
		}
	}
	
	public override string ToString() {
		return frameSize + " " + @object + " " + value;
	}
	
    public int CompareTo(object obj) {
        if (obj == null) return 1;

        Preset otherPreset = obj as Preset;
        if (otherPreset != null) 
            return ToString().CompareTo(otherPreset.ToString());
        else
           throw new ArgumentException("Object is not a Preset");
    }

}

