cd ../src/TotovBuilder.Configurator.Test
rmdir /s /q "TestResults"
dotnet test --collect:"XPlat Code Coverage"
"%UserProfile%\.nuget\packages\reportgenerator\4.8.13\tools\net5.0\ReportGenerator.exe" -reports:"TestResults\*\coverage.cobertura.xml" -targetdir:"TestResults\html" -reporttypes:HTML
"TestResults\html\index.html"
pause