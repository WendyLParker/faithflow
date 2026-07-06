import { Link } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';

const faqs = [
  {
    q: 'What types of requests can I submit?',
    a: 'The portal supports multiple request types — including prayer requests and supply requests. More types will appear as departments are onboarded.',
  },
  {
    q: 'Who can see my team’s open requests?',
    a: 'Team visibility is based on your assigned department and role. Full team routing is in progress; today you see your own submitted requests.',
  },
  {
    q: 'How do I check the status of a request?',
    a: 'Open Search Requests or your request list from the dashboard. Select a request to view details and updates.',
  },
  {
    q: 'Need additional support?',
    a: 'Contact your portal administrator or department lead. Email and ticketing integration coming soon.',
  },
];

export default function FAQ() {
  return (
    <div className="page-container">
      <Link to="/dashboard" className="back-link">
        <ArrowLeft size={16} />
        Back to dashboard
      </Link>

      <h1 className="page-title">Frequently Asked Questions</h1>
      <p className="page-subtitle">Answers and support for using the portal.</p>

      <div className="space-y-3">
        {faqs.map((item) => (
          <article key={item.q} className="content-card-sm">
            <h2 className="font-semibold text-neutral-100 text-sm">{item.q}</h2>
            <p className="text-sm text-neutral-400 mt-2 leading-relaxed">{item.a}</p>
          </article>
        ))}
      </div>
    </div>
  );
}
