import {
  Box,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  useTheme,
} from "@mui/material";
import { Link, useRouterState } from "@tanstack/react-router";
import { navItems } from "../../../pages/dashboard/main/types/NavItems";
import nurtur from "../../../assets/nurtur-logo.svg"
import { useInstanceContext } from "../../../utils/InstanceContext";
import { InstanceSelector } from "./InstanceSelector";

const drawerWidth = 280;

type SidebarNavProps = {
  mobile: boolean;
  onCloseMobile: () => void;
};

export default function SidebarNav({ mobile, onCloseMobile }: SidebarNavProps) {
  const theme = useTheme();
  const routerState = useRouterState();
  const currentPath = routerState.location.pathname;
  const { selectedInstance } = useInstanceContext();

  const content = (
    <Box>
      <Toolbar>
        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
          <img src={nurtur} width={26} alt="Nurtur Logo" />
          <Box component="span" sx={{ fontWeight: 600 }}>
            Nurtur Onboarding
          </Box>
        </Box>
      </Toolbar>
      <List sx={{ px: 2, py: 1 }}>
        <InstanceSelector />
        {navItems.map((item) => {
          const isActive = currentPath === item.to;
          return (
            <ListItem key={item.to} disablePadding sx={{ mb: 0.5 }}>
              <ListItemButton
                component={Link}
                to={item.to}
                onClick={() => {
                  if (mobile) {
                    onCloseMobile();
                  }
                }}
                selected={isActive}
                sx={{
                  borderRadius: 2,
                  "&.Mui-selected": {
                    backgroundColor: theme.palette.primary.main,
                    color: "white",
                    "&:hover": {
                      backgroundColor: theme.palette.primary.dark,
                    },
                    "& .MuiListItemIcon-root": {
                      color: "white",
                    },
                  },
                }}
                disabled={selectedInstance == null}
              >
                <ListItemIcon
                  sx={{ minWidth: 40, color: isActive ? "white" : "inherit" }}
                >
                  {item.icon}
                </ListItemIcon>
                <ListItemText
                  primary={item.label}
                  primaryTypographyProps={{ fontWeight: 500 }}
                />
              </ListItemButton>
            </ListItem>
          );
        })}
      </List>
    </Box>
  );

  return (
    <>
      {/* Mobile temporary drawer */}
      <Drawer
        variant="temporary"
        open={mobile}
        onClose={onCloseMobile}
        ModalProps={{ keepMounted: true }}
        sx={{
          display: { xs: "block", sm: "none" },
          "& .MuiDrawer-paper": { boxSizing: "border-box", width: drawerWidth },
        }}
      >
        {content}
      </Drawer>

      {/* Desktop permanent drawer */}
      <Drawer
        variant="permanent"
        open
        sx={{
          display: { xs: "none", sm: "block" },
          "& .MuiDrawer-paper": { boxSizing: "border-box", width: drawerWidth },
        }}
      >
        {content}
      </Drawer>
    </>
  );
}
