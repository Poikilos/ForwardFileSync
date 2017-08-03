set MYFOLDERNAME=ForwardFileSync
set MYEXENAME=ForwardFileSync.exe
set AllUsersProjects=C:\Documents and Settings\All Users\Documents\Projects
IF EXIST "C:\Users\Public\Documents" set AllUsersProjects=C:\Users\Public\Documents\Projects
IF EXIST "C:\Users\Public\Projects" set AllUsersProjects=C:\Users\Public\Projects
md "%AllUsersProjects%\%MYFOLDERNAME%"
md "%AllUsersProjects%\%MYFOLDERNAME%\bin"
REM NOTE: ON NEXT LINE ONLY DELETE COPY IF AN ORIGINAL EXISTS (otherwise user may lose access to program)!
IF EXIST "%AllUsersProjects%\%MYFOLDERNAME%\bin\%MYEXENAME%" del "%AllUsersProjects%\%MYFOLDERNAME%\bin\%MYFOLDERNAME% - Copy.exe"
IF EXIST "%AllUsersProjects%\%MYFOLDERNAME%\bin\%MYEXENAME%" Copy /y "%AllUsersProjects%\%MYFOLDERNAME%\bin\%MYEXENAME%" "%AllUsersProjects%\%MYFOLDERNAME%\bin\%MYFOLDERNAME% - Copy.exe"
REM IF EXIST "%AllUsersProjects%\%MYFOLDERNAME%\bin\%MYEXENAME%" del "%AllUsersProjects%\%MYFOLDERNAME%\bin\%MYEXENAME%"

copy /y .\bin\%MYEXENAME% "%AllUsersProjects%\%MYFOLDERNAME%\bin\"
copy /y \\Hslab-teacher\c$\Users\jgustafson\OneDrive\Projects-cs\%MYFOLDERNAME%\bin\%MYEXENAME% "%AllUsersProjects%\%MYFOLDERNAME%\bin\"

REM rename "%AllUsersProjects%\%MYFOLDERNAME%\deploy.bat" "deploy_bat"
REM rename "%AllUsersProjects%\%MYFOLDERNAME%\deploywithsource.bat" "deploywithsource_bat"
echo echo deploy from source instead > "%AllUsersProjects%\%MYFOLDERNAME%\deploywithsource.bat"
echo pause >> "%AllUsersProjects%\%MYFOLDERNAME%\deploy.bat"
echo echo deploy from source instead > "%AllUsersProjects%\%MYFOLDERNAME%\deploywithsource.bat"
echo pause >> "%AllUsersProjects%\%MYFOLDERNAME%\deploywithsource.bat"
REM KEEP PREVIOUS 4 LINES (or 6 for privacy) OTHERWISE USER MAY MESS UP THE ORIGINAL WHEN RUN FROM DEST (TRIES TO COPY OVER SELF AND MAY CAUSE DELETIONS)!
