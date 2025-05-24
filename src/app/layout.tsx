import "@/app/globals.css";
import { Inter } from "next/font/google";
import { Metadata } from "next";
import ClientLayout from "./ClientLayout";

export const metadata: Metadata = {
  title: {
    template: '%s | Rack Management System',
    default: 'Rack Management System',
  },
  description: 'Rack Management System for data centers',
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body>
        <ClientLayout>{children}</ClientLayout>
      </body>
    </html>
  );
} 