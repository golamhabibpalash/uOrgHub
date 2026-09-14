import axios from 'axios';
import toast from 'react-hot-toast';
import { useAuthStore } from '../store/authStore';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5177/api/v1',
  headers: { 'Content-Type': 'application/json' },
});

function extractFirstError(errors: unknown): string | undefined {
  if (!errors) return undefined;
  if (Array.isArray(errors)) return errors[0];
  if (typeof errors === 'object') {
    const vals = Object.values(errors as Record<string, unknown>);
    const first = vals[0];
    if (Array.isArray(first)) return first[0];
    if (typeof first === 'string') return first;
  }
  return undefined;
}

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

let isRefreshing = false;
// Requests that 401'd while a refresh was already in flight wait here for its outcome — resolved
// with the new token on success, rejected on failure so they fail cleanly instead of hanging.
let refreshQueue: Array<{ resolve: (token: string) => void; reject: (reason: unknown) => void }> = [];

function settleQueue(token: string | null, error?: unknown) {
  const queued = refreshQueue;
  refreshQueue = [];
  queued.forEach(({ resolve, reject }) => (token ? resolve(token) : reject(error)));
}

apiClient.interceptors.response.use(
  (response) => {
    const method = response.config.method?.toLowerCase();
    if (method && ['post', 'put', 'delete', 'patch'].includes(method)) {
      const msg = response.data?.message;
      if (msg && msg !== 'Success') {
        toast.success(msg, { duration: 3000 });
      }
    }
    return response;
  },
  async (error) => {
    const original = error.config;
    const status = error.response?.status;

    if (status === 401 && !original._retry) {
      original._retry = true;

      const { token, refreshToken, setTokens, logout } = useAuthStore.getState();
      if (!token && !refreshToken) {
        return Promise.reject(error);
      }
      if (!refreshToken) { logout(); return Promise.reject(error); }

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          refreshQueue.push({
            resolve: (token) => {
              original.headers.Authorization = `Bearer ${token}`;
              resolve(apiClient(original));
            },
            reject,
          });
        });
      }

      isRefreshing = true;
      const attemptedRefreshToken = refreshToken;
      try {
        const { data } = await axios.post(
          `${apiClient.defaults.baseURL ?? 'http://localhost:5177/api/v1'}/auth/refresh-token`,
          { refreshToken },
          { headers: { 'Content-Type': 'application/json' } }
        );
        const { accessToken, refreshToken: newRefresh } = data.data;
        setTokens(accessToken, newRefresh);
        settleQueue(accessToken);
        original.headers.Authorization = `Bearer ${accessToken}`;
        return apiClient(original);
      } catch (refreshError) {
        // Refresh tokens are single-use — another tab (or a request that beat this one to the
        // server) may have already redeemed this exact token for a new pair, which is why the
        // server just rejected it. That doesn't mean the session is dead: if the store now holds a
        // *different* refresh token than the one we tried, someone else already rotated it, so
        // retry with what's current instead of logging out from under an otherwise-live session.
        const latest = useAuthStore.getState();
        if (latest.refreshToken && latest.refreshToken !== attemptedRefreshToken && latest.token) {
          settleQueue(latest.token);
          original.headers.Authorization = `Bearer ${latest.token}`;
          return apiClient(original);
        }
        settleQueue(null, refreshError);
        logout();
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    const errMsg = error.response?.data?.message
      || extractFirstError(error.response?.data?.errors)
      || error.message
      || 'An error occurred';
    if (status && status !== 401 && status !== 403) {
      toast.error(errMsg, { duration: 4000 });
    }

    if (status === 403) {
      window.dispatchEvent(new CustomEvent('auth:forbidden'));
    }

    return Promise.reject(error);
  }
);

export default apiClient;
