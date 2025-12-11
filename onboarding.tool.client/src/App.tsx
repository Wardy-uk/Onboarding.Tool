import { CssBaseline, ThemeProvider } from "@mui/material";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { createRouter, RouterProvider } from "@tanstack/react-router";
import { rootRoute } from "./routes/__root.ts";
import { indexRoute } from "./routes/index/index.tsx";
import {
  dashboardChildren,
  dashboardRoute,
} from "./routes/dashboard/dashboard.tsx";
import { AuthenticationContextProvider } from "./utils/AuthenticationContext.tsx";
import theme from "./theme.ts";
import { SnackbarProvider } from "./utils/SnackbarContext.tsx";
import { enterpriseChildren, enterpriseRoute } from "./routes/enterprise/enterprise.tsx";

const queryClient = new QueryClient();

export const dashboardRouteWithChildren =
  dashboardRoute.addChildren(dashboardChildren);
export const enterpriseRouteWithChildren =
  enterpriseRoute.addChildren(enterpriseChildren);
export const routeTree = rootRoute.addChildren([
  indexRoute,
  dashboardRouteWithChildren,
  enterpriseRouteWithChildren
]);

const router = createRouter({
  routeTree: routeTree,
  basepath: import.meta.env.VITE_BASE_URL || "/",
});

declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
  }
}

const App = () => {
  return (
    <SnackbarProvider>
      <QueryClientProvider client={queryClient}>
        <CssBaseline />
        <AuthenticationContextProvider>
          <ThemeProvider theme={theme}>
            <RouterProvider router={router} />
          </ThemeProvider>
        </AuthenticationContextProvider>
      </QueryClientProvider>
    </SnackbarProvider>
  );
};

export default App;
