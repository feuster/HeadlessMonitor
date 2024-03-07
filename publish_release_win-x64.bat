@echo off
Powershell.exe -executionpolicy remotesigned -File build.ps1 --configuration=Release --framework=net8.0 --runtime=win-x64
