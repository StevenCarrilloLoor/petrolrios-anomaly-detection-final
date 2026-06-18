import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;
let connectionReady: Promise<signalR.HubConnection> | null = null;

export function getSignalRConnection(): signalR.HubConnection | null {
  return connection;
}

export function getConnectionReady(): Promise<signalR.HubConnection> | null {
  return connectionReady;
}

export function createSignalRConnection(
  token: string,
  user?: { id: number; nombreCompleto: string; rol: string },
): signalR.HubConnection {
  if (connection) {
    void connection.stop();
  }

  // Identidad del usuario por query (respaldo de los claims del JWT) para "Usuarios conectados".
  const params = new URLSearchParams();
  if (user) {
    params.set("usuarioId", String(user.id));
    params.set("nombre", user.nombreCompleto);
    params.set("rol", user.rol);
  }
  const url = "/hubs/alerts" + (params.toString() ? `?${params.toString()}` : "");

  const conn = new signalR.HubConnectionBuilder()
    .withUrl(url, { accessTokenFactory: () => token })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  connection = conn;
  connectionReady = conn.start().then(() => conn);

  return conn;
}

export function stopSignalRConnection(): void {
  if (connection) {
    void connection.stop();
    connection = null;
    connectionReady = null;
  }
}
