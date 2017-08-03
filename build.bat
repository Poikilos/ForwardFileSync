REM SET PATH=%PATH%;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727

move bin/ForwardFileSync.exe bin/ForwardFileSync.exe.wip
REM NOTE: if above was in quotes, mac put a quote in the filename and then had 'cannot contain "'  error when I try to rename!
mcs /out:bin/ForwardFileSync.exe AssemblyInfo.cs Common.cs LocInfo.cs MyCallBack_Console.cs Program.cs /resource:MainForm.resx,MainForm /r:System.Data.dll /r:System.Xml.dll
