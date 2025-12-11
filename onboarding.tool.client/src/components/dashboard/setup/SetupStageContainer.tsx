import { Paper, Skeleton, Typography } from "@mui/material";
import { useSetupPageContext } from "../../../pages/dashboard/setup/SetupContext";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { dashboardStatistics } from "../../../pages/dashboard/main/types/dashboardStatistics";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useQuery } from "@tanstack/react-query";
import { useEffect, useCallback } from "react";
import { steps } from "../../../pages/dashboard/setup/types/steps";
import { useSnackbar } from "../../../utils/SnackbarContext";
import { templateState } from "../../../pages/dashboard/setup/types/setupTemplateSteps";
import { SetupSolutionSteps } from "./SetupSolutionSteps";
import { SetupAdditionalSteps } from "./SetupAdditionalSteps";

export const SetupStageContainer = () => {
  const { setupEligible, setSetupEligible, setupSteps, setSetupSteps } =
    useSetupPageContext();
  const { selectedInstance, setLoading } = useInstanceContext();
  const api = new Api(env.apiBaseUrl);
  const { showSnackbar } = useSnackbar();

  const fetchOverview = async (signal?: AbortSignal): Promise<boolean> => {
    if (!selectedInstance) return false;

    const endpoint = `/api/v1/dashboard/instance/${selectedInstance.host}/info/overview`;

    const response: dashboardStatistics = await api.get<dashboardStatistics>(
      endpoint,
      undefined,
      { credentials: "include", signal }
    );

    return (
      response.branches > 0 &&
      response.users > 0 &&
      Boolean(response.requiredBrandSettings) &&
      Boolean(response.requiredImages)
    );
  };

  const fetchSteps = useCallback(
    async (signal?: AbortSignal): Promise<void> => {
      if (!selectedInstance || setupEligible !== true) return;

      const endpoint = `/api/v1/dashboard/instance/${selectedInstance.host}/info/setup-state`;
      const result: steps = await api.get(endpoint, undefined, {
        credentials: "include",
        signal: signal,
      });

      if (result) {
        setSetupSteps(result);
      } else {
        showSnackbar("Failed to fetch steps", "error");
      }

      setLoading(false);
    },
    [selectedInstance, setupEligible, setSetupSteps, showSnackbar, setLoading]
  );

  const { data: stats, isFetching } = useQuery<boolean>({
    queryKey: ["eligible", selectedInstance?.host],
    queryFn: ({ signal }) => fetchOverview(signal),
    enabled: selectedInstance != null,
  });

  useEffect(() => {
    if (selectedInstance) {
      setLoading(true);
    }
  }, [selectedInstance, setLoading]);

  useEffect(() => {
    if (selectedInstance && stats !== undefined) {
      setSetupEligible(stats);
      setLoading(false);
    }
  }, [selectedInstance, stats, setSetupEligible, setLoading]);

  useEffect(() => {
    if (stats === true) {
      setLoading(true);
      fetchSteps();
    }
  }, [stats, fetchSteps]);

  const setupStepsComplete = (): boolean => {
    return (
      setupSteps.templateSteps.templatesConfirmed == templateState.complete &&
      setupSteps.templateSteps.letterheadConfirmed == templateState.complete &&
      setupSteps.templateSteps.directMailConfirmed == templateState.complete
    );
  };

  return (
    <Paper sx={{ p: 2, mt: 2 }} variant="outlined">
      {setupEligible == null || isFetching ? (
        <Skeleton />
      ) : setupEligible === false ? (
        <Typography variant="body1">
          This instance is not yet ready to be setup. Please go back and review
          the setup steps.
        </Typography>
      ) : !setupStepsComplete() ? (
        <SetupSolutionSteps />
      ) : (
        <SetupAdditionalSteps />
      )}
    </Paper>
  );
};
