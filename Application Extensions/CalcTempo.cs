// Copyright (C) 2011-2013 Andrey Chislenko
// $Id$
// Monitor current selection and calculate tempo

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
			
			calcTempoView.DefaultFloatingSize = new Size(165, 265);
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
	private GroupBox gbCalcTempo = new GroupBox();
	private Label lblTempo = new Label();
	public TextBox txtTempo = new TextBox();
	public CheckBox chkDoubleTempo = new CheckBox();
	public CheckBox chkMonitorRegion = new CheckBox();
	private Button btnCalcTempo = new Button();

	public CalcTempoControl() {
		gbCalcTempo.Size = new Size(135, 220);
		gbCalcTempo.Location = new Point(10, 10);
		gbCalcTempo.Text = "Calculate Tempo";
		gbCalcTempo.Controls.AddRange(new Control[] {
			lblTempo,
			txtTempo,
			chkDoubleTempo,
			chkMonitorRegion,
			btnCalcTempo});
			
		lblTempo.Size = new Size(50, 20);
		lblTempo.Location = new Point(10, 20);
		lblTempo.Text = "&Tempo:";
		
		txtTempo.Size = new Size(60, 20);
		txtTempo.Location = new Point(60, 20);
		txtTempo.ReadOnly = true;
		
		chkDoubleTempo.Size = new Size(100, 20);
		chkDoubleTempo.Location = new Point(10, 60);
		chkDoubleTempo.Text = "&Double Tempo";
		chkDoubleTempo.Click += new EventHandler(chkDoubleTempo_Click);
		
		chkMonitorRegion.Size = new Size(120, 50);
		chkMonitorRegion.Location = new Point(10, 70);
		chkMonitorRegion.Text = "&Monitor Regions";
		chkMonitorRegion.Click += new EventHandler(chkMonitorRegion_Click);
		
		btnCalcTempo.Location = new Point(30, 130);
		btnCalcTempo.Text = "&Calculate";
		btnCalcTempo.Click += new EventHandler(btnCalcTempo_Click);
		
		Size = new Size(1000, 1000);
		Controls.AddRange(new Control[] {
			gbCalcTempo});
			
		Common.vegas.ProjectClosed += HandleProjectClosed;
		InitializeCalcTempoForm();
	}
	
	private void InitializeCalcTempoForm() {
		txtTempo.Text = "000.0000";
		chkDoubleTempo.Checked = false;
		chkMonitorRegion.Checked = false;
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
	
	void chkDoubleTempo_Click(object sender, EventArgs e) {
		double n;
		
		try {
			n = Convert.ToDouble(txtTempo.Text);
			if (n == 0) {
				throw new Exception("tempo is zero");
			}
		} catch (Exception ex) {
			return;
		}

		if (chkDoubleTempo.Checked) {
			txtTempo.Text = (n * 2).ToString("F4");
		} else {
			txtTempo.Text = (n / 2).ToString("F4");
		}
	}
	
	void chkMonitorRegion_Click(object sender, EventArgs e) {
			updateTempo();
	}
	
	void HandleProjectClosed(Object sender, EventArgs args) {
		InitializeCalcTempoForm();
	}
	
	//
	// Event handlers END
	//
	//
	////////////////////////////////////////////////////////////////////////////////
	
	private double calcTempo(Timecode length) {
		double tempo = 60.0 / (length.ToMilliseconds() / 1000.0);
		
		if (chkDoubleTempo.Checked) {
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

}

