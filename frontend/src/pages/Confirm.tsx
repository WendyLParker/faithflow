import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { API_BASE_URL } from '../config';

const Confirm = () => {
  const [formData, setFormData] = useState({
    username: '',
    confirmationCode: '',
  });
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
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

    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/confirm`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(formData),
      });

      const data = await response.json();

      if (response.ok) {
        setMessage('Account confirmed! Redirecting to login...');
        setTimeout(() => navigate('/login'), 1500);
      } else {
        setMessage(data.error || 'Confirmation failed');
      }
    } catch (error) {
      setMessage('Network error. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-md mx-auto mt-10 p-6 bg-white rounded-xl shadow-md">
      <h2 className="text-2xl font-bold mb-6 text-center">Confirm Your Account</h2>

      <form onSubmit={handleSubmit} className="space-y-4">
        <input
          type="text"
          name="username"
          placeholder="Email / Username"
          value={formData.username}
          onChange={handleChange}
          required
          className="w-full p-3 border rounded-lg"
        />

        <input
          type="text"
          name="confirmationCode"
          placeholder="Confirmation Code"
          value={formData.confirmationCode}
          onChange={handleChange}
          required
          className="w-full p-3 border rounded-lg"
        />

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-green-600 text-white py-3 rounded-lg hover:bg-green-700 disabled:opacity-50"
        >
          {loading ? 'Confirming...' : 'Confirm Account'}
        </button>
      </form>

      {message && <p className="mt-4 text-center text-green-600">{message}</p>}
    </div>
  );
};

export default Confirm;