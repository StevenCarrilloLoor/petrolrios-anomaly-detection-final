export interface ConexionEstacionResponse {
  estacionId: number;
  codigo: string;
  nombre: string;
  zona: string;
  activa: boolean;
  conectada: boolean;
  estado: "En línea" | "Sin conexión" | "Nunca conectada";
  ultimoHeartbeat: string | null;
  minutosDesdeUltimoHeartbeat: number | null;
  versionAgente: string | null;
  ultimaIngesta: string | null;
  minutosDesdeUltimaIngesta: number | null;
  transaccionesUltimas24Horas: number;
  transaccionesTotales: number;
  pendientesAnalisis: number;
  horaApertura: string;
  horaCierre: string;
  correoContacto: string | null;
}

export interface EstadoSistemaResponse {
  versionApi: string;
  inicioApi: string;
  uptimeSegundos: number;
  entorno: string;
  baseDatosConectada: boolean;
  latenciaBaseDatosMs: number | null;
  clientesSignalRConectados: number;
  estacionesConectadas: number;
  estacionesTotales: number;
  ultimoCicloDeteccion: string | null;
  ultimoCicloEstado: string | null;
  ultimoCicloAlertas: number | null;
  ultimoCicloDuracionSegundos: number | null;
  minutosDesdeUltimoCiclo: number | null;
}
