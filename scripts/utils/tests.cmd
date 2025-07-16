:: Deleting old test results
rmdir /s /q "./TestResults"

:: Testing and generating test results
dotnet test "./src/TotovBuilder.Deployer.sln" --collect:"Code Coverage" --settings "./.runsettings"