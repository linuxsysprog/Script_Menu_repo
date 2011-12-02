// File: MyScript.cs

using System.Windows.Forms;
using Sony.Vegas;
using UtilityMethods;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
        MessageBox.Show("" + AddClass.Add(2, 3));
    }
}
