import { noop } from "lodash-es";
import {
  createContext,
  Dispatch,
  SetStateAction,
  useContext,
  useState,
  type ReactNode,
} from "react";
import { branch } from "./types/branch";

interface BranchesPageContextType {
  branches: branch[];
  setBranches: Dispatch<SetStateAction<branch[]>>;
}

const defaultContext: BranchesPageContextType = {
  branches: [],
  setBranches: noop
};

const branchesPageContext =
  createContext<BranchesPageContextType>(defaultContext);

export function useBranchesPageContext() {
  return useContext(branchesPageContext);
}

export function BranchesPageContextProvider({
  children,
}: {
  children: ReactNode;
}) {
  const [branches, setBranches] = useState<branch[]>(defaultContext.branches);

  return (
    <branchesPageContext.Provider
      value={{
        branches,
        setBranches,
      }}
    >
      {children}
    </branchesPageContext.Provider>
  );
}
