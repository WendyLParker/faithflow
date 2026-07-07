import api from '@/lib/api';

export interface NotificationDto {
  id: number;
  type: 'NewRequest' | 'RequestAcknowledged';
  isRead: boolean;
  createdAt: string;
  prayerId: number;
  prayerTitle: string;
  prayerContent?: string;
  requestTypeName: string;
  requestStatus: string;
}

export interface DepartmentDto {
  id: number;
  name: string;
  description?: string;
  requestTypeNames: string[];
}

export interface DepartmentMemberDto {
  id: number;
  userId: string;
  displayName: string;
  userEmail: string;
  emailNotificationsEnabled: boolean;
  departmentId: number;
  departmentName: string;
}

export interface AddDepartmentMemberDto {
  userId: string;
  displayName: string;
  userEmail: string;
  emailNotificationsEnabled: boolean;
}

export const notificationService = {
  async getAll(): Promise<NotificationDto[]> {
    const res = await api.get<NotificationDto[]>('/api/notification');
    return res.data;
  },

  async getUnreadCount(): Promise<number> {
    const res = await api.get<{ count: number }>('/api/notification/unread-count');
    return res.data.count;
  },

  async acknowledge(id: number): Promise<void> {
    await api.post(`/api/notification/${id}/acknowledge`);
  },

  async dismiss(id: number): Promise<void> {
    await api.post(`/api/notification/${id}/dismiss`);
  },
};

export const departmentService = {
  async getAll(): Promise<DepartmentDto[]> {
    const res = await api.get<DepartmentDto[]>('/api/department');
    return res.data;
  },

  async getMembers(departmentId: number): Promise<DepartmentMemberDto[]> {
    const res = await api.get<DepartmentMemberDto[]>(`/api/department/${departmentId}/members`);
    return res.data;
  },

  async getMyMemberships(): Promise<DepartmentMemberDto[]> {
    const res = await api.get<DepartmentMemberDto[]>('/api/department/my');
    return res.data;
  },

  async addMember(departmentId: number, data: AddDepartmentMemberDto): Promise<DepartmentMemberDto> {
    const res = await api.post<DepartmentMemberDto>(`/api/department/${departmentId}/members`, data);
    return res.data;
  },

  async updateEmailPreference(membershipId: number, enabled: boolean): Promise<void> {
    await api.patch(`/api/department/members/${membershipId}/email-preference`, {
      emailNotificationsEnabled: enabled,
    });
  },

  async removeMember(membershipId: number): Promise<void> {
    await api.delete(`/api/department/members/${membershipId}`);
  },
};
