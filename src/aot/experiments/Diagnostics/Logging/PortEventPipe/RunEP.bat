cls
call taskkill /IM dotnet.exe /F
REM git clean -xfd

rem call build.cmd -arch x64 -os windows -s clr.aot+libs -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_201.log

REM call build.cmd -arch x64 -os windows -s clr.aot+libs+libs.tests -c Release /p:TestNativeAot=true /p:RunSmokeTestsOnly=true  > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_503.log

REM call build.cmd -arch x64 -os windows -s clr.aot+clr.nativeaotlibs -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_100.log

REM call build.cmd -arch x64 -os windows -s clr.aot -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_656.log

REM call build.cmd -arch x64 -os windows -s clr.aot -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_901.log

@REM call build.cmd -arch x64 -os windows -s clr.aot -c Debug > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build3\build_1.log
@REM call build.cmd -arch x64 -os windows -s clr.aot+libs -c Debug > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build3\build_2.log
REM call build.cmd -arch x64 -os windows -s Clr.Runtime -c Debug > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\build_1.log
REM call build.cmd -arch x64 -os windows -s Clr+libs -c Debug > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build3\build_4.log

@REM call build.cmd -subset clr+libs -configuration Release -runtimeConfiguration Debug > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build3\build_10.log
@REM call src\tests\build.cmd nativeaot Debug tree nativeaot > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build3\build_11.log
@REM call src\tests\run.cmd runnativeaottests Debug > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build3\build_12.log
@REM call src\tests\build.cmd nativeaot Debug x64 tree nativeaot /p:BuildNativeAotFrameworkObjects=true -bl > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build3\build_13.log


REM call build.cmd -arch x64 -os windows -s clr.aot+host.native+libs -rc Release -lc Release -hc Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build3\build_20.log
call src\tests\build.cmd nativeaot Release x64 tree nativeaot /p:BuildNativeAotFrameworkObjects=true > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build3\build_21.log
call build.cmd -ci -arch x64 -os windows -s libs.tests -c Release /p:TestAssemblies=false /p:RunNativeAotTestApps=true /bl:C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build3\aottests.binlog  > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build3\build_22.log
REM call build.cmd -ci -arch x64 -os windows -s libs.tests -c Release /p:TestAssemblies=false /p:RunNativeAotTestApps=true /bl:C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build3\aottests.binlog  > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build3\build_22.log



REM call build.cmd -arch x64 -os windows -s clr -c Release > C:\Work\Core\LakshanF\CSharp\src\aot\experiments\Diagnostics\Logging\PortEventPipe\build\buildLog_change_50.log