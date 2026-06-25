; Instalador del Station Agent PetrolRíos (Inno Setup 6)
; Compilar con: ISCC.exe instalador_agente.iss (o desde publicar.bat)

#define AppName "PetrolRios Station Agent"
#define AppVersion "2.0"
#define Publisher "PetrolRios S.A."

[Setup]
AppId={{8F4B2C71-9A33-4E0D-B7E1-PETROLRIOS02}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#Publisher}
DefaultDirName={autopf}\PetrolRios\Agente
DefaultGroupName=PetrolRios
OutputDir=..\..\dist\instaladores
OutputBaseFilename=PetrolRios-Agente-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
Source: "..\..\dist\PetrolRios-Agente\*"; DestDir: "{app}"; \
  Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\PetrolRios Agente"; Filename: "{app}\PetrolRios.StationAgent.exe"
Name: "{group}\Panel del agente"; Filename: "http://localhost:5180"
Name: "{autodesktop}\PetrolRios Agente"; Filename: "{app}\PetrolRios.StationAgent.exe"

[Run]
Filename: "{app}\PetrolRios.StationAgent.exe"; Description: "Iniciar el agente ahora"; \
  Flags: nowait postinstall skipifsilent

[Messages]
spanish.WelcomeLabel2=Se instalará el Station Agent de PetrolRíos en esta estación de servicio.%n%nTras instalar, edite appsettings.json con: el código de la estación, la URL del servidor central y la conexión al Firebird local de Contaplus. El panel de control queda en http://localhost:5180.
