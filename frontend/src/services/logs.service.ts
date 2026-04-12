import { api } from "./api";
import type { LogAuditoriaResponse } from "@/types/log";
import type { PaginatedResponse } from "@/types/common";

export const logsService = {
  getAll: (page = 1, pageSize = 50) =>
    api
      .get<PaginatedResponse<LogAuditoriaResponse>>("/logs", {
        params: { page, pageSize },
      })
      .then((r) => r.data),
};
