# ForwardFileSync
A console sync program with easy filtering syntax (filename of YAML-like script is only parameter).

## License
* Authors: Jacob Gustafson ( http://github.com/expertmm http://www.expertmultimedia.com )
* Only use and copy in accordance with the LICENSE file.

## Usage
* run program and follow instructions

## Troubleshooting
* If the program fails to load, client needs to download the runtime (such as .NET framework or Mono)

## Changes
* (2011-02-24) script language changes:
	* parameterless commands are now possible to implement (see below; commands no longer need a colon to run)
	* now always displays what follows "show:" as a message (if quoted, removes quotes and changes 	escaped quotes to quotes.)
	* new variable "showdelay" (param is #of seconds, else "infinity") for "show" commands that follow it
	* "commmand" command eliminated
		command:synchronize to synchronize
		command:deletelogs to deletelogs
		command:showtests to showtests
		"command:attribs " to "attribs "
	* other changes:
		show:options to showoptions
		waitseconds to wait
	* synchronize command must be present for synchronize to run (there is no longer an automatic synchronize)
* (2009-12-11) move mode is specified by global and is noted in output
* (2009-12-11) refactoring (changed variables names, changed from arrays to ArrayLists)
* (2009-12-11) changed default exclusions
* (2009-09-01) writes retry batch
* (2009-09-01) writes log
* (2009-09-01) added move option

## Known Issues
* Change to Rsync syntax (see sh file on root of TRANSCEND MicroSDHC card)
* fix issue where writes lots of commands for destination to ResolvePermissions though there are no file copy errors or file copy retry commands
* make sure that, ONLY in windows (Windows if sDirSep[0]='\'), exclusions and filters are case-insensitive (change Common.is_exclusion_list_case_sensitive)
* make sure that CreateBatch:no then DoGeneratedActions is NOT created
* make sure to only go into folder for sync IF date of SOURCE is NEWER
	* same for DeleteFilesIfNotOnSource!
* Command:movefolder command:
	* remove quotes from params if present, and reprocess params if missing end quote and closed on a later param (insert spaces between params and add to new array)
	* delete folder/file if can't move AND same as dest
* Show cancellation message (e.g. fix problem where exits silently if spacebar is pressed after continue (y/n) is asked)
* Recurse into subfolder if DIFFERS in date, not just if less than source (in case both were changed)!
* Change newer dest symbol to '>' (output "Destination>:")
* Change "1.ResolvePermissionIssues--RunMe-then-delete-to-fix.bat" to "ForwardFileSync-ResolvePermissionIssues-"+CurrentlyRunningScript_Name_NoExt+"."+DatePathString_NoTime()+".bat"
* do NOT check SourceForCompare for deletion if excluded!
* CHECK if DeleteIfNotOnSource:yes when filename contains NODELETE
	* AND if opposite is true
* document sResultChar in output
* pause if missing dest OR source
	* AND exit

## Planned features
* remove all TXT file output
	* stderr: output retry shell commands
	* stdout: output successful shell commands (or commands attempted?)
		* eliminate other output (optionally REM file copying status?)
		* possibly comment out the ones that worked
			* possibly make it readable:
				TODO: see if error is caused by filename too long: L:\Music\Public Domain\Chosvex\warcraftmovies.com\Choasvex - warcraftmovies.com movieview.php id 38604 - Leeroy's Unfinished Business (adjusted mp3 version by Jake).mp3
				REM overwrite...OK:
				REM copy c:\filename1.ext x:\filename1.ext
				REM overwrite...OK:
				REM copy c:\filename2.ext x:\filename2.ext
				REM add...OK:
				REM copy c:\filename3.ext x:\filename3.ext
				REM add...FAIL:
				takeown /f "L:\Music\Public Domain\Chosvex" /r /d y
				icacls "L:\Music\Public Domain\Chosvex" /grant administrators:F /t
				copy E:\Music\Public Domain\Chosvex\warcraftmovies.com\Choasvex - warcraftmovies.com movieview.php id 38604 - Leeroy's Unfinished Business (adjusted mp3 version by Jake).mp3 L:\Music\Public Domain\Chosvex\warcraftmovies.com\Choasvex - warcraftmovies.com movieview.php id 38604 - Leeroy's Unfinished Business (adjusted mp3 version by Jake).mp3
				REM delnotonsource...FAIL:
				attrib -s +w x:\filename5.ext
				del x:\filename5.ext
	* sections needed to be redirected:
		ForwardFileSync Copy Errors.txt
			* verify all non-command outputs have COMMENT before them as:
			Console.Error.WriteLine(sComment+sMsg);
			Console.Error.WriteLine(sRetryBatchCommandNow);
		ForwardFileSync-Retry-part1-ResolvePermissionIssues--RunMe-then-delete-to-fix.bat
			* usually has a bunch of junk no matter what
		ForwardFileSync-Retry-part2-AttributeCommands.ffs
		ForwardFileSync-Retry-part3-FileCommands.bat
* Differential backup
	* by comparing to drive
	* by comparing to log
* option to delete files on destination if in the excluded list (BUT NEVER DELETE SOURCE!)

### Merge with Backup GoNow
* Main difference between "Command:Synchronize" and Backup GoNow's sync commands (AddFolder, AddFile)
is the way that the DESTINATION is determined
* implement AddFile
	* Make sure AddFile IGNORES all filters (folder AND file filters!)
* Place all backup-related features into Common.cs
* Place RunScriptLine and runtime variables into ForwardFileSync.cs
	* do this for Backup GoNow as well, with file named the same
	* then merge the ForwardFileSync.cs of ForwardFileSync with that of Backup GoNow
