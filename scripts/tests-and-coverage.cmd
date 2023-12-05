cd ../src
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
"%UserProfile%\.nuget\packages\reportgenerator\4.8.13\tools\net5.0\ReportGenerator.exe" -reports:"TestResults\*\coverage.cobertura.xml" -targetdir:"TestResults\html" -reporttypes:HTML
"TestResults\html\index.html"
pause
rmdir /s /q "TestResults"