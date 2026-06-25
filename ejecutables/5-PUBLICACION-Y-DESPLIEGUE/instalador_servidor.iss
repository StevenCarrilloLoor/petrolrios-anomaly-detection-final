; Instalador del Servidor Central PetrolRíos (Inno Setup 6)
; Compilar con: ISCC.exe instalador_servidor.iss (o desde publicar.bat)

#define AppName "PetrolRios Servidor Central"
#define AppVersion "2.0"
#define Publisher "PetrolRios S.A."

[Setup]
AppId={{8F4B2C71-9A33-4E0D-B7E1-PETROLRIOS01}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#Publisher}
DefaultDirName={autopf}\PetrolRios\Servidor
DefaultGroupName=PetrolRios
OutputDir=..\..\dist\instaladores
OutputBaseFilename=PetrolRios-Servidor-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
Source: "..\..\dist\PetrolRios-Servidor\*"; DestDir: "{app}"; \
  Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\PetrolRios Servidor"; Filename: "{app}\PetrolRios.Api.exe"
Name: "{group}\Aplicacion web"; Filename: "http://localhost:5170"
Name: "{autodesktop}\PetrolRios Servidor"; Filename: "{app}\PetrolRios.Api.exe"

[Run]
Filename: "{app}\PetrolRios.Api.exe"; Description: "Iniciar el servidor ahora"; \
  Flags: nowait postinstall skipifsilent

[Messages]
spanish.WelcomeLabel2=Se instalará el Servidor Central del Sistema de Detección de Anomalías de PetrolRíos S.A.%n%nRequiere una instancia de PostgreSQL accesible (configure appsettings.json tras la instalación).
