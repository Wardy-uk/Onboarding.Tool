import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  SelectChangeEvent,
  Skeleton,
  Typography,
} from "@mui/material";
import { useBrandingPageContext } from "../../../pages/dashboard/branding/BrandingContextProvider";
import SaveIcon from "@mui/icons-material/Save";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { brandBranch } from "../../../pages/dashboard/branding/types/brandBranch";
import { BrandSettingList } from "./BrandSettingList";
import { brandSetting } from "../../../pages/dashboard/branding/types/brandSetting";
import { useSnackbar } from "../../../utils/SnackbarContext";
import { BrandSettingImportModal } from "./BrandSettingImportModal";

export const BrandSettings = () => {
  const { loading, setLoading, selectedInstance } = useInstanceContext();
  const {
    selectedBrand,
    setSelectedBrand,
    brands,
    setBrands,
    currentSettings,
  } = useBrandingPageContext();
  const api = new Api(env.apiBaseUrl);
  const queryClient = useQueryClient();
  const { showSnackbar } = useSnackbar();
  const [settingImportOpen, setSettingImportOpen] = useState<boolean>(false);

  const fetchBranches = async (
    signal?: AbortSignal
  ): Promise<brandBranch[]> => {
    const endpoint = `/api/v1/dashboard/instance/${
      selectedInstance!.host
    }/settings/branch`;

    const response: brandBranch[] = await api.get<brandBranch[]>(
      endpoint,
      undefined,
      {
        credentials: "include",
        signal,
      }
    );

    return response;
  };

  const { data: apiBranches, isFetching: isFetchingBranches } = useQuery<
    brandBranch[]
  >({
    queryKey: ["branches"],
    queryFn: ({ signal }) => fetchBranches(signal),
    staleTime: Infinity,
    enabled: selectedInstance != null,
  });

  useEffect(() => {
    if (selectedInstance) {
      setLoading(true);
      setSelectedBrand(-1);
      queryClient.invalidateQueries({ queryKey: ["branches"] });
    }
  }, [selectedInstance, setSelectedBrand, setLoading, queryClient]);

  useEffect(() => {
    if (selectedInstance && apiBranches) {
      setBrands(apiBranches);
      setLoading(false);
    }
  }, [selectedInstance, apiBranches]);

  const saveSettings = async () => {
    if (selectedInstance == null) return;

    const endpoint: string = `/api/v1/dashboard/instance/${
      selectedInstance.host
    }/settings/${selectedBrand != -1 ? "branch/" + selectedBrand : ""}`;
    const result: brandSetting[] = await api.post(endpoint, currentSettings, {
      credentials: "include",
    });

    if (result) {
      showSnackbar("Saved settings", "success");
      queryClient.invalidateQueries({
        queryKey: ["branchSettings", selectedInstance?.host, selectedBrand],
      });
    } else {
      showSnackbar("Failed to save settings", "error");
    }
  };

  const handleBranchChange = (e: SelectChangeEvent) => {
    setSelectedBrand(Number(e.target.value));
  };

  return (
    <Paper variant="outlined" sx={{ mt: 2, p: 2 }}>
      {loading || selectedInstance == null ? (
        <>
          <Skeleton />
          <Skeleton />
          <Skeleton />
        </>
      ) : (
        <>
          <Typography variant="h6" fontWeight={600}>
            Settings
          </Typography>
          <Box
            sx={{
              display: "flex",
              alignItems: "center",
              my: 2,
              justifyContent: "space-between",
            }}
          >
            <FormControl sx={{ minWidth: 320, maxWidth: 400 }} size="small">
              <InputLabel id="settings-context-label">
                Settings Context
              </InputLabel>

              {isFetchingBranches ? (
                <Skeleton />
              ) : (
                <Select
                  labelId="settings-context-label"
                  label="Settings Context"
                  value={selectedBrand.toString()}
                  onChange={handleBranchChange}
                >
                  <MenuItem value="-1">Default</MenuItem>
                  {brands.map((b) => (
                    <MenuItem key={b.id} value={b.id}>
                      {b.name}
                    </MenuItem>
                  ))}
                </Select>
              )}
            </FormControl>
            <Button
              variant="contained"
              color="warning"
              onClick={() => setSettingImportOpen(true)}
              sx={{ ml: 2, minWidth: 160 }}
            >
              Import Settings
            </Button>
          </Box>
          <BrandSettingList />
          <Box
            sx={{
              display: "flex",
              justifyContent: "flex-end",
              alignItems: "center",
              mb: 2,
              marginTop: "20px",
            }}
          >
            <Button
              variant="contained"
              color="primary"
              startIcon={<SaveIcon />}
              onClick={saveSettings}
              disabled={loading || selectedInstance == null}
            >
              Save Settings
            </Button>
          </Box>
        </>
      )}
      <BrandSettingImportModal
        open={settingImportOpen}
        onCancel={() => setSettingImportOpen(false)}
        onImport={() => setSettingImportOpen(false)}
      />
    </Paper>
  );
};
