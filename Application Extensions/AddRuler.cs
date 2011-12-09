// File: AddRuler.cs - Insert a frame with a ruler at the top or bottom

using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using Sony.Vegas;

public class AddRuler : ICustomCommandModule {
	private Vegas vegas;
	private CustomCommand addRulerCmd = new CustomCommand(CommandCategory.View, "AddRulerCmd");
	
	public void InitializeModule(Vegas vegas) {
		this.vegas = vegas;
	}

	public ICollection GetCustomCommands() {
		addRulerCmd.DisplayName = "Add Ruler View";
		addRulerCmd.Invoked += this.HandleInvoked;
		addRulerCmd.MenuPopup += this.HandleMenuPopup;
		return new CustomCommand[] { addRulerCmd };
	}

	void HandleInvoked(Object sender, EventArgs args) {
		if (!vegas.ActivateDockView("Add Ruler")) {
			DockableControl addRulerView = new DockableControl("Add Ruler");
			
			// create a label
			Label label = new Label();
			label.Dock = DockStyle.Fill;
			label.Text = "Under Construction";
			label.TextAlign = ContentAlignment.MiddleCenter;
			addRulerView.Controls.Add(label);
			
			vegas.LoadDockView(addRulerView);
		}
	}

	void HandleMenuPopup(Object sender, EventArgs args) {
		addRulerCmd.Checked = vegas.FindDockView("Add Ruler");
	}
}

public class CustomForm : Form {
	private GroupBox groupBox1;
	private RadioButton radioButton2;
	private RadioButton radioButton1;
	private RadioButton selectedrb;
	private Button getSelectedRB;

	public void InitializeRadioButtons()
	{
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.radioButton2 = new System.Windows.Forms.RadioButton();
		this.radioButton1 = new System.Windows.Forms.RadioButton();
		this.getSelectedRB = new System.Windows.Forms.Button();

		this.groupBox1.Controls.Add(this.radioButton2);
		this.groupBox1.Controls.Add(this.radioButton1);
		this.groupBox1.Controls.Add(this.getSelectedRB);
		this.groupBox1.Location = new System.Drawing.Point(10, 10);
		this.groupBox1.Size = new System.Drawing.Size(220, 125);
		this.groupBox1.Text = "Location";

		this.radioButton2.Location = new System.Drawing.Point(31, 53);
		this.radioButton2.Size = new System.Drawing.Size(67, 17);
		this.radioButton2.Text = "Choice 2";
		this.radioButton2.CheckedChanged += new EventHandler(radioButton_CheckedChanged);

		this.radioButton1.Location = new System.Drawing.Point(31, 20);
		this.radioButton1.Size = new System.Drawing.Size(67, 17);
		this.radioButton1.Text = "Choice 1";
		this.radioButton1.CheckedChanged += new EventHandler(radioButton_CheckedChanged);

		this.getSelectedRB.Location = new System.Drawing.Point(10, 75);
		this.getSelectedRB.Size = new System.Drawing.Size(200, 25);
		this.getSelectedRB.Text = "Get selected RadioButton";
		this.getSelectedRB.Click += new EventHandler(getSelectedRB_Click);

		this.ClientSize = new System.Drawing.Size(292, 266);
		this.Controls.Add(this.groupBox1);
	}

	void radioButton_CheckedChanged(object sender, EventArgs e)
	{
		RadioButton rb = sender as RadioButton;

		if (rb == null)
		{
			MessageBox.Show("Sender is not a RadioButton");
			return;
		}

		// Ensure that the RadioButton.Checked property
		// changed to true.
		if (rb.Checked)
		{
			// Keep track of the selected RadioButton by saving a reference
			// to it.
			selectedrb = rb;
		}
	}

	// Show the text of the selected RadioButton.
	void getSelectedRB_Click(object sender, EventArgs e)
	{
		MessageBox.Show(selectedrb.Text);
	}

	// MSDN UserControl example
	private Label label;

	public CustomForm( )
	{
		InitializeRadioButtons();
		
		// create the objects
		this.label = new Label ( );

		// Add the controls and set the client area
		this.AutoScaleBaseSize = new System.Drawing.Size (5, 13);
		this.ClientSize = new System.Drawing.Size (640, 480);
		this.Controls.Add (this.label);
	}
	
	// Run the app
	public static void Main_( )
	{
		// MessageBox.Show("Gaps have been filled", "Add Ruler");
		// return;
		Application.Run(new CustomForm( ));
	}
}

public class CustomForm_ : Form {
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
	private TextBox tbNotes = new TextBox();

	public CustomForm_() {
		gbLocation.Size = new Size(150, 50);
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
		rbBottom.Location = new Point(80, 20);
		rbBottom.Text = "&Bottom";
		
		lblNumber.Location = new Point(10, 80);
		lblNumber.Text = "&Number:";
		
		cbNumber.Size = new Size(40, 50);
		cbNumber.Location = new Point(120, 80);
		cbNumber.Items.AddRange(TOP_RANGE);
		cbNumber.SelectedIndex = 0;
		cbNumber.Validated += new EventHandler(cbNumber_Validated);
		
		lblNotes.Size = new Size(40, 60);
		lblNotes.Location = new Point(10, 120);
		lblNotes.Text = "N&otes:";
		
		tbNotes.Multiline = true;
		tbNotes.ScrollBars = ScrollBars.Vertical;
		tbNotes.Size = new Size(100, 60);
		tbNotes.Location = new Point(60, 120);
		tbNotes.Text = "[Section]";
		
		ClientSize = new Size(640, 480);
		Controls.AddRange(new Control[] {
			gbLocation,
			lblNumber,
			cbNumber,
			lblNotes,
			tbNotes});
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
	
	private void Validate_cbNumber() {
		int rulerNumber;
		
		try {
			rulerNumber = Convert.ToInt32(cbNumber.Text);
		} catch (Exception exception) {
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
	
	public static void Main() {
		// MessageBox.Show("Main() Entry.");
		Application.Run(new CustomForm_());
	}
}
