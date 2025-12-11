import {
  Box,
  Button,
  CircularProgress,
  Typography,
  Paper,
} from "@mui/material";
import { FC, ReactNode } from "react";
import { templateState } from "../../../pages/dashboard/setup/types/setupTemplateSteps";
import { useInstanceContext } from "../../../utils/InstanceContext";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import ErrorIcon from "@mui/icons-material/Error";
import WarningAmberIcon from "@mui/icons-material/WarningAmber";

interface SetupStepItemProps {
  state: templateState;
  onAction: () => void;
  label: string;
  submitting: boolean;
}

export const SetupStepItem: FC<SetupStepItemProps> = ({
  state,
  onAction,
  label,
  submitting,
}) => {
  const { loading, selectedInstance } = useInstanceContext();

  const getIcon = (): ReactNode => {
    if (state === templateState.incomplete) {
      return <ErrorIcon color="error" />;
    } else if (state === templateState.complete) {
      return <CheckCircleIcon color="success" />;
    } else {
      return <WarningAmberIcon color="warning" />;
    }
  };

  const getStatusColor = () => {
    if (state === templateState.incomplete) return "error.main";
    if (state === templateState.complete) return "success.main";
    return "warning.main";
  };

  return (
    <Paper
      elevation={2}
      sx={{
        display: "flex",
        alignItems: "center",
        p: 2,
        borderRadius: 2,
        gap: 2,
        m: 2,
      }}
    >
      <Box display="flex" alignItems="center" gap={1} minWidth={160}>
        {getIcon()}
        <Typography variant="subtitle1" fontWeight={500}>
          {label}
        </Typography>
      </Box>

      <Typography
        variant="body2"
        sx={{ color: getStatusColor(), fontWeight: 500 }}
      >
        {templateState[state]}
      </Typography>

      <Box flex={1} />

      <Button
        variant="contained"
        color="primary"
        onClick={onAction}
        disabled={
          loading ||
          state === templateState.queued ||
          state === templateState.complete ||
          submitting ||
          selectedInstance == null
        }
        startIcon={loading ? <CircularProgress size={16} /> : undefined}
      >
        Setup
      </Button>
    </Paper>
  );
};
