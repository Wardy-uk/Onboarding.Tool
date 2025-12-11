import { Box, Button } from "@mui/material";
import { InfoBar } from "../../../components/common/dashboard/InfoBar";
import AddIcon from "@mui/icons-material/Add";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { user } from "../../../pages/dashboard/users/types/user";
import { UserEditDialog } from "../../../components/dashboard/users/UserEditDialog";
import { useUsersPageContext } from "../../../pages/dashboard/users/UsersContextProvider";
import { useState } from "react";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { useSnackbar } from "../../../utils/SnackbarContext";
import { UserImportDialog } from "../../../components/dashboard/users/UserImportDialog";

const blankUser: user = {
  id: null,
  username: "",
};

export const UserOptions = () => {
  const { loading, selectedInstance } = useInstanceContext();
  const { setUsers } = useUsersPageContext();
  const [creatingUser, setCreatingUser] = useState<user | null>(null);
  const [createUserModalOpen, setCreateUserModalOpen] = useState<boolean>(false);
  const [importModalOpen, setImportModalOpen] = useState<boolean>(false);
  const api = new Api(env.apiBaseUrl);
  const { showSnackbar } = useSnackbar();

  const importUsers = () => {
    setImportModalOpen(true);
  };

  const createUser = () => {
    setCreatingUser(blankUser);
    setCreateUserModalOpen(true);
  };

  const addUser = async (user: user) => {
    if (selectedInstance == null || user.id != null) return;

    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance.host}/user/create`;
    const result: user = await api.post(
      endpoint,
      { Email: user.username },
      { credentials: "include" }
    );

    if (result) {
      showSnackbar("Created " + user.username, "success");
      setUsers((prev) => [...prev, result]);
    } else {
      showSnackbar("Failed to Create");
    }
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
          Each user will be given full administrative access in the system.
        </InfoBar>
        <Box sx={{ display: "flex", gap: 1 }}>
          <Button
            variant="contained"
            color="warning"
            startIcon={<AddIcon />}
            onClick={importUsers}
            disabled={loading || selectedInstance == null}
          >
            Import CSV
          </Button>
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={createUser}
            disabled={loading || selectedInstance == null}
          >
            Add User
          </Button>
        </Box>
      </Box>
      <UserEditDialog
        title="Create User"
        open={createUserModalOpen}
        user={creatingUser!}
        onClose={() => setCreateUserModalOpen(false)}
        onSave={(updatedUser) => {
          addUser(updatedUser);
          setCreateUserModalOpen(false);
        }}
      />

      <UserImportDialog open={importModalOpen} onCancel={() => setImportModalOpen(false)} onImport={() => setImportModalOpen(false)} />
    </>
  );
};
