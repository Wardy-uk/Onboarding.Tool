import { Box, Paper, Skeleton, Typography } from "@mui/material";
import { BuildPortalAccountsList } from "./BuildPortalAccountsList";
import { BuildPortalAccountOptions } from "./BuildPortalAccountInfo";
import { FC } from "react";
import { useInstanceContext } from "../../../utils/InstanceContext";

interface BuildPortalAccountsProps {
  fetching: boolean;
}

export const BuildPortalAccounts: FC<BuildPortalAccountsProps> = ({
  fetching,
}) => {
  const { loading, selectedInstance } = useInstanceContext();

  return (
    <Paper variant="outlined" sx={{ p: 2, my: 2 }}>
      <Typography variant="h6" fontWeight={600}>
        Portal Accounts
      </Typography>

      {fetching || loading || selectedInstance == null ? (
        <Box sx={{ my: 2 }}>
          <Skeleton />
          <Skeleton />
        </Box>
      ) : (
        <Box sx={{ my: 2 }}>
          <BuildPortalAccountsList />
          <BuildPortalAccountOptions />
        </Box>
      )}
    </Paper>
  );
};
