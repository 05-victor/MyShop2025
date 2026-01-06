; MyShop2025 Installer Script for Inno Setup
; Download Inno Setup from: https://jrsoftware.org/isdl.php

#define MyAppName "MyShop 2025"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "MyShop Team"
#define MyAppURL "https://github.com/05-victor/MyShop2025"
#define MyAppExeName "MyShop-Launcher.bat"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=LICENSE
OutputDir=Output
OutputBaseFilename=MyShop2025-Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
; Require Windows 10 or later
MinVersion=10.0.10240
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Request admin privileges for installation
PrivilegesRequired=admin
; Uninstaller
UninstallDisplayIcon={app}\frontend\MyShop.Client.exe
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Frontend (WinUI 3)
Source: "publish-package\frontend\*"; DestDir: "{app}\frontend"; Flags: ignoreversion recursesubdirs createallsubdirs

; Backend (ASP.NET Core)
Source: "publish-package\backend\*"; DestDir: "{app}\backend"; Flags: ignoreversion recursesubdirs createallsubdirs

; Python ML API
Source: "publish-package\python-ml\*"; DestDir: "{app}\python-ml"; Flags: ignoreversion recursesubdirs createallsubdirs

; Python Embeddable Runtime
Source: "publish-package\python-embed\*"; DestDir: "{app}\python-embed"; Flags: ignoreversion recursesubdirs createallsubdirs

; Launcher
Source: "publish-package\MyShop-Launcher.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish-package\MyShop-Diagnostics.bat"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

; Documentation
Source: "WPExtension\README.md"; DestDir: "{app}\docs"; DestName: "API-README.md"; Flags: ignoreversion skipifsourcedoesntexist
Source: "INSTALLER-GUIDE.md"; DestDir: "{app}\docs"; Flags: ignoreversion skipifsourcedoesntexist
Source: "QUICK-START.md"; DestDir: "{app}\docs"; Flags: ignoreversion skipifsourcedoesntexist

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
var
  InstallationProgressPage: TOutputProgressWizardPage;

procedure InitializeWizard;
begin
  InstallationProgressPage := CreateOutputProgressPage('Installing MyShop 2025', 'Please wait while Setup installs MyShop 2025 on your computer.');
end;

function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
  DotNetInstalled: Boolean;
begin
  Result := True;
  
  // Show system requirements
  if MsgBox('MyShop 2025 Installation' + #13#10 + #13#10 + 
            'This installation includes:' + #13#10 + 
            '- Python ML API (first run: 2-5 minutes to install dependencies)' + #13#10 + 
            '- ASP.NET Core Backend' + #13#10 + 
            '- WinUI 3 Frontend (requires Windows 10 version 1809 or later)' + #13#10 + #13#10 + 
            'Note: Windows App SDK Runtime will be installed if not present.' + #13#10 + #13#10 + 
            'Continue with installation?', mbConfirmation, MB_YESNO) = IDNO then
  begin
    Result := False;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    InstallationProgressPage.SetText('Extracting files...', '');
    InstallationProgressPage.SetProgress(0, 100);
    InstallationProgressPage.Show;
  end
  else if CurStep = ssPostInstall then
  begin
    InstallationProgressPage.SetText('Finalizing installation...', '');
    InstallationProgressPage.SetProgress(100, 100);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
begin
  if CurUninstallStep = usUninstall then
  begin
    // Kill any running processes
    Exec('taskkill', '/F /IM python.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Exec('taskkill', '/F /IM MyShop.Server.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Exec('taskkill', '/F /IM MyShop.Client.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;
end;

[UninstallDelete]
Type: filesandordirs; Name: "{app}\python-embed\Lib"
Type: filesandordirs; Name: "{app}\python-embed\Scripts"
