import { Box, Typography } from "@mui/material";
import { BranchesContainer } from "../../../components/dashboard/branches/BranchesContainer";
import { BranchesPageContextProvider } from "./BranchesContextProvider";

export const BranchesPage = () => {
  return (
    <BranchesPageContextProvider>
      <Box sx={{ padding: 2, overflowY: "auto" }}>
        <Typography variant="h5" fontWeight={600}>
          Branches
        </Typography>
        <BranchesContainer />
      </Box>
    </BranchesPageContextProvider>
  );
};
export default BranchesPage;
