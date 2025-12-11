import { Box, Typography } from "@mui/material";
import { SetupStageContainer } from "../../../components/dashboard/setup/SetupStageContainer";
import { SetupConsole } from "../../../components/dashboard/setup/SetupConsole";

export const SetupPage = () => {
  return (
    <Box sx={{ padding: 2, overflowY: "auto" }}>
      <Typography variant="h5" fontWeight={600}>
        Setup
      </Typography>
			<SetupStageContainer />
      <SetupConsole />
    </Box>
  );
};
