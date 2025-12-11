import { useQuery } from "@tanstack/react-query";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useBrandingPageContext } from "../../../pages/dashboard/branding/BrandingContextProvider";
import { useEffect } from "react";
import { instanceBrandSetting } from "../../../pages/dashboard/branding/types/instanceBrandSetting";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { Box, TextField, Typography } from "@mui/material";
import { brandSetting } from "../../../pages/dashboard/branding/types/brandSetting";

export const BrandSettingList = () => {
  const { setLoading, selectedInstance } = useInstanceContext();
  const {
    selectedBrand,
    brandSettings,
    setBrandSettings,
    currentSettings,
    setCurrentSettings,
  } = useBrandingPageContext();
  const api = new Api(env.apiBaseUrl);

  const fetchSettingConfig = async (signal?: AbortSignal) => {
    if (selectedInstance == null) return;

    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance.host}/info/required-settings`;
    const response: brandSetting[] = await api.get<brandSetting[]>(
      endpoint,
      undefined,
      { credentials: "include", signal: signal }
    );

    if (response) {
      setBrandSettings(response);
    }
  };

  const fetchSettings = async (
    signal?: AbortSignal
  ): Promise<instanceBrandSetting[]> => {
    const endpoint = `/api/v1/dashboard/instance/${
      selectedInstance!.host
    }/settings/${selectedBrand !== -1 ? "branch/" + selectedBrand : ""}`;

    const response: instanceBrandSetting[] = await api.get<
      instanceBrandSetting[]
    >(endpoint, undefined, {
      credentials: "include",
      signal,
    });

    return response;
  };

  const { data: branchSettings } = useQuery<instanceBrandSetting[]>({
    queryKey: ["branchSettings", selectedInstance?.host, selectedBrand],
    queryFn: ({ signal }) => fetchSettings(signal),
    staleTime: Infinity,
    enabled: selectedInstance != null && brandSettings.length > 0,
  });

  useEffect(() => {
    if (selectedInstance && branchSettings) {
      const mapped = Object.fromEntries(
        branchSettings.map((s) => [s.setting, s.value])
      );
      setCurrentSettings(mapped);
      setLoading(false);
    }
  }, [selectedInstance, branchSettings, setLoading]);

  useEffect(() => {
    if (brandSettings.length === 0) {
      fetchSettingConfig();
    }
  });

  return (
    <Box display="flex" flexDirection="column" gap={3}>
      {brandSettings.map((s) => {
        const value = currentSettings[s.key] ?? "";

        switch (s.type) {
          case "string":
          case "url":
            return (
              <Box key={s.key} display="flex" flexDirection="column">
                <TextField
                  label={`${s.label} (${s.key})`}
                  type={s.type === "url" ? "url" : "text"}
                  value={value}
                  required={s.required && selectedBrand == -1}
                  onChange={(e) => {
                    const newValue = e.target.value;
                    const updatedSettings = {
                      ...currentSettings,
                      [s.key]: newValue,
                    };
                    setCurrentSettings(updatedSettings);
                  }}
                  variant="outlined"
                  fullWidth
                />
              </Box>
            );

          case "colour":
            return (
              <Box key={s.key} display="flex" flexDirection="column">
                <Typography variant="body1" fontWeight={500}>
                  {s.label}{" "}
                  {s.required && selectedBrand == -1 && (
                    <span style={{ color: "red" }}>*</span>
                  )}
                </Typography>
                <input
                  type="color"
                  value={value || "#000000"}
                  required={s.required}
                  onChange={(e) => {
                    const newValue = e.target.value;
                    const updatedSettings = {
                      ...currentSettings,
                      [s.key]: newValue,
                    };
                    setCurrentSettings(updatedSettings);
                  }}
                  style={{
                    width: "100%",
                    height: 40,
                    border: "1px solid #ccc",
                    borderRadius: 4,
                    cursor: "pointer",
                  }}
                />
              </Box>
            );

          default:
            return null;
        }
      })}
    </Box>
  );
};
