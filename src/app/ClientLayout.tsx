"use client";

import { Inter } from "next/font/google";
import { Toaster } from "sonner";
import { Suspense, lazy } from "react";
import { ThemeProvider } from "@/providers/theme-provider";
import { QueryProvider } from "@/providers/QueryProvider";
import Sidebar from "@/components/layout/Sidebar";
import { usePathname } from "next/navigation";

const Header = lazy(() => import("@/components/layout/Header"));
const HeaderFallback = () => <div className="h-16 bg-background border-b"></div>;

const inter = Inter({ subsets: ["latin"] });

export default function ClientLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const showSidebar = ['/rack-management', '/racks', '/data-centers','/accounts','/roles', '/settings'].some(
    path => pathname === path || pathname.startsWith(`${path}/`)
  );

  return (
    <ThemeProvider attribute="class" defaultTheme="system" enableSystem disableTransitionOnChange>
      <QueryProvider>
        <div className="flex min-h-screen flex-col">
          <Suspense fallback={<HeaderFallback />}>
            <Header />
          </Suspense>
          <div className="flex flex-1">
            {showSidebar && <Sidebar />}
            <main className={`flex-1 p-4 ${!showSidebar ? 'w-full' : ''}`}>{children}</main>
          </div>
        </div>
        <Toaster position="top-center" richColors />
      </QueryProvider>
    </ThemeProvider>
  );
} 