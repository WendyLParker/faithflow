import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000',   // Your backend URL
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Automatically attach JWT token if available
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken'); // we'll use this later
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;