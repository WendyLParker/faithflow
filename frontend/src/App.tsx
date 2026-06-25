import { useState } from 'react'
import { Plus, Home, List, Bot, Flame } from 'lucide-react'
import TestPrayer from './components/TestPrayer'

function App() {
  const [currentTab, setCurrentTab] = useState<'home' | 'prayers' | 'add' | 'ai'>('home')

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-gradient-to-r from-indigo-600 to-violet-600 text-white p-5 shadow-md">
        <div className="flex items-center justify-center gap-3">
          <Flame className="w-8 h-8" />
          <h1 className="text-3xl font-bold tracking-tight">FaithFlow</h1>
        </div>
        <p className="text-center text-indigo-100 mt-1 text-sm">Grow closer to God, one prayer at a time</p>
      </header>

      {/* Main Content */}
      <main className="pb-24 min-h-[calc(100vh-180px)]">
        {currentTab === 'home' && (
          <div className="p-6 space-y-6">
            <div className="bg-white rounded-3xl shadow p-8 text-center">
              <div className="flex justify-center mb-4">
                <div className="bg-orange-100 text-orange-600 w-20 h-20 rounded-2xl flex items-center justify-center">
                  <Flame className="w-12 h-12" />
                </div>
              </div>
              <p className="text-6xl font-bold text-gray-800">7</p>
              <p className="text-xl text-gray-600 mt-1">Day Streak</p>
              <p className="text-sm text-emerald-600 mt-4">Keep going. God is with you.</p>
            </div>

            <div className="bg-white rounded-3xl p-6 shadow">
              <p className="text-gray-500 text-sm">Today's Verse</p>
              <p className="mt-3 text-gray-800 italic">"Be still, and know that I am God."</p>
              <p className="text-xs text-gray-500 mt-2">— Psalm 46:10</p>
            </div>
          </div>
        )}

        {currentTab === 'prayers' && (
          <div className="p-6 text-center text-gray-500 pt-20">
            Your Prayer History<br />Coming in next step
          </div>
        )}

        {currentTab === 'add' && (
          <div className="p-6 text-center text-gray-500 pt-20">
            <TestPrayer/>
          </div>
        )}

        {currentTab === 'ai' && (
          <div className="p-6 text-center text-gray-500 pt-20">
            AI Prayer Helper<br />Coming soon
          </div>
        )}
      </main>

      {/* Bottom Navigation */}
      <nav className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 shadow">
        <div className="max-w-md mx-auto flex justify-around items-center py-1">
          <button onClick={() => setCurrentTab('home')} className={`flex flex-col items-center py-3 px-4 ${currentTab === 'home' ? 'text-indigo-600' : 'text-gray-500'}`}>
            <Home size={26} />
            <span className="text-[10px] mt-1">Home</span>
          </button>

          <button onClick={() => setCurrentTab('prayers')} className={`flex flex-col items-center py-3 px-4 ${currentTab === 'prayers' ? 'text-indigo-600' : 'text-gray-500'}`}>
            <List size={26} />
            <span className="text-[10px] mt-1">Prayers</span>
          </button>

          <button 
            onClick={() => setCurrentTab('add')}
            className="flex flex-col items-center -mt-8 bg-indigo-600 text-white p-5 rounded-3xl shadow-2xl active:scale-95 transition"
          >
            <Plus size={36} strokeWidth={3} />
          </button>

          <button onClick={() => setCurrentTab('ai')} className={`flex flex-col items-center py-3 px-4 ${currentTab === 'ai' ? 'text-indigo-600' : 'text-gray-500'}`}>
            <Bot size={26} />
            <span className="text-[10px] mt-1">AI</span>
          </button>
        </div>
      </nav>
    </div>
  )
}

export default App