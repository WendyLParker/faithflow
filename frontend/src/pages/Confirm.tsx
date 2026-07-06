import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { API_BASE_URL } from '../config';

const Confirm = () => {
  const location = useLocation();
  const prefilledEmail = (location.state as { email?: string } | null)?.email ?? '';

  const [email, setEmail] = useState(prefilledEmail);
  const [confirmationCode, setConfirmationCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [isSuccess, setIsSuccess] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setMessage('');
    setIsSuccess(false);

    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/confirm`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          username: email,
          confirmationCode,
        }),
      });

      const data = await response.json();

      if (response.ok) {
        setIsSuccess(true);
        setMessage('Account confirmed! Redirecting to sign in...');
        setTimeout(() => navigate('/login'), 1500);
      } else {
        setMessage(data.error || 'Confirmation failed');
      }
    } catch {
      setMessage('Network error. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container max-w-md">
      <h1 className="page-title text-center">Confirm Account</h1>
      <p className="page-subtitle text-center">Enter the code sent to your email.</p>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="email" className="form-label">
            Email
          </label>
          <input
            id="email"
            type="email"
            name="email"
            placeholder="Email address"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoComplete="email"
            className="form-input"
          />
        </div>

        <div>
          <label htmlFor="confirmationCode" className="form-label">
            Confirmation code
          </label>
          <input
            id="confirmationCode"
            type="text"
            name="confirmationCode"
            placeholder="Confirmation code"
            value={confirmationCode}
            onChange={(e) => setConfirmationCode(e.target.value)}
            required
            autoComplete="one-time-code"
            className="form-input"
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="btn-apple w-full py-3.5 rounded-xl font-semibold disabled:opacity-50"
        >
          {loading ? 'Confirming...' : 'Confirm Account'}
        </button>
      </form>

      {message && (
        <p className={`mt-4 text-center ${isSuccess ? 'message-success' : 'message-error'}`}>
          {message}
        </p>
      )}

      <p className="mt-6 text-center text-sm text-neutral-400">
        <Link to="/login" className="link-accent">
          Back to sign in
        </Link>
      </p>
    </div>
  );
};

export default Confirm;
