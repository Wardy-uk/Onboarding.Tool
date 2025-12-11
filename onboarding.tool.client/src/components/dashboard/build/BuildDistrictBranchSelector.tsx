import { Autocomplete, TextField } from "@mui/material";
import { useBuildPageContext } from "../../../pages/dashboard/build/BuildContextProvider";

export const BuildDistrictBranchSelector = () => {
  const { branches, selectedBranch, setSelectedBranch } = useBuildPageContext();

  return (
    <Autocomplete
      options={branches}
      value={selectedBranch}
      getOptionLabel={(option) => option.name}
      renderInput={(params) => (
        <TextField {...params} label="Select Branch" size="small" />
      )}
      onChange={(_, newValue) => setSelectedBranch(newValue)}
      isOptionEqualToValue={(option, selectedBranch) => {
        if (!option || !selectedBranch) return false;
        return option.id === selectedBranch.id;
      }}
      sx={{ mb: 2 }}
      noOptionsText="No branches found"
      renderOption={(props, option) => {
        const { key, ...rest } = props;
        return (
          <li key={key} {...rest}>
            {option.name}
          </li>
        );
      }}
    />
  );
};
