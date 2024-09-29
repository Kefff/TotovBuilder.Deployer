:: Generates the report
dotnet "%UserProfile%\.nuget\packages\reportgenerator\5.3.10\tools\net8.0\ReportGenerator.dll" -reports:"./TestResults/*/*.cobertura.xml" -targetdir:"./TestResults/html" -reporttypes:HTML

:: Opens the report
"./TestResults/html/index.html"