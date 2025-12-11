import { createRoute } from "@tanstack/react-router";
import { rootRoute } from "../__root";
import EnterpriseLayout from "../../pages/enterprise/main/EnterpriseLayout";
import { EnterprisePage } from "../../pages/enterprise/main/EnterprisePage";

const basePath = import.meta.env.VITE_BASE_URL || "/";

export const enterpriseRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: basePath + "enterprise",
  component: EnterpriseLayout,
});

export const enterpriseIndex = createRoute({
  getParentRoute: () => enterpriseRoute,
  path: "/",
  component: EnterprisePage,
});

export const enterpriseChildren = [enterpriseIndex];
