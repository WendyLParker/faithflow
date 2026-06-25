import { useState, useEffect } from 'react';
import { prayerService, type PrayerResponseDto, type PrayerCreateDto } from '@/services/prayerService';

export default function TestPrayer() {
  const [prayers, setPrayers] = useState<PrayerResponseDto[]>([]);
  const [loading, setLoading] = useState(false);

  const loadPrayers = async () => {
    setLoading(true);
    try {
      const data = await prayerService.getAll();
      setPrayers(data);
    } catch (err) {
      console.error(err);
      alert("Failed to load prayers");
    } finally {
      setLoading(false);
    }
  };

  const createPrayer = async () => {
    const newPrayer: PrayerCreateDto = {
      title: "Test Prayer " + new Date().toLocaleTimeString(),
      content: "This is a test from the frontend",
      categories: ["Testing"]
    };

    try {
      await prayerService.create(newPrayer);
      loadPrayers();
    } catch (err) {
      alert("Failed to create prayer");
    }
  };

  const markAnswered = async (id: number) => {
    try {
      await prayerService.markAsAnswered(id);
      loadPrayers();
    } catch (err) {
      alert("Failed to mark as answered");
    }
  };

  const deletePrayer = async (id: number) => {
    if (!confirm("Delete this prayer?")) return;
    try {
      await prayerService.delete(id);
      loadPrayers();
    } catch (err) {
      alert("Failed to delete prayer");
    }
  };

  useEffect(() => {
    loadPrayers();
  }, []);

  return (
    <div className="p-6 max-w-2xl mx-auto">
      <h1 className="text-3xl font-bold mb-6">FaithFlow - Backend Test</h1>

      <div className="mb-6 space-x-3">
        <button onClick={createPrayer} className="bg-indigo-600 text-white px-5 py-3 rounded-lg hover:bg-indigo-700">
          + Create Test Prayer
        </button>
        <button onClick={loadPrayers} className="bg-gray-600 text-white px-5 py-3 rounded-lg hover:bg-gray-700">
          Refresh
        </button>
      </div>

      <div className="space-y-4">
        {prayers.map(prayer => (
          <div key={prayer.id} className="border rounded-xl p-5 bg-white shadow-sm">
            <div className="flex justify-between items-start">
              <div>
                <h3 className="font-semibold text-lg">{prayer.title}</h3>
                {prayer.content && <p className="text-gray-600 mt-1">{prayer.content}</p>}
              </div>
              {prayer.isAnswered && (
                <span className="text-green-600 text-sm font-medium">✓ Answered</span>
              )}
            </div>

            <div className="mt-4 flex gap-3">
              {!prayer.isAnswered && (
                <button
                  onClick={() => markAnswered(prayer.id)}
                  className="text-sm bg-green-100 text-green-700 px-4 py-2 rounded-lg hover:bg-green-200"
                >
                  Mark as Answered
                </button>
              )}
              <button
                onClick={() => deletePrayer(prayer.id)}
                className="text-sm bg-red-100 text-red-700 px-4 py-2 rounded-lg hover:bg-red-200"
              >
                Delete
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}