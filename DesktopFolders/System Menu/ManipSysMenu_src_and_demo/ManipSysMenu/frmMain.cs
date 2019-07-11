using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace ManipSysMenu
{
	/// <summary>
	/// Zusammendfassende Beschreibung für Form1.
	/// </summary>
	public class frmMain : System.Windows.Forms.Form
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmMain()
		{
			//
			// Erforderlich für die Windows Form-Designerunterstützung
			//
			InitializeComponent();

			//
			// TODO: Fügen Sie den Konstruktorcode nach dem Aufruf von InitializeComponent hinzu
			//
		}

		/// <summary>
		/// Die verwendeten Ressourcen bereinigen.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			this.lblInfo = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblInfo
			// 
			this.lblInfo.Location = new System.Drawing.Point(24, 24);
			this.lblInfo.Name = "lblInfo";
			this.lblInfo.Size = new System.Drawing.Size(312, 48);
			this.lblInfo.TabIndex = 0;
			this.lblInfo.Text = "This example shows how to manipulate the Systemmenu using C#. Just click the wind" +
				"ow icon or right click the window title and you will see the result.";
			// 
			// frmMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(360, 86);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.lblInfo});
			this.Name = "frmMain";
			this.Text = "Manipulating the Systemmenu using C#";
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.ResumeLayout(false);

		}
		#endregion
		
		private SystemMenu m_SystemMenu = null;

		// ID's of the entries
		private const int m_AboutID		= 0x100;
		private System.Windows.Forms.Label lblInfo;
		private const int m_ResetID		= 0x101;

		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new frmMain());
		}

		protected override void WndProc ( ref Message msg )
		{
			// Now we should catch the WM_SYSCOMMAND message
			// and process it.

			// We override the WndProc to catch the WM_SYSCOMMAND message.
			// You don't have to look this message up in the MSDN it is defined
			// in WindowMessages enumeration.
			// The WParam of the message contains the ID which was pressed. It is the
			// same value you have passed through InsertMenu() or AppendMenu() member
			// functions of my class.

			// Just check for them and do the proper action.
			//
			if ( msg.Msg == (int)WindowMessages.wmSysCommand )
			{
				switch ( msg.WParam.ToInt32() )
				{
					case m_ResetID:
						{
							if ( MessageBox.Show(this, "\tAre you sure?", "Question", MessageBoxButtons.YesNo) == DialogResult.Yes )
							{ // Reset the Systemmenu
								SystemMenu.ResetSystemMenu(this);
							}
						}
						break;

					case m_AboutID:
						{ // Our about id
							MessageBox.Show(this, "Written by Florian \"nohero\"" +
								                  "Stinglmayr.\nEMail: nohero@coding.at", "About");
						} 
						break;

					// TODO: Add more handles, for more menu items

					default:
						{ // Do nothing
						} break;
				}
			}
			// Call base class function
			base.WndProc(ref msg);
		}

		private void frmMain_Load(object sender, System.EventArgs e)
		{
			try
			{
				m_SystemMenu = SystemMenu.FromForm(this);	
				// Add a separator ...
				m_SystemMenu.AppendSeparator();
				// ... and an "About" entry
				m_SystemMenu.AppendMenu(m_AboutID, "About this...");
				// And a "Reset" item on top
				m_SystemMenu.InsertSeparator(0);
				m_SystemMenu.InsertMenu(0, m_ResetID, "Reset Systemmenu");
			}
			catch ( NoSystemMenuException /* err */ )
			{ 
				// Do some error handling
			}
		}
	}
}
