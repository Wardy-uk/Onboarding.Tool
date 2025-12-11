import { Box } from "@mui/material";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useSetupPageContext } from "../../../pages/dashboard/setup/SetupContext";
import { SetupStepItem } from "./SetupStepItem";
import { templateState } from "../../../pages/dashboard/setup/types/setupTemplateSteps";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { useSnackbar } from "../../../utils/SnackbarContext";
import { setupResult } from "../../../pages/dashboard/setup/types/setupResult";
import { usePolling } from "../../../utils/Polling";
import { useState } from "react";

type templateTypes = "templates" | "letter" | "cards";

const defaultSubmitting: Record<templateTypes, boolean> = {
  templates: false,
  letter: false,
  cards: false,
};

export const SetupSolutionSteps = () => {
  const { selectedInstance } = useInstanceContext();
  const { setupSteps, setSetupSteps, setSetupConsoleLogs } =
    useSetupPageContext();
  const { showSnackbar } = useSnackbar();
  const [submitting, setSubmitting] =
    useState<Record<templateTypes, boolean>>(defaultSubmitting);
  const api = new Api(env.apiBaseUrl);

  const updateSubmitting = (type: templateTypes, state: boolean) => {
    setSubmitting((prev) => ({
      ...prev,
      [type]: state,
    }));
  };

  const setupTemplate = async (type: templateTypes, state: templateState) => {
    if (!selectedInstance) return;

    updateSubmitting(type, true);

    try {
      const isUnconfirmed = state === templateState.unconfirmed;
      const endpoint = `/api/v1/briefyourmarket/project/${
        selectedInstance.host
      }/${type}${isUnconfirmed ? "/confirm" : ""}`;

      const result: setupResult = await api.post(endpoint, undefined, {
        credentials: "include",
      });

      if (!result) {
        showSnackbar(`Failed to update ${type}`, "error");
        return;
      }

      const logPrefix = `[${type} ${result.success ? "Success" : "Error"}]`;
      const logMessage =
        result.message ?? (result.success ? "Success." : "Error.");
      setSetupConsoleLogs((prev) => [...prev, `${logPrefix}: ${logMessage}`]);

      if (!result.success) return;

      setSetupSteps((prev) => {
        const typeFieldMap: Record<
          typeof type,
          keyof typeof prev.templateSteps
        > = {
          templates: "templatesConfirmed",
          letter: "letterheadConfirmed",
          cards: "directMailConfirmed",
        };

        const field = typeFieldMap[type];

        if (type == "cards" && state === templateState.incomplete) {
          return {
            ...prev,
            templateSteps: {
              ...prev.templateSteps,
              [field]: isUnconfirmed
                ? templateState.complete
                : templateState.queued,
            },
          };
        } else {
          return {
            ...prev,
            templateSteps: {
              ...prev.templateSteps,
              [field]: isUnconfirmed
                ? templateState.complete
                : templateState.unconfirmed,
            },
          };
        }
      });
    } catch (err) {
      showSnackbar(`Unexpected error updating ${type}`, "error");
    }

    updateSubmitting(type, false);
  };

  const getDirectMailStatus = async () => {
    const endpoint = `/api/v1/briefyourmarket/project/${selectedInstance?.host}/cards/status`;
    const result: templateState = await api.get(endpoint, undefined, {
      credentials: "include",
    });

    if (typeof result === "number") return { state: result };
    return { state: templateState.queued };
  };

  usePolling({
    fn: async () => {
      if (!selectedInstance) return { state: templateState.incomplete };
      const result = await getDirectMailStatus();
      if (typeof result === "number") return { state: result };
      if (result && typeof result.state === "number") return result;
      return { state: 3 };
    },
    interval: 2000,
    shouldContinue: (res) => res.state === templateState.queued,
    enabled:
      setupSteps.templateSteps.directMailConfirmed === templateState.queued,
    onComplete: (res) => {
      if (!selectedInstance) return;

      setSetupSteps((prev) => ({
        ...prev,
        templateSteps: {
          ...prev.templateSteps,
          directMailConfirmed: res.state,
        },
      }));

      if (res.state === templateState.unconfirmed) {
        showSnackbar("DirectMail Cards are ready for review.", "success");
        setSetupConsoleLogs((prev) => [
          ...prev,
          "[cards Success]: DirectMail returned success. Please review.",
        ]);
      } else if (res.state === templateState.incomplete) {
        showSnackbar("DirectMail Cards failed.", "error");
        setSetupConsoleLogs((prev) => [
          ...prev,
          "[cards Error]: DirectMail returned error.",
        ]);
      }
    },
  });

  return (
    <Box>
      <SetupStepItem
        label="Instance Templates"
        onAction={() =>
          setupTemplate(
            "templates",
            setupSteps.templateSteps.templatesConfirmed
          )
        }
        state={setupSteps.templateSteps.templatesConfirmed}
        submitting={submitting.templates}
      />
      <SetupStepItem
        label="Letterhead Template"
        onAction={() =>
          setupTemplate("letter", setupSteps.templateSteps.letterheadConfirmed)
        }
        state={setupSteps.templateSteps.letterheadConfirmed}
        submitting={submitting.letter}
      />
      <SetupStepItem
        label="Print Templates"
        onAction={() =>
          setupTemplate("cards", setupSteps.templateSteps.directMailConfirmed)
        }
        state={setupSteps.templateSteps.directMailConfirmed}
        submitting={submitting.cards}
      />
    </Box>
  );
};
