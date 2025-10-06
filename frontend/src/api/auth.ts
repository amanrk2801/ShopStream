import api from './client'

export interface RegisterRequest {
  email: string
  password: string
  firstName: string
  lastName: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface AuthResponse {
  token: string
  email: string
  firstName: string
  lastName: string
  role: string
}

export const authApi = {
  register: (data: RegisterRequest) => 
    api.post<AuthResponse>('/auth/register', data),
  
  login: (data: LoginRequest) => 
    api.post<AuthResponse>('/auth/login', data),
}
