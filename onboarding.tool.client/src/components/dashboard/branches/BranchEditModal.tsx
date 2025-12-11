import { FC, useEffect, useState } from "react";
import { address, branch } from "../../../pages/dashboard/branches/types/branch";
import { useSnackbar } from "../../../utils/SnackbarContext";
import {
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  TextField,
} from "@mui/material";

interface BranchEditModalProps {
  title: string;
  branch: branch;
  open: boolean;
  onSave: (updatedBranch: branch) => void;
  onClose: () => void;
}

interface BranchErrors {
  name?: string;
  salesEmail?: string;
  salesPhone?: string;
  lettingsEmail?: string;
  lettingsPhone?: string;
  address?: {
    address1?: string;
    town?: string;
    postCode1?: string;
    postCode2?: string;
  };
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

export const BranchEditModal: FC<BranchEditModalProps> = ({
  title,
  branch,
  open,
  onSave,
  onClose,
}) => {
  const { showSnackbar } = useSnackbar();
  const [branchForm, setBranchForm] = useState<branch>(defaultBranch);
  const [errors, setErrors] = useState<BranchErrors>({});

  const onClickSave = () => {
    const newErrors: BranchErrors = {};

    if (!branchForm.name.trim()) newErrors.name = "Name is required";
    if (!branchForm.salesEmail.trim())
      newErrors.salesEmail = "Sales Email is required";
    if (!branchForm.salesPhone.trim())
      newErrors.salesPhone = "Sales Phone is required";
    if (!branchForm.lettingsEmail.trim())
      newErrors.lettingsEmail = "Lettings Email is required";
    if (!branchForm.lettingsPhone.trim())
      newErrors.lettingsPhone = "Lettings Phone is required";

    if (
      branchForm.salesEmail &&
      !/^\S+@\S+\.\S+$/.test(branchForm.salesEmail)
    ) {
      newErrors.salesEmail = "Sales Email must be an email";
    }

    if (
      branchForm.lettingsEmail &&
      !/^\S+@\S+\.\S+$/.test(branchForm.lettingsEmail)
    ) {
      newErrors.lettingsEmail = "Lettings Email must be an email";
    }

    if (branchForm.salesPhone && !/^\+44\d{10}$/.test(branchForm.salesPhone)) {
      newErrors.salesPhone =
        "Sales Phone must be a UK phone number (+44 format)";
    }

    if (
      branchForm.lettingsPhone &&
      !/^\+44\d{10}$/.test(branchForm.lettingsPhone)
    ) {
      newErrors.lettingsPhone =
        "Lettings Phone must be a UK phone number (+44 format)";
    }

    console.log(branchForm.address);
    if (!branchForm.address?.address1?.trim()) {
      newErrors.address = {
        ...newErrors.address,
        address1: "Address 1 is required",
      };
    }

    if (!branchForm.address?.town?.trim()) {
      newErrors.address = {
        ...newErrors.address,
        town: "Town is required",
      };
    }

    if (!branchForm.address?.postCode1?.trim()) {
      newErrors.address = {
        ...newErrors.address,
        postCode1: "Postcode 1 is required",
      };
    }

    if (!branchForm.address?.postCode2?.trim()) {
      newErrors.address = {
        ...newErrors.address,
        postCode2: "Postcode 2 is required",
      };
    }

    if (Object.keys(newErrors).length > 0 || newErrors.address) {
      showSnackbar("There are some issues with your branch.", "error");
      setErrors(newErrors);
      return;
    }

    setErrors({});
    onSave(branchForm);
  };

  const handleChange =
    (field: keyof branch) => (e: React.ChangeEvent<HTMLInputElement>) => {
      setBranchForm((prev) => ({
        ...prev,
        [field]: e.target.value,
      }));
    };

  const handleAddressChange =
    (field: keyof address) => (e: React.ChangeEvent<HTMLInputElement>) => {
      setBranchForm((prev) => ({
        ...prev,
        address: {
          ...prev.address,
          [field]: e.target.value,
        },
      }));
    };

  useEffect(() => {
    if (branch) {
      setBranchForm(branch);
    }
  }, [branch, open]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent sx={{ p: 2 }}>
        <FormControlLabel
          control={
            <Checkbox
              checked={branchForm.isDefault}
              onChange={(e) =>
                setBranchForm((prev) => ({
                  ...prev,
                  isDefault: e.target.checked,
                }))
              }
            />
          }
          label="Is Default?"
        />{" "}
        <TextField
          label="Branch Name"
          value={branchForm.name}
          onChange={handleChange("name")}
          fullWidth
          required
          sx={{
            mb: 2,
          }}
          error={!!errors.name}
          helperText={errors.name}
        />
        <Box sx={{ display: "flex", gap: 2 }}>
          <TextField
            label="Sales Email"
            value={branchForm.salesEmail}
            onChange={handleChange("salesEmail")}
            fullWidth
            required
            sx={{
              mb: 2,
            }}
            error={!!errors.salesEmail}
            helperText={errors.salesEmail}
          />

          <TextField
            label="Sales Phone Number"
            value={branchForm.salesPhone}
            onChange={handleChange("salesPhone")}
            fullWidth
            required
            sx={{
              mb: 2,
            }}
            error={!!errors.salesPhone}
            helperText={errors.salesPhone}
          />
        </Box>
        <Box sx={{ display: "flex", gap: 2 }}>
          <TextField
            label="Lettings Email"
            value={branchForm.lettingsEmail}
            onChange={handleChange("lettingsEmail")}
            fullWidth
            required
            sx={{
              mb: 2,
            }}
            error={!!errors.lettingsEmail}
            helperText={errors.lettingsEmail}
          />

          <TextField
            label="Lettings Phone Number"
            value={branchForm.lettingsPhone}
            onChange={handleChange("lettingsPhone")}
            fullWidth
            required
            sx={{
              mb: 2,
            }}
            error={!!errors.lettingsPhone}
            helperText={errors.lettingsPhone}
          />
        </Box>
        <TextField
          label="Address Line 1"
          value={branchForm.address?.address1 ?? ""}
          onChange={handleAddressChange("address1")}
          fullWidth
          required
          sx={{
            mb: 2,
          }}
          error={!!errors.address?.address1}
          helperText={errors.address?.address1}
        />
        <TextField
          label="Address Line 2 (Optional)"
          value={branchForm.address?.address2 ?? ""}
          onChange={handleAddressChange("address2")}
          fullWidth
          sx={{
            mb: 2,
          }}
        />
        <TextField
          label="Address Line 3 (Optional)"
          value={branchForm.address?.address3 ?? ""}
          onChange={handleAddressChange("address3")}
          fullWidth
          sx={{
            mb: 2,
          }}
        />
        <TextField
          label="Town"
          value={branchForm.address?.town ?? ""}
          onChange={handleAddressChange("town")}
          fullWidth
          sx={{
            mb: 2,
          }}
          required
          error={!!errors.address?.town}
          helperText={errors.address?.town}
        />
        <Box sx={{ display: "flex", gap: 2 }}>
          <TextField
            label="Postcode 1"
            value={branchForm.address?.postCode1 ?? ""}
            onChange={handleAddressChange("postCode1")}
            fullWidth
            required
            sx={{
              mb: 2,
            }}
            error={!!errors.address?.postCode1}
            helperText={errors.address?.postCode1}
          />

          <TextField
            label="Postcode 2"
            value={branchForm.address?.postCode2 ?? ""}
            onChange={handleAddressChange("postCode2")}
            fullWidth
            required
            sx={{
              mb: 2,
            }}
            error={!!errors.address?.postCode2}
            helperText={errors.address?.postCode2}
          />
        </Box>
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
