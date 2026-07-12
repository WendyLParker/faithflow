import api from '@/lib/api';

export interface NotificationDto {
  id: number;
  type: 'NewRequest' | 'RequestAcknowledged';
  isRead: boolean;
  createdAt: string;
  requestId: number;
  requestTitle: string;
  requestContent?: string;
  requestTypeName: string;
  requestStatus: string;
}

export interface GroupDto {
  id: number;
  name: string;
  description?: string;
}

export interface GroupMemberDto {
  id: number;
  userId: string;
  displayName: string;
  userEmail: string;
  emailNotificationsEnabled: boolean;
  groupId: number;
  groupName: string;
  canManage: boolean;
}

export interface AddGroupMemberDto {
  userId: string;
  displayName: string;
  userEmail: string;
  emailNotificationsEnabled: boolean;
}

export interface CreateGroupDto {
  name: string;
  description?: string;
}

export interface GroupManagerAssignmentDto {
  groupId: number;
  groupName: string;
  canManage: boolean;
}

export interface SetGroupManagersDto {
  userId: string;
  displayName: string;
  userEmail: string;
  groupIds: number[];
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

export const groupService = {
  async getAll(): Promise<GroupDto[]> {
    const res = await api.get<GroupDto[]>('/api/group');
    return res.data;
  },

  async getManaged(): Promise<GroupDto[]> {
    const res = await api.get<GroupDto[]>('/api/group/managed');
    return res.data;
  },

  async create(data: CreateGroupDto): Promise<GroupDto> {
    const res = await api.post<GroupDto>('/api/group', data);
    return res.data;
  },

  async delete(groupId: number): Promise<void> {
    await api.delete(`/api/group/${groupId}`);
  },

  async getMembers(groupId: number): Promise<GroupMemberDto[]> {
    const res = await api.get<GroupMemberDto[]>(`/api/group/${groupId}/members`);
    return res.data;
  },

  async getMyMemberships(): Promise<GroupMemberDto[]> {
    const res = await api.get<GroupMemberDto[]>('/api/group/my');
    return res.data;
  },

  async addMember(groupId: number, data: AddGroupMemberDto): Promise<GroupMemberDto> {
    const res = await api.post<GroupMemberDto>(`/api/group/${groupId}/members`, data);
    return res.data;
  },

  async updateEmailPreference(membershipId: number, enabled: boolean): Promise<void> {
    await api.patch(`/api/group/members/${membershipId}/email-preference`, {
      emailNotificationsEnabled: enabled,
    });
  },

  async removeMember(membershipId: number): Promise<void> {
    await api.delete(`/api/group/members/${membershipId}`);
  },

  async updateManager(membershipId: number, canManage: boolean): Promise<void> {
    await api.patch(`/api/group/members/${membershipId}/manager`, { canManage });
  },

  async getManagerAssignments(userId: string): Promise<GroupManagerAssignmentDto[]> {
    const res = await api.get<GroupManagerAssignmentDto[]>(`/api/group/manager-assignments/${userId}`);
    return res.data;
  },

  async setManagerAssignments(data: SetGroupManagersDto): Promise<void> {
    await api.post('/api/group/manager-assignments', data);
  },
};
