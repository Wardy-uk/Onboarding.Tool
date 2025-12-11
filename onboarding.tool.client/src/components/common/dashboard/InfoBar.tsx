import { FC, ReactNode } from "react";
import { Box, Typography, useTheme } from "@mui/material";
import { FaCircleInfo } from "react-icons/fa6";

interface InfoBarProps {
  children: ReactNode;
}

export const InfoBar: FC<InfoBarProps> = ({ children }) => {
  const theme = useTheme();

  return (
    <Box display="flex" alignItems="center" gap={1}>
      <FaCircleInfo size={16} color={theme.palette.warning.main} />
      <Typography variant="body2" color="textSecondary">
        {children}
      </Typography>
    </Box>
  );
};
