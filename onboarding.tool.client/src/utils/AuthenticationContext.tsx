import {
  createContext,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from "react";
import { noop } from "lodash-es";
import { Api } from "./api";
import { env } from "./env";
import type { AuthenticationState } from "./types/authentication/AuthenticationState";

interface AuthenticationContextType {
  authenticated: AuthenticationState | undefined;
  setAuthenticated: (auth: AuthenticationState | undefined) => void;
}

const defaultContext: AuthenticationContextType = {
  authenticated: undefined,
  setAuthenticated: noop,
};

const authenticationContext =
  createContext<AuthenticationContextType>(defaultContext);

export function useAuthenticationContext() {
  return useContext(authenticationContext);
}

export function AuthenticationContextProvider({
  children,
}: {
  children: ReactNode;
}) {
  const [authenticated, setAuthenticated] = useState<
    AuthenticationState | undefined
  >(undefined);

  useEffect(() => {
    const checkAuth = async () => {
      try {
        const api = new Api(env.apiBaseUrl);

        const authentication: AuthenticationState =
          await api.get<AuthenticationState>("/auth/me", undefined, {
            credentials: "include",
          });
        setAuthenticated(authentication);
      } catch (error) {
        console.error("Unable to check auth.", error);
        setAuthenticated({
          authenticated: false,
          name: "",
          claims: [],
        });
      }
    };

    checkAuth();
  }, []);

  return (
    <authenticationContext.Provider
      value={{
        authenticated,
        setAuthenticated,
      }}
    >
      {children}
    </authenticationContext.Provider>
  );
}
