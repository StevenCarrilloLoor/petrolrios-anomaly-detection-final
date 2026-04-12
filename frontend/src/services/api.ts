import axios from "axios";

const api = axios.create({
  baseURL: "/api/v1",
  headers: { "Content-Type": "application/json" },
});

interface QueueItem {
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}

let isRefreshing = false;
let failedQueue: QueueItem[] = [];
const retriedRequests = new WeakSet<object>();

function processQueue(error: unknown, token: string | null): void {
  for (const item of failedQueue) {
    if (error) {
      item.reject(error);
    } else if (token) {
      item.resolve(token);
    }
  }
  failedQueue = [];
}

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (!axios.isAxiosError(error) || !error.config) {
      return Promise.reject(error);
    }

    const config = error.config;

    if (error.response?.status === 401 && !retriedRequests.has(config)) {
      if (isRefreshing) {
        return new Promise<string>((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((token) => {
          config.headers.Authorization = `Bearer ${token}`;
          return api(config);
        });
      }

      retriedRequests.add(config);
      isRefreshing = true;

      const refreshToken = localStorage.getItem("refreshToken");
      if (!refreshToken) {
        localStorage.clear();
        window.location.href = "/login";
        return Promise.reject(error);
      }

      try {
        const { data } = await axios.post<{ token: string; refreshToken: string }>(
          "/api/v1/auth/refresh",
          { refreshToken }
        );
        localStorage.setItem("token", data.token);
        localStorage.setItem("refreshToken", data.refreshToken);
        processQueue(null, data.token);
        config.headers.Authorization = `Bearer ${data.token}`;
        return api(config);
      } catch (refreshError) {
        processQueue(refreshError, null);
        localStorage.clear();
        window.location.href = "/login";
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export { api };
