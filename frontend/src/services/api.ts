import axios from 'axios';
import type { AuthResponse, CreatePoemRequest, Poem, PoemListResponse, UpdatePoemRequest } from '../types';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const authService = {
  async register(email: string, password: string, displayName: string): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/register', {
      email,
      password,
      displayName,
    });
    return response.data;
  },

  async login(email: string, password: string): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/login', {
      email,
      password,
    });
    return response.data;
  },
};

export const poemService = {
  async getFeed(page = 1, pageSize = 10): Promise<PoemListResponse> {
    const response = await api.get<PoemListResponse>('/poems/feed', {
      params: { page, pageSize },
    });
    return response.data;
  },

  async getMyPoems(page = 1, pageSize = 10): Promise<PoemListResponse> {
    const response = await api.get<PoemListResponse>('/poems/my-poems', {
      params: { page, pageSize },
    });
    return response.data;
  },

  async getById(id: number): Promise<Poem> {
    const response = await api.get<Poem>(`/poems/${id}`);
    return response.data;
  },

  async create(data: CreatePoemRequest): Promise<Poem> {
    const response = await api.post<Poem>('/poems', data);
    return response.data;
  },

  async update(id: number, data: UpdatePoemRequest): Promise<Poem> {
    const response = await api.put<Poem>(`/poems/${id}`, data);
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/poems/${id}`);
  },
};

export default api;
