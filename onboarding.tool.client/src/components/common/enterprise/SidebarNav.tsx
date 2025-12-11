import {
  Box,
  Drawer,
  Toolbar,
} from "@mui/material";
import nurtur from "../../../assets/nurtur-logo.svg"

const drawerWidth = 280;

type SidebarNavProps = {
  mobile: boolean;
  onCloseMobile: () => void;
};

export default function SidebarNav({ mobile, onCloseMobile }: SidebarNavProps) {
  const content = (
    <Box>
      <Toolbar>
        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
          <img src={nurtur} width={26} alt="Nurtur Logo" />
          <Box component="span" sx={{ fontWeight: 600 }}>
            Nurtur Onboarding
            Enterprise
          </Box>
        </Box>
      </Toolbar>
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
