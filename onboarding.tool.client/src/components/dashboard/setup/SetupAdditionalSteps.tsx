import { Box } from "@mui/material";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useSnackbar } from "../../../utils/SnackbarContext";
import { useSetupPageContext } from "../../../pages/dashboard/setup/SetupContext";
import { SetupStepItem } from "./SetupStepItem";
import {
  setupAdditionalStep,
  setupAdditionalSteps,
} from "../../../pages/dashboard/setup/types/setupAdditionalSteps";
import { setupStep } from "../../../pages/dashboard/setup/types/setupStep";
import { templateState } from "../../../pages/dashboard/setup/types/setupTemplateSteps";
import { useState } from "react";

export const SetupAdditionalSteps = () => {
  const { selectedInstance } = useInstanceContext();
  const { setupSteps, setSetupSteps, setSetupConsoleLogs } =
    useSetupPageContext();
  const { showSnackbar } = useSnackbar();

  const [submitting, setSubmitting] = useState<Record<string, boolean>>({});

  const updateSubmitting = (key: string, value: boolean) => {
    setSubmitting((prev) => ({
      ...prev,
      [key]: value,
    }));
  };

  const handleAction = async (step: setupAdditionalStep) => {
    if (!selectedInstance) return;

    updateSubmitting(step.key, true);

    try {
      const result = await step.onSetup(selectedInstance.host);
      const message: string = await step.onSuccess(result);

      setSetupConsoleLogs((prev) => [...prev, message]);

      if (result.success) {
        showSnackbar("Success " + step.label, "success");
      } else {
        showSnackbar("Failed " + step.label, "error");
      }

      setSetupSteps((prev) => {
        const existingIndex = prev.additionalSteps.findIndex(
          (s) => s.setupStep === step.key
        );

        const newStep: setupStep = {
          setupStep: step.key,
          state: result.success
            ? templateState.complete
            : templateState.incomplete,
        };

        let updatedAdditionalSteps: setupStep[];

        if (existingIndex >= 0) {
          updatedAdditionalSteps = [...prev.additionalSteps];
          updatedAdditionalSteps[existingIndex] = newStep;
        } else {
          updatedAdditionalSteps = [...prev.additionalSteps, newStep];
        }

        return {
          ...prev,
          additionalSteps: updatedAdditionalSteps,
        };
      });
    } catch {
      showSnackbar("Failed to " + step.label, "error");
    } finally {
      updateSubmitting(step.key, false);
    }
  };

  const setupStepCompleted = (step: setupAdditionalStep): templateState => {
    if (!selectedInstance) return templateState.incomplete;

    const currentStep: setupStep | undefined = setupSteps.additionalSteps.find(
      (s) => s.setupStep === step.key
    );

    return currentStep?.state ?? templateState.incomplete;
  };

  return (
    <Box>
      {setupAdditionalSteps.map((i) => (
        <SetupStepItem
          key={i.key}
          label={i.label}
          onAction={() => handleAction(i)}
          state={setupStepCompleted(i)}
          submitting={submitting[i.key] ?? false}
        />
      ))}
    </Box>
  );
};
