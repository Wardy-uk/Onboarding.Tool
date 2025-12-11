import { Box, Button } from "@mui/material";
import { InfoBar } from "../../common/dashboard/InfoBar";
import AddIcon from "@mui/icons-material/Add";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useState } from "react";
import { BuildPortalAccountEditDialog } from "./BuildPortalAccountEditDialog";
import { useBuildPageContext } from "../../../pages/dashboard/build/BuildContextProvider";
import { useSnackbar } from "../../../utils/SnackbarContext";
import { BuildPortalAccountImportDialog } from "./BuildPortalAccountImportDialog";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { buildPortalAccount } from "../../../pages/dashboard/build/types/buildPortalAccount";

export const BuildPortalAccountOptions = () => {
  const { loading, selectedInstance } = useInstanceContext();
  const { portalAccounts, setPortalAccounts } = useBuildPageContext();
  const [showPortalModal, setShowPortalModal] = useState<boolean>(false);
  const [showImportModal, setShowImportModal] = useState<boolean>(false);
  const { showSnackbar } = useSnackbar();
  const api = new Api(env.apiBaseUrl);

  const onSavePortalAccount = async (portal: string) => {
    if (portalAccounts.find((p) => p.portalName == portal)) {
      showSnackbar("Failed to add existing portal account", "error");
      return;
    }

    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance?.host}/build/portal/create/${portal}`;

    const result: buildPortalAccount = await api.post<
      buildPortalAccount,
      undefined
    >(endpoint, undefined, { credentials: "include" });

    if (result) {
      setPortalAccounts((prev) => [...prev, result]);
      showSnackbar("Successfully added " + portal, "success");
      setShowPortalModal(false);
    } else {
      showSnackbar("Failed to add " + portal, "error");
    }
  };

  const onImportPortalAccounts = async () => {
   setShowImportModal(false);
  };

  return (
    <>
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          mb: 2,
          marginTop: "20px",
        }}
      >
        <InfoBar>
          Exclusions are applied to the build process. Remove exclusions to
          include them.
        </InfoBar>
        <Box sx={{ display: "flex", gap: 1 }}>
          <Button
            variant="contained"
            color="warning"
            startIcon={<AddIcon />}
            onClick={() => setShowImportModal(true)}
            disabled={loading || selectedInstance == null}
          >
            Import CSV
          </Button>
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={() => setShowPortalModal(true)}
            disabled={loading || selectedInstance == null}
          >
            Add Portal Account
          </Button>
        </Box>
      </Box>

      <BuildPortalAccountEditDialog
        title="Add Portal Account"
        onClose={() => setShowPortalModal(false)}
        open={showPortalModal}
        onSave={onSavePortalAccount}
      />

      <BuildPortalAccountImportDialog
        open={showImportModal}
        onCancel={() => setShowImportModal(false)}
        onImport={onImportPortalAccounts}
      />
    </>
  );
};
