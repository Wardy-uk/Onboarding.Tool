import { Box, Paper, Skeleton, Typography } from "@mui/material";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useDashboardPageContext } from "../../../pages/dashboard/main/DashboardContextProvider";
import { InstanceOverviewStatistic } from "./InstanceOverviewStatistic";
import { useEffect } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { dashboardStatistics } from "../../../pages/dashboard/main/types/dashboardStatistics";

export const InstanceOverview = () => {
  const { loading, setLoading, selectedInstance } = useInstanceContext();
  const { statistics, setStatistics } = useDashboardPageContext();
  const queryClient = useQueryClient();

  const fetchOverview = async (
    signal?: AbortSignal
  ): Promise<dashboardStatistics> => {
    const api = new Api(env.apiBaseUrl);
    const endpoint = `/api/v1/dashboard/instance/${
      selectedInstance!.host
    }/info/overview`;

    const response: dashboardStatistics = await api.get<dashboardStatistics>(
      endpoint,
      undefined,
      { credentials: "include", signal }
    );
    return response;
  };

  const { data: stats, isFetching } = useQuery<dashboardStatistics>({
    queryKey: ["statistics"],
    queryFn: ({ signal }) => fetchOverview(signal),
    staleTime: Infinity,
    enabled: selectedInstance != null,
  });

  useEffect(() => {
    if (selectedInstance) {
      setLoading(true);
      queryClient.invalidateQueries({ queryKey: ["statistics"] });
    }
  }, [selectedInstance, setLoading, queryClient]);

  useEffect(() => {
    if (selectedInstance && stats) {
      setStatistics(stats);
      setLoading(false);
    }
  }, [setStatistics, selectedInstance, stats]);

  return (
    <Box sx={{ my: 2 }}>
      {loading || isFetching || selectedInstance == null ? (
        <Skeleton sx={{ p: 3, borderRadius: 2, mb: 2 }} />
      ) : (
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Typography variant="h5" fontWeight={500} sx={{ mb: 2 }}>
            Progress
          </Typography>
          <InstanceOverviewStatistic
            success={statistics.branches > 0}
            text={
              statistics.branches > 0
                ? `${statistics.branches} branches have been setup`
                : "No branches have been setup, or none are marked as default."
            }
          />
          <InstanceOverviewStatistic
            success={statistics.users > 0}
            text={`${
              statistics.users > 0 ? statistics.users : "No"
            } users have been setup`}
          />
          <InstanceOverviewStatistic
            success={statistics.requiredBrandSettings}
            text={
              statistics.requiredBrandSettings
                ? "All required brand settings have been provided"
                : "There are outstanding required brand settings to configure"
            }
          />
          <InstanceOverviewStatistic
            success={statistics.requiredImages}
            text={
              statistics.requiredImages
                ? "All required images have been provided"
                : "There are outstanding required images to upload"
            }
          />
        </Paper>
      )}
    </Box>
  );
};
