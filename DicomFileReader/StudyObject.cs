using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomFileReader
{
	internal struct StudyObject
	{
		internal string Folder;
		internal string File;
		internal string PatientName;
		internal string PatientID;
		internal string StudyDate;
		internal string StudyTime;
		internal short nFrames;
		internal string NewFolder;
	}
}
