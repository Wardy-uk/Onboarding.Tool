import { createRoute } from "@tanstack/react-router";
import { rootRoute } from "../__root";
import IndexPage from "../../pages/index/IndexPage";

export const indexRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: import.meta.env.VITE_BASE_URL || "/",
  component: () => <IndexPage />,
});