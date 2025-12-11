import { noop } from "lodash-es";
import {
  createContext,
  useContext,
  useState,
  type ReactNode,
} from "react";
import { dashboardStatistics } from "./types/dashboardStatistics";

interface DashboardPageContextType {
  statistics: dashboardStatistics;
  setStatistics: (statistics: dashboardStatistics) => void;
}

const defaultContext: DashboardPageContextType = {
  statistics: {
    branches: 0,
    users: 0,
    requiredBrandSettings: false,
    requiredImages: false
  },
  setStatistics: noop,
};

const dashboardPageContext =
  createContext<DashboardPageContextType>(defaultContext);

export function useDashboardPageContext() {
  return useContext(dashboardPageContext);
}

export function DashboardPageContextProvider({
  children,
}: {
  children: ReactNode;
}) {
  const [statistics, setStatistics] = useState<dashboardStatistics>(defaultContext.statistics);

  return (
    <dashboardPageContext.Provider
      value={{
        statistics,
        setStatistics,
      }}
    >
      {children}
    </dashboardPageContext.Provider>
  );
}
