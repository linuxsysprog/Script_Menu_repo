// File: MyScript.cs

using System.Windows.Forms;
using Sony.Vegas;
using UtilityMethods;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
		// 
		System.Diagnostics.Debug.WriteLine("elmo FromVegas() Entry.");
		
        MessageBox.Show("" + AddClass.Add(2, 3));
        MessageBox.Show(Sony.Vegas.Script.File);
    }
}
