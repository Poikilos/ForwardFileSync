/*
 * Created by SharpDevelop.
 * User: Owner
 * Date: 3/9/2009
 * Time: 10:45 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections;

namespace ForwardFileSync {
	class Program {
		public static string sParticiple="";
		public static bool bTestOnly=false; //true: does not actually copy files
		public static bool bCreateBatch=true; //true: outputs batch instructions to standard output
		public static bool bCopyErrorsLogHereInsteadOfCurrentUserDesktop=true;
		public static bool bForceOverWriteExisting=true;
		public static string[] sarrFolderStack=new string[1024];
		public static int iFolderDepth=0;
		public static string sSourceRoot=@"";
		public static string sDestRoot=@"";
		public static string sRemark="REM ";//fixed at beginning of program
		public static string sMD="md";//fixed at beginning of program
		public static string sCP="copy /y";//fixed at beginning of program
		public static string LastCreatedDirectory_FullName="";
		public static ArrayList alExcludeFolder_NameI_Lower=new ArrayList(new string[]{"temp","temporary internet files"});//must be lower
		public static ArrayList alExcludeFolder_FullNameI_Lower=new ArrayList(new string[]{@"c:\windows"});//new string[]{""};//must be lower
		public static ArrayList alExcludeFile_FullNameI_Lower=null;//new ArrayList(new string[]{@"c:\pagefile.sys"});
		public static ArrayList alExcludeFileStartsWithI_Lower=null;//new string[] {"itunes library","itunes music library"};//must be lower
		public static ArrayList alExcludeFileEndsWithI_Lower=new ArrayList(new string[] {".tmp","hiberfil.sys","pagefile.sys","delloperatingsystemreinstallationcd-slipstream-sp3-sata-nlite.iso"});//must be lower
		public static string Batch_FullName_FirstPart="ForwardFileSync-DoGeneratedActions";//fixed at beginning of program
		public static string Batch_FullName="ForwardFileSync-DoGeneratedActions.bat";//fixed at beginning of program
		public static DirectoryInfo diSourceRoot=null;
		public static DirectoryInfo diDestRoot=null;
		public static StreamWriter streamErr=null;
		public static StreamWriter streamBatch=null;
		public static string sDirSep=char.ToString(Path.DirectorySeparatorChar);
		public static string ErrorWriteBuffer="";
		public static string sYYYY_MM_DD="";//fixed at beginning of program
		public static string sHH_MM_SS="";//fixed at beginning of program
		public static string sDateTimeNow="";
		public static string FailureList_FullName="ForwardFileSync-FailedSourcesList.txt";
		public static string CopyErrorsFile_FullName="";//fixed at beginning of program
		public static StreamWriter streamFailedSourcesList=null;
		
		public static void FailedSourcesList_WriteLine(string sLine) {
			try {
				if (streamFailedSourcesList!=null) streamFailedSourcesList.WriteLine(sLine);
			}
			catch {}
		}
		public static void AddFailedSourceFile(string File_FullName) {
			FailedSourcesList_WriteLine(sRemark+"--FAILED to copy from file:"+File_FullName);
		}
		public static void AddFailedSourceFolder(string Folder_FullName) {
			try {
				if (Folder_FullName!=sDirSep&&Folder_FullName.EndsWith(sDirSep)) Folder_FullName=Folder_FullName.Substring(0,Folder_FullName.Length-1);
				FailedSourcesList_WriteLine(sRemark+"--FAILED to copy from folder:"+Folder_FullName);
			}
			catch {}
		}
		public static void Batch_Write(string val) {
			try {  if (streamBatch!=null) { streamBatch.Write(val); streamBatch.Flush(); }  }
			catch{}
		}
		public static void Batch_WriteLine() {
			try {  if (streamBatch!=null) { streamBatch.WriteLine(); }  }
			catch{}
		}
		public static void Batch_WriteLine(string val) {
			Batch_Write(val);
			Batch_WriteLine();
		}
		public static void Error_Write(string val) {
			Console.Error.Write(val);
			try {if (streamErr!=null) streamErr.Write(val);}
			catch {}
			if (bCreateBatch) {
				Batch_Write(((ErrorWriteBuffer=="")?sRemark:"")+ToOneLine(val));
			}
			ErrorWriteBuffer+=val;
		}
		public static string ToOneLine(string val) {
			string sReturn=val;
			if (sReturn.Contains(Environment.NewLine)) sReturn=sReturn.Replace(Environment.NewLine,"");
			if (sReturn.Contains("\r")) sReturn=sReturn.Replace("\r","");
			if (sReturn.Contains("\n")) sReturn=sReturn.Replace("\n","");
			return sReturn;
		}
		public static void Error_WriteLine() {
			Console.Error.WriteLine();
			try {if (streamErr!=null) streamErr.WriteLine();}
			catch {}
			if (bCreateBatch) {
				Batch_WriteLine();
			}
			ErrorWriteBuffer="";
		}
		public static void Error_WriteLine(string val) {
			Error_Write(val);
			Error_WriteLine();
		}
		public static void ShowExn(Exception exn, string ParticipleNow) {
			Error_WriteLine();
			Error_Write("Could not finish");
			if (ParticipleNow!=null&&ParticipleNow!="") Error_Write(" while "+ParticipleNow);
			else Error_Write("step");
			Error_WriteLine(":");
			Error_WriteLine(exn.ToString());
		}
		public static string ToCSharpConstant(string[] array) {
			string sReturn="";
			if (array!=null) {
				sReturn+="new string[]{";
				for (int i=0; i<array.Length; i++) {
					sReturn+=((i!=0)?"; ":"")+ToCSharpConstant(array[i]);
				}
				sReturn+="}";
			}
			else sReturn="null";
			return sReturn;
		}
		public static string ToList(ArrayList array, string sListItemOpener_index_becomes_number_indexPLUSSIGN1_can_be_used) {
			string sReturn="";
			int i=0;
			if (array!=null&&array.Count>0) {
				foreach (string val in array) {
					sReturn+=sListItemOpener_index_becomes_number_indexPLUSSIGN1_can_be_used.Replace("index+1",(i+1).ToString()).Replace("index",i.ToString())+val;
					i++;
				}
			}
			else {
				sReturn="n/a";
			}
			return sReturn;
		}
		public static string ToCSharpConstant(ArrayList array) {
			string sReturn="";
			if (array!=null) {
				if (array.Count==0) {
					sReturn="new ArrayList()";
				}
				else {
					sReturn+="new ArrayList(new string[]{";
					int i=0;
					foreach (string val in array) {
						sReturn+=((i!=0)?"; ":"")+ToCSharpConstant(val); //even add "; " on last since c# constant
						i++;
					}
					sReturn+="})";
				}
			}
			else sReturn="null";
			return sReturn;
		}
		public static void ShowErr(string sMsg) {
			if (sMsg!=null) {
				if (!sMsg.ToUpper().Contains("ERROR")&&!sMsg.ToUpper().Contains("WARNING")) sMsg="ERROR: "+sMsg;
				Error_WriteLine(sMsg);
			}
		}
		public static char cDigit0="0"[0];
		public static void Main(string[] args) {
			bool bOK=false;
			if (sDirSep=="\\") {
				sRemark="REM ";
				sMD="md";
				sCP="copy /y";
				Batch_FullName=Batch_FullName_FirstPart+".bat";
			}
			else {
				sRemark="#";
				sMD="mkdir";
				sCP="cp -f";
				Batch_FullName=Batch_FullName_FirstPart+".sh";
			}
			bool bGetLocations=true;
			ConsoleKeyInfo cki=new ConsoleKeyInfo('r', ConsoleKey.R,false,false,false);
			Console.WriteLine("Welcome to ForwardFileSync!");
			while (bGetLocations) {
				Console.WriteLine();
				Console.Write("source:#");
				sSourceRoot=Console.ReadLine();
				Console.Write("destination:#");
				sDestRoot=Console.ReadLine();
				Console.WriteLine();
				Console.WriteLine("source=\""+sSourceRoot+"\"");
				Console.WriteLine("destination=\""+sDestRoot+"\"");
				Console.Write("OK?(y[es]/n[o]/r[etry]): ");
				cki=Console.ReadKey();
				if (cki.Key==ConsoleKey.R) bGetLocations=true;
				else bGetLocations=false;
			}
			if (cki.Key==ConsoleKey.Y) {
				Batch_FullName_FirstPart="ForwardFileSync";
				DateTime dtNow=DateTime.Now;
				sYYYY_MM_DD=dtNow.Year.ToString().PadLeft(4, cDigit0);//Year.ToString().PadLeft(length, cDigit0);
				sYYYY_MM_DD+="-";
				sYYYY_MM_DD+=dtNow.Month.ToString().PadLeft(2, cDigit0);
				sYYYY_MM_DD+="-";
				sYYYY_MM_DD+=dtNow.Day.ToString().PadLeft(2, cDigit0);
				sHH_MM_SS=dtNow.Hour.ToString().PadLeft(2, cDigit0);
				sHH_MM_SS+=":";
				sHH_MM_SS+=dtNow.Minute.ToString().PadLeft(2, cDigit0);
				sHH_MM_SS+=":";
				sHH_MM_SS+=dtNow.Second.ToString().PadLeft(2, cDigit0);
				sDateTimeNow=sYYYY_MM_DD+"@"+sHH_MM_SS;
				if (bCreateBatch) streamBatch=new StreamWriter(Batch_FullName);
				Batch_WriteLine(sRemark+" -- Created "+sDateTimeNow);
				streamFailedSourcesList=new StreamWriter(FailureList_FullName);
				FailedSourcesList_WriteLine(sRemark+" -- Created "+sDateTimeNow);
				sParticiple="initializing output";
				try {
					//DateTime.Now.Year.ToString()+"-"+DateTime.Now.Month.ToString()+"-"+DateTime.Now.Day.ToString()
					try {
						string CopyErrorsFile_FullName="ForwardFileSync Copy Errors.txt";
						if (!bCopyErrorsLogHereInsteadOfCurrentUserDesktop) CopyErrorsFile_FullName=Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+sDirSep+CopyErrorsFile_FullName;
						streamErr=new StreamWriter(CopyErrorsFile_FullName);
						streamErr.WriteLine(sRemark+" -- Created "+sDateTimeNow);
					}
					catch (Exception exn) {
						ShowExn(exn,"opening error log");
					}
					sParticiple="opening source {SourceRoot:"+ToCSharpConstant(sSourceRoot)+"}";
					string sProcessedSource=NoEndSlashUnlessJustSlash(sSourceRoot);
					if (sProcessedSource.EndsWith(":")) sProcessedSource+=@"\";
					diSourceRoot=new DirectoryInfo(sProcessedSource);
					sParticiple="creating destination {SourceRoot:"+ToCSharpConstant(sSourceRoot)+"}";
					try {
						if (!Directory.Exists(sDestRoot)) {
							sParticiple="creating destination {DestRoot:"+ToCSharpConstant(sDestRoot)+"}";
							if (bCreateBatch) {
								Batch_WriteLine(sMD+" \""+sDestRoot+"\""); //there is no need to see if sLasMD already happened to this, because this is the root folder
								LastCreatedDirectory_FullName=sDestRoot;
							}
							else Directory.CreateDirectory(sDestRoot);
						}
					}
					catch (Exception exn) {
						ShowExn(exn,sParticiple);
					}
					string sProcessedDest=NoEndSlashUnlessJustSlash(sDestRoot);
					if (sProcessedDest.EndsWith(":")) sProcessedDest+=@"\";
					sParticiple="opening destination {DestRoot:"+ToCSharpConstant(sDestRoot)+"}";
					diDestRoot=new DirectoryInfo(sDestRoot);
					if (diDestRoot.Exists) {
						if (diSourceRoot.Exists) {
							sParticiple="displaying options";
							bool bKeepGettingPaths=true;
							while (bKeepGettingPaths) {
								Console.WriteLine("Excluding paths: "+((alExcludeFolder_FullNameI_Lower!=null)?ToList(alExcludeFolder_FullNameI_Lower,"\n\t(index+1)  "):"n/a"));
								Console.WriteLine("Do you want to exclude "+((alExcludeFolder_FullNameI_Lower!=null&&alExcludeFolder_FullNameI_Lower.Count>0)?"more":"any (case-insensitive)")+" paths from source (y[es]/n[o]/c[lear])");
								ConsoleKeyInfo ckiMorePaths=Console.ReadKey();
								if (ckiMorePaths.Key==ConsoleKey.C) {
									alExcludeFolder_FullNameI_Lower.Clear();
								}
								else if (ckiMorePaths.Key==ConsoleKey.Y) {
									Console.Write("Path to add (enter when done typing, blank to stop adding):#");
									string sLine=Console.ReadLine();
									if (sLine!=null&&sLine!="") {
										if (alExcludeFolder_FullNameI_Lower==null) alExcludeFolder_FullNameI_Lower=new ArrayList();
										alExcludeFolder_FullNameI_Lower.Add(sLine.ToLower());
									}
									else bKeepGettingPaths=false;
								}
								else bKeepGettingPaths=false;
							}
							bool bKeepGettingFileFullNames=true;
							while (bKeepGettingFileFullNames) {
								Console.WriteLine("Excluding filenames with full path: "+((alExcludeFile_FullNameI_Lower!=null)?ToList(alExcludeFile_FullNameI_Lower,"\n\t(index+1)  "):"n/a"));
								Console.WriteLine("Do you want to exclude "+((alExcludeFile_FullNameI_Lower!=null&&alExcludeFile_FullNameI_Lower.Count>0)?"more":"any (case-insensitive)")+" filenames with path from source (y[es]/n[o]/c[lear])");
								ConsoleKeyInfo ckiMorePaths=Console.ReadKey();
								if (ckiMorePaths.Key==ConsoleKey.C) {
									alExcludeFile_FullNameI_Lower.Clear();
								}
								else if (ckiMorePaths.Key==ConsoleKey.Y) {
									Console.Write("Enter full filename with path to add (enter when done typing, blank to stop adding):#");
									string sLine=Console.ReadLine();
									if (sLine!=null&&sLine!="") {
										if (alExcludeFile_FullNameI_Lower==null) alExcludeFile_FullNameI_Lower=new ArrayList();
										alExcludeFile_FullNameI_Lower.Add(sLine.ToLower());
									}
									else bKeepGettingFileFullNames=false;
								}
								else bKeepGettingFileFullNames=false;
							}
							Console.WriteLine();
							Error_WriteLine("Excluding files starting with: "+((alExcludeFileStartsWithI_Lower!=null)?ToList(alExcludeFileStartsWithI_Lower,"\n\t(index+1)  "):"n/a"));
							Error_WriteLine("Excluding files ending with: "+((alExcludeFileEndsWithI_Lower!=null)?ToList(alExcludeFileEndsWithI_Lower,"\n\t(index+1)  "):"n/a"));
							Error_WriteLine("Excluding any folders named: "+((alExcludeFolder_NameI_Lower!=null)?ToList(alExcludeFolder_NameI_Lower,"\n\t(index+1)  "):"n/a"));
							Error_WriteLine("Excluding full paths: "+((alExcludeFolder_FullNameI_Lower!=null)?ToList(alExcludeFolder_FullNameI_Lower,"\n\t(index+1)  "):"n/a"));
							Error_WriteLine("Excluding full filenames with paths: "+((alExcludeFile_FullNameI_Lower!=null)?ToList(alExcludeFolder_FullNameI_Lower,"\n\t(index+1)  "):"n/a"));
							Error_WriteLine("source: "+diSourceRoot.FullName);
							Error_WriteLine("destination: "+diDestRoot.FullName);
							Console.WriteLine();
							Console.WriteLine("Continue (y/n)? ");
							ConsoleKeyInfo ckiContinue=Console.ReadKey();
							if (ckiContinue.Key==ConsoleKey.Y) {
								Console.Error.WriteLine("Starting...");
								sParticiple="showing source and destination folders";
								string sDestinationFoldersCSharpConstant="{diSourceRoot:"+diSourceRoot.FullName+"; diDestRoot:"+diDestRoot.FullName+"}";
								if (bCreateBatch) Batch_WriteLine(sRemark+sDestinationFoldersCSharpConstant);
								Console.Error.WriteLine(sDestinationFoldersCSharpConstant);
								//DirectoryInfo diSourceRoot=new DirectoryInfo(@"\\TOSHIBA1\My Documents");
								//foreach (FileInfo fiNow in diSourceRoot.GetFiles()) {
								//	Console.Error.WriteLine(fiNow.Name);
								//}
								//Console.Error.WriteLine();
								//Console.Error.WriteLine("Folders:");
								//foreach (DirectoryInfo diSourceSubFolder in diSourceRoot.GetDirectories()) {
								//	Console.Error.WriteLine(diSourceSubFolder.Name);
								//}
								sParticiple="running backup at source root";
								BackupTree(diSourceRoot);
								bOK=true;
							}
							else {
								Console.WriteLine("User cancelled ForwardFileSync.");
							}
						}//end if diSourceRoot.Exists
						else {
							sParticiple="cancelling due to missing source";
							ShowErr("ERROR: Source folder "+ToCSharpConstant(sSourceRoot)+" does not exist--cannot continue!");
						}
					}//end if diDestRoot.Exists
					else {
						sParticiple="cancelling due to missing destination";
						ShowErr("ERROR: Destination folder "+ToCSharpConstant(sDestRoot)+" does not exist--cannot continue!");
					}
				}
				catch (Exception exn) {
					ShowExn(exn,sParticiple);
					//sParticiple="cancelling primary sync method due to exception error";
				}
				sParticiple="closing output files";
				if (bCreateBatch) Batch_WriteLine(sRemark+(bOK?"Finished Successfully":"Failed!"));
				Console.Error.WriteLine((bOK?"Finished Successfully.":"Failed!"));
				try {
					sParticiple="closing error output stream";
					streamErr.Close();
					sParticiple="closing failed sources list";
					streamFailedSourcesList.Close();
					if (bCreateBatch&&streamBatch!=null) {
						sParticiple="closing batch";
						streamBatch.Close();
					}
				}
				catch {
					Console.Error.WriteLine("could not finish "+sParticiple+".");
				}
				Console.Error.WriteLine("A batch file was created called \""+Batch_FullName+"\"");
				Console.Error.WriteLine();
				Console.Error.Write("Press any key to continue . . . ");
			}//end if OK to start
			else {
				Console.WriteLine();
				Console.WriteLine("User cancelled ForwardFileSync.");
				Console.WriteLine("Press any key to continue . . .");
			}
			Console.ReadKey();
		}//end Main
		public static bool ArrayHas(string[] array, string has) {
			if (has!=null&&array!=null) {
				for (int i=0; i<array.Length; i++) {
					if (array[i]==has) return true;
				}
			}
			return false;
		}
		public static bool ArrayHasI(string[] array, string has) {
			if (has!=null&&array!=null) {
				has=has.ToLower();
				for (int i=0; i<array.Length; i++) {
					if (array[i].ToLower()==has) return true;
				}
			}
			return false;
		}
		public static bool ArrayHasI(ArrayList array, string has) {
			if (has!=null&&array!=null) {
				has=has.ToLower();
				foreach (string val in array) {
					if (val.ToLower()==has) return true;
				}
			}
			return false;
		}
		//public static bool ArrayHasI(string[] array, string has) {
		//	if (array!=null&&has!=null&&has!="") {
		//		for (int i=0; i<array.Length; i++) {
		//			if (array[i].ToLower()==has.ToLower()) return true;
		//		}
		//	}
		//	return false;
		//}
		public static bool StartsWithAny(string Needle, string[] StartsWithAnyOfTheseStrings) {
			if (Needle!=null&&StartsWithAnyOfTheseStrings!=null) {
				for (int i=0; i<StartsWithAnyOfTheseStrings.Length; i++) {
					if (Needle.StartsWith(StartsWithAnyOfTheseStrings[i])) return true;
				}
			}
			return false;
		}
		public static bool StartsWithAnyI(string Needle, string[] StartsWithAnyOfTheseStrings) {
			if (Needle!=null&&StartsWithAnyOfTheseStrings!=null) {
				string Needle_ToLower=Needle.ToLower();
				for (int i=0; i<StartsWithAnyOfTheseStrings.Length; i++) {
					if (Needle_ToLower.StartsWith(StartsWithAnyOfTheseStrings[i].ToLower())) return true;
				}
			}
			return false;
		}
		public static bool StartsWithAnyI(string Needle, ArrayList StartsWithAnyOfTheseStrings) {
			if (Needle!=null&&StartsWithAnyOfTheseStrings!=null) {
				string Needle_ToLower=Needle.ToLower();
				foreach (string val in StartsWithAnyOfTheseStrings) {
					if (Needle_ToLower.StartsWith(val.ToLower())) return true;
				}
			}
			return false;
		}
		public static bool EndsWithAny(string Needle, string[] EndsWithAnyOfTheseStrings) {
			if (Needle!=null&&EndsWithAnyOfTheseStrings!=null) {
				for (int i=0; i<EndsWithAnyOfTheseStrings.Length; i++) {
					if (Needle.EndsWith(EndsWithAnyOfTheseStrings[i])) return true;
				}
			}
			return false;
		}
		public static bool EndsWithAnyI(string Needle, string[] EndsWithAnyOfTheseStrings) {
			if ((Needle!=null)&&(EndsWithAnyOfTheseStrings!=null)) {
				string Needle_ToLower=Needle.ToLower();
				for (int i=0; i<EndsWithAnyOfTheseStrings.Length; i++) {
					if (Needle_ToLower.EndsWith(EndsWithAnyOfTheseStrings[i].ToLower())) return true;
				}
			}
			return false;
		}
		public static bool EndsWithAnyI(string Needle, ArrayList EndsWithAnyOfTheseStrings) {
			if ((Needle!=null)&&(EndsWithAnyOfTheseStrings!=null)) {
				string Needle_ToLower=Needle.ToLower();
				foreach (string val in EndsWithAnyOfTheseStrings) {
					if (Needle_ToLower.EndsWith(val.ToLower())) return true;
				}
			}
			return false;
		}
		public static string Repeat(string val, int times) {
			string sReturn="";
			if (val!=null&&val!="") {
				for (int i=0; i<times; i++) {
					sReturn+=val;
				}
			}
			return sReturn;
		}
		public static string ToCSharpConstant(string val) {
			return (val!=null)?("\""+val.Replace("\"","\\\"").Replace("\\","\\\\")+"\""):"null";
		}
		public static string ToCSharpConstant(DirectoryInfo val) {
			return ((val!=null)?("\""+val.FullName+"\""):("null"));
		}
		public static string ToCSharpConstant(FileInfo val) {
			return ((val!=null)?("\""+val.FullName+"\""):("null"));
		}
		public static void BackupTree(DirectoryInfo diSourceParent) {
			string Space_Then_ParentFolderFullName="";
			string DestParentFolder_FullNameSlash=DestParentEquivalentToSourceParentFullNameSlash();
			try {
				if (diSourceParent!=null) Space_Then_ParentFolderFullName=" "+diSourceParent.FullName;
				if (diSourceParent!=null&&diSourceParent.Exists) {
					//Space_Then_ParentFolderFullName=" "+diSourceParent.FullName;
					//if (DestParentNowFullName()!="")
					if (DestParentFolder_FullNameSlash!=""&&!Directory.Exists(DestParentFolder_FullNameSlash)) {
						if (bCreateBatch) {
							if (LastCreatedDirectory_FullName!=DestParentFolder_FullNameSlash) {
								Batch_WriteLine(sMD+" \""+DestParentFolder_FullNameSlash+"\"");
								LastCreatedDirectory_FullName=DestParentFolder_FullNameSlash;
							}
						}
						if (!bTestOnly) Directory.CreateDirectory(DestParentFolder_FullNameSlash);
					}
					foreach (DirectoryInfo diSourceSubFolder in diSourceParent.GetDirectories()) {
						try {
							string DestSubFolder_FullName=DestParentFolder_FullNameSlash+diSourceSubFolder.Name;
							bool SourceSubFolder_Exists=diSourceSubFolder.Exists;
							bool DestSubFolder_Exists=Directory.Exists(DestSubFolder_FullName);
							bool bIgnore=false;
							if (SourceSubFolder_Exists) {
								if (ArrayHasI(alExcludeFolder_NameI_Lower,diSourceSubFolder.Name.ToLower())) {
									bIgnore=true;
								}
								if (ArrayHasI(alExcludeFolder_FullNameI_Lower,diSourceSubFolder.FullName.ToLower())) {
									bIgnore=true;
								}
								if (!bIgnore) {
									Console.Error.WriteLine("Folder"+(DestSubFolder_Exists?"":"*")+": "+DestSubFolder_FullName);
									sarrFolderStack[iFolderDepth]=diSourceSubFolder.Name;
									iFolderDepth++;
									BackupTree(diSourceSubFolder);
									iFolderDepth--;
								}
								else {
									Console.Error.WriteLine("Folder ignored: "+DestSubFolder_FullName);
								}
							}
							else {
								Error_WriteLine("Folder could not be read: "+DestSubFolder_FullName);
							}
						}
						catch (Exception exn) {
							//FailedSourcesList_WriteLine(DestSubFolder_FullName);
							AddFailedSourceFolder(diSourceSubFolder.FullName.ToString()); //just add a comment since couldn't even read it
							ShowExn(exn,"processing folder "+ToCSharpConstant(diSourceSubFolder));
						}
					}//end foreach diSourceSubFolder in diSourceParent
					string DestFile_FullName="";
					foreach (FileInfo fiNow in diSourceParent.GetFiles()) {
						try {
							DestFile_FullName=DestParentFolder_FullNameSlash+fiNow.Name;
							bool DestFile_Exists=false;
							try {
								DestFile_Exists=File.Exists(DestFile_FullName);
							}
							catch (Exception exn) {
								ShowExn(exn,"checking destination ");
							}
							bool bIgnore=false;
							string sNameLower=fiNow.Name.ToLower();
							if ( EndsWithAnyI(sNameLower,alExcludeFileEndsWithI_Lower)
							    || StartsWithAnyI(sNameLower,alExcludeFileStartsWithI_Lower)
							    || ArrayHasI(alExcludeFile_FullNameI_Lower,fiNow.FullName.ToLower()) ) bIgnore=true;
							
							Console.Error.WriteLine((bIgnore?"(ignored)":"")+"Destination"+(DestFile_Exists?"":"*")+": "+DestFile_FullName);
							if (!bIgnore) {
								if (!DestFile_Exists||bForceOverWriteExisting) {
									if (!bTestOnly) fiNow.CopyTo(DestFile_FullName,bForceOverWriteExisting);
									if (bCreateBatch) Batch_WriteLine(sCP+" \""+fiNow.FullName+"\" \""+DestFile_FullName+"\"");
								}
							}
						}
						catch (Exception exn) {
							FailedSourcesList_WriteLine(sCP+" \""+fiNow.FullName+"\" \""+DestFile_FullName+"\"");
							ShowExn(exn,"processing file \""+fiNow.FullName+"\"");
						}
					}//end foreach fiNow in diSourceParent
				}//end if diSourceParent.Exists
				else {
					ShowErr("ERROR: Cannot backup folder "+ToCSharpConstant(diSourceParent)+" because it does not exist.");
				}
			}
			catch (Exception exn) {
				FailedSourcesList_WriteLine(sMD+" \""+DestParentFolder_FullNameSlash+"\""); //only add line to create folder, to distinguish from subfolder reading error above
				ShowExn(exn,"processing parent folder"+Space_Then_ParentFolderFullName);
			}
		}//BackupTree
		public static string NoEndSlashUnlessJustSlash(string sPath) {
			if (sPath!=null) {
				while (sPath.Length>0&&sPath!=sDirSep&&sPath.EndsWith(sDirSep)) {
					if (sPath.Length==1) sPath="";
					else sPath=sPath.Substring(0,sPath.Length-1);
				}
			}
			else {
				sPath="";
			}
			return sPath;
		}
		public static string DestParentEquivalentToSourceParentFullNameSlash() {
			string sReturn=sDestRoot;
			if (!sReturn.EndsWith(sDirSep)) sReturn+=sDirSep;
			for (int iNow=0; iNow<iFolderDepth; iNow++) {
				sReturn+=sarrFolderStack[iNow]+sDirSep;
			}
			return sReturn;
		}
		//public static string NameToDestFullName(string FileOrSubFolder_NameOnly) {
		//	string sReturn=DestParentEquivalentToSourceParentFullNameSlash()+FileOrSubFolder_NameOnly;
		//	//if (!sReturn.EndsWith(sDirSep)) sReturn+=sDirSep;
		//	//sReturn+=FileOrSubFolder_NameOnly;
		//	return sReturn;
		//}
		//public static string NameToDestFullName(string sFolderNameOnly) {
		//	return DestParentNowFullName()+@"\"+sFolderNameOnly;
		//}
	}//end Program class
}//end namespace
