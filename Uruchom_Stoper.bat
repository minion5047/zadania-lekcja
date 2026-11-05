@echo off
title Stoper Desktop
cd /d "%~dp0"
dotnet run --configuration Release
if %errorlevel% neq 0 (
    echo.
    echo Wystapil problem z uruchomieniem przez dotnet run.
    pause
)
