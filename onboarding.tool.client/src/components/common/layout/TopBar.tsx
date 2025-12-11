import { AppBar, Avatar, Box, IconButton, Toolbar } from "@mui/material";
import MenuIcon from "@mui/icons-material/Menu";
import { useAuthenticationContext } from "../../../utils/AuthenticationContext";

const drawerWidth = 280;

type TopBarProps = {
  mobile: boolean;
  onMenuClick: () => void;
};

export function TopBar({ mobile, onMenuClick }: TopBarProps) {
  const { authenticated } = useAuthenticationContext();

  return (
    <AppBar
      position="fixed"
      sx={{
        width: { sm: `calc(100% - ${drawerWidth}px)` },
        ml: { sm: `${drawerWidth}px` },
        backgroundColor: "#fff",
        color: "black",
        boxShadow: "none",
        borderBottom: "1px solid #ccc",
      }}
    >
      <Toolbar>
        {mobile && (
          <IconButton
            color="inherit"
            aria-label="open drawer"
            edge="start"
            onClick={onMenuClick}
            sx={{ mr: 2 }}
          >
            <MenuIcon />
          </IconButton>
        )}

        <Box sx={{ flexGrow: 1 }} />

        <Box sx={{ ml: 2 }}>
          <Avatar
            sx={{ bgcolor: "primary.main" }}
            alt={authenticated?.name || "User"}
          >
            {authenticated?.name ? authenticated.name[0].toUpperCase() : "U"}
          </Avatar>
        </Box>
      </Toolbar>
    </AppBar>
  );
}

export default TopBar;

