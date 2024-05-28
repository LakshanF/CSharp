// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//On unix make sure to compile using -ldl and -pthread flags.

//Set this value accordingly to your workspace settings
#if defined(_WIN32)
#define PathToLibrary1 "..\\NativeLibrary1\\bin\\Debug\\net8.0\\win-x64\\publish\\NativeLibrary1.dll"
#define PathToLibrary2 "..\\NativeLibrary2\\bin\\Debug\\net8.0\\win-x64\\publish\\NativeLibrary2.dll"
#elif defined(__APPLE__)
#define PathToLibrary "./bin/Debug/net6.0/osx-x64/native/NativeLibrary.dylib"
#else
#define PathToLibrary "./bin/Debug/net6.0/linux-x64/native/NativeLibrary.so"
#endif

#ifdef _WIN32
#include "windows.h"
#define symLoad GetProcAddress
#else
#include "dlfcn.h"
#include <unistd.h>
#define symLoad dlsym
#endif

#include <stdlib.h>
#include <stdio.h>

#ifndef F_OK
#define F_OK    0
#endif

int callSumFunc1(char *path, char *funcName, int a, int b);
int callSumFunc2(char *path, char *funcName, int a, int b);

int main()
{
    printf("Starting...");
    // Check if the library file exists
    if (access(PathToLibrary1, F_OK) == -1)
    {
        puts("Couldn't find library1 at the specified path");
        return 0;
    }
    if (access(PathToLibrary2, F_OK) == -1)
    {
        puts("Couldn't find library2 at the specified path");
        return 0;
    }

    // Sum two integers from library2
    printf("Calling function in library2\n");
    int sum2 = callSumFunc2(PathToLibrary2, "addInLib2", 12, 80);
    printf("The sum is %d \n", sum2);

    // Sum two integers from library1
    printf("Calling function in library1\n");
    int sum1 = callSumFunc1(PathToLibrary1, "add", 2, 8);
    printf("The sum is %d \n", sum1);

    printf("Done!");
}

int callSumFunc1(char *path, char *funcName, int firstInt, int secondInt)
{
    // printf("DEBUG-1\n");
    // Call sum function defined in C# shared library
    #ifdef _WIN32
        HINSTANCE handle = LoadLibraryA(path);
    #else
        void *handle = dlopen(path, RTLD_LAZY);
    #endif

    // printf("DEBUG-2\n");
    typedef int(*myFunc)(int,int);
    myFunc MyImport = (myFunc)symLoad(handle, funcName);
    // printf("DEBUG-3\n");

    int result = MyImport(firstInt, secondInt);
    // printf("DEBUG-4\n");

    // CoreRT libraries do not support unloading
    // See https://github.com/dotnet/corert/issues/7887
    return result;
}

int callSumFunc2(char *path, char *funcName, int firstInt, int secondInt)
{
    // Call sum function defined in C# shared library
    #ifdef _WIN32
        HINSTANCE handle = LoadLibraryA(path);
    #else
        void *handle = dlopen(path, RTLD_LAZY);
    #endif

    typedef int(*myFunc)(int,int);
    myFunc MyImport = (myFunc)symLoad(handle, funcName);

    int result = MyImport(firstInt, secondInt);

    // CoreRT libraries do not support unloading
    // See https://github.com/dotnet/corert/issues/7887
    return result;
}
