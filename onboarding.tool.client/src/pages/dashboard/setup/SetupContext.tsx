import { noop } from "lodash-es";
import { createContext, Dispatch, ReactNode, SetStateAction, useContext, useState } from "react";
import { steps } from "./types/steps";
import { templateState } from "./types/setupTemplateSteps";

interface SetupContextType {
  setupEligible: null | boolean;
  setSetupEligible: (status: null | boolean) => void;
  setupSteps: steps;
  setSetupSteps: Dispatch<SetStateAction<steps>>;
  setupConsoleLogs: string[];
  setSetupConsoleLogs: Dispatch<SetStateAction<string[]>>;
}

const defaultContext: SetupContextType = {
  setupEligible: null,
  setSetupEligible: noop,
  setupSteps: {
    templateSteps: {
      templatesConfirmed: templateState.incomplete,
      letterheadConfirmed: templateState.incomplete,
      directMailConfirmed: templateState.incomplete
    },
    additionalSteps: []
  },
  setSetupSteps: noop,
  setupConsoleLogs: [],
  setSetupConsoleLogs: noop
};

const setupPageContext = createContext<SetupContextType>(defaultContext);

export function useSetupPageContext() {
  return useContext(setupPageContext);
}

export function SetupPageContextProvider({
  children,
}: {
  children: ReactNode;
}) {
  const [setupEligible, setSetupEligible] = useState<null | boolean>(defaultContext.setupEligible);
  const [setupSteps, setSetupSteps] = useState<steps>(defaultContext.setupSteps);
  const [setupConsoleLogs, setSetupConsoleLogs] = useState<string[]>(defaultContext.setupConsoleLogs);

  return (
    <setupPageContext.Provider
      value={{
        setupEligible,
        setSetupEligible,
        setupSteps,
        setSetupSteps,
        setupConsoleLogs,
        setSetupConsoleLogs
      }}
    >
      {children}
    </setupPageContext.Provider>
  );
}
