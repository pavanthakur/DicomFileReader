using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

namespace DicomFileReader
{
	static class DICOMReader
	{
		const string ImplicitVRLittleEndian = "1.2.840.10008.1.2";
		const string ExplicitVRLittleEndian = "1.2.840.10008.1.2.1";
		const string ExplicitVRBigEndian = "1.2.840.10008.1.2.2";

		static int PREAMBLE_SIZE = 128;
		static string[] TwoByteVRCodes = new string[] { "OB", "OD", "OF", "OL", "OW", "SQ", "UC", "UR", "UT", "UN" };
		static uint[] SequenceGETypes = new uint[] { };

		internal class SequenceItem
		{
			internal readonly uint GroupElement;
			internal readonly string Name = "(Undefined)";
			private readonly string VR;         //Value Representation
			private readonly int ValueLength;   //Value Representations of UC, UR and UT may not have an Undefined Length, i.e.,a Value Length of FFFFFFFFH.
		}

		internal class Tag
		{
			internal readonly uint GroupElement;
			internal readonly string Name = "(Undefined)";
			private readonly string VR;         //Value Representation
			private readonly int ValueLength;   //Value Representations of UC, UR and UT may not have an Undefined Length, i.e.,a Value Length of FFFFFFFFH.
												//Value lengths are always even and if necessary are padded with a space (0x20) if a string or else a binary 0 (0x00)
			internal byte[] _Value;             //Object or String?
			internal string Value = null;
			internal int ValueMultiplicity = 1;

			public Tag(uint GE, int ValueLength, string Value, byte[] _Value, string VR = "IMPLICIT")
			{
				GroupElement = GE;
				this.ValueLength = ValueLength;
				this.Value = Value;
				this._Value = _Value;
				this.VR = VR;
			}

			//default constructor
			public Tag(DICOMReader.File Dicomfile)
			{
				GroupElement = (uint)((Dicomfile.EASR.GetUShort() << 16) | Dicomfile.EASR.GetUShort()); //grab two Ushorts, mash together into GroupElement
				Console.WriteLine("* Reading element 0x{0:x8}", GroupElement);

				if (DicomTagLookup.Tags.ContainsKey(GroupElement))
				{
					Name = DicomTagLookup.Tags[GroupElement];
					Console.WriteLine("* Element identified as \"{0}\"", Name);
				}

				if (Dicomfile.TransferSyntax == TransferSyntax.ExplicitVR)
				{
					//If VR is explicit (according to transfer syntax agreed upon in file header), there's two different types of possible encoding (Table 7.1-1 and 7.1-2)
					VR = Dicomfile.EASR.GetString(2);
					if (TwoByteVRCodes.Contains(VR))
					{               //some VR types encode themselves differently and need two bytes of spacing after the actual value, for SOME reason.
						Dicomfile.EASR.GetUShort();                 //Advance by two bytes (those two bytes are padding set to 0x000, thus they're discarded)
																	//Those bytes have to be discarded; if you just did a ReadInt32() you'd get a wrong number... right?
						ValueLength = Dicomfile.EASR.GetInt();  //Value length field is 4 bytes long for these VR codes
					}
					else { ValueLength = Dicomfile.EASR.GetUShort(); } //Value length is a simple 2 byte integer for all other VR codes 
				}//if TransferSyntax == TransferSyntax.ExplicitVR

				if (Dicomfile.TransferSyntax == TransferSyntax.ImplicitVR)
				{
					//If VR is implicit (according to transfer syntax agreed upon in file header), there's just one possible encoding (Table 7.1-3)
					VR = "IMPLICIT";
					ValueLength = Dicomfile.EASR.GetInt();  //Value length field is 4 bytes long for these VR codes
				}

				if (GroupElement == 0xFFFEE0DD && ValueLength == 0)
				{           //Sequence Delimination tag (not item delimination!) found, e.g. a sequence has ended
					Dicomfile.Tags.Add(new Tag(GroupElement, -1, "Sequence End", new byte[0]));
					return;
				} //if (SubTag is Sequence End Tag)

				if (Dicomfile.TransferSyntax == TransferSyntax.ExplicitVR && VR == "SQ")
				{ //|| (Dicomfile.TransferSyntax == TransferSyntax.ImplicitVR && DICOMDataDictionary.SequenceItems.Keys.Contains(GroupElement))
					Dicomfile.Tags.Add(new Tag(this.GroupElement, this.ValueLength, "Sequence Start", new byte[0]));
					if (this.ValueLength > 0)
					{
						Dicomfile.EASR.BaseStream.Seek(this.ValueLength, SeekOrigin.Current);
						Dicomfile.Tags.Add(new Tag(this.GroupElement, 0, "Sequence End", new byte[0]));
						return;
					} //skip this data. TODO: Do proper implementation, see Table 7.5-1
					else
					{
						//this is very hack-y and I apologise
						ushort LastUS2 = 0;
						while (true)
						{
							ushort us1 = Dicomfile.EASR.GetUShort();
							ushort us2 = Dicomfile.EASR.GetUShort();
							uint GE1 = (uint)((us1 << 16) | us2);
							uint GE2 = (uint)((LastUS2 << 16) | us1);
							if (GE1 == 0xFFFEE0DD || GE2 == 0xFFFEE0DD)
							{
								Dicomfile.EASR.GetInt();
								return;
							} //0xFFFEE0DD is the Sequence Delimiter, followed by 0x00000000 (thus the GetInt())
							else { LastUS2 = us2; }
						}
					}
				}// if VR == SQ and transfer syntax == Explicit

				if (Name == "Pixel Data") { return; } //again, stop reading in a bajillion pixels.

				if (ValueLength == -1)
				{
					ValueLength = 0;
					Dicomfile.Tags.Add(this);
					return;
				}

				if (ValueLength % 2 != 0) { throw new ArgumentException("Value Length must be an even number?!"); }
				_Value = Dicomfile.EASR.ReadBytes(ValueLength);
				Value = System.Text.Encoding.UTF8.GetString(_Value);
				DateTime ConversionDummy = new DateTime();
				switch (VR)
				{
					//---Important ones:---
					case "DA": //date, 8b fixed, YYYYMMDD
						if (DateTime.TryParseExact(Value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out ConversionDummy)) { Value = ConversionDummy.ToString("yyyy-MM-dd"); }
						break;

					case "DT": //DateTime 26b max, YYYYMMDDHHMMSS.FFFFFF&ZZXX
						if (DateTime.TryParseExact(Value, "yyyyMMddHHmmss.ffffff", CultureInfo.InvariantCulture, DateTimeStyles.None, out ConversionDummy)) { Value = ConversionDummy.ToString("yyyy-MM-dd_HH-mm"); }
						break;

					case "TM": //Time
						string format = Value.Contains('.') ? "HHmmss.ffffff" : "HHmmss";
						if (DateTime.TryParseExact(Value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out ConversionDummy)) { Value = ConversionDummy.ToString("HH-mm-ss"); }
						break;

					case "FL":
						if (ValueLength == 4) { Value = System.BitConverter.ToSingle(_Value, 0).ToString(); }
						break;

					case "FD":
						if (ValueLength == 8) { Value = System.BitConverter.ToDouble(_Value, 0).ToString(); }
						break;

					case "SS":
						if (ValueLength == 2) { Value = System.BitConverter.ToInt16(_Value, 0).ToString(); }
						break;

					case "US":
						if (ValueLength == 2) { Value = System.BitConverter.ToUInt16(_Value, 0).ToString(); }
						break;

					case "SL":
						if (ValueLength == 4) { Value = System.BitConverter.ToInt32(_Value, 0).ToString(); }
						break;

					case "UL":
						if (ValueLength == 4) { Value = System.BitConverter.ToUInt32(_Value, 0).ToString(); }
						break;

					//---values no one cares about---
					case "UI":
					case "UN":
					case "UR":
					case "UT":
					//Unique Identifier			Unknown			Universal Resource Locator	Unlimited text				
					case "CS":
					//Code string, 16b max, trim leading/trailing space [20]
					case "UC":
					case "SH":
					case "LO":
					//Unlimited Characters		short string	Long string	
					case "LT":
					case "OB":
					case "OD":
					case "OF":
					case "OL":
					//Long Text					Other Byte		Other Double	Other Float	Other Long
					case "OW":
					case "AT":
					case "DS":
					//Other Word				"Attribute Tag"	decimal string
					case "AE":
					case "ST":
					case "AS":
					//Application Entity		Short Text		Age String, 4 bytes fixed
					case "IS": //12b max, 0-9+-[space]
							   //int BufferInt;
							   //if (int.TryParse(Value, out BufferInt)) {  }
					case "PN": //Person Name 64char max times 5, = [3D] delimits groups
							   //Value = "PN Converted: " + System.Text.Encoding.UTF8.GetString(_Value);
					case "SQ": //Sequence of Items
							   //TODO do multiplicity = n, loop until next SQ (get length, read length bytes, repeat)
					default:
						break;
				}//switch VR
				Dicomfile.Tags.Add(this);
			}//public Tag

			public override string ToString() { return string.Format("(0x{0:X8})\t \"{1}\"\t {2}:{3}\t \"{4}\"\n", GroupElement, Name, VR, ValueLength, Value); }
		}//class Tag

		internal enum TransferSyntax { ExplicitVR, ImplicitVR, Undefined };

		internal class File
		{
			private static readonly IReadOnlyList<uint> NeededTags = new List<uint> { 0x00100010, 0x00100020, 0x00080023, 0x00080033 }; //These are PatientName, PatientID, StudyDate and StudyTime
			internal TransferSyntax TransferSyntax;
			Stream FileStream;
			System.IO.FileInfo _File;
			internal List<Tag> Tags = new List<Tag>();
			internal EndianAmbiguousStreamReader EASR;

			public File(string filename, bool DoSimpleParse = false)
			{
				_File = new System.IO.FileInfo(filename);
				if (!_File.Exists) { throw new FileNotFoundException("Cannot locate or open DICOM file.", filename); }
				TransferSyntax = DICOMReader.TransferSyntax.ExplicitVR;
				Load(DoSimpleParse);
			}

			internal void Load(bool DoSimpleParse)
			{
				FileStream = new FileStream(_File.FullName, FileMode.Open, FileAccess.Read);
				FileStream.Seek(PREAMBLE_SIZE, SeekOrigin.Begin);
				EASR = new LittleEndianStreamReader(FileStream);
				string FirstFour = EASR.GetString(4);
				if (FirstFour != "DICM") { throw new InvalidDataException("Not a valid DICOM File"); }
				while (true)
				{ //Keep reading until start of Pixel Data;  PIXEL_DATA = 0x 7FE0 0010;
					Tag tag = new Tag(this);
					if (tag.Name == "Pixel Data" || tag.GroupElement == 0x7FE00010) { break; }
					if (tag.Name == "TRANSFER_SYNTAX_UID")
					{
						switch (tag.Value)
						{
							case DICOMReader.ExplicitVRBigEndian:
								EASR = new BigEndianStreamReader(FileStream);
								TransferSyntax = DICOMReader.TransferSyntax.ExplicitVR;
								break;

							case DICOMReader.ExplicitVRLittleEndian:
								TransferSyntax = DICOMReader.TransferSyntax.ExplicitVR;
								break;

							case DICOMReader.ImplicitVRLittleEndian:
								TransferSyntax = DICOMReader.TransferSyntax.ImplicitVR;
								break;

							default:
								TransferSyntax = DICOMReader.TransferSyntax.Undefined;
								break;
						}
					}//switch tag.Name == TRANSFER_SYNTAX_UID
					if (DoSimpleParse)
					{
						uint[] FoundTags = Tags.Select(n => n.GroupElement).ToArray();
						if (NeededTags.All(x => FoundTags.Contains(x))) { break; } //All crucial tags are found so why keep parsing?
					}
				} //while true
				EASR.Close();
			} //void Load()
		} //class File 
	} //class DICOMReader

}
