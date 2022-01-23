using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DicomFileReader
{
	public partial class Form1 : Form
	{
		static int lostTagCount = 0;
		char[] InvalidFileChars = Path.GetInvalidFileNameChars();
		char[] InvalidPathChars = Path.GetInvalidPathChars();
		const int BYTES_TO_READ = sizeof(Int64);

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.Text = "DICOM Processor v" + Application.ProductVersion;
#if DEBUG
			this.textBoxDICOMDirectory.Text = @"E:\TeamSystem\DicomFileReader\DicomFileReader\SampleDicomImages";
			this.textBoxOutputDirectory.Text = @"E:\TeamSystem\DicomFileReader\DicomFileReader\SampleDicomImages\Output";
#endif
		}

		private void buttonBrowseDirectory_Click(object sender, EventArgs e)
		{
			DialogResult result = folderBrowserDialog1.ShowDialog();
			TextBox target = ((Button)sender).Name.Contains("DICOM") ? this.textBoxDICOMDirectory : this.textBoxOutputDirectory;
			if (result == DialogResult.OK) { target.Text = folderBrowserDialog1.SelectedPath; }
		}

		private void buttonConvert_Click(object sender, EventArgs e)
		{
			if (textBoxDICOMDirectory.Text.Length == 0 || textBoxOutputDirectory.Text.Length == 0)
			{
				MessageBox.Show("Either Source or Destination directories not set. Cannot continue.", "Not all paths specified", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			if (!Directory.Exists(textBoxDICOMDirectory.Text))
			{
				MessageBox.Show("Source directory not found: " + textBoxDICOMDirectory.Text, "Invalid Source Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			if (!Directory.Exists(textBoxOutputDirectory.Text))
			{
				MessageBox.Show("Output directory not found: " + textBoxOutputDirectory.Text, "Invalid Output Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			labelProgressBar.BringToFront();
			labelProgressBar.Visible = true;
			progressBarDICOMFiles.BringToFront();
			progressBarDICOMFiles.Visible = true;

			buttonBrowseDICOMDirectory.Enabled = false;
			buttonBrowseOutputDirectory.Enabled = false;
			buttonConvert.Enabled = false;

			try { TraverseDirectoryTree(textBoxDICOMDirectory.Text); }

			finally
			{
				labelProgressBar.SendToBack();
				labelProgressBar.Visible = false;
				progressBarDICOMFiles.SendToBack();
				progressBarDICOMFiles.Visible = false;

				buttonBrowseDICOMDirectory.Enabled = true;
				buttonBrowseOutputDirectory.Enabled = true;
				buttonConvert.Enabled = true;
			}
		}

		private void TraverseDirectoryTree(string root)
		{
			StreamWriter MetadataTable = null;
			StreamWriter CountsTable = null;
			int errorCount = 0, copyCount = 0, deleteCount = 0, extractCount = 0, tableCount = 0;
			lostTagCount = 0;
			Stack<string> dirs = new Stack<string>(100);
			List<string> files = new List<string>();

			//Acquisition
			if (!Directory.Exists(root)) { throw new ArgumentException("Specified root directory doesn't exist: " + root); }
			dirs.Push(root);
			while (dirs.Count > 0)
			{
				string currentDir = dirs.Pop();

				string[] subDirs;
				try { subDirs = Directory.GetDirectories(currentDir); }
				catch (UnauthorizedAccessException) { continue; }
				catch (DirectoryNotFoundException) { continue; }
				foreach (string str in subDirs) { dirs.Push(str); }             // Push subdirectories on stack for traversal.

				string[] DICOMFiles = null;
				try { DICOMFiles = Directory.GetFiles(currentDir, "*.dcm"); }
				catch (UnauthorizedAccessException) { continue; }
				catch (DirectoryNotFoundException) { continue; }
				foreach (string file in DICOMFiles) { files.Add(file); }
			} // while dirs.Count > 0

			//Processing; could technically be split off into a separate fuuuuunctionnnnnn
			//but i don't give a shit
			bool DoSimpleParse = false;

			List<StudyObject> StudyFiles = new List<StudyObject>();

			int maxFiles = files.Count;
			for (int i = 0; i < maxFiles; i++)
			{
				float progress = ((float)(i + 1) / maxFiles);
				labelProgressBar.Text = string.Format("Processing file {0} of {1} ({2:p0})", i + 1, maxFiles, progress);
				progressBarDICOMFiles.Value = (int)Math.Floor(progress * 100);
				Application.DoEvents(); //keeps GUI alive by returning control to OS thread

				string DICOMFile = files[i];
				string NewFolderName, NewFileName, StudyTime, PatientName, StudyDate, PatientID;
				int nFrames;
				DICOMReader.File dcf = null;
				try
				{ //Take and open file, extract metadata
					dcf = new DICOMReader.File(DICOMFile, DoSimpleParse);
					PatientName = TagValueOrDefault(0x00100010, dcf.Tags);
					PatientID = TagValueOrDefault(0x00100020, dcf.Tags);
					StudyDate = TagValueOrDefault(0x00080023, dcf.Tags);
					StudyTime = TagValueOrDefault(0x00080033, dcf.Tags);
					if (!DoSimpleParse) { nFrames = int.Parse(TagValueOrDefault(0x00280008, dcf.Tags, "1")); }
					else { nFrames = ((new FileInfo(DICOMFile).Length / 1024) > 811) ? 125 : 1; } //single-image files are always 811KB big, so 811 * 1024 bytes; 125 frames is max and an assumption...

					//Images only
					if (nFrames > 1)
					{
						MessageBox.Show("Image file contains more than one frame, please select different image: " + nFrames.ToString(), "Invalid Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					NewFolderName = string.Format("{0}_{1}_{2}", StudyDate, PatientID, PatientName);
				}// try
				catch (FileNotFoundException) { errorCount++; continue; }


				StudyObject obj = new StudyObject();
				string[] FolderSplit = DICOMFile.Split(Path.DirectorySeparatorChar);
				obj.Folder = string.Join(Path.DirectorySeparatorChar.ToString(), FolderSplit.Take(FolderSplit.Length - 1));
				obj.File = FolderSplit.Skip(FolderSplit.Length - 1).ToArray()[0];
				obj.PatientID = PatientID;
				obj.PatientName = PatientName;
				obj.StudyDate = StudyDate;
				obj.StudyTime = StudyTime;
				obj.nFrames = (short)nFrames;
				obj.NewFolder = NewFolderName;
				StudyFiles.Add(obj);

				DirectoryInfo OutputFolder = new DirectoryInfo(Path.Combine(textBoxOutputDirectory.Text, NewFolderName));
				var OutputFolderFullName = OutputFolder.FullName.Replace(" ", "");

				//prepare receiving folder
				if (!Directory.Exists(OutputFolderFullName))
				{
					try { Directory.CreateDirectory(OutputFolderFullName); }
					catch (UnauthorizedAccessException)
					{
						MessageBox.Show("Cannot gain access to " + textBoxOutputDirectory.Text + ". Please select another directory or run as administrator.");
						return;
					}
				}//if FolderName doesn't exist


				//ExtractMetadata
				using (StreamWriter metadataFile = new StreamWriter(Path.Combine(OutputFolderFullName, "metadata.txt"), true))
				{
					metadataFile.WriteLine("Metadata extracted from " + DICOMFile + "\nTag\tValue");
					foreach (DICOMReader.Tag tag in dcf.Tags) { metadataFile.WriteLine(tag.Name + "\t" + tag.Value); } //TODO something fucky here.
					metadataFile.WriteLine("\n");
					extractCount++;
				}
			}//foreach DICOMFile

			//ExtractMetadataTable
			MetadataTable = new StreamWriter(Path.Combine(textBoxOutputDirectory.Text, DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + "_DICOMFilesMetadata-Table.txt"), false);
			MetadataTable.WriteLine("Filename\tPatient ID\tPatient Name\tStudy Date\tStudy Time\tData Type\tFrames");
			foreach (var obj in StudyFiles)
			{
				MetadataTable.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", Path.Combine(obj.Folder, obj.File), obj.PatientID, obj.PatientName, obj.StudyDate, obj.StudyTime, (obj.nFrames > 1 ? "Movie" : "Image"), obj.nFrames));
				tableCount++;
			}
			MetadataTable.Flush();
			MetadataTable.Close();


			//Generating success message to user
			string Report = "Operation(s) completed successfully\n";
			string LostTagReport = lostTagCount > 0 ? lostTagCount + " values could not found and defaulted to 'undefined'.\n" : "";
			//ExtractMetadata
			Report += "Metadata extracted for " + extractCount + " files. " + LostTagReport;
			//ExtractMetadataStudyTable
			Report += tableCount + " files processed for metadata table. " + LostTagReport; 
			progressBarDICOMFiles.Value = 100;
			Application.DoEvents();
			MessageBox.Show(Report, "DICOM processing complete.");
		} // TraverseDirectoryTree

		private void Form1_HelpButtonClicked(object sender, CancelEventArgs e)
		{
			MessageBox.Show("DICOM Processor Help", "DICOM Processor was created and optimised for use with the specific model of GE VIVID7 Echocardiograph stationed in the University of Manchester." +
				"There is no warranty, express or implied. Use at your own risk. Back up your data regularly.");
		}

		private string TagValueOrDefault(int TagKey, List<DICOMReader.Tag> SearchSpace, string defaultValue = "Undefined")
		{
			try
			{
				DICOMReader.Tag tag = SearchSpace.Find(n => n.GroupElement == TagKey);
				return tag != null ? tag.Value : defaultValue;
			}
			catch
			{
				if (TagKey != 0x00280008) { lostTagCount++; } //we expect not to see Number of Frames in Image Files (single frame, but not listed, because why have a consistent format???)
				return defaultValue;
			}
		}

		private bool TestFileEquality(FileInfo file1, FileInfo file2)
		{
			if (file1.Length != file2.Length) { return false; }
			int iterations = (int)Math.Ceiling((double)file1.Length / BYTES_TO_READ);
			using (FileStream fs1 = file1.OpenRead(), fs2 = file2.OpenRead())
			{
				byte[] one = new byte[BYTES_TO_READ];
				byte[] two = new byte[BYTES_TO_READ];
				for (int i = 0; i < iterations; i++)
				{
					fs1.Read(one, 0, BYTES_TO_READ);
					fs2.Read(two, 0, BYTES_TO_READ);
					if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0)) { return false; }//tripped at the first mismatch
				}// for iterations
			}//using FileStream fs1 and FileStream fs2
			return true; //files are of equal length, all iterations have given the same value for both files - they are identical
		} //bool TestFileEquality
	} //class Form 1
} // namespace DicomFileReader

