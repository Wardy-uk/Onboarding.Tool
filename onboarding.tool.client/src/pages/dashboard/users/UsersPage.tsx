import { Box, Typography } from "@mui/material";
import { UsersPageContextProvider } from "./UsersContextProvider";
import { UserManager } from "../../../components/dashboard/users/UserManager";

export const UsersPage = () => {
  return (
    <UsersPageContextProvider>
      <Box sx={{ padding: 2, overflowY: "auto" }}>
        <Typography variant="h5" fontWeight={600}>
          User Management
        </Typography>
        <UserManager />
      </Box>
    </UsersPageContextProvider>
  );
};
