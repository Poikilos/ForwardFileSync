/*
 * Created by SharpDevelop.
 * Author: Jake Gustafson
 * Date: 2/6/2010
 * Time: 3:07 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Collections;
//using System.Security.AccessControl;

namespace ExpertMultimedia {
	/// <summary>
	/// Description of Common.
	/// </summary>
	public class Common {
#region debug vars
		public static MyCallBack mcbNow=new MyCallBack();
		public static string sLastFileUsed="";
		public const int DebugLevel_On=1;
		public const int DebugLevel_Mega=2;
		public const int DebugLevel_Ultra=3;
		public static int iDebugLevel=0;//(SET THIS FROM the calling program NOT HERE!)
		public static bool bDebug {
			get {
				return iDebugLevel>=1;
			}
		}
		public static bool bMegaDebug { //(SET THIS FROM the calling program NOT HERE!)
			get {
				return iDebugLevel>=2;
			}
		}
		public static bool bUltraDebug {
			get {
				return iDebugLevel>=3;
			}
		}
		private static string participle="not yet initialized";
		public static string sParticiple {
			get {
				if (participle==null) participle="";
				return participle;
			}
			set {
				if (bMegaDebug) {
					Console.Error.WriteLine((participle!=null)?participle:"null participle");
				}
				if (value!=null&&value.Length>0) {
					participle=value;
				}
			}
		}
#endregion debug vars
#region backup vars
		public static ArrayList alInvalidDrives=new ArrayList();
		public static bool is_exclusion_list_case_sensitive=false;
		public static ArrayList alExtraPseudoRootsToManuallyAdd=new ArrayList();//exists for paths such as those specified to Backup script IncludeDest command can be added to PseudoRoot list regardless of whitelist or excluded drives after PseudoRoot list is calculated
		private static int iSelectableDrives=0;
		private static DriveInfo[] driveinfoarrSelectableDrive=null;
		private static LocInfo[] locinfoarrPseudoRoot=null;
		private static int iPseudoRoots=0;
		private static DriveInfo[] driveinfoarrAbsoluteDrives=null;
		public static int MaskCount {
			get { return (allowed_names!=null)?allowed_names.Count:0; }
		}
#endregion backup vars
#region vars
		public static char[] carrBreakAfter=new char[] {'-',':','/','\\'};
		public static ArrayList excluded_names=new ArrayList(); //operates on folder or file (not full path); formerly alExclusions
		public static ArrayList excluded_paths=new ArrayList(); //operates on path; formerly alExcludedFolderNames
		public static ArrayList allowed_names=new ArrayList(); //operates on folder or file (not full path); formerly alMasks
		public static int Search_MinFileSize=0;//formerly Search_MinFileSize; 0 is flag for no checking
		public static int Search_MaxFileSize=0;//formerly Search_MaxFileSize; 0 is flag for no checking
		public static bool bMustIncludeAllTerms=false;
		private static byte[][][] by3dContentSearch=null;//2nd dimension is always an OR search, and contains different encodings (3d dim is raw data).
		public static bool bContentSearch=false;
		public static int iContentSearches=0;
		public static string sFieldDelim=",";//sCommaDelim
		public static readonly char[] carrHorzSpacing=new char[]{' ','\t','\0'};
		public static readonly char[] carrVertSpacing=new char[]{'\n','\r','\v'};
		public static readonly char[] carrDirectorySeparators=new char[]{':','\\','/'};
		public static string sDirSep=char.ToString(Path.DirectorySeparatorChar);
		public static readonly string SlashWildSlash=Common.sDirSep+"*"+Common.sDirSep;
		private static readonly string[] sarrSpecialFolders_Names_ToLower=new string[]{"%mydocs%","%appdata%","%commonappdata%","%commonprogramfiles%","%cookies%","%desktop%",/*6*/"%desktopdirectory%","%favorites%","%history%","%internetcache%","%localappdata%",
			"%mycomputer%","%mymusic%","%mypictures%","%personal%","%programfiles%","%programs%","%recent%","%sendto%","%startmenu%",/*20*/"%startup%",
			"%system%","%templates%"};
		private static string[] sarrSpecialFolders_Values=new string[]{""};
#endregion vars
		static Common() {
			try { sarrSpecialFolders_Values[0]=Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); }
			catch{}
			try { sarrSpecialFolders_Values[1]=Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); }
			catch{}
			try { sarrSpecialFolders_Values[2]=Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); }
			catch{}
			try { sarrSpecialFolders_Values[3]=Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles); }
			catch{}
			try { sarrSpecialFolders_Values[4]=Environment.GetFolderPath(Environment.SpecialFolder.Cookies); }
			catch{}
			try { sarrSpecialFolders_Values[5]=Environment.GetFolderPath(Environment.SpecialFolder.Desktop); }
			catch{}
			try { sarrSpecialFolders_Values[6]=Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); }
			catch{}
			try { sarrSpecialFolders_Values[7]=Environment.GetFolderPath(Environment.SpecialFolder.Favorites); }
			catch{}
			try { sarrSpecialFolders_Values[8]=Environment.GetFolderPath(Environment.SpecialFolder.History); }
			catch{}
			try { sarrSpecialFolders_Values[9]=Environment.GetFolderPath(Environment.SpecialFolder.InternetCache); }
			catch{}
			try { sarrSpecialFolders_Values[10]=Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); }
			catch{}
			try { sarrSpecialFolders_Values[11]=Environment.GetFolderPath(Environment.SpecialFolder.MyComputer); }
			catch{}
			try { sarrSpecialFolders_Values[12]=Environment.GetFolderPath(Environment.SpecialFolder.MyMusic); }
			catch{}
			try { sarrSpecialFolders_Values[13]=Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); }
			catch{}
			try { sarrSpecialFolders_Values[14]=Environment.GetFolderPath(Environment.SpecialFolder.Personal); }
			catch{}
			try { sarrSpecialFolders_Values[15]=Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles); }
			catch{}
			try { sarrSpecialFolders_Values[16]=Environment.GetFolderPath(Environment.SpecialFolder.Programs); }
			catch{}
			try { sarrSpecialFolders_Values[17]=Environment.GetFolderPath(Environment.SpecialFolder.Recent); }
			catch{}
			try { sarrSpecialFolders_Values[18]=Environment.GetFolderPath(Environment.SpecialFolder.SendTo); }
			catch{}
			try { sarrSpecialFolders_Values[19]=Environment.GetFolderPath(Environment.SpecialFolder.StartMenu); }
			catch{}
			try { sarrSpecialFolders_Values[20]=Environment.GetFolderPath(Environment.SpecialFolder.Startup); }
			catch{}
			try { sarrSpecialFolders_Values[21]=Environment.GetFolderPath(Environment.SpecialFolder.System); }
			catch{}
			try { sarrSpecialFolders_Values[22]=Environment.GetFolderPath(Environment.SpecialFolder.Templates); }
			catch{}
		}//end static constructor
		public Common()
		{
		}
		
#region old ffs backup methods now only used by Backup GoNow (deprecate these?)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="param"></param>
		/// <returns>true if param == "none", =="null", ==null, or =="" (case insensitive)</returns>
		public static bool IsNone(string param) {
			string param_lower=param.ToLower();
			return param_lower=="none" || param_lower==null || param_lower=="null" || param_lower=="";
		}
		public static bool IsExcludedFile(FileInfo fiNow) {//formerly public bool UseFile(DirectoryInfo diBranch, FileInfo fiNow) {
			bool is_excluded=false;
			if (allowed_names!=null&&allowed_names.Count>0) {
				is_excluded=true;
				foreach (string allowed_name in allowed_names) {
					if (Common.IsLike(fiNow.Name,allowed_name,is_exclusion_list_case_sensitive)) is_excluded=false;
				}
			}
			if (!is_excluded) {
				foreach (string exclusion_string in excluded_paths) {
					if (Common.IsLike(fiNow.FullName,exclusion_string,is_exclusion_list_case_sensitive)) {
						is_excluded=true;
						break;
					}
				}
			}
			if (!is_excluded) {
				foreach (string sExclusion in excluded_names) {
					if (Common.IsLike(fiNow.Name,sExclusion,is_exclusion_list_case_sensitive)) {
						is_excluded=true;
						break;
					}
				}
			}
			string sVerb="initializing";
			if (!is_excluded) {
				try {
					if (!is_excluded&&bDebug) {
						if (fiNow.Exists) {
							if (Search_MaxFileSize!=0&&fiNow.Length>Search_MaxFileSize) Console.Error.Write("(file is too big ["+fiNow.Length.ToString()+"bytes])");
							if (Search_MinFileSize!=0&&fiNow.Length<Search_MaxFileSize) Console.Error.Write("(file is too small ["+fiNow.Length.ToString()+"bytes])");	
							//NOTE: actual exclusions based on filesize are BELOW
						}
						else Console.Error.Write("(does not exist)");
					}
					
					if ( fiNow.Exists && (Search_MaxFileSize==0||fiNow.Length<=Search_MaxFileSize)
					         && (Search_MinFileSize==0||fiNow.Length>=Search_MinFileSize) ) {
						sVerb="checking whether content search is appropriate";
						if (bDebug) Console.Error.Write("File size "+fiNow.Length+" for "+fiNow.Name+" is ok--"+sVerb+".");
						if (bContentSearch) {
							//is_excluded=true;
							int iFound=0;
							sVerb="counting content search buffers";
							int iFind=bMustIncludeAllTerms?iContentSearches:1;
							///TODO: "replace" feature -- if (bReplace) iFind=iContentSearches //so that it replaces in ALL matches
							sVerb="running content searches";
							for (int iTerm=0; iTerm<by3dContentSearch.Length; iTerm++) {
								sVerb="running content search "+iTerm.ToString();;
								if (by3dContentSearch[iTerm]!=null) {
									for (int iEncoding=0; iEncoding<by3dContentSearch[iTerm].Length; iEncoding++) {
										if (by3dContentSearch[iTerm][iEncoding]!=null&&by3dContentSearch[iTerm][iEncoding].Length>0) {
											if (FileContains(fiNow.FullName,by3dContentSearch[iTerm][iEncoding])) { //sOnlyIfContains);//diBranch.FullName+char.ToString(System.IO.Path.DirectorySeparatorChar)+fiNow.Name,sOnlyIfContains);
												iFound++;
												if (iFound>=iFind) break;
											}
										}
										else Console.Error.WriteLine("Term "+iTerm.ToString()+" encoding "+iEncoding.ToString()+" is blank!");
									}
								}//end if not found yet
								if (iFound>=iFind) break;
							}//end for find strings
							if (bDebug&&iFound<iFind) Console.Error.Write("("+iFound.ToString()+"/"+iFind.ToString()+" match)");
							sVerb="exiting content search";
							if (iFound<iFind) is_excluded=true;//ok since this line only runs if bContentSearch
						}//end if content search
						//else is_excluded=false;//no content search needed
					}//end if exists and good size
					else is_excluded=true;
				}
				catch (Exception exn) {
					is_excluded=true;
					Console.Error.WriteLine("Exception error in FolderLister UseFile check while "+sVerb+": \""+exn.ToString()+"\"");
				}
			}//end if is_excluded (is not like exclusions)
			//if (bDebug) Console.Error.WriteLine();
			//if (bDebug) Console.Error.WriteLine("File \""+fiNow.Name+"\" fullname \""+fiNow.FullName+"\" ok: "+(is_excluded?"yes":"no"));
			//if (bDebug) Console.Error.WriteLine();
			return is_excluded;
		}//end IsExcludedFile //UseFile
		
		/// <summary>
		/// Checks if folder FullName is in excluded_paths. For excluded_names and allowed_names, only checks Name (not ancestor names).
		/// </summary>
		/// <param name="diBranch"></param>
		/// <returns></returns>
		public static bool IsExcludedFolder(DirectoryInfo diBranch) {//formerly UseFolder
			bool is_excluded=false;
			string sLastExclusion="";
			if (!is_excluded) {
				foreach (string original_exclusion_string in excluded_paths) {
					string exclusion_string = original_exclusion_string;
					if (exclusion_string!=sDirSep && exclusion_string.EndsWith(sDirSep)) exclusion_string=exclusion_string.Substring(0,exclusion_string.Length-1);
					if (Common.IsLike(diBranch.FullName,exclusion_string,is_exclusion_list_case_sensitive)) {
						is_excluded=true;
						break;
					}
				}
			}
			if (!is_excluded) {
				foreach (string sExclusion in excluded_names) {
					sLastExclusion=sExclusion;
					if (Common.IsLike(diBranch.Name,sExclusion,is_exclusion_list_case_sensitive)) {
						is_excluded=true;
						break;
					}
				}
			}
			if (bDebug) Console.Error.WriteLine("("+excluded_names.Count.ToString()+(excluded_names.Count==1?"\""+sLastExclusion+"\"":"")+" excluded) Use folder \""+diBranch.Name+"\"? "+(is_excluded?"Yes":"No"));
			//if (bDebug) Console.Error.WriteLine();
			//if (bDebug) Console.Error.WriteLine("Folder \""+diBranch.Name+"\" ok: "+(is_excluded?"yes":"no"));
			//if (bDebug) Console.Error.WriteLine();
			return is_excluded;
		}
#endregion old ffs backup methods now only used by Backup GoNow (deprecate these?)

#region backup methods

		public static void AddDriveToInvalidDrives(string sPath) {
			if (alInvalidDrives==null) alInvalidDrives=new ArrayList();
			alInvalidDrives.Add(sPath);
		}
		public static void ClearInvalidDrives() {
			if (alInvalidDrives==null) alInvalidDrives=new ArrayList();
			else alInvalidDrives.Clear();
		}
		/// <summary>
		/// This compare method loads files as a stream to save memory.
		/// </summary>
		/// <param name="File1_FullName"></param>
		/// <param name="File2_FullName"></param>
		/// <returns>returns -1 if cannot access files, long.MaxValue if all matched, or #of bytes that matched</returns>
		public static long Compare(string File1_FullName, string File2_FullName) {
			string sParticiple="";
			long valReturn=-1;
			try {
				sParticiple="accessing file {File1_FullName:"+( (File1_FullName!=null) ? ("\""+File1_FullName+"\"") : "null" )+"}";
				FileInfo fi1=new FileInfo(File1_FullName);
				sParticiple="accessing file {File2_FullName:"+( (File2_FullName!=null) ? ("\""+File2_FullName+"\"") : "null" )+"}";
				FileInfo fi2=new FileInfo(File2_FullName);
				valReturn=Compare(fi1,fi2);
			}
			catch (Exception exn) {
				Console.WriteLine("Could not finish Compare(File1_FullName,File2_FullName):"+exn.ToString());
			}
			return valReturn;
		}
		public const long COMPARE_COULDNOTFINISH=long.MinValue;
		/// <summary>
		/// This compare method loads files as a stream to save memory.
		/// </summary>
		/// <param name="fi1"></param>
		/// <param name="fi2"></param>
		/// <returns>returns positive if matched, Common.COMPARE_COULDNOTFINISH if exception, or other negative if not exact match (-1 TIMES #of bytes at beginning that matched)</returns>
		public static long Compare(FileInfo fi1, FileInfo fi2) {
			long valReturn=COMPARE_COULDNOTFINISH;
			FileStream fs1=null;
			FileStream fs2=null;
			try {
				if (fi1!=null) {
					if (fi2!=null) {
						if (fi1.Exists) {
							if (fi2.Exists) {
								if (fi1.Length==fi2.Length) {
									fs1=fi1.OpenRead();
									fs2=fi2.OpenRead();
									for (long index=0; index<fi1.Length; index++) {
										if (fs1.ReadByte()==fs2.ReadByte()) {
											valReturn++;
										}
										else break;
									}
									if (valReturn!=fi1.Length) valReturn*=-1;
									fs1.Close();
									fs1=null;
									fs2.Close();
									fs2=null;
								}
								else valReturn=0;
							}
							else Console.Error.WriteLine("Error in Compare: fi2 does not exist.");
						}
						else Console.Error.WriteLine("Error in Compare: fi1 does not exist.");
					}
					else Console.Error.WriteLine("Error in Compare: fi2 null");
				}
				else Console.Error.WriteLine("Error in Compare: fi1 null");
			}
			catch (Exception exn) {
				valReturn=COMPARE_COULDNOTFINISH;
				Console.Error.WriteLine("Could not finish Compare:"+exn.ToString());
				if (fs1!=null) {
					try { fs1.Close(); fs1=null;}
					catch {};//don't care
				}
				if (fs2!=null) {
					try { fs2.Close(); fs2=null;}
					catch {};//don't care
				}
			}
			return valReturn;
		}
		public static string ToString(ArrayList alNow, string sIndent) {
			if (sIndent==null) sIndent="";
			string sReturn=sIndent+"null";
			if (alNow!=null) {
				sReturn="\n"+sIndent+"{";
				//bool bFirst=true;
				foreach (string sNow in alNow) {
					//if (!bFirst) {
					sReturn+=sIndent+"\n"+sIndent+((sNow!=null)?("\""+sNow.Replace("\"","\\\"")+"\""):"null");
					//}
					//else bFirst=false;
				}
				sReturn+=sIndent+"\n"+sIndent+"}";
			}
			return sReturn;
		}//end ToString
		public static ArrayList GetInvalidDrivesList() {
			ArrayList alReturn=new ArrayList();
			if (alInvalidDrives!=null) {
				foreach (string sNow in alInvalidDrives) {
					alReturn.Add(sNow);
				}
			}
			return alReturn;
		}
		public static void AddPathToExtraPseudoRootsToManuallyAdd(string sPath) {
			if (alExtraPseudoRootsToManuallyAdd==null) alExtraPseudoRootsToManuallyAdd=new ArrayList();
			alExtraPseudoRootsToManuallyAdd.Add(sPath);
		}
		public static void ClearExtraDestinations() {
			if (alExtraPseudoRootsToManuallyAdd==null) alExtraPseudoRootsToManuallyAdd=new ArrayList();
			else alExtraPseudoRootsToManuallyAdd.Clear();
		}
		/// <summary>
		/// Checks string against ExcludeDest strings (all, literal) WITHOUT checking whether the drive exists.
		/// The whitelist is NOT checked here.  The whitelist is comprised of a specified whitelist where drives are removed that don't exist
		/// (done before this method is used).
		/// </summary>
		/// <param name="DestName"></param>
		/// <returns>True if DestName was not specified by an ExcludeDest statement</returns>
		public static bool IsValidDest(string DestName) {
			bool bValid=true;
			string DestNameThenSlashUnlessOtherOSOrLabel=DestName;
			if (!DestName.EndsWith(sDirSep)&&DestName.Contains(sDirSep)) DestNameThenSlashUnlessOtherOSOrLabel+=sDirSep;
			foreach (string sInvalid in alInvalidDrives) {
				string thisInvalidPathThenSlashUnlessOtherOSOrLabel=sInvalid;
				if (!sInvalid.EndsWith(sDirSep)&&sInvalid.Contains(sDirSep)) thisInvalidPathThenSlashUnlessOtherOSOrLabel+=sDirSep;
				if (DestNameThenSlashUnlessOtherOSOrLabel.ToLower()==thisInvalidPathThenSlashUnlessOtherOSOrLabel.ToLower()) { //if (DestNameThenSlashUnlessOtherOSOrLabel.ToLower()==sInvalid.ToLower() || DestName.ToLower()==sInvalid.ToLower()) {
					bValid=false;
					Console.Error.WriteLine(DestName+" ("+DestNameThenSlashUnlessOtherOSOrLabel+") MATCHES invalid "+sInvalid+" ("+thisInvalidPathThenSlashUnlessOtherOSOrLabel+")");
					break;
				}
				else {
					Console.Error.WriteLine(DestName+" ("+DestNameThenSlashUnlessOtherOSOrLabel+") does not match invalid "+sInvalid+" ("+thisInvalidPathThenSlashUnlessOtherOSOrLabel+")");
				}
			}
			return bValid;
		}
		public static void setPseudoRootCustomInt(int InternalPseudoRootIndex, int set_CustomValue) {
			try {
				locinfoarrPseudoRoot[InternalPseudoRootIndex].CustomInt=set_CustomValue;
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Could not finish setting pseudoroot ["+InternalPseudoRootIndex.ToString()+"].CustomInt to "+set_CustomValue.ToString());
			}
		}
		public static LocInfo GetPseudoRoot_ByCustomInt(int thisCustomValue) {
			LocInfo result=null;
			if (locinfoarrPseudoRoot!=null) {
				for (int index=0; index<iPseudoRoots; index++) {
					if (locinfoarrPseudoRoot[index]!=null&&locinfoarrPseudoRoot[index].CustomInt==thisCustomValue) {
						result=locinfoarrPseudoRoot[index];
						break;
					}
				}
			}
			return result;
		}
		public static int GetPseudoRootIndex_ByCustomInt(int thisCustomValue) {
			int result=-1;
			if (locinfoarrPseudoRoot!=null) {
				for (int index=0; index<iPseudoRoots; index++) {
					if (locinfoarrPseudoRoot[index]!=null&&locinfoarrPseudoRoot[index].CustomInt==thisCustomValue) {
						result=index;
						break;
					}
				}
			}
			return result;
		}
		public static LocInfo GetPseudoRoot(int InternalPseudoRootIndex) {
			LocInfo locinfoReturn=null;
			try {
				if (InternalPseudoRootIndex>=0) {
					if (locinfoarrPseudoRoot!=null) {
						if (InternalPseudoRootIndex<iPseudoRoots && InternalPseudoRootIndex<locinfoarrPseudoRoot.Length) {
							locinfoReturn=locinfoarrPseudoRoot[InternalPseudoRootIndex];
						}
						else throw new ApplicationException("Tried to get PseudoRoot out of range at "+InternalPseudoRootIndex.ToString());
					}
				}
				else throw new ApplicationException("Tried to get PseudoRoot at "+InternalPseudoRootIndex.ToString());
			}
			catch (Exception exn) {
				ShowExn(exn,"","GetPseudoRoot");
			}
			return locinfoReturn;
		}//end GetPseudoRoot
		public static string GetSelectableDriveArrayMsg_LengthColonCount_else_ColonNull() {
			return (driveinfoarrSelectableDrive!=null?(".Length:"+driveinfoarrSelectableDrive.Length.ToString()):":null");
		}
		public static string GetPseudoRootArrayMsg_LengthColonCount_else_ColonNull() {
			return (locinfoarrPseudoRoot!=null?(".Length:"+locinfoarrPseudoRoot.Length.ToString()):":null");
		}
		public static int GetSelectableDriveMsg_EntriesCount() {
			return iSelectableDrives;
		}
		public static int GetPseudoRoots_EntriesCount() {
			return iPseudoRoots;
		}
		public static int GetPseudoRoots_CountNonNull(bool bIncludeOldEntriesPastEnd) {
			int iReturn=0;
			try {
				if (locinfoarrPseudoRoot!=null) {
					for (int i=0; (bIncludeOldEntriesPastEnd||i<iPseudoRoots) && i<locinfoarrPseudoRoot.Length; i++) {
						if (locinfoarrPseudoRoot[i]!=null) iReturn++;
					}
				}
			}
			catch (Exception exn) {
				ShowExn(exn,"","GetPseudoRoots_CountNonNull");
			}
			return iReturn;
		}//end GetPseudoRoots_CountNonNull
		public static int GetSelectableDrives_CountNonNull(bool bIncludeOldEntriesPastEnd) {
			int iReturn=0;
			try {
				if (driveinfoarrSelectableDrive!=null) {
					for (int i=0; (bIncludeOldEntriesPastEnd||i<iSelectableDrives) && i<driveinfoarrSelectableDrive.Length; i++) {
						if (driveinfoarrSelectableDrive[i]!=null) iReturn++;
					}
				}
			}
			catch (Exception exn) {
				ShowExn(exn,"","GetSelectableDrives_CountNonNull");
			}
			return iReturn;
		}//end GetSelectableDrives_CountNonNull
		public static int GetAbsoluteDrives_CountNonNull() {
			int iReturn=0;
			try {
				if (driveinfoarrAbsoluteDrives!=null) {
					for (int i=0; i<driveinfoarrAbsoluteDrives.Length; i++) {
						if (driveinfoarrAbsoluteDrives[i]!=null) iReturn++;
					}
				}
			}
			catch (Exception exn) {
				ShowExn(exn,"","GetAbsoluteDrives_CountNonNull");
			}			return iReturn;
		}//end GetAbsoluteDrives_CountNonNull
		public static bool AddFolderToPseudoRoots(string FolderNow) {
			bool bGood=false;
			try {
				LocInfo locinfoNew=new LocInfo();
				locinfoNew.DriveRoot_FullNameThenSlash=LocalFolderThenSlash(FolderNow);
				if (iPseudoRoots<locinfoarrPseudoRoot.Length) {
					//Get data about free space if available:
					int iDrive=SelectableDriveIndexWhereIsRootOfFolder(FolderNow,false);
					if (iDrive>-1) {
						locinfoNew.TotalSize=driveinfoarrSelectableDrive[iDrive].TotalSize;
						locinfoNew.AvailableFreeSpace=driveinfoarrSelectableDrive[iDrive].AvailableFreeSpace;
					}
				}
				AddFolderToPseudoRoots(locinfoNew);
			}
			catch (Exception exn) {
				ShowExn(exn,"adding location","AddFolderToPseudoRoots");
			}
			return bGood;
		}//AddFolderToPseudoRoots(string)

		public static bool AddFolderToPseudoRoots(LocInfo locinfoNow) {
			bool bGood=false;
			try {
				if (locinfoNow!=null) {
					if (locinfoarrPseudoRoot==null) {
						Common.sParticiple="creating PseudoRoot array";
						int iTempSize=iPseudoRoots;
						if (iTempSize<9) iTempSize=9;
						Common.sParticiple="creating PseudoRoot array {NewSize:"+iTempSize.ToString()+"}";
						locinfoarrPseudoRoot=new LocInfo[iTempSize];
						iPseudoRoots=0;
						for (int i=0; i<iTempSize; i++) {
							locinfoarrPseudoRoot[i]=null;
						}
					}
					//Resize array:
					int iNewSize=locinfoarrPseudoRoot.Length;
					if (iPseudoRoots+1>iNewSize) iNewSize=(iPseudoRoots+iPseudoRoots/2+1);
					if (iNewSize>locinfoarrPseudoRoot.Length) {
						Common.sParticiple="resizing PseudoRoot array";
						LocInfo[] locinfoarrOld = locinfoarrPseudoRoot;
						locinfoarrPseudoRoot = new LocInfo[iNewSize];
						int iOld = 0;
						for (int iNow = 0; iNow<iNewSize; iNow++) {
							if (iNow<locinfoarrOld.Length) {
								locinfoarrPseudoRoot[iNow] = locinfoarrOld[iOld];
								iOld++;
							}
							else locinfoarrPseudoRoot[iNow]=null;
						}
						Common.sParticiple="trying to add entry to PseudoRoots array after resize";
					}
					else Common.sParticiple="trying to add entry to PseudoRoots array without resize";
					if (iPseudoRoots<locinfoarrPseudoRoot.Length) {
						locinfoarrPseudoRoot[iPseudoRoots]=locinfoNow;
						iPseudoRoots++;
					}
					else throw new ApplicationException("Could not resize PseudoRoot folder list");
				}
				else throw new ApplicationException("Tried to add null LocInfo");
			}
			catch (Exception exn) {
				ShowExn(exn,"adding location","AddFolderToPseudoRoots");
			}
			Common.sParticiple="finished AddFolderToPseudoRoots";
			return bGood;
		}//AddFolderToPseudoRoots(LocInfo)
		public static void AddDriveToSelectableDrives(DriveInfo driveinfoNow) {
			try {
				if (driveinfoNow!=null) {
					if (driveinfoarrSelectableDrive==null) {
						int iTempSize=iSelectableDrives;
						if (iTempSize<9) iTempSize=9;
						driveinfoarrSelectableDrive=new DriveInfo[iTempSize];
						iSelectableDrives=0;
						for (int i=0; i<iTempSize; i++) {
							driveinfoarrSelectableDrive[i]=null;
						}
					}
					//Resize array:
					int iNewSize=driveinfoarrSelectableDrive.Length;
					if (iSelectableDrives+1>iNewSize) iNewSize=(iSelectableDrives+iSelectableDrives/2+1);
					if (iNewSize>driveinfoarrSelectableDrive.Length) {
						DriveInfo[] driveinfoarrOld = driveinfoarrSelectableDrive;
						driveinfoarrSelectableDrive = new DriveInfo[iNewSize];
						int iOld = 0;
						for (int iNow = 0; iNow<iNewSize; iNow++) {
							if (iNow<driveinfoarrOld.Length) {
								driveinfoarrSelectableDrive[iNow] = driveinfoarrOld[iOld];
								iOld++;
							}
							else driveinfoarrSelectableDrive[iNow]=null;
						}
					}
					if (iSelectableDrives<driveinfoarrSelectableDrive.Length) {
						driveinfoarrSelectableDrive[iSelectableDrives]=driveinfoNow;
						iSelectableDrives++;
					}
					else throw new ApplicationException("Could not resize Selectable Drive list");					
				}
				else throw new ApplicationException("Tried to add a null drive object to Selected Drives");
			}
			catch (Exception exn) {
				ShowExn(exn,"","AddDriveToSelectableDrives");
			}
		}//end AddDriveToSelectableDrives
		/// <summary>
		/// 
		/// </summary>
		/// <param name="bRefreshAbsoluteDriveListEvenIfNotFirstRunOfThisMethod">Rescan for new drives even if not first run (in case a drive was plugged in since last run)</param>
		public static void UpdateSelectableDrivesAndPseudoRoots(bool bRefreshAbsoluteDriveListEvenIfNotFirstRunOfThisMethod) {
			Common.sParticiple="updating drives list";
			try {
				//string[] sarrDrive=Environment.GetLogicalDrives();
				//foreach (string sDrivePathNow in sarrDrive) {
				//	if (IsValidDest(sDrivePathNow)) {
				//		comboDest.Items.Add(sDrivePathNow);
				//	}
				//}
				iPseudoRoots = 0;
				Common.driveinfoarrSelectableDrive=null;
	
				Common.driveinfoarrAbsoluteDrives = null;
				if (driveinfoarrAbsoluteDrives==null || bRefreshAbsoluteDriveListEvenIfNotFirstRunOfThisMethod) {
					driveinfoarrAbsoluteDrives = DriveInfo.GetDrives();//aka GetLogicalDrivesInfo();
					Common.sParticiple="continuing with new drive list {driveinfoarrAbsoluteDrives"+((driveinfoarrAbsoluteDrives!=null)?(".Length:"+driveinfoarrAbsoluteDrives.Length.ToString()):(":null"))+"}";
				}
				else Common.sParticiple="continuing with existing drive list {driveinfoarrAbsoluteDrives"+((driveinfoarrAbsoluteDrives!=null)?(".Length:"+driveinfoarrAbsoluteDrives.Length.ToString()):(":null"))+"}";
				if (driveinfoarrAbsoluteDrives != null && driveinfoarrAbsoluteDrives.Length > 0) {
					driveinfoarrSelectableDrive = new DriveInfo[driveinfoarrAbsoluteDrives.Length];
					Common.sParticiple="continuing after creating new selectable drive list {driveinfoarrSelectableDrive"+((driveinfoarrSelectableDrive!=null)?(".Length:"+driveinfoarrSelectableDrive.Length.ToString()):(":null"))+"}";
					for (int iAbsolute = 0; iAbsolute < driveinfoarrAbsoluteDrives.Length; iAbsolute++) {//foreach (DriveInfo driveinfoNow in DriveInfo.GetDrives()) {
						if (driveinfoarrAbsoluteDrives[iAbsolute] != null && driveinfoarrAbsoluteDrives[iAbsolute].IsReady) {
							if (IsValidDest(driveinfoarrAbsoluteDrives[iAbsolute].RootDirectory.FullName)
								 && ((driveinfoarrAbsoluteDrives[iAbsolute].VolumeLabel == "") || (IsValidDest(driveinfoarrAbsoluteDrives[iAbsolute].VolumeLabel))))
							{
								//comboDest.Items.Add(driveinfoarrAbsoluteDrives[iAbsolute].RootDirectory.FullName);
								Common.AddDriveToSelectableDrives(driveinfoarrAbsoluteDrives[iAbsolute]);
								LocInfo locinfoNew=new LocInfo(); //locinfoarrPseudoRoot[iPseudoRoots] = new LocInfo();
								locinfoNew.VolumeLabel = driveinfoarrAbsoluteDrives[iAbsolute].VolumeLabel;
								locinfoNew.TotalSize = driveinfoarrAbsoluteDrives[iAbsolute].TotalSize;
								locinfoNew.AvailableFreeSpace = driveinfoarrAbsoluteDrives[iAbsolute].AvailableFreeSpace;
								locinfoNew.DriveRoot_FullNameThenSlash = LocalFolderThenSlash(driveinfoarrAbsoluteDrives[iAbsolute].RootDirectory.FullName);
								Common.AddFolderToPseudoRoots(locinfoNew);
								Common.sParticiple="finished adding destination {driveinfoarrAbsoluteDrives[iAbsolute].RootDirectory.FullName:{"+SafeString(driveinfoarrAbsoluteDrives[iAbsolute].RootDirectory.FullName,true)+"}";
							}
							else Common.sParticiple="skipping unusable destination {"
								+"driveinfoarrAbsoluteDrives[iAbsolute].RootDirectory.FullName:"+SafeString(driveinfoarrAbsoluteDrives[iAbsolute].RootDirectory.FullName,true)
								+"; driveinfoarrAbsoluteDrives[iAbsolute].VolumeLabel:"+SafeString(driveinfoarrAbsoluteDrives[iAbsolute].VolumeLabel,true)
								+"}";
						}
					}
				}//end if found any volumes on computer
	
				/* <http://codeidol.com/csharp/csharpckbk2/Filesystem-I-O/Querying-Information-for-All-Drives-on-a-System/> 2008-10-27
				foreach (DriveInfo drive in DriveInfo.GetDrives()) {
					if (drive.IsReady) {
						Console.WriteLine("Drive " + drive.Name + " is ready.");
						Console.WriteLine("AvailableFreeSpace: " + drive.AvailableFreeSpace);
						Console.WriteLine("DriveFormat: " + drive.DriveFormat);
						Console.WriteLine("DriveType: " + drive.DriveType);
						Console.WriteLine("Name: " + drive.Name);
						Console.WriteLine("RootDirectory.FullName: " +
								drive.RootDirectory.FullName);
						Console.WriteLine("TotalFreeSpace: " + drive.TotalFreeSpace);
						Console.WriteLine("TotalSize: " + drive.TotalSize);
						Console.WriteLine("VolumeLabel: " + drive.VolumeLabel);
					}
					else {
							Console.WriteLine("Drive " + drive.Name + " is not ready.");
					}
				}
				*/
				if (Path.DirectorySeparatorChar=='/') {
					Common.sParticiple="getting drive info for system with nix pathing";
					if (Directory.Exists("/Volumes")) {
						DirectoryInfo diMedia = new DirectoryInfo("/Volumes");
						foreach (DirectoryInfo diNow in diMedia.GetDirectories()) {
							if ((diNow.Attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly) {
								if (IsValidDest(diNow.FullName)) {
									AddFolderToPseudoRoots(diNow.FullName);
									iSelectableDrives++;
								}
							}
						}
					}			
				}
				sParticiple="appending ExtraPseudoRootsToManuallyAdd";
				foreach (string sExtraDest in alExtraPseudoRootsToManuallyAdd) {
					//comboDest.Items.Add(Common.LocalFolderThenSlash(sExtraDest));
					try {
						if (Directory.Exists(sExtraDest)) {
							Common.AddFolderToPseudoRoots(sExtraDest);
						}
					}
					catch (Exception exn) {
						Common.ShowExn(exn,"adding extra destinations");
					}
				}
			}
			catch (Exception exn) {
				ShowExn(exn,Common.sParticiple);
			}
		}//end UpdateSelectableDrivesAndPseudoRoots
		public static ArrayList PseudoRoots_DriveRootFullNameThenSlash_ToArrayList() {
			ArrayList alReturn=new ArrayList();
			if (locinfoarrPseudoRoot!=null) {
				for (int i=0; i<iPseudoRoots && i<locinfoarrPseudoRoot.Length; i++) {
					try {
						alReturn.Add(locinfoarrPseudoRoot[i].DriveRoot_FullNameThenSlash);
					}
					catch (Exception exn) {
						ShowExn(exn,"accessing Pseudo Root ["+i.ToString()+"]","PseudoRoots_DriveRootFullNameThenSlash_ToArrayList");
					}
				}
			}
			return alReturn;
		}
		public static int SafeCount(ArrayList alTest) {
			return (alTest!=null)?alTest.Count:-1;
		}
		public static int CountSelectableDrives() {
			return (driveinfoarrSelectableDrive!=null)?driveinfoarrSelectableDrive.Length:0;
		}
		public static ArrayList SelectableDrives_DriveRootFullNameThenSlash_ToArrayList() {
			ArrayList alReturn=new ArrayList();
			try {
				if (driveinfoarrSelectableDrive!=null) {
					Common.sParticiple="accessing non-null selectable drive array";
					for (int i=0; i<iSelectableDrives && i<driveinfoarrSelectableDrive.Length; i++) {
						Common.sParticiple="accessing Selectable Drive ["+i.ToString()+"]";
						try {
							string sNow=LocalFolderThenSlash(driveinfoarrSelectableDrive[i].RootDirectory.FullName);
							Common.sParticiple="getting Selectable Drive ["+i.ToString()+"]";
							alReturn.Add( sNow );
						}
						catch (Exception exn) {
							Common.sParticiple="accessing Selectable Drive ["+i.ToString()+"] {"
								+"driveinfoarrSelectableDrive"+( (driveinfoarrSelectableDrive==null)?":null":(".Length:"+driveinfoarrSelectableDrive.Length.ToString()) )
								+( (driveinfoarrSelectableDrive!=null&&i<driveinfoarrSelectableDrive.Length) ? ("; driveinfoarrSelectableDrive["+i.ToString()+"]:"+((driveinfoarrSelectableDrive[i]==null)?"non-null":"null")) : ("") )
								+"}";
							ShowExn(exn,Common.sParticiple,"SelectableDrives_DriveRootFullNameThenSlash_ToArrayList");
						}
					}
				}
				else Common.sParticiple="accessing null selectable drive array";
			}
			catch (Exception exn) {
				ShowExn(exn,Common.sParticiple,"");
			}
			return alReturn;
		}//end SelectableDrives_DriveRootFullNameThenSlash_ToArrayList
		public static int InternalIndexOfPseudoRootByLabel(string sDriveLabel, bool bCaseSensitive) {
			int iReturn=-1;
			if (sDriveLabel==null) sDriveLabel="";
			if (sDriveLabel!=null&&sDriveLabel!=""&&locinfoarrPseudoRoot!=null) {
				string sDriveLabel_ToLower=bCaseSensitive?"":sDriveLabel.ToLower();
				for (int i=locinfoarrPseudoRoot.Length-1; i>=0; i--) {//start at end in case subfolder is a mounted volume
					if (locinfoarrPseudoRoot[i]!=null) {
						if (bCaseSensitive) {
							if (locinfoarrPseudoRoot[i].VolumeLabel==sDriveLabel) {
								iReturn=i;
								break;
							}
						}
						else {
							if (locinfoarrPseudoRoot[i].VolumeLabel.ToLower()==sDriveLabel_ToLower) {
								iReturn=i;
								break;
							}
						}
					}
				}
			}
			return iReturn;
		}//end InternalIndexOfPseudoRootByLabel
		/// <summary>
		/// Gets the drive that could be the place where the given folder exists (whether it does or not)
		/// (formerly InternalIndexOfPseudoRootWhereFolderStartsWithItsRoot)
		/// </summary>
		/// <param name="FolderNow"></param>
		/// <param name="bCaseSensitive"></param>
		/// <returns></returns>
		private static int InternalIndexOfPseudoRoot_WhereIsOrIsParentOf_FolderFullName(string FolderNow, bool bCaseSensitive) {
			//TODO: deprecate this method
			int iReturn=-1;
			try {
				if (FolderNow!=null&&FolderNow!=""&&locinfoarrPseudoRoot!=null) {
					FolderNow=LocalFolderThenSlash(FolderNow);
					string FolderNowThenSlash_ToLower=bCaseSensitive?"":FolderNow.ToLower();
					for (int i=locinfoarrPseudoRoot.Length-1; i>=0; i--) {//start at end in case subfolder is a mounted volume
						if (locinfoarrPseudoRoot[i]!=null) {
							if (bCaseSensitive) {
								if (  FolderNow.StartsWith( LocalFolderThenSlash(locinfoarrPseudoRoot[i].DriveRoot_FullNameThenSlash) )  ) {//TODO: change this?
									iReturn=i;
									break;
								}
							}
							else {
								if (  FolderNowThenSlash_ToLower.StartsWith( LocalFolderThenSlash(locinfoarrPseudoRoot[i].DriveRoot_FullNameThenSlash.ToLower()) )  ) {//TODO: change this?
									iReturn=i;
									break;
								}
							}
						}
					}
				}
			}
			catch (Exception exn) {
				ShowExn(exn,"","InternalIndexOfPseudoRoot_WhereIsOrIsParentOf_FolderFullName");
			}
			return iReturn;
		}//end 	InternalIndexOfPseudoRoot_WhereIsOrIsParentOf_FolderFullName
		private static int SelectableDriveIndexWhereIsRootOfFolder(string FolderNow, bool bCaseSensitive) {
			//TODO: deprecate this method
			int iReturn=-1;
			try {
				if (FolderNow!=null&&FolderNow!=""&&driveinfoarrSelectableDrive!=null) {
					FolderNow=LocalFolderThenSlash(FolderNow);
					string FolderNowThenSlash_ToLower=bCaseSensitive?"":FolderNow.ToLower();
					for (int i=driveinfoarrSelectableDrive.Length-1; i>=0; i--) {//start at end in case subfolder is a mounted volume
						if (driveinfoarrSelectableDrive[i]!=null) {
							if (bCaseSensitive) {
								if (FolderNow.StartsWith(LocalFolderThenSlash(driveinfoarrSelectableDrive[i].RootDirectory.FullName))) {
									iReturn=i;
									break;
								}
							}
							else {
								if (FolderNowThenSlash_ToLower.StartsWith(LocalFolderThenSlash(driveinfoarrSelectableDrive[i].RootDirectory.FullName).ToLower())) {
									iReturn=i;
									break;
								}
							}
						}
					}
				}
			}
			catch (Exception exn) {
				ShowExn(exn,"","SelectableDriveIndexWhereIsRootOfFolder");
			}
			return iReturn;
		}//end SelectableDriveIndexWhereIsRootOfFolder
		/// <summary>
		/// Gets the directory separator character for a given path
		/// </summary>
		/// <param name="path"></param>
		/// <returns>A directory separator char (only slash or backslash) otherwise 0x00 if fails to find either.</returns>
		public static char getSlash(string path) {
			char slash=(char)0;
			char notSlash='/';
			if (Path.DirectorySeparatorChar=='/') notSlash='\\';
			if (path!=null) {
				for (int charIndex=0; charIndex<path.Length; charIndex++) {
					if (path[charIndex]==Path.DirectorySeparatorChar || path[charIndex]==notSlash) {
						slash=path[charIndex];
						break;
					}
				}
			}
			return slash;
		}
		public static string SafeString(string val, bool bShowValueAndQuoteIfNonNull) {
			return ( (val!=null) ? (bShowValueAndQuoteIfNonNull?"\""+val+"\"":"non-null") : "null" );
		}
		public static string SafeString(string val, bool bShowValueIfNonNull, bool bQuoteIfNonNull) {
			return (  (val!=null)  ?  ( bShowValueIfNonNull ? ((bQuoteIfNonNull?"\"":"")+val+(bQuoteIfNonNull?"\"":"")) : "non-null")  :  "null"  );
		}
		public static string FolderThenSlash(string sPath, string DirectorySeparatorChar_ToUseNow) {
			return (sPath!=null&&sPath!="")?(sPath.EndsWith(DirectorySeparatorChar_ToUseNow)?sPath:sPath+DirectorySeparatorChar_ToUseNow):DirectorySeparatorChar_ToUseNow;
		}
		public static string SlashThenFolder(string sPath, string DirectorySeparatorChar_ToUseNow) {
			return (sPath!=null&&sPath!="")?(sPath.StartsWith(DirectorySeparatorChar_ToUseNow)?sPath:DirectorySeparatorChar_ToUseNow+sPath):DirectorySeparatorChar_ToUseNow;
		}
		public static string FolderThenNoSlash(string sPath, string DirectorySeparatorChar_ToUseNow) {
			return (sPath!=null&&sPath!="")?(sPath.EndsWith(DirectorySeparatorChar_ToUseNow)?sPath.Substring(0,sPath.Length-1):sPath):"";
		}
		public static string NoSlashThenFolder(string sPath, string DirectorySeparatorChar_ToUseNow) {
			return (sPath!=null&&sPath!="")?(sPath.StartsWith(DirectorySeparatorChar_ToUseNow)?sPath.Substring(1):sPath):"";
		}
		public static string LocalFolderThenSlash(string sPath) {
			return FolderThenSlash(sPath,sDirSep);
		}
		public static string LocalFolderThenNoSlash(string sPath) {
			return FolderThenNoSlash(sPath,sDirSep);
		}
		public static string RemoteFolderThenSlash(string sPath) {
			return FolderThenSlash(sPath,"/");
		}
		public static string RemoteFolderThenNoSlash(string sPath) {
			return FolderThenNoSlash(sPath,"/");
		}
		public static bool IsAbsolutePath(string sPath) {
			bool bAbsolute=false;
			try {
				if (sPath!=null) {
					if (Path.DirectorySeparatorChar=='/') {
						if (sPath==""||sPath[0]=='/') bAbsolute=true; //allow blank for "'/' not ending in slash" scenario
					}
					else {
						if (sPath.Length>=2&&sPath[1]==':') bAbsolute=true;
					}
					if (sPath.Length>=2&&sPath[0]=='/'&&sPath[1]=='/') bAbsolute=true; //LDAP
					else {
						int iColon=sPath.IndexOf(":");
						if (iColon>=0&&sPath.Length>iColon+2) {
							if (sPath[iColon+1]=='/'&&sPath[iColon+2]=='/') bAbsolute=true; //URL
						}
					}
				}
			}
			catch (Exception exn) {
				bAbsolute=false;
				ShowExn(exn,"checking for absolute path {sPath:"+SafeString(sPath,true,true)+"}");
			}
			return bAbsolute;
		}//end IsAbsolutePath
		public static bool IsNotBlank(string sNow) {
			return sNow!=null&&sNow.Length>0;
		}
		public static void ShowExn(Exception exn) {
			ShowExn(exn,"");
		}
		public static void ShowExn(Exception exn, string sParticiple) {
			ShowExn(exn,sParticiple,"");
		}
		public static void ShowExn(Exception exn, string sParticiple, string sMethodName) {
			Console.Error.WriteLine();
			string sMsg="Could not finish";
			if (IsNotBlank(sParticiple)) sMsg+=" "+sParticiple;
			if (IsNotBlank(sMethodName)) sMsg+=" in "+sMethodName;
			if (mcbNow!=null) mcbNow.ShowMessage(sMsg);
			Console.Error.WriteLine(sMsg);
			if (exn!=null) Console.Error.WriteLine(exn.ToString());
			else Console.Error.WriteLine(" exn is null so no further information can be displayed");
		}
		public static bool Contains(char[] Haystack, char Needle) {
			bool bReturn=false;
			if (Haystack!=null) {
				for (int iTest=0; iTest<Haystack.Length; iTest++) {
					if (Needle==Haystack[iTest]) {
						bReturn=true;
						break;
					}
				}
			}
			return bReturn;
		}
		
		public static int MoveBackToOrStayAt(string Haystack, char Needle, int FindBeforeAndIncludingIndex) {
			int iFound=-1;
			if (Haystack!=null&&FindBeforeAndIncludingIndex>=0&&FindBeforeAndIncludingIndex<Haystack.Length) {
				for (int iChar=FindBeforeAndIncludingIndex; iChar>-1; iChar--) {
					if (Haystack[iChar]==Needle) {
						iFound=iChar;
						break;
					}
				}
			}
			return iFound;
		}//end MoveBackToOrStayAt
		public static int MoveBackToOrStayAtAny(string Haystack, char[] Needles, int FindBeforeAndIncludingIndex) {
			int iFound=-1;
			if (Haystack!=null&&FindBeforeAndIncludingIndex>=0&&FindBeforeAndIncludingIndex<Haystack.Length) {
				for (int iChar=FindBeforeAndIncludingIndex; iChar>-1; iChar--) {
					if (Contains(Needles,Haystack[iChar])) {//inentionally find haystack char in needles
						iFound=iChar;
						break;
					}
				}
			}
			return iFound;
		}//end MoveBackToOrStayAtAny
		public static string SafeSubstring(string AllText, int start) {
			string sReturn="";
			int length=0;
			int startOrig=start;
			try {
				if (AllText!=null) {
					if (start<0) {
						Console.Error.WriteLine("Warning: start is negative in SafeSubstring(AllText.Length="+AllText.Length.ToString()+",start="+startOrig.ToString()+")");
						start=0;
					}
					else if (start>=AllText.Length) {
						Console.Error.WriteLine("Warning: start is beyond range in SafeSubstring(AllText.Length="+AllText.Length.ToString()+",start="+startOrig.ToString()+")");
						start=AllText.Length; //ok since checked by SafeString primary overload
					}
					length=AllText.Length-start;
					if (start+length>AllText.Length) {
						Console.Error.WriteLine("Warning: SafeSubstring(AllText.Length="+AllText.Length.ToString()+",start="+startOrig.ToString()+")");
						length=AllText.Length-start;
					}
					sReturn=SafeSubstring(AllText,start,length);
				}
				else Console.Error.WriteLine("Warning: SafeSubstring(AllText=null,start="+startOrig.ToString()+")");
			}
			catch (Exception exn) {
				ShowExn(exn,"getting substring","SafeSubstring(AllText=null,start="+startOrig.ToString()+")");
			}
			return sReturn;
		}//end SafeSubstring
		public static string SafeSubstring(string AllText, int start, int length) {
			string sReturn="";
			int startOrig=start;
			int lengthOrig=length;
			try {
				if (AllText!=null) {
					if (start<0) {
						Console.Error.WriteLine("Warning: start is negative in SafeSubstring(AllText.Length="+AllText.Length.ToString()+",start="+startOrig.ToString()+",length="+lengthOrig.ToString()+")");
						start=0;
					}
					else if (start>=AllText.Length) {
						Console.Error.WriteLine("Warning: start is beyond range in SafeSubstring(AllText.Length="+AllText.Length.ToString()+",start="+startOrig.ToString()+",length="+lengthOrig.ToString()+")");
						start=AllText.Length; //ok since checked by final if clause
					}
					if (start+length>AllText.Length) {
						Console.Error.WriteLine("Warning: SafeSubstring(AllText.Length="+AllText.Length.ToString()+",start="+startOrig.ToString()+",length="+lengthOrig.ToString()+")");
						length=AllText.Length-start;
					}
					if (length>0) {
						sReturn=AllText.Substring(start,length);
					}
					else Console.Error.WriteLine("Warning: returning blank in SafeSubstring(AllText.Length="+AllText.Length.ToString()+",start="+startOrig.ToString()+",length="+lengthOrig.ToString()+")");
				}
				else Console.Error.WriteLine("Warning: null string sent to SafeSubstring(AllText=null,start="+startOrig.ToString()+",length="+lengthOrig.ToString()+")");
			}
			catch (Exception exn) {
				ShowExn(exn,"getting substring","SafeSubstring(AllText=null,start="+startOrig.ToString()+",length="+lengthOrig.ToString()+")");
			}
			return sReturn;
		}//end SafeSubstring
		/// <summary>
		/// Keeps width limited to LineWidth characters and returns the result with LineDelimiter added where needed
		/// </summary>
		/// <param name="AllText"></param>
		/// <param name="LineWidth"></param>
		/// <param name="LineDelimiter"></param>
		/// <returns></returns>
		public static string LimitedWidth(string AllText, int LineWidth, string LineDelimiter,bool KeepTrailingSpaceOnNextLine_FalseRemovesIt) { //Aka WrapText aka AutoWrap aka StringWrap
			string sReturn="";
			try {
				if (LineWidth<1) LineWidth=1;
				int iStartNow=0;
				string AllTextSparse=AllText;//debug only
				int iSparsenAt=AllText.Length;//debug only
				while (iSparsenAt>=0) {
					AllTextSparse=AllTextSparse.Insert(iSparsenAt," ["+iSparsenAt.ToString()+"]");
					if (iSparsenAt==AllText.Length) iSparsenAt=(((int)(iSparsenAt/10))*10)-10;
					else iSparsenAt-=10;
				}
				Console.Error.WriteLine();
				Console.Error.WriteLine();
				Console.Error.WriteLine(AllText);//debug only
				Console.Error.WriteLine(AllTextSparse);//debug only
				while (iStartNow<AllText.Length) {
					if (iStartNow+LineWidth>=AllText.Length) {
						Console.Error.WriteLine();
						sReturn+=((sReturn!="")?LineDelimiter:"") + SafeSubstring(AllText,iStartNow);
						Console.Error.WriteLine("SafeSubstring(AllText,"+iStartNow.ToString()+")");//debug only
						Console.Error.WriteLine(SafeSubstring(AllText,iStartNow));
						iStartNow=iStartNow+LineWidth;
						break;
					}
					else {
						int iPrevBreakBeforeAndDiscard=MoveBackToOrStayAt(AllText,' ',iStartNow+LineWidth);//intentionally uses index past end (endbfore) for start
						int iPrevBreakAfter=MoveBackToOrStayAtAny(AllText,carrBreakAfter,iStartNow+LineWidth-1);//intentionally uses last character (endbefore-1) for start
						if (iPrevBreakBeforeAndDiscard>-1||iPrevBreakAfter>-1) {
							if (iPrevBreakBeforeAndDiscard>iPrevBreakAfter) {
								Console.Error.WriteLine();
								sReturn+=((sReturn!="")?LineDelimiter:"") + SafeSubstring(AllText,iStartNow,iPrevBreakBeforeAndDiscard-iStartNow);
								Console.Error.WriteLine("SafeSubstring(AllText,"+iStartNow.ToString()+","+(iPrevBreakBeforeAndDiscard-iStartNow).ToString()+")",true);//debug only
								Console.Error.WriteLine(SafeSubstring(AllText,iStartNow,iPrevBreakBeforeAndDiscard-iStartNow));
								if (KeepTrailingSpaceOnNextLine_FalseRemovesIt) iStartNow=iPrevBreakBeforeAndDiscard;
								else iStartNow=iPrevBreakBeforeAndDiscard+1;//+1 discards the character e.g. ' '
							}
							else {
								Console.Error.WriteLine();
								sReturn+=((sReturn!="")?LineDelimiter:"") + SafeSubstring(AllText,iStartNow,iPrevBreakAfter+1-iStartNow);
								Console.Error.WriteLine("SafeSubstring(AllText,"+iStartNow.ToString()+","+(iPrevBreakAfter+1-iStartNow).ToString()+")",true);//debug only
								Console.Error.WriteLine(SafeSubstring(AllText,iStartNow,iPrevBreakAfter+1-iStartNow));
								iStartNow=iPrevBreakAfter+1;//+1 discards the character e.g. ' '
							}
						}
						else {//else no breaks, so force break at LineWidth
							Console.Error.WriteLine();
							sReturn+=((sReturn!="")?LineDelimiter:"") + SafeSubstring(AllText,iStartNow,iStartNow+LineWidth-iStartNow);
							CallBackSafe_ShowMessage("SafeSubstring(AllText,"+iStartNow.ToString()+","+(iStartNow+LineWidth-iStartNow).ToString()+")");//debug only
							CallBackSafe_ShowMessage(SafeSubstring(AllText,iStartNow,iStartNow+LineWidth-iStartNow));
							iStartNow=iStartNow+LineWidth;
						}
					}//end else more than one line of text remains
				}//end while iStartNow<AllText.Length
			}
			catch (Exception exn) {
				ShowExn(exn,"limiting width");
			}
			return sReturn;
		}
		private static void CallBackSafe_ShowMessage(string sMsg) {
			if (mcbNow!=null) mcbNow.ShowMessage(sMsg);
		}
#endregion backup methods
		
		/// <summary>
		/// Splits a command line into params delimited by spaces, except spaces within quotes.
		/// </summary>
		/// <param name="sCommandLine"></param>
		/// <param name="bKeepFirstParam">true: keep all params; false:discard first param, e.g. when it is the program name</param>
		/// <returns></returns>
		public static string[] SplitSpaceDelimitedParams(string sCommandLine, bool bKeepFirstParam, bool bRemoveQuotesAroundParams) {
			bool bInQuotes=false;
			ArrayList alNow=null;
			int iFound=0;//params (including first) found
			int iCount=0;//params actually saved
			int iSaving=0;//params actually stored in returned array
			int iChar=0;
			int iStartNow=0;
			string[] sarrReturn=null;
			try {
				alNow=new ArrayList();
				iStartNow=0;
				iFound=0;
				for (iChar=0; iChar<=sCommandLine.Length; iChar++) {
					if ( iChar==sCommandLine.Length
					    || (!bInQuotes&&(sCommandLine[iChar]==' ')) ) { //if at an endbefore
						if ((iChar-iStartNow)>0) {
							if (iFound>=1 || bKeepFirstParam) {
								if ( (iChar-iStartNow)>=2
								    && sCommandLine[iStartNow]=='"' && sCommandLine[iChar-1]=='"' ) {
									if ((iChar-iStartNow)>2) alNow.Add(sCommandLine.Substring(iStartNow+1,iChar-1-(iStartNow+1)));
									else alNow.Add("");
								}
								else alNow.Add(sCommandLine.Substring(iStartNow,iChar-iStartNow));
								iCount++;
							}
							iFound++;
						}
						else {
							if (iStartNow<sCommandLine.Length) alNow.Add("");
						}
						iStartNow=iChar+1;//+1 to exclude the endbefore character (e.g. ' ')
					}//end if at an endbefore
					else if (sCommandLine[iChar]=='"') {
						bInQuotes=!bInQuotes;
					}
				}//end for iChar
				if (alNow.Count>0) {
					sarrReturn=new string[alNow.Count];
					iSaving=0;
					foreach (string sNow in alNow) {
						sarrReturn[iSaving]=sNow;
						iSaving++;
					}
				}
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Could not finish parsing parameter {count:"+iCount.ToString()+"; saving:"+iSaving.ToString()+"; command:"+((sCommandLine!=null)?("\""+sCommandLine+"\""):"null")+"}:");
				Console.Error.WriteLine(exn.ToString());
				Console.Error.WriteLine();
			}
			return sarrReturn;
		}//end SplitSpaceDelimitedParams

		public static string ReplaceDriveLabelInBracketsWithDriveSlash(string Path_WithLabelInBrackets, string start_bracket, string end_bracket) {
			string sReturn=Path_WithLabelInBrackets;
			try {
				if (Path_WithLabelInBrackets!=null) {
					int iPreOpener=Path_WithLabelInBrackets.IndexOf(start_bracket);
					if (iPreOpener>=0) {
						int iPostCloser=Path_WithLabelInBrackets.IndexOf(end_bracket);
						if (iPostCloser>iPreOpener) {
							int iLabelStart=iPreOpener+1;
							int iLabelLen=iPostCloser-iPreOpener-1;
							string sLabelWas=Path_WithLabelInBrackets.Substring(iLabelStart,iLabelLen);
							string sLabelWas_ToUpper=sLabelWas.ToUpper();
							if (Common.locinfoarrPseudoRoot!=null) {
								int iReplacedWith=-1;
								for (int iDrive=0; iDrive<Common.locinfoarrPseudoRoot.Length; iDrive++) {
									if (Common.locinfoarrPseudoRoot[iDrive].VolumeLabel.ToUpper()==sLabelWas_ToUpper) {
										iReplacedWith=iDrive;
										sReturn=Path_WithLabelInBrackets.Substring(0,iPreOpener)  +  Common.locinfoarrPseudoRoot[iDrive].DriveRoot_FullNameThenSlash  +  ( (iPostCloser+1<Path_WithLabelInBrackets.Length) ? Path_WithLabelInBrackets.Substring(iPostCloser+1) : "" );
										break;
									}
								}
							}
							else throw new ApplicationException("Common.locinfoarrPseudoRoot is null");
						}
						else throw new ApplicationException("closing bracket not present after opening bracket in: \""+Path_WithLabelInBrackets+"\"");
					}
				}//end if param not null;
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Could not finish replacing drive label:");
				Console.Error.WriteLine(exn.ToString());
				Console.Error.WriteLine();
			}
			return sReturn;
		}//end ReplaceDriveLabelInBracketsWithDriveSlash
		public static string ReplaceSpecialFolders(string Path_WithEnvironmentVars) {
			//string MyDocs_FullName=Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string sParam_Processed=Path_WithEnvironmentVars;
			//string sChunk_ToLower="%mydocs%";
			for (int iSpecial=0; iSpecial<sarrSpecialFolders_Names_ToLower.Length; iSpecial++) {
				int iChunk=Path_WithEnvironmentVars.ToLower().IndexOf(sarrSpecialFolders_Names_ToLower[iSpecial]);
				try {
					if (iChunk>=0) {
						string sSecondPart=sParam_Processed.Substring(iChunk+sarrSpecialFolders_Names_ToLower[iSpecial].Length);
						if (sarrSpecialFolders_Names_ToLower[iSpecial]==Path_WithEnvironmentVars.ToLower()) {
							sParam_Processed=sarrSpecialFolders_Values[iSpecial];
						}
						else if (iChunk==0) {
							sParam_Processed=sarrSpecialFolders_Values[iSpecial]+(sSecondPart.StartsWith(sDirSep)?"":sDirSep)+sSecondPart;
						}
						else {
							sParam_Processed=/*sParam_Processed.Substring(0,iChunk)+*/sarrSpecialFolders_Values[iSpecial]+(sSecondPart.StartsWith(sDirSep)?"":sDirSep)+sSecondPart;
							Console.Error.WriteLine("WARNING: part of string before "+sarrSpecialFolders_Names_ToLower[iSpecial]+" was removed since special folder is always absolute path!");
						}
					}
				}
				catch (Exception exn) {
					Console.Error.WriteLine("Could not finish replacing special folder "+iSpecial.ToString()+":");
					Console.Error.WriteLine(exn.ToString());
					Console.Error.WriteLine();
				}
			}
			return sParam_Processed;
		}//end ReplaceSpecialFolders

		public static bool ArrayHas(char Needle, char[] Haystack) {
			bool bFound=false;
			if (Haystack!=null) {
				for (int i=0; i<Haystack.Length; i++) {
					if (Haystack[i]==Needle) {
						bFound=true;
						break;
					}
				}
			}
			return bFound;
		}
		/// <summary>
		/// Removes horizontal and vertical spacing characters from start end end (leaves null as null)
		/// </summary>
		/// <param name="val"></param>
		public static void RemoveEndsWhiteSpaceByRef(ref string val) {
			if (val!=null&&val.Length>0) {
				int start=0;
				int endinclusive=val.Length-1;
				while (start<val.Length) {
					if ( ArrayHas(val[start],carrHorzSpacing) || ArrayHas(val[start],carrVertSpacing) ) start++;
					else break;
				}
				while (endinclusive>=0) {
					if ( ArrayHas(val[endinclusive],carrHorzSpacing) || ArrayHas(val[endinclusive],carrVertSpacing) ) endinclusive--;
					else break;
				}
				if (endinclusive>=start) {
					val=val.Substring(start,endinclusive-start+1);
				}
				else val="";
			}
		}

		public static string ToCSVLine(string[] sarrX) {
			string sReturn="";
			if (sarrX!=null&&sarrX.Length>0) {
				for (int iNow=0; iNow<sarrX.Length; iNow++) {
					string sVal=sarrX[iNow];
					sVal=sVal.Replace("\"","\"\"");
					if (sVal.IndexOf(sFieldDelim)>-1) sVal="\""+sVal+"\"";
					sReturn+=(iNow==0?"":sFieldDelim)+sVal;
				}
			}
			return sReturn;
		}
		public static string ToCSVLine(ArrayList alNow) {
			return ToCSVLine( (string[])alNow.ToArray(typeof(string)) );
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns>allowed_names as CSV line ready to write to file</returns>
		public static string MasksToCSV() {
			return ToCSVLine(allowed_names);
		}
		public static string Plural(string sBase, string sSingularEnding, string sPluralEnding, int iCount) {
			return iCount==1?sBase+sSingularEnding:sBase+sPluralEnding;
		}//TODO: move to UniWinForms
		
		public static bool IsAnyDirectorySeparator(char cX) {
			bool bFound=false;
			for (int iNow=0; iNow<carrDirectorySeparators.Length; iNow++) {
				if (carrDirectorySeparators[iNow]==cX) {
					bFound=true;
					break;
				}
			}
			return bFound;
		}//end IsAnyDirectorySeparator

		public static bool GreaterThan(long val1, long val2, long FudgeAllowed) {
			return (val1-val2)>FudgeAllowed;
		}
		public static bool LessThan(long val1, long val2, long FudgeAllowed) {
			return (val2-val1)>FudgeAllowed;
		}
		public static long AbsoluteDifference(long val1, long val2) {
			long valDiff=(val1-val2);
			if (valDiff<0) valDiff=0-valDiff;
			return valDiff;
		}
		public static bool EqualTo(long val1, long val2, long FudgeAllowed) {
			return AbsoluteDifference(val1,val2)<FudgeAllowed;
		}
		public override string ToString()
		{
			return base.ToString();
		}
		public static string ToString(bool var) {
			return ToString(var,"yes","no");
		}
		public static string ToString(bool var, string sTrue, string sFalse) {
			return var?"true":"false";
		}
		public static bool IsLike(string sHaystack, string sNeedle, bool IsCaseSensitive) {
			bool bMatch=false;
			string sDebug="";
			int iFind=0;
			int iFound=0;
			int iNeedle=0;
			int iHaystack=0;
			if (!IsCaseSensitive) {
				sHaystack=sHaystack.ToLower();
				sNeedle=sNeedle.ToLower();
			}
			try {
				for (int iChar=0; iChar<sNeedle.Length; iChar++) {
					if (sNeedle[iChar]!='*') iFind++;
				}
				sDebug="find"+iFind.ToString()+":";
				while (iNeedle<sNeedle.Length&&iHaystack<sHaystack.Length) {
					if (sNeedle[iNeedle]=='?') {
						iFound++;
						iNeedle++;
						iHaystack++;
						sDebug+="?";
					}
					else if (sNeedle[iNeedle]=='*') {
						if (iNeedle+1==sNeedle.Length) {
							sDebug+="(*+skipall)";
							break;//no more need to be matched
						}
						else {
							if (sHaystack[iHaystack]==sNeedle[iNeedle+1]||sNeedle[iNeedle+1]=='?') {
								iNeedle++;
								//iHaystack++;
								sDebug+="(*+match)";
							}
							else {
								sDebug+="*";
								iHaystack++;
							}
						}
					}
					else {
						if (sHaystack[iHaystack]==sNeedle[iNeedle]) {
							sDebug+=char.ToString(sHaystack[iHaystack]);
							iFound++;
							iHaystack++;
							iNeedle++;
						}
						else {
							sDebug+="("+char.ToString(sHaystack[iHaystack])+"!="+char.ToString(sNeedle[iNeedle])+")";
							break;
						}
					}
				}//end while not at end
				if (iFound==iFind) bMatch=true;
			}
			catch (Exception exn) {
				//Console.Error.WriteLine("Exception in IsLike(" + StringNote("string",sHaystack) + "," + StringNote("mask",sNeedle) + ")");
				Console.Error.WriteLine("Error in IsLike("+(sHaystack!=null?sHaystack:"null")+","+(sHaystack!=null?sNeedle:"null")+"): "+(bMatch?"yes":"no")
					+"{"
					+"iFind:"+iFind.ToString()+";"
					+"iFound:"+iFound.ToString()+";"
					+"iNeedle:"+iNeedle.ToString()+";"
					+"iHaystack:"+iHaystack.ToString()+";"
					+"debug:"+sDebug
					+"}"
				);
				Console.Error.WriteLine(exn.ToString());
				Console.Error.WriteLine();
			}
			return bMatch;
		}//end IsLike
		public static string ConvertDirectorySeparatorsToNormal(string sSrcPathX) {
			string sReturn="";
			char cNow;
			if (sSrcPathX!=null) {
				for (int iNow=0; iNow<sSrcPathX.Length; iNow++) {
					cNow=sSrcPathX[iNow];
					if (IsAnyDirectorySeparator(cNow)) cNow=Path.DirectorySeparatorChar;
					sReturn+=char.ToString(cNow);
				}
			}
			return sReturn;
		}//end ConvertDirectorySeparatorsToNormal
		public static bool CreateFolderRecursively(out string GetException, ref ArrayList return_alFoldersCreated_FullNameCollection, string sFullPath) {
			bool bGood=false;
			GetException="";
			int iEnder=0;
			if (return_alFoldersCreated_FullNameCollection==null) return_alFoldersCreated_FullNameCollection=new ArrayList();
			try {
				DirectoryInfo diNow=new DirectoryInfo(sFullPath);
				if (!diNow.Exists) {
					string sDirSep=char.ToString(Path.DirectorySeparatorChar);
					if (!sFullPath.EndsWith(sDirSep)) sFullPath+=sDirSep;
					while (iEnder<sFullPath.Length) {
						iEnder=sFullPath.IndexOf(Path.DirectorySeparatorChar,iEnder+1);
						if (iEnder>-1) {
							if (!Directory.Exists(sFullPath.Substring(0,iEnder))) {
								return_alFoldersCreated_FullNameCollection.Add(sFullPath.Substring(0,iEnder));
								try {
									Directory.CreateDirectory(sFullPath.Substring(0,iEnder));
									bGood=true;
								}
								catch (Exception exn) {
									Console.Error.WriteLine("Error in CreateFolderRecursively(\""+sFullPath+"\") {current-substring:"+sFullPath.Substring(0,iEnder)+"}:");
									Console.Error.WriteLine(exn.ToString());
									Console.Error.WriteLine();
									GetException=exn.ToString();
								}
							}
						}
						else break;
					}
				}
				else bGood=true;
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Error in CreateFolderRecursively(\""+sFullPath+"\") {current-substring:"+sFullPath.Substring(0,iEnder)+"}:");
				Console.Error.WriteLine(exn.ToString());
				Console.Error.WriteLine();
				GetException=exn.ToString();
			}
			return bGood;
		}//end CreateFolderRecursively
		/*deprecated--used CreateFolderRecursively instead
		public static bool CreateSubfolderRecursively(string sOrigPath, ref ArrayList return_alFoldersCreated_FullNameCollection) {
			bool bExistsNow=false;
			string sPath=sOrigPath;
			try {
				if (sPath!=null&&sPath!="") {
					if (sPath.Length==3&&sPath[1]==':'&&sPath[2]=='\\') {
						bExistsNow=Directory.Exists(sPath);
					}
					else if (sPath.Length==2&&sPath[1]==':') {
						bExistsNow=Directory.Exists(sPath+"\\");
					}
					else {
						if (!sPath.EndsWith(sDirSep)) sPath+=sDirSep;
						int iSlashNow=sPath.IndexOf(Path.DirectorySeparatorChar);
						if (iSlashNow>0) {
							if (iSlashNow==sPath.Length-1) { //e.g. =="/" ('nix-like)
								bExistsNow=Directory.Exists(sPath);
							}
							if (sPath=="/") {
								bExistsNow=( (Path.DirectorySeparatorChar=='/') && Directory.Exists("/") );
							}
							else {
								if (Directory.Exists(sPath.Substring(0,iSlashNow))) {
									while (iSlashNow<=sPath.Length-1 && iSlashNow>=0) {
										if (!Directory.Exists(sPath.Substring(0,iSlashNow))) {//ok to start size at iSlashNow since "\" was handled above
											//DirectorySecurity dsNow=new DirectorySecurity(
											Directory.CreateDirectory(sPath.Substring(0,iSlashNow));
											if (return_alFoldersCreated_FullNameCollection==null) return_alFoldersCreated_FullNameCollection=new ArrayList();
											return_alFoldersCreated_FullNameCollection.Add(sPath.Substring(0,iSlashNow));
											//if calling program is running as administrator, all folders in return_alFoldersCreated_FullNameCollection should be changed like this (for vista to allow all administrators full control):
											//"takeown /f "+sFolderNow+" /r /d y"
											//"icacls "+sFolderNow+" /grant administrators:F /t"
											//where sFolderNow is each string in return_alFoldersCreated_FullNameCollection
										}
										if (iSlashNow<sPath.Length-1) {
											iSlashNow=sPath.IndexOf(sDirSep,iSlashNow+1);
										}
										else iSlashNow=-1;//force end since already checked last one (since added slash to end [above] if not present)
									}//end if attempt to create subfolder
									bExistsNow=Directory.Exists(sOrigPath);
								}//end if there is a root in which to recreate subfolders
							}
						}
					}
				}
				else Console.Error.WriteLine("CreateSubfolderRecursively error: "+((sPath==null)?"null":"blank")+" path!");
			}
			catch (Exception exn) {
				Console.Error.WriteLine();
				Console.Error.WriteLine("CreateSubfolderRecursively could not finish:");
				Console.Error.WriteLine(exn.ToString());
			}
			return bExistsNow;
		}//end CreateSubfolderRecursively
		*/
		public static int IndexOfAnyDirectorySeparatorChar(string Haystack) {
			int iFound=-1;
			if (Haystack!=null&&Haystack!="") {
				for (int iNow=0; iNow<Haystack.Length; iNow++) {
					if (IsAnyDirectorySeparator(Haystack[iNow])) {
						iFound=iNow;
						break;
					}
				}
			}
			return iFound;
		}//end IndexOfAnyDirectorySeparatorChar
		public static string RemoveDoubleDirectorySeparators(string sPathX) {
			string sReturn="";
			bool bPrevWasDirSep=false;
			if (sPathX!=null) {
				for (int iNow=0; iNow<sPathX.Length; iNow++) {
					char cNow=sPathX[iNow];
					if (!bPrevWasDirSep||!IsAnyDirectorySeparator(cNow)) {
						sReturn+=char.ToString(cNow);
					}
					if (cNow==Path.DirectorySeparatorChar) bPrevWasDirSep=true;
					else bPrevWasDirSep=false;
				}
			}
			return sReturn;
		}//end RemoveDoubleDirectorySeparators
		public static bool FileContains(string sFile, byte[] byarrNeedle) {
			bool bMatch=false;
			try {
				if (bDebug) Console.Error.WriteLine("Starting \""+sFile+"\" content search");
				bMatch=Common.Contains(sFile,byarrNeedle);
			}
			catch (Exception exn) {
				string sExn=exn.ToString();
				if (sExn.IndexOf("FileNotFoundException")>-1) sExn="FileContains(...,binary): Cannot find  \""+sFile+"\" for binary search-may be broken symbolic link or in different relative path.";
				Console.Error.WriteLine(sExn);
			}
			return bMatch;
		}//end FileContains(...,byarrNeedle) //TODO: move to UniWinForms
		public static bool Contains(string sFile, byte[] byarrNeedle) {
			bool bMatch=false;
			if (bDebug) Console.Error.Write(".");
			if (bDebug) Console.Error.Flush();
			try {
				if (File.Exists(sFile)) {
					if (byarrNeedle!=null&&byarrNeedle.Length>0) {
						if (bDebug) Console.Error.Write(".");
						if (bDebug) Console.Error.Flush();
						byte[][] by2dHaystacks=new byte[2][];
						int[] byarrHaystackUsed=new int[2];
						long FirstHaystack_Position=0; //ok since 2nd is moved to first when new chunk comes
						long File_Position=0;//should ONLY be modified by ProcessDoubleFileBuffersAsQueue!
						byarrHaystackUsed[0]=0;//MUST start at 0 so ProcessDoubleFileBuffersAsQueue knows they're unused
						byarrHaystackUsed[1]=0;
						//byarrHaystackPosition[0]=0;
						//byarrHaystackPosition[1]=0;
						by2dHaystacks[0]=new byte[byarrNeedle.Length];
						by2dHaystacks[1]=new byte[byarrNeedle.Length];
						int Haystacks_LengthTotal=by2dHaystacks[0].Length+by2dHaystacks[1].Length;
						long File_Length=(new FileInfo(sFile)).Length;
						FileStream streamIn=new FileStream(sFile,FileMode.Open);
						int valNow;
						ProcessDoubleFileBuffersAsQueue(by2dHaystacks,byarrHaystackUsed,streamIn,ref File_Position,File_Length);
						long PositionVirtualCheckStart=0;
						long PositionVirtualCheckNow=0;
						int Buffer0_RelPosition=0;
						int Buffers_PositionTranscendentNow=0;
						int Buffers_PositionTranscendentStart=0;
						int Needle_Position=0;
						//TODO: finish this -- finish checking logic
						while ( PositionVirtualCheckStart<File_Length ) { //(valNow=streamIn.ReadByte) > -1 ) {
							PositionVirtualCheckNow=PositionVirtualCheckStart;
							int iMatched=0;
							if (Buffer0_RelPosition==by2dHaystacks[0].Length) {
								Buffer0_RelPosition=0;
								ProcessDoubleFileBuffersAsQueue(by2dHaystacks,byarrHaystackUsed,streamIn,ref File_Position,File_Length);//DOES index 1 pointer to index 0
							}
							Needle_Position=0;
							Buffers_PositionTranscendentNow=Buffers_PositionTranscendentStart;
							while (PositionVirtualCheckNow<File_Length) {
								if (byarrNeedle[Needle_Position]==by2dHaystacks[Buffers_PositionTranscendentNow>=by2dHaystacks[0].Length?1:0][Buffers_PositionTranscendentNow>=by2dHaystacks[0].Length?Buffers_PositionTranscendentNow-by2dHaystacks[0].Length:Buffers_PositionTranscendentNow]) iMatched++;
								PositionVirtualCheckNow++;
								Needle_Position++;
								Buffers_PositionTranscendentNow++;
							}
							if (iMatched==byarrNeedle.Length) {
								bMatch=true;
								break;
							}
							PositionVirtualCheckStart++;
							Buffer0_RelPosition++;
							Buffers_PositionTranscendentStart++;
							if (Buffers_PositionTranscendentStart==Haystacks_LengthTotal) {
								Buffers_PositionTranscendentStart=0;
							}
						}//end while virtual start PositionVirtualCheckStart<File_Length
						streamIn.Close();
					}
					else Console.Error.WriteLine("Common.Contains error: No search term");
				}
				else Console.Error.WriteLine("Common.Contains cannot find \""+sFile+"\"");
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Common.Contains error in \""+sFile+"\":");
				Console.Error.WriteLine(exn.ToString());
				Console.Error.WriteLine();
			}
			return bMatch;
		}//end Contains static version
		public static void ClearContentSearch() {//was ClearSearchTerms
			by3dContentSearch=null;
		}
		public static void SetContentSearch(string Term, bool bAlsoAllowUnicode, bool bAlsoAllowBigEndianMacUnicode) {
			SetContentSearch(Term,true,bAlsoAllowUnicode,bAlsoAllowBigEndianMacUnicode);
		}
		public static void SetContentSearch(string Term, bool bAllowUnicode, bool bAllowBigEndianMacUnicode, bool bAllowAsciiCanProvideMatch) {
			SetContentSearch(new string[]{Term},true,bAllowUnicode, bAllowBigEndianMacUnicode, bAllowAsciiCanProvideMatch);
		}
		public static void SetContentSearch(string[] Terms, bool MustIncludeAllTerms) {
			SetContentSearch(Terms,MustIncludeAllTerms,false,false,true);
		}
		public static void SetContentSearch(string[] Terms, bool MustIncludeAllTerms, bool bAllowUnicode, bool bAllowBigEndianMacUnicode, bool bAllowAscii) {
			by3dContentSearch=null;
			bMustIncludeAllTerms=MustIncludeAllTerms;
			if (Search_MaxFileSize==0) Search_MaxFileSize=102400; //TODO: allow user to confirm file limits here (content search has it's own default Search_MaxFileSize)
			if (Terms!=null&&Terms.Length>0) {
				int iGoodTermRelatives=0;
				for (int iX=0; iX<Terms.Length; iX++) {
					if (Terms[iX]!=null&&Terms[iX].Length>0) iGoodTermRelatives++;
				}
				if (iGoodTermRelatives>0) {
					if (Search_MaxFileSize<1) Search_MaxFileSize=2048000000;//2048000000bytes==2GB
					if (!bAllowAscii&&!bAllowUnicode) bAllowAscii=true;
					by3dContentSearch=new byte[iGoodTermRelatives][][];
					int iGoodTermRelative=0;
					for (int iTerm=0; iTerm<Terms.Length; iTerm++) {
						if (Terms[iTerm]!=null&&Terms[iTerm].Length>0) {
							by3dContentSearch[iGoodTermRelative]=new byte[(bAllowAscii?1:0)+(bAllowUnicode?1:0)+(bAllowBigEndianMacUnicode?1:0)][];
							int iEncodings=0;
							if (bAllowAscii) {
								by3dContentSearch[iGoodTermRelative][iEncodings]=new byte[Terms[iTerm].Length];
								for (int iChar=0; iChar<Terms[iTerm].Length; iChar++) {
									by3dContentSearch[iGoodTermRelative][iEncodings][iChar]=(byte)((uint)Terms[iTerm][iChar]&0xFF);
								}
								iEncodings++;
							}
							if (bAllowUnicode) {
								by3dContentSearch[iGoodTermRelative][iEncodings]=new byte[Terms[iTerm].Length*2];
								for (int iChar=0; iChar<Terms[iTerm].Length; iChar++) {
									by3dContentSearch[iGoodTermRelative][iEncodings][iChar*2]=(byte)((uint)Terms[iTerm][iChar]&0xFF);
									by3dContentSearch[iGoodTermRelative][iEncodings][iChar*2+1]=(byte)((uint)Terms[iTerm][iChar]>>8);
								}
								iEncodings++;
							}
							if (bAllowBigEndianMacUnicode) {
								by3dContentSearch[iGoodTermRelative][iEncodings]=new byte[Terms[iTerm].Length*2];
								for (int iChar=0; iChar<Terms[iTerm].Length; iChar++) {
									by3dContentSearch[iGoodTermRelative][iEncodings][iChar*2]=(byte)((uint)Terms[iTerm][iChar]>>8);
									by3dContentSearch[iGoodTermRelative][iEncodings][iChar*2+1]=(byte)((uint)Terms[iTerm][iChar]&0xFF);
								}
								iEncodings++;
							}
							if (bDebug) Console.Error.WriteLine(iEncodings.ToString()+" encoding"+(iEncodings==1?"":"s")+" added for "+Terms[iTerm]);
							iGoodTermRelative++;
						}//end if term is not blank
					}//end for terms
					if (bDebug) Console.Error.WriteLine(iGoodTermRelative.ToString()+" term"+(iGoodTermRelative==1?" ("+Terms[0]+")":"s")+" added.");
				}//if any terms given
				else Console.Error.WriteLine("SetContentSearch error: Terms array with empty terms sent!");
			}//terms array is not blank
			else Console.Error.WriteLine("SetContentSearch error: Blank terms array sent!");
		}//end SetContentSearch
		/// <summary>
		/// 
		/// </summary>
		/// <param name="array_of_byte_arrays"></param>
		public static void SetContentSearch(byte[] byarrContains) {
			by3dContentSearch=new byte[1][][];
			by3dContentSearch[0]=new byte[1][];
			by3dContentSearch[0][0]=byarrContains;
		}
		/// <summary>
		/// Loads Buffers_by2dData[1] into Buffers_by2dData[0] and loads the new data from the file into Buffers_by2dData[1]
		/// </summary>
		/// <param name="Buffers_by2dData"></param>
		/// <param name="Buffers_iarrUsedLengths"></param>
		/// <param name="streamIn"></param>
		/// <param name="File_Position">Calling program MUST set this to ZERO before first calling this, then never change it again.  This method will increment it and only use it to determine when to expect the file to end.</param>
		/// <param name="File_Length"></param>
		/// <returns></returns>
		private static bool ProcessDoubleFileBuffersAsQueue(byte[][] Buffers_by2dData, int[] Buffers_iarrUsedLengths, FileStream streamIn, ref long File_Position, long File_Length) { //formerly LoadBuffers
			bool bGood=true;
			try {
				if (File_Position<File_Length) {
					int iPositionTranscendent=0;
					if (Buffers_by2dData[1]!=null) {
						byte[] byarrTemp=Buffers_by2dData[0];
						Buffers_by2dData[0]=Buffers_by2dData[1];
						Buffers_by2dData[1]=byarrTemp;//ok since used amount set to 0
						Buffers_iarrUsedLengths[0]=Buffers_iarrUsedLengths[1];
						Buffers_iarrUsedLengths[1]=0;
					}
					//TODO: finish this -- finish checking logic
					if (Buffers_iarrUsedLengths[0]==0) {
						Buffers_iarrUsedLengths[0]=streamIn.Read(Buffers_by2dData[0],0,Buffers_by2dData[0].Length);
						File_Position+=Buffers_iarrUsedLengths[0];
					}
					if (File_Position<File_Length) {
						Buffers_iarrUsedLengths[1]=streamIn.Read(Buffers_by2dData[1],0,Buffers_by2dData[1].Length);
						File_Position+=Buffers_iarrUsedLengths[1];
						//int Buffer1_RelPosition=0;
						//while ( (valNow=streamIn.ReadByte() > -1 ) {
						//int valNow;
						//Buffers_iarrUsedLengths[1][Buffer1_RelPosition]=(byte)valNow;
						//File_Position++;
						//if (iPositionTranscendent>=Haystacks_LengthTotal) break;
						//}
					}
				}//end if File_Position<File_Length
				else bGood=false;
			}
			catch (Exception exn) {
				bGood=false;
				Console.Error.WriteLine();
				Console.Error.WriteLine("Could not finish ProcessDoubleFileBuffersAsQueue: "+exn.ToString());
			}
			return bGood;
		}//end ProcessDoubleFileBuffersAsQueue
		public static bool StartsWithAny(string Haystack, string[] Needles) {
			bool IsFound=false;
			if (!string.IsNullOrEmpty(Haystack) && Needles!=null) {
				foreach (string Needle in Needles) {
					if (!string.IsNullOrEmpty(Needle)) {
						if (Haystack.StartsWith(Needle)) {
							IsFound=true;
							break;
						}
					}
				}
			}
			return IsFound;
		}

	}//end Common
}//end namespace
