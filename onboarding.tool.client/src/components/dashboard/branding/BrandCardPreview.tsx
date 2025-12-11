import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
} from "@mui/material";
import { FC, useEffect, useState } from "react";
import { env } from "../../../utils/env";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { Api } from "../../../utils/api";
import { useSnackbar } from "../../../utils/SnackbarContext";

interface BrandCardPreviewProps {
  open: boolean;
  onCancel: () => void;
}

interface PreviewResponse {
  x: number;
  y: number;
  height: number;
  width: number;
}

export const BrandCardPreview: FC<BrandCardPreviewProps> = ({
  open,
  onCancel,
}) => {
  const { selectedInstance } = useInstanceContext();
  const [preview, setPreview] = useState<PreviewResponse | null>(null);
  const [iframeSrc, setIframeSrc] = useState<string>(
    `${env.apiBaseUrl}/api/v1/dashboard/instance/${selectedInstance?.host}/info/render-preview#toolbar=0&navpanes=0&scrollbar=0`
  );
  const api = new Api(env.apiBaseUrl);
  const { showSnackbar } = useSnackbar();

  const closeModal = () => {
    setPreview(null);
    onCancel();
  };

  const saveOverrides = async () => {
    if (!selectedInstance || !preview) return;

    const baseUrl: string = `/api/v1/dashboard/instance/${selectedInstance.host}/image/preview/override`;
    const result: boolean = await api.post(baseUrl, preview, { credentials: "include" });

    if (result) {
      showSnackbar("Updated image overrides", "success");
      closeModal();
    } else {
      showSnackbar("Failed to save overrides", "error");
    }
  };

  const resetOverrides = async () => {
    if (!selectedInstance) return;

    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance.host}/image/preview/reset`;
    const response: boolean = await api.post(endpoint, undefined, { credentials: "include" });

    if (response) {
      setPreview(null);
      showSnackbar("The preview has been reset.", "success");
    } else {
      showSnackbar("Failed to reset preview parameters.", "error");
    }
  }

  useEffect(() => {
    if (!selectedInstance) return;

    const baseUrl = `${env.apiBaseUrl}/api/v1/dashboard/instance/${selectedInstance.host}/info/render-preview`;
    const params = new URLSearchParams();

    if (preview) {
      if (preview.x !== undefined) params.append("x", preview.x.toString());
      if (preview.y !== undefined) params.append("y", preview.y.toString());
      if (preview.width !== undefined) params.append("width", preview.width.toString());
      if (preview.height !== undefined) params.append("height", preview.height.toString());
    }

    const queryString = params.toString();
    const hash = "toolbar=0&navpanes=0&scrollbar=0";
    const url = queryString ? `${baseUrl}?${queryString}#${hash}` : `${baseUrl}#${hash}`;
    setIframeSrc(url);
  }, [preview, selectedInstance, env]);

  useEffect(() => {
    if (!open || !selectedInstance) return;

    const fetchDefaults = async () => {
      try {
        if (!preview) {
          const endpoint = `/api/v1/dashboard/instance/${selectedInstance.host}/info/preview-card-info`;
          const response: PreviewResponse = await api.get(endpoint, undefined, { credentials: "include" });
          setPreview(response);
        }
      } catch (err) {
        console.error(err);
      }
    };

    fetchDefaults();
  }, [open, selectedInstance, preview]);

  return (
    <Dialog open={open} maxWidth={false} fullWidth>
      <DialogTitle>Preview</DialogTitle>
      <DialogContent>
        <Box sx={{ border: "1px solid #ccc", borderRadius: 2, overflow: "hidden", width: "100%", height: 700 }}>
          <iframe src={iframeSrc} width="100%" height="90%" style={{ border: "none" }} title="Render Preview" />
          <Stack direction="row" spacing={2} alignItems="center" justifyContent="center">
            <TextField
              label="Position X"
              type="number"
              value={preview?.x ?? ""}
              onChange={(e) => setPreview(prev => ({ ...prev, x: Number(e.target.value) } as PreviewResponse))}
              InputLabelProps={{ shrink: true }}
              inputProps={{ min: 0, max: 500, step: 1 }}
            />
            <TextField
              label="Position Y"
              type="number"
              value={preview?.y ?? ""}
              onChange={(e) => setPreview(prev => ({ ...prev, y: Number(e.target.value) } as PreviewResponse))}
              InputLabelProps={{ shrink: true }}
              inputProps={{ min: 0, max: 500, step: 1 }}
            />
            <TextField
              label="Width"
              type="number"
              value={preview?.width ?? ""}
              onChange={(e) => setPreview(prev => ({ ...prev, width: Number(e.target.value) } as PreviewResponse))}
              InputLabelProps={{ shrink: true }}
              inputProps={{ min: 0, max: 100, step: 1 }}
            />
            <TextField
              label="Height"
              type="number"
              value={preview?.height ?? ""}
              onChange={(e) => setPreview(prev => ({ ...prev, height: Number(e.target.value) } as PreviewResponse))}
              InputLabelProps={{ shrink: true }}
              inputProps={{ min: 0, max: 100, step: 1 }}
            />
            <Button
              variant="contained"
              color="success"
              onClick={resetOverrides}
            >
              Reset
            </Button>
          </Stack>
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={closeModal}>Close</Button>
        <Button onClick={saveOverrides}>Save</Button>
      </DialogActions>
    </Dialog>
  );
};

