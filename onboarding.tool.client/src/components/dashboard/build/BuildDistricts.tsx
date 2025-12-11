import {
  Box,
  Collapse,
  Paper,
  Skeleton,
  Typography,
} from "@mui/material";
import { FC } from "react";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useBuildPageContext } from "../../../pages/dashboard/build/BuildContextProvider";
import { BuildDistrictBranchSelector } from "./BuildDistrictBranchSelector";
import { BuildDistrictBranchEditor } from "./BuildDistrictBranchEditor";
import { BuildDistrictOptions } from "./BuildDistrictOptions";

interface BuildDistrictsProps {
  fetching: boolean;
}

export const BuildDistricts: FC<BuildDistrictsProps> = ({ fetching }) => {
  const { loading, selectedInstance } = useInstanceContext();
  const { selectedBranch } = useBuildPageContext();

  return (
    <Paper variant="outlined" sx={{ p: 2, my: 2 }}>
      <Typography variant="h6" fontWeight={600}>
        Branch Districts & Sectors
      </Typography>

      {loading || fetching || selectedInstance == null ? (
        <Box sx={{ my: 2 }}>
          <Skeleton />
          <Skeleton />
        </Box>
      ) : (
        <Box sx={{ my: 2 }}>
          <BuildDistrictBranchSelector />

          <Collapse in={selectedBranch != null}>
            <BuildDistrictBranchEditor />
            <BuildDistrictOptions />
          </Collapse>
        </Box>
      )}
    </Paper>
  );
};
