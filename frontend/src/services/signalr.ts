import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;
let connectionReady: Promise<signalR.HubConnection> | null = null;
type SignalRHandler = (...args: unknown[]) => void;
const handlers = new Map<string, Set<SignalRHandler>>();

export function getSignalRConnection(): signalR.HubConnection | null {
  return connection;
}

export function getConnectionReady(): Promise<signalR.HubConnection> | null {
  return connectionReady;
}

/**
 * Registra un evento tanto en la conexión actual como en cualquier conexión que
 * React vuelva a crear durante montaje estricto, refresh de token o reconexión.
 */
export function subscribeSignalREvent<T>(
  eventName: string,
  handler: (payload: T) => void,
): () => void {
  const wrapped: SignalRHandler = (...args) => handler(args[0] as T);
  const eventHandlers = handlers.get(eventName) ?? new Set<SignalRHandler>();
  eventHandlers.add(wrapped);
  handlers.set(eventName, eventHandlers);
  connection?.on(eventName, wrapped);

  return () => {
    handlers.get(eventName)?.delete(wrapped);
    connection?.off(eventName, wrapped);
  };
}

export function createSignalRConnection(
  token: string,
  user?: {
    id: number;
    nombreCompleto: string;
    rol: string;
    estacionId?: number | null;
  },
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
    if (user.estacionId != null) {
      params.set("estacionId", String(user.estacionId));
    }
  }
  const url = "/hubs/alerts" + (params.toString() ? `?${params.toString()}` : "");

  const conn = new signalR.HubConnectionBuilder()
    .withUrl(url, { accessTokenFactory: () => token })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  for (const [eventName, eventHandlers] of handlers) {
    for (const handler of eventHandlers) {
      conn.on(eventName, handler);
    }
  }

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
