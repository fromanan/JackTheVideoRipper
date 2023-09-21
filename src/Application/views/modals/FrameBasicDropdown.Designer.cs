namespace JackTheVideoRipper.views.modals
{
   partial class FrameBasicDropdown
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.comboBox = new System.Windows.Forms.ComboBox();
         this.MainLabel = new System.Windows.Forms.Label();
         this.buttonCancel = new System.Windows.Forms.Button();
         this.buttonConfirm = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // comboBox
         // 
         this.comboBox.FormattingEnabled = true;
         this.comboBox.Location = new System.Drawing.Point(11, 48);
         this.comboBox.Name = "comboBox";
         this.comboBox.Size = new System.Drawing.Size(185, 28);
         this.comboBox.TabIndex = 0;
         // 
         // MainLabel
         // 
         this.MainLabel.BackColor = System.Drawing.SystemColors.ControlLight;
         this.MainLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
         this.MainLabel.Location = new System.Drawing.Point(11, 9);
         this.MainLabel.Name = "MainLabel";
         this.MainLabel.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
         this.MainLabel.Size = new System.Drawing.Size(185, 32);
         this.MainLabel.TabIndex = 14;
         this.MainLabel.Text = "Please select a value";
         // 
         // buttonCancel
         // 
         this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.buttonCancel.Location = new System.Drawing.Point(11, 83);
         this.buttonCancel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
         this.buttonCancel.Name = "buttonCancel";
         this.buttonCancel.Size = new System.Drawing.Size(91, 37);
         this.buttonCancel.TabIndex = 15;
         this.buttonCancel.Text = "Cancel";
         this.buttonCancel.UseVisualStyleBackColor = true;
         // 
         // buttonConfirm
         // 
         this.buttonConfirm.Location = new System.Drawing.Point(106, 83);
         this.buttonConfirm.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
         this.buttonConfirm.Name = "buttonConfirm";
         this.buttonConfirm.Size = new System.Drawing.Size(91, 37);
         this.buttonConfirm.TabIndex = 16;
         this.buttonConfirm.Text = "Confirm";
         this.buttonConfirm.UseVisualStyleBackColor = true;
         // 
         // FrameBasicDropdown
         // 
         this.AcceptButton = this.buttonConfirm;
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
         this.CancelButton = this.buttonCancel;
         this.ClientSize = new System.Drawing.Size(210, 135);
         this.ControlBox = false;
         this.Controls.Add(this.buttonConfirm);
         this.Controls.Add(this.buttonCancel);
         this.Controls.Add(this.MainLabel);
         this.Controls.Add(this.comboBox);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Name = "FrameBasicDropdown";
         this.ShowIcon = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "BasicDropdown";
         this.TopMost = true;
         this.ResumeLayout(false);

      }

      #endregion

      private ComboBox comboBox;
      private Label MainLabel;
      private Button buttonCancel;
      private Button buttonConfirm;
   }
}