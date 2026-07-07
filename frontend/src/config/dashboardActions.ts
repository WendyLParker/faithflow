import type { LucideIcon } from 'lucide-react';
import { FilePlus, List, MessageCircle, Search } from 'lucide-react';

export type TileTone = 'slate' | 'sage' | 'sand' | 'mist';

export type DashboardAction = {
  id: string;
  title: string;
  description: string;
  to: string;
  icon: LucideIcon;
  tone: TileTone;
};

export const tileToneStyles: Record<
  TileTone,
  { tile: string; icon: string; description: string }
> = {
  slate: {
    tile: 'bg-[#2e3640] border-[#3d4654] hover:bg-[#343d48] hover:border-[#4a5563]',
    icon: 'text-[#9aa8b8]',
    description: 'text-[#8b97a5]',
  },
  sage: {
    tile: 'bg-[#2f3834] border-[#3d4a44] hover:bg-[#354038] hover:border-[#4a5c54]',
    icon: 'text-[#9bada3]',
    description: 'text-[#8a9d93]',
  },
  sand: {
    tile: 'bg-[#38342f] border-[#4a443d] hover:bg-[#3e3a35] hover:border-[#5c554d]',
    icon: 'text-[#b5a99a]',
    description: 'text-[#a39688]',
  },
  mist: {
    tile: 'bg-[#2d3636] border-[#3b4848] hover:bg-[#333c3c] hover:border-[#4a5959]',
    icon: 'text-[#96abab]',
    description: 'text-[#859a9a]',
  },
};

export const dashboardActions: DashboardAction[] = [
  {
    id: 'create',
    title: 'Create Request',
    description: 'Start a new request. Pick a type and assign to a department.',
    to: '/add',
    icon: FilePlus,
    tone: 'slate',
  },
  {
    id: 'track',
    title: 'Track Requests',
    description: 'View and follow the status of your open and completed requests.',
    to: '/prayers',
    icon: List,
    tone: 'sage',
  },
  {
    id: 'search',
    title: 'Search Requests',
    description: 'Find requests by title, category, or keyword.',
    to: '/search',
    icon: Search,
    tone: 'mist',
  },
  {
    id: 'ask',
    title: 'View FAQs',
    description: 'Get answers to frequently asked questions.',
    to: '/faq',
    icon: MessageCircle,
    tone: 'sand',
  }
];
