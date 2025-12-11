import { noop } from "lodash-es";
import {
  createContext,
  Dispatch,
  SetStateAction,
  useContext,
  useState,
  type ReactNode,
} from "react";
import { buildBranch } from "./types/buildBranch";
import { buildPortalAccount } from "./types/buildPortalAccount";

interface BuildPageContextType {
  branches: buildBranch[];
  setBranches: Dispatch<SetStateAction<buildBranch[]>>;
  portalAccounts: buildPortalAccount[];
  setPortalAccounts: Dispatch<SetStateAction<buildPortalAccount[]>>;
  selectedBranch: buildBranch | null;
  setSelectedBranch: (branch: buildBranch | null) => void;
}

const defaultContext: BuildPageContextType = {
  branches: [],
  setBranches: noop,
  portalAccounts: [],
  setPortalAccounts: noop,
  selectedBranch: null,
  setSelectedBranch: noop
};

const buildPageContext =
  createContext<BuildPageContextType>(defaultContext);

export function useBuildPageContext() {
  return useContext(buildPageContext);
}

export function BuildPageContextProvider({
  children,
}: {
  children: ReactNode;
}) {
  const [branches, setBranches] = useState<buildBranch[]>(defaultContext.branches);
  const [portalAccounts, setPortalAccounts] = useState<buildPortalAccount[]>(defaultContext.portalAccounts);
  const [selectedBranch, setSelectedBranch] = useState<buildBranch | null>(defaultContext.selectedBranch);

  return (
    <buildPageContext.Provider
      value={{
        branches,
        setBranches,
        portalAccounts,
        setPortalAccounts,
        selectedBranch,
        setSelectedBranch
      }}
    >
      {children}
    </buildPageContext.Provider>
  );
}
