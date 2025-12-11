import { noop } from "lodash-es";
import {
  createContext,
  Dispatch,
  SetStateAction,
  useContext,
  useState,
  type ReactNode,
} from "react";
import { user } from "./types/user";

interface UsersPageContextType {
  users: user[];
  setUsers: Dispatch<SetStateAction<user[]>>;
}

const defaultContext: UsersPageContextType = {
  users: [],
  setUsers: noop
};

const usersPageContext =
  createContext<UsersPageContextType>(defaultContext);

export function useUsersPageContext() {
  return useContext(usersPageContext);
}

export function UsersPageContextProvider({
  children,
}: {
  children: ReactNode;
}) {
  const [users, setUsers] = useState<user[]>(defaultContext.users);

  return (
    <usersPageContext.Provider
      value={{
        users,
        setUsers,
      }}
    >
      {children}
    </usersPageContext.Provider>
  );
}
