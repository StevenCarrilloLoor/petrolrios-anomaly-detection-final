export interface ConexionEstacionResponse {
  estacionId: number;
  codigo: string;
  nombre: string;
  zona: string;
  conectada: boolean;
  estado: "Conectada" | "Sin conexión" | "Nunca conectada";
  ultimaIngesta: string | null;
  minutosDesdeUltimaIngesta: number | null;
  transaccionesUltimas24Horas: number;
  transaccionesTotales: number;
  pendientesAnalisis: number;
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
