import { Box, Button } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { InfoBar } from "../../common/dashboard/InfoBar";
import { Link } from "@tanstack/react-router";
import { dashboardBuild } from "../../../routes/dashboard/dashboard";
import { BranchEditModal } from "./BranchEditModal";
import { branch } from "../../../pages/dashboard/branches/types/branch";
import { useState } from "react";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { useBranchesPageContext } from "../../../pages/dashboard/branches/BranchesContextProvider";
import { useSnackbar } from "../../../utils/SnackbarContext";
import { importBranch } from "../../../pages/dashboard/branches/types/importBranch";
import { BranchImportDialog } from "./BranchImportModal";

const defaultBranch: branch = {
  id: null,
  isDefault: false,
  name: "",
  salesEmail: "",
  salesPhone: "",
  lettingsEmail: "",
  lettingsPhone: "",
  address: {
    id: null,
    address1: "",
    address2: null,
    address3: null,
    town: "",
    postCode1: "",
    postCode2: "",
  },
};

export const BranchesOptions = () => {
  const { loading, selectedInstance } = useInstanceContext();
  const { setBranches } = useBranchesPageContext();
  const [editBranch, setEditBranch] = useState<branch>(defaultBranch);
  const [editBranchOpen, setEditBranchOpen] = useState<boolean>(false);
  const [importBranchesOpen, setImportBranchesOpen] = useState<boolean>(false);
  const { showSnackbar } = useSnackbar();
  const api = new Api(env.apiBaseUrl);

  const addBranch = () => {
    setEditBranch(defaultBranch);
    setEditBranchOpen(true);
  };

  const saveBranch = async (branch: branch) => {
    if (selectedInstance == null || branch.id != null) return;

    const endpoint = `/api/v1/dashboard/instance/${selectedInstance.host}/branch/create`;

    const body: importBranch = {
      isDefault: branch.isDefault,
      name: branch.name,
      salesEmail: branch.salesEmail,
      salesPhone: branch.salesPhone,
      lettingsEmail: branch.lettingsEmail,
      lettingsPhone: branch.lettingsPhone,
      Address1: branch.address.address1,
      Address2: branch.address.address2,
      Address3: branch.address.address3,
      Town: branch.address.town,
      PostCode1: branch.address.postCode1,
      PostCode2: branch.address.postCode2,
    };

    const result: branch = await api.post(endpoint, body, {
      credentials: "include",
    });

    if (result) {
      setBranches((prev) => {
        if (branch.isDefault) {
          return [...prev.map((b) => ({ ...b, isDefault: false })), result];
        }
        return [...prev, result];
      });

      setEditBranchOpen(false);
      showSnackbar("Created branch", "success");
    } else {
      showSnackbar("Failed to create branch.", "error");
    }
  };

  const importBranches = async () => {
    setImportBranchesOpen(false);
  }

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
          Each branch can have its own settings and districts. These can be
          configured{" "}
          <Link
            to={dashboardBuild.fullPath}
            disabled={loading || selectedInstance == null}
          >
            here
          </Link>
          .
        </InfoBar>
        <Box sx={{ display: "flex", gap: 1 }}>
          <Button
            variant="contained"
            color="warning"
            startIcon={<AddIcon />}
            onClick={() => setImportBranchesOpen(true)}
            disabled={loading || selectedInstance == null}
          >
            Import CSV
          </Button>
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={addBranch}
            disabled={loading || selectedInstance == null}
          >
            Add Branch
          </Button>
        </Box>
      </Box>

      <BranchEditModal
        branch={editBranch}
        title={"Create Branch"}
        open={editBranchOpen}
        onClose={() => setEditBranchOpen(false)}
        onSave={saveBranch}
      />

      <BranchImportDialog
        open={importBranchesOpen}
        onCancel={() => setImportBranchesOpen(false)}
        onImport={importBranches}
      />
    </>
  );
};
