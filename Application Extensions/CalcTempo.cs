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
			
			calcTempoView.DefaultFloatingSize = new Size(170, 170);
			Common.vegas.LoadDockView(calcTempoView);
		}
	}

	void HandleMenuPopup(Object sender, EventArgs args) {
		calcTempoCmd.Checked = Common.vegas.FindDockView(Common.CALC_TEMPO);
	}
	
	void HandleMarkersChanged(Object sender, EventArgs args) {
		if (!calcTempoControl.chkMonitorRegion.Checked) {
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
			MessageBox.Show("Tempo region not found");
			return;
		}
		if (tempoRegion.Length.ToMilliseconds() == 0) {
			MessageBox.Show("Tempo region is zero");
			return;
		}
		
		// calculate tempo
		double tempo = 60.0 / (tempoRegion.Length.ToMilliseconds() / 1000.0);
		if (calcTempoControl.chkDoubleTime.Checked) {
			tempo *= 2;
		}
		calcTempoControl.txtTempo.Text = "" + tempo.ToString("F4");
	}
	
}

public class CalcTempoControl : UserControl {

	private Label lblTempo = new Label();
	public TextBox txtTempo = new TextBox();
	public CheckBox chkDoubleTime = new CheckBox();
	public CheckBox chkMonitorRegion = new CheckBox();
	private Button btnCalcTempo = new Button();

	public CalcTempoControl() {
	
		lblTempo.Size = new Size(50, 20);
		lblTempo.Location = new Point(10, 10);
		lblTempo.Text = "&Tempo:";
		
		txtTempo.Size = new Size(50, 20);
		txtTempo.Location = new Point(100, 10);
		txtTempo.Text = "000.0000";
		txtTempo.ReadOnly = true;
		
		chkDoubleTime.Size = new Size(100, 20);
		chkDoubleTime.Location = new Point(10, 50);
		chkDoubleTime.Text = "&Double Time";
		
		chkMonitorRegion.Size = new Size(150, 50);
		chkMonitorRegion.Location = new Point(10, 60);
		chkMonitorRegion.Text = "&Monitor \"Tempo\" Region";
		
		btnCalcTempo.Location = new Point(40, 110);
		btnCalcTempo.Text = "&Calculate";
		btnCalcTempo.Click += new EventHandler(btnCalcTempo_Click);
		
		Size = new Size(1000, 1000);
		Controls.AddRange(new Control[] {
			lblTempo,
			txtTempo,
			chkDoubleTime,
			chkMonitorRegion,
			btnCalcTempo});
	}
	
		void btnCalcTempo_Click(object sender, EventArgs e) {
			MessageBox.Show("btnCalcTempo_Click() Entry.");
		}
	
}

public class CalcTempoControlTest : Form {
	private CalcTempoControl calcTempoControl = new CalcTempoControl();
	
	public CalcTempoControlTest() {
		Controls.Add(calcTempoControl);
		Size = new Size(170, 170);
	}
	
	public static void Main() {
		Application.Run(new CalcTempoControlTest());
	}
	
}

