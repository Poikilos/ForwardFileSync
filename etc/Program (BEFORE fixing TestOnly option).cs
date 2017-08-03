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
using System.Threading;//Sleep etc
using System.Reflection;//Application.ExecutablePath etc


using ExpertMultimedia;//Common.cs (not yet synchronized w/ JakeGustafson or other apps that may need to move methods to it (DigitalMusicMC has best of most versions) )

namespace ForwardFileSync {
	class Program {
		public static int iDebugLevel=2;
		public static bool bDebug {
			get {
				return iDebugLevel>1;
			}
		}
		public static bool bMegaDebug {
			get {
				return iDebugLevel>1;
			}
		}
		public static bool bUltraDebug {
			get {
				return iDebugLevel>2;
			}
		}
		
		//public static long ExpectedBytesPerSecond=7999004; //nodesoft.com DiskBench copied (from 500G hard drive to Transcend 8G Flash) (Ooo-3.1.1) 157484384 in 19688ms (7.628MiB/s; ~7812KiB/s ~7999004 B/s,  ~7999b/ms)
		public const long UtcTicks1Millisecond=10000; //(There are 10,000 ticks in a millisecond!!!!!)
		public const long UtcTicks1Second=10000000; //(UtcTicks1Millisecond=10000)*1000
		public const long UtcTicks1Minute=600000000; //(UtcTicks1Millisecond=10000)*1000
		public static long ExpectedMillisecondsPerMByte=131; //19688ms/153793.34375 KB= 0.1280159434728462 ms/KB; 19688ms/150.1888122558594 MB
		//public static bool bDoAutoSyncIfNoneRunByScript=false;
		public static bool bLineNotEnded_Console_Out=false;
		public static bool bIgnoreDriveSpecificFiles=true;
		public static readonly char[] carrInvalidFileNameChars_Windows=new char[] {'\\','/',':','*','?','"','<','>','|'};
		public static bool bMove=false;
		public static bool bOptionsEverShown=false;
		public static string sMyName="ForwardFileSync";
		public static bool bInteractive=true;
		public static string sParticiple="";
		public static string sDeleteIfNotOnSource=null;
		public static long BytesProcessed=0;
		public static long BytesCopyable=0;
		public static long BytesRemovable=0; 
		public static long BytesIgnorable=0;
		public static long FilesProcessed=0;
		public static long FilesCopyable=0;
		public static long FilesRemovable=0;
		public static long FilesIgnorable=0;
		/// <summary>
		/// This is how long to wait after displaying message with "show" command; a decimal number in seconds ("infinity" waits for keypress).
		/// </summary>
		public static string sShowDelay="infinity";
		
		public static bool bTestOnly=false; //true: does not actually copy files
		public static bool bCreateBatch=false; //true: outputs batch instructions to standard output
		//public static bool bCopyErrorsLogHereInsteadOfCurrentUserDesktop=true;
		public static string[] sarrFolderStack=new string[1024];
		public static int iFolderDepth=0;
		public static string sSourceRoot=null;//MUST start as null
		public static string sDestRoot=null;//MUST start as null
		public static string sRemark_WithSpaceIfNeeded="REM ";//fixed at beginning of program
		public static string sMD="md";//fixed at beginning of program
		public static string sCP="copy /y";//fixed at beginning of program
		public static string sMV="move /y";//fixed at beginning of program
		public static string sRM="del";//fixed at beginning of program
		public static string sRmdir="rd";//fixed at beginning of program
		public static string sChmod_plus_w="chmod +w";
		public static string LastCreatedDirectory_FullName="";
		public static ArrayList alInvalidDrives=new ArrayList();
		public static ArrayList alExcludeFolder_NameI_Lower=null;//new string[]{""};//must be lower
		public static ArrayList alExcludeFolder_FullNameI_Lower=null;//MUST be null //new ArrayList(new string[]{@"c:\windows"});//new string[]{""};//must be lower
		public static ArrayList alExcludeFile_FullNameI_Lower=null;//new ArrayList(new string[]{@"c:\pagefile.sys"});
		public static ArrayList alExcludeFileStartsWithI_Lower=null;//new string[] {"itunes library","itunes music library"};//must be lower
		public static ArrayList alExcludeFileEndsWithI_Lower=null;//new ArrayList(new string[] {@"pagefile.sys",@"hiberfil.sys"});//must be lower
		public static ArrayList alCreated=new ArrayList();
		public static string Batch_Name=sMyName+"-DoGeneratedActions"+((Path.DirectorySeparatorChar=='\\')?(".bat"):(".sh"));
		public static string Batch_FullName=sMyName+"-DoGeneratedActions"+((Path.DirectorySeparatorChar=='\\')?(".bat"):(".sh"));//fixed at start (based on Batch_Name_FirstPart)
		public static string PermissionsBatch_Name=sMyName+"-Retry-part1-ResolvePermissionIssues--RunMe-then-delete-to-fix"+((Path.DirectorySeparatorChar=='\\')?(".bat"):(".sh"));
		public static string PermissionsBatch_FullName=sMyName+"-Retry-part1-ResolvePermissionIssues--RunMe-then-delete-to-fix"+((Path.DirectorySeparatorChar=='\\')?(".bat"):(".sh"));//path fixed later (see "PermissionsBatch_FullName=" below)
		public static string RetryFFS_Name=sMyName+"-Retry-part2-AttributeCommands.ffs";
		public static string RetryFFS_FullName=sMyName+"-Retry-part2-AttributeCommands.ffs";//fixed at beginning
		public static string RetryBatch_Name=sMyName+"-Retry-part3-FileCommands.bat";
		public static string CopyErrorsFile_Name=sMyName+" Copy Errors.txt"; //fixed at beginning of program (ONLY if used)
		//public static string LogFolder_NameThenSlashElseBlankNonNull=""; //replaced by LogRootFolder_FullName
		public static string LogRootFolder_FullName="";
		//public static string ScriptFile_Name="script.txt";
		
		public static string sDirSep=char.ToString(Path.DirectorySeparatorChar);
		public static string ErrorWriteBuffer="";
		public static string sYYYY_MM_DD="";//fixed at beginning of program
		public static string sHH_MM_SS="";//fixed at beginning of program
		public static string sDateTimeNow="";
		
		public static DirectoryInfo diSourceRoot=null;
		public static DirectoryInfo diDestRoot=null;
		public static StreamWriter streamErr=null;
		public static StreamWriter streamBatch=null;
		public static StreamWriter streamSetPermissions=null;
		public static StreamWriter streamRetryFailedSourcesBatch=null;
		public static StreamWriter streamRetryFailedSourcesFFS=null;
		
		public static readonly string sDeletableMarker="ThisUsedToStartWithDot-ChangedBy"+sMyName;
		public static decimal dWaitSecondsMax=60;
		public static bool bEverSynchronized=false;
		public static char cDigit0='0';
		public const string sScriptEXT_Lower="ffs";
		public static void OpenLogs() {
			try {
				if (bCreateBatch) streamBatch=new StreamWriter(Batch_FullName);
			}
			catch(Exception exn) {
				ShowExn(exn,"opening repeat all batch ("+SafeString(Batch_FullName,true,true)+") for writing");
			}
			try {
				streamRetryFailedSourcesFFS=new StreamWriter(RetryFFS_FullName+".tmp");
			}
			catch (Exception exn) {
				ShowExn(exn,"opening retry failed sources ffs script for writing");
			}
			try {
				streamRetryFailedSourcesBatch=new StreamWriter(LogRootFolder_FullName+Common.sDirSep+RetryBatch_Name);
			}
			catch (Exception exn) {
				ShowExn(exn,"opening retry failed sources batch for writing");
			}
			
			try {
				streamErr=new StreamWriter(LogRootFolder_FullName+Common.sDirSep+CopyErrorsFile_Name);
			}
			catch (Exception exn) {
				ShowExn(exn,"opening error log for writing");
			}
		}//end OpenLogs
		
		public static void CloseLogs() {
			try {
				if (bCreateBatch&&streamBatch!=null) {
					streamBatch.Close();
					streamBatch=null;
				}
			}
			catch(Exception exn) {
				ShowExn(exn,"closing repeat all batch ("+SafeString(Batch_FullName,true,true)+") for writing");
			}
			try {
				if (streamRetryFailedSourcesFFS!=null) streamRetryFailedSourcesFFS.Close();
				streamRetryFailedSourcesFFS=null;
			}
			catch (Exception exn) {
				ShowExn(exn,"closing retry failed sources ffs script for writing");
			}
			try {
				streamRetryFailedSourcesBatch.Close();
				streamRetryFailedSourcesBatch=null;
			}
			catch (Exception exn) {
				ShowExn(exn,"closing retry failed sources batch for writing");
			}
			
			try {
				streamErr.Close();
				streamErr=null;
			}
			catch (Exception exn) {
				ShowExn(exn,"closing error log for writing");
			}			
		}//end CloseLogs()
		
		public static void Main(string[] args) {
			bool bMainGood=false;
			Console.WriteLine("Welcome to "+sMyName+"!");
			try {
				LogRootFolder_FullName=(Path.DirectorySeparatorChar=='\\')
					?(new FileInfo(Assembly.GetEntryAssembly().Location)).Directory.FullName //if Windows//System.IO.Path.GetFullPath(Application.ExecutablePath) //if Windows
					:System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); //if Nix-like
				if (LogRootFolder_FullName.EndsWith(Common.sDirSep)) {
					if (LogRootFolder_FullName.Length==1) LogRootFolder_FullName="";
					else LogRootFolder_FullName=LogRootFolder_FullName.Substring(0,LogRootFolder_FullName.Length-1);
				}
				Console.Error.WriteLine("Setting log folder to \""+LogRootFolder_FullName+"\"...OK");
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Setting log folder...FAILED (will use current folder)");
				LogRootFolder_FullName="";
				Console.Error.WriteLine(exn.ToString());
			}
			Console.WriteLine("Setting os commands...");
			if (sDirSep=="\\") {
				sRemark_WithSpaceIfNeeded="REM ";
				sMD="md";
				sCP="copy /y";
				sMV="move /y";
				sRM="DEL /F /Q";
				sRmdir="RD /Q";
				sChmod_plus_w="attrib -r";
			}
			else {
				sRemark_WithSpaceIfNeeded="#";
				sMD="mkdir";
				sCP="cp -f";
				sMV="mv -f";
				sRM="rm -f";
				sRmdir="rmdir";
				sChmod_plus_w="chmod +w";
			}
			//NOTE: files below had already had ".bat" or ".sh" set in their initializer line
			Batch_FullName=LogRootFolder_FullName+Common.sDirSep+Batch_Name;
			PermissionsBatch_FullName=LogRootFolder_FullName+Common.sDirSep+PermissionsBatch_Name;
			RetryFFS_FullName=LogRootFolder_FullName+Common.sDirSep+RetryFFS_Name;//unconditional extension since ffs

			sParticiple="initializing output";
			Console.WriteLine(sParticiple+"...");
			DeleteLogs();
			OpenLogs();
			Console.Error.WriteLine("Finding Drives...");
			Common.UpdateSelectableDrivesAndPseudoRoots(false);//driveinfoarrAbsoluteDrives = DriveInfo.GetDrives();//aka GetLogicalDrivesInfo();
			Console.Error.WriteLine("Reading script...");
			if (args!=null&&args.Length>0) {
				for (int iArg=0; iArg<args.Length; iArg++) {
					string sArgNow=args[iArg];
					if (sArgNow!=null) {
						if (sArgNow.Length>=1) {
							if (sArgNow[0]=='"'&&sArgNow[sArgNow.Length-1]=='"') {
								if (sArgNow.Length==2||sArgNow.Length==1) sArgNow="";
								else sArgNow=sArgNow.Substring(1,sArgNow.Length-2);
							}
						}
					}
					if (sArgNow!=null && sArgNow!="") {
						if (!sArgNow.Contains("\"")) {
							try {
								if (File.Exists(sArgNow)) {
									if (sArgNow.ToLower().EndsWith("."+sScriptEXT_Lower)) {
										RunScript(sArgNow);//ScriptFile_Name
									}
									else {
										if (bLineNotEnded_Console_Out) Console.WriteLine();
										bLineNotEnded_Console_Out=false;
										Console.WriteLine("Not a "+sScriptEXT_Lower+" file: "+SafeString(args[iArg],true,true));
									}
								}
								else {
									if (bLineNotEnded_Console_Out) Console.WriteLine();
									bLineNotEnded_Console_Out=false;
									Console.WriteLine("ScriptNotFound: "+SafeString(args[iArg],true,true));
								}
							}
							catch (Exception exn) {
								ShowExn(exn,"checking for script");
							}
						}
						else {
							if (bLineNotEnded_Console_Out) Console.WriteLine();
							bLineNotEnded_Console_Out=false;
							Console.WriteLine("Bad Filename: "+SafeString(args[iArg],true,true));
						}
					}
				}
			}
			
			if (!bEverSynchronized) {
				Console.Error.WriteLine("Files were not synchronized yet.");
			}
			//if (!bEverSynchronized&&bDoAutoSyncIfNoneRunByScript) {
			//	if (bLineNotEnded_Console_Out) Console.WriteLine();
			//	bLineNotEnded_Console_Out=false;
				
			//	Console.WriteLine("Running default synchronize ([available commands:{\"Synchronize\"}] a command can be written as a line in script)...");
			//	if (Synchronize()) bMainGood=true; //unscripted synchronize worked
			//}
			//else bMainGood=true;//TODO: see if scripted Synchronize worked

			try {
				sParticiple="closing error output stream";
				streamErr.Close();
				
				sParticiple="closing fix failed sources ffs";
				streamRetryFailedSourcesFFS.Close();
				sParticiple="deleting old fix failed sources ffs";
				File.Delete(RetryFFS_FullName);
				sParticiple="renaming fix failed sources .ffs.tmp to .ffs";
				File.Move(RetryFFS_FullName+".tmp",RetryFFS_FullName);				
				
				sParticiple="closing retry failed sources batch";
				streamRetryFailedSourcesBatch.Close();
				
				if (bCreateBatch&&streamBatch!=null) {
					sParticiple="closing generated actions batch file";
					streamBatch.Close();
				}
			}
			catch {
				Console.Error.WriteLine("could not finish "+sParticiple+".");
			}
			if (bCreateBatch) Console.Error.WriteLine("A batch file was created called \""+Batch_FullName+"\"");
			
			if (bInteractive) {
				if (bLineNotEnded_Console_Out) Console.WriteLine();
				bLineNotEnded_Console_Out=false;
				Console.WriteLine("Press any key to continue . . .");
				Console.ReadKey();
			}
		}//end Main
		public static void ShowTests() {
			if (bLineNotEnded_Console_Out) Console.WriteLine();
			//string Wildcard=@"c:\abcd.jpg";
			string Literal=@"c:\abcd.jpg";
			string[] Wildcards=new string[]{@"c:\abcd.jpg",@"c:\*.jpg",@"c:\*.jpg",@"c:\abcd.*",@"c:\a*d.jpg",@"c:\a*g",@"?:\abcd.jpg",@"*.jpg",@"*.jpe",@"c:\*",@"C:\*",@"*.*",@"C:\*.*",@"c:\*.*"};
			for (int iWildcard=0; iWildcard<Wildcards.Length; iWildcard++) {
				Console.Write(" \""+Literal+"\" IsLike \""+Wildcards[iWildcard]+"\" : " );
				Console.WriteLine( Common.ToString(IsLike(Literal,Wildcards[iWildcard],true)) );
				bLineNotEnded_Console_Out=false;
			}
			Literal=@"c:\abcd";
			for (int iWildcard=0; iWildcard<Wildcards.Length; iWildcard++) {
				Console.Write(" \""+Literal+"\" IsLike \""+Wildcards[iWildcard]+"\" : " );
				Console.WriteLine( Common.ToString(IsLike(Literal,Wildcards[iWildcard],true)) );
				bLineNotEnded_Console_Out=false;
			}
		}
		
		public static bool SafeAdd(ref long var1, long var2) {
			//TODO: implement negatives (four "if" cases)
			bool bWorked=false;
			try {
				if (long.MaxValue-var2>=var1-var2) { //>= is okay since result when "==" would be long.MaxValue and not overflow
					var1+=var2;
					bWorked=true;
				}
			}
			catch {
				bWorked=false;
				var1=long.MaxValue;
			}
			return bWorked;
		}
		/// <summary>
		/// Synchronize will delete files not on source
		/// BEFORE moving otherwise ALL FILES WOULD BE DELETED when bMove is true!
		/// </summary>
		public static bool bDeleteDestFilesNotOnSource {
			get {
				return ToBool(sDeleteIfNotOnSource);
			}
			set {
				sDeleteIfNotOnSource=value?"true":"false";
			}
		}
		public static void RetryFailedSourcesBatch_WriteLine(string sLine) {
			try {
				if (streamRetryFailedSourcesBatch!=null) streamRetryFailedSourcesBatch.WriteLine(sLine);
			}
			catch {}
		}
		public static void RetryFailedSourcesFFS_WriteLine(string sLine) {
			try {
				if (streamRetryFailedSourcesFFS!=null) streamRetryFailedSourcesFFS.WriteLine(sLine);
			}
			catch {}
		}
		public static void AddFailedSourceFile(string File_FullName) {
			RetryFailedSourcesBatch_WriteLine(sRemark_WithSpaceIfNeeded+"--FAILED to "+(bMove?"move":"copy")+" from file:"+File_FullName);
		}
		public static void AddFailedSourceFolder(string Folder_FullName) {
			try {
				if (Folder_FullName!=sDirSep&&Folder_FullName.EndsWith(sDirSep)) Folder_FullName=Folder_FullName.Substring(0,Folder_FullName.Length-1);
				RetryFailedSourcesBatch_WriteLine(sRemark_WithSpaceIfNeeded+"--FAILED to "+(bMove?"move":"copy")+" from folder:"+Folder_FullName);
			}
			catch {}
		}
		public static void Batch_Write(string val) {
			if (bCreateBatch) {
				try {  if (streamBatch!=null) { streamBatch.Write(val); streamBatch.Flush(); }  }
				catch{}
			}
		}
		public static void Batch_WriteLine() {
			if (bCreateBatch) {
				try {  if (streamBatch!=null) { streamBatch.WriteLine(); }  }
				catch{}
			}
		}
		public static void Batch_WriteLine(string val) {
			if (bCreateBatch) {
				Batch_Write(val);
				Batch_WriteLine();
			}
		}
		public static void Error_Write(string val) {
			Console.Error.Write(val);
			try {if (streamErr!=null) streamErr.Write(val);}
			catch {}
			if (bCreateBatch) {
				Batch_Write(((ErrorWriteBuffer=="")?sRemark_WithSpaceIfNeeded:"")+ToOneLine(val));
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
		public static string ToList(string[] array, string sListItemOpener_index_becomes_number_indexPLUSSIGN1_can_be_used) {
			string sReturn="";
			int i=0;
			if (array!=null&&array.Length>0) {
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
		/// <summary>
		/// returns true if found
		/// </summary>
		/// <param name="val"></param>
		/// <param name="sEnding"></param>
		/// <returns></returns>
		public static bool RemoveEndsWith(ref string val, string sEnding) { //formerly SafeRemoveEnding
			bool bFound=false;
			//int iEndBefore=0;
			if (val!=null) {
				if (val.Length>0) {
					//iEndBefore=val.Length;
					while (val.EndsWith(sEnding)) {
						if (val.Length==sEnding.Length) {
							val="";
							bFound=true;
							break;
						}
						else {
							bFound=true;
							val=val.Substring(0,val.Length-sEnding.Length);//MUST be done this way (unless CompareAt is used) since checking for a string not char
						}
					}
				}
			}
			return bFound;
		}//end RemoveEndsWith
		public static void RemoveEndLine(ref string val) {
			while (RemoveEndsWith(ref val, Environment.NewLine)) {
			}
			while (RemoveEndsWith(ref val, "\r")||RemoveEndsWith(ref val, "\n")) {
			}
		}
		public static string SafeString(string val, bool bShowIfNonNull, bool bQuoteIfNonNullAndShowNullIfNull) {
			return (val!=null)
				? ( bShowIfNonNull ? ((bQuoteIfNonNullAndShowNullIfNull?"\"":"")+val+(bQuoteIfNonNullAndShowNullIfNull?"\"":"")) : "non-null" ) //non-null
				: (bQuoteIfNonNullAndShowNullIfNull?"null":""); //null
		}
		public static void SafeDelete(string File_RelOrFullName, bool bWriteToBatchOutput, bool bWriteToRetryBatchIfFails) {
			try {
				if (File.Exists(File_RelOrFullName)) {
					if (bWriteToBatchOutput) {
						if (File_RelOrFullName!=null&&File_RelOrFullName!="") Batch_WriteLine(sRM+" "+SafeString(File_RelOrFullName,true,true));
						else Batch_WriteLine(Program.sRemark_WithSpaceIfNeeded+sRM+" "+SafeString(File_RelOrFullName,true,true));
					}
					(new FileInfo(File_RelOrFullName)).Attributes=FileAttributes.Normal;
					File.Delete(File_RelOrFullName);
				}
			}
			catch (Exception exn) {
				if (bWriteToRetryBatchIfFails) {
					if (File_RelOrFullName!=null&&File_RelOrFullName!="") RetryFailedSourcesBatch_WriteLine(sRM+" "+SafeString(File_RelOrFullName,true,true));
					else RetryFailedSourcesBatch_WriteLine(Program.sRemark_WithSpaceIfNeeded+" "+sRM+" "+SafeString(File_RelOrFullName,true,true));
				}
				ShowExn(exn,"deleting file {File_RelOrFullName:"+SafeString(File_RelOrFullName,true,true)+"}");
				Console.Error.WriteLine();
				Console.Error.WriteLine("(You can still continue)");
				Console.Error.WriteLine();
			}
		}//end SafeDelete
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sParam">can equal a decimal number of seconds, or equal "infinity"</param>
		/// <param name="sMessage"></param>
		public static void WaitUsingStringParam(string sParam, string sMessage) {
			try {
				if (sMessage!=null) Console.WriteLine(sMessage);//intentionally NOT indented
				string sIndent="   ";
				if (sParam.ToLower()=="infinity") {
					if (bLineNotEnded_Console_Out) Console.WriteLine();
					bLineNotEnded_Console_Out=false;
					Console.WriteLine(sIndent+"Press any key to continue (wait:infinity) . . .");
					ConsoleKeyInfo cki=Console.ReadKey();
				}
				else {
					decimal dSeconds;
					bool bNumber=decimal.TryParse(sParam,out dSeconds);
					if (bNumber) {
						if (dSeconds<=dWaitSecondsMax) {
							if (bLineNotEnded_Console_Out) Console.WriteLine();
							bLineNotEnded_Console_Out=false;
							Console.WriteLine(sIndent+"Continuing automatically in "+dSeconds.ToString("#.0")+" second(s) (from "+DateTime.Now.Hour+":"+DateTime.Now.Minute+":"+DateTime.Now.Second+").");
							Thread.Sleep((int)(dSeconds*1000.0M));
						}
						else {
							Thread.Sleep((int)(dWaitSecondsMax*1000.0M));
							throw new ApplicationException(sIndent+"Wait was too long: limited to "+dWaitSecondsMax.ToString());
						}
					}
					else {
						throw new ApplicationException(sIndent+"Failed to parse number (custom exception)");
					}
				}
			}
			catch (Exception exn) {
				Console.Error.WriteLine("Could not finish WaitUsingStringParam:");
				Console.Error.WriteLine(exn.ToString());
				Console.Error.WriteLine();
			}
		}//WaitUsingStringParam
		public static string ConvertSymbolicPathToLiteral(string sPathWithSymbols) {
			string sParam_Processed=Common.ReplaceSpecialFolders(sPathWithSymbols);
			string sParam_AsLiteralPath=sParam_Processed;
			if (sParam_Processed.Contains("[")) {
				sParam_AsLiteralPath=Common.ReplaceDriveLabelInBracketsWithDriveSlash(sParam_Processed);
				if (sParam_Processed==sParam_AsLiteralPath) {
					Console.Error.WriteLine();
					Console.Error.WriteLine();
					Console.Error.WriteLine();
					Console.Error.WriteLine("Could not find drive label in brackets for \""+sParam_Processed+"\"");
					//Console.Beep(800,500);
					//Console.Beep(37,2000);
					Console.Beep(1400,400);
					Console.Beep(1000,700);
					Console.Beep(40,2000);
				}
			}
			sParam_Processed=sParam_AsLiteralPath;
			return sParam_Processed;
		}//end ConvertSymbolicPathToLiteral
		/// <summary>
		/// Run a script line
		/// </summary>
		/// <param name="sLine">script (one line) to run</param>
		/// <param name="iLine">for error output only</param>
		public static void RunScriptLine(string sLine, int iLine) {
			try {
				RemoveEndLine(ref sLine);//TODO: RemoveEndsWhitespace
				string sLine_ToLower=sLine.ToLower();
				if (sLine!=null&&sLine!="") {
					if (!sLine_ToLower.StartsWith("#")) {
						int iOperator=sLine.IndexOf(':');
						if ( iOperator<0 || (iOperator>0&&iOperator<(sLine.Length-1)) ) { //NOT ok to be zero or last character
							string sCommandLower="";
							string sParam="";
							string sParamLower="";
							if (iOperator>=0) {//NEVER ==0 since outer case prevents line from running in that case
								sCommandLower=sLine.Substring(0,iOperator).ToLower();
								sParam=sLine.Substring(iOperator+1,sLine.Length-(iOperator+1));
								sParamLower=sParam.ToLower();
							}
							else {
								sCommandLower=sLine_ToLower;
								sParam="";
								sParamLower="";
							}
								
							if (sCommandLower=="waitseconds" || sCommandLower=="wait") {
								WaitUsingStringParam(sParam,(sParam.ToLower()=="infinity")?"press any key":"");
							}//end waitseconds
							else if (sCommandLower=="logfolder") {
								LogRootFolder_FullName=sParam;//LogRootFolder_FullName+Common.sDirSep+
								try {
									if (!Directory.Exists(LogRootFolder_FullName)
									    && !Common.IsAbsolutePath(LogRootFolder_FullName)
									   ) {
										try {
											Directory.CreateDirectory(LogRootFolder_FullName);
											Program.AllowFolderToBeAccessibleByAllAdministrators(LogRootFolder_FullName);
										}
										catch (Exception exn) {
											ShowExn(exn,"creating log folder {logfolder:"+SafeString(LogRootFolder_FullName,true,true)+"}");
										}
									}
									if (Directory.Exists(LogRootFolder_FullName)) {
										if (LogRootFolder_FullName!=""&&!LogRootFolder_FullName.EndsWith(sDirSep)) {
											LogRootFolder_FullName+=sDirSep;
										}
										if (bLineNotEnded_Console_Out) Console.WriteLine();
										bLineNotEnded_Console_Out=false;
										Console.WriteLine("Log folder is now "+SafeString(LogRootFolder_FullName,true,true)+".");
									}
									else {
										Console.Error.WriteLine("Log folder does not exist ("+SafeString(LogRootFolder_FullName,true,true)+") so current folder will be used.");
										LogRootFolder_FullName="";
									}
									
									//REMOVE TRAILING SLASH:
									
									if (LogRootFolder_FullName.EndsWith(Common.sDirSep)) {
										if (LogRootFolder_FullName.Length==1) LogRootFolder_FullName="";
										else LogRootFolder_FullName=LogRootFolder_FullName.Substring(0,LogRootFolder_FullName.Length-1);
									}
								}
								catch (Exception exn) {
									Console.Error.WriteLine("Log folder was an unusable path ("+SafeString(LogRootFolder_FullName,true,true)+") so current folder will be used.");
									LogRootFolder_FullName="";
								}
								
								/*
								LogFolder_RelOrFullNameThenSlashElseBlankNonNull=sParam;
								//if (LogFolder_RelOrFullNameThenSlashElseBlankNonNull.EndsWith(sDirSep)) {
								//	LogFolder_RelOrFullNameThenSlashElseBlankNonNull=LogFolder_RelOrFullNameThenSlashElseBlankNonNull.SubString(LogFolder_RelOrFullNameThenSlashElseBlankNonNull.Length-1);
								//}
								try {
									if (!Directory.Exists(LogFolder_RelOrFullNameThenSlashElseBlankNonNull)
									    && !IsAbsolutePath(LogFolder_RelOrFullNameThenSlashElseBlankNonNull)
									   ) {
										try {
											Directory.CreateDirectory(LogFolder_RelOrFullNameThenSlashElseBlankNonNull);
										}
										catch (Exception exn) {
											ShowExn(exn,"creating log folder {logfolder:"+SafeString(LogFolder_RelOrFullNameThenSlashElseBlankNonNull,true,true)+"}");
										}
									}
									if (Directory.Exists(LogFolder_RelOrFullNameThenSlashElseBlankNonNull)) {
										if (LogFolder_RelOrFullNameThenSlashElseBlankNonNull!=""&&!LogFolder_RelOrFullNameThenSlashElseBlankNonNull.EndsWith(sDirSep)) {
											LogFolder_RelOrFullNameThenSlashElseBlankNonNull+=sDirSep;
										}
										if (bLineNotEnded_Console_Out) Console.WriteLine();
										bLineNotEnded_Console_Out=false;
										Console.WriteLine("Log folder is now "+SafeString(LogFolder_RelOrFullNameThenSlashElseBlankNonNull,true,true)+".");
									}
									else {
										Console.Error.WriteLine("Log folder does not exist ("+SafeString(LogFolder_RelOrFullNameThenSlashElseBlankNonNull,true,true)+") so current folder will be used.");
										LogFolder_RelOrFullNameThenSlashElseBlankNonNull="";
									}
								}
								catch (Exception exn) {
									Console.Error.WriteLine("Log folder was an unusable path ("+SafeString(LogFolder_RelOrFullNameThenSlashElseBlankNonNull,true,true)+") so current folder will be used.");
									LogFolder_RelOrFullNameThenSlashElseBlankNonNull="";
								}
								*/
							}//end logfolder
							else if (sCommandLower=="excludefolderfullname") {
								if (alExcludeFolder_FullNameI_Lower==null) alExcludeFolder_FullNameI_Lower=new ArrayList();
								alExcludeFolder_FullNameI_Lower.Add(sParamLower);
							}
							else if (sCommandLower=="excludefilefullname") {
								if (alExcludeFile_FullNameI_Lower==null) alExcludeFile_FullNameI_Lower=new ArrayList();
								if (sParamLower!="none") {
									alExcludeFile_FullNameI_Lower.Add(sParamLower);
								}
							}
							else if (sCommandLower=="excludeanyfoldernamed") {
								if (alExcludeFolder_NameI_Lower==null) alExcludeFolder_NameI_Lower=new ArrayList();
								alExcludeFolder_NameI_Lower.Add(sParamLower);
							}
							else if (sCommandLower=="excludeanyfileendingwith") {
								if (alExcludeFileEndsWithI_Lower==null) alExcludeFileEndsWithI_Lower=new ArrayList();
								alExcludeFileEndsWithI_Lower.Add(sParamLower);
							}
							else if (sCommandLower=="source") {
								string sParam_Processed=ConvertSymbolicPathToLiteral(sParam);
								SetSource(sParam_Processed);
							}
							else if (sCommandLower=="destination") {
								string sParam_Processed=ConvertSymbolicPathToLiteral(sParam);
								SetDestination(sParam_Processed);
							}
							else if (sCommandLower=="deleteifnotonsource") {
								sDeleteIfNotOnSource=sParam;
							}
							else if (sCommandLower=="showdelay") {
								sShowDelay=sParam;
							}
							else if (sCommandLower=="showoptions") {
								ShowOptions();
							}
							else if (sCommandLower=="show") {
								string sMessage=sParam;
								if ( sParam.Length>2 && sParam.StartsWith("\"") && sParam.EndsWith("\"") ) {
									if (bLineNotEnded_Console_Out) Console.WriteLine();
									bLineNotEnded_Console_Out=false;
									sMessage=sParam.Substring(1,sParam.Length-2).Replace("\\\"","\"");
								}
								WaitUsingStringParam(sShowDelay,sMessage);
							}
							else if (sCommandLower=="interactive") { //formerly waitonexit
								bInteractive=ToBool(sParamLower);
							}
							else if (sCommandLower=="createbatch") {
								bCreateBatch=ToBool(sParam);
							}
							else if (sCommandLower=="doautosyncifnonerunbyscript") {
								Console.Error.WriteLine("Warning: used deprecated command (autosync must be placed in each script now if synchronize is desired) {"+Common.sLastFileUsed+"("+(iLine+1).ToString()+",0):\""+sLine.Replace("\"","\\\"")+"\"}");
								//bDoAutoSyncIfNoneRunByScript=ToBool(sParam);
							}
							else if (sCommandLower=="testonly") {
								bTestOnly=ToBool(sParam);
							}
							else if (sCommandLower=="move") {
								bMove=ToBool(sParamLower);
							}
							else if (sParamLower.StartsWith("attribs ")) {
								sParticiple="parsing attribs command";
								bool bWorked=false;
								string[] sarrParams=null;
								int iWorked=0;
								try {
									sParticiple="splitting attribs subcommands";
									sarrParams=Common.SplitSpaceDelimitedParams(sParam,false,true);
									if (sarrParams!=null) {
										int iNow=0;
										sParticiple="getting fileinfo object";
										FileInfo fiChmod=new FileInfo(sarrParams[iNow]);
										Console.Write("ATTRIBS "+SafeString(sarrParams[iNow],true,true)+" ");
										Console.Out.Flush();
										iWorked=0;
										for (iNow=1; iNow<sarrParams.Length; iNow++) {
											sParticiple="parsing attribute subcommand ["+iNow.ToString()+"]";
											if (sarrParams[iNow]!=null) {
												int iSign=sarrParams[iNow].IndexOf('=');
												if (iSign>0&&iSign<(sarrParams[iNow].Length-1)) {
													bool bUnknown=false;
													sParticiple="parsing substrings for attribute subcommand ["+iNow.ToString()+"]";
													string sSubCommand_Name=sarrParams[iNow].Substring(0,iSign);
													string sSubCommand_Name_Lower=sSubCommand_Name.ToLower();
													string sSubCommand_Value=sarrParams[iNow].Substring(iSign+1);
													long iSubCommand_Value;
													sParticiple="parsing number for attribute subcommand ["+iNow.ToString()+"]";
													bool bSubCommand_Value=long.TryParse(sSubCommand_Value,out iSubCommand_Value);
													if (bSubCommand_Value) {
														sParticiple="setting write to allow set attribute subcommand ["+iNow.ToString()+"]";
														FileAttributes fiPrev=fiChmod.Attributes;
														fiChmod.Attributes=FileAttributes.Normal;
														if (sSubCommand_Name_Lower=="lastwritetimeutc.ticks") {
															sParticiple="setting LastWriteTime for attribute subcommand ["+iNow.ToString()+"]";
															DateTime dtNew=new DateTime(iSubCommand_Value,DateTimeKind.Utc);
															fiChmod.LastWriteTimeUtc=dtNew;
															sParticiple="setting back to original attributes since done time-change-only attribute subcommand ["+iNow.ToString()+"]";
															fiChmod.Attributes=fiPrev;
														}
														else if (sSubCommand_Name_Lower=="creationtimeutc.ticks") {
															sParticiple="setting CreationTime for attribute subcommand ["+iNow.ToString()+"]";
															DateTime dtNew=new DateTime(iSubCommand_Value,DateTimeKind.Utc);
															fiChmod.CreationTimeUtc=dtNew;
															sParticiple="setting back to original attributes since done time-change-only attribute subcommand ["+iNow.ToString()+"]";
															fiChmod.Attributes=fiPrev;
														}
														else {
															bUnknown=true;
															sParticiple="setting back to original attributes since unknown attribute subcommand ["+iNow.ToString()+"]";
															fiChmod.Attributes=fiPrev;
														}
														if (!bUnknown) {
															Console.Write(" "+SafeString(sarrParams[iNow],true,false));
															iWorked++;
														}
														else Console.Write(" UNKNOWNATTRIB:"+SafeString(sarrParams[iNow],true,false));
													}
													else Console.Write(" UNUSABLENUMBERAFTERSIGN:"+SafeString(sarrParams[iNow],true,false));
												}
												else Console.Write(" DONOTHING:"+SafeString(sarrParams[iNow],true,false));
											}
											else Console.Write(" DONOTHING:null");
											Console.Out.Flush();
										}
										Console.WriteLine("");
										if (iWorked<sarrParams.Length-1) {
											bWorked=false;
											Error_Write("Failed to set attributes {set:"+(sarrParams.Length-1).ToString()+"; iWorked:"+iWorked+"}");
										}
										else bWorked=true;
										
									}//if sarrParams!=null
								}
								catch (Exception exn) {
									bWorked=false;
									Error_Write("scripted ATTRIBS command could not finish while "+sParticiple+" {sarrParams"+((sarrParams!=null)?(".Length:"+sarrParams.Length):(":null"))+"; iWorked:"+iWorked.ToString()+"}");
								}
								if (!bWorked) {
									RetryFailedSourcesFFS_WriteLine(sLine);
								}
							}//end startswith "attribs "
							else if (sCommandLower=="synchronize") {
								//sSourceRoot and sDestRoot must be good already or this will stop prog with error
								Synchronize();
							}
							else if (sCommandLower=="deletelogs") {
								CloseLogs();
								DeleteLogs();
								OpenLogs();
							}
							else if (sCommandLower=="showtests") {
								ShowTests();
							}
							else if (sCommandLower=="excludedest") {
								alInvalidDrives.Add(sParam);
								Common.UpdateSelectableDrivesAndPseudoRoots(false);
								if ( Common.CountSelectableDrives() > 0) {//driveinfoarrAbsoluteDrives != null && driveinfoarrAbsoluteDrives.Length > 0) {
									//do nothing since everything is ok
								}//end if there are any drives on computer
								else {
									throw new ApplicationException("no drives found (this should never happen)");
								}
							}//end excludedest
							else {
								throw new ApplicationException("unknown command {sCommandLower:"+SafeString(sCommandLower,true,true)+"} (custom exception)");
							}
						}
						else {
							throw new ApplicationException("no ':' operator (custom exception)");
						}
					}
					//else is comment
				}
				//else is blank line
			}
			catch (Exception exn) {
				ShowExn(exn,"processing line {sLine:"+SafeString(sLine,true,true)+"}");
			}
		}//end RunScriptLine
		public static void RunScript(string File_Name) {
			string sLine=null;
			try {
				FileInfo fiScript=new FileInfo(File_Name);
				Common.sLastFileUsed=File_Name;
				if (fiScript.Exists) {
					StreamReader streamIn=fiScript.OpenText();
					int iLine=0;
					while ( (sLine=streamIn.ReadLine()) != null ) {
						RunScriptLine(sLine, iLine);
						iLine++;
					}//end while lines in script
					streamIn.Close();
				}//end if script exists
			}
			catch (Exception exn) {
				ShowExn(exn,"processing script {sLine:"+SafeString(sLine,true,true)+"}");
			}			
		}//end RunScript
		public static bool ToBool(string val) {
			return (val.ToLower()=="yes")||(val=="1")||(val.ToLower()=="true")||(val.ToLower()=="on");
		}
		public static void ShowOptions() {
			bOptionsEverShown=true;//TODO: change to bOptionsSelected and set whenever options change (via selecting OR script) instead
			try {
				Console.Clear();
			}
			catch {
				//ok since only fails upon output being redirected
			}
			try {
				Console.WriteLine();
				Error_WriteLine("Wait on exit: "+(bInteractive?"yes":"no"));
				if (bCreateBatch) Error_WriteLine("Create "+SafeString(Batch_FullName,true,true)+": "+(bCreateBatch?"yes":"no"));
				else Error_WriteLine("No batch will be created.");
				if (bTestOnly) Error_WriteLine("TEST MODE (no "+(bMove?"move":"copy")+" commands will be invoked during synchronize)");
				else Error_WriteLine("Program will "+(bMove?"move":"copy")+" files.");
				Console.WriteLine();
				Console.WriteLine("Excluded files WILL still be deleted if don't exist on source.");
				Error_WriteLine("Excluding files starting with: "+((alExcludeFileStartsWithI_Lower!=null)?ToList(alExcludeFileStartsWithI_Lower,"\n\t(index+1)  "):"n/a"));
				Error_WriteLine("Excluding files ending with: "+((alExcludeFileEndsWithI_Lower!=null)?ToList(alExcludeFileEndsWithI_Lower,"\n\t(index+1)  "):"n/a"));
				Console.WriteLine("Excluded folders WILL still be deleted if don't exist on source.");
				Error_WriteLine("Excluding any folders named: "+((alExcludeFolder_NameI_Lower!=null)?ToList(alExcludeFolder_NameI_Lower,"\n\t(index+1)  "):"n/a"));
				Error_WriteLine("Excluding full paths: "+((alExcludeFolder_FullNameI_Lower!=null)?ToList(alExcludeFolder_FullNameI_Lower,"\n\t(index+1)  "):"n/a"));
				if (bIgnoreDriveSpecificFiles) Error_WriteLine("Excluding full paths of drive-specific files (with wildcard): "+((alExcludeFolder_FullNameI_Lower!=null)?ToList(sarrDriveSpecificFolder_FullNames_ToUpper,"\n\t(index+1)  "):"n/a"));
				else {
					Error_WriteLine("Warning: drive-specific files (recycle bins and System Volume Information folders) are being copied in this mode.");
				}
				Error_WriteLine("Excluding full filenames with paths (neither "+(bMove?"moved":"copied")+" nor deleted): "+((alExcludeFile_FullNameI_Lower!=null)?ToList(alExcludeFile_FullNameI_Lower,"\n\t(index+1)  "):"n/a"));
				//Error_WriteLine("Will automatically run if no \"Command:Synchronize\" line is present: "+Common.ToString(bDoAutoSyncIfNoneRunByScript));
				Error_WriteLine(); 
				string sSourceMessage="(NOT FOUND)";
				if (diSourceRoot!=null&&diSourceRoot.Exists) sSourceMessage="(Found)";
				string sDestMessage="(NOT FOUND)";
				if (diDestRoot!=null&&diDestRoot.Exists) sDestMessage="(Found)";
				Error_WriteLine("  Source"+sSourceMessage+":" + ((diSourceRoot!=null)?(" "+diSourceRoot.FullName):"null") );
				Error_WriteLine("  Destination"+sDestMessage+":" + ((diDestRoot!=null)?(" "+diDestRoot.FullName):"null") );//+diDestRoot.FullName);
				Error_WriteLine("  Delete on destination if not on source: "+(bDeleteDestFilesNotOnSource?"yes":"no"));
				Console.WriteLine();
			}
			catch (Exception exn) {
				Error_WriteLine("ShowOptions could not finish while accessing source and dest! ("+exn.ToString()+")");
				Console.WriteLine();
			}
		}//end ShowOptions
		public static void SetSource(string Folder_FullName) {
			sSourceRoot=Folder_FullName;
			sParticiple="opening source {SourceRoot:"+ToCSharpConstant(sSourceRoot)+"}";
			string sProcessedSource=NoEndSlashUnlessJustSlash(sSourceRoot);
			if (sProcessedSource.EndsWith(":")) sProcessedSource+=@"\";
			if (!sProcessedSource.StartsWith("[")) diSourceRoot=new DirectoryInfo(sProcessedSource);
			else diSourceRoot=null;
		}
		public static void SetDestination(string Folder_FullName) {
			sDestRoot=Folder_FullName;
			sParticiple="opening destination {DestRoot:"+ToCSharpConstant(sDestRoot)+"}";
			string sProcessedDest=NoEndSlashUnlessJustSlash(sDestRoot);
			if (sProcessedDest.EndsWith(":")) sProcessedDest+=@"\";
			if (!sProcessedDest.StartsWith("[")) diDestRoot=new DirectoryInfo(sProcessedDest);
			else diDestRoot=null;
		}
		/// <summary>
		/// Remember to CloseLogs() then call this then OpenLogs() in that order whenever calling this.
		/// </summary>
		public static void DeleteLogs() {
			SafeDelete(Batch_FullName,false,false);
			SafeDelete(LogRootFolder_FullName+Common.sDirSep+RetryBatch_Name,false,false);
			SafeDelete(LogRootFolder_FullName+Common.sDirSep+CopyErrorsFile_Name,false,false);
			SafeDelete(RetryFFS_FullName,false,false);
		}
		public static void AllowFolderToBeAccessibleByAllAdministrators(string Folder_FullName) {
			try {
				//REMOVE TRAILING SLASH:
				if (Folder_FullName.EndsWith(Common.sDirSep)) Folder_FullName=Folder_FullName.Substring(0,Folder_FullName.Length-1);
				if (streamSetPermissions==null) {
					//bool bExisted=File.Exists(PermissionsBatch_FullName);
					if (File.Exists(PermissionsBatch_FullName)) {
						StreamWriter streamSetPermissionsBAK=new StreamWriter(PermissionsBatch_FullName+".bak.bat",/*append*/true);
						//APPEND to backup infinitely (program or user that calls batch should delete every time it is run)!
						StreamReader srSetPermissions=new StreamReader(PermissionsBatch_FullName);
						string sLine=null;
						while ( (sLine=srSetPermissions.ReadLine()) != null ) {
							streamSetPermissionsBAK.WriteLine(sLine);
						}
						srSetPermissions.Close();
						streamSetPermissionsBAK.Close();
						File.Delete(PermissionsBatch_FullName);
					}
					streamSetPermissions=new StreamWriter(PermissionsBatch_FullName);
					if (
						//!bExisted&&
						(Path.DirectorySeparatorChar!='\\') )
						streamSetPermissions.WriteLine("#!/bin/sh");
				}
				
				if (Path.DirectorySeparatorChar=='\\') {
					streamSetPermissions.WriteLine("takeown /f \""+Folder_FullName+"\" /r /d y");
					streamSetPermissions.WriteLine("icacls \""+Folder_FullName+"\" /grant administrators:F /t");
				}
				else {
					//chmod syntax:
					// chmod [options] [who][operator][attributes]
					// where
					// [who] is u for user who owns file, g for group that owns the file, o is for others, and a is for all (same as ugo)
					// [operator] is '-' to remove, '+' to add, and '=' to set exact attributes
					// [attributes] are
					// r read
					// w write
					// x execute
					// X special execute (if folder, or any [who] has x, e.g. for quickly setting recursively: chmod -R a+rX)
					// s setuid/gid
					// t sticky
					// [options] are
					//   -R recursive
					streamSetPermissions.WriteLine("chmod ug+r \""+Folder_FullName+"\"");
					//streamSetPermissions.WriteLine("chgrp "+sNixUser+" \""+Folder_FullName+"\"");
					//streamSetPermissions.WriteLine("chown "+sNixUser+" \""+Folder_FullName+"\"");
				}
			}
			catch (Exception exn) {
				Console.Error.WriteLine();
				Console.Error.WriteLine("Could not finish AllowFolderToBeAccessibleByAllAdministrators: "+exn.ToString());
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sPathConsideredOKEvenIfNoSubfolderExists"></param>
		/// <returns>returns true if root exists even if subfolder doesn't exist</returns>
		public static bool VerifyRoot(string sPath, bool bCreateSubfolderIfMissing) {//formerly GoodFolder
			bool bFoundRoot=false;
			try {
				if (!sPath.StartsWith("[")) {
					if (sPath!=null&&sPath!="") {
						if (sPath.Length==3&&sPath[1]==':'&&sPath[2]=='\\') {//is a windows drive
							bFoundRoot=Directory.Exists(sPath);
						}
						else if (sPath.Length==2&&sPath[1]==':') {//is a windows drive without trailing slash so add that before accessing
							bFoundRoot=Directory.Exists(sPath+"\\");
						}
						else {//else is a folder
							if (bCreateSubfolderIfMissing) {
								string sExnReturned="";
								Common.CreateFolderRecursively(out sExnReturned, ref alCreated, sPath);
								//if running as administrator, all folders in alFoldersCreated should be changed like this (for vista to allow all administrators full control):
								//"takeown /f "+sFolderNow+" /r /d y"
								//"icacls "+sFolderNow+" /grant administrators:F /t"
								//where sFolderNow is each string in alFoldersCreated
								if (alCreated!=null&&alCreated.Count>0) {
									foreach (string sFolderNowOrig in alCreated) {
										string sFolderNow=sFolderNowOrig;
										AllowFolderToBeAccessibleByAllAdministrators(sFolderNow);
									}
								}
								alCreated.Clear();
							}
							bFoundRoot=Directory.Exists(sPath);
						}//else full folder path (not a windowsish-root)
					}//end if path not blank
				}
				else {//is a path that is missing a root
					bFoundRoot=false;
				}
			}
			catch (Exception exn) {
				Console.Error.WriteLine(exn.ToString());
			}
			return bFoundRoot;
		}//end VerifyRoot
		public static void WriteTimeStrings(string sCommandNote_Noun) {
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
						
			Batch_WriteLine(sRemark_WithSpaceIfNeeded+" -- Starting "+sCommandNote_Noun+" "+sDateTimeNow);
			RetryFailedSourcesFFS_WriteLine("# -- Starting "+sCommandNote_Noun+" "+sDateTimeNow);//# is ok since it is a platform-independent ffs file
			RetryFailedSourcesBatch_WriteLine(sRemark_WithSpaceIfNeeded+" -- Starting "+sCommandNote_Noun+" "+sDateTimeNow);
			try {
				//if (!bCopyErrorsLogHereInsteadOfCurrentUserDesktop) CopyErrorsFile_Name=Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+sDirSep+CopyErrorsFile_FullName;
				streamErr.WriteLine(sRemark_WithSpaceIfNeeded+" -- Starting "+sCommandNote_Noun+" "+sDateTimeNow);
			}
			catch (Exception exn) {
				ShowExn(exn,"writing date to error log");
			}
		}
		public static bool Synchronize() {
			bEverSynchronized=true;
			bool bReturnGood=false;
			bool bGetLocations=true;
			ConsoleKeyInfo cki=new ConsoleKeyInfo('y', ConsoleKey.Y,false,false,false);//default is yes (continue)
			if (!VerifyRoot(sSourceRoot,false)) sSourceRoot=null;
			if (!VerifyRoot(sDestRoot,true)) sDestRoot=null;
			if (sSourceRoot==null||sDestRoot==null) {
				if (!bInteractive) {
					Console.Clear();
					Console.WriteLine();
					Console.WriteLine();
					Console.WriteLine();
					Console.Write("  BACKUP CAN'T FIND DRIVE OR LOCATION for ");
					string sMissing="";
					if (sSourceRoot==null) sMissing="source";
					if (sDestRoot==null) sMissing=((sMissing!="")?" and ":"")+"destination";
					Console.WriteLine(sMissing+"!");
					Console.WriteLine();
					Console.WriteLine();
					Console.WriteLine();
					Console.WriteLine("Insert drive then run Backup again.");
					Console.WriteLine("press any key to exit...");
					cki=Console.ReadKey();
				}
				else {//else interactive
					Console.WriteLine();
					while (bGetLocations) {
						string UserSource_FullName=null;
						string UserDest_FullName=null;
						if (sSourceRoot==null) {
							Console.Write("source:#");
							UserSource_FullName=Console.ReadLine();
							SetSource(UserSource_FullName);
						}
						if (sDestRoot==null) {
							Console.Write("destination:#");
							UserDest_FullName=Console.ReadLine();
							SetDestination(UserDest_FullName);
						}
						
						Console.WriteLine();
						Console.WriteLine("source=\""+sSourceRoot+"\"");
						Console.WriteLine("destination=\""+sDestRoot+"\"");
						Console.Write("OK?(y[es]/n[o]/c[hange]): ");
						cki=Console.ReadKey();
						if (cki.Key==ConsoleKey.C) bGetLocations=true;
						else bGetLocations=false;
					}
				}
				
			}//end if dest or source null (not specified by script)
			else cki=new ConsoleKeyInfo('y', ConsoleKey.Y,false,false,false);
			if (sSourceRoot!=null&&sDestRoot!=null) {
				if (cki.Key==ConsoleKey.Y) {
					WriteTimeStrings("Synchronize");
					
					try {
						//DateTime.Now.Year.ToString()+"-"+DateTime.Now.Month.ToString()+"-"+DateTime.Now.Day.ToString()
						sParticiple="creating destination {SourceRoot:"+ToCSharpConstant(sSourceRoot)+"}";
						try {
							if (!Directory.Exists(sDestRoot)) {
								sParticiple="creating destination {DestRoot:"+ToCSharpConstant(sDestRoot)+"}";
								if (bCreateBatch) {
									Batch_WriteLine(sMD+" \""+sDestRoot+"\""); //there is no need to see if sLasMD already happened to this, because this is the root folder
									LastCreatedDirectory_FullName=sDestRoot;
								}
								else {
									Directory.CreateDirectory(sDestRoot);
									Program.AllowFolderToBeAccessibleByAllAdministrators(sDestRoot);
								}
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
								
								bool bKeepGettingPaths=bInteractive;
								if (alExcludeFolder_FullNameI_Lower!=null) bKeepGettingPaths=false;
								while (bKeepGettingPaths) {
									Console.WriteLine();
									Console.WriteLine();
									Console.WriteLine("Excluding paths: "+((alExcludeFolder_FullNameI_Lower!=null)?ToList(alExcludeFolder_FullNameI_Lower,"\n\t(index+1)  "):"n/a"));
									Console.WriteLine(" Exclude "+((alExcludeFolder_FullNameI_Lower!=null&&alExcludeFolder_FullNameI_Lower.Count>0)?"more":"any (case-insensitive)")+" paths from source (y[es]/n[o]/c[lear])");
									ConsoleKeyInfo ckiMorePaths=Console.ReadKey();
									if (ckiMorePaths.Key==ConsoleKey.C) {
										alExcludeFolder_FullNameI_Lower.Clear();
									}
									else if (ckiMorePaths.Key==ConsoleKey.Y) {
										Console.Write("Path to add (enter when done typing, blank to stop adding):#");
										string UserInputExcludeFolder_FullName=Console.ReadLine();
										if (UserInputExcludeFolder_FullName!=null&&UserInputExcludeFolder_FullName!="") {
											if (alExcludeFolder_FullNameI_Lower==null) alExcludeFolder_FullNameI_Lower=new ArrayList();
											alExcludeFolder_FullNameI_Lower.Add(UserInputExcludeFolder_FullName.ToLower());
										}
										else bKeepGettingPaths=false;
									}
									else bKeepGettingPaths=false;
								}
								bool bKeepGettingFileFullNames=bInteractive;
								if (alExcludeFile_FullNameI_Lower!=null) bKeepGettingFileFullNames=false;
								while (bKeepGettingFileFullNames) {//starts true if interactive AND doesn't have exclusions already
									Console.WriteLine();
									Console.WriteLine();
									Console.WriteLine("Excluding filenames with full path: "+((alExcludeFile_FullNameI_Lower!=null)?ToList(alExcludeFile_FullNameI_Lower,"\n\t(index+1)  "):"n/a"));
									Console.WriteLine(" Exclude "+((alExcludeFile_FullNameI_Lower!=null&&alExcludeFile_FullNameI_Lower.Count>0)?"more":"any (case-insensitive)")+" filenames with path from source (y[es]/n[o]/c[lear])");
									ConsoleKeyInfo ckiMorePaths=Console.ReadKey();
									if (ckiMorePaths.Key==ConsoleKey.C) {
										alExcludeFile_FullNameI_Lower.Clear();
									}
									else if (ckiMorePaths.Key==ConsoleKey.Y) {
										Console.Write("Enter full filename with path to add (enter when done typing, blank to stop adding):#");
										string UserInputExcludeFile_FullName=Console.ReadLine();
										if (UserInputExcludeFile_FullName!=null&&UserInputExcludeFile_FullName!="") {
											if (alExcludeFile_FullNameI_Lower==null) alExcludeFile_FullNameI_Lower=new ArrayList();
											alExcludeFile_FullNameI_Lower.Add(UserInputExcludeFile_FullName.ToLower());
										}
										else bKeepGettingFileFullNames=false;
									}
									else bKeepGettingFileFullNames=false;
								}
								Console.WriteLine();
								if (sDeleteIfNotOnSource==null) {
									Console.WriteLine();
									Console.Write("Delete files on destination that are not on the source (all files and folders included regardless of exclusions) (y[es]/n[o])?");
									ConsoleKeyInfo ckeyDeleteIfNotOnSrc=Console.ReadKey();
									if (ckeyDeleteIfNotOnSrc.Key==ConsoleKey.Y) bDeleteDestFilesNotOnSource=true;
									else bDeleteDestFilesNotOnSource=false;
								}
								//else {
								//	Console.WriteLine("Delete if not on source:"+sDeleteIfNotOnSource);
								//}
								ConsoleKeyInfo ckiContinue=new ConsoleKeyInfo('y', ConsoleKey.Y,false,false,false);
								if (bInteractive) {
									if (!bOptionsEverShown) { //TODO: avoid interactive stuff when bInteractive==false
										ShowOptions();
										Console.WriteLine("    Continue (y/n)? ");
										ckiContinue=Console.ReadKey();
									}
								}
								//else ckiContinue=new ConsoleKeyInfo('y', ConsoleKey.Y,false,false,false);
								
								
								if (ckiContinue.Key==ConsoleKey.Y) {
									Console.Error.WriteLine();
									Console.Error.WriteLine("Starting...");
									sParticiple="showing source and destination folders";
									string sDestinationFoldersCSharpConstant="{diSourceRoot:"+diSourceRoot.FullName+"; diDestRoot:"+diDestRoot.FullName+"}";
									if (bCreateBatch) Batch_WriteLine(sRemark_WithSpaceIfNeeded+sDestinationFoldersCSharpConstant);
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
									sParticiple="deleting file that do not exist on source";
									if (bDeleteDestFilesNotOnSource) {
										if (bLineNotEnded_Console_Out) Console.WriteLine();
										bLineNotEnded_Console_Out=false;
										Console.WriteLine("Looking for files to delete from destination that no longer exist on source drive");
										DeleteFileOnDestNotOnSource(new DirectoryInfo(sDestRoot));
									}
									if (bLineNotEnded_Console_Out) Console.WriteLine();
									bLineNotEnded_Console_Out=false;
									Console.WriteLine("Synchronizing folders");
									sParticiple="running backup at source root";
									BackupTree(diSourceRoot);
									if (sDirSep=="\\") {
										if (bMove) {
											MakeTreeChangeable(diSourceRoot);
											DeleteEmptyFoldersInTree(diSourceRoot);
										}
									}
									bReturnGood=true;
								}
								else {
									if (bLineNotEnded_Console_Out) Console.WriteLine();
									bLineNotEnded_Console_Out=false;
									Console.WriteLine("User cancelled Synchronize ("+sMyName+").");
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
					if (bCreateBatch) {
						Batch_WriteLine("");
						Batch_WriteLine(sRemark_WithSpaceIfNeeded+(bReturnGood?"Finished Successfully":"Failed!"));
					}
					Console.Error.WriteLine();
					Console.Error.WriteLine();
					Console.Error.WriteLine((bReturnGood?"Finished Successfully.":"Failed!"));
					Error_WriteLine(sRemark_WithSpaceIfNeeded+"{"
				                +"Copyable:"+(FilesCopyable).ToString()+" files; "
				                +"Removed:"+(FilesRemovable).ToString()+" files; "
				                +"AddedNonexistent:"+(FilesCopyable-FilesRemovable).ToString()+" files; "
				                +"Processed:"+(FilesProcessed).ToString()+" files; "
				                +"Ignored:"+(FilesIgnorable).ToString()+" files"
				                +"Copyable:"+(BytesCopyable/1024/1024).ToString()+"MiB; "
				                +"Removed:"+(BytesRemovable/1024/1024).ToString()+"MiB; "
				                +"Difference:"+(BytesCopyable/1024/1024-BytesRemovable/1024/1024).ToString()+"MiB; "
				                +"Processed:"+(BytesProcessed/1024/1024).ToString()+"MiB; "
				                +"Ignored:"+(BytesIgnorable/1024/1024).ToString()+"MiB"
				                +"}");
					if (!bReturnGood) Error_WriteLine(sRemark_WithSpaceIfNeeded+"Finished:FAILED");
					else Error_WriteLine(sRemark_WithSpaceIfNeeded+"Finished:OK");
					Console.Error.WriteLine();
				}//end if OK to start
				else {
					Console.WriteLine();
					Console.WriteLine("User cancelled Command:Synchronize ("+sMyName+").");
				}
				try {
					if (streamSetPermissions!=null) {
						streamSetPermissions.Close();
						streamSetPermissions=null;
					}
				}
				catch (Exception exn) {
					Console.WriteLine("Could not finish closing set permissions batch: "+exn.ToString());
				}
			}//end if dest and source strings are both non-null
			else {//if either NULL (not found)
				if (bInteractive) {
					Console.Write("  BACKUP CAN'T FIND HAND-TYPED DRIVE OR LOCATION for ");
					string sMissing="";
					if (sSourceRoot==null) sMissing="source";
					if (sDestRoot==null) sMissing=((sMissing!="")?" and ":"")+"destination";
					Console.WriteLine(sMissing+"!");
					Console.WriteLine();
					Console.WriteLine();
					Console.WriteLine("press any key to exit...");//if interactive & no location
					cki=Console.ReadKey();
				}
				//else message already shown if !bInteractive (see beginning of method)
			}
			return bReturnGood;
		}//end Synchronize
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
		public static readonly string[] sarrDriveSpecificFolder_FullNames_ToUpper=new string[]{"?:\\$RECYCLE.BIN","?:\\RECYCLER","?:\\SYSTEM VOLUME INFORMATION"};
		/// <summary>
		/// (two wildcards in a row is unpredictable but this method tries to treat the second as a literal to match to the next character in the Literal string).
		/// </summary>
		/// <param name="Literal"></param>
		/// <param name="Wildcard"></param>
		/// <param name="bAllowStarDotStarToRepresentStar">Allows *.* to match to files without an extension (changes instances of "*.*" in Wildcard to "*")</param>
		/// <returns></returns>
		public static bool IsLike(string Literal, string Wildcard, bool bAllowStarDotStarToRepresentStar) {
			bool bMatch=false;
			if (bUltraDebug) Console.Write("IsLike(){matching:");
			if (Literal!=null&&Literal.Length>0&&Wildcard!=null&&Wildcard.Length>0) {
				if (bAllowStarDotStarToRepresentStar) Wildcard=Wildcard.Replace("*.*","*");
				int iMustMatch=Literal.Length;
				int iWildcard=0;
				int iLiteral=0;
				while (iLiteral<Literal.Length&&iWildcard<Wildcard.Length) {
					if (Wildcard[iWildcard]=='?') { 
						if (bUltraDebug) Console.Write("[?~="+char.ToString(Literal[iLiteral])+"]");
						iWildcard++;
						iLiteral++;
					}
					else if (Wildcard[iWildcard]=='*') {
						if (bUltraDebug) Console.Write("[*~="+char.ToString(Literal[iLiteral]));
						if (iWildcard+1<Wildcard.Length&&Wildcard[iWildcard+1]==Literal[iLiteral]) {
						//MUST come before (iLiteral+1==Literal.Length)  in order for ending wildcard to work, otherwise won't be advanced +=2!
							if (bUltraDebug) Console.Write("(WildcardConcludedByMatch)]");
							iLiteral++;
							iWildcard+=2;//go past '*' and matched character
						}
						else if (iLiteral+1==Literal.Length) {
							if (bUltraDebug) Console.Write("(WildcardFullyConcluded)]");
							iLiteral++;//denotes that Literal got fully matched
							iWildcard++;//denotes that Wildcard got fully matched
						}
						else if (iWildcard+1==Wildcard.Length) {
							if (bUltraDebug) Console.Write("(++AtEnd)]");
							iLiteral++;
						}
						else {
							if (bUltraDebug) Console.Write("(++)]");
							iLiteral++;//OK since always matches '*'
						}
					}
					else if (Wildcard[iWildcard]==Literal[iLiteral]) {
						if (bUltraDebug) Console.Write("["+char.ToString(Wildcard[iWildcard])+"=="+char.ToString(Literal[iLiteral])+"]");
						iWildcard++;
						iLiteral++;
					}
					else {
						if (bUltraDebug) Console.Write("["+char.ToString(Wildcard[iWildcard])+"!="+char.ToString(Literal[iLiteral])+"]");
						break;//not a match
					}
				}
				if ( (iLiteral==Literal.Length) && (iWildcard==Wildcard.Length-1) && (Wildcard[Wildcard.Length-1]=='*') ) {
					if (bUltraDebug) Console.Write("["+char.ToString(Wildcard[iWildcard])+"(ConcludingEndingWildcard)]");
					iWildcard++;
				}
				if ( (iWildcard==Wildcard.Length) && (iLiteral==Literal.Length) ) bMatch=true;
			}//if both strings are non-blank
			if (bUltraDebug) Console.Write("}");
			return bMatch;
		}//end IsLike
		public static bool ArrayWildcardHas(string[] Haystacks, string Needle) {
			bool bFound=false;
			if (Haystacks!=null) {
				if (Haystacks!=null) {
					for (int iHaystack=0; iHaystack<Haystacks.Length; iHaystack++) {
						if (IsLike(Needle,Haystacks[iHaystack],true)) {
							bFound=true;
							break;
						}
					}
				}
			}
			return bFound;
		} //ArrayWildcardHas
		public static void DeleteFileOnDestNotOnSource(DirectoryInfo diDestParent) {
			string Space_Then_ParentFolderFullName="";
			sParticiple="determining equivalent source path for removing files no longer on source";
			string SourceParentFolder_FullNameSlash=SourceParentEquivalentToDestParentFullNameSlash();
			if (bDebug) {
				if (bLineNotEnded_Console_Out) Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine("SourceForCompare:"+SourceParentFolder_FullNameSlash);
				bLineNotEnded_Console_Out=true;
			}
			else {
				Console.Write(".");
				bLineNotEnded_Console_Out=false;
			}
			bool bIn=false;
			try {
				sParticiple="accessing parent DirectoryInfo for removing files no longer on source";
				if (diDestParent!=null) Space_Then_ParentFolderFullName=" "+diDestParent.FullName;
				if (diDestParent!=null&&diDestParent.Exists) {
					sParticiple="checking for folders to delete from destination where no longer on source";
					bool bIgnore;
					bIgnore=false;
					if (bIgnoreDriveSpecificFiles) {
						if (ArrayWildcardHas(sarrDriveSpecificFolder_FullNames_ToUpper,diDestParent.FullName.ToUpper())) {
							bIgnore=true;
						}
					}
					//do NOT skip deletion of excluded files
					//if (ArrayHas(alExcludeFolder_NameI_Lower,diDestParent.Name.ToLower()) bIgnore=true;
					//if (ArrayHas(Program.alExcludeFolder_FullNameI_Lower,diDestParent.FullName.ToLower()) bIgnore=true;
					if (!bIgnore) {
						foreach (DirectoryInfo diDestSubFolder in diDestParent.GetDirectories()) {
							
							string SourceSubFolder_FullName=SourceParentFolder_FullNameSlash+diDestSubFolder.Name;//intentionally add dest name to source path to check whether exists
							bool DestSubFolder_Exists=diDestSubFolder.Exists;
							bool SourceSubFolder_Exists=Directory.Exists(SourceSubFolder_FullName);						
							sarrFolderStack[iFolderDepth]=diDestSubFolder.Name;
							iFolderDepth++;
							bIn=true;
							
							DeleteFileOnDestNotOnSource(diDestSubFolder);
							iFolderDepth--;
							bIn=false;
						}
						sParticiple="deleting files on destination that are no longer on source";
					
						foreach (FileInfo fiDest in diDestParent.GetFiles()) {
							string SourceFile_FullName=SourceParentFolder_FullNameSlash+fiDest.Name;//intentionally add dest name to source path to check whether exists
							bIgnore=false;
							if (!File.Exists(SourceFile_FullName)&&!bIgnore) {
								BytesRemovable+=fiDest.Length;
								FilesRemovable++;
								Batch_WriteLine(sChmod_plus_w+" \""+fiDest.FullName+"\"");
								try {
									fiDest.Attributes=FileAttributes.Normal;
								}
								catch (Exception exn) {
									ShowExn(exn,"making file writable");
									Program.RetryFailedSourcesBatch_WriteLine(sChmod_plus_w+" \""+fiDest.FullName+"\"");
								}
								if (bLineNotEnded_Console_Out) Console.WriteLine();
								Console.WriteLine("Delete:"+fiDest.FullName);
								bLineNotEnded_Console_Out=false;
								Batch_WriteLine(sRM+" \""+fiDest.FullName+"\"");
								try {
									fiDest.Delete();
								}
								catch (Exception exn) {
									ShowExn(exn,"deleting destination file that does not exist on source");
									Program.RetryFailedSourcesBatch_WriteLine(sRM+" \""+fiDest.FullName+"\"");
								}
							}
						}
						if (!Directory.Exists(SourceParentFolder_FullNameSlash)) {
							sParticiple="deleting folder on destination that are no longer on source";
							Batch_WriteLine(sChmod_plus_w+" \""+diDestParent.FullName+"\"");
							diDestParent.Attributes=FileAttributes.Normal;
							if (bLineNotEnded_Console_Out) Console.WriteLine();
							bLineNotEnded_Console_Out=false;
							Console.WriteLine("Delete:"+diDestParent.FullName);
							Batch_WriteLine(sRmdir+" \""+diDestParent.FullName+"\"");
							diDestParent.Delete(true);
						}
						else sParticiple="skipping (not deleting) destination folder that still exists on source";
					}//if not a drive-specific folder to leave alone
				}
			}
			catch (Exception exn) {
				if (bIn) iFolderDepth--;
				ShowExn(exn,sParticiple);
			}
		}//end DeleteFileOnDestNotOnSource
		
		public static void MakeTreeChangeable(DirectoryInfo diParent) {
			if (sDirSep=="\\") {
				if (diParent.Name.StartsWith(".")) {
					string ChangeableParent_FullName=diParent.Parent.FullName+sDirSep+Program.sDeletableMarker+diParent.Name;
					while (ChangeableParent_FullName.Contains(sDirSep+sDirSep)) ChangeableParent_FullName=ChangeableParent_FullName.Replace(sDirSep+sDirSep,sDirSep);
					try {
						FileAttributes faPrev=diParent.Attributes;
						diParent.Attributes=FileAttributes.Normal;
						diParent.MoveTo(ChangeableParent_FullName);
						diParent.Attributes=faPrev;
					}
					catch {}
				}
				foreach (FileInfo fiNow in diParent.GetFiles()) {
					if (fiNow.Name.StartsWith(".")) {
						try {
							string Changeable_FullName=fiNow.Directory.FullName+sDirSep+sDeletableMarker+fiNow.Name;
							FileAttributes faPrev=fiNow.Attributes;
							fiNow.Attributes=FileAttributes.Normal;
							fiNow.MoveTo(Changeable_FullName);
							fiNow.Attributes=faPrev;
						}
						catch {}
					}
				}
				foreach (DirectoryInfo diSub in diParent.GetDirectories()) {
					MakeTreeChangeable(diSub);
				}
			}
		}
		public static void DeleteEmptyFoldersInTree(DirectoryInfo diParent) {
			foreach (DirectoryInfo diSub in diParent.GetDirectories()) {
				DeleteEmptyFoldersInTree(diSub);
			}
			try {
				if (diParent.GetDirectories().Length==0
				    &&diParent.GetFiles().Length==0) {
					diParent.Attributes=FileAttributes.Normal;
					diParent.Delete(false);
				}
				else {
					Batch_WriteLine(sRemark_WithSpaceIfNeeded+"not empty:"+SafeString(diParent.FullName,true,true));
				}
			}
			catch {}
		}
		/// <summary>
		/// Either copies or moves, according to bMove
		/// </summary>
		/// <param name="fiSource"></param>
		/// <param name="DestFile_FullName"></param>
		public static void SafeCopyOrMoveIfNotTestOnly(FileInfo fiSource, FileInfo fiDest, string DestFile_FullName) {
			BytesCopyable+=fiSource.Length;
			FilesCopyable++;
			if (fiDest.Exists) {
				BytesRemovable+=fiDest.Length;
				FilesRemovable++;
				try {
					try {
						Batch_WriteLine(sChmod_plus_w+" "+SafeString(DestFile_FullName,true,true));
						fiDest.Attributes=FileAttributes.Normal;
					}
					catch {
						RetryFailedSourcesBatch_WriteLine(sChmod_plus_w+" "+SafeString(DestFile_FullName,true,true));
					}
					fiDest.Delete();
				}
				catch {
					RetryFailedSourcesBatch_WriteLine(sRM+" "+SafeString(DestFile_FullName,true,true));
				}
			}
			if (bMove) Batch_WriteLine(Program.sMV+" "+SafeString(fiSource.FullName,true,true)+" "+SafeString(DestFile_FullName,true,true));
			else Batch_WriteLine(Program.sCP+" "+SafeString(fiSource.FullName,true,true)+" "+SafeString(DestFile_FullName,true,true));
			if (!bTestOnly) {
				if (bMove) {
					try {
						fiSource.MoveTo(DestFile_FullName);
					}
					catch {
						RetryFailedSourcesBatch_WriteLine(Program.sMV+" "+SafeString(fiSource.FullName,true,true)+" "+SafeString(DestFile_FullName,true,true));
					}
				}
				else {
					try {
						fiSource.CopyTo(DestFile_FullName);
					}
					catch {
						RetryFailedSourcesBatch_WriteLine(Program.sCP+" "+SafeString(fiSource.FullName,true,true)+" "+SafeString(DestFile_FullName,true,true));
					}
					try {
						FileInfo fiDestNew=new FileInfo(DestFile_FullName);
						bool bForceWriteAttribs=(fiDestNew.Attributes&FileAttributes.ReadOnly)==FileAttributes.ReadOnly;
						FileAttributes fattribsPrev=fiDestNew.Attributes;
						if (bForceWriteAttribs) {
							fiDestNew.Attributes=FileAttributes.Normal;
						}
						fiDestNew.CreationTimeUtc=fiSource.CreationTimeUtc;
						fiDestNew.LastWriteTimeUtc=fiSource.LastWriteTimeUtc;
						if (bForceWriteAttribs) {
							fiDestNew.Attributes=fattribsPrev;
						}
					}
					catch {
						RetryFailedSourcesFFS_WriteLine("Command:Attribs "+SafeString(DestFile_FullName,true,true)+" LastWriteTimeUtc.Ticks="+fiSource.LastWriteTimeUtc.Ticks.ToString()+" CreationTimeUtc.Ticks="+fiSource.CreationTimeUtc.Ticks.ToString());
					}
				}
			}
		}//end SafeCopyOrMoveIfNotTestOnly
		public static void BackupTree(DirectoryInfo diSourceParent) {
			string Space_Then_ParentFolderFullName="";
			string DestParentFolder_FullNameSlash=DestParentEquivalentToSourceParentFullNameSlash();
			bool bIn=false;
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
						if (!bTestOnly) {
							Directory.CreateDirectory(DestParentFolder_FullNameSlash);
							Program.AllowFolderToBeAccessibleByAllAdministrators(DestParentFolder_FullNameSlash);
						}
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
								if (bIgnoreDriveSpecificFiles) {
									
									if (ArrayWildcardHas(sarrDriveSpecificFolder_FullNames_ToUpper,diSourceSubFolder.FullName.ToUpper())) {
										bIgnore=true;
									}
								}
								
								if (!bIgnore) {
									if (DestSubFolder_Exists) {
										if (bDebug) {
											if (bLineNotEnded_Console_Out) Console.WriteLine();
											Console.WriteLine("--CheckingFolder: "+DestSubFolder_FullName);
											bLineNotEnded_Console_Out=false;
											
										}
										else {
											Console.Write(".");
											bLineNotEnded_Console_Out=true;
										}
									}
									else {
										if (bLineNotEnded_Console_Out) Console.WriteLine();
										bLineNotEnded_Console_Out=false;
										Console.WriteLine("Folder*: "+DestSubFolder_FullName);
									}
									//Console.WriteLine("Folder"+(DestSubFolder_Exists?"":"*")+": "+DestSubFolder_FullName);
									sarrFolderStack[iFolderDepth]=diSourceSubFolder.Name;
									iFolderDepth++;
									bIn=true;
									BackupTree(diSourceSubFolder);
									iFolderDepth--;
									bIn=false;
								}
								else {
									if (bLineNotEnded_Console_Out) Console.WriteLine();
									bLineNotEnded_Console_Out=false;
									Console.WriteLine("Folder ignored: "+DestSubFolder_FullName);
								}
							}
							else {
								Error_WriteLine("Folder could not be read: "+DestSubFolder_FullName);
							}
						}
						catch (Exception exn) {
							if (bIn) iFolderDepth--;
							//RetryFailedSourcesBatch_WriteLine(DestSubFolder_FullName);
							AddFailedSourceFolder(diSourceSubFolder.FullName.ToString()); //just add a comment since couldn't even read it
							ShowExn(exn,"processing folder "+ToCSharpConstant(diSourceSubFolder));
						}
					}//end foreach diSourceSubFolder in diSourceParent
					string DestFile_FullName="";
					string sSrcMod="[src.mod=]";//was .
					long FileTimeFudgeAllowedIn100NanosecondUtcTicks=5*UtcTicks1Second;//adjusted to filesize for each file below
					foreach (FileInfo fiSourceNow in diSourceParent.GetFiles()) {
						try {
							DestFile_FullName=DestParentFolder_FullNameSlash+fiSourceNow.Name;
							FileInfo fiDest=new FileInfo(DestFile_FullName);
							//bool DestFile_Exists=false;
							//try {
							//	DestFile_Exists=fiDest.Exists;
							//}
							//catch (Exception exn) {
							//	ShowExn(exn,"checking destination ");
							//}
							bool bIgnore=false;
							string sNameLower=fiSourceNow.Name.ToLower();
							if ( EndsWithAnyI(sNameLower,alExcludeFileEndsWithI_Lower)
							    || StartsWithAnyI(sNameLower,alExcludeFileStartsWithI_Lower)
							    || ArrayHasI(alExcludeFile_FullNameI_Lower,fiSourceNow.FullName.ToLower()) ) bIgnore=true;
							//done AFTER knowing if replaceable now://Console.Error.WriteLine((bIgnore?"(ignored)":"")+"Destination"+(fiDest.Exists?"":"*")+": "+DestFile_FullName);
							bool bSourceIsNewer_OrHasSameDateAndDestIsIncomplete=false;
							string sResultChar=bIgnore?"[excluded]":"[not checked (this should never happen)]";//? is fixed below //was string sResultChar=bIgnore?"#":"?"
							
							if (!bIgnore) {
								if (fiDest.Exists) { //DestFile_Exists) {
									long fiSourceNow_Length_ToMB=(long)fiSourceNow.Length/1024/1024;
									try {
										FileTimeFudgeAllowedIn100NanosecondUtcTicks=5*UtcTicks1Second+UtcTicks1Millisecond*ExpectedMillisecondsPerMByte*2*fiSourceNow_Length_ToMB; //*2 for computers that are half the speed of mine.  allows for copying time
									}
									catch (Exception exn) {
										FileTimeFudgeAllowedIn100NanosecondUtcTicks=5*UtcTicks1Minute;
										ShowExn(exn,"getting time allowed between start and end of previous time file was written");
									}
									
									//if ( (fiDest.Length!=fiSourceNow.Length)
									//    || (fiDest.LastWriteTimeUtc.CompareTo(fiSourceNow.LastWriteTimeUtc)!=0) )
									if ( Common.EqualTo(fiDest.LastWriteTimeUtc.Ticks,fiSourceNow.LastWriteTimeUtc.Ticks,FileTimeFudgeAllowedIn100NanosecondUtcTicks)) {
									//this has to happen FIRST no matter what since in case they are not exactly equal to in date (allows for write time)
										sResultChar=sSrcMod; //was .
									}
									//TODO: make 'if' clauses start over here??? there is duplication of case above in case below
									else if ( Common.EqualTo(fiDest.LastWriteTimeUtc.Ticks,fiSourceNow.LastWriteTimeUtc.Ticks,FileTimeFudgeAllowedIn100NanosecondUtcTicks) && fiDest.Length!=fiSourceNow.Length) {
										sResultChar="[src.len=]";//was ^
										bSourceIsNewer_OrHasSameDateAndDestIsIncomplete=true;
									}
									else if ( Common.LessThan(fiDest.LastWriteTimeUtc.Ticks,fiSourceNow.LastWriteTimeUtc.Ticks,FileTimeFudgeAllowedIn100NanosecondUtcTicks) ) {
										sResultChar="[dest.mod<]"; //was *
										bSourceIsNewer_OrHasSameDateAndDestIsIncomplete=true;
									}
									else if ( Common.EqualTo(fiDest.LastWriteTimeUtc.Ticks,fiSourceNow.LastWriteTimeUtc.Ticks,FileTimeFudgeAllowedIn100NanosecondUtcTicks)
											&& (fiDest.Length>fiSourceNow.Length) ) {
										RetryFailedSourcesBatch_WriteLine(sRemark_WithSpaceIfNeeded+" NEWER DESTINATION "+fiDest.LastWriteTimeUtc.ToString()+" same date but larger than source (or not modified on source since first copied):");
										//NOTE: intentionally ignore bMove in order to keep dest!!!
										RetryFailedSourcesBatch_WriteLine(sRemark_WithSpaceIfNeeded+" "+(sCP)+" "+SafeString(fiDest.FullName,true,true)+" "+SafeString(fiSourceNow.FullName,true,true));
										sResultChar="[dest.len>]"; //was ~
									}
									else if (Common.GreaterThan(fiDest.LastWriteTimeUtc.Ticks,fiSourceNow.LastWriteTimeUtc.Ticks,FileTimeFudgeAllowedIn100NanosecondUtcTicks)) {
										RetryFailedSourcesBatch_WriteLine(sRemark_WithSpaceIfNeeded+" NEWER DESTINATION "+fiDest.LastWriteTimeUtc.ToString()+" newer than source (by "+Common.AbsoluteDifference(fiDest.LastWriteTimeUtc.Ticks,fiSourceNow.LastWriteTimeUtc.Ticks)+"x100ns (10,000ths of a millisecond) ["+fiDest.LastWriteTimeUtc.Ticks.ToString()+"-"+fiSourceNow.LastWriteTimeUtc.Ticks.ToString()+"=="+(fiDest.LastWriteTimeUtc.Ticks-fiSourceNow.LastWriteTimeUtc.Ticks).ToString()+"]) "+fiSourceNow.LastWriteTimeUtc.ToString()+" (or not modified on source since first copied):");
										//NOTE: intentionally ignore bMove in order to keep dest!!!
										RetryFailedSourcesBatch_WriteLine(sRemark_WithSpaceIfNeeded+" "+(sCP)+" "+SafeString(fiDest.FullName,true,true)+" "+SafeString(fiSourceNow.FullName,true,true));
										sResultChar="[dest.mod>]"; //was !
									}
									else if (fiDest.Length!=fiSourceNow.Length) {
										sResultChar="[dest.len!=src.len (this should never happen)]";//was ' //should never happen
										bSourceIsNewer_OrHasSameDateAndDestIsIncomplete=true;
									}
									else sResultChar="[unidentified situation (this should never happen)]"; //was % //should never happen
								}
								else sResultChar="[add]";//was +
							}
							if (bDebug) {
								if (bLineNotEnded_Console_Out) Console.WriteLine();
								Console.Error.WriteLine("Destination"+((sResultChar!=sSrcMod)?sResultChar:"")+": "+DestFile_FullName);
								bLineNotEnded_Console_Out=false;
							}
							else {
								Console.Write(sResultChar);
								bLineNotEnded_Console_Out=true;
							}
							
							if (!bIgnore) {
								BytesProcessed+=fiSourceNow.Length;
								FilesProcessed++;
								if (!fiDest.Exists||bSourceIsNewer_OrHasSameDateAndDestIsIncomplete) {
									SafeCopyOrMoveIfNotTestOnly(fiSourceNow,fiDest,DestFile_FullName); //DOES increment bytes removable and copyable
									//if (!bTestOnly) {
									//	if (fiDest.Exists) {
									//		fiDest.Attributes=FileAttributes.Normal;
									//		Batch_WriteLine(Program.sChmod_plus_w+" \""+DestFile_FullName+"\"");
									//	}
									//	if (bMove) {
									//		//if (File.Exists(DestFile_FullName)) {
									//		//	(new FileInfo(DestFile_FullName)).Attributes= FileAttributes.Normal;
									//		//	File.Delete(DestFile_FullName);
									//		//}
									//		//NO NEED to delete since was done above if needed (see fiDest usage above)
									//		bool bChangeAttrib=false;
									//		FileAttributes faPrev=fiSourceNow.Attributes;
									//		if ( (fiSourceNow.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ) {
									//			try {
									//				//faPrev=fiNow.Attributes;
									//				fiSourceNow.Attributes=FileAttributes.Normal;
									//				bChangeAttrib=true;
									//			}
									//			catch {}
									//		}
									//		(new FileInfo(DestFile_FullName)).Attributes=FileAttributes.Normal;
									//		File.Delete(DestFile_FullName);
									//		fiSourceNow.MoveTo(DestFile_FullName);
									//		if (bChangeAttrib) {
									//			try {
									//				(new FileInfo(DestFile_FullName)).Attributes=faPrev;
									//			}
									//			catch {}
									//		}
									//	}
									//	else fiSourceNow.CopyTo(DestFile_FullName,true);
									//}
									//Batch_WriteLine((bMove?sMV:sCP)+" \""+fiSourceNow.FullName+"\" \""+DestFile_FullName+"\"");
								}
							}
							else {
								BytesIgnorable+=fiSourceNow.Length;
								FilesIgnorable++;
							}
						}
						catch (Exception exn) {
							RetryFailedSourcesBatch_WriteLine((bMove?sMV:sCP)+" \""+fiSourceNow.FullName+"\" \""+DestFile_FullName+"\"");
							ShowExn(exn,"processing file \""+fiSourceNow.FullName+"\"");
						}
					}//end foreach fiNow in diSourceParent
					if (bMove&&!bTestOnly) {
						if (diSourceParent.GetFiles().Length==0
						    &&diSourceParent.GetDirectories().Length==0) {
							string Deletable_FullName=null;
							
							try {
								if (sDirSep=="\\") {
									if (diSourceParent.Name.StartsWith(".")) {
										Deletable_FullName=diSourceParent.Parent.FullName+sDirSep+sDeletableMarker+diSourceParent.Name;
										Batch_WriteLine(sMV+" "+SafeString(diSourceParent.FullName,true,true)+" "+SafeString(Deletable_FullName,true,true));
										diSourceParent.MoveTo(Deletable_FullName);
									}
								}
								if (Deletable_FullName!=null) {
									Batch_WriteLine(sRmdir+" "+SafeString(Deletable_FullName,true,true));
									(new DirectoryInfo(Deletable_FullName)).Attributes=FileAttributes.Normal;
									(new DirectoryInfo(Deletable_FullName)).Delete(false);
								}
								else {
									Batch_WriteLine(sRmdir+" "+SafeString(diSourceParent.FullName,true,true));
									diSourceParent.Attributes=FileAttributes.Normal;
									diSourceParent.Delete(false);
								}
								
							}
							catch (Exception exn) {
								if (Deletable_FullName!=null) RetryFailedSourcesBatch_WriteLine(sRmdir+" \""+Deletable_FullName+"\"");
								else RetryFailedSourcesBatch_WriteLine(sRmdir+" \""+diSourceParent.FullName+"\"");
							}
						}
						//else {
							//NOTE: this is done in DeleteEmptyFoldersInTree INSTEAD OF BELOW
							//Batch_WriteLine(sRemark_WithSpaceIfNeeded+"not empty:"+SafeString(diSourceParent.FullName,true,true));
						//}
					}
				}//end if diSourceParent.Exists
				else {
					ShowErr("ERROR: Cannot backup folder "+ToCSharpConstant(diSourceParent)+" because it does not exist.");
				}
			}
			catch (Exception exn) {
				if (bIn) iFolderDepth--;
				RetryFailedSourcesBatch_WriteLine(sMD+" \""+DestParentFolder_FullNameSlash+"\""); //only add line to create folder, to distinguish from subfolder reading error above
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
		public static string SourceParentEquivalentToDestParentFullNameSlash() {
			string sReturn=sSourceRoot;
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
