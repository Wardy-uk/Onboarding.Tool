import { Box, Divider, List, Paper, Skeleton, Typography } from "@mui/material";
import { BranchesOptions } from "./BranchesOptions";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useBranchesPageContext } from "../../../pages/dashboard/branches/BranchesContextProvider";
import { branch } from "../../../pages/dashboard/branches/types/branch";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { useEffect } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { BranchListItem } from "./BranchListItem";

export const BranchesContainer = () => {
  const { loading, selectedInstance, setLoading } = useInstanceContext();
  const { branches, setBranches } = useBranchesPageContext();
  const queryClient = useQueryClient();

  const fetchBranches = async (signal?: AbortSignal): Promise<branch[]> => {
    const api = new Api(env.apiBaseUrl);
    const endpoint = `/api/v1/dashboard/instance/${
      selectedInstance!.host
    }/info/branches`;

    const response: branch[] = await api.get<branch[]>(endpoint, undefined, {
      credentials: "include",
      signal,
    });
    return response;
  };

  const { data: apiBranches, isFetching } = useQuery<branch[]>({
    queryKey: ["branches"],
    queryFn: ({ signal }) => fetchBranches(signal),
    staleTime: Infinity,
    enabled: selectedInstance != null,
  });

  useEffect(() => {
    if (selectedInstance) {
      setLoading(true);
      queryClient.invalidateQueries({ queryKey: ["branches"] });
    }
  }, [selectedInstance, setLoading, queryClient]);

  useEffect(() => {
    if (selectedInstance && apiBranches) {
      setBranches(apiBranches);
      setLoading(false);
    }
  }, [setBranches, selectedInstance, apiBranches]);

  return (
    <Box sx={{ my: 2 }}>
      {loading || isFetching || selectedInstance == null ? (
        <Skeleton sx={{ p: 3, borderRadius: 2, mb: 2 }} />
      ) : (
        <Paper variant="outlined" sx={{ p: 2 }}>
          {branches.length === 0 ? (
            <Typography variant="body1">
              There are no branches to show.
            </Typography>
          ) : (
            <List>
              {branches.map((b) => (
                <Typography key={b.id} variant="body2">
                  <BranchListItem branch={b} />
                  <Divider component="hr" />
                </Typography>
              ))}
            </List>
          )}
          <BranchesOptions />
        </Paper>
      )}
    </Box>
  );
};
