import { FC, useState } from "react";
import { branch } from "../../../pages/dashboard/branches/types/branch";
import {
  Box,
  IconButton,
  ListItem,
  ListItemText,
  Stack,
  Typography,
} from "@mui/material";
import ModeEditIcon from "@mui/icons-material/ModeEdit";
import DeleteIcon from "@mui/icons-material/Delete";
import { BranchEditModal } from "./BranchEditModal";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { useSnackbar } from "../../../utils/SnackbarContext";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useBranchesPageContext } from "../../../pages/dashboard/branches/BranchesContextProvider";

interface BranchListItemProps {
  branch: branch;
}

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

export const BranchListItem: FC<BranchListItemProps> = ({ branch }) => {
  const { selectedInstance } = useInstanceContext();
  const { setBranches } = useBranchesPageContext();
  const [editBranchModalOpen, setEditBranchModalOpen] =
    useState<boolean>(false);
  const [editingBranch, setEditingBranch] = useState<branch>(defaultBranch);
  const { showSnackbar } = useSnackbar();
  const api = new Api(env.apiBaseUrl);

  const saveBranch = async (newBranch: branch) => {
    if (newBranch.id === 0 || editingBranch === null) return;

    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance?.host}/branch/save`;
    const result = await api.post(endpoint, newBranch, {
      credentials: "include",
    });

    if (result) {
      showSnackbar("Updated branch", "success");

      setEditBranchModalOpen(false);
      setEditingBranch(defaultBranch);

      setBranches((prev) =>
        prev.map((b) => {
          if (b.id === newBranch.id) {
            return { ...b, ...newBranch };
          }

          if (newBranch.isDefault) {
            return { ...b, isDefault: false };
          }

          return b;
        })
      );
    } else {
      showSnackbar("Failed to update branch.", "error");
    }
  };

  const onEdit = () => {
    setEditingBranch(branch);
    setEditBranchModalOpen(true);
  };

  const onDelete = async () => {
    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance?.host}/branch/delete/${branch.id}`;

    const result: boolean = await api.post(endpoint, undefined, { credentials: "include" });

    if (result) {
      showSnackbar("Deleted branch " + branch.name, "success");
      setBranches((prev) => prev.filter((b) => b.id !== branch.id));
    } else {
      showSnackbar("Unable to delete branch.", "error");
    }
  };

  return (
    <>
      <ListItem
        secondaryAction={
          <Stack direction="row" spacing={2} alignItems="center">
            <Box textAlign="center">
              <Typography variant="caption" color="textSecondary">
                Modify
              </Typography>
              <IconButton
                edge="end"
                aria-label="modify"
                onClick={onEdit}
                disabled={false}
              >
                <ModeEditIcon color="primary" />
              </IconButton>
            </Box>
            <Box textAlign="center">
              <Typography variant="caption" color="textSecondary">
                Delete
              </Typography>
              <IconButton
                edge="end"
                aria-label="delete"
                color="error"
                onClick={onDelete}
                disabled={false}
              >
                <DeleteIcon color="error" />
              </IconButton>
            </Box>
          </Stack>
        }
      >
        <ListItemText
          primary={branch.name}
          secondary={branch.isDefault ? "Default Branch" : undefined}
        />
      </ListItem>

      <BranchEditModal
        title={"Edit Branch"}
        branch={editingBranch}
        open={editBranchModalOpen}
        onClose={() => setEditBranchModalOpen(false)}
        onSave={saveBranch}
      />
    </>
  );
};
