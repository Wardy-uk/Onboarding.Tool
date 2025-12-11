import { Box, Typography } from "@mui/material";
import { BuildPortalAccounts } from "../../../components/dashboard/build/BuildPortalAccounts";
import { BuildDistricts } from "../../../components/dashboard/build/BuildDistricts";
import { useBuildPageContext } from "./BuildContextProvider";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { buildOverview } from "./types/buildOverview";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { useEffect } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";

export const BuildPage = () => {
  const { setLoading, selectedInstance } = useInstanceContext();
  const { setPortalAccounts, setBranches, setSelectedBranch } = useBuildPageContext();
  const queryClient = useQueryClient();

  const fetchBuildOverview = async (
    signal?: AbortSignal
  ): Promise<buildOverview> => {
    const api = new Api(env.apiBaseUrl);
    const endpoint = `/api/v1/dashboard/instance/${
      selectedInstance!.host
    }/info/build`;

    const response: buildOverview = await api.get<buildOverview>(endpoint, undefined, {
      credentials: "include",
      signal,
    });
    return response;
  };

  const { data: overview, isFetching } = useQuery<buildOverview>({
    queryKey: ["build"],
    queryFn: ({ signal }) => fetchBuildOverview(signal),
    staleTime: Infinity,
    enabled: selectedInstance != null,
  });

  useEffect(() => {
    if (selectedInstance) {
      setSelectedBranch(null);
      setLoading(true);
      queryClient.invalidateQueries({ queryKey: ["build"] });
    }
  }, [selectedInstance, setLoading, setSelectedBranch, queryClient]);

  useEffect(() => {
    if (selectedInstance && overview) {
      setBranches(overview.branches);
      setPortalAccounts(overview.portalAccounts);
      setLoading(false);
    }
  }, [setBranches, setPortalAccounts, selectedInstance, overview]);

  return (
    <Box sx={{ padding: 2, overflowY: "auto" }}>
      <Typography variant="h5" fontWeight={600}>
        Build
      </Typography>
      <BuildPortalAccounts fetching={isFetching} />
      <BuildDistricts fetching={isFetching} />
    </Box>
  );
};

export default BuildPage;
