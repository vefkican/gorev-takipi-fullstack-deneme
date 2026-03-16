import axios from "axios";

const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5052/api";

const api = axios.create({
  baseURL: API_URL,
});

// Request interceptor — her isteğe token ekle
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor — 401 gelince refresh token kullan
api.interceptors.response.use(
  (response) => response, // Başarılı isteklerde bir şey yapma
  async (error) => {
    const originalRequest = error.config;

    // 401 geldi ve daha önce retry yapılmadıysa
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true; // Sonsuz döngüyü önle

      try {
        const refreshToken = localStorage.getItem("refreshToken");
        if (!refreshToken) {
          // Refresh token yok, login'e yönlendir
          localStorage.removeItem("token");
          localStorage.removeItem("refreshToken");
          window.location.href = "/login";
          return Promise.reject(error);
        }

        // Yeni token al
        const response = await axios.post(`${API_URL}/auth/refresh`, {
          refreshToken,
        });

        const { accessToken, refreshToken: newRefreshToken } = response.data;

        // Yeni token'ları kaydet
        localStorage.setItem("token", accessToken);
        localStorage.setItem("refreshToken", newRefreshToken);

        // Orijinal isteği yeni token ile tekrar at
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return api(originalRequest);
      } catch {
        // Refresh token da geçersiz — login'e yönlendir
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
        window.location.href = "/login";
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  },
);

export interface TaskItem {
  id: number;
  title: string;
  description?: string;
  isCompleted: boolean;
  createdAt: string;
  dueDate?: string;
  userId: number;
}

export interface CreateTaskDto {
  title: string;
  description?: string;
  dueDate?: string;
}

export interface AuthDto {
  username: string;
  password: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface TokenDto {
  accessToken: string;
  refreshToken: string;
}

export const authService = {
  register: (data: AuthDto) => api.post("/auth/register", data),
  login: (data: AuthDto) => api.post<TokenDto>("/auth/login", data),
  refresh: (refreshToken: string) =>
    api.post<TokenDto>("/auth/refresh", { refreshToken }),
  logout: (refreshToken: string) => api.post("/auth/logout", { refreshToken }),
};

export const taskService = {
  getAll: (
    search?: string,
    isCompleted?: boolean,
    page: number = 1,
    pageSize: number = 10,
  ) => {
    const params: Record<string, string> = {};
    if (search) params.search = search;
    if (isCompleted !== undefined) params.isCompleted = isCompleted.toString();
    params.page = page.toString();
    params.pageSize = pageSize.toString();
    return api.get<PagedResult<TaskItem>>("/tasks", { params });
  },
  create: (data: CreateTaskDto) => api.post<TaskItem>("/tasks", data),
  update: (id: number, data: TaskItem) => api.put(`/tasks/${id}`, data),
  delete: (id: number) => api.delete(`/tasks/${id}`),
};
