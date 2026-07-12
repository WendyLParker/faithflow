import api from '@/lib/api';

export type AppRole = 'Admin' | 'Member';

export interface MyRoleDto {
  userId: string;
  role: AppRole;
  isAdmin: boolean;
  displayName: string;
  profileColor?: string;
}

export interface UpdateProfileDto {
  displayName?: string;
  profileColor?: string;
}

export interface UserRoleDto {
  userId: string;
  displayName: string;
  userEmail: string;
  role: AppRole;
}

export interface SetUserRoleDto {
  userId: string;
  displayName: string;
  userEmail: string;
  role: AppRole;
}

export const userRoleService = {
  async getMine(): Promise<MyRoleDto> {
    const params: Record<string, string> = {};
    const idToken = localStorage.getItem('idToken');
    if (idToken) {
      try {
        const payload = idToken.split('.')[1];
        const decoded = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
        if (decoded.email) params.email = String(decoded.email);
      } catch {
        // ignore malformed token; backend will try access-token claims
      }
    }

    const res = await api.get<MyRoleDto>('/api/userrole/me', { params });
    return res.data;
  },

  async getAll(): Promise<UserRoleDto[]> {
    const res = await api.get<UserRoleDto[]>('/api/userrole');
    return res.data;
  },

  async setRole(data: SetUserRoleDto): Promise<UserRoleDto> {
    const res = await api.post<UserRoleDto>('/api/userrole', data);
    return res.data;
  },

  async updateMyProfile(data: UpdateProfileDto): Promise<MyRoleDto> {
    const res = await api.patch<MyRoleDto>('/api/userrole/me', data);
    return res.data;
  },
};
