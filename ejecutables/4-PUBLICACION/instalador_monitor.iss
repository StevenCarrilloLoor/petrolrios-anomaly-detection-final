; Instalador del Monitor de Estación PetrolRíos (Inno Setup 6)

#define AppName "PetrolRios Monitor de Estacion"
#define AppVersion "2.2"
#define Publisher "PetrolRios S.A."

[Setup]
AppId={{8F4B2C71-9A33-4E0D-B7E1-PETROLRIOS03}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#Publisher}
DefaultDirName={autopf}\PetrolRios\Monitor
DefaultGroupName=PetrolRios
OutputDir=..\..\dist\instaladores
OutputBaseFilename=PetrolRios-Monitor-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
Source: "..\..\dist\PetrolRios-Monitor\*"; DestDir: "{app}"; \
  Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\PetrolRios Monitor"; Filename: "{app}\PetrolRios.StationMonitor.exe"
Name: "{group}\Panel del monitor"; Filename: "http://localhost:5190"
Name: "{autodesktop}\Monitor PetrolRios"; Filename: "http://localhost:5190"

[Run]
Filename: "{app}\PetrolRios.StationMonitor.exe"; Description: "Iniciar el monitor ahora"; \
  Flags: nowait postinstall skipifsilent

[Messages]
spanish.WelcomeLabel2=Se instalará el Monitor de Estación de PetrolRíos.%n%nEste componente NO accede a Firebird ni envía transacciones: consulta al servidor central únicamente los problemas operativos de la estación asignada. Su panel queda en http://localhost:5190.
