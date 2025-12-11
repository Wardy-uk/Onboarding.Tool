import { Stack, Typography, useTheme } from "@mui/material";
import { FC } from "react";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import ErrorIcon from "@mui/icons-material/Error";

interface InstanceOverviewStatisticProps {
  success: boolean;
  text: string;
}

export const InstanceOverviewStatistic: FC<InstanceOverviewStatisticProps> = ({
  success,
  text,
}) => {
  const theme = useTheme();

  return (
    <Stack direction="row" alignItems="center" spacing={1} sx={{ my: 1 }}>
      {success ? (
        <CheckCircleIcon
          sx={{ fontSize: 20, color: theme.palette.success.main }}
        />
      ) : (
        <ErrorIcon sx={{ fontSize: 20, color: theme.palette.error.main }} />
      )}
      <Typography variant="body2">{text}</Typography>
    </Stack>
  );
};
