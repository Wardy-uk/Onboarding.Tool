import { Paper, Skeleton } from "@mui/material";
import { UserOptions } from "./UserOptions";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { UserList } from "../../../components/dashboard/users/UserList";
import { useUsersPageContext } from "../../../pages/dashboard/users/UsersContextProvider";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { user } from "../../../pages/dashboard/users/types/user";
import { env } from "../../../utils/env";
import { Api } from "../../../utils/api";
import { useEffect } from "react";

export const UserManager = () => {
  const { loading, setLoading, selectedInstance } = useInstanceContext();
  const { setUsers } = useUsersPageContext();
  const queryClient = useQueryClient();

  const fetchUsers = async (signal?: AbortSignal): Promise<user[]> => {
    const api = new Api(env.apiBaseUrl);
    const endpoint = `/api/v1/dashboard/instance/${
      selectedInstance!.host
    }/info/users`;

    const response: user[] = await api.get<user[]>(endpoint, undefined, {
      credentials: "include",
      signal,
    });
    return response;
  };

  const { data: apiUsers, isFetching } = useQuery<user[]>({
    queryKey: ["users"],
    queryFn: ({ signal }) => fetchUsers(signal),
    staleTime: Infinity,
    enabled: selectedInstance != null,
  });

  useEffect(() => {
    if (selectedInstance) {
      setLoading(true);
      queryClient.invalidateQueries({ queryKey: ["users"] });
    }
  }, [selectedInstance, setLoading, queryClient]);

  useEffect(() => {
    if (selectedInstance && apiUsers) {
      setUsers(apiUsers);
      setLoading(false);
    }
  }, [setUsers, selectedInstance, apiUsers]);

  return (
    <Paper sx={{ mt: 2, p: 2 }}>
      {loading || isFetching || selectedInstance == null ? (
        <>
          <Skeleton />
          <Skeleton />
        </>
      ) : (
        <>
          <UserList  />
          <UserOptions />
        </>
      )}
    </Paper>
  );
};
