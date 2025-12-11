import { createRoute } from "@tanstack/react-router";
import { rootRoute } from "../__root";
import DashboardLayout from "../../pages/dashboard/main/DashboardLayout";
import DashboardPage from "../../pages/dashboard/main/DashboardPage";
import BranchesPage from "../../pages/dashboard/branches/BranchesPage";
import BuildPageLayout from "../../pages/dashboard/build/BuildPageLayout";
import BrandingLayout from "../../pages/dashboard/branding/BrandingLayout";
import { UsersPage } from "../../pages/dashboard/users/UsersPage";
import { SetupPageLayout } from "../../pages/dashboard/setup/SetupPageLayout";

const basePath = import.meta.env.VITE_BASE_URL || "/";

export const dashboardRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: basePath + "dashboard",
  component: DashboardLayout,
});

export const dashboardIndex = createRoute({
  getParentRoute: () => dashboardRoute,
  path: "/",
  component: DashboardPage,
});

export const dashboardBranches = createRoute({
  getParentRoute: () => dashboardRoute,
  path: "branches",
  component: BranchesPage,
});

export const dashboardBuild = createRoute({
  getParentRoute: () => dashboardRoute,
  path: "build",
  component: BuildPageLayout,
});

export const dashboardBranding = createRoute({
  getParentRoute: () => dashboardRoute,
  path: "branding",
  component: BrandingLayout,
});

export const dashboardUsers = createRoute({
  getParentRoute: () => dashboardRoute,
  path: "users",
  component: UsersPage,
});

export const dashboardSetup = createRoute({
  getParentRoute: () => dashboardRoute,
  path: "setup",
  component: SetupPageLayout,
});

export const dashboardChildren = [
  dashboardIndex,
  dashboardBranches,
  dashboardBuild,
  dashboardBranding,
  dashboardUsers,
  dashboardSetup
];
