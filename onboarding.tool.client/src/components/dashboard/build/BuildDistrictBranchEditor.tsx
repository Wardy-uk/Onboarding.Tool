import {
  Accordion,
  AccordionDetails,
  AccordionSummary,
  Box,
  Chip,
  FormControlLabel,
  IconButton,
  Switch,
  Typography,
} from "@mui/material";
import { useBuildPageContext } from "../../../pages/dashboard/build/BuildContextProvider";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import ModeEditIcon from "@mui/icons-material/ModeEdit";
import DeleteIcon from "@mui/icons-material/Delete";
import { BuildDistrictBranchEditorDialog } from "./BuildDistrictBranchEditorDialog";
import { useState } from "react";
import { buildDistrict } from "../../../pages/dashboard/build/types/buildDistrict";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useSnackbar } from "../../../utils/SnackbarContext";

const defaultDistrict: buildDistrict = {
  districtId: null,
  branchId: 0,
  district: "",
  allSectors: false,
  sectors: [],
};

export const BuildDistrictBranchEditor = () => {
  const { selectedInstance } = useInstanceContext();
  const { selectedBranch, setSelectedBranch, branches, setBranches } = useBuildPageContext();
  const [editBranchDialogOpen, setEditBranchDialogOpen] =
    useState<boolean>(false);
  const [editingDistrict, setEditingDistrict] =
    useState<buildDistrict>(defaultDistrict);
  const api = new Api(env.apiBaseUrl);
  const { showSnackbar } = useSnackbar();

  const editDistrict = (district: buildDistrict) => {
    setEditingDistrict(district);
    setEditBranchDialogOpen(true);
  };

  const deleteDistrict = async (district: buildDistrict) => {
    if (district.districtId == null || selectedInstance == null || selectedBranch == null) return;

    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance?.host}/build/districts/delete/${district.districtId}`;
    const result: boolean = await api.post(endpoint, undefined, {
      credentials: "include",
    });

    if (result) {
      showSnackbar("Deleted district " + district.district, "success");

      const updatedBranch = {
        ...selectedBranch,
        districts: (selectedBranch.districts || []).filter(
          (d) => d.districtId !== district.districtId
        ),
      };

      setBranches(
        branches.map((branch) =>
          branch.id === selectedBranch.id ? updatedBranch : branch
        )
      );
      setSelectedBranch(updatedBranch);
    } else {
      showSnackbar("Failed to delete district", "error");
    }
  };

  return (
    <>
      {selectedBranch ? (
        selectedBranch.districts.map((d) => (
          <Accordion key={d.districtId} sx={{ mb: 1 }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon fontSize="medium" />}>
              <Typography sx={{ flex: 1 }}>
                {d.district.toUpperCase()}
              </Typography>
              <FormControlLabel
                control={<Switch checked={d.allSectors} disabled />}
                label="All Sectors"
                sx={{ ml: 2 }}
              />
            </AccordionSummary>
            <AccordionDetails>
              <Box sx={{ display: "flex", alignItems: "center", mb: 1 }}>
                <IconButton
                  onClick={() => editDistrict(d)}
                  size="small"
                  sx={{ mr: 1 }}
                >
                  <ModeEditIcon fontSize="medium" color="primary" />
                </IconButton>
                <IconButton onClick={() => deleteDistrict(d)} size="small">
                  <DeleteIcon fontSize="medium" color="error" />
                </IconButton>
              </Box>

              <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1, py: 2 }}>
                {d.sectors.map((s, index) => (
                  <Chip key={index} label={s} color="primary" />
                ))}
              </Box>
            </AccordionDetails>
          </Accordion>
        ))
      ) : (
        <Typography variant="body1">No branch selected</Typography>
      )}

      <BuildDistrictBranchEditorDialog
        open={editBranchDialogOpen}
        onCancel={() => setEditBranchDialogOpen(false)}
        district={editingDistrict}
      />
    </>
  );
};
