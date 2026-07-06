import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { API_BASE_URL } from '../config';

const Register = () => {
  const [formData, setFormData] = useState({
    email: '',
    password: '',
  });
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [isSuccess, setIsSuccess] = useState(false);
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setMessage('');
    setIsSuccess(false);

    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(formData),
      });

      const data = await response.json();

      if (response.ok) {
        setIsSuccess(true);
        setMessage('Registration successful! Check your email for the confirmation code.');
        setTimeout(() => navigate('/confirm', { state: { email: formData.email } }), 2000);
      } else {
        setMessage(data.error || 'Registration failed');
      }
    } catch {
      setMessage('Network error. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container max-w-md">
      <h1 className="page-title text-center">Create Account</h1>
      <p className="page-subtitle text-center">Register to submit and track requests.</p>

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
            value={formData.email}
            onChange={handleChange}
            required
            autoComplete="email"
            className="form-input"
          />
        </div>

        <div>
          <label htmlFor="password" className="form-label">
            Password
          </label>
          <input
            id="password"
            type="password"
            name="password"
            placeholder="Password"
            value={formData.password}
            onChange={handleChange}
            required
            autoComplete="new-password"
            className="form-input"
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="btn-apple w-full py-3.5 rounded-xl font-semibold disabled:opacity-50"
        >
          {loading ? 'Creating Account...' : 'Register'}
        </button>
      </form>

      {message && (
        <p className={`mt-4 text-center ${isSuccess ? 'message-success' : 'message-error'}`}>
          {message}
        </p>
      )}

      <p className="mt-6 text-center text-sm text-neutral-400">
        Already have an account?{' '}
        <Link to="/login" className="link-accent">
          Sign in
        </Link>
      </p>
    </div>
  );
};

export default Register;
