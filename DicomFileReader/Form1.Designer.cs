namespace DicomFileReader
{
    partial class Form1
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
            this.label2 = new System.Windows.Forms.Label();
            this.buttonConvert = new System.Windows.Forms.Button();
            this.labelProgressBar = new System.Windows.Forms.Label();
            this.progressBarDICOMFiles = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxDICOMDirectory = new System.Windows.Forms.TextBox();
            this.textBoxOutputDirectory = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonBrowseOutputDirectory = new System.Windows.Forms.Button();
            this.buttonBrowseDICOMDirectory = new System.Windows.Forms.Button();
            this.panelProgressBar = new System.Windows.Forms.Panel();
            this.panelProgressBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Output Directory:";
            // 
            // buttonConvert
            // 
            this.buttonConvert.Location = new System.Drawing.Point(251, 116);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(75, 23);
            this.buttonConvert.TabIndex = 6;
            this.buttonConvert.Text = "Convert";
            this.buttonConvert.UseVisualStyleBackColor = true;
            this.buttonConvert.Click += new System.EventHandler(this.buttonConvert_Click);
            // 
            // labelProgressBar
            // 
            this.labelProgressBar.AutoSize = true;
            this.labelProgressBar.Location = new System.Drawing.Point(234, 100);
            this.labelProgressBar.Name = "labelProgressBar";
            this.labelProgressBar.Size = new System.Drawing.Size(137, 13);
            this.labelProgressBar.TabIndex = 1;
            this.labelProgressBar.Text = "Processing file 0 of 0 (0%)...";
            this.labelProgressBar.Visible = false;
            // 
            // progressBarDICOMFiles
            // 
            this.progressBarDICOMFiles.Location = new System.Drawing.Point(149, 67);
            this.progressBarDICOMFiles.Name = "progressBarDICOMFiles";
            this.progressBarDICOMFiles.Size = new System.Drawing.Size(307, 23);
            this.progressBarDICOMFiles.TabIndex = 0;
            this.progressBarDICOMFiles.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "DICOM Directory:";
            // 
            // textBoxDICOMDirectory
            // 
            this.textBoxDICOMDirectory.Location = new System.Drawing.Point(99, 9);
            this.textBoxDICOMDirectory.Name = "textBoxDICOMDirectory";
            this.textBoxDICOMDirectory.Size = new System.Drawing.Size(400, 20);
            this.textBoxDICOMDirectory.TabIndex = 15;
            // 
            // textBoxOutputDirectory
            // 
            this.textBoxOutputDirectory.Location = new System.Drawing.Point(99, 36);
            this.textBoxOutputDirectory.Name = "textBoxOutputDirectory";
            this.textBoxOutputDirectory.Size = new System.Drawing.Size(400, 20);
            this.textBoxOutputDirectory.TabIndex = 14;
            // 
            // buttonBrowseOutputDirectory
            // 
            this.buttonBrowseOutputDirectory.Location = new System.Drawing.Point(505, 35);
            this.buttonBrowseOutputDirectory.Name = "buttonBrowseOutputDirectory";
            this.buttonBrowseOutputDirectory.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseOutputDirectory.TabIndex = 13;
            this.buttonBrowseOutputDirectory.Text = "Browse...";
            this.buttonBrowseOutputDirectory.UseVisualStyleBackColor = true;
            this.buttonBrowseOutputDirectory.Click += new System.EventHandler(this.buttonBrowseDirectory_Click);
            // 
            // buttonBrowseDICOMDirectory
            // 
            this.buttonBrowseDICOMDirectory.Location = new System.Drawing.Point(505, 8);
            this.buttonBrowseDICOMDirectory.Name = "buttonBrowseDICOMDirectory";
            this.buttonBrowseDICOMDirectory.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseDICOMDirectory.TabIndex = 12;
            this.buttonBrowseDICOMDirectory.Text = "Browse...";
            this.buttonBrowseDICOMDirectory.UseVisualStyleBackColor = true;
            this.buttonBrowseDICOMDirectory.Click += new System.EventHandler(this.buttonBrowseDirectory_Click);
            // 
            // panelProgressBar
            // 
            this.panelProgressBar.Controls.Add(this.label2);
            this.panelProgressBar.Controls.Add(this.buttonConvert);
            this.panelProgressBar.Controls.Add(this.labelProgressBar);
            this.panelProgressBar.Controls.Add(this.progressBarDICOMFiles);
            this.panelProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProgressBar.Location = new System.Drawing.Point(0, 0);
            this.panelProgressBar.Name = "panelProgressBar";
            this.panelProgressBar.Size = new System.Drawing.Size(800, 450);
            this.panelProgressBar.TabIndex = 17;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxDICOMDirectory);
            this.Controls.Add(this.textBoxOutputDirectory);
            this.Controls.Add(this.buttonBrowseOutputDirectory);
            this.Controls.Add(this.buttonBrowseDICOMDirectory);
            this.Controls.Add(this.panelProgressBar);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panelProgressBar.ResumeLayout(false);
            this.panelProgressBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonConvert;
        private System.Windows.Forms.Label labelProgressBar;
        private System.Windows.Forms.ProgressBar progressBarDICOMFiles;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDICOMDirectory;
        private System.Windows.Forms.TextBox textBoxOutputDirectory;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button buttonBrowseOutputDirectory;
        private System.Windows.Forms.Button buttonBrowseDICOMDirectory;
        private System.Windows.Forms.Panel panelProgressBar;
    }
}

