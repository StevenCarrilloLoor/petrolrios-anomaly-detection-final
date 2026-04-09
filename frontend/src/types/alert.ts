import { z } from "zod";

export const NivelRiesgoSchema = z.enum(["Bajo", "Medio", "Alto", "Critico"]);
export type NivelRiesgo = z.infer<typeof NivelRiesgoSchema>;

export const TipoDetectorSchema = z.enum([
  "CashFraud",
  "InvoiceAnomaly",
  "PaymentFraud",
  "ComplianceViolation",
]);
export type TipoDetector = z.infer<typeof TipoDetectorSchema>;

export const EstadoAlertaSchema = z.enum([
  "Nueva",
  "EnRevision",
  "Confirmada",
  "FalsoPositivo",
  "Cerrada",
]);
export type EstadoAlerta = z.infer<typeof EstadoAlertaSchema>;

export const AlertaSchema = z.object({
  id: z.number(),
  tipoDetector: TipoDetectorSchema,
  nivelRiesgo: NivelRiesgoSchema,
  estado: EstadoAlertaSchema,
  descripcion: z.string(),
  score: z.number(),
  estacionId: z.number(),
  estacionNombre: z.string(),
  fechaDeteccion: z.string(),
});

export type Alerta = z.infer<typeof AlertaSchema>;
