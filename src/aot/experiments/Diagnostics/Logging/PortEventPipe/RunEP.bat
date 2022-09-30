cls
call taskkill /IM dotnet.exe /F
git clean -xfd

call build.cmd -arch x64 -os windows -s clr.aot+clr.nativeaotlibs -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_100.log

REM call build.cmd -arch x64 -os windows -s clr.aot -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_49.log

REM call build.cmd -arch x64 -os windows -s clr.aot -c Release -v:diag > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_38.log


REM call build.cmd -arch x64 -os windows -s clr -c Release -v:diag > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_50.log