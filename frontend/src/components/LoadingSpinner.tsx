/**
 * LoadingSpinner Component
 * Professional loading indicator using design system styles
 */

export function LoadingSpinner() {
  return (
    <div className="flex items-center justify-center min-h-[400px]">
      <div className="text-center space-y-4 fade-in">
        <div className="spinner mx-auto"></div>
        <p className="text-gray-600 font-medium">Carregando...</p>
      </div>
    </div>
  );
}
