import { noop } from "lodash-es";
import { createContext, ReactNode, useContext, useEffect, useState } from "react";
import { instance } from "./types/instance";
import { Api } from "./api";
import { env } from "./env";

interface InstanceContext {
  selectedInstance: instance | null;
  setSelectedInstance: (instance: instance | null) => void;
  validInstances: instance[];
  setValidInstances: (instances: instance[]) => void;
  instanceSelectionError: string | null;
  setInstanceSelectionError: (error: string | null) => void;
  loading: boolean;
  setLoading: (loading: boolean) => void;
}

const defaultContext: InstanceContext = {
  selectedInstance: null,
  setSelectedInstance: noop,
  validInstances: [],
  setValidInstances: noop,
  instanceSelectionError: null,
  setInstanceSelectionError: noop,
  loading: false,
  setLoading: noop
};

const instanceContext = createContext<InstanceContext>(defaultContext);

export function useInstanceContext() {
  return useContext(instanceContext);
}

export function InstanceContextProvider({ children }: { children: ReactNode }) {
  const [selectedInstance, setSelectedInstance] = useState<instance | null>(defaultContext.selectedInstance);
  const [validInstances, setValidInstances] = useState<instance[]>([]);
  const [instanceSelectionError, setInstanceSelectionError] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(false);

  useEffect(() => {
    const fetchInstances = async () => {
      const api = new Api(env.apiBaseUrl);

      const instances: instance[] = await api.get<instance[]>("/api/v1/dashboard/instances", undefined, { credentials: "include" });
      setValidInstances(instances);
    }

    fetchInstances();
  }, []);

  return (
    <instanceContext.Provider
      value={{
        selectedInstance,
        setSelectedInstance,
        validInstances,
        setValidInstances,
        instanceSelectionError,
        setInstanceSelectionError,
        loading,
        setLoading
      }}
    >
      {children}
    </instanceContext.Provider>
  );
}
