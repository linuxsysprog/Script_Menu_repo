// File: CalcTempo.cs - Monitor current selection and calculate tempo

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
			
			calcTempoView.DefaultFloatingSize = new Size(200, 260);
			Common.vegas.LoadDockView(calcTempoView);
		}
	}

	void HandleMenuPopup(Object sender, EventArgs args) {
		calcTempoCmd.Checked = Common.vegas.FindDockView(Common.CALC_TEMPO);
	}
	
	void HandleMarkersChanged(Object sender, EventArgs args) {
		calcTempoControl.txtTempo.Text = "elmo";
	}
	
}

public class CalcTempoControl : UserControl {

	private Label lblTempo = new Label();
	public TextBox txtTempo = new TextBox();
	public CheckBox chkDoubleTime = new CheckBox();

	public CalcTempoControl() {
	
		lblTempo.Size = new Size(50, 20);
		lblTempo.Location = new Point(10, 10);
		lblTempo.Text = "&Tempo:";
		
		txtTempo.Size = new Size(50, 20);
		txtTempo.Location = new Point(60, 10);
		txtTempo.Text = "120.1234";
		
		chkDoubleTime.Size = new Size(100, 50);
		chkDoubleTime.Location = new Point(10, 30);
		chkDoubleTime.Text = "&Double Time";
		
		Size = new Size(1000, 1000);
		Controls.AddRange(new Control[] {
			lblTempo,
			txtTempo,
			chkDoubleTime});
	}
	
}

public class CalcTempoControlTest : Form {
	private CalcTempoControl calcTempoControl = new CalcTempoControl();
	
	public CalcTempoControlTest() {
		Controls.Add(calcTempoControl);
		Size = new Size(130, 110);
	}
	
	public static void Main() {
		Application.Run(new CalcTempoControlTest());
	}
	
}

