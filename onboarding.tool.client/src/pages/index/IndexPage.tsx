import { Box, Button, Stack, Typography } from "@mui/material";
import { env } from "../../utils/env";
import { useAuthenticationContext } from "../../utils/AuthenticationContext";
import { useNavigate } from "@tanstack/react-router";
import { useEffect } from "react";
import nurtur from "../../assets/nurtur-logo.svg";

const basePath = import.meta.env.VITE_BASE_URL || "/";

const IndexPage = () => {
  const navigate = useNavigate();
  const { authenticated } = useAuthenticationContext();

  useEffect(() => {
    if (authenticated?.authenticated) {
      const params = new URLSearchParams(location.search);
      const path = params.get("path");

      if (path) {
        navigate({ to: path, replace: true });
      } else {
        navigate({ to: basePath + "dashboard", replace: true });
      }
    }
  }, [authenticated, navigate]);

  const handleSignin = () => {
    window.location.href = `${env.apiBaseUrl}/login`;
  };

  return (
    <Box
      sx={{
        height: "100vh",
        width: "100vw",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        flexDirection: "column",
        gap: 1,
      }}
    >
      <Stack
        direction="row"
        alignItems="center"
        justifyContent="center"
        spacing={2}
        sx={{ my: 4 }}
      >
        <Box
          component="img"
          src={nurtur}
          alt="Nurtur Logo"
          sx={{ height: 80 }}
        />
        <Box>
          <Typography variant="h4" fontWeight="bold">
            Nurtur
          </Typography>
          <Typography variant="h6" color="text.secondary">
            On-Boarding
          </Typography>
        </Box>
      </Stack>
      <Button onClick={handleSignin} variant="contained" color="primary">
        Login with Nurtur
      </Button>
    </Box>
  );
};

export default IndexPage;
