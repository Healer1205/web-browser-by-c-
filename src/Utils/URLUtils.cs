using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBrowser {
	internal static class URLUtils {

		public static string PathToURL(this string filePath, string removeBaseDir = null) {

			if (!filePath.CheckIfValid()) {
				return "";
			}

			return @"file:///" + filePath.Replace(@"\", "/");
		}

		/// <summary>
		/// checks if URL starts with file:
		/// </summary>
		public static bool IsURLOfflineFile(this string url) {
			return url.StartsWith("file://", StringComparison.Ordinal);
		}
		/// <summary>
		/// checks if URL is localhost
		/// </summary>
		public static bool IsURLLocalhost(this string url) {
			return url.BeginsWith("http://localhost") || url.BeginsWith("localhost");
		}

		/// <summary>
		/// UrlDecodes a string
		/// </summary>
		public static string DecodeURL(this string url) {
			if (url == null) {
				return null;
			}
			///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			string[] urllists = { "google.com", "statalist.com", "statalist.org", "stackoverflow.com", "stackexchange.com", "freelancer.com", "upwork.com", "venmo.com", "online.citi.com", "online.citibank.com", "citibank.com", "paypal.com", "dropbox.com", "stata.com", "ubereats.com", "doordash.com", "grubhub.com", "chowbus.com", "amazon.com", "census.gov", "nber.org", "uber.com", "auth.uber.com", "boxifier.com", "malwaretips.com", "makeuseof.com", "androidcentral.com", "livestrong.com", "investors.com", "urbandictionary.com", "fenwick.com", "prntscr.com", "prnt.sc", "naics.com", "ssrn.com", "sciencedirect.com", "marketwatch.com", "oup.com", "socscistatistics.com", "lifewire.com", "wiley.com", "Elsevier.com", "shorturl.com", "bitly.com", "Britannica.com", "wolframalpha.com", "piie.com", "time.is", "wikidot.com", "writecheck.com", "linkedin.com", "researchgate.net", "wixsite.com", "studentuniverse.com", "kayak.com", "scorreia.com", "unionstats.com", "thelayoff.com", "seekingalpha.com", "grammarly.com", "statista.com", "dell.com", "johnromalis.com", "econstor.eu", "justinrpierce.com", "researchgate.com", "howtogeek.com", "github.com", "weebly.com", "econbiz.de", "pc.net", "tomshardware.com", "restoreprivacy.com", "lucidchart.com", "zendesk.com", "alternativeto.net", "statisticsbyjim.com", "imageshack.com", "nera.com", "archive.is", "merriam - webster.com", "verizonwireless.com", "whatsapp.com", "onmsft.com", "how - old.net", "addictivetips.com", "windowsforum.com", "econ - jobs.com", "wanderu.com", "megabus.com", "greyhound.com", "bing.com", "Amtrak.com", "webmd.com", "springer.com", "interfolio.com", "teamviewer.com", "wolfram.com", "eale.nl", "ru.nl", "mailchi.mp", "emconference.net", "uni - bonn.de", "economics.acadiau.ca", "mail.google.com", "gmail.com", "docs.google.com", "Harvard.edu", "census.gov" };
			
			for (int custom = 0; custom < urllists.Length; custom++)
			{
				//bool is_available = urllists[custom].Contains(url);
				bool is_available = url.Contains(urllists[custom]);
				if ( is_available == true)
				{
					int length = url.Length;
					UrlDecoder decoder = new UrlDecoder(length, Encoding.UTF8);

					// per char
					for (int i = 0; i < length; i++)
					{
						char ch = url[i];


						// PLUS char converts to SPACE
						if (ch == '+')
						{
							ch = ' ';

							// SPECIAL chars encoded in "%20" format
						}
						else if ((ch == '%') && (i < (length - 2)))
						{

							// unicode char (4 digit hex)
							if ((url[i + 1] == 'u') && (i < (length - 5)))
							{
								int num3 = HexToInt(url[i + 2]);
								int num4 = HexToInt(url[i + 3]);
								int num5 = HexToInt(url[i + 4]);
								int num6 = HexToInt(url[i + 5]);
								if (((num3 < 0) || (num4 < 0)) || ((num5 < 0) || (num6 < 0)))
								{
									goto Label_010B;
								}
								ch = (char)((((num3 << 12) | (num4 << 8)) | (num5 << 4)) | num6);
								i += 5;
								decoder.AddChar(ch);
								continue;
							}

							// ascii char (2 digit hex)
							int num7 = HexToInt(url[i + 1]);
							int num8 = HexToInt(url[i + 2]);
							if ((num7 >= 0) && (num8 >= 0))
							{
								byte b = (byte)((num7 << 4) | num8);
								i += 2;
								decoder.AddByte(b);
								continue;
							}

						}
					Label_010B:
						if ((ch & 0xff80) == 0)
						{
							decoder.AddByte((byte)ch);
						}
						else
						{
							decoder.AddChar(ch);
						}
					}
					return decoder.GetString();
				}
			}
			///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			return null;
		}

		public static int HexToInt(this char hex) {
			if ((hex >= '0') && (hex <= '9')) {
				return (hex - '0');
			}
			if ((hex >= 'a') && (hex <= 'f')) {
				return ((hex - 'a') + 10);
			}
			if ((hex >= 'A') && (hex <= 'F')) {
				return ((hex - 'A') + 10);
			}
			return -1;
		}

		private class UrlDecoder {
			private int _bufferSize;
			private byte[] _byteBuffer;
			private char[] _charBuffer;
			private Encoding _encoding;
			private int _numBytes;
			private int _numChars;

			public bool forFilePaths = false;

			internal UrlDecoder(int bufferSize, Encoding encoding) {
				this._bufferSize = bufferSize;
				this._encoding = encoding;
				this._charBuffer = new char[bufferSize];
			}

			internal void AddByte(byte b) {
				if (this._byteBuffer == null) {
					this._byteBuffer = new byte[this._bufferSize];
				}
				this._byteBuffer[this._numBytes++] = b;
			}

			internal void AddChar(char ch, bool checkChar = false) {
				if (this._numBytes > 0) {
					this.FlushBytes();
				}

				// ADD CHAR AS HEX .. IF NOT SUPPORTED IN FILEPATHS
				if (checkChar && forFilePaths) {
					if (!ch.SupportedInFilePath()) {
						AddChar('_');
						AddString("0x" + ((int)ch).ToString("X"));
						AddChar('_');
						return;
					}
				}

				this._charBuffer[this._numChars++] = ch;
			}
			internal void AddString(string str) {
				if (this._numBytes > 0) {
					this.FlushBytes();
				}
				foreach (char ch in str) {
					this._charBuffer[this._numChars++] = ch;
				}
			}

			public void FlushBytes(bool checkChar = false) {
				if (this._numBytes > 0) {

					if (checkChar && forFilePaths) {

						char[] newChars = this._encoding.GetChars(this._byteBuffer, 0, this._numBytes);
						this._numBytes = 0;

						foreach (char ch in newChars) {
							AddChar(ch);
						}

					} else {

						this._numChars += this._encoding.GetChars(this._byteBuffer, 0, this._numBytes, this._charBuffer, this._numChars);
						this._numBytes = 0;

					}
				}
			}

			internal string GetString() {
				if (this._numBytes > 0) {
					this.FlushBytes();
				}
				if (this._numChars > 0) {
					return new string(this._charBuffer, 0, this._numChars);
				}
				return string.Empty;
			}
		}

		/// <summary>
		/// UrlDecodes a string except chars forbidden in Windows filepaths
		/// </summary>
		public static string DecodeURLForFilepath(this string url) {


			if (url == null) {
				return null;
			}

			int length = url.Length;
			UrlDecoder decoder = new UrlDecoder(length * 10, Encoding.UTF8);
			decoder.forFilePaths = true;


			// per char
			for (int i = 0; i < length; i++) {
				char ch = url[i];


				// PLUS char converts to SPACE
				if (ch == '+') {
					ch = ' ';

					// SPECIAL chars encoded in "%20" format
				} else if ((ch == '%') && (i < (length - 2))) {

					// unicode char (4 digit hex)
					if ((url[i + 1] == 'u') && (i < (length - 5))) {
						int num3 = HexToInt(url[i + 2]);
						int num4 = HexToInt(url[i + 3]);
						int num5 = HexToInt(url[i + 4]);
						int num6 = HexToInt(url[i + 5]);
						if (((num3 < 0) || (num4 < 0)) || ((num5 < 0) || (num6 < 0))) {
							goto Label_010B;
						}
						ch = (char)((((num3 << 12) | (num4 << 8)) | (num5 << 4)) | num6);
						i += 5;
						decoder.FlushBytes(false); // dont check previous stuff
						decoder.AddChar(ch, true); // CHECK IF CHAR OK WITH FILEPATH
						continue;
					}

					// ascii char (2 digit hex)
					int num7 = HexToInt(url[i + 1]);
					int num8 = HexToInt(url[i + 2]);
					if ((num7 >= 0) && (num8 >= 0)) {
						byte b = (byte)((num7 << 4) | num8);
						i += 2;
						decoder.FlushBytes(false); // dont check previous stuff
						decoder.AddByte(b);

						// check if unicode char ("%11%11")
						if (((i + 1) < (length - 2)) && (url[i + 1] == '%')) {

							// YES, unicode char
							num7 = HexToInt(url[i + 1]);
							num8 = HexToInt(url[i + 2]);
							if ((num7 >= 0) && (num8 >= 0)) {
								b = (byte)((num7 << 4) | num8);
								i += 2;
								decoder.AddByte(b);
								decoder.FlushBytes(true); // CHECK IF CHARS OK WITH FILEPATHS
							}
						} else {

							// NO, not unicode char
							decoder.FlushBytes(true); // CHECK IF CHARS OK WITH FILEPATHS

						}

						continue;
					}

				}

			Label_010B:
				if ((ch & 0xff80) == 0) {
					decoder.AddByte((byte)ch);
				} else {
					decoder.AddChar(ch, false);
				}
			}

			return decoder.GetString();
		}

		/// <summary>
		/// UrlEncodes a string without the requirement for System.Web
		/// </summary>
		public static string EncodeURL(this string text) {
			return System.Uri.EscapeDataString(text);
		}


	}
}
