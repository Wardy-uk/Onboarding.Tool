import {
  Box,
  Button,
  Chip,
  Collapse,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  Stack,
  Switch,
  TextField,
} from "@mui/material";
import { FC, useEffect, useState } from "react";
import { buildDistrict } from "../../../pages/dashboard/build/types/buildDistrict";
import { useSnackbar } from "../../../utils/SnackbarContext";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { useBuildPageContext } from "../../../pages/dashboard/build/BuildContextProvider";

interface BuildDistrictBranchEditorDialogProps {
  open: boolean;
  onCancel: () => void;
  district: buildDistrict;
}

const defaultDistrict: buildDistrict = {
  districtId: null,
  branchId: 0,
  district: "",
  allSectors: false,
  sectors: [],
};

export const BuildDistrictBranchEditorDialog: FC<
  BuildDistrictBranchEditorDialogProps
> = ({ open, onCancel, district }) => {
  const [districtForm, setDistrictForm] =
    useState<buildDistrict>(defaultDistrict);
  const [currentSector, setCurrentSector] = useState<string>("");
  const { selectedInstance } = useInstanceContext();
  const { selectedBranch, setSelectedBranch, setBranches, branches } =
    useBuildPageContext();
  const { showSnackbar } = useSnackbar();
  const api = new Api(env.apiBaseUrl);

  const onSetDistrictName = (input: string) => {
    setDistrictForm((prev) => ({
      ...prev,
      district: input,
    }));
  };

  const setAllSectors = (status: boolean) => {
    setDistrictForm((prev) => ({
      ...prev,
      allSectors: status,
    }));
  };

  const saveDistrict = async () => {
    if (selectedBranch == null) return;

    if (!districtForm.allSectors && districtForm.sectors.length === 0) {
      showSnackbar(
        "You must specify sectors for the district, or set it to all.",
        "error"
      );
      return;
    }

    const payload = {
      ...districtForm,
      branchId: selectedBranch?.id ?? 0,
      sectors: districtForm.allSectors ? [] : districtForm.sectors,
    };

    const endpoint: string = `/api/v1/dashboard/instance/${
      selectedInstance?.host
    }/build/districts/${
      districtForm.districtId != null
        ? "save/" + districtForm.districtId
        : "create"
    }`;
    const result: buildDistrict = await api.post(endpoint, payload, {
      credentials: "include",
    });

    if (result) {
      showSnackbar("Saved district", "success");

      const updatedBranch = {
        ...selectedBranch,
        districts: (selectedBranch.districts || []).map((d) =>
          d.districtId === result.districtId
            ? {
                ...d,
                sectors: result.sectors,
                allSectors: result.allSectors,
              }
            : d
        ),
      };

      if (
        !updatedBranch.districts.some((d) => d.districtId === result.districtId)
      ) {
        updatedBranch.districts.push(result);
      }

      setBranches(
        branches.map((branch) =>
          branch.id === selectedBranch.id ? updatedBranch : branch
        )
      );

      setSelectedBranch(updatedBranch);

      setDistrictForm(defaultDistrict);
      setCurrentSector("");
      onCancel();
    } else {
      showSnackbar("Failed to save district", "error");
    }
  };

  const deleteSector = (selectedSector: string) => {
    setDistrictForm((prev) => ({
      ...prev,
      sectors: prev.sectors.filter((s) => s !== selectedSector),
    }));
  };

  const addSector = () => {
    if (!currentSector.trim() || isNaN(Number(currentSector))) return;

    setDistrictForm((prev) => {
      if (prev.sectors.includes(currentSector)) {
        return prev;
      }

      return {
        ...prev,
        sectors: [...prev.sectors, currentSector],
      };
    });

    setCurrentSector("");
  };

  useEffect(() => {
    if (district) {
      setDistrictForm(district);
    }
  }, [district]);

  return (
    <Dialog open={open} onClose={onCancel} maxWidth="sm" fullWidth>
      <DialogTitle>Editing District</DialogTitle>
      <DialogContent sx={{ p: 2 }}>
        <Box sx={{ m: 2 }}>
          <TextField
            label="District"
            value={districtForm.district}
            onChange={(e) => onSetDistrictName(e.target.value)}
            fullWidth
            sx={{ mb: 2 }}
          />

          <FormControlLabel
            control={
              <Switch
                checked={districtForm.allSectors}
                onChange={(e) => setAllSectors(e.target.checked)}
              />
            }
            label="All Sectors"
            sx={{ mb: 2 }}
          />

          <Collapse in={!districtForm.allSectors}>
            <Stack direction="row" spacing={2} alignItems="center">
              <TextField
                label="Sector"
                value={currentSector}
                onChange={(e) => setCurrentSector(e.target.value)}
                fullWidth
              />

              <Button
                variant="contained"
                sx={{ whiteSpace: "nowrap" }}
                onClick={addSector}
              >
                Add
              </Button>
            </Stack>

            <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1, py: 2 }}>
              {districtForm.sectors.map((sector, index) => (
                <Chip
                  key={index}
                  label={sector}
                  onDelete={() => deleteSector(sector)}
                />
              ))}
            </Box>
          </Collapse>
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>Cancel</Button>
        <Button onClick={saveDistrict} variant="contained" color="primary">
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
};
