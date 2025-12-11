import { useEffect, useState } from "react";
import { Outlet, useNavigate } from "@tanstack/react-router";
import { useAuthenticationContext } from "../../../utils/AuthenticationContext";
import { Box, styled, Toolbar, useMediaQuery, useTheme } from "@mui/material";
import TopBar from "../../../components/common/layout/TopBar";
import SidebarNav from "../../../components/common/enterprise/SidebarNav";

const basePath = import.meta.env.VITE_BASE_URL || "/";
const drawerWidth = 280;

const Main = styled("main", {
  shouldForwardProp: (prop) => prop !== "open",
})<{ open: boolean }>(({ theme, open }) => ({
  flexGrow: 1,
  height: "100vh",
  display: "flex",
  flexDirection: "column",
  overflow: "hidden",
  transition: theme.transitions.create("margin", {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.leavingScreen,
  }),
  marginLeft: 0,
  ...(open && {
    marginLeft: drawerWidth,
    transition: theme.transitions.create("margin", {
      easing: theme.transitions.easing.easeOut,
      duration: theme.transitions.duration.enteringScreen,
    }),
  }),
}));

export default function EnterpriseLayout() {
  const theme = useTheme();
  const navigate = useNavigate();
  const { authenticated } = useAuthenticationContext();
  const mobile = useMediaQuery(theme.breakpoints.down("sm"));
  const [mobileOpen, setMobileOpen] = useState(false);

  useEffect(() => {
    if (!authenticated?.authenticated) {
      navigate({ to: `${basePath}?path=${location.pathname}`, replace: true });
    }
  }, [authenticated, navigate]);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

return (
    <Box sx={{ display: "flex", width: "100%", height: "100%" }}>
      {/*<InstanceContextProvider>*/}
        <TopBar mobile={mobile} onMenuClick={handleDrawerToggle} />
        <SidebarNav mobile={mobileOpen} onCloseMobile={handleDrawerToggle} />
        <Main open={!mobile}>
          <Toolbar />
          <Box
            sx={{
              flex: 1,
              display: "flex",
              flexDirection: "column",
              width: "100%",
              overflow: "hidden",
            }}
          >
            <Outlet />
          </Box>
        </Main>
      {/*</InstanceContextProvider>*/}
    </Box>
  );
}
