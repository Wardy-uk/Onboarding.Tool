import { useUsersPageContext } from "../../../pages/dashboard/users/UsersContextProvider";
import {
  Box,
  IconButton,
  List,
  ListItem,
  ListItemText,
  Stack,
  Typography,
} from "@mui/material";
import ModeEditIcon from "@mui/icons-material/ModeEdit";
import DeleteIcon from "@mui/icons-material/Delete";
import { user } from "../../../pages/dashboard/users/types/user";
import { useState } from "react";
import { UserEditDialog } from "./UserEditDialog";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useSnackbar } from "../../../utils/SnackbarContext";

export const UserList = () => {
  const { selectedInstance } = useInstanceContext();
  const { users, setUsers } = useUsersPageContext();
  const [editingUser, setEditingUser] = useState<user | null>(null);
  const [editUserModalOpen, setEditUserModalOpen] = useState<boolean>(false);
  const api = new Api(env.apiBaseUrl);
  const { showSnackbar } = useSnackbar();

  const editUser = (user: user) => {
    setEditingUser(user);
    setEditUserModalOpen(true);
  };

  const handleSave = async (user: user) => {
    if (selectedInstance == null || user.id == null) return;

    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance.host}/user/edit/${user.id}`;
    const result: boolean = await api.post(
      endpoint,
      { Email: user.username },
      { credentials: "include" }
    );

    if (result) {
      showSnackbar("Updated " + user.username, "success");

      setUsers((prev) =>
        prev.map((u) =>
          u.id === user.id ? { ...u, username: user.username } : u
        )
      );
    } else {
      showSnackbar("Failed to update " + user.username, "error");
    }
  };

  const deleteUser = async (user: user) => {
    if (selectedInstance == null || user.id == null) return;

    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance.host}/user/delete/${user.id}`;
    const result: boolean = await api.post(
      endpoint,
      { userId: user.id.toString() },
      { credentials: "include" }
    );

    if (result) {
      showSnackbar("Removed " + user.username, "success");
      setUsers((prev) => prev.filter((u) => u.id !== user.id));
    } else {
      showSnackbar("Failed to remove " + user.username, "error");
    }
  };

  return users.length === 0 ? (
    <Typography>No users setup</Typography>
  ) : (
    <>
      <List>
        {users.map((u) => (
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
                    onClick={() => editUser(u)}
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
                    onClick={() => deleteUser(u)}
                    disabled={false}
                  >
                    <DeleteIcon color="error" />
                  </IconButton>
                </Box>
              </Stack>
            }
          >
            <ListItemText primary={u.username} />
          </ListItem>
        ))}
      </List>

      <UserEditDialog
        title="Edit User"
        open={editUserModalOpen}
        user={editingUser!}
        onClose={() => setEditUserModalOpen(false)}
        onSave={(updatedUser) => {
          handleSave(updatedUser);
          setEditUserModalOpen(false);
        }}
      />
    </>
  );
};
