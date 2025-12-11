import { FC, useRef, useState } from "react";
import { useBrandingPageContext } from "../../../pages/dashboard/branding/BrandingContextProvider";
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Link,
  Typography,
} from "@mui/material";
import { useInstanceContext } from "../../../utils/InstanceContext";
import { useSnackbar } from "../../../utils/SnackbarContext";
import {
  csvHasRequiredHeaders,
  splitCSVLine,
} from "../../../utils/helpers/validationHelper";
import { Api } from "../../../utils/api";
import { env } from "../../../utils/env";
import { useQueryClient } from "@tanstack/react-query";

interface BrandSettingImportModalProps {
  open: boolean;
  onCancel: () => void;
  onImport: (settings: any[]) => void;
}

function parseCSV(
  text: string,
  requiredFields: string[],
  allFields: string[]
): {
  data: any[];
  errors: string[];
} {
  const lines = text.trim().split(/\r?\n/);
  if (lines.length < 2) {
    return {
      data: [],
      errors: [
        "Your CSV must include at least two lines with the first being the headers.",
      ],
    };
  }

  const rawHeaders = splitCSVLine(lines[0]).map((h) => h.trim());
  const normalizedHeaders = rawHeaders.map((h) => h.toLowerCase());

  const headerMap: Record<string, string> = {};
  allFields.forEach((f) => {
    const idx = normalizedHeaders.findIndex((h) => h === f.toLowerCase());
    if (idx !== -1) headerMap[rawHeaders[idx]] = f;
  });

  const issuesWithHeaders: string[] = csvHasRequiredHeaders(
    Object.values(headerMap).map((h) => h.toLowerCase()),
    requiredFields.map((f) => f.toLowerCase())
  );
  if (issuesWithHeaders.length > 0) {
    return { data: [], errors: issuesWithHeaders };
  }

  let errors: string[] = [];
  let hasDefaultContext = false;

  const data = lines.slice(1).map((line, idx) => {
    const values = splitCSVLine(line);
    const row: any = {};

    rawHeaders.forEach((h, i) => {
      const key = headerMap[h];
      if (key) {
        if (key === "isDefault") {
          const value = values[i]?.trim() || "";
          const lower = value.toLowerCase();
          row[key] = lower === "true" || lower === "1" || lower === "yes";
        } else {
          row[key] = values[i]?.trim() || "";
        }
      }
    });

    if (row.context?.toLowerCase() === "default") {
      hasDefaultContext = true;

      requiredFields.forEach((f) => {
        if (!row[f]) {
          errors.push(
            `Row ${idx + 2}: Missing value for '${f}' in default context`
          );
        }
      });
    }

    return row as any;
  });

  if (!hasDefaultContext) {
    errors.push("At least one row must have context = 'default'.");
  }

  return { data, errors };
}

export const BrandSettingImportModal: FC<BrandSettingImportModalProps> = ({
  open,
  onCancel,
  onImport,
}) => {
  const { loading, selectedInstance } = useInstanceContext();
  const { brandSettings } = useBrandingPageContext();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [csvErrors, setCsvErrors] = useState<string[]>([]);
  const [csvData, setCsvData] = useState<any[] | null>(null);
  const { showSnackbar } = useSnackbar();
	const queryClient = useQueryClient();
  const api = new Api(env.apiBaseUrl);

  const requiredHeaders: string[] = [
    "context",
    ...brandSettings.filter((bs) => bs.required).map((bs) => bs.key),
  ];
  const allHeaders: string[] = [
    "context",
    ...brandSettings.map((bs) => bs.key),
  ];

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const text = await file.text();
    const { data, errors } = parseCSV(text, requiredHeaders, allHeaders);
    setCsvData(errors.length === 0 ? data : null);
    setCsvErrors(errors);
  };

  const handleUpload = async () => {
    if (csvData == null || selectedInstance == null) return;

    const endpoint: string = `/api/v1/dashboard/instance/${selectedInstance.host}/settings/import`;
    const result: boolean = await api.post<boolean, any[]>(endpoint, csvData, {
      credentials: "include",
    });

    if (result) {
      showSnackbar("Imported brand settings successfully.", "success");
    } else {
      showSnackbar("Failed to import brand settings.", "error");
    }

    queryClient.invalidateQueries({
      predicate: (query) =>
        Array.isArray(query.queryKey) &&
        query.queryKey[0] === "branchSettings" &&
        query.queryKey[1] === selectedInstance?.host,
    });

		setCsvData(null);
		setCsvErrors([]);
    onImport(csvData);
  };

  return (
    <Dialog open={open}>
      <DialogTitle>Import Brand Settings</DialogTitle>
      <DialogContent>
        <Box mb={2}>
          <Typography gutterBottom>
            Please upload a CSV file with the following required columns:
          </Typography>
          {requiredHeaders.map((rh) => (
            <Typography>{rh} *</Typography>
          ))}
          <Typography gutterBottom>
            Any column marked with an asterisk (*) is required.
          </Typography>
          <Link
            href={`${import.meta.env.VITE_BASE_URL || "/"}sample-settings.csv`}
            download
          >
            Download sample CSV
          </Link>
        </Box>
        <Box sx={{ display: "flex", justifyContent: "flex-end" }}>
          <Button
            variant="outlined"
            component="label"
            disabled={loading || selectedInstance == null}
          >
            Select CSV File
            <input
              type="file"
              accept=".csv"
              hidden
              ref={fileInputRef}
              onChange={handleFileChange}
              disabled={loading || selectedInstance == null}
            />
          </Button>
        </Box>
        {csvErrors.length > 0 && (
          <Box mt={2}>
            {csvErrors.map((err, i) => (
              <Alert severity="error" key={i}>
                {err}
              </Alert>
            ))}
          </Box>
        )}
        {csvData && csvErrors.length === 0 && (
          <Alert severity="success" sx={{ mt: 2 }}>
            CSV looks good! Ready to upload {csvData.length} brand's settings.
          </Alert>
        )}
      </DialogContent>
      <DialogActions>
        <Button
          onClick={onCancel}
          disabled={loading || selectedInstance == null}
        >
          Cancel
        </Button>
        <Button
          onClick={handleUpload}
          variant="contained"
          color="primary"
          disabled={
            loading ||
            selectedInstance == null ||
            !csvData ||
            csvErrors.length > 0
          }
        >
          Import
        </Button>
      </DialogActions>
    </Dialog>
  );
};
