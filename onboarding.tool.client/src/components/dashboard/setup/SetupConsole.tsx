import { Box, Typography } from "@mui/material";
import { useSetupPageContext } from "../../../pages/dashboard/setup/SetupContext";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useEffect } from "react";

export const SetupConsole = () => {
	const { selectedInstance } = useInstanceContext();
  const { setupConsoleLogs, setSetupConsoleLogs } = useSetupPageContext();

	useEffect(() => {
		if (selectedInstance) {
			setSetupConsoleLogs([]);
		}
	}, [selectedInstance, setSetupConsoleLogs])

  return (
    <Box
      sx={{
        mt: 2,
        p: 2,
        bgcolor: "black",
        color: "green.300",
        fontFamily: "monospace",
        borderRadius: 2,
        height: 300,
        overflowY: "auto",
        border: "1px solid",
        borderColor: "grey.800",
      }}
    >
      {setupConsoleLogs.length === 0 ? (
        <Typography variant="body2" color="grey.500" fontFamily="monospace">
          No logs yet...
        </Typography>
      ) : (
        setupConsoleLogs.map((log, idx) => (
          <Typography
            key={idx}
            variant="body2"
            color="grey.500"
            sx={{ whiteSpace: "pre-wrap", fontFamily: "monospace" }}
          >
            {`> ${log}`}
          </Typography>
        ))
      )}
    </Box>
  );
};
