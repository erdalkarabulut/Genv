import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { getStoredToken } from "./auth";

let connection: HubConnection | null = null;
let startPromise: Promise<HubConnection> | null = null;
const TENANT = "default";

export type CryoEvent =
  | "BagStored"
  | "BagMoved"
  | "BagUsed"
  | "DashboardUpdated";

type StateListener = (state: HubConnectionState) => void;
const stateListeners = new Set<StateListener>();

function notifyState(state: HubConnectionState) {
  stateListeners.forEach((l) => {
    try {
      l(state);
    } catch {
      /* swallow */
    }
  });
}

export function onConnectionState(listener: StateListener): () => void {
  stateListeners.add(listener);
  // Emit current state immediately so consumers don't have to poll.
  if (connection) listener(connection.state);
  return () => stateListeners.delete(listener);
}

async function joinTenant(c: HubConnection): Promise<void> {
  try {
    await c.invoke("JoinTenant", TENANT);
  } catch (err) {
    // Non-fatal: without the group the client silently misses broadcasts,
    // but there is nothing useful the app can do besides log.
    // eslint-disable-next-line no-console
    console.warn("[signalr] JoinTenant failed", err);
  }
}

function buildConnection(): HubConnection {
  const c = new HubConnectionBuilder()
    .withUrl("/hubs/cryo", {
      accessTokenFactory: () => getStoredToken() ?? "",
    })
    .withAutomaticReconnect([0, 1000, 2000, 5000, 10000, 15000])
    .configureLogging(LogLevel.Warning)
    .build();

  c.onreconnecting(() => notifyState(HubConnectionState.Reconnecting));
  c.onreconnected(async () => {
    // SignalR reconnect creates a new ConnectionId on the server,
    // so the previous group membership is gone. Re-join the tenant
    // group before any more broadcasts arrive.
    await joinTenant(c);
    notifyState(HubConnectionState.Connected);
  });
  c.onclose(() => {
    notifyState(HubConnectionState.Disconnected);
  });
  return c;
}

export async function getCryoConnection(): Promise<HubConnection> {
  if (connection && connection.state === HubConnectionState.Connected) {
    return connection;
  }
  if (startPromise) return startPromise;

  if (!connection) connection = buildConnection();
  const c = connection;

  if (c.state !== HubConnectionState.Disconnected) {
    // Already starting or reconnecting — wait briefly and return.
    return c;
  }

  startPromise = (async () => {
    try {
      await c.start();
      notifyState(HubConnectionState.Connected);
      await joinTenant(c);
    } finally {
      startPromise = null;
    }
    return c;
  })();

  return startPromise;
}

export function onCryo<TPayload = unknown>(
  event: CryoEvent,
  handler: (payload: TPayload) => void,
): () => void {
  let active = true;
  let unsub: (() => void) | null = null;

  getCryoConnection()
    .then((c) => {
      if (!active) return;
      const fn = (p: TPayload) => handler(p);
      c.on(event, fn);
      unsub = () => c.off(event, fn);
    })
    .catch(() => {
      /* connection failed; will retry via automatic reconnect */
    });

  return () => {
    active = false;
    if (unsub) unsub();
  };
}
