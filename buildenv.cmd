@REM Initialize the developer environment just like a developer box. Note that 'call' keyword that ensures that the script does not exist after 
@REM calling the other batch file.
@CALL "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat" -arch=amd64 -host_arch=amd64 -test 1>nul 2>&1 || @(
    @ECHO NOT IN BUILD ENVIRONMENT, Initializing Environment
    @CALL "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat" -arch=amd64 -host_arch=amd64
    @CALL "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsDevCmd.bat" -arch=amd64 -host_arch=amd64 -test 1>nul 2>&1 || @(
        @ECHO "Failed to Start Dev Environment."
        @EXIT /B 1
    )
)

@EXIT /B 0