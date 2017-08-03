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
		
		public static bool b_TEST_ONLY=false;
		
		public static string[] sarrFolderStack=new string[1024];
		public static int iFolderDepth=0;
		public static string sSourceRoot=@"K:\WINDOWS";//no slash
		public static string sDestRoot=@"I:\old computer 2009-08\WINDOWS";//no slash
		public static string[] sarrExcludeFolder_NameI=new string[] {"system","command","system32","ime","fonts","help","catroot","cache","inf","downloaded program files","cursors","msagent","samples","drwatson","pif","sysbckup","web","tasks","sun","Start Menu","spool","shellnew","sendto","recent","printhood","pif","offline web pages","nethood","msdownld.tmp","msapps","msagent","media","java","inf","ime","history","help","fonts","drwatson","downloaded program files","cwcdata","cursors","cookies","command","catroot","apppatch","applog","temp","temporary internet files"};//must be lower
		public static string[] sarrExcludeFolder_FullNameI=new string[] {@"k:\windows\options",@"k:\windows\history",@"k:\windows\installer",@"k:\windows\twain32",@"k:\windows\config",@"k:\windows\cache"};//must be lower
		public static string[] sarrExcludeFileStartsWithI=new string[] {""};//must be lower
		public static string[] sarrExcludeFileEndsWithI=new string[] {".swp",".sys",".exe",".dll",".com",".scr","net.msg","neth.msg",".tmp"};//must be lower
		
		public static DirectoryInfo diSourceRoot=null;
		public static DirectoryInfo diDestRoot=null;
		public static StreamWriter streamErr=null;
		public static void Main(string[] args) {
			string sDesktop_FullName=System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			streamErr=new StreamWriter(sDesktop_FullName
			                           +char.ToString(Path.DirectorySeparatorChar)
			                           +@"ForwardFileSync Copy Errors.txt"); //streamErr=new StreamWriter(@"C:\Documents and Settings\Owner\Desktop\ForwardFileSync Copy Errors.txt");
			diSourceRoot=new DirectoryInfo(sSourceRoot);
			if (!Directory.Exists(sDestRoot)) Directory.CreateDirectory(sDestRoot);
			diDestRoot=new DirectoryInfo(sDestRoot);
			Console.Error.WriteLine("Starting...");
			//DirectoryInfo diSourceRoot=new DirectoryInfo(@"\\TOSHIBA1\My Documents");
			//foreach (FileInfo fiNow in diSourceRoot.GetFiles()) {
			//	Console.Error.WriteLine(fiNow.Name);
			//}
			//Console.Error.WriteLine();
			//Console.Error.WriteLine("Folders:");
			//foreach (DirectoryInfo diNow in diSourceRoot.GetDirectories()) {
			//	Console.Error.WriteLine(diNow.Name);
			//}
			BackupTree(diSourceRoot);
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
			streamErr.Close();
		}
		public static int ArrayHas(string[] array, string has) {
			if (array!=null) {
				for (int i=0; i<array.Length; i++) {
					if (array[i]!=null&&array[i]!=""&&array[i]==has) return i;
				}
			}
			return -1;
		}
		public static int ArrayHasI(string[] array, string has) {
			has=has.ToLower();
			if (array!=null) {
				for (int i=0; i<array.Length; i++) {
					if (array[i]!=null&&array[i]!=""&&array[i].ToLower()==has) return i;
				}
			}
			return -1;
		}
		//public static bool ArrayHasI(string[] array, string has) {
		//	if (array!=null&&has!=null&&has!="") {
		//		for (int i=0; i<array.Length; i++) {
		//			if (array[i].ToLower()==has.ToLower()) return true;
		//		}
		//	}
		//	return false;
		//}
		public static int StartsWithAny(string s, string[] StartsWithAnyOfTheseStrings) {
			if (StartsWithAnyOfTheseStrings!=null) {
				for (int i=0; i<StartsWithAnyOfTheseStrings.Length; i++) {
					if (StartsWithAnyOfTheseStrings[i]!=null && StartsWithAnyOfTheseStrings[i]!="" && s.StartsWith(StartsWithAnyOfTheseStrings[i])) return i;
				}
			}
			return -1;
		}
		public static int StartsWithAnyI(string s, string[] StartsWithAnyOfTheseStrings) {
			s=s.ToLower();
			if (StartsWithAnyOfTheseStrings!=null) {
				for (int i=0; i<StartsWithAnyOfTheseStrings.Length; i++) {
					if (StartsWithAnyOfTheseStrings[i]!=null && StartsWithAnyOfTheseStrings[i]!="" && s.StartsWith(StartsWithAnyOfTheseStrings[i].ToLower())) return i;
				}
			}
			return -1;
		}
		public static int EndsWithAny(string s, string[] EndsWithAnyOfTheseStrings) {
			if (EndsWithAnyOfTheseStrings!=null) {
				for (int i=0; i<EndsWithAnyOfTheseStrings.Length; i++) {
					if (EndsWithAnyOfTheseStrings[i]!=null && EndsWithAnyOfTheseStrings[i]!="" && s.EndsWith(EndsWithAnyOfTheseStrings[i])) return i;
				}
			}
			return -1;
		}
		public static int EndsWithAnyI(string s, string[] EndsWithAnyOfTheseStrings) {
			s=s.ToLower();
			if (EndsWithAnyOfTheseStrings!=null) {
				for (int i=0; i<EndsWithAnyOfTheseStrings.Length; i++) {
					if (EndsWithAnyOfTheseStrings[i]!=null && EndsWithAnyOfTheseStrings[i]!="" && s.EndsWith(EndsWithAnyOfTheseStrings[i].ToLower())) return i;
				}
			}
			return -1;
		}
		public static void BackupTree(DirectoryInfo diParent) {
			foreach (DirectoryInfo diNow in diParent.GetDirectories()) {
				try {
					
					bool bExists=Directory.Exists(diNow.FullName);//;Directory.Exists(DestFolder(diNow.Name));
					
					bool bIgnore=false;
					string sIgnoreFolderReason="";
					if (bExists) {
						int iIgnoreWhichFolder_NameIndex=ArrayHasI(sarrExcludeFolder_NameI,diNow.Name.ToLower());
						if (iIgnoreWhichFolder_NameIndex>=0) {
							bIgnore=true;
							sIgnoreFolderReason="Name is "+sarrExcludeFolder_NameI[iIgnoreWhichFolder_NameIndex];
						}
						int iIgnoreWhichFolder_FullNameIndex=ArrayHasI(sarrExcludeFolder_FullNameI,diNow.FullName.ToLower());
						if (iIgnoreWhichFolder_FullNameIndex>=0) {
							bIgnore=true;
							sIgnoreFolderReason+= ((sIgnoreFolderReason!="")?"; ":"")+"FullName is "+sarrExcludeFolder_FullNameI[iIgnoreWhichFolder_FullNameIndex];
						}
						if (!bIgnore) {
							if (!Directory.Exists(DestFolder(diNow.Name))) Directory.CreateDirectory(DestFolder(diNow.Name));
							Console.Error.WriteLine("Folder"+(bExists?"":"*")+": "+DestFolder(diNow.Name));
							if (!bExists) Directory.CreateDirectory(DestFolder(diNow.Name));
							sarrFolderStack[iFolderDepth]=diNow.Name;
							iFolderDepth++;
							BackupTree(diNow);
							iFolderDepth--;
						}
						else {
							Console.Error.WriteLine("Folder excluded ("+sIgnoreFolderReason+"): "+DestFolder(diNow.Name));
						}
					}
					else {
						Console.Error.WriteLine("Folder could not be read: "+DestFolder(diNow.Name));
					}
				}
				catch (Exception exn) {
					streamErr.WriteLine(exn.ToString());
					Console.Error.WriteLine(exn.ToString());
				}
			}//end foreach subdirectory in directory
			string sLastMD="";
			foreach (FileInfo fiNow in diParent.GetFiles()) {
				try {
					bool bExists=File.Exists(NameToDestFullName(fiNow.Name));
					bool bIgnore=false;
					string sIgnoreFileReason="";
					string sNameLower=fiNow.Name.ToLower();
					int iIgnoreWhichFile_NameEndsWithIndex=EndsWithAnyI(sNameLower,sarrExcludeFileEndsWithI);
					if (iIgnoreWhichFile_NameEndsWithIndex>=0) {
						bIgnore=true;
						sIgnoreFileReason="EndsWith \""+sarrExcludeFileEndsWithI[iIgnoreWhichFile_NameEndsWithIndex]+"\" (I)";
					}
					int iIgnoreWhichFile_NameStartsWithIndex=StartsWithAnyI(sNameLower,sarrExcludeFileStartsWithI);
				    if (iIgnoreWhichFile_NameStartsWithIndex>=0) {
				    	bIgnore=true;
				    	sIgnoreFileReason=((sIgnoreFileReason!="")?"; ":"")+"StartsWith \""+sarrExcludeFileEndsWithI[iIgnoreWhichFile_NameStartsWithIndex]+"\" (I)";
				    }
					
					Console.Error.WriteLine((bIgnore?("(excluded: "+sIgnoreFileReason+")"):"")+"File"+(bExists?"":"*")+": "+NameToDestFullName(fiNow.Name));
					if (!bIgnore) {
						if (!bExists) {
							if (!b_TEST_ONLY) fiNow.CopyTo(NameToDestFullName(fiNow.Name));
							else {
								if (!Directory.Exists(DestNamePrecedent()) 
								    &&(sLastMD!=DestNamePrecedent())) {
									Console.WriteLine("md \""+DestNamePrecedent()+"\"");
									sLastMD=DestNamePrecedent();
								}
								Console.WriteLine("copy /y \""+fiNow.FullName+"\" \""+NameToDestFullName(fiNow.Name)+"\"");
							}
						}
					}
					
				}
				catch (Exception exn) {
					streamErr.WriteLine(exn.ToString());
					Console.Error.WriteLine(exn.ToString());
				}
			}//end foreach file in directory
		}//BackupTree
		public static string DestNamePrecedent() {
			string sReturn=sDestRoot;
			for (int iNow=0; iNow<iFolderDepth; iNow++) {
				sReturn+=@"\"+sarrFolderStack[iNow];
			}
			return sReturn;
		}
		public static string NameToDestFullName(string sFileNameOnly) {
			return DestNamePrecedent()+@"\"+sFileNameOnly;
		}
		public static string DestFolder(string sFolderNameOnly) {
			return DestNamePrecedent()+@"\"+sFolderNameOnly;
		}
	}//end Program class
}//end namespace
