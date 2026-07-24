import type { LucideIcon } from 'lucide-react';
import { Building2, FilePlus, List, MessageCircle, Search, ShieldCheck } from 'lucide-react';

export type TileTone = 'slate' | 'sage' | 'sand' | 'mist' | 'clay' | 'stone';

export type DashboardAction = {
  id: string;
  title: string;
  description: string;
  to: string;
  icon: LucideIcon;
  tone: TileTone;
};

const lightTile = 'bg-[#f4f0e9] border-[#17130f] hover:bg-[#e7e2d9] hover:border-[#17130f]';

export const tileToneStyles: Record<
  TileTone,
  { tile: string; icon: string; description: string }
> = {
  slate: {
    tile: lightTile,
    icon: 'text-[#9e1b32]',
    description: 'text-[#6f675b]',
  },
  sage: {
    tile: lightTile,
    icon: 'text-[#6f7a5b]',
    description: 'text-[#6f675b]',
  },
  sand: {
    tile: lightTile,
    icon: 'text-[#9c7a4d]',
    description: 'text-[#6f675b]',
  },
  mist: {
    tile: lightTile,
    icon: 'text-[#5f7370]',
    description: 'text-[#6f675b]',
  },
  clay: {
    tile: lightTile,
    icon: 'text-[#a65a4a]',
    description: 'text-[#6f675b]',
  },
  stone: {
    tile: lightTile,
    icon: 'text-[#8a6f3e]',
    description: 'text-[#6f675b]',
  },
};

export const dashboardActions: DashboardAction[] = [
  {
    id: 'create',
    title: 'Create Request',
    description: 'Start a new request. Pick a type and describe what you need.',
    to: '/add',
    icon: FilePlus,
    tone: 'slate',
  },
  {
    id: 'track',
    title: 'Track Requests',
    description: 'View and follow the status of your open and completed requests.',
    to: '/requests',
    icon: List,
    tone: 'sage',
  },
  {
    id: 'search',
    title: 'Search Requests',
    description: 'Find requests by title, type, or keyword.',
    to: '/search',
    icon: Search,
    tone: 'mist',
  },
  {
    id: 'departments',
    title: 'Departments',
    description: 'See request types grouped by the department that handles them.',
    to: '/departments',
    icon: Building2,
    tone: 'stone',
  },
  {
    id: 'ask',
    title: 'View FAQs',
    description: 'Get answers to frequently asked questions.',
    to: '/faq',
    icon: MessageCircle,
    tone: 'sand',
  },
  {
    id: 'admin',
    title: 'Admin',
    description: 'Manage users and groups to control notification routing.',
    to: '/group-management',
    icon: ShieldCheck,
    tone: 'clay',
  }
];
