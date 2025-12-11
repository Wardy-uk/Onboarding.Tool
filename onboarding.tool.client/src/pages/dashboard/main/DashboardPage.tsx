import { Box, Typography } from "@mui/material";
import { DashboardPageContextProvider } from "./DashboardContextProvider";
import { InstanceOverview } from "../../../components/common/dashboard/InstanceOverview";

export const DashboardPage = () => {
  return (
    <DashboardPageContextProvider>
      <Box sx={{ padding: 2, overflowY: "auto" }}>
        <Typography variant="h5" fontWeight={600}>Overview</Typography>
        <InstanceOverview />
      </Box>
    </DashboardPageContextProvider>
  );
};
export default DashboardPage;
