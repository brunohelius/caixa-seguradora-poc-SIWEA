/**
 * Main App Component
 * Updated for T042-T047 - Claim Search and Detail routes
 */

import { BrowserRouter as Router, Routes, Route, Navigate, Link } from 'react-router-dom';
import { ClaimSearchPage } from './pages/ClaimSearchPage';
import { ClaimDetailPage } from './pages/ClaimDetailPage';
import MigrationDashboardPage from './pages/MigrationDashboardPage';

function App() {
  return (
    <Router>
      <div className="App min-h-screen bg-background">
        <nav className="border-b bg-card">
          <div className="container mx-auto px-4 py-3 flex items-center justify-between max-w-[1200px]">
            <Link to="/" className="text-lg font-semibold text-foreground">
              Caixa Seguradora - Sistema de Sinistros
            </Link>
            <div className="flex gap-4">
              <Link
                to="/claims/search"
                className="text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
              >
                Pesquisa
              </Link>
              <Link
                to="/dashboard"
                className="text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
              >
                Dashboard
              </Link>
            </div>
          </div>
        </nav>

        <main className="container mx-auto px-4 py-6 max-w-[1200px]">
          <Routes>
            <Route path="/" element={<Navigate to="/claims/search" replace />} />
            <Route path="/claims/search" element={<ClaimSearchPage />} />
            <Route path="/claims/detail" element={<ClaimDetailPage />} />
            <Route path="/dashboard" element={<MigrationDashboardPage />} />
          </Routes>
        </main>
      </div>
    </Router>
  );
}

export default App;
