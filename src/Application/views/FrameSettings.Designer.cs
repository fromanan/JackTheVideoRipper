namespace JackTheVideoRipper
{
   partial class FrameSettings
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
         buttonCancel = new Button();
         buttonSave = new Button();
         groupBox2 = new GroupBox();
         buttonLocationBrowse = new Button();
         textLocation = new TextBox();
         label1 = new Label();
         groupBox1 = new GroupBox();
         skipMetadata = new CheckBox();
         enableMultithreadedDownloads = new CheckBox();
         enableDeveloperMode = new CheckBox();
         storeHistory = new CheckBox();
         simplifiedDownload = new CheckBox();
         numMaxConcurrent = new NumericUpDown();
         groupBox3 = new GroupBox();
         buttonTempFolderBrowse = new Button();
         textTempFolder = new TextBox();
         groupBox2.SuspendLayout();
         groupBox1.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)numMaxConcurrent).BeginInit();
         groupBox3.SuspendLayout();
         SuspendLayout();
         // 
         // buttonCancel
         // 
         buttonCancel.DialogResult = DialogResult.Cancel;
         buttonCancel.FlatStyle = FlatStyle.System;
         buttonCancel.Location = new Point(211, 424);
         buttonCancel.Name = "buttonCancel";
         buttonCancel.Size = new Size(114, 37);
         buttonCancel.TabIndex = 15;
         buttonCancel.Text = "Cancel";
         buttonCancel.UseVisualStyleBackColor = true;
         // 
         // buttonSave
         // 
         buttonSave.FlatStyle = FlatStyle.System;
         buttonSave.Location = new Point(331, 424);
         buttonSave.Name = "buttonSave";
         buttonSave.Size = new Size(114, 37);
         buttonSave.TabIndex = 14;
         buttonSave.Text = "Save";
         buttonSave.UseVisualStyleBackColor = true;
         // 
         // groupBox2
         // 
         groupBox2.BackColor = SystemColors.ControlLightLight;
         groupBox2.Controls.Add(buttonLocationBrowse);
         groupBox2.Controls.Add(textLocation);
         groupBox2.Location = new Point(8, 236);
         groupBox2.Name = "groupBox2";
         groupBox2.Size = new Size(437, 88);
         groupBox2.TabIndex = 16;
         groupBox2.TabStop = false;
         groupBox2.Text = "Default Save Location";
         // 
         // buttonLocationBrowse
         // 
         buttonLocationBrowse.FlatStyle = FlatStyle.System;
         buttonLocationBrowse.Location = new Point(356, 31);
         buttonLocationBrowse.Name = "buttonLocationBrowse";
         buttonLocationBrowse.Size = new Size(78, 37);
         buttonLocationBrowse.TabIndex = 1;
         buttonLocationBrowse.Text = "Browse";
         buttonLocationBrowse.UseVisualStyleBackColor = true;
         // 
         // textLocation
         // 
         textLocation.Location = new Point(13, 36);
         textLocation.Name = "textLocation";
         textLocation.ReadOnly = true;
         textLocation.Size = new Size(337, 27);
         textLocation.TabIndex = 0;
         // 
         // label1
         // 
         label1.AutoSize = true;
         label1.Location = new Point(13, 29);
         label1.Margin = new Padding(2, 0, 2, 0);
         label1.Name = "label1";
         label1.Size = new Size(195, 20);
         label1.TabIndex = 17;
         label1.Text = "Max Concurrent Downloads:";
         // 
         // groupBox1
         // 
         groupBox1.Controls.Add(skipMetadata);
         groupBox1.Controls.Add(enableMultithreadedDownloads);
         groupBox1.Controls.Add(enableDeveloperMode);
         groupBox1.Controls.Add(storeHistory);
         groupBox1.Controls.Add(simplifiedDownload);
         groupBox1.Controls.Add(numMaxConcurrent);
         groupBox1.Controls.Add(label1);
         groupBox1.Location = new Point(8, 15);
         groupBox1.Margin = new Padding(2, 3, 2, 3);
         groupBox1.Name = "groupBox1";
         groupBox1.Padding = new Padding(2, 3, 2, 3);
         groupBox1.Size = new Size(437, 215);
         groupBox1.TabIndex = 18;
         groupBox1.TabStop = false;
         groupBox1.Text = "General";
         // 
         // skipMetadata
         // 
         skipMetadata.AutoSize = true;
         skipMetadata.Location = new Point(16, 91);
         skipMetadata.Name = "skipMetadata";
         skipMetadata.Size = new Size(284, 24);
         skipMetadata.TabIndex = 23;
         skipMetadata.Text = "Skip Metadata Preview for Downloads";
         skipMetadata.UseVisualStyleBackColor = true;
         // 
         // enableMultithreadedDownloads
         // 
         enableMultithreadedDownloads.AutoSize = true;
         enableMultithreadedDownloads.Location = new Point(16, 181);
         enableMultithreadedDownloads.Name = "enableMultithreadedDownloads";
         enableMultithreadedDownloads.Size = new Size(253, 24);
         enableMultithreadedDownloads.TabIndex = 22;
         enableMultithreadedDownloads.Text = "Enable Multithreaded Downloads";
         enableMultithreadedDownloads.UseVisualStyleBackColor = true;
         // 
         // enableDeveloperMode
         // 
         enableDeveloperMode.AutoSize = true;
         enableDeveloperMode.Location = new Point(16, 151);
         enableDeveloperMode.Name = "enableDeveloperMode";
         enableDeveloperMode.Size = new Size(192, 24);
         enableDeveloperMode.TabIndex = 21;
         enableDeveloperMode.Text = "Enable Developer Mode";
         enableDeveloperMode.UseVisualStyleBackColor = true;
         // 
         // storeHistory
         // 
         storeHistory.AutoSize = true;
         storeHistory.Location = new Point(16, 121);
         storeHistory.Name = "storeHistory";
         storeHistory.Size = new Size(117, 24);
         storeHistory.TabIndex = 20;
         storeHistory.Text = "Store History";
         storeHistory.UseVisualStyleBackColor = true;
         // 
         // simplifiedDownload
         // 
         simplifiedDownload.AutoSize = true;
         simplifiedDownload.Location = new Point(16, 61);
         simplifiedDownload.Name = "simplifiedDownload";
         simplifiedDownload.Size = new Size(208, 24);
         simplifiedDownload.TabIndex = 19;
         simplifiedDownload.Text = "Simplified Download View";
         simplifiedDownload.UseVisualStyleBackColor = true;
         // 
         // numMaxConcurrent
         // 
         numMaxConcurrent.Location = new Point(213, 28);
         numMaxConcurrent.Margin = new Padding(2, 3, 2, 3);
         numMaxConcurrent.Name = "numMaxConcurrent";
         numMaxConcurrent.Size = new Size(80, 27);
         numMaxConcurrent.TabIndex = 18;
         // 
         // groupBox3
         // 
         groupBox3.Controls.Add(buttonTempFolderBrowse);
         groupBox3.Controls.Add(textTempFolder);
         groupBox3.Location = new Point(8, 330);
         groupBox3.Name = "groupBox3";
         groupBox3.Size = new Size(437, 88);
         groupBox3.TabIndex = 17;
         groupBox3.TabStop = false;
         groupBox3.Text = "Temp Folder";
         // 
         // buttonTempFolderBrowse
         // 
         buttonTempFolderBrowse.FlatStyle = FlatStyle.System;
         buttonTempFolderBrowse.Location = new Point(356, 31);
         buttonTempFolderBrowse.Name = "buttonTempFolderBrowse";
         buttonTempFolderBrowse.Size = new Size(78, 37);
         buttonTempFolderBrowse.TabIndex = 1;
         buttonTempFolderBrowse.Text = "Browse";
         buttonTempFolderBrowse.UseVisualStyleBackColor = true;
         // 
         // textTempFolder
         // 
         textTempFolder.Location = new Point(13, 36);
         textTempFolder.Name = "textTempFolder";
         textTempFolder.ReadOnly = true;
         textTempFolder.Size = new Size(337, 27);
         textTempFolder.TabIndex = 0;
         // 
         // FrameSettings
         // 
         AcceptButton = buttonSave;
         AutoScaleDimensions = new SizeF(8F, 20F);
         AutoScaleMode = AutoScaleMode.Font;
         CancelButton = buttonCancel;
         ClientSize = new Size(457, 472);
         Controls.Add(groupBox3);
         Controls.Add(groupBox1);
         Controls.Add(groupBox2);
         Controls.Add(buttonCancel);
         Controls.Add(buttonSave);
         FormBorderStyle = FormBorderStyle.FixedDialog;
         Margin = new Padding(2, 3, 2, 3);
         MaximizeBox = false;
         MinimizeBox = false;
         Name = "FrameSettings";
         ShowInTaskbar = false;
         StartPosition = FormStartPosition.CenterParent;
         Text = "Settings";
         Load += FrameSettings_Load;
         groupBox2.ResumeLayout(false);
         groupBox2.PerformLayout();
         groupBox1.ResumeLayout(false);
         groupBox1.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)numMaxConcurrent).EndInit();
         groupBox3.ResumeLayout(false);
         groupBox3.PerformLayout();
         ResumeLayout(false);
      }

      #endregion

      private System.Windows.Forms.Button buttonCancel;
      private System.Windows.Forms.Button buttonSave;
      private System.Windows.Forms.GroupBox groupBox2;
      private System.Windows.Forms.Button buttonLocationBrowse;
      private System.Windows.Forms.TextBox textLocation;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.NumericUpDown numMaxConcurrent;
      private CheckBox simplifiedDownload;
      private CheckBox storeHistory;
      private CheckBox enableDeveloperMode;
      private CheckBox enableMultithreadedDownloads;
      private GroupBox groupBox3;
      private Button buttonTempFolderBrowse;
      private TextBox textTempFolder;
      private CheckBox skipMetadata;
   }
}