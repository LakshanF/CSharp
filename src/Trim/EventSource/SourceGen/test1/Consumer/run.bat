cls
rd /s/q ..\MyEventSource\bin
rd /s/q ..\MyEventSource\obj
rd /s/q ..\UserType\bin
rd /s/q ..\UserType\obj
rd /s/q bin
rd /s/q obj
dotnet publish -r win-x64 --self-contained
pushd D:\work\Core\LakshanF\CSharp\src\Trim\EventSource\SourceGen\test1\Consumer\bin\Debug\net7.0\win-x64\publish\
Consumer.exe
popd

