import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;
let connectionReady: Promise<signalR.HubConnection> | null = null;

export function getSignalRConnection(): signalR.HubConnection | null {
  return connection;
}

export function getConnectionReady(): Promise<signalR.HubConnection> | null {
  return connectionReady;
}

export function createSignalRConnection(token: string): signalR.HubConnection {
  if (connection) {
    void connection.stop();
  }

  const conn = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/alerts", { accessTokenFactory: () => token })
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
