import { Box, Typography } from "@mui/material";
import { BrandingImages } from "../../../components/dashboard/branding/BrandingImages";
import { BrandSettings } from "../../../components/dashboard/branding/BrandSettings";

export const BrandingPage = () => {
  return (
    <Box sx={{ padding: 2, overflowY: "auto" }}>
      <Typography variant="h5" fontWeight={600}>
        Branding
      </Typography>
      <BrandingImages />
      <BrandSettings />
    </Box>
  );
};
