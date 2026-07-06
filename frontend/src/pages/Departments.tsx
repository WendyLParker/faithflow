import { Link } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';

const departments = [
  {
    name: 'Chaplain Services',
    types: ['Prayer Request', 'Pastoral Care'],
  },
  {
    name: 'Supply & Logistics',
    types: ['Supply Request', 'Equipment Request'],
  },
  {
    name: 'Facilities',
    types: ['Maintenance Request', 'Space Request'],
  },
  {
    name: 'Human Resources',
    types: ['Leave Request', 'Benefits Inquiry'],
  },
];

export default function Departments() {
  return (
    <div className="page-container">
      <Link to="/dashboard" className="back-link">
        <ArrowLeft size={16} />
        Back to dashboard
      </Link>

      <h1 className="page-title">Departments</h1>
      <p className="page-subtitle">Request types by department — routing rules coming soon.</p>

      <ul className="space-y-3">
        {departments.map((dept) => (
          <li key={dept.name} className="content-card-sm">
            <h2 className="font-semibold text-neutral-100">{dept.name}</h2>
            <p className="text-sm text-neutral-400 mt-2">{dept.types.join(' · ')}</p>
          </li>
        ))}
      </ul>
    </div>
  );
}
