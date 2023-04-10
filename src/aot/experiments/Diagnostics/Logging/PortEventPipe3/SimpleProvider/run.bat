cls

del c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\windows.x64.Debug\tracing\eventpipe\simpleprovidervalidation\simpleprovidervalidation\native\simpleprovidervalidation.exe

dir c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\obj\windows.x64.Debug\Managed\tracing\eventpipe\simpleprovidervalidation\simpleprovidervalidation\simpleprovidervalidation.dll
c:\work\core\CurrentWork4\runtime\artifacts\bin\coreclr\windows.x64.Debug\ilc-published\ilc @"c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\obj\windows.x64.Debug\Managed\tracing\eventpipe\simpleprovidervalidation\simpleprovidervalidation\native\simpleprovidervalidation.ilc.rsp"
echo %ERRORLEVEL%

copy /Y C:\work\core\LakshanF\CSharp\src\temp\SimpleProvider\link.rsp c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\obj\windows.x64.Debug\Managed\tracing\eventpipe\simpleprovidervalidation\simpleprovidervalidation\native
copy /Y C:\work\core\LakshanF\CSharp\src\temp\SimpleProvider\simpleprovidervalidation.res c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\obj\windows.x64.Debug\Managed\tracing\eventpipe\simpleprovidervalidation\simpleprovidervalidation\native


"C:\Program Files\Microsoft Visual Studio\2022\Preview\VC\Tools\MSVC\14.35.32124\bin\Hostx64\x64\link.exe" @"c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\obj\windows.x64.Debug\Managed\tracing\eventpipe\simpleprovidervalidation\simpleprovidervalidation\native\link.rsp"
echo %ERRORLEVEL%

dir c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\windows.x64.Debug\tracing\eventpipe\simpleprovidervalidation\simpleprovidervalidation\native\simpleprovidervalidation.exe

c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\windows.x64.Debug\tracing\eventpipe\simpleprovidervalidation\simpleprovidervalidation\native\simpleprovidervalidation.exe

pause

del c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\windows.x64.Debug\tracing\eventpipe\simple2\simple2\native\simple2.exe

call "c:\work\core\CurrentWork4\runtime\artifacts\bin\coreclr\windows.x64.Debug\ilc-published\ilc" @"c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\obj\windows.x64.Debug\Managed\tracing\eventpipe\simple2\simple2\native\simple2.ilc.rsp"
echo %ERRORLEVEL%

call "C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Tools\MSVC\14.35.32215\bin\Hostx64\x64\link.exe" @"c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\obj\windows.x64.Debug\Managed\tracing\eventpipe\simple2\simple2\native\link.rsp"
echo %ERRORLEVEL%

dir c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\windows.x64.Debug\tracing\eventpipe\simple2\simple2\native

REM dotnet-trace collect --providers LaksDemoEventSource,Microsoft-Windows-DotNETRuntime --name simple2 

pause

del c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\windows.x64.Debug\tracing\eventpipe\simple3\simple3\native\simple3.exe

call c:\work\core\CurrentWork4\runtime\artifacts\bin\coreclr\windows.x64.Debug\ilc-published\\ilc @"c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\obj\windows.x64.Debug\Managed\tracing\eventpipe\simple3\simple3\native\simple3.ilc.rsp"
echo %ERRORLEVEL%

call "C:\Program Files\Microsoft Visual Studio\2022\Professional\VC\Tools\MSVC\14.35.32215\bin\Hostx64\x64\link.exe" @"c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\obj\windows.x64.Debug\Managed\tracing\eventpipe\simple3\simple3\native\link.rsp"
echo %ERRORLEVEL%

dir c:\work\core\CurrentWork4\runtime\artifacts\tests\coreclr\windows.x64.Debug\tracing\eventpipe\simple3\simple3\native
