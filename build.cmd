@SETLOCAL

@CALL %~dp0buildenv.cmd

@ECHO Starting building...

@REM Build the code.
@ECHO Finding .sln files to build
@for /r . %%i in (*.sln) do @(
    @ECHO.
    @ECHO.
    @ECHO ************************************************************
    @ECHO Found and building %%i
    @ECHO ************************************************************
    @ECHO.
    @REM publish will build and generate an exe
    @CALL dotnet publish -c Release -r win10-x64 "%%i" || @(
        @ECHO Failed to build "%%i" correctly.
        @EXIT /B 1
    )
)

@ECHO Completed building
@EXIT /B 0