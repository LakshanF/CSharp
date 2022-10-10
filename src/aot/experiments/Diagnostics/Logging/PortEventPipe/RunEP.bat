cls
call taskkill /IM dotnet.exe /F
git clean -xfd

rem call build.cmd -arch x64 -os windows -s clr.aot+libs -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_201.log

REM call build.cmd -arch x64 -os windows -s clr.aot+libs+libs.tests -c Release /p:TestNativeAot=true /p:RunSmokeTestsOnly=true  > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_503.log

REM call build.cmd -arch x64 -os windows -s clr.aot+clr.nativeaotlibs -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_100.log

call build.cmd -arch x64 -os windows -s clr.aot -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_576.log

REM call build.cmd -arch x64 -os windows -s clr.aot -c Release -v:diag > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_38.log


REM call build.cmd -arch x64 -os windows -s clr -c Release -v:diag > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_50.log