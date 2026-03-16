import axios from "axios";

const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5052/api";

const api = axios.create({
  baseURL: API_URL,
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

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

export const authService = {
  register: (data: AuthDto) => api.post("/auth/register", data),
  login: (data: AuthDto) => api.post<{ token: string }>("/auth/login", data),
};

export const taskService = {
  getAll: (search?: string, isCompleted?: boolean) => {
    const params: Record<string, string> = {};
    if (search) params.search = search;
    if (isCompleted !== undefined) params.isCompleted = isCompleted.toString();
    return api.get<TaskItem[]>("/tasks", { params });
  },
  create: (data: CreateTaskDto) => api.post<TaskItem>("/tasks", data),
  update: (id: number, data: TaskItem) => api.put(`/tasks/${id}`, data),
  delete: (id: number) => api.delete(`/tasks/${id}`),
};
