import axios from 'axios';
import { useAuthStore } from '../store/authStore';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5177/api/v1',
  headers: { 'Content-Type': 'application/json' },
});

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

let isRefreshing = false;
let refreshQueue: Array<(token: string) => void> = [];

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;
    const status = error.response?.status;

    if (status === 401 && !original._retry) {
      original._retry = true;

      const { refreshToken, setTokens, logout } = useAuthStore.getState();
      if (!refreshToken) { logout(); return Promise.reject(error); }

      if (isRefreshing) {
        return new Promise((resolve) => {
          refreshQueue.push((token) => {
            original.headers.Authorization = `Bearer ${token}`;
            resolve(apiClient(original));
          });
        });
      }

      isRefreshing = true;
      try {
        const { data } = await axios.post(
          `${apiClient.defaults.baseURL ?? 'http://localhost:5177/api/v1'}/auth/refresh-token`,
          { refreshToken },
          { headers: { 'Content-Type': 'application/json' } }
        );
        const { accessToken, refreshToken: newRefresh } = data.data;
        setTokens(accessToken, newRefresh);
        refreshQueue.forEach((cb) => cb(accessToken));
        refreshQueue = [];
        original.headers.Authorization = `Bearer ${accessToken}`;
        return apiClient(original);
      } catch {
        logout();
        return Promise.reject(error);
      } finally {
        isRefreshing = false;
      }
    }

    if (status === 403) {
      window.dispatchEvent(new CustomEvent('auth:forbidden'));
    }

    return Promise.reject(error);
  }
);

export default apiClient;
