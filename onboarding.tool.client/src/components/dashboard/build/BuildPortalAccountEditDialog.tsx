import { FC, useEffect, useState } from "react";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
} from "@mui/material";

interface BuildPortalAccountEditDialogProps {
  title: string;
  open: boolean;
  onSave: (savePortalAccount: string) => void;
  onClose: () => void;
}

export const BuildPortalAccountEditDialog: FC<
  BuildPortalAccountEditDialogProps
> = ({ title, open, onSave, onClose }) => {
  const [portalInput, setPortalInput] = useState("");

  const onClickSave = () => {
    onSave(portalInput);
  };

  useEffect(() => {
    if (open) {
      setPortalInput("");
    }
  }, [open]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <TextField
          label="Portal Account"
          value={portalInput}
          onChange={(e) => setPortalInput(e.target.value)}
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
