import { api } from "./api";

export interface ReporteFilters {
  tipo?: string;
  nivelRiesgo?: string;
  estado?: string;
  estacionId?: number;
  fechaDesde?: string;
  fechaHasta?: string;
}

async function descargarArchivo(url: string, filters: ReporteFilters, nombre: string) {
  const response = await api.get(url, {
    params: filters,
    responseType: "blob",
  });

  const blob = new Blob([response.data]);
  const enlace = document.createElement("a");
  enlace.href = URL.createObjectURL(blob);
  enlace.download = nombre;
  document.body.appendChild(enlace);
  enlace.click();
  enlace.remove();
  URL.revokeObjectURL(enlace.href);
}

export const reportesService = {
  descargarPdf: (filters: ReporteFilters) =>
    descargarArchivo(
      "/reportes/alertas/pdf",
      filters,
      `reporte-alertas-${new Date().toISOString().slice(0, 10)}.pdf`,
    ),

  descargarExcel: (filters: ReporteFilters) =>
    descargarArchivo(
      "/reportes/alertas/excel",
      filters,
      `reporte-alertas-${new Date().toISOString().slice(0, 10)}.xlsx`,
    ),
};
