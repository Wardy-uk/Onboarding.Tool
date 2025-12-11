import { type ReactNode } from "react";
import DashboardIcon from "@mui/icons-material/Dashboard";
import SettingsIcon from '@mui/icons-material/Settings';
import { Brush, Business, Home, Person } from "@mui/icons-material";

export type NavItem = {
  label: string;
  to: string;
  icon: ReactNode;
};

const basePath: string = import.meta.env.VITE_BASE_URL || "/";

export const navItems: NavItem[] = [
  { label: "Overview", to: basePath + "dashboard", icon: <DashboardIcon /> },
  { label: "Branches", to: basePath + "dashboard/branches", icon: <Business /> },
  { label: "Build", to: basePath + "dashboard/build", icon: <Home /> },
  { label: "Branding", to: basePath + "dashboard/branding", icon: <Brush /> },
  { label: "Users", to: basePath + "/dashboard/users", icon: <Person /> },
  { label: "Setup", to: basePath + "/dashboard/setup", icon: <SettingsIcon /> }
];

