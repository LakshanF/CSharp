cls
call taskkill /IM dotnet.exe /F
git clean -xfd

rem call build.cmd -arch x64 -os windows -s clr.aot+libs -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_201.log

REM call build.cmd -arch x64 -os windows -s clr.aot+libs+libs.tests -c Release /p:TestNativeAot=true /p:RunSmokeTestsOnly=true  > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_503.log

REM call build.cmd -arch x64 -os windows -s clr.aot+clr.nativeaotlibs -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_100.log

REM call build.cmd -arch x64 -os windows -s clr.aot -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_656.log

REM call build.cmd clr.aot+libs -rc Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe2\build\buildLog_change_80.log

REM call call build.cmd -arch x64 -os windows -s clr.aot -c Debug > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe2\build\buildLog_change_75.log

REM call build.cmd -arch x64 -os windows -s clr.aot -c Debug -cmakeargs "--log-level=TRACE" > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe2\build\buildLog_change_83.log
REM call build.cmd -arch x64 -os windows -s clr.aot -c Debug > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe2\build\buildLog_change_84.log

REM   --log-level=<ERROR|WARNING|NOTICE|STATUS|VERBOSE|DEBUG|TRACE>

REM call build.cmd -arch x64 -os windows -s clr.aot -c Release -v:diag > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_38.log

REM call build.cmd -arch x64 -os windows -s clr -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_50.log


REM call build.cmd -arch x64 -os windows -s clr.aot -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe2\build\buildLog_change_58.log

call build.cmd -arch x64 -os windows -s clr.aot -c Debug > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe2\build\buildLog_change_49.log

REM call build.cmd clr.aot+libs -rc Debug > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe2\build\buildLog_change_50.log