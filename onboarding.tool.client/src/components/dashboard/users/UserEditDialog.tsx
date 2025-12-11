import { FC, useState, useEffect } from "react";
import { user } from "../../../pages/dashboard/users/types/user";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
} from "@mui/material";
import { validateEmail } from "../../../utils/helpers/validationHelper";
import { useSnackbar } from "../../../utils/SnackbarContext";

interface UserEditDialogProps {
  title: string;
  user: user;
  open: boolean;
  onSave: (updatedUser: user) => void;
  onClose: () => void;
}

export const UserEditDialog: FC<UserEditDialogProps> = ({
  title,
  user,
  open,
  onSave,
  onClose,
}) => {
  const [usernameInput, setUsernameInput] = useState("");
  const { showSnackbar } = useSnackbar();

  const onClickSave = () => {
    if (!validateEmail(usernameInput)) {
      showSnackbar("Invalid email provided " + usernameInput, "error");
      return;
    }

    onSave({ ...user, username: usernameInput });
  };

  useEffect(() => {
    if (user) setUsernameInput(user.username);
  }, [user]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <TextField
          label="Username"
          value={usernameInput}
          onChange={(e) => setUsernameInput(e.target.value)}
          fullWidth
          margin="normal"
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" color="primary" onClick={onClickSave}>
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
};
