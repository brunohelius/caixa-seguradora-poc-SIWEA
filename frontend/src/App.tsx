/**
 * Main App Component
 * Updated for T042-T047 - Claim Search and Detail routes
 * T143: Lazy loading for performance optimization
 * Updated to use modern Layout component
 */

import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { lazy, Suspense } from 'react';
import { ClaimSearchPage } from './pages/ClaimSearchPage';
import Layout from './components/Layout';
import { Toaster } from './components/ui/sonner';

// T143: Lazy load large components for better initial load performance
const ClaimDetailPage = lazy(() => import('./pages/ClaimDetailPage').then(module => ({ default: module.ClaimDetailPage })));
const MigrationDashboardPage = lazy(() => import('./pages/MigrationDashboardPage'));
const ClaimHistoryPage = lazy(() => import('./pages/ClaimHistoryPage').then(module => ({ default: module.ClaimHistoryPage })));
const ReportsPage = lazy(() => import('./pages/ReportsPage').then(module => ({ default: module.ReportsPage })));
const LegacySystemDocsPage = lazy(() => import('./pages/LegacySystemDocsPage').then(module => ({ default: module.LegacySystemDocsPage })));

function LoadingFallback() {
  return (
    <div className="flex items-center justify-center min-h-[400px]">
      <div className="text-center space-y-4 fade-in">
        <div className="spinner mx-auto"></div>
        <p className="text-gray-600 font-medium">Carregando...</p>
      </div>
    </div>
  );
}

function App() {
  return (
    <Router>
      <Layout>
        {/* T143: Suspense boundary for lazy-loaded components */}
        <Suspense fallback={<LoadingFallback />}>
          <Routes>
            <Route path="/" element={<Navigate to="/claims/search" replace />} />
            <Route path="/claims/search" element={<ClaimSearchPage />} />
            <Route path="/claims/detail" element={<ClaimDetailPage />} />
            <Route path="/claims/history" element={<ClaimHistoryPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/dashboard" element={<MigrationDashboardPage />} />
            <Route path="/docs" element={<LegacySystemDocsPage />} />
          </Routes>
        </Suspense>
      </Layout>
      <Toaster richColors position="top-right" />
    </Router>
  );
}

export default App;
