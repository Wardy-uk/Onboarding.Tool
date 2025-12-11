import { Autocomplete, TextField } from "@mui/material";
import { useInstanceContext } from "../../../utils/InstanceContext";

export const InstanceSelector = () => {
  const { selectedInstance, setSelectedInstance, validInstances, instanceSelectionError } = useInstanceContext();

  return (
    <Autocomplete
      options={validInstances}
      value={selectedInstance}
      getOptionLabel={(option) => option.host}
      renderInput={(params) => (
        <TextField
          {...params}
          label="Select Instance"
          size="small"
          error={!!instanceSelectionError}
          helperText={instanceSelectionError}
        />
      )}
      onChange={(_, newValue) => setSelectedInstance(newValue)}
      isOptionEqualToValue={(option, selectedInstance) => {
        if (!option || !selectedInstance) return false;
        return option.instanceId === selectedInstance.instanceId;
      }}
      sx={{ mb: 2 }}
      noOptionsText="No instances found"
      renderOption={(props, option) => {
        const { key, ...rest } = props;
        return (
          <li key={key} {...rest}>
            {option.host}
          </li>
        );
      }}
    />
  );
};
