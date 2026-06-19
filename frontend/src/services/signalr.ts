import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;
let connectionReady: Promise<signalR.HubConnection> | null = null;
type SignalRHandler = (...args: unknown[]) => void;
const handlers = new Map<string, Set<SignalRHandler>>();
const RETRY_DELAY_MS = 2000;

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

export function createSignalRConnection(token: string): signalR.HubConnection {
  if (connection) {
    void connection.stop();
  }

  const conn = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/alerts", { accessTokenFactory: () => token })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    // Los fallos transitorios se gestionan abajo. Evita que el aborto esperado del
    // primer montaje de React Strict Mode aparezca como error rojo en consola.
    .configureLogging(signalR.LogLevel.Critical)
    .build();

  for (const [eventName, eventHandlers] of handlers) {
    for (const handler of eventHandlers) {
      conn.on(eventName, handler);
    }
  }

  connection = conn;
  connectionReady = startCurrentConnection(conn);

  return conn;
}

export function stopSignalRConnection(): void {
  if (connection) {
    const current = connection;
    connection = null;
    connectionReady = null;
    void current.stop();
  }
}

async function startCurrentConnection(
  conn: signalR.HubConnection,
): Promise<signalR.HubConnection> {
  while (connection === conn) {
    try {
      await conn.start();
      return conn;
    } catch (error) {
      // El proveedor ya reemplazó/detuvo esta conexión (montaje estricto o logout).
      if (connection !== conn) return conn;

      console.warn(
        `SignalR no disponible; nuevo intento en ${RETRY_DELAY_MS / 1000} s.`,
        error,
      );
      await new Promise((resolve) => window.setTimeout(resolve, RETRY_DELAY_MS));
    }
  }

  return conn;
}
