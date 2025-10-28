/**
 * Layout Component - Modern Navigation and Footer
 * Professional design following GUIA_REDESIGN_LAYOUT_PROFISSIONAL.md
 */

import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Building2, Home, Search, Clock, BarChart, Menu, X, BookOpen } from 'lucide-react';

const navigation = [
  {
    name: 'Dashboard',
    href: '/dashboard',
    icon: <Home className="w-5 h-5" />,
  },
  {
    name: 'Pesquisa',
    href: '/claims/search',
    icon: <Search className="w-5 h-5" />,
  },
  {
    name: 'Histórico',
    href: '/claims/history',
    icon: <Clock className="w-5 h-5" />,
  },
  {
    name: 'Relatórios',
    href: '/reports',
    icon: <BarChart className="w-5 h-5" />,
  },
  {
    name: 'Documentação',
    href: '/docs',
    icon: <BookOpen className="w-5 h-5" />,
  },
];

interface LayoutProps {
  children: React.ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  const location = useLocation();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const isActive = (path: string) => location.pathname === path;

  return (
    <div className="min-h-screen flex flex-col">
      {/* Header / Navbar */}
      <header className="bg-gradient-caixa text-white shadow-lg sticky top-0 z-50 no-print">
        <div className="container-modern">
          <div className="flex items-center justify-between h-16">
            {/* Logo and Brand */}
            <Link to="/" className="flex items-center gap-3 hover:opacity-90 transition-opacity">
              <Building2 className="w-8 h-8" />
              <div className="hidden sm:block">
                <div className="text-white font-bold text-lg">
                  Caixa Seguradora
                </div>
                <div className="text-blue-100 text-xs">
                  Sistema de Sinistros - SIWEA
                </div>
              </div>
            </Link>

            {/* Desktop Navigation */}
            <nav className="hidden lg:flex items-center gap-1">
              {navigation.map((item) => (
                <Link
                  key={item.name}
                  to={item.href}
                  className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-all ${
                    isActive(item.href)
                      ? 'bg-white/20 text-white'
                      : 'text-blue-100 hover:bg-white/10 hover:text-white'
                  }`}
                >
                  {item.icon}
                  <span>{item.name}</span>
                </Link>
              ))}
            </nav>

            {/* Mobile Menu Button */}
            <button
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              className="lg:hidden p-2 rounded-lg hover:bg-white/10 transition-colors"
              aria-label="Toggle menu"
            >
              {mobileMenuOpen ? (
                <X className="w-6 h-6" />
              ) : (
                <Menu className="w-6 h-6" />
              )}
            </button>
          </div>

          {/* Mobile Navigation Dropdown */}
          {mobileMenuOpen && (
            <div className="lg:hidden py-4 border-t border-white/20 fade-in">
              <nav className="flex flex-col gap-2">
                {navigation.map((item) => (
                  <Link
                    key={item.name}
                    to={item.href}
                    onClick={() => setMobileMenuOpen(false)}
                    className={`flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition-all ${
                      isActive(item.href)
                        ? 'bg-white/20 text-white'
                        : 'text-blue-100 hover:bg-white/10 hover:text-white'
                    }`}
                  >
                    {item.icon}
                    <span>{item.name}</span>
                  </Link>
                ))}
              </nav>
            </div>
          )}
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 py-8">
        {children}
      </main>

      {/* Footer */}
      <footer className="bg-white border-t border-gray-200 mt-auto no-print">
        <div className="container-modern py-8">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {/* Column 1: Company Info */}
            <div>
              <div className="flex items-center gap-2 mb-4">
                <Building2 className="w-6 h-6 text-gray-700" />
                <h3 className="text-lg font-bold text-gray-900">Caixa Seguradora</h3>
              </div>
              <p className="text-sm text-gray-600">
                Migração Visual Age para .NET 9.0
              </p>
              <p className="text-sm text-gray-600 mt-2">
                Sistema de Gestão de Sinistros (SIWEA)
              </p>
            </div>

            {/* Column 2: Quick Links */}
            <div>
              <h4 className="text-sm font-bold text-gray-900 mb-4 uppercase tracking-wide">
                Links Rápidos
              </h4>
              <ul className="space-y-2">
                {navigation.map((item) => (
                  <li key={item.name}>
                    <Link
                      to={item.href}
                      className="text-sm text-gray-600 hover:text-gray-900 transition-colors"
                    >
                      {item.name}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>

            {/* Column 3: System Info */}
            <div>
              <h4 className="text-sm font-bold text-gray-900 mb-4 uppercase tracking-wide">
                Informações do Sistema
              </h4>
              <ul className="space-y-2 text-sm text-gray-600">
                <li>Versão: 1.0.0</li>
                <li>Build: .NET 9.0</li>
                <li>Frontend: React 19 + TypeScript</li>
                <li>Última atualização: {new Date().toLocaleDateString('pt-BR')}</li>
              </ul>
            </div>
          </div>

          <div className="divider"></div>

          {/* Bottom Bar */}
          <div className="flex flex-col sm:flex-row items-center justify-between gap-4 text-sm text-gray-600">
            <p>
              © {new Date().getFullYear()} Caixa Seguradora. Todos os direitos reservados.
            </p>
            <p className="text-xs">
              Desenvolvido com React 19 e .NET 9.0
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
}
