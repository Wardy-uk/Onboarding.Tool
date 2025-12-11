import { Box, Button } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { InfoBar } from "../../common/dashboard/InfoBar";
import { BuildDistrictBranchEditorDialog } from "./BuildDistrictBranchEditorDialog";
import { useState } from "react";
import { buildDistrict } from "../../../pages/dashboard/build/types/buildDistrict";

const defaultDistrict: buildDistrict = {
  districtId: null,
  branchId: 0,
  district: "",
  allSectors: false,
  sectors: [],
};

export const BuildDistrictOptions = () => {
  const { loading, selectedInstance } = useInstanceContext();
  const [editBranchDialogOpen, setEditBranchDialogOpen] =
    useState<boolean>(false);

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
          The districts and sectors selected here will be the area covered by
          the branch in BuildYourMarket.
        </InfoBar>
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          onClick={() => setEditBranchDialogOpen(true)}
          disabled={loading || selectedInstance == null}
        >
          Add District
        </Button>
      </Box>

      <BuildDistrictBranchEditorDialog
        open={editBranchDialogOpen}
        onCancel={() => setEditBranchDialogOpen(false)}
        district={defaultDistrict}
      />
    </>
  );
};
