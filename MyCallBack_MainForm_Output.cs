/*
 * Created by SharpDevelop.
 * User: Owner
 * Date: 2/21/2011
 * Time: 5:50 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ExpertMultimedia {
	/// <summary>
	/// Description of MyCallBack.
	/// </summary>
	public class MyCallBack {
		//public MainForm mainformNow=null;
		public MyCallBack() {
			
		}
		public void ShowMessage(string sLine) {
			ShowMessage(sLine,true);
		}
		public void ShowMessage(string sLine, bool bRefreshForm) {
			MainForm.Output( (sLine!=null&&sLine!="") ? sLine : "Unknown error received by MyCallBack ShowMessage", bRefreshForm );
		}
	}
}
