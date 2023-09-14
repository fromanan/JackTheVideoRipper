namespace JackCrashHandler
{
   partial class FrameCrashHandler
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
         label1 = new Label();
         pictureBox1 = new PictureBox();
         ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
         SuspendLayout();
         // 
         // label1
         // 
         label1.AutoSize = true;
         label1.Font = new Font("Calibri", 12F, FontStyle.Bold, GraphicsUnit.Point);
         label1.Location = new Point(74, 20);
         label1.Name = "label1";
         label1.Size = new Size(351, 48);
         label1.TabIndex = 0;
         label1.Text = "Please wait while we collect information\r\nabout the crash...";
         label1.TextAlign = ContentAlignment.MiddleCenter;
         // 
         // pictureBox1
         // 
         pictureBox1.Dock = DockStyle.Fill;
         pictureBox1.Image = Properties.Resources.Spinner;
         pictureBox1.Location = new Point(0, 0);
         pictureBox1.Name = "pictureBox1";
         pictureBox1.Size = new Size(494, 253);
         pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
         pictureBox1.TabIndex = 1;
         pictureBox1.TabStop = false;
         // 
         // FrameCrashHandler
         // 
         AutoScaleDimensions = new SizeF(8F, 20F);
         AutoScaleMode = AutoScaleMode.Font;
         ClientSize = new Size(494, 253);
         Controls.Add(label1);
         Controls.Add(pictureBox1);
         Name = "FrameCrashHandler";
         ShowIcon = false;
         Text = "Crash Handler";
         ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
         ResumeLayout(false);
         PerformLayout();
      }

      #endregion

      private Label label1;
      private PictureBox pictureBox1;
   }
}